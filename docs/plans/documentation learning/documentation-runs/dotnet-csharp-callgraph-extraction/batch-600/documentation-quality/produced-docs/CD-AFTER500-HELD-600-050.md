# Documentation: WebApiConfig.Register

## Stack And Version
- Runtime family: NET_CORE
- Target framework: netcoreapp3.1
- C# language version: CSharp8
- Web framework: ASP.NET Core MVC 3.1
- Project style: sdk_style

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\WebApiConfig.cs
- Symbol: WebApiConfig.Register
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ // Web API configuration and services config.Filters.Add(new AuthorizeAttribute()); // Web API routes config.EnableCors(); config.MapHttpAttributeRoutes(); config.Routes.MapHttpRoute( name: "DefaultApi", routeTemplate: "api/{controller}/{id}", defaults: new { id = RouteParameter.Optional } ); }

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
