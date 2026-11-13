# Documentation: BoardController.GetStages

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\BoardController.cs
- Symbol: BoardController.GetStages
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); var ActionP = _context.Actions.Where(a => a.Title.Contains(name) && a.Language == language).FirstOrDefault(); if (ActionP != null) { var parent_id = ActionP.Id.ToString(); var stages = this.GetStages(parent_id,0, language); return stages; } else { return new List<Stages>(); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
