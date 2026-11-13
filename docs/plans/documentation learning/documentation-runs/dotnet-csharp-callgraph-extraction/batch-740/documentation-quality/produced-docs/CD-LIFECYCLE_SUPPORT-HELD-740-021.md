# Documentation: Startup.AddElasticsearch

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Startup.cs
- Symbol: Startup.AddElasticsearch
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (configuration["elasticsearch:username"] != null) { var credentials = new BasicAuthenticationCredentials(configuration["elasticsearch:username"], configuration["elasticsearch:password"]); var pool = new CloudConnectionPool(configuration["elasticsearch:cloudId"], credentials); var client = new ElasticClient(new ConnectionSettings(pool)); services.AddSingleton(client); } else { var url = configuration["elasticsearch:url"]; var defaultIndex = configuration["elasticsearch:index"]; var settings = new ConnectionSettings(new Uri(url)); var client = new ElasticClient(settings); services.AddSingleton(client); } //CreateIndex(client, defaultIndex); }

## Delegated Callee Behavior
- Startup.CreateIndex: Accepted callee meaning for Startup.CreateIndex: { var createIndexResponse = client.Indices.Create(indexName, index => index.Map<object>(x => x.AutoMap()) ); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
