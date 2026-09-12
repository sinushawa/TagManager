using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using Autodesk.Max;

namespace TagManager
{
    /// <summary>
    /// String-only facade over the entity tree for MAXScript and the MCP bridge.
    ///
    /// Every method takes plain strings (handles as a comma separated list) and returns a JSON
    /// string. Nothing here throws: failures come back as {"ok":false,"error":"..."} so a
    /// MAXScript caller never has to catch .NET exceptions. All methods must run on the Max main
    /// thread, which is where dotNetClass calls from MAXScript already execute.
    ///
    ///   api = dotNetClass "TagManager.TagApi"
    ///   api.Evidence "1943,1950"
    ///   api.Apply "Building_Floor1_Kitchen" "1943" true true
    /// </summary>
    public static partial class TagApi
    {
        public const string ApiVersion = "1.2";

        /// <summary>Candidates kept per object in Evidence().</summary>
        public static int MaxCandidates = 6;
        /// <summary>An object larger than this fraction of an entity's volume is not offered that entity.</summary>
        public static float SizeRatioCap = 0.5f;
        /// <summary>Node bounding boxes are reused for this long before being re-evaluated.</summary>
        public static double BoxCacheMaxAgeMs = 5000;
        /// <summary>Present() applies a top candidate directly at or above this confidence.</summary>
        public static double AutoApplyThreshold = 0.9;
        public static bool AutoApplyEnabled = false;
        /// <summary>Minimum extent used when an object or entity box is flat, so volumes never reach zero.</summary>
        public static float MinExtent = 0.001f;

        // ------------------------------------------------------------------ public API

        public static string Version()
        {
            return Guard(() =>
            {
                var sb = new StringBuilder();
                sb.Append("{\"ok\":true,\"apiVersion\":\"").Append(ApiVersion).Append('"');
                sb.Append(",\"session\":").Append(Str(_session));
                sb.Append(",\"autoApplyEnabled\":").Append(AutoApplyEnabled ? "true" : "false");
                sb.Append(",\"initialized\":").Append(IsReady() ? "true" : "false");
                sb.Append(",\"autoRename\":").Append(TagGlobals.autoRename ? "true" : "false");
                sb.Append(",\"autoLayer\":").Append(TagGlobals.autoLayer ? "true" : "false");
                sb.Append(",\"autoApplyThreshold\":").Append(Num(AutoApplyThreshold));
                sb.Append(",\"boxCacheEntries\":").Append(_nodeBoxes.Count);
                sb.Append(",\"pending\":").Append(_pending.Count);
                sb.Append('}');
                return sb.ToString();
            });
        }

        /// <summary>Every entity below Project with its path, id, depth and member count.</summary>
        public static string ListEntities()
        {
            return Guard(() =>
            {
                RequireReady();
                var sb = new StringBuilder(4096);
                sb.Append("{\"ok\":true,\"entities\":[");
                bool first = true;
                foreach (TagNode e in Entities())
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append("{\"path\":").Append(Str(e.LongName));
                    sb.Append(",\"name\":").Append(Str(e.Name));
                    sb.Append(",\"id\":").Append(Str(e.ID.ToString()));
                    sb.Append(",\"depth\":").Append(Depth(e));
                    sb.Append(",\"count\":").Append(e.Nodes.Count);
                    sb.Append(",\"children\":").Append(e.Children.Count);
                    sb.Append(",\"nameable\":").Append(e.IsNameable ? "true" : "false");
                    sb.Append(",\"shortcut\":").Append(e.IsShortcut ? "true" : "false");
                    sb.Append('}');
                }
                sb.Append("]}");
                return sb.ToString();
            });
        }

        /// <summary>Entity paths per handle. Handles that own no entity map to an empty list.</summary>
        public static string GetEntities(string handlesCsv)
        {
            return Guard(() =>
            {
                RequireReady();
                List<uint> handles = ParseHandles(handlesCsv);
                Dictionary<uint, List<TagNode>> index = ReverseIndex(Entities());
                var sb = new StringBuilder();
                sb.Append("{\"ok\":true,\"objects\":{");
                bool first = true;
                foreach (uint h in handles)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append('"').Append(h).Append("\":[");
                    List<TagNode> owners;
                    if (index.TryGetValue(h, out owners))
                    {
                        for (int i = 0; i < owners.Count; i++)
                        {
                            if (i > 0) sb.Append(',');
                            sb.Append(Str(owners[i].LongName));
                        }
                    }
                    sb.Append(']');
                }
                sb.Append("}}");
                return sb.ToString();
            });
        }

        /// <summary>World bounding box of every entity's branch (own objects plus descendants).</summary>
        public static string EntityBounds()
        {
            return Guard(() =>
            {
                RequireReady();
                var sw = Stopwatch.StartNew();
                _nodeBoxes.Clear();
                Dictionary<Guid, Box> boxes = EntityBoxes(Entities(), null);
                var sb = new StringBuilder(4096);
                sb.Append("{\"ok\":true,\"entities\":[");
                bool first = true;
                foreach (TagNode e in Entities())
                {
                    Box b;
                    if (!boxes.TryGetValue(e.ID, out b) || !b.Valid) continue;
                    if (!first) sb.Append(',');
                    first = false;
                    sb.Append("{\"path\":").Append(Str(e.LongName)).Append(",\"bbox\":").Append(b.ToJson()).Append('}');
                }
                sb.Append("],\"ms\":").Append(Num(sw.Elapsed.TotalMilliseconds)).Append('}');
                return sb.ToString();
            });
        }

        /// <summary>
        /// Features and containment candidates for each handle. This is the plugin's half of the
        /// evidence stage: the server adds neighbour votes and ranks the combined list.
        /// </summary>
        public static string Evidence(string handlesCsv)
        {
            return Guard(() =>
            {
                RequireReady();
                var sw = Stopwatch.StartNew();
                List<uint> handles = ParseHandles(handlesCsv);
                // Evidence must reflect the current time and transforms, including edits made
                // through other plugins. Reuse boxes within this batch only.
                _nodeBoxes.Clear();
                var exclude = new HashSet<uint>(handles);
                List<TagNode> entities = Entities();
                Dictionary<uint, List<TagNode>> index = ReverseIndex(entities);
                Dictionary<Guid, Box> entityBoxes = EntityBoxes(entities, exclude);
                var entityDepth = new Dictionary<Guid, int>();
                foreach (TagNode e in entities) entityDepth[e.ID] = Depth(e);
                int t = Now();

                var sb = new StringBuilder(8192);
                sb.Append("{\"ok\":true,\"apiVersion\":\"").Append(ApiVersion).Append("\",\"time\":").Append(t);
                sb.Append(",\"entityCount\":").Append(entities.Count).Append(",\"objects\":[");
                bool first = true;
                foreach (uint h in handles)
                {
                    IINode node = MaxPluginUtilities.GetNodeByHandle(h);
                    if (!first) sb.Append(',');
                    first = false;
                    if (node == null)
                    {
                        sb.Append("{\"handle\":").Append(h).Append(",\"missing\":true}");
                        continue;
                    }
                    Box nb = NodeBox(node, t);
                    sb.Append("{\"handle\":").Append(h);
                    sb.Append(",\"name\":").Append(Str(node.Name));
                    sb.Append(",\"class\":").Append(Str(ClassNameOf(node, t)));
                    sb.Append(",\"mtl\":").Append(Str(MaterialNameOf(node)));
                    sb.Append(",\"layer\":").Append(Str(LayerNameOf(node)));
                    sb.Append(",\"parent\":").Append(Str(ParentNameOf(node)));
                    sb.Append(",\"bbox\":").Append(nb.ToJson());
                    sb.Append(",\"size\":").Append(nb.SizeJson());
                    double metres = MaxPluginUtilities.Global.GetSystemUnitScale(5); // UNITS_METERS
                    sb.Append(",\"sizeMeters\":[").Append(Num((nb.X1 - nb.X0) * metres)).Append(',')
                        .Append(Num((nb.Y1 - nb.Y0) * metres)).Append(',').Append(Num((nb.Z1 - nb.Z0) * metres)).Append(']');
                    sb.Append(",\"boundsVolumeM3\":").Append(Num(nb.Valid ? nb.Volume() * metres * metres * metres : 0));
                    sb.Append(",\"entities\":[");
                    List<TagNode> owners;
                    if (index.TryGetValue(h, out owners))
                    {
                        for (int i = 0; i < owners.Count; i++)
                        {
                            if (i > 0) sb.Append(',');
                            sb.Append(Str(owners[i].LongName));
                        }
                    }
                    sb.Append("],\"candidates\":[");
                    List<Candidate> cands = Containment(nb, entities, entityBoxes, entityDepth);
                    if (nb.Valid) foreach (var guess in ArchitecturalSuggestions.FromDimensions(
                        (nb.X1 - nb.X0) * metres, (nb.Y1 - nb.Y0) * metres, (nb.Z1 - nb.Z0) * metres))
                    {
                        string leaf = guess.Path.Split('_').Last();
                        var existing = entities.Where(e => e.Name.Equals(leaf, StringComparison.OrdinalIgnoreCase) && !e.IsShortcut).ToList();
                        var paths = existing.Count > 0 ? existing.Select(e => e.LongName) : new[] { guess.Path };
                        foreach (string path in paths)
                        {
                            var candidate = cands.FirstOrDefault(c => c.Path == path);
                            if (candidate == null) { candidate = new Candidate { Path = path }; cands.Add(candidate); }
                            candidate.GeometryScore = guess.Score;
                            candidate.Reason = guess.Reason;
                            candidate.ProposedHierarchy = !entities.Any(e => e.LongName == path);
                        }
                    }
                    for (int i = 0; i < cands.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        Candidate c = cands[i];
                        sb.Append("{\"path\":").Append(Str(c.Path));
                        sb.Append(",\"overlap\":").Append(Num(c.Overlap));
                        sb.Append(",\"depth\":").Append(c.Depth);
                        sb.Append(",\"sizeRatio\":").Append(Num(c.SizeRatio));
                        sb.Append(",\"centerInside\":").Append(c.CenterInside ? "true" : "false");
                        sb.Append(",\"geometryScore\":").Append(Num(c.GeometryScore));
                        sb.Append(",\"reason\":").Append(Str(c.Reason));
                        sb.Append(",\"proposedHierarchy\":").Append(c.ProposedHierarchy ? "true" : "false");
                        sb.Append('}');
                    }
                    sb.Append("]}");
                }
                sb.Append("],\"ms\":").Append(Num(sw.Elapsed.TotalMilliseconds)).Append('}');
                return sb.ToString();
            });
        }

        /// <summary>
        /// Receive a ranked candidate list per handle. Top candidates at or above the auto-apply
        /// threshold are applied at once; the rest wait in the pending list for confirmation.
        /// Accepted input: {"proposals":[{"handle":1943,"ranked":[{"path":"A_B","confidence":0.93,"reason":"..."}]}],"threshold":0.9}
        /// </summary>
        public static string Present(string rankedJson)
        {
            return Guard(() =>
            {
                RequireReady();
                var ser = new JavaScriptSerializer();
                object root = ser.DeserializeObject(rankedJson);
                double threshold = AutoApplyThreshold;
                object proposalsObj = root;
                var rootDict = root as Dictionary<string, object>;
                if (rootDict == null || Convert.ToString(Get(rootDict, "session")) != _session)
                    throw new ArgumentException("stale or missing proposal session");
                if (rootDict != null)
                {
                    object th;
                    if (rootDict.TryGetValue("threshold", out th)) threshold = ToDouble(th, threshold);
                    rootDict.TryGetValue("proposals", out proposalsObj);
                }
                var proposals = proposalsObj as object[];
                if (proposals == null) throw new ArgumentException("expected a 'proposals' array");

                var applied = new List<string>();
                int queued = 0;
                int rejected = 0;
                foreach (object p in proposals)
                {
                    var pd = p as Dictionary<string, object>;
                    if (pd == null) continue;
                    uint handle = (uint)ToDouble(Get(pd, "handle"), 0);
                    var ranked = Get(pd, "ranked") as object[];
                    if (handle == 0 || ranked == null || ranked.Length == 0) continue;
                    string token = Convert.ToString(Get(pd, "token"));
                    ProposalTicket ticket;
                    if (!TryTicket(handle, token, out ticket))
                    {
                        rejected++;
                        if (ticket != null && ticket.Token == token) _created[handle] = _clock.ElapsedMilliseconds;
                        continue;
                    }
                    var prop = new Proposal { Handle = handle, Token = token, Ranked = new List<Candidate>() };
                    foreach (object r in ranked)
                    {
                        var rd = r as Dictionary<string, object>;
                        if (rd == null) continue;
                        string candidatePath = Convert.ToString(Get(rd, "path")) ?? "";
                        TagNode candidateEntity = TagHelperMethods.RetrieveEntityFromTag(candidatePath);
                        if (candidateEntity == null && !TicketAllowsPath(ticket, candidatePath)) continue;
                        if (candidateEntity != null && (candidateEntity.IsShortcut || TagGlobals.baseNames.Contains(candidateEntity.Name))) continue;
                        double confidence = ToDouble(Get(rd, "confidence"), 0);
                        if (double.IsNaN(confidence) || double.IsInfinity(confidence) || confidence < 0 || confidence > 1) continue;
                        prop.Ranked.Add(new Candidate
                        {
                            Path = candidatePath,
                            Confidence = confidence,
                            Reason = Convert.ToString(Get(rd, "reason")) ?? "",
                        });
                    }
                    if (prop.Ranked.Count == 0) continue;
                    Candidate top = prop.Ranked[0];
                    if (AutoApplyEnabled && top.Confidence >= threshold && TagHelperMethods.RetrieveEntityFromTag(top.Path) != null)
                    {
                        ApplyCore(top.Path, new List<uint> { handle }, TagGlobals.autoLayer, TagGlobals.autoRename);
                        LogDecision(handle, top.Path, top.Path, "auto: " + top.Reason);
                        applied.Add(handle.ToString(CultureInfo.InvariantCulture));
                        _pending.Remove(handle);
                    }
                    else
                    {
                        _pending[handle] = prop;
                        queued++;
                    }
                }
                RefreshProposalReview();
                var sb = new StringBuilder();
                sb.Append("{\"ok\":true,\"applied\":[").Append(string.Join(",", applied.ToArray())).Append(']');
                sb.Append(",\"queued\":").Append(queued).Append(",\"pending\":").Append(_pending.Count).Append('}');
                if (rejected > 0) return sb.ToString().TrimEnd('}') + ",\"rejected\":" + rejected + "}";
                return sb.ToString();
            });
        }

        /// <summary>Proposals waiting for confirmation, newest last.</summary>
        public static string Pending()
        {
            return Guard(() =>
            {
                var sb = new StringBuilder();
                sb.Append("{\"ok\":true,\"pending\":[");
                bool first = true;
                foreach (Proposal p in _pending.Values)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    IINode node = MaxPluginUtilities.GetNodeByHandle(p.Handle);
                    sb.Append("{\"handle\":").Append(p.Handle);
                    sb.Append(",\"name\":").Append(Str(node == null ? "" : node.Name));
                    sb.Append(",\"ranked\":[");
                    for (int i = 0; i < p.Ranked.Count; i++)
                    {
                        if (i > 0) sb.Append(',');
                        sb.Append("{\"path\":").Append(Str(p.Ranked[i].Path));
                        sb.Append(",\"confidence\":").Append(Num(p.Ranked[i].Confidence));
                        sb.Append(",\"reason\":").Append(Str(p.Ranked[i].Reason)).Append('}');
                    }
                    sb.Append("]}");
                }
                sb.Append("]}");
                return sb.ToString();
            });
        }

        /// <summary>Confirm a pending proposal: apply candidate <paramref name="index"/> (0 = top) and log the decision.</summary>
        public static string AcceptPending(uint handle, int index)
        {
            return Guard(() =>
            {
                RequireReady();
                Proposal p;
                if (!_pending.TryGetValue(handle, out p)) throw new ArgumentException("no pending proposal for handle " + handle);
                ProposalTicket ticket;
                if (!TryTicket(handle, p.Token, out ticket))
                {
                    RequestEvidence(handle.ToString());
                    throw new ArgumentException("proposal changed; fresh suggestions requested");
                }
                if (index < 0 || index >= p.Ranked.Count) throw new ArgumentException("candidate index out of range");
                Candidate chosen = p.Ranked[index];
                string result = ApplyCore(chosen.Path, new List<uint> { handle }, TagGlobals.autoLayer, TagGlobals.autoRename);
                LogDecision(handle, chosen.Path, p.Ranked[0].Path, "confirmed: " + chosen.Reason);
                _pending.Remove(handle);
                _tickets.Remove(handle);
                RefreshProposalReview();
                return result;
            });
        }

        /// <summary>Drop pending proposals; pass an empty string to drop all.</summary>
        public static string ClearPending(string handlesCsv)
        {
            return Guard(() =>
            {
                var drop = string.IsNullOrWhiteSpace(handlesCsv) ? _pending.Keys.ToList() : ParseHandles(handlesCsv);
                foreach (uint h in drop) { LogDecision(h, "", "", "ignored:"); _pending.Remove(h); _tickets.Remove(h); _outbox.Remove(h); }
                RefreshProposalReview();
                return "{\"ok\":true,\"pending\":" + _pending.Count + "}";
            });
        }

        /// <summary>Apply an entity path to handles, creating missing branches. One undo step for the scene side.</summary>
        public static string Apply(string path, string handlesCsv, bool layer, bool rename)
        {
            return Guard(() =>
            {
                RequireReady();
                return ApplyCore(path, ParseHandles(handlesCsv), layer, rename);
            });
        }

        /// <summary>Remove handles from one entity path.</summary>
        public static string Remove(string path, string handlesCsv)
        {
            return Guard(() =>
            {
                RequireReady();
                TagNode entity = TagHelperMethods.RetrieveEntityFromTag(path);
                if (entity == null) throw new ArgumentException("unknown entity path: " + path);
                List<uint> handles = ParseHandles(handlesCsv);
                var removed = handles.Where(h => entity.Nodes.Contains(h)).ToList();
                IHold hold = MaxPluginUtilities.Global.TheHold;
                bool holding = false;
                try
                {
                    hold.Begin();
                    holding = true;
                    if (removed.Count > 0) hold.Put(new MembershipRestore(entity, new List<uint>(), removed));
                    TagMethods.RemoveObjects(new List<TagNode> { entity }, handles);
                    hold.Accept("Remove from entity " + entity.LongName);
                    holding = false;
                }
                finally
                {
                    if (holding) hold.Cancel();
                }
                return "{\"ok\":true,\"path\":" + Str(entity.LongName) + ",\"removed\":" + removed.Count + "}";
            });
        }

        /// <summary>Record a decision made outside Present(), for the neighbour index and audit log.</summary>
        public static string Accept(uint handle, string chosen, string proposed, string reason)
        {
            return Guard(() =>
            {
                LogDecision(handle, chosen, proposed, reason);
                return "{\"ok\":true}";
            });
        }

        /// <summary>Forget cached node boxes, for example after a scripted transform burst.</summary>
        public static string InvalidateCache()
        {
            return Guard(() =>
            {
                int n = _nodeBoxes.Count;
                _nodeBoxes.Clear();
                return "{\"ok\":true,\"dropped\":" + n + "}";
            });
        }

        /// <summary>
        /// Development helper: bring the plugin up when the assembly was loaded by script instead of
        /// from binssemblies (where Max calls IPlugin.Initialize and AssemblyMain itself). Returns the
        /// full exception text on failure so the cause is visible from MAXScript.
        /// </summary>
        public static string Bootstrap()
        {
            try
            {
                if (TagCenter.Instance == null)
                {
                    var tc = new TagCenter();
                    tc.Initialize(GlobalInterface.Instance, null);
                }
                if (TagGlobals.tagCenter != null && TagGlobals.tagCenter._descriptor == null)
                {
                    AssemblyFunctions.AssemblyMain();
                }
                return Version();
            }
            catch (Exception ex)
            {
                return "{\"ok\":false,\"error\":" + Str(ex.ToString()) + "}";
            }
        }

        /// <summary>Path of the decision log, one JSON object per line.</summary>
        public static string DecisionLogPath
        {
            get { return Path.Combine(MaxPluginUtilities.GetMaxDir(MaxDirectory.PlugcfgLn), "TagManager_decisions.jsonl"); }
        }

        // ------------------------------------------------------------------ core operations

        private static string ApplyCore(string path, List<uint> handles, bool layer, bool rename)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("empty entity path");
            var valid = new List<uint>();
            var missing = new List<uint>();
            foreach (uint h in handles)
            {
                if (MaxPluginUtilities.GetNodeByHandle(h) != null) valid.Add(h); else missing.Add(h);
            }
            if (valid.Count == 0) throw new ArgumentException("no valid handles");

            TagNode entity = TagHelperMethods.RetrieveEntityFromTag(path);
            bool created = false;
            if (entity != null && entity.IsShortcut) throw new ArgumentException("entity is a shortcut: " + path);
            IHold hold = MaxPluginUtilities.Global.TheHold;
            bool holding = false;
            try
            {
                hold.Begin();
                holding = true;
                if (entity == null)
                {
                    var previous = new HashSet<Guid>(Entities().Select(e => e.ID));
                    entity = TagHelperMethods.GetLonguestMatchingTag(path, true, null);
                    var branches = Entities().Where(e => !previous.Contains(e.ID)).ToList();
                    if (branches.Count > 0) hold.Put(new BranchRestore(branches));
                    created = true;
                }
                if (entity == null || TagGlobals.baseNames.Contains(entity.Name)) throw new ArgumentException("could not resolve entity path: " + path);
                var added = valid.Where(h => !entity.Nodes.Contains(h)).ToList();
                if (added.Count > 0) hold.Put(new MembershipRestore(entity, added, new List<uint>()));
                TagMethods.ApplyEntities(new List<TagNode> { entity }, valid, layer);
                if (rename && entity.IsNameable) TagMethods.RenameUsingStructure(valid);
                hold.Accept("Apply entity " + entity.LongName);
                holding = false;
            }
            finally
            {
                if (holding) hold.Cancel();
            }

            var sb = new StringBuilder();
            sb.Append("{\"ok\":true,\"path\":").Append(Str(entity.LongName));
            sb.Append(",\"created\":").Append(created ? "true" : "false");
            sb.Append(",\"applied\":").Append(valid.Count);
            sb.Append(",\"missing\":[").Append(string.Join(",", missing.Select(m => m.ToString(CultureInfo.InvariantCulture)).ToArray())).Append(']');
            sb.Append(",\"count\":").Append(entity.Nodes.Count).Append('}');
            return sb.ToString();
        }

        private static List<Candidate> Containment(Box obj, List<TagNode> entities, Dictionary<Guid, Box> boxes, Dictionary<Guid, int> depth)
        {
            var result = new List<Candidate>();
            if (!obj.Valid) return result;
            float objVol = obj.Volume();
            float cx = (obj.X0 + obj.X1) * 0.5f, cy = (obj.Y0 + obj.Y1) * 0.5f, cz = (obj.Z0 + obj.Z1) * 0.5f;
            foreach (TagNode e in entities)
            {
                Box eb;
                if (!boxes.TryGetValue(e.ID, out eb) || !eb.Valid) continue;
                float inter = Box.IntersectionVolume(obj, eb);
                if (inter <= 0) continue;
                float overlap = inter / objVol;
                float sizeRatio = objVol / eb.Volume();
                if (sizeRatio > SizeRatioCap) continue;
                result.Add(new Candidate
                {
                    Path = e.LongName,
                    Overlap = overlap,
                    Depth = depth[e.ID],
                    SizeRatio = sizeRatio,
                    CenterInside = eb.ContainsPoint(cx, cy, cz),
                });
            }
            // deepest, best-contained first; the ranker sees the numbers and decides
            result.Sort((a, b) =>
            {
                int c = b.Overlap.CompareTo(a.Overlap);
                if (c != 0) return c;
                c = b.Depth.CompareTo(a.Depth);
                if (c != 0) return c;
                return a.SizeRatio.CompareTo(b.SizeRatio);
            });
            if (result.Count > MaxCandidates) result.RemoveRange(MaxCandidates, result.Count - MaxCandidates);
            return result;
        }

        // ------------------------------------------------------------------ bounding boxes

        private struct Box
        {
            public float X0, Y0, Z0, X1, Y1, Z1;
            public bool Valid;

            public static Box Invalid { get { return new Box(); } }

            public static Box FromPoints(float x0, float y0, float z0, float x1, float y1, float z1)
            {
                var b = new Box { X0 = x0, Y0 = y0, Z0 = z0, X1 = x1, Y1 = y1, Z1 = z1, Valid = true };
                b.Pad();
                return b;
            }

            private void Pad()
            {
                float e = MinExtent;
                if (X1 - X0 < e) { float c = (X0 + X1) * 0.5f; X0 = c - e * 0.5f; X1 = c + e * 0.5f; }
                if (Y1 - Y0 < e) { float c = (Y0 + Y1) * 0.5f; Y0 = c - e * 0.5f; Y1 = c + e * 0.5f; }
                if (Z1 - Z0 < e) { float c = (Z0 + Z1) * 0.5f; Z0 = c - e * 0.5f; Z1 = c + e * 0.5f; }
            }

            public void Include(Box o)
            {
                if (!o.Valid) return;
                if (!Valid) { this = o; return; }
                if (o.X0 < X0) X0 = o.X0; if (o.Y0 < Y0) Y0 = o.Y0; if (o.Z0 < Z0) Z0 = o.Z0;
                if (o.X1 > X1) X1 = o.X1; if (o.Y1 > Y1) Y1 = o.Y1; if (o.Z1 > Z1) Z1 = o.Z1;
            }

            public float Volume()
            {
                return Math.Max(X1 - X0, MinExtent) * Math.Max(Y1 - Y0, MinExtent) * Math.Max(Z1 - Z0, MinExtent);
            }

            public bool ContainsPoint(float x, float y, float z)
            {
                return x >= X0 && x <= X1 && y >= Y0 && y <= Y1 && z >= Z0 && z <= Z1;
            }

            public static float IntersectionVolume(Box a, Box b)
            {
                float dx = Math.Min(a.X1, b.X1) - Math.Max(a.X0, b.X0);
                float dy = Math.Min(a.Y1, b.Y1) - Math.Max(a.Y0, b.Y0);
                float dz = Math.Min(a.Z1, b.Z1) - Math.Max(a.Z0, b.Z0);
                if (dx <= 0 || dy <= 0 || dz <= 0) return 0;
                return dx * dy * dz;
            }

            public string ToJson()
            {
                if (!Valid) return "null";
                return "[[" + Num(X0) + "," + Num(Y0) + "," + Num(Z0) + "],[" + Num(X1) + "," + Num(Y1) + "," + Num(Z1) + "]]";
            }

            public string SizeJson()
            {
                if (!Valid) return "null";
                return "[" + Num(X1 - X0) + "," + Num(Y1 - Y0) + "," + Num(Z1 - Z0) + "]";
            }
        }

        private struct CachedBox
        {
            public Box Box;
            public long Tick;
        }

        private static readonly Dictionary<uint, CachedBox> _nodeBoxes = new Dictionary<uint, CachedBox>();
        private static readonly Stopwatch _clock = Stopwatch.StartNew();

        /// <summary>World box of one node, cached for BoxCacheMaxAgeMs. EvalWorldState is the expensive part.</summary>
        private static Box NodeBox(IINode node, int t)
        {
            uint h = node.Handle;
            CachedBox cached;
            long now = _clock.ElapsedMilliseconds;
            if (_nodeBoxes.TryGetValue(h, out cached) && now - cached.Tick < BoxCacheMaxAgeMs) return cached.Box;

            Box result = Box.Invalid;
            try
            {
                IGlobal g = MaxPluginUtilities.Global;
                IMatrix3 tm = node.GetObjectTM(t, null);
                IObjectState os = node.EvalWorldState(t, true);
                IObject obj = os != null ? os.Obj : null;
                if (obj != null)
                {
                    IBox3 box = g.Box3.Create();
                    box.Init();
                    obj.GetDeformBBox(t, box, tm, false);
                    if (box.IsInitialized && !box.IsEmpty)
                    {
                        IPoint3 mn = box.Min, mx = box.Max;
                        result = Box.FromPoints(mn.X, mn.Y, mn.Z, mx.X, mx.Y, mx.Z);
                    }
                }
                if (!result.Valid)
                {
                    IPoint3 p = tm.Trans;
                    result = Box.FromPoints(p.X, p.Y, p.Z, p.X, p.Y, p.Z);
                }
            }
            catch
            {
                result = Box.Invalid;
            }
            _nodeBoxes[h] = new CachedBox { Box = result, Tick = now };
            return result;
        }

        /// <summary>Union box per entity over its whole branch, excluding the handles being evaluated.</summary>
        private static Dictionary<Guid, Box> EntityBoxes(List<TagNode> entities, HashSet<uint> exclude)
        {
            int t = Now();
            var own = new Dictionary<Guid, Box>(entities.Count);
            foreach (TagNode e in entities)
            {
                Box b = Box.Invalid;
                foreach (uint h in e.Nodes)
                {
                    if (exclude != null && exclude.Contains(h)) continue;
                    IINode node = MaxPluginUtilities.GetNodeByHandle(h);
                    if (node == null) continue;
                    b.Include(NodeBox(node, t));
                }
                own[e.ID] = b;
            }
            var branch = new Dictionary<Guid, Box>(entities.Count);
            foreach (TagNode e in entities) BranchBox(e, own, branch);
            foreach (var zone in _zones)
            {
                Box b = Box.Invalid;
                foreach (uint h in zone.Value)
                {
                    var node = MaxPluginUtilities.GetNodeByHandle(h);
                    if (node != null) b.Include(NodeBox(node, t));
                }
                if (b.Valid) branch[zone.Key] = b;
            }
            return branch;
        }

        private static Box BranchBox(TagNode e, Dictionary<Guid, Box> own, Dictionary<Guid, Box> memo)
        {
            Box b;
            if (memo.TryGetValue(e.ID, out b)) return b;
            Box o;
            b = own.TryGetValue(e.ID, out o) ? o : Box.Invalid;
            foreach (TagNode c in e.Children) b.Include(BranchBox(c, own, memo));
            memo[e.ID] = b;
            return b;
        }

        // ------------------------------------------------------------------ tree helpers

        private static bool IsReady()
        {
            return TagGlobals.root != null && TagGlobals.project != null && TagGlobals.tagCenter != null;
        }

        private static void RequireReady()
        {
            if (!IsReady()) throw new InvalidOperationException("TagManager is not initialized in this session");
        }

        /// <summary>All entities below Project, in tree order. Root and Project are excluded.</summary>
        private static List<TagNode> Entities()
        {
            var result = new List<TagNode>();
            Collect(TagGlobals.project, result);
            return result;
        }

        private static void Collect(TagNode parent, List<TagNode> into)
        {
            foreach (TagNode c in parent.Children)
            {
                if (!TagGlobals.baseNames.Contains(c.Name)) into.Add(c);
                Collect(c, into);
            }
        }

        private static int Depth(TagNode e)
        {
            int d = 0;
            TagNode p = e;
            while (p.Parent != null && !TagGlobals.baseNames.Contains(p.Name)) { d++; p = p.Parent; }
            return d;
        }

        /// <summary>One pass over the tree: handle to owning entities. Replaces the per-entity Intersect scans.</summary>
        private static Dictionary<uint, List<TagNode>> ReverseIndex(List<TagNode> entities)
        {
            var index = new Dictionary<uint, List<TagNode>>();
            foreach (TagNode e in entities)
            {
                foreach (uint h in e.Nodes)
                {
                    List<TagNode> owners;
                    if (!index.TryGetValue(h, out owners)) index[h] = owners = new List<TagNode>(1);
                    owners.Add(e);
                }
            }
            return index;
        }

        private static int Now()
        {
            return MaxPluginUtilities.Interface.Time;
        }

        private static string ClassNameOf(IINode node, int t)
        {
            try
            {
                IObjectState os = node.EvalWorldState(t, true);
                return os != null && os.Obj != null ? os.Obj.ClassName(false) : "";
            }
            catch { return ""; }
        }

        private static string MaterialNameOf(IINode node)
        {
            try { IMtl m = node.Mtl; return m != null ? m.Name : ""; }
            catch { return ""; }
        }

        private static string LayerNameOf(IINode node)
        {
            try { IILayer l = MaxPluginUtilities.GetObjectLayer(node); return l != null ? l.Name : ""; }
            catch { return ""; }
        }

        private static string ParentNameOf(IINode node)
        {
            try
            {
                IINode p = node.ParentNode;
                return p != null && !p.IsRootNode ? p.Name : "";
            }
            catch { return ""; }
        }

        // ------------------------------------------------------------------ undo support

        /// <summary>
        /// Puts entity membership into the Max undo stack. The tree lives in managed memory, so
        /// without this Ctrl+Z would revert the rename and layer move but leave the tag behind.
        /// </summary>
        private sealed class MembershipRestore : Autodesk.Max.Plugins.RestoreObj
        {
            private readonly TagNode _entity;
            private readonly List<uint> _added;
            private readonly List<uint> _removed;

            public MembershipRestore(TagNode entity, List<uint> added, List<uint> removed)
            {
                _entity = entity;
                _added = added;
                _removed = removed;
            }

            public override void Restore(bool isUndo)
            {
                if (_added.Count > 0) _entity.Nodes.RemoveRange(_added);
                if (_removed.Count > 0) _entity.Nodes.AddRange(_removed, true);
                if (isUndo) foreach (uint h in _added) LogDecision(h, "", "", "retracted:");
            }

            public override void Redo()
            {
                if (_added.Count > 0) _entity.Nodes.AddRange(_added, true);
                if (_removed.Count > 0) _entity.Nodes.RemoveRange(_removed);
            }

            public override void EndHold() { }
            public override int Size { get { return 64; } }
            public override string Description { get { return "TagManager entity membership"; } }
        }

        // ------------------------------------------------------------------ proposals and log

        private class Candidate
        {
            public double GeometryScore;
            public bool ProposedHierarchy;
            public string Path;
            public float Overlap;
            public int Depth;
            public float SizeRatio;
            public bool CenterInside;
            public double Confidence;
            public string Reason;
        }

        private class Proposal
        {
            public uint Handle;
            public string Token;
            public List<Candidate> Ranked;
        }

        private static readonly Dictionary<uint, Proposal> _pending = new Dictionary<uint, Proposal>();

        private static void LogDecision(uint handle, string chosen, string proposed, string reason)
        {
            try
            {
                IINode node = MaxPluginUtilities.GetNodeByHandle(handle);
                var sb = new StringBuilder();
                sb.Append("{\"ts\":").Append(Str(DateTime.UtcNow.ToString("o")));
                sb.Append(",\"handle\":").Append(handle);
                sb.Append(",\"name\":").Append(Str(node == null ? "" : node.Name));
                sb.Append(",\"class\":").Append(Str(node == null ? "" : ClassNameOf(node, Now())));
                sb.Append(",\"chosen\":").Append(Str(chosen ?? ""));
                sb.Append(",\"proposed\":").Append(Str(proposed ?? ""));
                sb.Append(",\"reason\":").Append(Str(reason ?? ""));
                ProposalTicket ticket;
                if (_tickets.TryGetValue(handle, out ticket))
                    sb.Append(",\"evidence\":").Append(ticket.EvidenceJson);
                sb.Append(",\"session\":").Append(Str(_session));
                string kind = reason != null && reason.StartsWith("auto:") ? "automatic" :
                    reason != null && reason.StartsWith("ignored:") ? "ignored" :
                    reason != null && reason.StartsWith("retracted:") ? "retracted" : "confirmed";
                sb.Append(",\"kind\":").Append(Str(kind));
                sb.Append('}');
                File.AppendAllText(DecisionLogPath, sb.ToString() + Environment.NewLine, new UTF8Encoding(false));
            }
            catch
            {
                // the log is advisory; never fail an apply because of it
            }
        }

        // ------------------------------------------------------------------ parsing and JSON

        private static List<uint> ParseHandles(string csv)
        {
            var result = new List<uint>();
            if (string.IsNullOrWhiteSpace(csv)) return result;
            foreach (string part in csv.Split(new[] { ',', ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                uint h;
                if (uint.TryParse(part.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out h) && h != 0 && !result.Contains(h))
                    result.Add(h);
            }
            return result;
        }

        private static object Get(Dictionary<string, object> d, string key)
        {
            object v;
            return d.TryGetValue(key, out v) ? v : null;
        }

        private static double ToDouble(object o, double fallback)
        {
            if (o == null) return fallback;
            try { return Convert.ToDouble(o, CultureInfo.InvariantCulture); }
            catch { return fallback; }
        }

        private static string Num(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) return "0";
            return v.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static string Str(string s)
        {
            if (s == null) return "\"\"";
            var sb = new StringBuilder(s.Length + 2);
            sb.Append('"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('"');
            return sb.ToString();
        }

        private static string Guard(Func<string> body)
        {
            try
            {
                return body();
            }
            catch (Exception ex)
            {
                return "{\"ok\":false,\"error\":" + Str(ex.GetType().Name + ": " + ex.Message) + "}";
            }
        }
    }
}
