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

## Delegated Callee Behavior
- geniesoftGameMVPServer::Startup.Constructor: Accepted callee meaning for geniesoftGameMVPServer::Startup.Constructor: public Startup(IConfiguration configuration) { Configuration = configuration; }
- geniesoftGameMVPServer::Startup.ConfigureServices: Accepted callee meaning for geniesoftGameMVPServer::Startup.ConfigureServices: public void ConfigureServices(IServiceCollection services) { services.AddControllers(); }
- geniesoftGameMVPServer::Startup.Configure: Accepted callee meaning for geniesoftGameMVPServer::Startup.Configure: public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); } app.UseHttpsRedirection(); app.UseRouting();

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
