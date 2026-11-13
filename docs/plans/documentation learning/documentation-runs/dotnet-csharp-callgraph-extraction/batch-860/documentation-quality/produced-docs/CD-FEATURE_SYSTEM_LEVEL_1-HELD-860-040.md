# Documentation: a1BreezeController.Messages

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\a1BreezeController.cs
- Symbol: a1BreezeController.Messages
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ UserBack myUser; if (!IsAuth(out myUser)) return new List<Message>().AsQueryable(); return _db.Context.Messages.Where(e => e.MainUserId==myUser.MainUserId); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
