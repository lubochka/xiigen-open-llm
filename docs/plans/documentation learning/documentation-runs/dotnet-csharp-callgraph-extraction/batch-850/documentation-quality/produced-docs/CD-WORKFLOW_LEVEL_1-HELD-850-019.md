# Documentation: ProcessDataResults.GetAgentsData

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.GetAgentsData
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ NewAgentData result = new NewAgentData(); int startRow = GetStartRow("הצעה ל", rows); if (startRow > -1) { foreach (CellElastic cell in rows[startRow].cells) { if (cell.word.IndexOf("הצעה ל") > -1) { var agency = cell.worInverted.Replace("הצעה ל", ""); result.productType = cell.worInverted; } } } int startRow2 = GetStartRow("סוכן", "מספר", rows); if (startRow2 > -1) { var titlesRow = startRow2; result.agentNum = GetValueCell("מספר","סוכן", titlesRow, rows, startRow2 + 1).worInverted; } return result; }

## Delegated Callee Behavior
- ProcessDataResults.GetStartRow: Accepted callee meaning for ProcessDataResults.GetStartRow: { var i = -1; foreach (var row in rows) { i++; foreach (var cell in row.cells) { if ( (cell.word!=null && cell.word != "" && cell.word.IndexOf(name) > -1 && cell.word.IndexOf(name2
- ProcessDataResults.GetValueCell: Accepted callee meaning for ProcessDataResults.GetValueCell: { if (titlesRow > -1) { CellElastic titleCell = GetCellByTitle(title, title2, rows[titlesRow].cells); if (titleCell != null && (titleCell.worInverted != null || titleCell.word!=nul

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
