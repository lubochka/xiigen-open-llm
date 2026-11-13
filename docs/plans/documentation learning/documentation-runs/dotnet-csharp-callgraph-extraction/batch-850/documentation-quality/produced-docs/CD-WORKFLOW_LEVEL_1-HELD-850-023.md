# Documentation: askapediaweb::UsersController.RegisterNewUser

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Controllers\UsersController.cs
- Symbol: askapediaweb::UsersController.RegisterNewUser
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ bool res = false; if (User.Identity.IsAuthenticated && !string.IsNullOrWhiteSpace(Token)) res = BL.User.RegisterWithToken(email, password, Token); else res = BL.User.Register(email, password); string resultT = OK; LoginResponse loginResponse = null; if (res) { SendWelcomeEmail(email, password); loginResponse = BL.User.Login(email, password); if (loginResponse == null) { resultT = ERROR; } } else { resultT = ERROR; return new ActionResult<RegistrationResponse>(resultT, null); } return new ActionResult<RegistrationResponse>(resultT, new RegistrationResponse { Token = loginResponse.Token, TokenExpiration = loginResponse.TokenExpiration }); }

## Delegated Callee Behavior
- askapediaweb::User.Login: Accepted callee meaning for askapediaweb::User.Login: { using (AskapediaDBContext db = new AskapediaDBContext()) { AskUser existingUser = db.Users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { string pass = Securi
- askapediaweb::UsersController.Login: Accepted callee meaning for askapediaweb::UsersController.Login: { var loginResult = BL.User.Login(email, password); if (loginResult == null) return new ActionResult<LoginResponse>(ERROR, null); return new ActionResult<LoginResponse>(OK, loginRe
- askapediaweb::AskUser.Login: Accepted callee meaning for askapediaweb::AskUser.Login: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { st
- askapediaweb::AskUser.Login: Accepted callee meaning for askapediaweb::AskUser.Login: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { st
- askapediaweb::AskUser.Login: Accepted callee meaning for askapediaweb::AskUser.Login: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser != null) { st
- askapediaweb::User.Register: Accepted callee meaning for askapediaweb::User.Register: { using (AskapediaDBContext db = new AskapediaDBContext()) { AskUser existingUser = db.Users.FirstOrDefault(a => a.Email == email); if (existingUser == null) { AskUser newUser = ne
- askapediaweb::AskUser.Register: Accepted callee meaning for askapediaweb::AskUser.Register: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser == null) { as
- askapediaweb::AskUser.Register: Accepted callee meaning for askapediaweb::AskUser.Register: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser == null) { as
- askapediaweb::AskUser.Register: Accepted callee meaning for askapediaweb::AskUser.Register: { using (askapedia2_ruEntities db = new askapedia2_ruEntities()) { ask355_users existingUser = db.ask355_users.FirstOrDefault(a => a.Email == email); if (existingUser == null) { as
- askapediaweb::User.RegisterWithToken: Accepted callee meaning for askapediaweb::User.RegisterWithToken: { using (AskapediaDBContext db = new AskapediaDBContext()) { AskUser existingUser = db.Users.FirstOrDefault(a => a.Email == email); if (existingUser != null) return false; existing
- askapediaweb::UsersController.SendWelcomeEmail: Accepted callee meaning for askapediaweb::UsersController.SendWelcomeEmail: { if (BL.User.mainLanguage == "en") { //EN: MailService.SendMail("info@askapedia.com", email, "Welcome to Askapedia!", "Welcome to <a href='https://www.askapedia.com'>Askapedia.com

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
