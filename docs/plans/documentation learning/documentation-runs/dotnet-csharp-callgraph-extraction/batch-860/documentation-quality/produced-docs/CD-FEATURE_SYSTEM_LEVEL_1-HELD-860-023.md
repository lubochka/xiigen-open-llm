# Documentation: ProcessDataResults.GetAllProductsDate

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.GetAllProductsDate
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ ProcessAtamaResults finalRes = new ProcessAtamaResults(); List<ProductData> results = new List<ProductData>(); List<CellExport> nilveSum = new List<CellExport>(); int rowNum = 1; int column = 1; nilveSum.Add(new CellExport() { column = column, row = rowNum, value = "שם מלא" }); nilveSum.Add(new CellExport() { column = column, row = rowNum + 1, value = newRequest.FullName }); column++; nilveSum.Add(new CellExport() { column = column, row = rowNum, value = "תעודת זהות" }); nilveSum.Add(new CellExport() { column = column, row = rowNum + 1, value = newRequest.Id }); column++; if (newRequest.FullName2 != null && newRequest.FullName2 != "") { nilveSum.Add(new CellExport() { column = column, row = rowNum, value = "שם מלא 2" }); nilveSum.Add(new CellExport() { column = column, row = rowNum + 1, value = newRequest.FullName2 }); column++; } if (newRequest.Id2 != null && newRequest.Id2 != "") { n

## Delegated Callee Behavior
- ProcessDataResults.GetStartRow: Accepted callee meaning for ProcessDataResults.GetStartRow: { var i = -1; foreach (var row in rows) { i++; foreach (var cell in row.cells) { if ( (cell.word!=null && cell.word != "" && cell.word.IndexOf(name) > -1 && cell.word.IndexOf(name2
- ProcessDataResults.AddRowData: Accepted callee meaning for ProcessDataResults.AddRowData: { int column = 1; for (var j = 1; j < rows[rowToPrint].cells.Count - 1; j++) { var cell = rows[rowToPrint].cells[j]; nilveSum.Add(new CellExport() { column = column, row = rowNum, 
- ProcessDataResults.GetValueCell: Accepted callee meaning for ProcessDataResults.GetValueCell: { if (titlesRow > -1) { CellElastic titleCell = GetCellByTitle(title, title2, rows[titlesRow].cells); if (titleCell != null && (titleCell.worInverted != null || titleCell.word!=nul

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
