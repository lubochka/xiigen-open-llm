# Documentation: DataController.GetData

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DataController.cs
- Symbol: DataController.GetData
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ using (DataBase database = new DataBase(_context)) { Views myView = _context.Views.SingleOrDefault(a => a.Id.ToString().ToLower() == ViewId.ToLower()); if (myView == null) { myView = _context.Views.SingleOrDefault(a => a.DisplayName == ViewId); } //select the view by view Id if (myView != null) { List<DataSourceFields> fields = _context.DataSourceFields.Where(a => a.DataSourceId == myView.DataSourceId).ToList(); if (fields.Count > 0) { string select = "", where = "", groupby = "", from = "",having=""; List<Entities> currEntities = new List<Entities>(); List<SqlParameter> parameters = new List<SqlParameter>(); for (var i = 0; i < fields.Count; i++) { DataSourceFields field = fields[i]; Fields myfield = _context.Fields.FirstOrDefault(a => a.Id.ToString() == field.FieldId); Entities myentity = currEntities.FirstOrDefault(a => a.Id.ToString().ToLower() == field.EntityId.ToLower()); if (mye

## Delegated Callee Behavior
- DataController.AddToSelect: Accepted callee meaning for DataController.AddToSelect: { if (field.ToDisplay == true) { if (select != "") select += ","; if (field.GroupFunc != "" && field.GroupFunc != null) { string groupFieldStr = "[" + myentity.DisplayName + "].[" 
- DataController.CreateJoinString: Accepted callee meaning for DataController.CreateJoinString: { for (var i = 1; i < currEntities.Count; i++) { Entities prevEntity = currEntities[i - 1]; Entities myentity = currEntities[i]; List<DataSourceFields> prevjoinfields = fields.Wher

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
