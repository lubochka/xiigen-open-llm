# Documentation: LoginDetails.IsAuth

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\AuthController.cs
- Symbol: LoginDetails.IsAuth
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser = new UserBack(); var identity = HttpContext.Current.User.Identity as ClaimsIdentity; if (identity != null) { myUser = new UserBack(identity); } try { if (myUser.Id > 0 ) { using (a1projmanageEntities lshdb = new a1projmanageEntities()) { Member authUser = lshdb.Members.FirstOrDefault(u => u.Id == myUser.Id); if (authUser != null) { return CreateBackUser(authUser); } } } return null; } catch(Exception em) { return null; } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
