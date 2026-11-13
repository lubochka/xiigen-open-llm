# Documentation: ProcessDataResults.GetCellByTitle

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.GetCellByTitle
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ CellElastic res = new CellElastic(); int length = -1; foreach (CellElastic cell in cells) { if (cell.worInverted.IndexOf(title) > -1 && cell.worInverted.IndexOf(title2) > -1) { if (length == -1 || length >= cell.worInverted.Length) { res = cell; res.type = "inverted"; length = cell.worInverted.Length; } } if (cell.word.IndexOf(title) > -1 && cell.word.IndexOf(title2) > -1) { if (length == -1 || length >= cell.word.Length) { res = cell; res.type = "regular"; length = cell.word.Length; } } } return res; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
