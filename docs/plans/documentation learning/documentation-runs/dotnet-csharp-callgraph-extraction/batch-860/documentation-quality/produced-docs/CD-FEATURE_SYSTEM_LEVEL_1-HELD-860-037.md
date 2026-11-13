# Documentation: a1BreezeController.JobsByDates

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.JobsByDates
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Job>().AsQueryable(); try { DateTime start = startI.ToUniversalTime(); DateTime end = endI.ToUniversalTime(); if (end.Hour != 0) { start = new DateTime(startI.Year, startI.Month, startI.Day, 0, 0, 0); end = new DateTime(endI.Year, endI.Month, endI.Day, 23, 59, 0); } if (myUser.IsSystemManager==true) { return (from job in _db.Context.Jobs.DefaultIfEmpty() join task in _db.Context.Tasks on job.ParentId equals task.Id where (job.start == null || (job.start <= end && job.end >= start)) && job.MainUserId==myUser.MainUserId && task.MainUserId==myUser.MainUserId select job ); } if (myUser.IsClientManager==true) { string Client = myUser.Client; string[] Clients = Client.Split(','); List<Task> mytasks = _db.Context.Tasks.Where(e => Clients.Contains(e.Client)).ToList(); return (from job in _db.Context.Jobs join task in _db.Context.Tasks o

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
