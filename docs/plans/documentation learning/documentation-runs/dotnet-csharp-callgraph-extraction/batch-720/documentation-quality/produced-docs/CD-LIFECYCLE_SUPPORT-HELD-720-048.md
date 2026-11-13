# Documentation: DynamicController.GetDocument

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\DynamicController.cs
- Symbol: DynamicController.GetDocument
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ ActionResult response = Unauthorized(); var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null) { QueryContainer queryContainer = new QueryContainer(); SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>(index_name).Query(q => q.Match(a => a.Field("_id").Query(id))); // SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>(index_name).QueryOnQueryString("{'query':{'match':{'_id':'" + id + "'}}}"); ISearchResponse<object> responseInternal = _client.Search<object>(searchDescriptor); if (responseInternal.Hits.Count > 0) { return Ok(responseInternal.Hits.Where(a => true).FirstOrDefault().Source); } else { return Ok(new { status = "no results" }); } } return response; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
