# Documentation: GameHelpers.BuildUpdatedGameState

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.BuildUpdatedGameState
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<Player> result = new List<Player>(); Games myGame = null; Teams myTeam = null; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { List<Teams> teams = _context.Teams.Where(a => a.GameId == gameId.ToString()).ToList(); foreach(Teams team in teams) { List<TeamToUsers> t2users = _context.TeamToUsers.Where(a => a.TeamId == team.Id.ToString()).ToList(); foreach(TeamToUsers t2user in t2users) { Users user = _context.Users.Where(a => a.Id.ToString() == t2user.UserId).FirstOrDefault(); Player player = new Player(); player.Name = user.Fullname; player.UserId = user.Userid; player.icon = t2user.Icon; player.color = t2user.Color; player.IsWinner = false; player.GameId = gameId.ToString(); player.TeamId = team.Id.ToString(); if (myGame.Status == "Done" && myGame.ActiveTeam == team.Id.ToString()) { player.IsWinner = true; } player.Status = "waiting"; if(myG

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
