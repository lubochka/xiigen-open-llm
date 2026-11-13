# Documentation: askapediaweb::QuestionsController.GetUnAnswerQuestions

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\QuestionsController.cs
- Symbol: askapediaweb::QuestionsController.GetUnAnswerQuestions
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ AskUser userT = null; try { userT = BL.User.GetUserByToken(Token); } catch(Exception em) { } List<AskQuestion> data = new List<AskQuestion>(); var userId = userT != null ? userT.id : Guid.NewGuid(); return Question.getUnAnsweredQuestions(userId); }

## Delegated Callee Behavior
- askapediaweb::Question.getUnAnsweredQuestions: Accepted callee meaning for askapediaweb::Question.getUnAnsweredQuestions: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.GetUnAnsweredQuestion(userId).Take(10).ToList(); } }
- askapediaweb::User.GetUserByToken: Accepted callee meaning for askapediaweb::User.GetUserByToken: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Token == token); } }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
