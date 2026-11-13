# Documentation: askapediaweb::UsersController.SendWelcomeEmail

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\UsersController.cs
- Symbol: askapediaweb::UsersController.SendWelcomeEmail
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (BL.User.mainLanguage == "en") { //EN: MailService.SendMail("info@askapedia.com", email, "Welcome to Askapedia!", "Welcome to <a href='https://www.askapedia.com'>Askapedia.com</a><br/> Your user have been created. Your password is:" + password + "<br/>Sincerely yours, <a href='https://askapedia.com'>Askapedia</a> team."); } if (BL.User.mainLanguage == "he") { //HE: MailService.SendMail("info@askapedia.com", email, "ברוכים הבאים לאסקפדיה!", "ברוכים הבאים ל <a href='https://www.askapedia.co.il'>Askapedia.co.il</a><br/>משתמש שלך נוצר. סיסמתך היא:" + password + "<br/>בכבוד רב, <a href='https://askapedia.co.il'>צוות אסקפדיה</a>."); } if (BL.User.mainLanguage == "ru") { //HE: MailService.SendMail("info@askapedia.com", email, "Добро пожаловать в аскапедию!", "Добро пожаловать на сайт <a href='https://www.askapedia.ru'>Askapedia.ru</a><br/>Ваша учетная запись создана. Ваш пароль:" + passwor

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
