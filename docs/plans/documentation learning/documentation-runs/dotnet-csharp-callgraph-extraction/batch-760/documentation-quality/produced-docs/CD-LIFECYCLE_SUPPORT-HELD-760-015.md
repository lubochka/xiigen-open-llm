# Documentation: BafiAuthorizeRequest.OpenLead

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\BafiLead.cs
- Symbol: BafiAuthorizeRequest.OpenLead
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ // configuration.GetSection("SMTPConfig").GetSection("Host").Value BafiAuthorizeRequest loginRequest = new BafiAuthorizeRequest(); loginRequest.password = configuration.GetSection("BafiParams").GetSection("password").Value; loginRequest.sessionid = configuration.GetSection("BafiParams").GetSection("sessionid").Value; loginRequest.username = configuration.GetSection("BafiParams").GetSection("username").Value; BafiResponce loginResp = await GetAuthorized(loginRequest); if (loginResp.success) { BafiLeadrRequest leadReq = new BafiLeadrRequest(); leadReq.token = loginResp.token; leadReq.sessionId = configuration.GetSection("BafiParams").GetSection("sessionid").Value; leadReq.user_lead_id = loginResp.user_lead_id; leadReq.source = 2; if(user.LeadId!=null && user.LeadId.Trim()!="") { try { leadReq.source = int.Parse(user.LeadId); } catch(Exception leadid) { } } leadReq.user_id = int.Parse(con

## Delegated Callee Behavior
- BafiAuthorizeRequest.GetAuthorized: Accepted callee meaning for BafiAuthorizeRequest.GetAuthorized: { BafiResponce result = new BafiResponce(); try { using (var client = new HttpClient()) { //Passing service base url client.BaseAddress = new Uri(Baseurl); client.DefaultRequestHea

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
