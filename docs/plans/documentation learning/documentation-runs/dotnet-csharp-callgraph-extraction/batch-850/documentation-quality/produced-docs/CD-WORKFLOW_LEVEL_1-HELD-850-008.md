# Documentation: DynamicController.GetDocuments

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DynamicController.cs
- Symbol: DynamicController.GetDocuments
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ QueryContainer queryContainer = new QueryContainer(); SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>(index_name); List<QueryContainer> queryContainerList = new List<QueryContainer>(); queryContainerList= CreateQueryContainerList("", document.ToString(), queryContainerList); searchDescriptor = searchDescriptor.Query(q=>q.Bool(b=>b.Filter(queryContainerList.ToArray()))) ; ISearchResponse<object> response = _client.Search<object>(searchDescriptor); if (response.Hits.Count > 0) { return Ok(response.Hits.Select(a=>a.Source).ToList()); } else { return Ok(new { status = "no results" }); } }

## Delegated Callee Behavior
- DynamicController.CreateQueryContainerList: Accepted callee meaning for DynamicController.CreateQueryContainerList: { var result = qlist; if (Document.IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document); for (var 
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use
- DynamicController.CreateQueryContainerList: Accepted callee meaning for DynamicController.CreateQueryContainerList: { var result = qlist; if (Document.IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document); for (var 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
