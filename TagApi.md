# TagApi - string facade for MAXScript and the MCP bridge

## API 1.2 inline architectural suggestions

Suggestions now require confirmation by default (`AutoApplyEnabled=false`). `Present`
requires `session` and a `token` on each proposal. Obtain them with
`RequestEvidence(handlesCsv)` or `ProposalBatch()`; old unversioned requests are rejected.
Tickets expire after two minutes and are checked against fresh evidence and the entity tree.

Creation callbacks queue new objects. A background-priority dispatcher collects provisional
shape/spatial suggestions after a 750 ms settling delay. FastTag shows them directly in its
existing autocomplete dropdown before typing: no button and no separate suggestions window.
`ReviewProposals()` now opens FastTag for the current selection. Down/Up then Enter or slash
accepts a suggestion; untouched Enter accepts the first. Typing preserves legacy filtering
and Enter-to-select behaviour. Escape dismisses. No selected objects shows architectural
starter paths, and the first untagged object works without any existing entities.

`Evidence` adds `sizeMeters`, `boundsVolumeM3` (world AABB volume, not mesh volume), and
architectural candidates with `geometryScore`, `reason` and `proposedHierarchy`. New paths
are accepted by `Present` only when issued in the object's evidence ticket. They cannot be
auto-applied even with opt-in: confirmation creates the hierarchy in one undo step.
`AcceptFastTag(handlesCsv,path)` checks all tickets and the current selection before applying.
Architectural guesses cover structural/envelope/interior elements, furniture, museum display
elements, station spaces and site hardscape. They are uncertain shape priors, not recognition.
`ProposalBatch()` provides bounded queued work and the decision-log path to an external
ranker. The Python MCP worker adds semantic neighbours and local LLM ranking.

`SetEntityZone(path, handlesCsv)` overrides inferred bounds with explicit volume nodes for
the current session. Empty handles removes the binding. These bindings are not persisted.

Scene resets clear proposals, tickets and boxes. Evidence refreshes boxes each batch,
including changes made outside TagManager. Accepted decisions retain original evidence;
undo retracts the learning example. New branches now participate in the apply undo step.

`TagManager.TagApi` is a static class that exposes the entity tree to scripts and to the
3dsmax-mcp server without any .NET collection types crossing the boundary. Handles go in as a
comma separated string, everything comes back as a JSON string, and no call ever throws:
failures return `{"ok":false,"error":"..."}`.

```maxscript
api = dotNetClass "TagManager.TagApi"
api.ListEntities()
api.Evidence "1943,1950"
api.Apply "Building_Floor1_Kitchen" "1943" true true   -- path, handles, layer, rename
```

All calls must run on the Max main thread. MAXScript `dotNetClass` calls, and MAXScript sent
through the MCP bridge, already do.

## Calls

| Call | Returns | Notes |
|---|---|---|
| `Version()` | api version, settings, cache and pending counts | Works before the plugin is initialized. |
| `ListEntities()` | every entity below Project: path, name, id, depth, count, children, nameable, shortcut | Root and Project are excluded. |
| `GetEntities(handles)` | `{"objects":{"1943":["A_B"], ...}}` | One pass over the tree; unknown handles map to `[]`. |
| `EntityBounds()` | world bbox per entity over its whole branch | Uses the node box cache. |
| `Evidence(handles)` | per handle: name, class, material, layer, parent, bbox, size, current entities, containment `candidates` | The plugin's half of the evidence stage. |
| `Present(rankedJson)` | `{"applied":[...],"queued":n,"pending":n}` | Requires session/tokens. Review-only unless `AutoApplyEnabled` is explicitly enabled. |
| `Pending()` | pending proposals with their ranked candidates | For the FastTag confirm flow. |
| `AcceptPending(handle, index)` | apply result | Applies candidate `index` (0 = top) and logs the decision. |
| `ClearPending(handles)` | pending count | Empty string clears all. |
| `Apply(path, handles, layer, rename)` | `{"path","created","applied","missing","count"}` | Creates missing branches through the longest-match logic. One undo step. |
| `Remove(path, handles)` | removed count | |
| `Accept(handle, chosen, proposed, reason)` | ok | Appends to the decision log. |
| `InvalidateCache()` | dropped count | Forget cached node boxes. |
| `Bootstrap()` | same as `Version()` | Dev only: initializes the plugin when the assembly was loaded by script, see `dev_load.ms`. |

`Present` accepts:

```json
{"session":"from RequestEvidence", "threshold":0.9,
 "proposals":[{"handle":1943,"token":"from RequestEvidence","ranked":[{"path":"Building_Floor1_Kitchen","confidence":0.93,"reason":"..."}]}]}
```

## Evidence record

```json
{"handle":1943,"name":"Box001","class":"Box","mtl":"","layer":"0","parent":"",
 "bbox":[[2.175,1.975,0.2],[2.625,2.425,1.1]],"size":[0.45,0.45,0.9],"entities":[],
 "candidates":[
   {"path":"Building_Floor1_Kitchen","overlap":1,"depth":3,"sizeRatio":0.0337,"centerInside":true},
   {"path":"Building_Floor1","overlap":1,"depth":2,"sizeRatio":0.0017,"centerInside":true}]}
```

`overlap` is the fraction of the object's box inside the entity's branch box, `sizeRatio` the
object volume over the entity volume. Entities the object would fill beyond `SizeRatioCap`
(default 0.5) are not offered. Candidates are ordered deepest and best contained first, capped
at `MaxCandidates` (default 6). The ranker on the server side makes the final call.

## Tunables (static fields)

`MaxCandidates`, `SizeRatioCap`, `BoxCacheMaxAgeMs` (default 5000), `AutoApplyThreshold`
(default 0.9), `MinExtent` (default 0.001, pads flat boxes so volumes never reach zero).

## Decision log

`plugcfg_ln\TagManager_decisions.jsonl`, one object per line:
`{"ts","handle","name","class","chosen","proposed","reason"}`. Written by `Present` on
auto-apply, by `AcceptPending`, and by `Accept`. The neighbour index on the server side is
built from it.

## Measured on a 4-entity, 60-object scene (3ds Max 2025)

| Operation | Time |
|---|---|
| `Evidence` for 50 objects, warm box cache | about 4 ms |
| `Evidence` for 1 object, cold | about 9 ms |
| `ListEntities` or `Version` | about 10 µs per call |
| MCP bridge round trip (named pipe ping) | about 2 ms |

Node bounding boxes are the only expensive part (`EvalWorldState` plus `GetDeformBBox`) and
are cached for `BoxCacheMaxAgeMs`; entity branch boxes are re-unioned from that cache on every
call. Call `InvalidateCache()` after a scripted burst of transforms if fresh boxes matter.
