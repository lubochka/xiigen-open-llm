# Documentation: GameHelpers.CreateStateRequirement

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\GameHelpers.cs
- Symbol: GameHelpers.CreateStateRequirement
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ StateRequirements myactionRequirements = new StateRequirements(); myactionRequirements.reqId = actReq.Id; myactionRequirements.id = state.Id; myactionRequirements.name = state.name; myactionRequirements.minimalPoinst = actReq.MinValue; myactionRequirements.currentStatePoints = state.points; myactionRequirements.pointsName = state.pointsName; if (state.points < myactionRequirements.minimalPoinst) { actionExt.status = "Error"; actionExt.enabled = false; myactionRequirements.status = "Not enough " + state.pointsName + " to minimal requirements"; actionExt.problems +=""+ myactionRequirements.status+""; actionExt.errors.Add(state.name + ":" + myactionRequirements.status); } myactionRequirements.costPoinst = actReq.Cost; if (state.points < myactionRequirements.costPoinst) { actionExt.status = "Error"; myactionRequirements.status = "Not enough " + state.pointsName + " on balance"; actionExt.p

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
