# Documentation: DynamicController.StoreDocument

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DynamicController.cs
- Symbol: DynamicController.StoreDocument
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ try { object myDocument = this.ParseObjectAlternative(document); // dynamic json = JsonConvert.DeserializeObject<dynamic>(document.ToString()); IndexRequest<object> request = new IndexRequest<object>(myDocument, index_name, id); var result = _client.Index<object>(request); return Ok(new { status = result.ApiCall.Success }); } catch(Exception ex) { return Ok(new { status = false, error=ex.Message}); } }

## Delegated Callee Behavior
- DynamicController.ParseObjectAlternative: Accepted callee meaning for DynamicController.ParseObjectAlternative: { if (Document.ToString().IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document.ToString()); for (va
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use
- ObjectValue.ParseObjectAlternative: Accepted callee meaning for ObjectValue.ParseObjectAlternative: { if (Document.ToString().IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document.ToString()); for (va
- MapModel.ComputeHash: Accepted callee meaning for MapModel.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- ProcessDataResults.ComputeHash: Accepted callee meaning for ProcessDataResults.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- LoginController.ComputeHash: Accepted callee meaning for LoginController.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
