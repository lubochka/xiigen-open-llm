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

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
