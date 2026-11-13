# Documentation: GameHelpers.UpdateNewPlacement

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.UpdateNewPlacement
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ activePlacement = 0; firstNum = rand.Next(1, 6); secondNum = rand.Next(1, 6); if (placement.ActivePlacement != null) { int.TryParse(placement.ActivePlacement.ToString(), out activePlacement); } if (placement.Status != "Choose Action") { activePlacement += firstNum + secondNum; if (gameCards.Count <= activePlacement) { GameHelpers.CloseRound(myGame, myTeam, myuser, _context); activePlacement = activePlacement - gameCards.Count; //test if the game limit has reached! } placement.FirstNum = firstNum; placement.LastNum = secondNum; placement.ActivePlacement = activePlacement; placement.Status = "Choose Action"; _context.SaveChanges(); } }

## Delegated Callee Behavior
- GameHelpers.CloseRound: Accepted callee meaning for GameHelpers.CloseRound: { List<StatesToActions> StatesToActions= _context.StatesToActions.Where(a => a.GameId == game.Id.ToString() && a.TeamId == team.Id.ToString() && a.Regularity == "Roundly").ToList()

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
