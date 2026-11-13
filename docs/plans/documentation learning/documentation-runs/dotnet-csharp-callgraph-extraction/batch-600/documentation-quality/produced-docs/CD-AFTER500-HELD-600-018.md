# Documentation: CellExport.JsonToExcel

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: ExcelReader.Converter\JsonToExcelConverter.cs
- Symbol: CellExport.JsonToExcel
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ // DataTable dataTable = JsonConvert.DeserializeObject<DataTable>(jsonObject); var workbook = new XLWorkbook(); IXLWorksheet worksheet = workbook.Worksheets.Add("Data"); int colNumber = 1; int row = 1; foreach (var dictionary in dictionaries) { colNumber = 1; foreach (var key in dictionary.Keys) { worksheet.Cell(row, colNumber++).Value = key; } row++; colNumber = 1; foreach (var value in dictionary.Values) { worksheet.Cell(row, colNumber++).Value = value; } row++; row++; } worksheet.Columns().AdjustToContents(); // Adjust column width worksheet.Rows().AdjustToContents(); using (var stream = new MemoryStream()) { workbook.SaveAs(stream); var content = stream.ToArray(); return content; } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
