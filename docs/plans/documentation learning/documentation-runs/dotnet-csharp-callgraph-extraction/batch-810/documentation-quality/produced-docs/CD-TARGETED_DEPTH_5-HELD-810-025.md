# Documentation: DynamicController.GetDocuments

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\DynamicController.cs
- Symbol: DynamicController.GetDocuments
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ ActionResult response = Unauthorized(); var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null) { if (index_name == "userstore" || index_name == "greetings" || index_name == "requests") { QueryContainer queryContainer = new QueryContainer(); SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>(index_name); List<QueryContainer> queryContainerList = new List<QueryContainer>(); queryContainerList = CreateQueryContainerList("", document.ToString(), queryContainerList); if (!users.IsAdmin) { if (index_name == "userstore") { return Ok(new { status = false, error = "invalid index" }); } if (index_name == "greetings") { MatchPhrasePrefixQuery query = new MatchPhrasePrefixQuery() { Field = "UserId", Query = $"*{users.UserName}*" }; queryContainerList.Add(query); } else { MatchPhrasePrefixQuery query =

## Delegated Callee Behavior
- DynamicController.CreateQueryContainerList: Accepted callee meaning for DynamicController.CreateQueryContainerList: { var result = qlist; if (Document.IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document); for (var 
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use
- DynamicController.CreateQueryContainerList: Accepted callee meaning for DynamicController.CreateQueryContainerList: { var result = qlist; if (Document.IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document); for (var 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
