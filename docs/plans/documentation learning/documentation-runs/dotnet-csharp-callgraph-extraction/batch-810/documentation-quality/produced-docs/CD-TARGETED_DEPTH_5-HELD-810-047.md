# Documentation: askapediaweb::User.IsValidToken

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\User.cs
- Symbol: askapediaweb::User.IsValidToken
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { AskUser existingUser = GetUserByToken(token); if (existingUser == null || !existingUser.TokenDateExpire.HasValue || existingUser.TokenDateExpire.Value < DateTime.Now) { expirationDate = null; return false; } expirationDate = existingUser.TokenDateExpire; return true; } }

## Delegated Callee Behavior
- askapediaweb::User.GetUserByToken: Accepted callee meaning for askapediaweb::User.GetUserByToken: { using (AskapediaDBContext db = new AskapediaDBContext()) { return db.Users.FirstOrDefault(a => a.Token == token); } }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
