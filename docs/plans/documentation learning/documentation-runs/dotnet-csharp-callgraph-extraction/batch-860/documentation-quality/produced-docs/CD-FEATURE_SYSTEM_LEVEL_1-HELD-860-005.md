# Documentation: askapediaweb::QuestionsController.GetQuestion

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.GetQuestion
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ string qid = Question.GetQuestionId(questionId); AskQuestion res = Question.getQuestion(new Guid(qid)); string resultT = OK; AskQuestionResponce dataT = null; if (res != null) { AskQuestionResponce result = new AskQuestionResponce { Id = res.Id, UserOwner = res.UserOwner, Question = res.Question, url = Question.GetUrl(res.Id) }; dataT = result; } else { resultT = ERROR; } return new List<ActionResult> { new ActionResult(resultT, dataT) }; }

## Delegated Callee Behavior
- askapediaweb::Question.GetQuestionId: Accepted callee meaning for askapediaweb::Question.GetQuestionId: { if (IsGuid(questionparam)) return questionparam; else { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion q = db.Questions.FirstOrDefault(a => a.Url == quest
- askapediaweb::Question.GetUrl: Accepted callee meaning for askapediaweb::Question.GetUrl: { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion data = db.Questions.FirstOrDefault(a => a.Id == id); if (data != null) { return data.Url; } return null; } 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Feature Boundary Meaning
This feature-level documentation lifts the accepted workflow and callee evidence into a feature boundary. It must keep local behavior, delegated behavior, and user-facing outcome separate.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
