# Documentation: MapModel.ConvertUsersToJSON

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: ExcelReader.Converter\ExcelConverter.cs
- Symbol: MapModel.ConvertUsersToJSON
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (var reader = ExcelReaderFactory.CreateReader(fileStream, new ExcelReaderConfiguration() { FallbackEncoding = Encoding.UTF8, Password = password })) { var result = GetUsersJSON(reader, mappingOptions); // var test = JsonConvert.DeserializeObject(result); return result; } }

## Delegated Callee Behavior
- MapModel.GetUsersJSON: Accepted callee meaning for MapModel.GetUsersJSON: { List<object> results = new List<object>(); var result = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHead

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
