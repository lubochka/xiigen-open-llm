# Documentation: geniesoftGameMVPServer::Program.Main

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\geniesoftGameMVPServer
- File: geniesoftGameMVPServer\geniesoftGameMVPServer\Program.cs
- Symbol: geniesoftGameMVPServer::Program.Main
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
public static void Main(string[] args) { CreateHostBuilder(args).Build().Run(); }

## Delegated Callee Behavior
- geniesoftGameMVPServer::Program.CreateHostBuilder: Accepted callee meaning for geniesoftGameMVPServer::Program.CreateHostBuilder: public static IHostBuilder CreateHostBuilder(string[] args) => Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
