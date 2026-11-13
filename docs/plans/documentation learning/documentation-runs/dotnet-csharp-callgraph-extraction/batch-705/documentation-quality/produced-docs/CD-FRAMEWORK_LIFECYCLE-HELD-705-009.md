# Documentation: ColorData.FindMiddle

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.FindMiddle
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ for (var i = Start.X + 1; i < img.Width; i++) { for (var j = Start.Y; j < img.Height; j++) { Color middle = img.GetPixel(i, j); var deltam = calculateDelta(middle, start); if (!isBlack(middle)) { return new ColorData() { i = i, j = j, pixel = middle }; } } } return null; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
