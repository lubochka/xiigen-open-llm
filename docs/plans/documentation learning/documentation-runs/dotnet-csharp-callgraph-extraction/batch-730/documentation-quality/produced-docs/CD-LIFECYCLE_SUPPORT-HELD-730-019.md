# Documentation: GameHelpers.ChooseAction

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.ChooseAction
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ Games myGame = null; Teams myTeam = null; Teams newActiveTeam = null; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { List<Player> Internalres = GameHelpers.BuildUpdatedGameState(myuser, _context, gameId, teamId, language); Player player = Internalres.Where(a => a.UserId == myuser.Userid).FirstOrDefault(); ActionWithStates actionWithStates = player.actionWithStates.Where(a => a.action.Id == actionId).FirstOrDefault(); if (actionWithStates != null && actionWithStates.status == "Ok") { foreach (StateRequirements stateRequirement in actionWithStates.statesRequirements) { myTeam.ActionNum++; StatesToActions newStateToAction = new StatesToActions(); newStateToAction.ActionId = actionWithStates.action.Id.ToString(); newStateToAction.CardId = actionWithStates.action.ParentId; newStateToAction.TeamId = teamId.ToString(); newStateToAction.Regularity = st

## Delegated Callee Behavior
- GameHelpers.IsValidTeam: Accepted callee meaning for GameHelpers.IsValidTeam: { bool result = false; myTeam = null; myGame = _context.Games.Where(a => a.Id == gameId).FirstOrDefault(); if (myGame != null && myGame.ActiveTeam == teamId.ToString()) { myTeam = 
- GameHelpers.BuildUpdatedGameState: Accepted callee meaning for GameHelpers.BuildUpdatedGameState: { List<Player> result = new List<Player>(); Games myGame = null; Teams myTeam = null; if (GameHelpers.IsValidTeam(myuser, _context, gameId, teamId, out myGame, out myTeam)) { List<

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
