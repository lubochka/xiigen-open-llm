# Documentation: ProcessDataResults.ProcessAgentData

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ProcessLogic.cs
- Symbol: ProcessDataResults.ProcessAgentData
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ //TODO: Process atama //TODO: List<MemoryStream> attachments = new List<MemoryStream>(); //if(filesAttached.Count>0) //{ // foreach(var f in filesAttached) // { // attachments.Add(f); // } //} NewAgentData newAgentData = GetAgentsData(rows); newAgentData.fileId = job.objectId; newAgentData.agentId = Guid.NewGuid().ToString(); IndexRequest<object> prequest = new IndexRequest<object>(newAgentData, "agentData", newAgentData.agentId); _client.Index<object>(prequest); //ProcessAtamaResults processRes = GetAllProductsDate(rows, newRequest); //List<ProductData> products = processRes.products; //int i = 0; //foreach (var prod in products) //{ // var prodId = Guid.NewGuid().ToString(); // prod.prodId = prodId; // IndexRequest<object> prequest = new IndexRequest<object>(prod, "product", prodId); // _client.Index<object>(prequest); // if (i == 0) // { // var taskId = Guid.NewGuid().ToString(); //

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
