# Documentation: askapediaweb::Question.removeQuestion

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.removeQuestion
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { if (user.IsAdmin == 1) { AskQuestion existing = db.Questions.FirstOrDefault(q => q.Id == questionId); if (existing != null) { List<Data.AskQuestionLead> leads = db.QuestionLeads.Where(lead => lead.Question.Id == questionId).ToList(); foreach (var lead in leads) { db.QuestionLeads.Remove(lead); } db.Questions.Remove(existing); //remove answers List<Data.AskAnswer> answers = db.Answers.Where(ans => ans.Question.Id == questionId).ToList(); foreach (var answer in answers) { db.Answers.Remove(answer); } List<AskAnswersChoice> answersChoses = db.AnswersChoses.Where(ans => ans.QuestionId == questionId).ToList(); foreach (var answerChose in answersChoses) { db.AnswersChoses.Remove(answerChose); } db.SaveChanges(); return true; } } return false; } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
