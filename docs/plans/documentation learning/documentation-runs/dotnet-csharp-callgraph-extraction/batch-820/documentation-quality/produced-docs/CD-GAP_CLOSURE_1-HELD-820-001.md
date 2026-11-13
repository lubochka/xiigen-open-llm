# Documentation: ProcessDataResults.BuildFollowing

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.BuildFollowing
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<List<CellExport>> results = new List<List<CellExport>>(); if (products.Count > 0) { products = products.OrderBy(a => a.company).ToList(); List<CellExport> rexp = new List<CellExport>(); var company = products[0].company; int rowTitleNum = 1; int rowProductNum = 2; int column = 1; for (var i = 0; i<products.Count;i++) { column = 1; if (company!= products[i].company) { results.Add(rexp); rexp = new List<CellExport>(); company = products[i].company; rowTitleNum = 1; rowProductNum = 2; } rexp.Add(new CellExport() { column = column, row = rowTitleNum, value = "שם מלא" }); rexp.Add(new CellExport() { column = column, row = rowProductNum, value = products[i].fullName }); column++; rexp.Add(new CellExport() { column = column, row = rowTitleNum, value = "תעודת זהות" }); rexp.Add(new CellExport() { column = column, row = rowProductNum, value = products[i].id }); column++; rexp.Add(new CellE

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
