# Documentation: askapediaweb::AskapediaApp.Application_Start

## Stack And Version
- Runtime family: NET_FRAMEWORK
- Target framework: v4.6.1
- C# language version: CSharp5
- Web framework: ASP.NET MVC/WebForms 4.x
- Project style: legacy_non_sdk

## Source Evidence
- Repository: C:\Projects\askapediaweb
- File: askapedia\Askapedia\Global.asax.cs
- Symbol: askapediaweb::AskapediaApp.Application_Start
- Source id: local-callgraph-extraction-validation-only

## Local Behavior
{ GlobalConfiguration.Configure(WebApiConfig.RegisterRoutes); AutoMapperConfig.Initialize(); AskapediaDBContext.Initialize(); }

## Delegated Callee Behavior
- askapediaweb::AutoMapperConfig.Initialize: Accepted callee meaning for askapediaweb::AutoMapperConfig.Initialize: { var cfg = new MapperConfiguration(x => { x.AddProfile(new QuestionProfile()); x.AddProfile(new AnswerProfile()); x.AddProfile(new CampaignProfile()); x.AddProfile(new QuestionLea
- askapediaweb::AskapediaDBContext.Initialize: Accepted callee meaning for askapediaweb::AskapediaDBContext.Initialize: { Database.SetInitializer<AskapediaDBContext>(null); }

## Caller Meaning
This caller summary is valid only after the listed callee summaries are propagated upward. Local behavior and delegated behavior must remain separated.

## Maintenance Notes
- Do not infer behavior outside the shown source evidence.
- Preserve stack/version constraints before recommending changes.
- If a callee summary is missing, block caller-level documentation until it is available.
