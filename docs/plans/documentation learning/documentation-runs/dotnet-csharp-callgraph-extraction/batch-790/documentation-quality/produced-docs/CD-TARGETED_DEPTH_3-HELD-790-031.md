# Documentation: DynamicController.GetFilters

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\DynamicController.cs
- Symbol: DynamicController.GetFilters
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ ActionResult response = Unauthorized(); var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null) { try { if (index_name == "userstore" || index_name == "greetings" || index_name == "requests") { // dynamic json = JsonConvert.DeserializeObject<dynamic>(document.ToString()); SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>(index_name); List<QueryContainer> queryContainerList = new List<QueryContainer>(); if (!users.IsAdmin) { if(index_name=="userstore") { return Ok(new { status = false, error = "invalid index" }); } MatchPhrasePrefixQuery query = new MatchPhrasePrefixQuery() { Field = "userId", Query = $"*{users.UserName}*" }; queryContainerList.Add(query); } searchDescriptor = searchDescriptor.Query(q => q.Bool(b => b.Filter(queryContainerList.ToArray()))).Aggregations(a => a.Terms(fieldNam

## Delegated Callee Behavior
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
