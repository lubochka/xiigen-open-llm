# Documentation: a1BreezeController.TasksByDates

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.TasksByDates
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IQueryable<Task> result; UserBack myUser; if (!IsAuth(out myUser)) return new List<Task>().AsQueryable(); try { DateTime start = startI.ToUniversalTime(); DateTime end = endI.ToUniversalTime(); if (end.Hour != 0) { start = new DateTime(startI.Year, startI.Month, startI.Day, 0, 0, 0); end = new DateTime(endI.Year, endI.Month, endI.Day, 23, 59, 0); } if (myUser.IsSystemManager==true) { result=(from job in _db.Context.Jobs.DefaultIfEmpty() join task in _db.Context.Tasks on job.ParentId equals task.Id where (job.start == null || (job.start <= end && job.end >= start)) && task.MainUserId == myUser.MainUserId && job.MainUserId == myUser.MainUserId select task ).Union(_db.Context.Tasks.Where(c => !_db.Context.Jobs.Select(b => b.ParentId).Contains(c.Id) && c.MainUserId==myUser.MainUserId)); } else if (myUser.IsClientManager==true) { string Client = myUser.Client; string[] Clients = Client.Spli

## Delegated Callee Behavior
- a1BreezeController.IsAuth: Accepted callee meaning for a1BreezeController.IsAuth: { myUser = GetUser(); //return true; if (myUser.Id > 0) return true; return false; }
- LoginDetails.IsAuth: Accepted callee meaning for LoginDetails.IsAuth: { UserBack myUser = new UserBack(); var identity = HttpContext.Current.User.Identity as ClaimsIdentity; if (identity != null) { myUser = new UserBack(identity); } try { if (myUser.

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
