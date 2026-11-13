# Documentation: askapediaweb::User.RequestPasswordChange

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\User.cs
- Symbol: askapediaweb::User.RequestPasswordChange
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (AskapediaDBContext db = new AskapediaDBContext()) { AskUser existingUser = db.Users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { Guid Token = Guid.NewGuid(); existingUser.PassTokenRequest = Token; DateTime today = DateTime.Now; existingUser.PassTokenPasses = today.AddMinutes(40); db.SaveChanges(); //EN if (mainLanguage == "en") { MailService.SendMail("info@askapedia.com", email, "Password change request", "Change password secret key:" + Token.ToString()); } if (mainLanguage == "he") { //HE: MailService.SendMail("info@askapedia.com", email, "בקשה לשינוי סיסמא", "<div style='direction:rtl'>" + "להלן מספר סודי לשינוי סיסמא:" + Token.ToString() + "</div>"); } if (mainLanguage == "ru") { //RU: MailService.SendMail("info@askapedia.com", email, "Просьба сменить пароль", "" + "Ваш секретный код для изменения пароля:" + Token.ToString() + ""); } return "Key sent to 

## Delegated Callee Behavior
- askapediaweb::MailService.SendMail: Accepted callee meaning for askapediaweb::MailService.SendMail: { MailMessage o = new MailMessage(From, To, Subject, Body); o.IsBodyHtml = true; NetworkCredential netCred = new NetworkCredential("info@askapedia.com", "Luba5735"); SmtpClient smt
- askapediaweb::MailService.SendMail: Accepted callee meaning for askapediaweb::MailService.SendMail: { MailMessage o = new MailMessage(From, To, Subject, Body); o.IsBodyHtml = true; NetworkCredential netCred = new NetworkCredential("info@askapedia.com", "Luba5735"); SmtpClient smt
- askapediaweb::MailService.SendMail: Accepted callee meaning for askapediaweb::MailService.SendMail: { MailMessage o = new MailMessage(From, To, Subject, Body); o.IsBodyHtml = true; NetworkCredential netCred = new NetworkCredential("info@askapedia.com", "Luba5735"); SmtpClient smt

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
