# Documentation: LoginController.GetStatUserByClaim

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\LoginController.cs
- Symbol: LoginController.GetStatUserByClaim
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("userid", id, client); return users; }

## Delegated Callee Behavior
- LoginController.GetClaim: Accepted callee meaning for LoginController.GetClaim: { if (currentUser.HasClaim(c => c.Type == claim)) { return currentUser.Claims.FirstOrDefault(c => c.Type == claim).Value; } return null; }
- LoginController.getStatUser: Accepted callee meaning for LoginController.getStatUser: { SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>("userstore").Query(q => q.Match(a => a.Field(field).Query(value))); ISearchResponse<object> response = cl

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
