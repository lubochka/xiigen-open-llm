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

## Delegated Callee Behavior
- ColorData.calculateDelta: Accepted callee meaning for ColorData.calculateDelta: { var delta = Math.Sqrt(Math.Pow(origin.R - compare.R, 2) + Math.Pow(origin.B - compare.B, 2) + Math.Pow(origin.G - compare.G, 2)); return delta; }
- ColorData.isBlack: Accepted callee meaning for ColorData.isBlack: { return (pixel.B < 130 && pixel.G < 90 && pixel.R < 50); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
