# Code Documentation Stack-Version And Call-Transit Gate
# Version: 1.0 - 2026-05-13
# Domain: Legacy .NET / C# documentation routing and bottom-up propagation

---

## Purpose

This guide closes the gap between broad "document this project" routing and the
actual documentation behavior needed for legacy C# systems.

The documentation flow must route by:

- server/client side
- .NET runtime family
- target framework version
- inferred C# language version
- web/UI framework and version when present
- artifact kind
- documentation level D1-D4
- bottom-up call transit state

It is not enough to choose "C# server documentation." A .NET Framework 4.6
WebApi2 controller with AngularJS 1.4 client code must not be documented as an
ASP.NET Core 3.1 controller, and an ASP.NET Core 3.1 controller must not be
documented as a WebForms page.

---

## Required Version-Aware Routing Fields

Every source row, request, DPO pair, held-out row, and report must include:

```json
{
  "app_side": "server | client | shared | mixed",
  "runtime_family": "NET_FRAMEWORK | NET_CORE | NET_STANDARD | UNKNOWN",
  "target_framework": "net46 | netcoreapp3.1 | netstandard2.0 | ...",
  "target_framework_source": "csproj | packages.config | project.assets.json | inferred",
  "csharp_language_version": "CSharp5 | CSharp6 | CSharp8 | explicit:<value> | inferred_unknown",
  "language_version_confidence": "exact | inferred_high | inferred_review_required",
  "project_style": "legacy_non_sdk | sdk_style | mixed",
  "web_framework": "WebForms | WebApi2 | ASP.NET Core MVC | ASP.NET Core WebApi | ASMX | none | unknown",
  "web_framework_version": "System.Web.Http 5.2.3 | Microsoft.AspNetCore.App 3.1 | ...",
  "frontend_framework": "AngularJS | React | WPF | WinForms | none | unknown",
  "frontend_framework_version": "1.4.8 | ...",
  "stack_label": "NETFX_WEBAPI | NETCORE_WEBAPI | DATA_ACCESS_EF | ...",
  "artifact_kind": "method | controller_action | helper_method | repository_method | page_event | workflow",
  "documentation_level": "D1 | D2 | D3 | D4"
}
```

If a value is inferred, the evidence must name the file and field that produced
the inference.

---

## Version Inference Rules

### SDK-Style .NET Core

If a project file uses:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
<TargetFramework>netcoreapp3.1</TargetFramework>
```

then route as:

- `project_style`: `sdk_style`
- `runtime_family`: `NET_CORE`
- `target_framework`: `netcoreapp3.1`
- `csharp_language_version`: `CSharp8` unless `<LangVersion>` overrides it
- `language_version_confidence`: `inferred_high`
- `web_framework`: `ASP.NET Core WebApi` or `ASP.NET Core MVC` based on
  controllers/routing evidence

### Legacy Non-SDK .NET Framework

If a project file uses:

```xml
<Project ToolsVersion="12.0" ...>
<TargetFrameworkVersion>v4.6</TargetFrameworkVersion>
```

then route as:

- `project_style`: `legacy_non_sdk`
- `runtime_family`: `NET_FRAMEWORK`
- `target_framework`: `net46`
- `csharp_language_version`: `CSharp5_or_CSharp6_review_required` unless an
  explicit compiler package or `LangVersion` proves otherwise
- `language_version_confidence`: `inferred_review_required`
- `web_framework`: derived from `System.Web`, `System.Web.Http`,
  `.aspx/.asmx`, `Global.asax`, and controller base classes

### Frontend Version Evidence

If `package.json` or `packages.config` contains Angular 1.x packages, record:

- `app_side`: `mixed` if the same project contains server and client assets
- `frontend_framework`: `AngularJS`
- `frontend_framework_version`: exact package version, for example `1.4.8`

The documentation route must not collapse AngularJS 1.x into modern Angular.

---

## Bottom-Up Call-Transit Gate

Documentation must be built like a compiler pass.

### Stage T1 - Leaf Method Summary

Document methods whose behavior is locally visible and does not require a known
callee summary. The output becomes an accepted `CalleeSummary`.

### Stage T2 - Helper Or Component Summary

Document a helper or component only after the methods it calls are documented or
included in the A-side. The output must separate:

- local control flow
- delegated behavior
- parse/validation/error decisions inherited from callees

### Stage T3 - Caller / Entry-Point Summary

Document controller actions, ASMX endpoints, page events, and workflow entry
points only after all important helper summaries are available. The caller may
claim callee behavior only by citing the accepted `CalleeSummary`.

### Stage T4 - Workflow / Module Summary

Document the user-visible workflow only after the caller chain is documented.
The workflow summary must preserve data flow, side effects, error paths, and
framework lifecycle constraints.

### Stage T-F - Framework Lifecycle Support

Small customer projects may have almost no project-defined business call graph.
ASP.NET Core template-like projects are still valid documentation targets, but
their transit evidence is the framework lifecycle:

- `Program.Main` -> host builder -> `Startup`
- `Startup.ConfigureServices` -> service/controller activation
- `Startup.Configure` -> middleware and endpoint routing
- endpoint dispatch -> controller action
- response serialization -> expression-bodied properties or model getters

Rows for this case must be marked with:

- `artifact_kind`: `framework_lifecycle_surface`
- `source_root_role`: `framework_lifecycle_root`
- `documentation_level`: usually `D4` for host/startup/endpoint propagation

They must not invent product/domain workflow. If the only code is the default
weather forecast template, the correct documentation is framework lifecycle plus
visible response construction, not a fictional game-server domain.

---

## Hard Fail Conditions

A row, pair, or held-out answer fails if it:

- documents a caller before its required callee summaries exist
- treats a callee behavior as if it were local caller behavior
- omits a required callee summary from the A-side
- claims framework behavior from the wrong runtime family
- collapses `.NET Framework 4.6 + WebApi2 + AngularJS 1.x` into ASP.NET Core
- collapses `netcoreapp3.1` into modern .NET 8/9 behavior
- names a C# language feature that the inferred language version does not
  support
- writes module/workflow documentation before D1-D3 evidence exists

---

## Acceptance Metrics

Every local validation run must report:

```json
{
  "version_routing_accuracy": 0.0,
  "framework_lifecycle_accuracy": 0.0,
  "bottom_up_order_accuracy": 0.0,
  "callee_summary_transit_accuracy": 0.0,
  "local_vs_delegated_accuracy": 0.0,
  "invented_behavior_count": 0,
  "missing_callee_summary_count": 0,
  "wrong_version_count": 0
}
```

These are structural/evaluation metrics until a real DPO runner and
`CodeDocumentationRoutingFlow` exist. They must not be reported as model
training accuracy.

---

## Local Legacy Acceptance Targets

Use the local projects as acceptance gates only, not clean training authority,
unless their exact licenses are separately approved.

### wizGameserver

Expected route:

- `app_side`: `server`
- `runtime_family`: `NET_CORE`
- `target_framework`: `netcoreapp3.1`
- `csharp_language_version`: `CSharp8`
- `project_style`: `sdk_style`
- `web_framework`: `ASP.NET Core WebApi`
- `stack_label`: `NETCORE_WEBAPI`

Required call-transit scenario:

`ParseSimpleObject` -> `ParseJpropertyObject` -> `ParseObjectAlternative` ->
`StoreDocument`.

### GenieDynamicServer

Expected route:

- `app_side`: `server`
- `runtime_family`: `NET_CORE`
- `target_framework`: `netcoreapp3.1`
- `csharp_language_version`: `CSharp8`
- `project_style`: `sdk_style`
- `web_framework`: `ASP.NET Core WebApi`
- `stack_label`: `NETCORE_WEBAPI` plus `LIBRARY_INFRASTRUCTURE`

Required call-transit scenario:

`ParseSimpleObject` -> `ParseJpropertyObject` -> `ParseObjectAlternative` and
`IndexAttribute` -> `ElasticBaseService<T>` index selection -> CRUD methods.

### geniesoftGameMVPServer

Expected route:

- `app_side`: `server`
- `runtime_family`: `NET_CORE`
- `target_framework`: `netcoreapp3.1`
- `csharp_language_version`: `CSharp8`
- `project_style`: `sdk_style`
- `web_framework`: `ASP.NET Core WebApi`
- `stack_label`: `NETCORE_WEBAPI`

Required behavior:

If the project is mostly template or thin API surface, route either to
low-complexity D1/D2 source documentation or to framework-lifecycle D4
documentation. The D4 case is valid only when accepted support summaries cover
host builder, `Startup.ConfigureServices`, `Startup.Configure`, controller
activation, endpoint routing, and response serialization. Do not invent game
workflow documentation from the template code.

### genietasks

Expected route:

- `app_side`: `mixed`
- `runtime_family`: `NET_FRAMEWORK`
- `target_framework`: `net46`
- `csharp_language_version`: `CSharp5_or_CSharp6_review_required`
- `project_style`: `legacy_non_sdk`
- `web_framework`: `WebApi2` plus `ASMX` where endpoint evidence exists
- `frontend_framework`: `AngularJS`
- `frontend_framework_version`: `1.4.8`
- `stack_label`: `NETFX_WEBAPI`

Required call-transit scenario:

`ConvertToMD5` -> `Authenticate` / `Register` -> `CreateBackUser` ->
`UserBack(Member)` -> `JwtManager.GenerateToken`.

---

## Runtime Contract Extension

The runtime request must carry version and transit fields:

```csharp
public sealed record CodeDocumentationRequest(
    string SourceId,
    string Repository,
    string CommitSha,
    string FilePath,
    string SymbolId,
    string CodeText,
    IReadOnlyList<CallEdge> Calls,
    IReadOnlyList<CalleeSummary> KnownCallees,
    string AppSide,
    string RuntimeFamily,
    string TargetFramework,
    string CSharpLanguageVersion,
    string LanguageVersionConfidence,
    string ProjectStyle,
    string WebFramework,
    string WebFrameworkVersion,
    string FrontendFramework,
    string FrontendFrameworkVersion,
    string StackLabel,
    string ArtifactKind,
    string DocumentationLevel);
```

If implementation keeps the old request shape temporarily, the executor must
write these values in the pair coverage block and report that runtime support is
missing.
