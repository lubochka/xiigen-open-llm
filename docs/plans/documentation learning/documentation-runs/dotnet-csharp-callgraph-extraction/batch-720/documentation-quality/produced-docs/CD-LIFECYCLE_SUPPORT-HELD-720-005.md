# Documentation: askapediaweb::QuestionsController.DeleteQuestion

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.DeleteQuestion
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ AskUser userT = BL.User.GetUserByToken(Token); string resultT = OK; bool dataT = false; if (userT != null) { string qid = Question.GetQuestionId(questionId); dataT = Question.removeQuestion(new Guid(qid), userT); } return new List<ActionResult> { new ActionResult(resultT, dataT) }; }

## Delegated Callee Behavior
- askapediaweb::Question.GetQuestionId: Accepted callee meaning for askapediaweb::Question.GetQuestionId: { if (IsGuid(questionparam)) return questionparam; else { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion q = db.Questions.FirstOrDefault(a => a.Url == quest
- askapediaweb::User.GetUserByToken: Accepted callee meaning for askapediaweb::User.GetUserByToken: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Token == token); } }
- askapediaweb::Question.removeQuestion: Accepted callee meaning for askapediaweb::Question.removeQuestion: { using (AskapediaDBContext db = new AskapediaDBContext()) { if (user.IsAdmin == 1) { AskQuestion existing = db.Questions.FirstOrDefault(q => q.Id == questionId); if (existing != n

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
