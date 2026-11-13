# Documentation: askapediaweb::Question.getRelevantQuestions

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.getRelevantQuestions
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { string phraseNew = phrase.Replace("?", string.Empty); return db.GetRelatedQuestions(phraseNew.Trim()).Take(8).ToList(); } }

## Delegated Callee Behavior
- askapediaweb::AskapediaDBContext.GetRelatedQuestions: Accepted callee meaning for askapediaweb::AskapediaDBContext.GetRelatedQuestions: { SqlParameter phraseParam = new SqlParameter("@Phrase", v); return Questions.SqlQuery("[askapedia].GetRelatedQuestions @Phrase", phraseParam).AsQueryable(); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
