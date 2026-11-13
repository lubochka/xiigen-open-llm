# Documentation: DataController.AddToWhere

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DataController.cs
- Symbol: DataController.AddToWhere
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (where != "") where += ","; int count = parameters.Count + 1; where += "[" + myentity.DisplayName + "].[" + myfield.SystemName + "] " + field.WhereType + " @p" + count; SqlParameter param = CreateParameterByType(myfield, field.WhereValue, count); parameters.Add(param); return where; }

## Delegated Callee Behavior
- DataController.CreateParameterByType: Accepted callee meaning for DataController.CreateParameterByType: { SqlParameter param = null; switch (myfield.SysType) { case "nvarchar(50)": param = new SqlParameter("@p" + pcount, SqlDbType.NVarChar, 50); break; case "uniqueidentifier": param 

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
