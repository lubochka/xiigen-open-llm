# Documentation: NextStageJobScheduler.CalculateDeadlineDate

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Services\NextStageJobScheduler.cs
- Symbol: NextStageJobScheduler.CalculateDeadlineDate
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ DateTime endDate; switch (unit) { case "h": endDate = startDate.AddBusinessHours(Convert.ToInt32(deadline), offsetTimezone); break; case "d": endDate = startDate.AddBusinessDays(Convert.ToInt32(deadline)); break; case "w": endDate = startDate + TimeSpan.FromDays(deadline * 7); break; default: throw new ArgumentException("Invalid unit: " + unit, nameof(unit)); } return endDate; }

## Delegated Callee Behavior
- DateTimeExtensions.AddBusinessHours: Accepted callee meaning for DateTimeExtensions.AddBusinessHours: { var dayBegin = Constants.WorkdayBeginUtc - offetTimezone; var dayEnd = Constants.WorkdayBeginUtc - offetTimezone; var totalTimespan = TimeSpan.FromHours(hours); var todayEnd = da
- DateTimeExtensions.AddBusinessDays: Accepted callee meaning for DateTimeExtensions.AddBusinessDays: { if (days < 0) { throw new ArgumentException("days cannot be negative", nameof(days)); } if (days == 0) return date; if (date.DayOfWeek == DayOfWeek.Saturday) { date = date.AddDay

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
