# Documentation: askapediaweb::AskUser.getUser

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.5.2
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\AskapediaRu\AskUser.cs
- Symbol: askapediaweb::AskUser.getUser
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if (existingUser.Token == Token && existingUser.TokenDateExpire >= new DateTime()) { return existingUser; } } return null; } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
