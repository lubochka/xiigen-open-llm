# Documentation: geniesoftGameMVPServer::Startup.Configure

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\geniesoftGameMVPServer
- File: geniesoftGameMVPServer\geniesoftGameMVPServer\Startup.cs
- Symbol: geniesoftGameMVPServer::Startup.Configure
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
public void Configure(IApplicationBuilder app, IWebHostEnvironment env) { if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); } app.UseHttpsRedirection(); app.UseRouting(); app.UseAuthorization(); app.UseEndpoints(endpoints => { endpoints.MapControllers(); }); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
