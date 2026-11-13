# Documentation: BlobStorageService.UploadFileToBlob

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Services\BlobStorageService.cs
- Symbol: BlobStorageService.UploadFileToBlob
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (var memoryStream = new MemoryStream()) { file.CopyTo(memoryStream); var content= memoryStream.ToArray(); return UploadFileToBlob(strFileName, content, fileMimeType); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
