# Documentation: askapediaweb::Question.GetComments

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Question.cs
- Symbol: askapediaweb::Question.GetComments
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { int skip = (page - 1) * 5; return db.Comments.Where(q => q.QuestionId == questionId).OrderByDescending(a => a.Added).Skip(skip).Take(5).ToList(); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
