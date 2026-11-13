# Documentation: ColorData.BuildColorsRows

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\PdfProcessor.cs
- Symbol: ColorData.BuildColorsRows
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ int middlewidth = imgbw.Width * 2 / 5; int middleWidth2 = imgbw.Width * 4 / 5; int lastHeigh = 0; int row = job.row; int cutWidth = int.Parse(Math.Round(imgbw.Width * 1.0, 0).ToString()); int lastCut = 0; int newHeight = 0; int jobLimitRow = job.row + 6; string status = "Open"; for (int i = job.i; i < imgbw.Height && (LimitRows == -1 || row<LimitRows) && row < jobLimitRow; i++) { job.i = i; job.row = row; if(job.page==0 && row>5) { var isAtama = false; foreach(var rowX in Rows) { foreach(var cellX in rowX.cells) { if(cellX.worInverted.IndexOf("מסמך התאמה") >-1) { isAtama = true; } } } if(!isAtama) { i = imgbw.Height - 1; status = "Done"; break; } } try { Color pixel = bwImageBorders.GetPixel(middlewidth, i); if (!isAbsBlack(pixel)) { if (isLine(bwImageBorders, i, middlewidth, 12) && isLine(bwImageBorders, i, middleWidth2, 12)) { int prevLine = checkPrevLine(middlewidth, middleWidth2, i

## Delegated Callee Behavior
- ColorData.isAbsBlack: Accepted callee meaning for ColorData.isAbsBlack: { return (pixel.B < 5 && pixel.G < 5 && pixel.R < 5); }
- ColorData.isLine: Accepted callee meaning for ColorData.isLine: { int AbsBlack = 0; for (var i = widthp - testX; i < widthp + testX && i < imgbw.Width; i++) { Color pixel = imgbw.GetPixel(i, heightp); if (!isWhite(pixel)) { AbsBlack++; } } retu
- ColorData.checkPrevLine: Accepted callee meaning for ColorData.checkPrevLine: { for (var j = Y - 1; j > 0; j--) { if (isLine(bwImageBorders, j, middlewidth, xtest) && isLine(bwImageBorders, j, middleWidth2, xtest)) return j; } return 0; }
- ColorData.CropRow: Accepted callee meaning for ColorData.CropRow: { Size size = new Size(width, height); Rectangle section = new Rectangle(new Point(X, Y), size); string fileName = imagePath.Replace(".png", "_") + Row + ".png"; Bitmap CroppedImag
- ColorData.isLineVert: Accepted callee meaning for ColorData.isLineVert: { int AbsBlack = 0; for (var i = heightp; i < heightp + testX && i < imgbw.Height; i++) { Color pixel = imgbw.GetPixel(widthp, i); if (!isWhite(pixel)) { AbsBlack++; } } return (te
- ColorData.checkPrevVertLine: Accepted callee meaning for ColorData.checkPrevVertLine: { for (var j = X - 1; j > 0; j--) { if (isLineVert(bwImageBorders, middleHeight, j, xtest)) return j; } return 0; }
- ColorData.CropCell: Accepted callee meaning for ColorData.CropCell: { Size size = new Size(width, height); Rectangle section = new Rectangle(new Point(X, Y), size); string fileName = imagePath.Replace(".png", "_") + Row + "_cell_" + Cell + ".png"; 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
