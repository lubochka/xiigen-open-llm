# Documentation: FilesController.Upload

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\FilesController.cs
- Symbol: FilesController.Upload
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); List<string> blobs = new List<string>(); foreach (var file in files) { byte[] bytes; using (MemoryStream ms = new MemoryStream()) { file.CopyTo(ms); bytes = ms.ToArray(); } var url = _blobStorageService.UploadFileToBlob(file.FileName,bytes,file.ContentType); blobs.Add(url); } FileProcessingData processingData = new FileProcessingData() { UserEmail = users.Email, UserName = users.UserName, RequestDate = DateTime.Now, Status = FileStatus.Requested, StatusUpdate = DateTime.Now, FileList = blobs }; _fileProcessingService.Create(processingData); return Ok(); }

## Delegated Callee Behavior
- LoginController.GetStatUserByClaim: Accepted callee meaning for LoginController.GetStatUserByClaim: { var id = GetClaim("id", currentUser); //Guid GId = Guid.NewGuid(); //if (id != null) //{ // GId = Guid.Parse(id); //} string passwordHash = ""; UserStore users = getStatUser("use
- BlobStorageService.UploadFileToBlob: Accepted callee meaning for BlobStorageService.UploadFileToBlob: { try { var _task = Task.Run(() => this.UploadFileToBlobAsync(strFileName, fileData, fileMimeType)); _task.Wait(); string fileUrl = _task.Result; return fileUrl; } catch (Exception
- IndexAttribute.Create: Accepted callee meaning for IndexAttribute.Create: { IndexRequest<object> request = new IndexRequest<object>(data, IndexName, data.Id); var res = Client.Index<object>(request); return data; }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
