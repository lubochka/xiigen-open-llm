# Documentation: LoginDetails.Register

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\AuthController.cs
- Symbol: LoginDetails.Register
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (a1projmanageEntities lshdb = new a1projmanageEntities()) { try { Member authUser = lshdb.Members.FirstOrDefault(u => u.UserName == user.user && u.CompanyName == user.company); if (authUser == null) { Member newMember = new Member(); newMember.IsSystemManager = true; newMember.Name = user.user; newMember.IsClientManager = false; newMember.IsFilialManager = false; newMember.ManageHimself = false; newMember.WorkAtSaturday = true; newMember.Description = ""; newMember.UserName = user.user; newMember.Password = user.password.ConvertToMD5(); newMember.CompanyName = user.company; newMember.CalendarColor = "red"; lshdb.Members.Add(newMember); lshdb.SaveChanges(); newMember.MainUserId = newMember.Id; lshdb.SaveChanges(); return Authenticate(user); } else { throw new HttpResponseException(HttpStatusCode.ExpectationFailed); } } catch (Exception em) { throw new HttpResponseException(HttpSta

## Delegated Callee Behavior
- LoginDetails.ConvertToMD5: Accepted callee meaning for LoginDetails.ConvertToMD5: { byte[] hash = Encoding.ASCII.GetBytes(str); MD5 md5 = new MD5CryptoServiceProvider(); byte[] hashenc = md5.ComputeHash(hash); string result = string.Empty; foreach (var b in hash
- a1BreezeController.SaveChanges: Accepted callee meaning for a1BreezeController.SaveChanges: { if (!IsAuth()) return null; return _db.SaveChanges(saveBundle); }
- LoginDetails.Authenticate: Accepted callee meaning for LoginDetails.Authenticate: { using (a1projmanageEntities lshdb = new a1projmanageEntities()) { try { string passMD5 = user.password.ConvertToMD5(); Member authUser = lshdb.Members.FirstOrDefault(u => u.UserN

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
