# Documentation: geniesoftGameMVPServer::Program.CreateHostBuilder

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\geniesoftGameMVPServer
- File: geniesoftGameMVPServer\geniesoftGameMVPServer\Program.cs
- Symbol: geniesoftGameMVPServer::Program.CreateHostBuilder
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
