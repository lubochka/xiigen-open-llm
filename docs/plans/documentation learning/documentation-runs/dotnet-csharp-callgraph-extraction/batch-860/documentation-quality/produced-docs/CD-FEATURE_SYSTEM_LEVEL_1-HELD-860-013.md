# Documentation: a1BreezeController.UploadDocument

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.UploadDocument
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (!IsAuth()) throw new HttpResponseException(HttpStatusCode.Unauthorized); HttpResponseMessage result = null; var httpRequest = HttpContext.Current.Request; if (httpRequest.Files.Count > 0) { var postedFile = httpRequest.Files[0]; byte[] fileData = null; using (var binaryReader = new BinaryReader(postedFile.InputStream)) { fileData = binaryReader.ReadBytes(postedFile.ContentLength); } var uniquFolder = Guid.NewGuid().ToString(); string filename = postedFile.FileName.Substring(postedFile.FileName.LastIndexOf("\\", StringComparison.CurrentCulture) + 1).Replace(" ", "_"); //save to blob AzureBlobHelper.WriteFileToBlob("documents", $"{uniquFolder}/{filename}", fileData); var taskId = Convert.ToInt32(httpRequest.Params["TaskId"]); Tasks().First(e => e.Id == taskId); //security, throw unwanted ids (not mine) //save to DB _db.Context.Attachments.Add(new Attachment { ParentId = taskId, FileN

## Delegated Callee Behavior
- a1BreezeController.IsAuth: Accepted callee meaning for a1BreezeController.IsAuth: { myUser = GetUser(); //return true; if (myUser.Id > 0) return true; return false; }
- LoginDetails.IsAuth: Accepted callee meaning for LoginDetails.IsAuth: { UserBack myUser = new UserBack(); var identity = HttpContext.Current.User.Identity as ClaimsIdentity; if (identity != null) { myUser = new UserBack(identity); } try { if (myUser.
- AzureBlobHelper.WriteFileToBlob: Accepted callee meaning for AzureBlobHelper.WriteFileToBlob: { CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=lshusersdata;AccountKey=REDACTED_SECRET_REMOVED
- a1BreezeController.Tasks: Accepted callee meaning for a1BreezeController.Tasks: { UserBack myUser; if (!IsAuth(out myUser)) return new List<Task>().AsQueryable(); try { if (myUser.IsSystemManager==true) { return _db.Context.Tasks.Where(e => e.MainUserId == myU
- a1BreezeController.SaveChanges: Accepted callee meaning for a1BreezeController.SaveChanges: { if (!IsAuth()) return null; return _db.SaveChanges(saveBundle); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## System Boundary Meaning
This system-level documentation describes the externally visible system surface only after preserving source evidence, runtime version, workflow meaning, and delegated callee behavior.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
