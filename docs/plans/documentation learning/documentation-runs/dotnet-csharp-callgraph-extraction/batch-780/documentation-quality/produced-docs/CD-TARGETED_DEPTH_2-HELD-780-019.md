# Documentation: ColorData.isLine

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.isLine
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ int AbsBlack = 0; for (var i = widthp - testX; i < widthp + testX && i < imgbw.Width; i++) { Color pixel = imgbw.GetPixel(i, heightp); if (!isWhite(pixel)) { AbsBlack++; } } return (testX * 2 * 0.05) < (AbsBlack * 1.0) ? false : true; }

## Delegated Callee Behavior
- ColorData.isWhite: Accepted callee meaning for ColorData.isWhite: { return (pixel.B > 200 && pixel.G > 200 && pixel.R > 200); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
