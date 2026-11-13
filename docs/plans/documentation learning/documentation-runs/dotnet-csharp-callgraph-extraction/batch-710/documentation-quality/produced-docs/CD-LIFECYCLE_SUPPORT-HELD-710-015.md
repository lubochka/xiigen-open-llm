# Documentation: DynamicController.ParseObjectAlternative

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DynamicController.cs
- Symbol: DynamicController.ParseObjectAlternative
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (Document.ToString().IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document.ToString()); for (var i = 0; i < documents.Length; i++) { results.Add(ParseObjectAlternative(documents[i])); } return results.ToArray(); } else { if (Document.ToString().IndexOf("{") == 0) { Dictionary<string, object> myObject = new Dictionary<string, object>(); dynamic stuff = JObject.Parse(Document.ToString()); foreach (Newtonsoft.Json.Linq.JProperty jproperty in stuff) { if (jproperty.Value.ToString().IndexOf("[") >= 0 || jproperty.Value.ToString().IndexOf("{") >= 0) { myObject[jproperty.Name] = ParseObjectAlternative(jproperty.Value); } else { myObject[jproperty.Name] = ParseJpropertyObject(jproperty); } } return myObject; } else { return ParseSimpleObject(Document); } } } public object ParseJpropertyObject(JProperty jproperty

## Delegated Callee Behavior
- DynamicController.ParseJpropertyObject: Accepted callee meaning for DynamicController.ParseJpropertyObject: { switch(jproperty.Value.Type) { case JTokenType.String: return jproperty.Value.ToString(); case JTokenType.Date: return DateTime.Parse(jproperty.Value.ToString()); case JTokenType
- DynamicController.ParseSimpleObject: Accepted callee meaning for DynamicController.ParseSimpleObject: { var value = Document.ToString(); try { var res = int.Parse(value); return res; } catch (Exception ex2) { try { var res = decimal.Parse(value); return res; } catch (Exception ex3)
- DynamicController.GetDocument: Accepted callee meaning for DynamicController.GetDocument: { ActionResult response = Unauthorized(); var currentUser = HttpContext.User; UserStore users = LoginController.GetStatUserByClaim(currentUser, _client); if (users != null) { Query
- DynamicController.ParseObject: Accepted callee meaning for DynamicController.ParseObject: { try { var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Document.ToString()); return result; } catch (Exception ex) { if (Document.ToString().C

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
