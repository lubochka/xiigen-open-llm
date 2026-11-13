# Documentation: LoginController.UpdateMyDetails

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\LoginController.cs
- Symbol: LoginController.UpdateMyDetails
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); var currentUser = HttpContext.User; UserStore users = GetUserByClaim(currentUser); if (users != null) { if (Update.Password != "" && Update.Password == Update.PasswordRepeat) { string passwordHash = ProcessLogic.ComputeHash(Update.Password, new SHA256CryptoServiceProvider()); users.Password = passwordHash; } users.Userid = Update.UserName; users.Email = Update.Email; users.Anonymous = false; users.Fullname = Update.FullName; var expires = DateTime.Now.AddMinutes(30); var tokenString = GenerateJWTToken(users); response = Ok(new { token = tokenString, isAdmin = users.IsAdmin, userName = users.Userid, isAnonymous = users.Anonymous, expires = expires, fullName = users.Fullname, Email = users.Email }); } return response; }

## Delegated Callee Behavior
- GameHelpers.GetUserByContext: Accepted callee meaning for GameHelpers.GetUserByContext: { var id = GetClaim("id", currentUser); Guid GId = Guid.NewGuid(); if (id != null) { GId = Guid.Parse(id); } string passwordHash = ""; Users users = _context.Users.Where(a => a.Id 
- LoginController.ComputeHash: Accepted callee meaning for LoginController.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- LoginController.GetUserByClaim: Accepted callee meaning for LoginController.GetUserByClaim: { var id = GetClaim("id", currentUser); Guid GId = Guid.NewGuid(); if (id != null) { GId = Guid.Parse(id); } string passwordHash = ""; UserStore users = getUser("id", GId.ToString(
- MapModel.ComputeHash: Accepted callee meaning for MapModel.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- ProcessDataResults.ComputeHash: Accepted callee meaning for ProcessDataResults.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- LoginController.ComputeHash: Accepted callee meaning for LoginController.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
