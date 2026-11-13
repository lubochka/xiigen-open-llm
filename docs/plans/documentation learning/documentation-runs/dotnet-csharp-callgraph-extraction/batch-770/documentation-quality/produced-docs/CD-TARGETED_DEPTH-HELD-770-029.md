# Documentation: a1BreezeController.Members

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.Members
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Member>().AsQueryable(); try { if (myUser.IsSystemManager==true) { return _db.Context.Members.Where(e => e.MainUserId==myUser.MainUserId).OrderBy(q => q.Name); } if (myUser.IsFilialManager==true) { string Filial = myUser.Filial; return _db.Context.Members.Where(e => e.Filial==Filial && e.MainUserId == myUser.MainUserId).OrderBy(q => q.Name); } else { int member = myUser.Id; return _db.Context.Members.Where(e => e.Id == member && e.MainUserId == myUser.MainUserId).OrderBy(q => q.Name); } } catch (Exception em) { return new List<Member>().AsQueryable(); } }

## Delegated Callee Behavior
- a1BreezeController.IsAuth: Accepted callee meaning for a1BreezeController.IsAuth: { myUser = GetUser(); //return true; if (myUser.Id > 0) return true; return false; }
- LoginDetails.IsAuth: Accepted callee meaning for LoginDetails.IsAuth: { UserBack myUser = new UserBack(); var identity = HttpContext.Current.User.Identity as ClaimsIdentity; if (identity != null) { myUser = new UserBack(identity); } try { if (myUser.

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
