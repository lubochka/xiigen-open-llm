# Code Documentation External Source Registry
# Version: 1.0 - 2026-05-11
# Domain: Legacy .NET / C# code documentation

---

## Purpose

This registry defines which open-source repositories may be used to build
training and held-out evaluation pairs for bottom-up legacy .NET documentation.

Clean training is restricted to sources with an exact MIT or Apache-2.0 license
at the selected commit or tag.

Do not trust a repository name, blog post, package listing, or old memory of the
license. The executor must capture license proof for the exact commit/tag used.

---

## Required License Proof Per Source Slice

Every source slice must record:

- `source_id`
- `repository_url`
- `commit_sha` or immutable release tag
- `file_path`
- `symbol_id`
- `line_span` when available
- `license_spdx`
- `license_file_path`
- `license_proof_excerpt`
- `extracted_at`
- `source_role`
- `allowed_training_use`
- `app_side`
- `runtime_family`
- `target_framework`
- `target_framework_source`
- `csharp_language_version`
- `language_version_confidence`
- `project_style`
- `web_framework`
- `web_framework_version`
- `frontend_framework`
- `frontend_framework_version`

Allowed `license_spdx` for clean training:

- `MIT`
- `Apache-2.0`

All other licenses are blocked from clean training unless a separate legal
decision explicitly creates a quarantine stream.

---

## No Synthetic Source Authority

Do not create source ids that merely look like real repository rows.

Invalid examples:

- `aspnet-webhooks-method-0001` without repository commit, file path, and symbol
- `legacy-mvc-example-001` when the code was invented by the executor
- `dapper-like-row-0042` when the row is not traceable to a Dapper commit

Valid source locator shape:

```json
{
  "source_id": "github-aspnet-AspNetWebHooks-v1.2.2",
  "repository_url": "https://github.com/aspnet/AspNetWebHooks",
  "commit_sha": "<exact sha or immutable tag>",
  "license_spdx": "Apache-2.0",
  "file_path": "src/...",
  "symbol_id": "Namespace.Type.Method(...)",
  "line_span": "L10-L42",
  "source_role": "chosen_authority"
}
```

Synthetic snippets may be used only as `method_only` or `prompt_shape_only`
support rows. They cannot become chosen-side documentation authority.

---

## Accepted Candidate Families

These repositories are candidates only after exact commit/tag license proof is
captured.

### Family CSHARP-DOC-SUPPORT-01 - ASP.NET Core Lifecycle Documentation

Purpose:
Support thin ASP.NET Core projects where the important documentation meaning is
the framework lifecycle rather than a rich local business call graph.

Accepted support candidates after exact commit and license proof:

- `dotnet/AspNetCore.Docs` - CC-BY-4.0 documentation plus MIT code samples;
  use for host builder, `Startup.ConfigureServices`, middleware ordering,
  endpoint routing, controller activation, and controller action concepts.
- `dotnet/dotnet-api-docs` - CC-BY-4.0 API reference; use for API-level
  grounding such as `IHostBuilder`, `IApplicationBuilder`, `ControllerBase`,
  LINQ, and `IEnumerable<T>`.
- `dotnet/docs` - CC-BY-4.0 documentation plus MIT code samples; use for
  language/runtime/version support only.

Allowed use:

- `support_only` and `framework_lifecycle_support`
- attribution-bearing explanation pairs
- not direct chosen code authority unless the specific source file is covered
  by an MIT code license and exact file proof is captured

Required row marker:

- `artifact_kind`: `framework_lifecycle_surface` or `thin_template_surface`
- `source_role`: `framework_lifecycle_support`
- `wrong_route_consequence`: must mention hallucinating business workflow from
  framework/template code

### Family CSHARP-DOC-SUPPORT-02 - ASP.NET 4.x Framework Lifecycle Documentation

Purpose:
Support legacy `.NET Framework` projects where MVC5, Web API 2, WebForms,
Global.asax, Web.config, filters, handlers, and route tables must not be
collapsed into ASP.NET Core concepts.

Accepted support/code candidates after exact commit and license proof:

- `dotnet/AspNetDocs` - CC-BY-4.0 documentation plus MIT code samples; use for
  ASP.NET 4.x conceptual support.
- `aspnet/AspNetWebStack` - Apache-2.0 framework source; use for MVC5/Web API2
  routing, action selection, controller/filter pipeline, and Web Pages 3.x
  framework behavior.
- `aspnet/AspNetWebHooks` - Apache-2.0 ASP.NET 4.x integration source; use for
  Web API/MVC integration examples and request-processing flow.

Allowed use:

- `support_only` for conceptual docs
- `chosen_authority` for Apache-2.0 source code files after exact commit,
  license proof, file path, and symbol span are recorded

Required version split:

- `v3.5` / `net35` rows must not be merged with `v4.5.2` or `v4.6.1`
- MVC5/Web API2 rows must not be described as ASP.NET Core endpoint routing

### Family CSHARP-LEGACY-01 - Microsoft Legacy Modernization References

Purpose:
Legacy ASP.NET / WebForms / MVC reference applications and modernization
patterns.

Candidate repositories:

- `dotnet-architecture/eShopModernizing`
- `aspnet/AspNetWebHooks`
- `microsoft/dotnet-samples`

Expected patterns:

- WebForms page lifecycle
- MVC controllers and views
- Global.asax and Web.config behavior
- HttpHandlers, HttpModules, filters, route config
- IIS-hosted legacy application boundaries

Allowed use after license proof:

- chosen-side code documentation examples
- call-tree propagation examples
- held-out evaluation code

### Family CSHARP-LEGACY-02 - CMS / Portal Legacy Applications

Purpose:
Large real-world WebForms or MVC codebases with modules, providers, content
types, controllers, handlers, background tasks, and plugin architecture.

Candidate repositories:

- `dnnsoftware/Dnn.Platform` - expected MIT, verify exact tag
- `umbraco/Umbraco-CMS` legacy v7/v8 tags - expected MIT, verify exact tag

Review-required candidates:

- `OrchardCMS/OrchardProject` - useful architecture, but not MIT/Apache-only
  until exact selected tag proves allowed license
- `i7MEDIA/mojoportal` - review required until exact license is verified

Blocked for clean MIT/Apache stream:

- `BlogEngine/BlogEngine.NET` if selected tag is Microsoft Public License
- `Orckestra/C1-CMS-Foundation` if selected tag is LGPL

### Family CSHARP-LEGACY-03 - E-Commerce And Domain Layers

Purpose:
Repository/service/plugin patterns, order/customer/catalog workflows, and rich
business-layer call chains.

Accepted candidates after exact license proof:

- `SimplCommerce/SimplCommerce` - expected Apache-2.0, verify exact tag
- `dotnet-architecture/eShopOnWeb` - useful as modern MVC reference, verify
  license and mark as support or contrast if not legacy enough

Review or block:

- `nopSolutions/nopCommerce` - do not ingest until exact tag license is
  verified; many tags are not clean MIT/Apache training authority
- `VirtoCommerce/vc-platform` - block from clean MIT/Apache stream unless exact
  open-source license is approved for training

### Family CSHARP-LEGACY-04 - Infrastructure Libraries

Purpose:
Small-to-medium method-level and class-level examples with stable APIs.

Accepted candidates after exact license proof:

- `AutoMapper/AutoMapper` - expected MIT
- `FluentValidation/FluentValidation` - expected Apache-2.0
- `castleproject/Windsor` - expected Apache-2.0
- `structuremap/structuremap` - expected Apache-2.0
- `DapperLib/Dapper` - expected Apache-2.0
- `SignalR/SignalR` classic - expected Apache-2.0

Review or block:

- `NLog/NLog` - useful but commonly BSD; block from this MIT/Apache-only clean
  stream unless policy expands accepted licenses

---

## Legacy Stack Labels

Every extracted row must include one stack label:

- `NETFX_WEBFORMS`
- `NETFX_MVC`
- `NETFX_WEBAPI`
- `NETFX_HTTPMODULE`
- `NETFX_WINFORMS`
- `NETFX_WPF`
- `NETCORE_MVC`
- `NETCORE_WEBAPI`
- `LIBRARY_INFRASTRUCTURE`
- `DATA_ACCESS_ADO`
- `DATA_ACCESS_EF`
- `UNKNOWN_REVIEW_REQUIRED`

Rows with `UNKNOWN_REVIEW_REQUIRED` cannot enter train-ready.

---

## Version-Aware Routing Fields

Stack labels are not enough. Every row must also record exact platform evidence.

Required examples:

- SDK-style `.NET Core 3.1` Web API:
  - `app_side`: `server`
  - `runtime_family`: `NET_CORE`
  - `target_framework`: `netcoreapp3.1`
  - `target_framework_source`: `csproj`
  - `csharp_language_version`: `CSharp8`
  - `language_version_confidence`: `inferred_high`
  - `project_style`: `sdk_style`
  - `web_framework`: `ASP.NET Core WebApi`
- Legacy `.NET Framework 4.6` WebApi2 / ASMX / AngularJS:
  - `app_side`: `mixed`
  - `runtime_family`: `NET_FRAMEWORK`
  - `target_framework`: `net46`
  - `target_framework_source`: `csproj`
  - `csharp_language_version`: `CSharp5_or_CSharp6_review_required`
  - `language_version_confidence`: `inferred_review_required`
  - `project_style`: `legacy_non_sdk`
  - `web_framework`: `WebApi2` or `ASMX` based on endpoint evidence
  - `frontend_framework`: `AngularJS`
  - `frontend_framework_version`: exact `package.json` or package value

If the project has no explicit `<LangVersion>`, the executor must mark the
language version as inferred rather than exact.

---

## Source Roles

- `chosen_authority` - code can be used to form chosen documentation
- `support_only` - code can teach routing or contrast, not chosen doc text
- `held_out_only` - code can test generalization, not train
- `review_required` - no training until model-led or human license review
- `blocked` - do not ingest

---

## Blocked Source Rules

Block from clean training:

- GPL, LGPL, AGPL
- CC-BY-SA unless a separate share-alike quarantine stream is created
- MS-PL unless legal policy explicitly accepts it
- custom commercial or unclear open-source licenses
- GitHub repositories with no license file at selected commit
- copied snippets from blogs, Stack Overflow, or company docs

---

## First Source Batch Recommendation

Start with a conservative batch:

- 20 method-level rows from `aspnet/AspNetWebHooks`
- 15 method/class rows from `DapperLib/Dapper`
- 10 legacy WebForms/MVC rows from `eShopModernizing`
- 5 routing rows from `DNN Platform` or `Umbraco v7/v8`, only after license
  proof is captured

This gives the first run a mix of small functions, library methods, and real
legacy application context without overloading the model with full workflows.
