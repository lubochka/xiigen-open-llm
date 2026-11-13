# Documentation: LogProgress.ProcessData

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\JobProcess.cs
- Symbol: LogProgress.ProcessData
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ FileDetails file = new FileDetails(); SearchDescriptor<object> searchDescriptorf = new SearchDescriptor<object>("filedetails").Query(q => q.Match(a => a.Field("fileId").Query(guid))); ISearchResponse<object> responsef = _client.Search<object>(searchDescriptorf); if (responsef.Hits.Count > 0) { file = responsef.Hits.Select(x => new FileDetails(x.Source)).FirstOrDefault(); } SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>("rowelastic").Query(q => q.Match(a => a.Field("fileId").Query(guid))).Size(150); ISearchResponse<object> response = _client.Search<object>(searchDescriptor); if (response.Hits.Count > 0) { List<RowElastic> rows = response.Hits.Select(x => new RowElastic(x.Source)).ToList(); ProcessDataResults procress = logic.ProcessAtama(rows, _client, job); List<MemoryStream> attachments = procress.attachments; if (procress.specialStatus == "" && attachments.C

## Delegated Callee Behavior
- ProcessDataResults.ProcessAtama: Accepted callee meaning for ProcessDataResults.ProcessAtama: { //TODO: Process atama //TODO: ProcessDataResults procres = new ProcessDataResults(); procres.specialStatus = ""; procres.attachments = new List<MemoryStream>(); //if(filesAttache
- BlobStorageService.UploadFileToBlob: Accepted callee meaning for BlobStorageService.UploadFileToBlob: { try { var _task = Task.Run(() => this.UploadFileToBlobAsync(strFileName, fileData, fileMimeType)); _task.Wait(); string fileUrl = _task.Result; return fileUrl; } catch (Exception

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
