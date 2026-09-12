using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Autodesk.Max;

namespace TagManager
{
    public static partial class TagApi
    {
        private static string _session = Guid.NewGuid().ToString();
        private static readonly Dictionary<uint, long> _created = new Dictionary<uint, long>();
        private static readonly Dictionary<uint, ProposalTicket> _tickets = new Dictionary<uint, ProposalTicket>();
        private static readonly HashSet<uint> _outbox = new HashSet<uint>();
        private static readonly Dictionary<Guid, List<uint>> _zones = new Dictionary<Guid, List<uint>>();
        private static DispatcherTimer _proposalTimer;
        public static bool ProposalsEnabled = true;

        private sealed class ProposalTicket
        {
            public string Token;
            public string EvidenceJson;
            public string TreeJson;
            public long Tick;
        }

        private sealed class BranchRestore : Autodesk.Max.Plugins.RestoreObj
        {
            private readonly List<TagNode> _nodes;
            private readonly List<TagNode> _parents;
            public BranchRestore(List<TagNode> nodes) { _nodes = nodes; _parents = nodes.Select(n => n.Parent).ToList(); }
            public override void Restore(bool isUndo)
            {
                for (int i = _nodes.Count - 1; i >= 0; --i) _parents[i].Children.Remove(_nodes[i]);
            }
            public override void Redo()
            {
                for (int i = 0; i < _nodes.Count; ++i) if (!_parents[i].Children.Contains(_nodes[i])) _parents[i].Children.Add(_nodes[i]);
            }
            public override void EndHold() { }
            public override int Size { get { return 64 + _nodes.Count * 32; } }
            public override string Description { get { return "TagManager new branches"; } }
        }

        public static void ResetProposals()
        {
            _session = Guid.NewGuid().ToString();
            _created.Clear(); _tickets.Clear(); _pending.Clear(); _outbox.Clear();
            _nodeBoxes.Clear();
            _zones.Clear();
            RefreshProposalReview();
        }

        public static void QueueCreated(IINode node)
        {
            if (!ProposalsEnabled || node == null || node.IsRootNode) return;
            _created[node.Handle] = _clock.ElapsedMilliseconds;
        }

        public static void ForgetProposal(uint handle)
        {
            _created.Remove(handle); _tickets.Remove(handle); _pending.Remove(handle);
            _outbox.Remove(handle); _nodeBoxes.Remove(handle);
            RefreshProposalReview();
        }

        public static void StartProposalTimer()
        {
            if (_proposalTimer != null) return;
            _proposalTimer = new DispatcherTimer(DispatcherPriority.Background);
            _proposalTimer.Interval = TimeSpan.FromMilliseconds(500);
            _proposalTimer.Tick += (s, e) => { if (IsReady() && !TagGlobals.isMerging) PrepareReady(); };
            _proposalTimer.Start();
        }

        public static void StopProposalTimer()
        {
            if (_proposalTimer != null) { _proposalTimer.Stop(); _proposalTimer = null; }
        }

        private static Dictionary<string, object> ReadJson(string json)
        {
            return new JavaScriptSerializer().DeserializeObject(json) as Dictionary<string, object>;
        }

        private static void PrepareReady()
        {
            // Bounded batches keep callback work small. Max SDK reads stay on this dispatcher.
            if (!ProposalsEnabled || MaxPluginUtilities.Global.TheHold.Holding) return;
            var ready = _created.Where(p => _clock.ElapsedMilliseconds - p.Value >= 750)
                .Select(p => p.Key).Take(16).ToList();
            if (ready.Count == 0) return;
            foreach (uint h in ready) _created.Remove(h);
            RequestEvidence(string.Join(",", ready));
        }

        public static string RequestEvidence(string handlesCsv)
        {
            return Guard(() =>
            {
                RequireReady();
                var payload = ReadJson(Evidence(handlesCsv));
                if (payload == null || !payload.ContainsKey("objects")) return Evidence(handlesCsv);
                string tree = ListEntities();
                var serializer = new JavaScriptSerializer();
                var output = new List<object>();
                foreach (var raw in (object[])payload["objects"])
                {
                    var obj = raw as Dictionary<string, object>;
                    if (obj == null || obj.ContainsKey("missing")) continue;
                    uint h = Convert.ToUInt32(obj["handle"]);
                    _created.Remove(h);
                    // Tagged clones already inherited their entities. Helpers/cameras/targets
                    // are excluded from automatic proposals but remain available via Apply.
                    if (((object[])obj["entities"]).Length != 0) continue;
                    string cls = Convert.ToString(obj["class"]).ToLowerInvariant();
                    if (cls.Contains("target") || cls.Contains("camera") || cls.Contains("helper")) continue;
                    var ticket = new ProposalTicket { Token = Guid.NewGuid().ToString(),
                        EvidenceJson = serializer.Serialize(obj), TreeJson = tree, Tick = _clock.ElapsedMilliseconds };
                    _tickets[h] = ticket; _outbox.Add(h);
                    obj["token"] = ticket.Token;
                    output.Add(obj);
                    var ranked = new List<Candidate>();
                    foreach (var candidate in (object[])obj["candidates"])
                    {
                        var c = (Dictionary<string, object>)candidate;
                        ranked.Add(new Candidate { Path = Convert.ToString(c["path"]), Confidence = 0,
                            GeometryScore = ToDouble(Get(c, "geometryScore"), 0), Overlap = (float)ToDouble(Get(c, "overlap"), 0),
                            Reason = "Provisional: " + (Convert.ToString(Get(c, "reason")) ?? "spatial overlap " + Convert.ToString(c["overlap"])) });
                    }
                    _pending[h] = new Proposal { Handle = h, Token = ticket.Token,
                        Ranked = ranked.OrderByDescending(c => c.GeometryScore + c.Overlap * .3).ToList() };
                }
                RefreshProposalReview();
                return serializer.Serialize(new { ok = true, session = _session, objects = output });
            });
        }

        public static string ProposalBatch()
        {
            return Guard(() =>
            {
                RequireReady();
                PrepareReady();
                var output = new List<object>();
                foreach (uint h in _outbox.Take(16).ToList())
                {
                    _outbox.Remove(h);
                    ProposalTicket ticket;
                    if (!_tickets.TryGetValue(h, out ticket)) continue;
                    var obj = ReadJson(ticket.EvidenceJson);
                    obj["token"] = ticket.Token; output.Add(obj);
                }
                return new JavaScriptSerializer().Serialize(new { ok = true, session = _session,
                    objects = output, decisionLog = DecisionLogPath });
            });
        }

        public static string SetEntityZone(string path, string handlesCsv)
        {
            return Guard(() =>
            {
                RequireReady();
                var entity = TagHelperMethods.RetrieveEntityFromTag(path);
                if (entity == null) throw new ArgumentException("unknown entity");
                var handles = ParseHandles(handlesCsv);
                if (handles.Any(h => MaxPluginUtilities.GetNodeByHandle(h) == null)) throw new ArgumentException("missing zone node");
                if (handles.Count == 0) _zones.Remove(entity.ID); else _zones[entity.ID] = handles;
                _tickets.Clear(); _pending.Clear(); _outbox.Clear();
                RefreshProposalReview();
                return "{\"ok\":true,\"persistence\":\"current session only\"}";
            });
        }

        private static bool TryTicket(uint h, string token, out ProposalTicket ticket)
        {
            if (!_tickets.TryGetValue(h, out ticket) || ticket.Token != token ||
                _clock.ElapsedMilliseconds - ticket.Tick > 120000 || ticket.TreeJson != ListEntities()) return false;
            // Compare original inputs again at presentation and acceptance. This also catches
            // transform, material and time edits made outside TagManager.
            var fresh = ReadJson(Evidence(h.ToString()));
            if (fresh == null || !fresh.ContainsKey("objects")) return false;
            var objects = (object[])fresh["objects"];
            return objects.Length == 1 && new JavaScriptSerializer().Serialize(objects[0]) == ticket.EvidenceJson;
        }

        public static string ReviewProposals()
        {
            return Guard(() =>
            {
                RequireReady();
                TagGlobals.tagCenter.CreateFastTagWin();
                return Pending();
            });
        }

        private static void RefreshProposalReview()
        {
            var fast = TagGlobals.tagCenter == null ? null : TagGlobals.tagCenter.fastTag;
            if (fast != null) fast.RefreshSuggestionsWhenEmpty();
        }

        private static bool TicketAllowsPath(ProposalTicket ticket, string path)
        {
            var evidence = ReadJson(ticket.EvidenceJson);
            return ((object[])evidence["candidates"]).Cast<Dictionary<string, object>>()
                .Any(c => Convert.ToString(c["path"]) == path);
        }

        public static List<string> FastTagPaths(string handlesCsv)
        {
            var handles = ParseHandles(handlesCsv);
            if (handles.Count == 0) return ArchitecturalSuggestions.Starters.ToList();
            List<string> paths = null;
            foreach (uint h in handles)
            {
                Proposal proposal;
                var items = _pending.TryGetValue(h, out proposal) ? proposal.Ranked.Select(c => c.Path).ToList() : new List<string>();
                paths = paths == null ? items : paths.Where(items.Contains).ToList();
            }
            return paths ?? new List<string>();
        }

        public static void PrepareFastTag(string handlesCsv)
        {
            var refresh = new List<uint>();
            foreach (uint h in ParseHandles(handlesCsv))
            {
                ProposalTicket ticket;
                Proposal proposal;
                if (!_pending.TryGetValue(h, out proposal) || !TryTicket(h, proposal.Token, out ticket)) refresh.Add(h);
            }
            if (refresh.Count > 0) RequestEvidence(string.Join(",", refresh));
        }

        public static string AcceptFastTag(string handlesCsv, string path)
        {
            return Guard(() =>
            {
                var handles = ParseHandles(handlesCsv);
                if (handles.Count == 0) throw new ArgumentException("select objects to assign a suggestion");
                var current = new HashSet<uint>(MaxPluginUtilities.Selection.Select(n => n.Handle));
                if (!current.SetEquals(handles)) throw new ArgumentException("selection changed; reopen FastTag");
                foreach (uint h in handles)
                {
                    Proposal proposal; ProposalTicket ticket;
                    if (!_pending.TryGetValue(h, out proposal) || !TryTicket(h, proposal.Token, out ticket) ||
                        !proposal.Ranked.Any(c => c.Path == path))
                        throw new ArgumentException("suggestion changed; reopen FastTag for fresh choices");
                }
                string result = ApplyCore(path, handles, TagGlobals.autoLayer, TagGlobals.autoRename);
                foreach (uint h in handles)
                {
                    LogDecision(h, path, _pending[h].Ranked[0].Path, "confirmed: FastTag");
                    _pending.Remove(h); _tickets.Remove(h); _outbox.Remove(h);
                }
                return result;
            });
        }
    }
}
