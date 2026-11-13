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

## Delegated Callee Behavior
- a1BreezeController.IsAuth: Accepted callee meaning for a1BreezeController.IsAuth: { myUser = GetUser(); //return true; if (myUser.Id > 0) return true; return false; }
- LoginDetails.IsAuth: Accepted callee meaning for LoginDetails.IsAuth: { UserBack myUser = new UserBack(); var identity = HttpContext.Current.User.Identity as ClaimsIdentity; if (identity != null) { myUser = new UserBack(identity); } try { if (myUser.

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
