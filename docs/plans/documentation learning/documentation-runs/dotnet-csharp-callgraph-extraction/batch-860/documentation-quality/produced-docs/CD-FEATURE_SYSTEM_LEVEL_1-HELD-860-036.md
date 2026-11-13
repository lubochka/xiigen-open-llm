# Documentation: a1BreezeController.Jobs

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.Jobs
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Job>().AsQueryable(); try { if (myUser.IsSystemManager==true) { return _db.Context.Jobs.Where(e => e.MainUserId == myUser.MainUserId); } if (myUser.IsFilialManager==true) { string Filial = myUser.Filial; var entry = (from job in _db.Context.Jobs join task in _db.Context.Tasks on job.ParentId equals task.Id where task.Filial == Filial && task.MainUserId==myUser.MainUserId && job.MainUserId==myUser.MainUserId select job).ToList().AsQueryable(); ; return entry; } if (myUser.IsClientManager==true) { string Client = myUser.Client; string[] Clients = Client.Split(','); List<Task> mytasks = _db.Context.Tasks.Where(e => Clients.Contains(e.Client) && e.MainUserId == myUser.MainUserId).ToList(); var entry = (from job in _db.Context.Jobs join task in _db.Context.Tasks on job.ParentId equals task.Id where Clients.Contains(task.Client) && ta

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
