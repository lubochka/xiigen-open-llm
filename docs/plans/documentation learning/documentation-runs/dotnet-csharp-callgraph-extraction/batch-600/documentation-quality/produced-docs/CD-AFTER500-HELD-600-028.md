# Documentation: LoginController.UpdateMyDetails

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\LoginController.cs
- Symbol: LoginController.UpdateMyDetails
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); var currentUser = HttpContext.User; Users users = GameHelpers.GetUserByContext(currentUser,_context); if (users != null) { if (Update.Password!="" && Update.Password == Update.PasswordRepeat) { string passwordHash = ComputeHash(Update.Password, new SHA256CryptoServiceProvider()); users.Password = passwordHash; } users.Userid = Update.UserName; users.Email = Update.Email; users.Anonymous = false; users.Fullname = Update.FullName; var expires = DateTime.Now.AddMinutes(30); var tokenString = GenerateJWTToken(users); response = Ok(new { token = tokenString, isAdmin = users.IsAdmin, userName = users.Userid, isAnonymous = users.Anonymous, expires = expires, fullName = users.Fullname, Email = users.Email }) ; } return response; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
