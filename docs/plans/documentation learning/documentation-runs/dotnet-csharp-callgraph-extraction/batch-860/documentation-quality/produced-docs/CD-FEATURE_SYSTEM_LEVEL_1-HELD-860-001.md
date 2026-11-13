# Documentation: askapediaweb::QuestionsController.AddComment

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.AddComment
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ AskUser userT = BL.User.GetUserByToken(Token); string resultT = OK; object dataT = null; if (userT != null) { string qid = Question.GetQuestionId(questionId); dataT = Question.AddComment(userT.id, new Guid(qid), comment, page); } else { resultT = ERROR; dataT = null; } return new List<ActionResult> { new ActionResult(resultT, dataT) }; }

## Delegated Callee Behavior
- askapediaweb::Question.GetQuestionId: Accepted callee meaning for askapediaweb::Question.GetQuestionId: { if (IsGuid(questionparam)) return questionparam; else { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion q = db.Questions.FirstOrDefault(a => a.Url == quest
- askapediaweb::User.GetUserByToken: Accepted callee meaning for askapediaweb::User.GetUserByToken: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Token == token); } }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Feature Boundary Meaning
This feature-level documentation lifts the accepted workflow and callee evidence into a feature boundary. It must keep local behavior, delegated behavior, and user-facing outcome separate.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
