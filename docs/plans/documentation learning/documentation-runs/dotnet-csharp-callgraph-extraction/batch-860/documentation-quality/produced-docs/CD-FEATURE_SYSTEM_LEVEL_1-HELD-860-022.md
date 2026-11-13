# Documentation: NextStageJobScheduler.SheduleNext

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Services\NextStageJobScheduler.cs
- Symbol: NextStageJobScheduler.SheduleNext
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var newJob = new Job(); newJob.LoopId = finishedJob.LoopId; newJob.MainUserId = finishedJob.MainUserId; newJob.ParentId = finishedJob.ParentId; var finishedStage = _context.Stages.FirstOrDefault(s => s.Id == finishedJob.StageId); if (finishedStage == null) { return null; } var nextStage = _context.Stages .Where(s => s.LoopId == finishedStage.LoopId) .Where(s => s.SortOrder > finishedStage.SortOrder) .Include(s => s.Member) .Include(s => s.Filial) .OrderBy(s => s.SortOrder) .ThenBy(s => s.Id) .FirstOrDefault(); if (nextStage == null) { //could not locate next stage - can't create new job return null; } if (nextStage.Member == null && nextStage.Filial == null) { //no member or team specified - can't assing new job to anyone return null; } if (nextStage.Deadline == null || string.IsNullOrEmpty(nextStage.DeadlineUnit)) { //can't determine new job length return null; } if (!nextStage.Durati

## Delegated Callee Behavior
- NextStageJobScheduler.SelectMember: Accepted callee meaning for NextStageJobScheduler.SelectMember: { var jmpairs = (from member in _context.Members from job in _context.Jobs where member.Filial == filial.Id.ToString() && job.MemberId == member.Id.ToString() select new {Job = job
- NextStageJobScheduler.SetDates: Accepted callee meaning for NextStageJobScheduler.SetDates: { var memberId = Convert.ToInt32(newJob.MemberId); var member = _context.Members.FirstOrDefault(m => m.Id == memberId); var offsetTimespan = member.TimezoneOffsetMinutes.HasValue ?
- NextStageJobScheduler.SetTitle: Accepted callee meaning for NextStageJobScheduler.SetTitle: { var task = _context.Tasks.FirstOrDefault(t => t.Id == newJob.ParentId); var member = _context.Members.Find(Convert.ToInt32(newJob.MemberId)); if (task == null || member == null) 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
