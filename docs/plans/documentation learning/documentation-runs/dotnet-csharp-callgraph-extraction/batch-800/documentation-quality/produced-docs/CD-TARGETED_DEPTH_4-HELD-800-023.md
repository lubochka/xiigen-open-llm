# Documentation: ProcessDataResults.GetValue

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.GetValue
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var result = ""; foreach(var row in rows) { foreach(var cell in row.cells) { if(cell.word.IndexOf(name)>-1 ) { if(type=="table") { var rowAfter = rows.Find(a => a.num == row.num+additionalRow); var cellAfter = rowAfter.cells.Find(a => Math.Abs(a.X - cell.X) <= 10); if(cellAfter.word!="") { result = cellAfter.word; if(AddInfo=="en") { result = cellAfter.wordEn; } } } if(type=="regular") { var splitted = cell.word.Split(name); if(splitted.Length>1 && AddInfo == "allAfter") { for(var i=1; i<splitted.Length;i++) { if(result!="" && splitted[i]!=" ") { result += " "; } result += splitted[i]; } } else { for (var i = 1; i < splitted.Length; i++) { if (splitted[i]!=AddInfo && result != "" && splitted[i] != " ") { result += " "; } if (splitted[i] != AddInfo) { result += splitted[i]; } else { i = splitted.Length; } } } } } } } return result; }

## Delegated Callee Behavior
- ProcessDataResults.GetCellByTitle: Accepted callee meaning for ProcessDataResults.GetCellByTitle: { CellElastic res = new CellElastic(); int length = -1; foreach (CellElastic cell in cells) { if (cell.worInverted.IndexOf(title) > -1 && cell.worInverted.IndexOf(title2) > -1) { i
- ProcessDataResults.GetCellByTitle: Accepted callee meaning for ProcessDataResults.GetCellByTitle: { CellElastic res = new CellElastic(); int length = -1; foreach (CellElastic cell in cells) { if (cell.worInverted.IndexOf(title) > -1 && cell.worInverted.IndexOf(title2) > -1) { i

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
