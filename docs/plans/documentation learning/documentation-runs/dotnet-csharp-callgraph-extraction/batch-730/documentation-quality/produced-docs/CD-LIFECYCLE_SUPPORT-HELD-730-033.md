# Documentation: askapediaweb::MailService.SendMail

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\BL\MailService.cs
- Symbol: askapediaweb::MailService.SendMail
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ MailMessage msg = new MailMessage(from, to, subject, body) { IsBodyHtml = true }; SmtpClient smtpClient = CreateSmtpClient(); smtpClient.Send(msg); }

## Delegated Callee Behavior
- askapediaweb::MailService.CreateSmtpClient: Accepted callee meaning for askapediaweb::MailService.CreateSmtpClient: { NetworkCredential netCred = new NetworkCredential("info@askapedia.com", "krol1312"); SmtpClient smtpobj = new SmtpClient("smtp.gmail.com", 587); smtpobj.EnableSsl = true; smtpobj
- askapediaweb::MailService.CreateSmtpClient: Accepted callee meaning for askapediaweb::MailService.CreateSmtpClient: { NetworkCredential netCred = new NetworkCredential("info@askapedia.com", "krol1312"); SmtpClient smtpobj = new SmtpClient("smtp.gmail.com", 587); smtpobj.EnableSsl = true; smtpobj

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
