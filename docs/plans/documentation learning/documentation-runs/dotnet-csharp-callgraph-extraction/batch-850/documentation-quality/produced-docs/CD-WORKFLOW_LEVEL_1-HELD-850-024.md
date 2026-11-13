# Documentation: BoardController.SelectAction

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\BoardController.cs
- Symbol: BoardController.SelectAction
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ //get current game //check this is an active team //check who is an active user IActionResult response = Unauthorized(); var currentUser = HttpContext.User; Users myuser = GameHelpers.GetUserByContext(currentUser, _context); Games myGame = null; Teams myTeam = null; List<StateSummary> myStates; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { GameHelpers.ChooseAction(myuser, _context, gameId, teamId, rand, actionId, language); myStates = GameHelpers.CalculateStats(myGame, myTeam, _context); //check if finished if (myGame.PlayTeamToAnother == true) { Teams newActiveTeam; Users newActiveUser= GameHelpers.SwitchTeamsAndUser(myuser, _context, myTeam, myGame, out newActiveTeam ); if (newActiveUser.IsComputer == true) { List<Player> players = GameHelpers.MakeMove(newActiveUser, _context, gameId, newActiveTeam.Id, rand, language); Player currPlayer = pl

## Delegated Callee Behavior
- GameHelpers.GetUserByContext: Accepted callee meaning for GameHelpers.GetUserByContext: { var id = GetClaim("id", currentUser); Guid GId = Guid.NewGuid(); if (id != null) { GId = Guid.Parse(id); } string passwordHash = ""; Users users = _context.Users.Where(a => a.Id 
- GameHelpers.IsValidTeam: Accepted callee meaning for GameHelpers.IsValidTeam: { bool result = false; myTeam = null; myGame = _context.Games.Where(a => a.Id == gameId).FirstOrDefault(); if (myGame != null && myGame.ActiveTeam == teamId.ToString()) { myTeam = 
- GameHelpers.ChooseAction: Accepted callee meaning for GameHelpers.ChooseAction: { Games myGame = null; Teams myTeam = null; Teams newActiveTeam = null; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { List<Player> Intern
- GameHelpers.CalculateStats: Accepted callee meaning for GameHelpers.CalculateStats: { var result = new List<StateSummary>(); List<States> states = _context.States.Where(a=>true).ToList(); foreach(States state in states) { double? stateSummary = _context.TeamHistor
- GameHelpers.SwitchTeamsAndUser: Accepted callee meaning for GameHelpers.SwitchTeamsAndUser: { Teams newActiveTeam = null; List<Teams> teams = _context.Teams.Where(a => a.GameId == myGame.Id.ToString()).OrderBy(a => a.TeamNumber).ToList(); if (myTeam.TeamNumber >= teams.Co
- GameHelpers.MakeMove: Accepted callee meaning for GameHelpers.MakeMove: { List<Player> response = new List<Player>() ; if (myuser != null) { //get current game Games myGame = null; Teams myTeam = null; if(GameHelpers.IsValidTeam(myuser,_context,gameId,
- GameHelpers.BuildUpdatedGameState: Accepted callee meaning for GameHelpers.BuildUpdatedGameState: { List<Player> result = new List<Player>(); Games myGame = null; Teams myTeam = null; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { List<

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
