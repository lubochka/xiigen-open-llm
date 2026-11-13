# Documentation: ControllsController.GetControlls

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\ControllsController.cs
- Symbol: ControllsController.GetControlls
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ string sql = ""; List<SqlParameter> parameters = new List<SqlParameter>(); if (Guid.TryParse(ViewId, out var newGuid)) { sql = "SELECT * from [dbo].[ViewControls] where ViewId= @p"; parameters.Add(new SqlParameter("@p", newGuid)); } else { Views myView = _context.Views.SingleOrDefault(a => a.DisplayName == ViewId); if (myView != null) { sql = "SELECT * from [dbo].[ViewControls] where ViewId= @p"; parameters.Add(new SqlParameter("@p", myView.Id)); } } using (DataBase database = new DataBase(_context)) { return database.GetData(sql, parameters.ToArray()); } }

## Delegated Callee Behavior
- DataController.GetData: Accepted callee meaning for DataController.GetData: { using (DataBase database = new DataBase(_context)) { Views myView = _context.Views.SingleOrDefault(a => a.Id.ToString().ToLower() == ViewId.ToLower()); if (myView == null) { myVi
- DataBase.GetData: Accepted callee meaning for DataBase.GetData: { List<dynamic> mylist = new List<dynamic>(); using (var command = _context.Database.GetDbConnection().CreateCommand()) { command.CommandText = sql; command.CommandType = CommandTy

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Workflow / Use-Case Meaning
This workflow-level documentation is assembled from the local caller body plus every accepted callee summary. It must describe the use-case path without inventing behavior outside the evidence graph.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
