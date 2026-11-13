# Documentation: ColorData.getRows

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.getRows
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<RowElastic> results = new List<RowElastic>(); foreach(Row rowFrom in rows) { results.Add(new RowElastic(rowFrom, fileId)); } return results; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
