# Documentation: BoardController.GetWizardStages

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\BoardController.cs
- Symbol: BoardController.GetWizardStages
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ Actions action = _context.Actions.Where(a => a.Id.ToString() == parent_id).FirstOrDefault(); List<Actions> actions = _context.Actions.Where(a => a.ParentId == action.ParentId && a.OrderNum >= action.OrderNum).ToList(); List<WizardStages> stages = new List<WizardStages>(); foreach (Actions actionI in actions) { var newStages = _context.WizardStages.Where(a => a.ParentId == actionI.Id.ToString() && a.Language == language && a.Step>= prev_step).OrderBy(a => a.Step).ThenBy(a => a.Priority).ToList(); stages.AddRange(newStages); } return stages; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
