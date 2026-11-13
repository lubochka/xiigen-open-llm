# Documentation: GameHelpers.MakeMove

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.MakeMove
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<Player> response = new List<Player>() ; if (myuser != null) { //get current game Games myGame = null; Teams myTeam = null; if(GameHelpers.IsValidTeam(myuser,_context,gameId,teamId,out myGame,out myTeam)) { TeamToUsers placement = _context.TeamToUsers.Where(a => a.TeamId == myTeam.Id.ToString() && a.UserId == myuser.Id.ToString()).FirstOrDefault(); if (placement == null) { placement = GameHelpers.NewPlacement(myuser, myTeam, _context); } List<Cards> gameCards = _context.Cards.Where(a => a.Language == language).OrderBy(a => a.OrderNum).ToList(); int firstNum, secondNum, activePlacement; GameHelpers.UpdateNewPlacement(placement, gameCards, myGame, myTeam, myuser, _context, rand, out firstNum, out secondNum, out activePlacement); response = GameHelpers.BuildUpdatedGameState(myuser, _context, gameId, teamId, language); } } return response; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
