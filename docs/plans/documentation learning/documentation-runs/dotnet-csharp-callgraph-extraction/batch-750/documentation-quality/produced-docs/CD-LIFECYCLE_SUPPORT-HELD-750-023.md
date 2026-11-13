# Documentation: LogProgress.SendEmail

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\JobProcess.cs
- Symbol: LogProgress.SendEmail
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ SearchDescriptor<object> searchDescriptorf = new SearchDescriptor<object>("filedetails").Query(q => q.Match(a => a.Field("parentId").Query(guid))).Size(20); ISearchResponse<object> responsef = _client.Search<object>(searchDescriptorf); if (responsef.Hits.Count > 0) { List<FileDetails> files = responsef.Hits.Select(x => new FileDetails(x.Source)).ToList(); List<Dictionary<string,MemoryStream>> attachments = new List<Dictionary<string, MemoryStream>>(); var dic =new Dictionary<string, MemoryStream>(); foreach (FileDetails file in files) { MemoryStream filestr= _blobStorageService.GetBlobData(file.blobId); if (filestr != null) { dic[file.filename] = filestr; } else { var str = "warning"; } } if(dic.Count>0) { attachments.Add(dic); } // notificationService.SendMail("hafaka@kedem-ins.co.il", "hafaka@kedem-ins.co.il", "פרטי ביטוח חדשים", "קבצים מוכנים לשליחה", attachments); try { List<string

## Delegated Callee Behavior
- BlobStorageService.GetBlobData: Accepted callee meaning for BlobStorageService.GetBlobData: { try { var _task = Task.Run(() => this.GetBlobDataAsync(fileUrl)); _task.Wait(); MemoryStream res = _task.Result; return res; } catch (Exception ex) { throw (ex); } }
- NotificationService.SendMail: Accepted callee meaning for NotificationService.SendMail: { using (MailMessage message = new MailMessage(from, to, caption, messageText)) { foreach (string ccm in cc) { message.CC.Add(ccm); } if (atachments != null) foreach (var dic in at

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
