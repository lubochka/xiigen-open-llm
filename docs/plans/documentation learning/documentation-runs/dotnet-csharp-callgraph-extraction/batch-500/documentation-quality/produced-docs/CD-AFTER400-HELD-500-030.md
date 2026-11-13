# Documentation: a1BreezeController.Stages

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.Stages
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Stage>().AsQueryable(); try { return _db.Context.Stages.OrderBy(s => s.LoopId).ThenBy(s => s.SortOrder).ThenBy(s => s.Id).Where(e => e.MainUserId == myUser.MainUserId); } catch (Exception em) { return new List<Stage>().AsQueryable(); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
