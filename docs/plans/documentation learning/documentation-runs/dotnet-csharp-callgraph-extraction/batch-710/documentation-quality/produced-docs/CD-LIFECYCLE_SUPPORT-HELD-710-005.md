# Documentation: BoardController.SaveRequirements

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\BoardController.cs
- Symbol: BoardController.SaveRequirements
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); var currentUser = HttpContext.User; Users myuser = GameHelpers.GetUserByContext(currentUser, _context); if (myuser != null) { var req = _context.ActionToStatesDefs.Where(a => a.Id.ToString() == requirements.reqId).FirstOrDefault(); double minValue, cost, addition; Double.TryParse( requirements.minimalPoinst,out minValue); Double.TryParse(requirements.costPoinst, out cost); Double.TryParse(requirements.additionalPoinst, out addition); req.MinValue = minValue; req.Cost = cost; req.Addition = addition; _context.SaveChanges(); response = Ok(new { reqId = req.Id }); } return response; }

## Delegated Callee Behavior
- GameHelpers.GetUserByContext: Accepted callee meaning for GameHelpers.GetUserByContext: { var id = GetClaim("id", currentUser); Guid GId = Guid.NewGuid(); if (id != null) { GId = Guid.Parse(id); } string passwordHash = ""; Users users = _context.Users.Where(a => a.Id 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
