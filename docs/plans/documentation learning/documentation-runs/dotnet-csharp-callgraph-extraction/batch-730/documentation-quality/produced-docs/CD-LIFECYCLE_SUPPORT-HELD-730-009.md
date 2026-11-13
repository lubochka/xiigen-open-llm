# Documentation: DynamicController.ParseObject

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DynamicController.cs
- Symbol: DynamicController.ParseObject
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ try { var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Document.ToString()); return result; } catch (Exception ex) { if (Document.ToString().Contains("{")) { try { var result = new Dictionary<string, object>(); var workingDoc = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(Document.ToString()); var workingKeys = workingDoc.Keys.Where(a => true).ToList(); foreach (var key in workingKeys) { result[key] = ParseObject(workingDoc[key]); } return result; } catch(Exception exarr) { try { var results = new Dictionary<string, object[]>(); var workingDoc = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object[]>>(Document.ToString()); var workingKeys = workingDoc.Keys.Where(a => true).ToList(); foreach (var key in workingKeys) { var temp = new List<object>(); for (var i = 0; i < workingDoc[key].Length; i++) { temp.Add(

## Delegated Callee Behavior
- DynamicController.ParseSimpleObject: Accepted callee meaning for DynamicController.ParseSimpleObject: { var value = Document.ToString(); try { var res = int.Parse(value); return res; } catch (Exception ex2) { try { var res = decimal.Parse(value); return res; } catch (Exception ex3)

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
