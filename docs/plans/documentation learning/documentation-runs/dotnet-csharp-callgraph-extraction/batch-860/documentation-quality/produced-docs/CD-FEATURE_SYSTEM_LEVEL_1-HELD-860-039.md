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

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
