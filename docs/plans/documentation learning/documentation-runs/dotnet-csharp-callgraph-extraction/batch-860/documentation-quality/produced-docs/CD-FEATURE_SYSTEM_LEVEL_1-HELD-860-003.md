# Documentation: askapediaweb::QuestionsController.GetMyFollowed

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.GetMyFollowed
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ AskUser userT = BL.User.GetUserByToken(Token); List<AskQuestion> dataT = new List<AskQuestion>(); string resultT = OK; if (userT != null) { dataT = Question.FollowedQuestions(userT.id); if (dataT == null) { resultT = ERROR; } } return new List<ActionResult> { new ActionResult(resultT, dataT) }; }

## Delegated Callee Behavior
- askapediaweb::Question.FollowedQuestions: Accepted callee meaning for askapediaweb::Question.FollowedQuestions: { using (AskapediaDBContext db = new AskapediaDBContext()) { var entries = (from question in db.Questions join mailList in db.MailingLists on question.Id equals mailList.QuestionId
- askapediaweb::User.GetUserByToken: Accepted callee meaning for askapediaweb::User.GetUserByToken: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Token == token); } }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Feature Boundary Meaning
This feature-level documentation lifts the accepted workflow and callee evidence into a feature boundary. It must keep local behavior, delegated behavior, and user-facing outcome separate.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
