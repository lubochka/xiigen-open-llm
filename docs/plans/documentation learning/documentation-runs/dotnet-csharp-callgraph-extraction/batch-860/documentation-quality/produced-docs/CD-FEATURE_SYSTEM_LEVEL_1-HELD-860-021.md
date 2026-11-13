# Documentation: MapModel.GetUsersJSON

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: ExcelReader.Converter\ExcelConverter.cs
- Symbol: MapModel.GetUsersJSON
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<object> results = new List<object>(); var result = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true, } }); foreach (DataTable dataTable2 in result.Tables) { var columns = dataTable2.Columns; var rows = dataTable2.Rows; for (int i = 0; i < columns.Count; i++) { var col = map[columns[i].ToString()]; if (col != null && col.NewName != null) { columns[i].ColumnName = col.NewName; } } } DataTable dataTable = result.Tables[0]; DataTable dtCloned = dataTable.Clone(); foreach (DataColumn col in dtCloned.Columns) { var colType = map[col.ColumnName]?.Type; if (!string.IsNullOrEmpty(colType)) { if (col.DataType != Type.GetType("System." + colType)) { col.DataType = Type.GetType("System." + colType); } } } foreach (DataRow row in dataTable.Rows) { var cols = map.Columns.Where(x => x.Values != null && x.Values

## Delegated Callee Behavior
- MapModel.ComputeHash: Accepted callee meaning for MapModel.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- ProcessDataResults.ComputeHash: Accepted callee meaning for ProcessDataResults.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- LoginController.ComputeHash: Accepted callee meaning for LoginController.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
