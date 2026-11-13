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

## Delegated Callee Behavior
- ProcessDataResults.GetAgentsData: Accepted callee meaning for ProcessDataResults.GetAgentsData: { NewAgentData result = new NewAgentData(); int startRow = GetStartRow("הצעה ל", rows); if (startRow > -1) { foreach (CellElastic cell in rows[startRow].cells) { if (cell.word.Inde
- ProcessDataResults.GetAllProductsDate: Accepted callee meaning for ProcessDataResults.GetAllProductsDate: { ProcessAtamaResults finalRes = new ProcessAtamaResults(); List<ProductData> results = new List<ProductData>(); List<CellExport> nilveSum = new List<CellExport>(); int rowNum = 1;
- ProcessDataResults.BuildFollowing: Accepted callee meaning for ProcessDataResults.BuildFollowing: { List<List<CellExport>> results = new List<List<CellExport>>(); if (products.Count > 0) { products = products.OrderBy(a => a.company).ToList(); List<CellExport> rexp = new List<Ce
- CellExport.JsonToExcel: Accepted callee meaning for CellExport.JsonToExcel: { var workbook = new XLWorkbook(); IXLWorksheet worksheet = workbook.Worksheets.Add("Data"); foreach (var cell in cells) { worksheet.Cell(cell.row, cell.column).Value = cell.value;
- ConvertController.JsonToExcel: Accepted callee meaning for ConvertController.JsonToExcel: { var file = toExcelConverter.JsonToExcel(jsonObject); return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "data.xlsx"); }
- NotificationService.SendMail: Accepted callee meaning for NotificationService.SendMail: { using (MailMessage message = new MailMessage(from, to, caption, messageText)) { foreach (string ccm in cc) { message.CC.Add(ccm); } if (atachments != null) foreach (var dic in at
- NotificationController.SendPushForUser: Accepted callee meaning for NotificationController.SendPushForUser: { try { await notificationService.SendPushForUser(push.Id, push.Message); } catch (Exception ex) { return BadRequest(ex.Message); } return Ok(); }
- NotificationService.SendPushForUser: Accepted callee meaning for NotificationService.SendPushForUser: { return hub.Clients.User(userId).SendAsync("ReceiceMessage", message); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
