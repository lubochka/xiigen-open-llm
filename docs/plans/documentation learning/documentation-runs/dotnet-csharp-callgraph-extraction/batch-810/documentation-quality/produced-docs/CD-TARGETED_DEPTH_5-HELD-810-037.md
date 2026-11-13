# Documentation: askapediaweb::Question.createIndex

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.createIndex
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { List<AskQuestion> questions = db.Questions.ToList(); foreach (AskQuestion Q in questions) { Q.Url = getQuestionUrl(Q); db.SaveChanges(); } } return true; }

## Delegated Callee Behavior
- askapediaweb::Question.getQuestionUrl: Accepted callee meaning for askapediaweb::Question.getQuestionUrl: { string questionStr = question.Question; Regex rgx = new Regex("[^a-zA-Z0-9-]"); if (User.mainLanguage == "he") { rgx = new Regex("[^a-zA-Zא-ת0-9-]"); } if (User.mainLanguage == "

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
