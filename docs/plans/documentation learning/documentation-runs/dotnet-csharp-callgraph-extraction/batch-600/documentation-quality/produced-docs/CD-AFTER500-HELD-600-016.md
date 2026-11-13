# Documentation: TournirsController.PostTournirs

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\TournirsController.cs
- Symbol: TournirsController.PostTournirs
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ _context.Tournirs.Add(tournirs); try { await _context.SaveChangesAsync(); } catch (DbUpdateException) { if (TournirsExists(tournirs.Id)) { return Conflict(); } else { throw; } } return CreatedAtAction("GetTournirs", new { id = tournirs.Id }, tournirs); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
