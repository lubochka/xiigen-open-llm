# Documentation: a1BreezeController.AfterSaveEntities

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.AfterSaveEntities
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return; List<EntityInfo> entitiesInfo = new List<EntityInfo>(); if (saveMap.TryGetValue(typeof(Task), out entitiesInfo)) { if (myUser.ManageHimself != null && myUser.ManageHimself == true) { foreach (EntityInfo entity in entitiesInfo) { Task task = entity.Entity as Task; var Client = _db.Context.Clients.FirstOrDefault(a => a.Id.ToString() == task.Client); Job job = new Job(); job.MemberId = myUser.Id.ToString(); var clientName = ""; if (Client!=null) { clientName = Client.Name + ","; } job.title = myUser.Name + ","+ clientName + task.Title; _db.Context.Jobs.Add(job); _db.Context.SaveChanges(); } } } List<EntityInfo> savedJobEntities; if (saveMap.TryGetValue(typeof(Job), out savedJobEntities)) { foreach (var savedJobEntity in savedJobEntities) { var savedJob = savedJobEntity.Entity as Job; if (savedJob == null) { continue; } bool? oldFinishedVal

## Delegated Callee Behavior
- a1BreezeController.IsAuth: Accepted callee meaning for a1BreezeController.IsAuth: { myUser = GetUser(); //return true; if (myUser.Id > 0) return true; return false; }
- LoginDetails.IsAuth: Accepted callee meaning for LoginDetails.IsAuth: { UserBack myUser = new UserBack(); var identity = HttpContext.Current.User.Identity as ClaimsIdentity; if (identity != null) { myUser = new UserBack(identity); } try { if (myUser.
- a1BreezeController.SaveChanges: Accepted callee meaning for a1BreezeController.SaveChanges: { if (!IsAuth()) return null; return _db.SaveChanges(saveBundle); }
- BoolExtensions.FullValue: Accepted callee meaning for BoolExtensions.FullValue: { return input.HasValue && input.Value; }
- NextStageJobScheduler.SheduleNext: Accepted callee meaning for NextStageJobScheduler.SheduleNext: { var newJob = new Job(); newJob.LoopId = finishedJob.LoopId; newJob.MainUserId = finishedJob.MainUserId; newJob.ParentId = finishedJob.ParentId; var finishedStage = _context.Stage

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
