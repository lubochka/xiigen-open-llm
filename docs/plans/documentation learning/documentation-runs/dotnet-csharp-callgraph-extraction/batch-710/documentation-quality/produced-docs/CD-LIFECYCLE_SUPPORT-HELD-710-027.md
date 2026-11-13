# Documentation: ConvertController.ConvertFile

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\ConvertController.cs
- Symbol: ConvertController.ConvertFile
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null && users.IsAdmin) { string mapp = _mappService.Documents("firstdocument.json"); if (file != null) { var mapping = JsonConvert.DeserializeObject<MapModel>(mapp); var result1 = _client.Indices.Delete("requests"); List<object> data = excelConverter.ConvertRequestsToJSON(file.OpenReadStream(), mapping, pass); object res = ""; if (data.Count > 0) { res = JsonConvert.SerializeObject(data); } return Ok(res); } else { return Ok(new { error = "no file" }); } } return Unauthorized(); }

## Delegated Callee Behavior
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use
- MappService.Documents: Accepted callee meaning for MappService.Documents: { string res = ""; if (Documents().TryGetValue(name, out res)) { return res; } throw new Exception("You don't have a json for mapp"); }
- MapModel.ConvertRequestsToJSON: Accepted callee meaning for MapModel.ConvertRequestsToJSON: { using (var reader = ExcelReaderFactory.CreateReader(fileStream, new ExcelReaderConfiguration() { FallbackEncoding = Encoding.UTF8, Password = password })) { var result = GetReque

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
