# Documentation: askapediaweb::AskUser.IsValidToken

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.5.2
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\AskapediaHeb\AskUser.cs
- Symbol: askapediaweb::AskUser.IsValidToken
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (askapedia2_heEntities db = new askapedia2_heEntities()) { ask355_users existingUser = AskUser.getUser(email, Token); if (existingUser != null) { return true; } return false; } }

## Delegated Callee Behavior
- askapediaweb::User.GetUser: Accepted callee meaning for askapediaweb::User.GetUser: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Email == email); } }
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::User.GetUser: Accepted callee meaning for askapediaweb::User.GetUser: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Email == email); } }
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::User.GetUser: Accepted callee meaning for askapediaweb::User.GetUser: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Email == email); } }
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if
- askapediaweb::AskUser.getUser: Accepted callee meaning for askapediaweb::AskUser.getUser: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { if

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
