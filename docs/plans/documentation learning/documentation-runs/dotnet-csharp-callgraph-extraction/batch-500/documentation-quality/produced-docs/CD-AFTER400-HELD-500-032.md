# Documentation: NextStageJobScheduler.SetTitle

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Services\NextStageJobScheduler.cs
- Symbol: NextStageJobScheduler.SetTitle
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var task = _context.Tasks.FirstOrDefault(t => t.Id == newJob.ParentId); var member = _context.Members.Find(Convert.ToInt32(newJob.MemberId)); if (task == null || member == null) { return; } var client = _context.Clients.Find(Convert.ToInt32(task.Client)); var clientName = ""; if (client!=null) { clientName = client.Name + ","; } newJob.title = member.Name + ", "+ clientName + ", " + task.Title; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
