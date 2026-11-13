# Documentation: DataController.CreateJoinString

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DataController.cs
- Symbol: DataController.CreateJoinString
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ for (var i = 1; i < currEntities.Count; i++) { Entities prevEntity = currEntities[i - 1]; Entities myentity = currEntities[i]; List<DataSourceFields> prevjoinfields = fields.Where(a => a.JoinEntityId.ToLower() == prevEntity.Id.ToString().ToLower()).ToList(); List<DataSourceFields> joinfields = fields.Where(a => a.JoinEntityId.ToLower() == myentity.Id.ToString().ToLower()).ToList(); if (joinfields.Count > 0 && prevjoinfields.Count > 0) { if (from != "") from += " INNER JOIN "; else from += "[dbo].[" + prevEntity.SysName + "] as [" + prevEntity.DisplayName + "] INNER JOIN "; from += "[dbo].[" + myentity.SysName + "] as [" + myentity.DisplayName + "] ON "; bool and = false; for (var j = 0; j < joinfields.Count; j++) { DataSourceFields joinfield = joinfields[j]; Fields myfield = _context.Fields.FirstOrDefault(a => a.Id.ToString() == joinfield.FieldId); string field1 = "[dbo].[" + myentity.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
