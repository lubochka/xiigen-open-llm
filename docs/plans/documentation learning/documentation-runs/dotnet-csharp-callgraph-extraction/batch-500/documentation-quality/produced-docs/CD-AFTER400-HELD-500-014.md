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

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
