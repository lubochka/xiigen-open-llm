# Documentation: DataController.Update

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DataController.cs
- Symbol: DataController.Update
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ string json = RowStr.ToString(); dynamic Row = JsonConvert.DeserializeObject(json); string status = "OK"; var EditType = Row.EditType; if (EditType!=null && EditType.ToString()=="Changed") { using (DataBase database = new DataBase(_context)) { Views myView = _context.Views.SingleOrDefault(a => a.Id.ToString().ToLower() == ViewId.ToLower()); if (myView == null) { myView = _context.Views.SingleOrDefault(a => a.DisplayName == ViewId); } //select the view by view Id if (myView != null) { List<DataSourceFields> fields = _context.DataSourceFields.Where(a => a.DataSourceId == myView.DataSourceId).ToList(); List<SqlParameter> parameters = new List<SqlParameter>(); List<Entities> currEntities = new List<Entities>(); string values = "", update = "", where = ""; for (var i = 0; i < fields.Count; i++) { DataSourceFields field = fields[i]; //for now assume there is only one data entity in this case

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
