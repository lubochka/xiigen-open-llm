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

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
