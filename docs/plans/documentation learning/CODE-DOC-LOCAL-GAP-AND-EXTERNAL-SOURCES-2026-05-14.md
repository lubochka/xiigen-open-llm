# Code Documentation Gap Analysis And External Source Plan
# Date: 2026-05-14

## Scope

This note analyzes why the current local validation projects contribute unevenly
to bottom-up documentation learning, and which commercially usable open sources
should be added as support or training authority.

Local validation projects:

- `C:\Projects\wizGameserver`
- `C:\Projects\GenieDynamicServer`
- `C:\Projects\geniesoftGameMVPServer`
- `C:\Projects\genietasks`
- `C:\Projects\askapediaweb`

## Local Findings

| Project | Methods | Edges | Current coverage | Gap |
|---|---:|---:|---|---|
| `wizGameserver` | 85 | 73 | Good server-side ASP.NET Core controller/helper call chains. | Needs more framework-lifecycle contrast rows so `Startup`/middleware is not treated like ordinary business code. |
| `GenieDynamicServer` | 134 | 140 | Good dynamic-object, Elastic/NEST, job processing, and service/controller chains. | Needs external NEST/EF/data-access documentation support if we want richer semantic docs beyond source-local summaries. |
| `genietasks` | 61 | 64 | Good .NET Framework legacy MVC/Web API style validation. | Needs more ASP.NET 4.x/Web API framework authority for routing/action/filter lifecycle vocabulary. |
| `geniesoftGameMVPServer` | 4 | 0 | Only leaf/source/version validation. | This is mostly the default ASP.NET Core Web API template: `Program.Main`, `Startup.ConfigureServices`, `Startup.Configure`, and `WeatherForecastController.Get`. It has almost no project-defined internal call graph. |
| `askapediaweb` | 156 | 146 | Strong legacy .NET Framework validation across v3.5/v4.5.2/v4.6.1 with MVC/Web API/WebForms/Data projects. | Needs version-aware external support so v3.5/v4.5.2/v4.6.1 are not collapsed into one generic NET Framework bucket. |

## What Specifically Fails For `geniesoftGameMVPServer`

The project is structurally small:

- Only a minimal ASP.NET Core 3.1 Web API template is present.
- `WeatherForecastController.Get` has no calls to project-defined helpers or
  services.
- `Startup.ConfigureServices` only calls framework extension methods such as
  `services.AddControllers()`.
- `Startup.Configure` only calls framework middleware registration methods such
  as `UseRouting`, `UseAuthorization`, and `MapControllers`.
- `Program.Main` calls `CreateHostBuilder(args).Build().Run()`, but the current
  extractor does not capture expression-bodied methods such as
  `CreateHostBuilder`.
- `WeatherForecast.TemperatureF` is an expression-bodied property, not a
  method, and is currently outside the method extractor.

The result is expected:

- method-level source/version checks are valid;
- bottom-up caller/callee checks are weak because there are no meaningful local
  callees;
- project documentation requires framework-lifecycle knowledge rather than only
  local callgraph propagation.

## Required Learning Additions

### D1 - Framework Lifecycle Support Rows

Purpose:
Teach the model that ASP.NET Core template projects can still be documented
correctly even when the local call graph is shallow.

Needed topics:

- `Program.Main` and host builder startup flow
- `Startup.ConfigureServices` as dependency/service registration
- `Startup.Configure` as middleware pipeline ordering
- controller action routing via `[ApiController]`, `[Route]`, `[HttpGet]`
- LINQ/object-initializer generated response behavior

These rows should be marked as:

- `source_role`: `framework_lifecycle_support`
- `source_authority`: external documentation plus local source evidence
- `documentation_level`: `D3_FRAMEWORK_ENTRYPOINT` or `D4_FRAMEWORK_PIPELINE`

### D2 - Expression-Bodied Member Extraction

Purpose:
Cover C# constructs visible in small templates.

Needed extraction additions:

- expression-bodied methods: `CreateHostBuilder(...) => ...`
- expression-bodied properties: `TemperatureF => ...`
- constructors: `Startup(...)`, `WeatherForecastController(...)`
- object initializer semantics inside LINQ projections

These should not pretend to create rich business callgraph edges. They should
create local semantic units that can be documented and then referenced from
caller or endpoint documentation.

### D3 - ASP.NET 4.x Version Split

Purpose:
Prevent legacy projects from being flattened into one `NET_FRAMEWORK` label.

Needed rows:

- `v3.5` WebForms/System.Web samples
- `v4.5.2` class-library/data/entity rows
- `v4.6.1` MVC/Web API rows
- MVC 5.2.x versus ASP.NET Core MVC differences
- Web API 2 route/action selection versus ASP.NET Core endpoint routing

## External Sources Found

### ACCEPT_SUPPORT - `dotnet/AspNetCore.Docs`

Use for:

- ASP.NET Core 3.1 startup/middleware/controller concepts
- framework lifecycle support rows for `geniesoftGameMVPServer`

License:

- Repository reports `CC-BY-4.0` documentation and `MIT` code licenses.
- GitHub lines observed: repository navigation lists `CC-BY-4.0 license` and
  `MIT license`; repository license section says `CC-BY-4.0, MIT licenses
  found`.

Source:

- https://github.com/dotnet/AspNetCore.Docs
- https://raw.githubusercontent.com/dotnet/AspNetCore.Docs/main/LICENSE

Use rule:

- Documentation text can be support/authority with attribution.
- Code snippets can be used only when covered by `LICENSE-CODE` and exact file
  proof is captured.

### ACCEPT_SUPPORT - `dotnet/AspNetDocs`

Use for:

- ASP.NET 4.x MVC/Web API/WebForms documentation
- `genietasks` and `askapediaweb` framework-lifecycle vocabulary

License:

- Repository states it contains conceptual ASP.NET 4.x documentation.
- Repository license section says `CC-BY-4.0, MIT licenses found`.

Source:

- https://github.com/dotnet/AspNetDocs
- https://raw.githubusercontent.com/dotnet/AspNetDocs/main/LICENSE

Use rule:

- Good for documentation support pairs, not direct code-authority pairs unless
  the specific snippet/code file is captured under `LICENSE-CODE`.

### ACCEPT_CODE - `dotnet-architecture/eShopModernizing`

Use for:

- Rich legacy MVC/WebForms call chains
- examples where local projects like `geniesoftGameMVPServer` are too small
- controller/service/repository documentation examples

License:

- Repository license section says `MIT license`.
- Repository describes sample legacy ASP.NET WebForms/MVC and WCF/WinForms apps.

Source:

- https://github.com/dotnet-architecture/eShopModernizing

Use rule:

- Can supply clean code-authority rows after exact commit SHA and license file
  are captured.

### ACCEPT_CODE - `aspnet/AspNetWebStack`

Use for:

- ASP.NET MVC 5.x, Web API 2.x, Web Pages 3.x framework source
- action selection, routing, controller pipeline, filters
- legacy framework internals for `askapediaweb` and `genietasks`

License:

- Raw `LICENSE.txt` says Apache License, Version 2.0.
- Repository README states it is for ASP.NET MVC 5.x, Web API 2.x, and Web
  Pages 3.x.

Source:

- https://github.com/aspnet/AspNetWebStack
- https://raw.githubusercontent.com/aspnet/AspNetWebStack/main/LICENSE.txt

Use rule:

- Can supply code-authority rows for framework behavior after exact commit SHA
  and file path are captured.

### ACCEPT_CODE - `aspnet/AspNetWebHooks`

Use for:

- ASP.NET 4.x Web API/MVC integration examples
- webhook receiver/sender pipeline examples
- route/filter/request body processing patterns

License:

- Repository navigation lists `Apache-2.0 license`.
- Repository describes ASP.NET 4.x and .NET Framework 4.5 WebHooks libraries.

Source:

- https://github.com/aspnet/AspNetWebHooks

Use rule:

- Can supply code-authority rows after exact tag/commit and license proof.
- Mark as deprecated/maintenance-mode context; do not teach it as modern best
  practice.

### ACCEPT_SUPPORT - `dotnet/docs`

Use for:

- general .NET and C# version/language documentation
- C# language/version support rows

License:

- Repository license section says `CC-BY-4.0, MIT licenses found`.

Source:

- https://github.com/dotnet/docs

Use rule:

- Use for support and explanation rows with attribution.

### ACCEPT_SUPPORT - `dotnet/entityframework.docs`

Use for:

- EF6/EF Core conceptual documentation
- data-layer documentation vocabulary for `askapediaweb`, `GenieDynamicServer`,
  and any future EF-heavy legacy project

License:

- Repository license section says `CC-BY-4.0, MIT licenses found`.

Source:

- https://github.com/dotnet/entityframework.docs

Use rule:

- Use for support rows; exact code snippets require `LICENSE-CODE` proof.

### ACCEPT_SUPPORT - `dotnet/dotnet-api-docs`

Use for:

- API reference grounding for framework methods such as `IHostBuilder`,
  `IApplicationBuilder`, `ControllerBase`, LINQ, and `IEnumerable<T>`.

License:

- Raw `LICENSE` is Creative Commons Attribution 4.0 International.

Source:

- https://github.com/dotnet/dotnet-api-docs
- https://raw.githubusercontent.com/dotnet/dotnet-api-docs/main/LICENSE

Use rule:

- Use as API-reference support rows with attribution.

## Immediate Data Plan

1. Add a `framework_lifecycle_support` stream for `geniesoftGameMVPServer`.
2. Add source registry entries for the sources above with exact commit/tag
   capture before ingestion.
3. Create first 50-pair batch from `dotnet/AspNetCore.Docs` + local
   `geniesoftGameMVPServer` source:
   - positive: explain `Program`/`Startup`/controller endpoint as framework
     lifecycle documentation, not missing business callgraph;
   - negative: reject pretending a template project has rich internal domain
     calls.
4. Add extractor support for expression-bodied methods/properties and
   constructors.
5. Add legacy framework batches from `dotnet/AspNetDocs`,
   `aspnet/AspNetWebStack`, and `eShopModernizing` to strengthen
   `askapediaweb` and `genietasks` documentation.

## Implementation Update - 2026-05-14

The `geniesoftGameMVPServer` gap is now covered in the local validation stream.

Added support surface:

- `Program.Main`
- `Program.CreateHostBuilder`
- `Startup.Constructor`
- `Startup.ConfigureServices`
- `Startup.Configure`
- `WeatherForecastController.Constructor`
- `WeatherForecastController.Get`
- `WeatherForecast.TemperatureF.get`

Added lifecycle edges:

- `Program.Main` -> `Program.CreateHostBuilder`
- `Program.CreateHostBuilder` -> `Startup.Constructor`
- `Program.CreateHostBuilder` -> `Startup.ConfigureServices`
- `Program.CreateHostBuilder` -> `Startup.Configure`
- `Startup.ConfigureServices` -> `WeatherForecastController.Constructor`
- `Startup.Configure` -> `WeatherForecastController.Get`
- `WeatherForecastController.Get` -> `WeatherForecast.TemperatureF.get`

The documentation flow now classifies these rows as
`framework_lifecycle_root` and emits a `framework-lifecycle-summary` stage.

Verification:

- batches 706-720 ran after the fix, giving 750 clean lifecycle-support train
  rows and 750 clean held-out rows;
- batch 720 includes 10 `geniesoftGameMVPServer` training rows and 10 held-out
  rows;
- batch 720 includes 5 train and 4 held-out `framework_lifecycle_surface`
  rows, plus 4 train and 4 held-out `thin_template_surface` rows;
- runner metrics for batch 720: routing/source/version/transit accuracy all
  `1.0`;
- quality gate for batch 720: `EXCELLENT`, documentation_quality_score `1.0`,
  hard_failure_count `0`;
- stream state after batch 720: 36,000 train rows and 36,000 held-out rows in
  the callgraph stream, plus the earlier 1,000 adapter rows.

## External Source Proof Update - 2026-05-14

The first external source-proof stream is now running.

Stream:

- `external-dotnet-framework-lifecycle-source-proof`

Sources with exact commit proof:

- `dotnet/AspNetCore.Docs`
  - commit: `acba8a58d2c2d36bf058ea955dbae9ae04bc2195`
  - license: MIT code samples via `LICENSE-CODE`
  - role: framework lifecycle support
- `dotnet-architecture/eShopModernizing`
  - commit: `63bc9ec4414d7e281dc9f9d7bdcf70030950457d`
  - license: MIT via `LICENSE`
  - role: clean code authority
- `aspnet/AspNetWebStack`
  - commit: `af67fe18a182e1013482a7a088bae523a555c25c`
  - license: Apache-2.0 via `LICENSE.txt`
  - role: clean code authority

Artifacts:

- `EXTERNAL-SOURCE-PROOF.jsonl`
- `source-inventory.jsonl`
- 50 train rows and 50 held-out rows per batch
- runner metrics with CPU/RAM/time

Verification:

- batches 001-005 accepted;
- external stream totals: 250 train rows + 250 held-out rows;
- extracted external source units: 115;
- source proof rows per batch: 17;
- latest batch 005 metrics: routing/source/version/transit accuracy all `1.0`,
  bottom-up/callee/local-vs-delegated accuracy `0.9`, CPU `437.5 ms`, peak RAM
  `36.625 MB`.

## Acceptance Gate

A batch can be accepted only if:

- 50 train + 50 held-out rows are present.
- source/license proof is attached.
- generated documentation includes stack/version facts.
- source evidence is cited.
- framework lifecycle docs do not invent business domain behavior.
- D4 caller docs still require full callee summary coverage unless the row is
  explicitly classified as `framework_lifecycle_support`.
