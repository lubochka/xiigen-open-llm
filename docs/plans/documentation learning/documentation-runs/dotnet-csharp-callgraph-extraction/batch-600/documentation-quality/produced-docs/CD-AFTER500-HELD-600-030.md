# Documentation: MapModel.GetRequestsJSON

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: ExcelReader.Converter\ExcelConverter.cs
- Symbol: MapModel.GetRequestsJSON
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<object> results = new List<object>(); List<AgentNum> agents = new List<AgentNum>(); SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>("agentnums"); List<QueryContainer> queryContainerList = new List<QueryContainer>(); searchDescriptor = searchDescriptor.Query(q => q.MatchAll()).Size(255); ISearchResponse<object> responseInternal = _client.Search<object>(searchDescriptor); if (responseInternal.Hits.Count > 0) { List<object> agentsObg = responseInternal.Hits.Select(a => a.Source).ToList(); foreach(var agentObj in agentsObg) { agents.Add(new AgentNum(agentObj)); } } var result = reader.AsDataSet(new ExcelDataSetConfiguration() { ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true, } }); foreach (DataTable dataTable2 in result.Tables) { var columns = dataTable2.Columns; var rows = dataTable2.Rows; for (int i = 0; i < columns.Count

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
