# Documentation: PdfConverter.ReadPdfFile

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfConverter.cs
- Symbol: PdfConverter.ReadPdfFile
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var res = new PdfExtractor.Result(); try { var result = ""; var Result = PdfProcessor.ExtractData(fileStream, page, job); return Result; } catch(Exception ex) { res.Rows = new List<Row>(); var exeptionRow =new Row(); exeptionRow.rowDic.Add("exception", ex.Message + " trace:" + ex.StackTrace); res.Rows.Add(exeptionRow); //return ex.Message +" trace:" + ex.StackTrace; } return res; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
