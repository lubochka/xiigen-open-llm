# Documentation: LoginController.Register

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\LoginController.cs
- Symbol: LoginController.Register
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); Users newUser = new Users(); newUser.Userid = login.UserName; string passwordHash = ComputeHash(login.Password, new SHA256CryptoServiceProvider()); newUser.Password = passwordHash; newUser.Anonymous = false; newUser.IsAdmin = false; _context.Users.Add(newUser); _context.SaveChanges(); if (newUser != null) { var expires = DateTime.Now.AddMinutes(30); var tokenString = GenerateJWTToken(newUser); response = Ok(new { token = tokenString, isAdmin = newUser.IsAdmin, userName = newUser.Userid, isAnonymous = newUser.Anonymous, expires = expires, fullName = newUser.Fullname, Email = newUser.Email }); } return response; }

## Delegated Callee Behavior
- LoginController.ComputeHash: Accepted callee meaning for LoginController.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- MapModel.ComputeHash: Accepted callee meaning for MapModel.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- ProcessDataResults.ComputeHash: Accepted callee meaning for ProcessDataResults.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }
- LoginController.ComputeHash: Accepted callee meaning for LoginController.ComputeHash: { Byte[] inputBytes = Encoding.UTF8.GetBytes(input); Byte[] hashedBytes = algorithm.ComputeHash(inputBytes); return BitConverter.ToString(hashedBytes); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
