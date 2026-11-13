# Documentation: Startup.CreateIndex

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Startup.cs
- Symbol: Startup.CreateIndex
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ var createIndexResponse = client.Indices.Create(indexName, index => index.Map<object>(x => x.AutoMap()) ); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
