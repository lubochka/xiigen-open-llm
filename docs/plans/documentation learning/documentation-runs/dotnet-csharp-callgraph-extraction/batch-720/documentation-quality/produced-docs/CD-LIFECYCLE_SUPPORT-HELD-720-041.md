# Documentation: askapediaweb::Question.getAnswersStatistics

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.getAnswersStatistics
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { List<AskAnswerStatistic> existingAnswers = db.GetGenStatistics(questionid).OrderBy(d => d.OrderNum).ToList(); if (existingAnswers != null) { return existingAnswers; } return null; } }

## Delegated Callee Behavior
- askapediaweb::AskapediaDBContext.GetGenStatistics: Accepted callee meaning for askapediaweb::AskapediaDBContext.GetGenStatistics: { SqlParameter questionIdParam = new SqlParameter("@questionId", questionId); return AnswerStatistics.SqlQuery("[askapedia].GetGenStatistics @questionId", questionIdParam).AsQuerya

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
