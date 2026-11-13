# Documentation: IndexAttribute.GetAll

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Services\Base\ElasticBaseService.cs
- Symbol: IndexAttribute.GetAll
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ SearchDescriptor<object> searchDescriptor = new SearchDescriptor<object>(IndexName).From(from).Size(size).Query(q => q.MatchAll()); ISearchResponse<object> response = Client.Search<object>(searchDescriptor); if (response.Hits.Count > 0) { Type type = typeof(T); ConstructorInfo ctor = type.GetConstructor(new[] { typeof(object) }); List<T> resList = new List<T>(); foreach (var hit in response.Hits) { T instance = ctor.Invoke(new object[] { hit.Source }) as T; resList.Add(instance); } return resList; } return new List<T>(); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
