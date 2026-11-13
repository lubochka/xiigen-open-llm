# Documentation: LoginController.Register

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Controllers\LoginController.cs
- Symbol: LoginController.Register
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ IActionResult response = Unauthorized(); UserStore newUser = new UserStore(); newUser.Userid = login.UserName; string passwordHash = ProcessLogic.ComputeHash(login.Password, new SHA256CryptoServiceProvider()); newUser.Password = passwordHash; newUser.Anonymous = false; newUser.IsAdmin = false; //TODO: store in elastic IndexRequest<object> request = new IndexRequest<object>(newUser, "userstore", newUser.Userid); _client.Index<object>(request); if (newUser != null) { var expires = DateTime.Now.AddMinutes(30); var tokenString = GenerateJWTToken(newUser); response = Ok(new { token = tokenString, isAdmin = newUser.IsAdmin, userName = newUser.Userid, isAnonymous = newUser.Anonymous, expires = expires, fullName = newUser.Fullname, Email = newUser.Email }); } return response; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
