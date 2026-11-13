# Documentation: BAFIClientService.InsertNewLead

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Services\BAFIClientService.cs
- Symbol: BAFIClientService.InsertNewLead
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ try { using (var client = new HttpClient()) { client.BaseAddress = new Uri(baseURL); client.DefaultRequestHeaders.Clear(); client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")); var stringContent = new StringContent(JsonConvert.SerializeObject(insertLeadsRequest), Encoding.UTF8, "application/json"); HttpResponseMessage Res = await client.PostAsync("insert_leads", stringContent); if (Res.IsSuccessStatusCode) { var ObjResponse = Res.Content.ReadAsStringAsync().Result; return JsonConvert.DeserializeObject<BAFIInsertLeadsResponse>(ObjResponse); } } } catch (Exception ex) { ///throw exception } return null; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
