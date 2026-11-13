# Documentation: DateTimeExtensions.AddBusinessHours

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Extensions\DateTimeExtensions.cs
- Symbol: DateTimeExtensions.AddBusinessHours
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var dayBegin = Constants.WorkdayBeginUtc - offetTimezone; var dayEnd = Constants.WorkdayBeginUtc - offetTimezone; var totalTimespan = TimeSpan.FromHours(hours); var todayEnd = date.Date + dayEnd; if (todayEnd - date > totalTimespan) { return date + totalTimespan; } var finishingDate = date.AddBusinessDays(1); var todayPortion = todayEnd - date; var remainigSpan = totalTimespan - todayPortion; var wholeDays = Convert.ToInt32(Math.Floor(remainigSpan.TotalHours / Constants.WorkdayLength.TotalHours)); remainigSpan = remainigSpan - TimeSpan.FromHours(wholeDays * Constants.WorkdayLength.TotalHours); finishingDate = finishingDate.AddBusinessDays(wholeDays).Date + dayBegin + remainigSpan; return finishingDate; }

## Delegated Callee Behavior
- DateTimeExtensions.AddBusinessDays: Accepted callee meaning for DateTimeExtensions.AddBusinessDays: { if (days < 0) { throw new ArgumentException("days cannot be negative", nameof(days)); } if (days == 0) return date; if (date.DayOfWeek == DayOfWeek.Saturday) { date = date.AddDay

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
