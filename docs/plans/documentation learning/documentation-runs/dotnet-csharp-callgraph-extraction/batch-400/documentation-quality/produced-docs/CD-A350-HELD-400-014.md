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

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
