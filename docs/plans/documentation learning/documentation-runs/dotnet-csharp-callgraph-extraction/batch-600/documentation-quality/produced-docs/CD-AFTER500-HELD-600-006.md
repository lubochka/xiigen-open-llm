# Documentation: Startup.Configure

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\GenieDynamicServer
- File: TestXLS\Startup.cs
- Symbol: Startup.Configure
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ if (env.IsDevelopment()) { app.UseDeveloperExceptionPage(); } app.UseRouting(); app.UseAuthentication(); app.UseAuthorization(); app.UseCors("CorsPolicy"); app.UseEndpoints(e => { e.MapControllers(); e.MapHub<PushNotificationHub>("/push"); }); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
