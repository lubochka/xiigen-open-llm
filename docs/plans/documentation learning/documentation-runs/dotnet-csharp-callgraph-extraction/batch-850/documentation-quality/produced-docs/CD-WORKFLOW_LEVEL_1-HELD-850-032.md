# Documentation: BoardController.GetActions

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\BoardController.cs
- Symbol: BoardController.GetActions
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ List<ActionWithStates> actionWithStates = new List<ActionWithStates>(); List<StateSummary> EmptyStates = GameHelpers.CreateEmptyStates(_context); List<Actions> actions = _context.Actions.Where(a => a.ParentId == parent_id && a.Language == language).OrderBy(a=>a.OrderNum).ToList(); var currentUser = HttpContext.User; Users myuser = GameHelpers.GetUserByContext(currentUser, _context); foreach (Actions action in actions) { ActionWithStates actionExt = new ActionWithStates(); actionExt.status = "Ok"; actionExt.action = action; actionExt.statesRequirements = new List<StateRequirements>(); actionExt.errors = new List<string>(); foreach (StateSummary state in EmptyStates) { List<ActionToStatesDefs> statesToAction = _context.ActionToStatesDefs.Where(a => a.ActionId == action.Id.ToString() && a.StateId == state.Id.ToString()).ToList(); foreach (ActionToStatesDefs actReq in statesToAction) { Sta

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
