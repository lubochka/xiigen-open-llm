# Documentation: ProcessDataResults.GetStartRow

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.GetStartRow
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var i = -1; foreach (var row in rows) { i++; foreach (var cell in row.cells) { if(cell.word != "" && cell.word.IndexOf(name)>-1) { return i; } } } return -1; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
