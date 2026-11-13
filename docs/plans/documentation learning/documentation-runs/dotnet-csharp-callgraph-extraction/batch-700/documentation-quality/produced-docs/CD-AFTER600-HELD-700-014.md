# Documentation: askapediaweb::Vote.SaveVote

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\Vote.cs
- Symbol: askapediaweb::Vote.SaveVote
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ AskapediaDBContext db = new AskapediaDBContext(); AskAnswersChoice Chose = db.AnswersChoses.FirstOrDefault(a => a.QuestionId == questionId && a.UserId == UserId); if (Chose != null) { Chose.AnswerId = AnswerId; Chose.AddDate = DateTime.Now; } else { Chose = new AskAnswersChoice(); Chose.Id = Guid.NewGuid(); Chose.AnswerId = AnswerId; Chose.QuestionId = questionId; Chose.UserId = UserId; Chose.AddDate = DateTime.Now; db.AnswersChoses.Add(Chose); } db.SaveChanges(); return true; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
