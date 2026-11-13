# Documentation: Global.Application_Start

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: net46
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\genietasks
- File: A1ProjectManagement\Global.asax.cs
- Symbol: Global.Application_Start
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ WebApiConfig.Register(GlobalConfiguration.Configuration); GlobalConfiguration.Configuration.EnsureInitialized(); GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = Newtonsoft.Json.PreserveReferencesHandling.None; }

## Delegated Callee Behavior
- LoginDetails.Register: Accepted callee meaning for LoginDetails.Register: { using (a1projmanageEntities lshdb = new a1projmanageEntities()) { try { Member authUser = lshdb.Members.FirstOrDefault(u => u.UserName == user.user && u.CompanyName == user.compa
- WebApiConfig.Register: Accepted callee meaning for WebApiConfig.Register: { // Web API configuration and services config.Filters.Add(new AuthorizeAttribute()); // Web API routes config.EnableCors(); config.MapHttpAttributeRoutes(); config.Routes.MapHttpR

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
