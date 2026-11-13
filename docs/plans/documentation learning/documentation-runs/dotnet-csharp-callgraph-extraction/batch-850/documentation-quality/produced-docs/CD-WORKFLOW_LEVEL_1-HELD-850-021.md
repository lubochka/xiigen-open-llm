# Documentation: JobProcessController.ProcessTasks

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\JobProcessController.cs
- Symbol: JobProcessController.ProcessTasks
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var adminC = _configuration.GetSection("JobProcess").GetSection("admin").Value; var passwordC = _configuration.GetSection("JobProcess").GetSection("password").Value; List<string> results = new List<string>(); if (admin == adminC && passwordC == password) { SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>("taskjob").Query(q => q.Match(a => a.Field("status").Query("Open"))); searchDescriptor.Sort(so => so.Field("firstTaskDate", SortOrder.Ascending)); ISearchResponse<object> response = _client.Search<object>(searchDescriptor); SearchDescriptor<object> searchDescriptor2 = new SearchDescriptor<object>("taskjob").Query(q => q.Match(a => a.Field("status").Query("Processing")) && q.Match(a => a.Field("type").Query("ProcessPdf")) && q.Match(a => a.Field("server").Query("server1"))); ISearchResponse<object> response2 = _client.Search<object>(searchDescriptor2); SearchDesc

## Delegated Callee Behavior
- LogProgress.ProcessPDF: Accepted callee meaning for LogProgress.ProcessPDF: { ProcessPdfResults pdfresult = new ProcessPdfResults(); PdfExtractor.Result res = pdfConverter.ReadPdfFile(fstream, job.page, job); pdfresult.job = res.job; if(res.filePath!="") {
- LogProgress.ProcessPdf: Accepted callee meaning for LogProgress.ProcessPdf: { ProcessPdfResults pdfresult = new ProcessPdfResults(); PdfExtractor.Result res = pdfConverter.ReadPdfFile(fstream, job.page, job); pdfresult.job = res.job; if(res.filePath!="") {
- LogProgress.ProcessData: Accepted callee meaning for LogProgress.ProcessData: { FileDetails file = new FileDetails(); SearchDescriptor<object> searchDescriptorf = new SearchDescriptor<object>("filedetails").Query(q => q.Match(a => a.Field("fileId").Query(gui
- LogProgress.ProcessAgentData: Accepted callee meaning for LogProgress.ProcessAgentData: { FileDetails file = new FileDetails(); SearchDescriptor<object> searchDescriptorf = new SearchDescriptor<object>("filedetails").Query(q => q.Match(a => a.Field("fileId").Query(gui
- ProcessDataResults.ProcessAgentData: Accepted callee meaning for ProcessDataResults.ProcessAgentData: { //TODO: Process atama //TODO: List<MemoryStream> attachments = new List<MemoryStream>(); //if(filesAttached.Count>0) //{ // foreach(var f in filesAttached) // { // attachments.Ad
- LogProgress.DeleteFile: Accepted callee meaning for LogProgress.DeleteFile: { return pdfConverter.DeleteFile(filepath); }
- PdfConverter.DeleteFile: Accepted callee meaning for PdfConverter.DeleteFile: { return PdfProcessor.DeleteFile(filePath); }
- ColorData.DeleteFile: Accepted callee meaning for ColorData.DeleteFile: { try { if (File.Exists(imageJpgPath)) { File.Delete(imageJpgPath); return "Done"; } else { return "File not exists"; } } catch (Exception ex) { return "File Busy"; //TODO: remove 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
