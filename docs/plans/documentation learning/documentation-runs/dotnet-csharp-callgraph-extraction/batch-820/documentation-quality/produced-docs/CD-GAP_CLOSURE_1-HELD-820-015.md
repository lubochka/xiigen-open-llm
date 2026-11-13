# Documentation: DynamicJsonConverter.WrapResultObject

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\DynamicJsonConverterer.cs
- Symbol: DynamicJsonConverter.WrapResultObject
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var dictionary = result as IDictionary<string, object>; if (dictionary != null) return new DynamicJsonObject(dictionary); var arrayList = result as ArrayList; if (arrayList != null && arrayList.Count > 0) { return arrayList[0] is IDictionary<string, object> ? new List<object>(arrayList.Cast<IDictionary<string, object>>().Select(x => new DynamicJsonObject(x))) : new List<object>(arrayList.Cast<object>()); } return result; }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
