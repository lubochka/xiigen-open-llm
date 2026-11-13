# Documentation: a1BreezeController.Filials

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.Filials
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Filial>().AsQueryable(); try { if (myUser.IsSystemManager==true) { return _db.Context.Filials.Where(e =>e.MainUserId==myUser.MainUserId); } else { var FilialS = myUser.Filial; try { int Filial = int.Parse(FilialS); return _db.Context.Filials.Where(e => e.Id == Filial && e.MainUserId == myUser.MainUserId); } catch(Exception m) { return new List<Filial>().AsQueryable(); } } } catch(Exception em) { return new List<Filial>().AsQueryable(); } }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
