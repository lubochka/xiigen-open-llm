# Documentation: LogProgress.ProcessAgentData

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\JobProcess.cs
- Symbol: LogProgress.ProcessAgentData
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ FileDetails file = new FileDetails(); SearchDescriptor<object> searchDescriptorf = new SearchDescriptor<object>("filedetails").Query(q => q.Match(a => a.Field("fileId").Query(guid))); ISearchResponse<object> responsef = _client.Search<object>(searchDescriptorf); if (responsef.Hits.Count > 0) { file = responsef.Hits.Select(x => new FileDetails(x.Source)).FirstOrDefault(); } SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>("rowelastic").Query(q => q.Match(a => a.Field("fileId").Query(guid))).Size(150); ISearchResponse<object> response = _client.Search<object>(searchDescriptor); if (response.Hits.Count > 0) { List<RowElastic> rows = response.Hits.Select(x => new RowElastic(x.Source)).ToList(); List<MemoryStream> attachments = logic.ProcessAgentData(rows, _client, job); return "processed"; } else { return "no file found"; } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
