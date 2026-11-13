# Documentation: ConvertController.ReadPdf

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\ConvertController.cs
- Symbol: ConvertController.ReadPdf
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ ActionResult response = Unauthorized(); var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null) { userMessage = "<br><b>פרטי סוכן שולח:</b>" + users.Fullname + "<br/><b>הערות סוכן:</b>"+"<br/>"+ userMessage+"<br/>"; if (file != null) { string parentId = Guid.NewGuid().ToString(); List<FileDetails> filesDetails = new List<FileDetails>(); Dictionary<string, List<Row>> dicfiles = new Dictionary<string, List<Row>>(); List<Stream> filesAttached = new List<Stream>(); Dictionary<string, string> acumulateResult = new Dictionary<string, string>(); foreach (var f in file) { if (f.FileName.EndsWith(".pdf")) { var url = _blobStorageService.UploadFileToBlob(f.FileName, getBytes(f.OpenReadStream()), "application/pdf"); FileDetails fileDetails = DealFile(f.FileName, url, parentId); if (fileDetails != null) { openFirstTask(file

## Delegated Callee Behavior
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use
- BlobStorageService.UploadFileToBlob: Accepted callee meaning for BlobStorageService.UploadFileToBlob: { try { var _task = Task.Run(() => this.UploadFileToBlobAsync(strFileName, fileData, fileMimeType)); _task.Wait(); string fileUrl = _task.Result; return fileUrl; } catch (Exception
- ConvertController.getBytes: Accepted callee meaning for ConvertController.getBytes: { byte[] bytes; using (MemoryStream ms = new MemoryStream()) { fstream.CopyTo(ms); bytes = ms.ToArray(); } return bytes; }
- ConvertController.DealFile: Accepted callee meaning for ConvertController.DealFile: { FileDetails fileDetails = new FileDetails(); fileDetails.filename = fileName; fileDetails.fileId = Guid.NewGuid().ToString(); fileDetails.status = "Uploaded"; fileDetails.parentI
- ConvertController.openFirstTask: Accepted callee meaning for ConvertController.openFirstTask: { var taskId = Guid.NewGuid().ToString(); TaskJob newTask = new TaskJob() { objectId = fileDetails.fileId, status = "Open", taskId = taskId, type = "ProcessPdf", page = 0, row=1, i

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## System Boundary Meaning
This system-level documentation describes the externally visible system surface only after preserving source evidence, runtime version, workflow meaning, and delegated callee behavior.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
