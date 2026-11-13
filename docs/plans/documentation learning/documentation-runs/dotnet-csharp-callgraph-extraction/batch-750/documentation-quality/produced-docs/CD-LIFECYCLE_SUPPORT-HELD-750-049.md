# Documentation: BoardController.StartIndividualGame

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\BoardController.cs
- Symbol: BoardController.StartIndividualGame
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); var currentUser = HttpContext.User; Users myuser = GameHelpers.GetUserByContext(currentUser, _context); if (myuser != null) { Users computer = GameHelpers.GetComputer(_context); //open a new game Games newGame = GameHelpers.CreateNewGame(_context, myuser, language); Teams newTeam = GameHelpers.CreateNewIndividualTeam(myuser, _context, 1, newGame.Id, "Play"); Teams computerTeam = GameHelpers.CreateNewIndividualTeam(computer, _context, 2, newGame.Id,"waiting"); GameHelpers.AddInitialStates(_context, myuser, newTeam, newGame, language); GameHelpers.AddInitialStates(_context, computer, computerTeam, newGame, language); newGame.ActiveTeam = newTeam.Id.ToString(); _context.SaveChanges(); response = Ok(GameHelpers.BuildUpdatedGameState(myuser, _context, newGame.Id, newTeam.Id, language)); } return response; }

## Delegated Callee Behavior
- GameHelpers.GetUserByContext: Accepted callee meaning for GameHelpers.GetUserByContext: { var id = GetClaim("id", currentUser); Guid GId = Guid.NewGuid(); if (id != null) { GId = Guid.Parse(id); } string passwordHash = ""; Users users = _context.Users.Where(a => a.Id 
- GameHelpers.GetComputer: Accepted callee meaning for GameHelpers.GetComputer: { Users users = _context.Users.Where(a => a.IsComputer == true).FirstOrDefault(); return users; }
- GameHelpers.CreateNewGame: Accepted callee meaning for GameHelpers.CreateNewGame: { Games newGame = new Games(); newGame.Userid = myuser.Id.ToString(); newGame.OpensAt = DateTime.Now; newGame.PlayTeamToAnother = true; newGame.OpensMechanizm = "ByTime"; newGame.S
- GameHelpers.CreateNewIndividualTeam: Accepted callee meaning for GameHelpers.CreateNewIndividualTeam: { Teams newTeam = new Teams(); newTeam.Activeuser = myuser.Id.ToString(); newTeam.GameId = gameId.ToString(); newTeam.ActionNum = 1; newTeam.MaxPlayers = 1; newTeam.Userid = myuser
- GameHelpers.AddInitialStates: Accepted callee meaning for GameHelpers.AddInitialStates: { List<States> gameStates = _context.States.Where(a => a.Language == language).ToList(); foreach (States state in gameStates) { TeamHistoryMoves newMove = new TeamHistoryMoves(); n
- GameHelpers.BuildUpdatedGameState: Accepted callee meaning for GameHelpers.BuildUpdatedGameState: { List<Player> result = new List<Player>(); Games myGame = null; Teams myTeam = null; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { List<

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
