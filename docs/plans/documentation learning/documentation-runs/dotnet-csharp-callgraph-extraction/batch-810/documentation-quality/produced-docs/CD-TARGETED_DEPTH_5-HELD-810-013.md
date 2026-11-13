# Documentation: TournirsController.PutTournirs

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\TournirsController.cs
- Symbol: TournirsController.PutTournirs
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (id != tournirs.Id) { return BadRequest(); } _context.Entry(tournirs).State = EntityState.Modified; try { await _context.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { if (!TournirsExists(id)) { return NotFound(); } else { throw; } } return NoContent(); }

## Delegated Callee Behavior
- TournirsController.TournirsExists: Accepted callee meaning for TournirsController.TournirsExists: { return _context.Tournirs.Any(e => e.Id == id); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
