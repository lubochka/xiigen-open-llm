# Documentation: LogProgress.SendEmailError

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\JobProcess.cs
- Symbol: LogProgress.SendEmailError
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ SearchDescriptor<object> searchDescriptorf = new SearchDescriptor<object>("filedetails").Query(q => q.Match(a => a.Field("parentId").Query(guid))); ISearchResponse<object> responsef = _client.Search<object>(searchDescriptorf); if (responsef.Hits.Count > 0) { List<FileDetails> files = responsef.Hits.Select(x => new FileDetails(x.Source)).ToList(); List<Dictionary<string, MemoryStream>> attachments = new List<Dictionary<string, MemoryStream>>(); var dic = new Dictionary<string, MemoryStream>(); foreach (FileDetails file in files) { MemoryStream filestr = _blobStorageService.GetBlobData(file.blobId); if (filestr != null) { dic[file.filename] = filestr; } } if (dic.Count > 0) { attachments.Add(dic); } // notificationService.SendMail("hafaka@kedem-ins.co.il", "hafaka@kedem-ins.co.il", "פרטי ביטוח חדשים", "קבצים מוכנים לשליחה", attachments); try { List<string> cc = new List<string>(); cc.Add

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
