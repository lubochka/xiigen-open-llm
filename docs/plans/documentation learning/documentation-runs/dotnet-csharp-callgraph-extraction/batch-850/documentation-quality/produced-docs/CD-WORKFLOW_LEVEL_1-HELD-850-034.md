# Documentation: ConvertController.ConvertFileClients

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\ConvertController.cs
- Symbol: ConvertController.ConvertFileClients
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null && users.IsAdmin) { string mapp = _mappService.Documents("clientsmapp.json"); if (file != null) { var mapping = JsonConvert.DeserializeObject<MapModel>(mapp); List<object> data = excelConverter.ConvertClientsToJSON(file.OpenReadStream(), mapping, pass); object res = ""; if (data.Count > 0) { res = JsonConvert.SerializeObject(data); // var result = _client.IndexMany<object>(data, "requests"); //var bulkIndexResponse = _client.Bulk(b => b // .Index("requests") // .IndexMany(data) //); } return Ok(res); } else { return Ok(new { error = "no file" }); } } return Unauthorized(); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
