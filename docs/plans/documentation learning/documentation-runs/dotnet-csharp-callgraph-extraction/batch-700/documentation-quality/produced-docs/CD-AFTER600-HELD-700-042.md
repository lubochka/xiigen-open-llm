# Documentation: BafiAuthorizeRequest.GetAuthorized

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\BafiLead.cs
- Symbol: BafiAuthorizeRequest.GetAuthorized
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ BafiResponce result = new BafiResponce(); try { using (var client = new HttpClient()) { //Passing service base url client.BaseAddress = new Uri(Baseurl); client.DefaultRequestHeaders.Clear(); client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); var stringContent = new StringContent(JsonConvert.SerializeObject(bafiRequest), Encoding.UTF8, "application/json"); //Sending request to find web api REST service resource GetDepartments using HttpClient HttpResponseMessage Res = await client.PostAsync("leads_get_token", stringContent); //Checking the response is successful or not which is sent using HttpClient if (Res.IsSuccessStatusCode) { var ObjResponse = Res.Content.ReadAsStringAsync().Result; result = JsonConvert.DeserializeObject<BafiResponce>(ObjResponse); } //returning the student list to view } } catch (Exception ex) { } return result; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
