# Documentation: GameHelpers.GetUserByContext

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.GetUserByContext
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var id = GetClaim("id", currentUser); Guid GId = Guid.NewGuid(); if (id != null) { GId = Guid.Parse(id); } string passwordHash = ""; Users users = _context.Users.Where(a => a.Id == GId).FirstOrDefault(); return users; }

## Delegated Callee Behavior
- GameHelpers.GetClaim: Accepted callee meaning for GameHelpers.GetClaim: { if (currentUser.HasClaim(c => c.Type == claim)) { return currentUser.Claims.FirstOrDefault(c => c.Type == claim).Value; } return null; }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
