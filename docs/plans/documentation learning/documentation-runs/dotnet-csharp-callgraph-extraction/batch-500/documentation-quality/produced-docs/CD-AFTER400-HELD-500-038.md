# Documentation: ColorData.isLineVert

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.isLineVert
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ int AbsBlack = 0; for (var i = heightp; i < heightp + testX && i < imgbw.Height; i++) { Color pixel = imgbw.GetPixel(widthp, i); if (!isWhite(pixel)) { AbsBlack++; } } return (testX * 0.05) < (AbsBlack * 1.0) ? false : true; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
