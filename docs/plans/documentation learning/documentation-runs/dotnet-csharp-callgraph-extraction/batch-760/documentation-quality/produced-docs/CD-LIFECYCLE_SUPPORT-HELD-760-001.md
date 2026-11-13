# Documentation: askapediaweb::User.RegisterWithToken

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\User.cs
- Symbol: askapediaweb::User.RegisterWithToken
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { AskUser existingUser = db.Users.FirstOrDefault(a => a.Email == email); if (existingUser != null) return false; existingUser = db.Users.FirstOrDefault(a => a.Token == token); if (existingUser == null) return false; existingUser.Email = email; existingUser.pass = Security.Encrypt(password); db.SaveChanges(); return true; } }

## Delegated Callee Behavior
- askapediaweb::Security.Encrypt: Accepted callee meaning for askapediaweb::Security.Encrypt: { string EncryptionKey = "MAKV2SPBNI99212"; byte[] clearBytes = Encoding.Unicode.GetBytes(clearText); using (Aes encryptor = Aes.Create()) { Rfc2898DeriveBytes pdb = new Rfc2898Der
- askapediaweb::Security.Encrypt: Accepted callee meaning for askapediaweb::Security.Encrypt: { string EncryptionKey = "MAKV2SPBNI99212"; byte[] clearBytes = Encoding.Unicode.GetBytes(clearText); using (Aes encryptor = Aes.Create()) { Rfc2898DeriveBytes pdb = new Rfc2898Der

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
