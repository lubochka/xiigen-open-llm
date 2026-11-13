# Documentation: askapediaweb::QuestionsController.GetPagesNum

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.GetPagesNum
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ string qid = Question.GetQuestionId(questionId); int dataT = Question.getCommentsPageNum(new Guid(qid)); string resultT = OK; return new List<ActionResult> { new ActionResult(resultT, dataT) }; }

## Delegated Callee Behavior
- askapediaweb::Question.getCommentsPageNum: Accepted callee meaning for askapediaweb::Question.getCommentsPageNum: { using (AskapediaDBContext db = new AskapediaDBContext()) { var comments = db.Comments.Where(q => q.QuestionId == questionId).ToList(); return (comments.Count / 5) + 1; } }
- askapediaweb::Question.GetQuestionId: Accepted callee meaning for askapediaweb::Question.GetQuestionId: { if (IsGuid(questionparam)) return questionparam; else { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion q = db.Questions.FirstOrDefault(a => a.Url == quest

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Feature Boundary Meaning
This feature-level documentation lifts the accepted workflow and callee evidence into a feature boundary. It must keep local behavior, delegated behavior, and user-facing outcome separate.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
