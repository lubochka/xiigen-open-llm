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
{ if (titlesRow > -1) { CellElastic titleCell = GetCellByTitle(title, rows[titlesRow].cells); if (titleCell != null) { CellElastic productCell = rows[i].cells.Find(a => Math.Abs(a.X - titleCell.X) < 10); if (productCell != null) { productCell.type = titleCell.type; if (titleCell.type == "regular") { return productCell.word; } else { return productCell.worInverted; } } } } return ""; }

## Delegated Callee Behavior
- ProcessDataResults.GetCellByTitle: Accepted callee meaning for ProcessDataResults.GetCellByTitle: { CellElastic res = new CellElastic(); int length = -1; foreach (CellElastic cell in cells) { if (cell.worInverted.IndexOf(title) > -1 && cell.worInverted.IndexOf(title2) > -1) { i
- ProcessDataResults.GetCellByTitle: Accepted callee meaning for ProcessDataResults.GetCellByTitle: { CellElastic res = new CellElastic(); int length = -1; foreach (CellElastic cell in cells) { if (cell.worInverted.IndexOf(title) > -1 && cell.worInverted.IndexOf(title2) > -1) { i

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
