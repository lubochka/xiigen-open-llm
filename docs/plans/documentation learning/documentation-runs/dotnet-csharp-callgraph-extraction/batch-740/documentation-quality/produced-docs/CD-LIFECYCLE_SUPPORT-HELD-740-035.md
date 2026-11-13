# Documentation: askapediaweb::Question.getAnswersStatisticsAge

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.getAnswersStatisticsAge
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { List<GeneralStatAnswer> existingAnswers = db.GetGenStatisticsAge(questionid).OrderBy(d => d.OrderNum).ToList(); if (existingAnswers != null) { List<AnswerStatComplex> res = new List<AnswerStatComplex>(); foreach (var answer in existingAnswers) { AnswerStatComplex me = GetAnswerStatComplex(res, answer.Id, answer.Answer); AnswerStatType newStat = new AnswerStatType(); newStat.type = answer.Mytype; newStat.count = answer.CountUser; newStat.color = answer.color; me.AnswerStats.Add(newStat); } return res; } return null; } }

## Delegated Callee Behavior
- askapediaweb::Question.GetAnswerStatComplex: Accepted callee meaning for askapediaweb::Question.GetAnswerStatComplex: { foreach (AnswerStatComplex answer in answers) { if (answer.Id == Id) return answer; } AnswerStatComplex result = new AnswerStatComplex(); result.Id = Id; result.Answer = AnswerS;
- askapediaweb::AskapediaDBContext.GetGenStatisticsAge: Accepted callee meaning for askapediaweb::AskapediaDBContext.GetGenStatisticsAge: { SqlParameter questionIdParam = new SqlParameter("@questionId", questionId); return Database.SqlQuery<GeneralStatAnswer>("[askapedia].GetGenStatisticsAge @questionId", questionIdP

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
