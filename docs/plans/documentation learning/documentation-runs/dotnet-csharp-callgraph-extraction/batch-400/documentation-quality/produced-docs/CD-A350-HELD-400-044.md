# Documentation: ColorData.CropCell

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.CropCell
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ Size size = new Size(width, height); Rectangle section = new Rectangle(new Point(X, Y), size); string fileName = imagePath.Replace(".png", "_") + Row + "_cell_" + Cell + ".png"; Bitmap CroppedImage = CropImage(source, section, fileName, save); if (save) { CroppedImage.Save(fileName, ImageFormat.Png); } return CroppedImage; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
