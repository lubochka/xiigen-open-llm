# Documentation: ObjectValue.ParseObjectAlternative

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\BusinessLogic\ObjectProcess.cs
- Symbol: ObjectValue.ParseObjectAlternative
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (Document.ToString().IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document.ToString()); for (var i = 0; i < documents.Length; i++) { results.Add(ParseObjectAlternative(documents[i])); } return results.ToArray(); } else { if (Document.ToString().IndexOf("{") == 0) { Dictionary<string, object> myObject = new Dictionary<string, object>(); dynamic stuff = JObject.Parse(Document.ToString()); foreach (Newtonsoft.Json.Linq.JProperty jproperty in stuff) { if (jproperty.Value.ToString().IndexOf("[") >= 0 || jproperty.Value.ToString().IndexOf("{") >= 0) { myObject[jproperty.Name] = ParseObjectAlternative(jproperty.Value); } else { myObject[jproperty.Name] = ParseJpropertyObject(jproperty); } } return myObject; } else { return ParseSimpleObject(Document); } } } public static ObjectValue ParseObjectAlternativeObject

## Delegated Callee Behavior
- ObjectValue.ParseJpropertyObject: Accepted callee meaning for ObjectValue.ParseJpropertyObject: { switch (jproperty.Value.Type) { case JTokenType.String: return jproperty.Value.ToString(); case JTokenType.Date: return DateTime.Parse(jproperty.Value.ToString()); case JTokenTyp
- ObjectValue.ParseSimpleObject: Accepted callee meaning for ObjectValue.ParseSimpleObject: { var value = Document.ToString(); try { var res = int.Parse(value); return res; } catch (Exception ex2) { try { var res = decimal.Parse(value); return res; } catch (Exception ex3)
- ObjectValue.ParseObjectAlternativeObjectValue: Accepted callee meaning for ObjectValue.ParseObjectAlternativeObjectValue: { var resultObj = new ObjectValue(); if (Document.ToString().IndexOf("[") == 0) { var results = new List<ObjectValue>(); var documents = System.Text.Json.JsonSerializer.Deserialize
- ObjectValue.ParseElasticResult: Accepted callee meaning for ObjectValue.ParseElasticResult: { var res = new Dictionary<string, string>(); var work = ((Dictionary<string, object>)elasticRow); foreach(var key in work.Keys ) { res[key] = work[key].ToString(); } return res; }
- ObjectValue.ParseObject: Accepted callee meaning for ObjectValue.ParseObject: { try { var result = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(Document.ToString()); return result; } catch (Exception ex) { if (Document.ToString().C

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## System Boundary Meaning
This system-level documentation describes the externally visible system surface only after preserving source evidence, runtime version, workflow meaning, and delegated callee behavior.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
