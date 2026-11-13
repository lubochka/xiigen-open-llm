# Documentation: askapediaweb::Question.GetQuestionId

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.GetQuestionId
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (IsGuid(questionparam)) return questionparam; else { using (AskapediaDBContext db = new AskapediaDBContext()) { AskQuestion q = db.Questions.FirstOrDefault(a => a.Url == questionparam); if (q != null) return q.Id.ToString(); else return questionparam; } } return questionparam; }

## Delegated Callee Behavior
- askapediaweb::Question.IsGuid: Accepted callee meaning for askapediaweb::Question.IsGuid: { if (candidate != null) { if (isGuid.IsMatch(candidate)) { return true; } } return false; }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
