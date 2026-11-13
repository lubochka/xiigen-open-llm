# Documentation: askapediaweb::AnswersController.GetAnswersStatisticsAge

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\AnswersController.cs
- Symbol: askapediaweb::AnswersController.GetAnswersStatisticsAge
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ string qid = Question.GetQuestionId(questionid); List<AnswerStatComplex> answers = Question.getAnswersStatisticsAge(new Guid(qid)); string resultT = OK; if (answers == null) { resultT = ERROR; } return new List<ActionResult> { new ActionResult(resultT, answers) }; }

## Delegated Callee Behavior
- askapediaweb::Question.GetQuestionId: Accepted callee meaning for askapediaweb::Question.GetQuestionId: { if (IsGuid(questionparam)) return questionparam; else { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion q = db.Questions.FirstOrDefault(a => a.Url == quest

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
