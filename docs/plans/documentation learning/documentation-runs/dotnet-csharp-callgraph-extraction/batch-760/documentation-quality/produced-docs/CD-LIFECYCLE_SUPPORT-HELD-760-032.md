# Documentation: DynamicController.CreateSearchDescriptor

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\Controllers\DynamicController.cs
- Symbol: DynamicController.CreateSearchDescriptor
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var result = searchDescriptor; if (Document.IndexOf("[") == 0) { var results = new List<object>(); var documents = System.Text.Json.JsonSerializer.Deserialize<object[]>(Document); for (var i = 0; i < documents.Length; i++) { result = CreateSearchDescriptor(prefix, documents[i].ToString(), result); } return result; } else { if (Document.IndexOf("{") == 0) { Dictionary<string, object> myObject = new Dictionary<string, object>(); dynamic stuff = JObject.Parse(Document.ToString()); foreach (Newtonsoft.Json.Linq.JProperty jproperty in stuff) { if (jproperty.Value.ToString().IndexOf("[") >= 0 || jproperty.Value.ToString().IndexOf("{") >= 0) { var newPrefix = prefix + jproperty.Name; if(prefix!="") { newPrefix = prefix+ "."+ jproperty.Name; } result = CreateSearchDescriptor(newPrefix, jproperty.Value.ToString(), result); } else { var newPrefix = prefix + jproperty.Name; if (prefix != "") { ne

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
