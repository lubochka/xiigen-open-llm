# Documentation: ColorData.checkPrevLine

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.checkPrevLine
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ for (var j = Y - 1; j > 0; j--) { if (isLine(bwImageBorders, j, middlewidth, xtest) && isLine(bwImageBorders, j, middleWidth2, xtest)) return j; } return 0; }

## Delegated Callee Behavior
- ColorData.isLine: Accepted callee meaning for ColorData.isLine: { int AbsBlack = 0; for (var i = widthp - testX; i < widthp + testX && i < imgbw.Width; i++) { Color pixel = imgbw.GetPixel(i, heightp); if (!isWhite(pixel)) { AbsBlack++; } } retu

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
