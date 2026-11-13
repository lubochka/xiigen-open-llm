# Documentation: DynamicJsonConverter.TryGetIndex

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\wizGameserver
- File: WebApplication2\DataLogic\DynamicJsonConverterer.cs
- Symbol: DynamicJsonConverter.TryGetIndex
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (indexes.Length == 1 && indexes[0] != null) { if (!_dictionary.TryGetValue(indexes[0].ToString(), out result)) { // return null to avoid exception. caller can check for null this way... result = null; return true; } result = WrapResultObject(result); return true; } return base.TryGetIndex(binder, indexes, out result); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
