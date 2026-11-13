# Documentation: NextStageJobScheduler.SetDates

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Services\NextStageJobScheduler.cs
- Symbol: NextStageJobScheduler.SetDates
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var memberId = Convert.ToInt32(newJob.MemberId); var member = _context.Members.FirstOrDefault(m => m.Id == memberId); var offsetTimespan = member.TimezoneOffsetMinutes.HasValue ? TimeSpan.FromMinutes(member.TimezoneOffsetMinutes.Value) : Constants.DefaultTimezoneOffset; var dayBegin = Constants.WorkdayBeginUtc - offsetTimespan; var dayEnd = Constants.WorkdayEndUtc - offsetTimespan; var jobs = _context.Jobs .Where(j => j.MemberId == newJob.MemberId) .Where(j => j.start.HasValue && j.end.HasValue) .Where(j => !j.Finished.HasValue || !j.Finished.Value) .OrderBy(j => j.start) .ToList(); var startDate = finishedJob.ActualEnd ?? DateTime.Now; newJob.Deadline = CalculateDeadlineDate(startDate, nextStage.Deadline.Value, nextStage.DeadlineUnit, offsetTimespan); while (true) { var endDate = startDate + TimeSpan.FromMinutes(nextStage.DurationMinutes.Value); var overlappingJobs = jobs.Where(j => j

## Delegated Callee Behavior
- NextStageJobScheduler.CalculateDeadlineDate: Accepted callee meaning for NextStageJobScheduler.CalculateDeadlineDate: { DateTime endDate; switch (unit) { case "h": endDate = startDate.AddBusinessHours(Convert.ToInt32(deadline), offsetTimezone); break; case "d": endDate = startDate.AddBusinessDays(
- DateTimeExtensions.AddBusinessDays: Accepted callee meaning for DateTimeExtensions.AddBusinessDays: { if (days < 0) { throw new ArgumentException("days cannot be negative", nameof(days)); } if (days == 0) return date; if (date.DayOfWeek == DayOfWeek.Saturday) { date = date.AddDay

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
