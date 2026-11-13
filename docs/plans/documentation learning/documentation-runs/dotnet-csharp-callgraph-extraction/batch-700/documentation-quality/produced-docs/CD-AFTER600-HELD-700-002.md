# Documentation: GameHelpers.AddInitialStates

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.AddInitialStates
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<States> gameStates = _context.States.Where(a => a.Language == language).ToList(); foreach (States state in gameStates) { TeamHistoryMoves newMove = new TeamHistoryMoves(); newMove.Explanation = "Start Game"; newMove.ActionNum = 1; newMove.ActionNum = team.ActionNum; newMove.StateId = state.Id.ToString(); newMove.Points = 100; newMove.Teamid = team.Id.ToString(); newMove.Userid = myuser.Id.ToString(); newMove.GameId = game.Id.ToString(); _context.TeamHistoryMoves.Add(newMove); _context.SaveChanges(); } team.ActionNum++; _context.SaveChanges(); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
