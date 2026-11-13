# Codex DCM Platform Endpoint Location Discovery

## CURRENT TASK / NOT THE TASK / EVIDENCE-ONLY

CURRENT TASK: Worker 4 Platform Check 17 blocker resolution for DesignConversion server endpoint placement in `C:\Projects\xiigen-platform-dot-net-angular`, updating this generated document artifact only.

NOT THE TASK: Do not implement endpoint/product code. Do not edit plans. Do not run git, branch, commit, push. Do not run product tests/builds. Do not touch unrelated files.

EVIDENCE-ONLY: Prior packets are evidence only. The active task is to resolve the concrete fields reported by Worker 2 into existing paths or concrete future `to_create` paths.

## Source Map Of Inspected Platform Files And Directories

Inspected source/directories:

- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion server\codex-dcm-platform-endpoint-location.md`: existing Worker 2 artifact read before update.
- `C:\Projects\xiigen-platform-dot-net-angular` root listing: found `src`, `tests`, `docs`, `scripts`, `tools`, `.xiigen`, and `Xiigen.Platform.slnx`.
- `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`: inspected targeted route/header/auth/OpenAPI/idempotency terms.
- `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Xiigen.Platform.Api.csproj`: inspected API project references and package list.
- `C:\Projects\xiigen-platform-dot-net-angular\tests`: inspected test project names.
- `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Marketplace.Tests\Xiigen.Platform.Marketplace.Tests.csproj`: inspected current xUnit test project convention.
- `C:\Projects\xiigen-platform-dot-net-angular\src` and `tests`: targeted searches for tier/quota/subscription/entitlement/role/budget/rate-limit/idempotency primitives.
- `C:\Projects\xiigen-platform-dot-net-angular\docs`: inspected top-level docs shape and targeted OpenAPI/API-doc terms.
- `C:\Projects\llm_mvp_core\docs\plans\DCM-DESIGN-CONVERSION-SERVER-PLAN-v1-codex.md`: inspected only Check 17 and DCM public route/header/doc/test requirements.

Targeted search results:

- `DesignConversion`, `Design Conversion`, `design conversion`, `conversion/submit`, and `conversion/jobs`: no existing platform DesignConversion endpoint implementation was found in the inspected platform repo state.
- `OpenAPI`, `OpenApi`, `Swagger`, `Swashbuckle`, `NSwag`, `AddEndpointsApiExplorer`, `WithOpenApi`: no existing OpenAPI/Swagger wiring was found in scoped platform source/tests/docs searches.
- Tier/quota/subscription/entitlement/rate-limit terms: only existing role primitives were found in `Program.cs` and `TenantContext.cs`; no current DCM tier/quota/subscription enforcement primitive was found.
- Idempotency terms: kernel/fabric primitives and tests exist, but no existing HTTP route reads or enforces `X-Idempotency-Key`.

## Required Check 17 Fields

`endpoint_file_path`: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`

Evidence: existing API endpoint ownership is minimal API route mapping in `Program.cs`. Targeted route search found `MapGet`/`MapPost` routes at lines 50-405 and `app.Run()` after the route table.

`routing_or_module_registration_path`: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`

Evidence: route registration and host startup are in the same minimal API file. Service registration is in the same API project host file, and the current API project file has no MVC controller/OpenAPI package wiring.

`auth_tenant_tier_middleware_path`: concrete existing and `to_create` paths:

- Existing tenant/correlation header path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`, lines 21-46.
- Existing role/auth guard path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`, lines 672-686.
- Existing tenant context model path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Kernel\TenantContext.cs`.
- Existing idempotency contracts path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Kernel\FabricContracts.cs`.
- Existing idempotency implementation path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Fabrics.InMemory\InMemoryIdempotencyStore.cs`.
- Existing idempotency DI registration path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Fabrics.InMemory\XiigenInMemoryFabricServiceCollectionExtensions.cs`.
- Tier validation `to_create` helper path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionTierPolicy.cs`.
- Tier/header DTO handling `to_create` path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionEndpointModels.cs`.
- Route-level tier integration path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`, inside the future `POST /conversion/submit` route handler before FLOW-47 delegation.

Why the future tier paths match repo conventions: the platform API is currently a minimal API host, so route integration belongs in `Program.cs`; feature-specific DCM DTO/policy files should live under the API project because the tier headers and public DTOs are public endpoint boundary concerns, not Marketplace service concerns. Existing role/tenant primitives are already consumed at the API-host layer.

`api_docs_or_openapi_path`: `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\docs\api\dcm-conversion-submit.md`

Why this future path matches repo conventions: the platform repo already has a top-level `docs` directory, and the active DCM plan explicitly names `docs/api/dcm-conversion-submit.md` as the public API contract document for `POST /conversion/submit` and `GET /conversion/jobs/{jobId}`. No current OpenAPI/Swagger/NSwag wiring exists, so a Markdown API contract under `docs\api` is the concrete docs path to create before or with endpoint implementation.

`tests_path`: concrete existing pattern plus `to_create` public endpoint tests:

- Existing test root: `C:\Projects\xiigen-platform-dot-net-angular\tests`.
- Existing xUnit project convention: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Marketplace.Tests\Xiigen.Platform.Marketplace.Tests.csproj`.
- Public endpoint test project `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj`.
- Public endpoint test file `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs`.

Why the future test path matches repo conventions: existing tests are grouped as `tests\Xiigen.<Project>.Tests` xUnit projects with matching `.csproj` files. The missing public HTTP surface belongs to `Xiigen.Platform.Api`, so the concrete future integration/contract test project should be `Xiigen.Platform.Api.Tests`.

## Exact Recommended Endpoint Location

Recommended endpoint host file:

`C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`

Recommended route placement:

- Add `POST /conversion/submit` and `GET /conversion/jobs/{jobId}` as minimal API routes in `Program.cs`, near the other public API route blocks and before the HTML page routes that start at `app.MapGet("/marketplace", ...)`.
- Controller/module placement is not applicable in the current code shape because the inspected API project uses minimal API route mapping in `Program.cs`, not MVC controllers.

Recommended DCM API support files to create:

- `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionEndpointModels.cs` for public request/response/error DTOs and header binding helpers.
- `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionTierPolicy.cs` for `X-Design-Tier` and `X-Loop-Budget` validation.
- `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionIdempotency.cs` for HTTP `X-Idempotency-Key` reserve/replay/conflict helper logic around `IIdempotencyStore`.

Recommended canonical public routes from the active DCM plan:

- `POST /conversion/submit`
- `GET /conversion/jobs/{jobId}`

## Auth / Tenant / Tier / Idempotency Patterns Found

Auth/role pattern:

- `Program.cs` lines 672-686 defines `RequireStudioRole(HttpContext context)`.
- It requires `X-Roles` to exist, returns unauthorized when missing, and permits `platform_admin`, `tenant_admin`, or `tenant_ops`.
- Existing protected route blocks call this helper before service execution, for example studio/wiki/assistant route blocks at lines 142, 158, 177, 195, 213, 232, 249, 265, 281, 297, 313, 331, 347, 363, 379, and 395.

Tenant/correlation pattern:

- `Program.cs` lines 21-46 reads `X-Correlation-Id`, `X-Tenant-Id`, `X-Roles`, and `X-User-Id`, pushes `CorrelationContext` and `TenantContext`, and writes response `X-Correlation-Id`.
- `TenantContext.cs` defines tenant/user/roles/correlation snapshot records, `ITenantContextAccessor`, `AsyncLocalTenantContextAccessor`, and `ITenantContextRunner`.

Tier pattern:

- Existing role primitive paths are concrete: `Program.cs` and `TenantContext.cs`.
- No existing tier/quota/subscription/entitlement/rate-limit primitive was found in scoped `src`/`tests` searches.
- Concrete future tier helper path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionTierPolicy.cs`.
- Concrete future DTO/header handling path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionEndpointModels.cs`.
- Required headers to handle there and enforce in the `Program.cs` route: `X-Design-Tier`, `X-Loop-Budget`, plus tenant/user/correlation/idempotency headers named by the DCM plan.

Idempotency pattern:

- `FabricContracts.cs` defines `IdempotencyReservation` and `IIdempotencyStore`.
- `InMemoryIdempotencyStore.cs` implements reserve/succeeded/failed records, replaying a succeeded reservation from cache.
- `XiigenInMemoryFabricServiceCollectionExtensions.cs` registers `IIdempotencyStore`.
- Existing fabric tests cover idempotency replay in `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs` and scheduler idempotency in `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Fabrics.Scheduler.Tests\SchedulerTests.cs`.
- No existing HTTP `X-Idempotency-Key` enforcement path was found.
- Concrete future route-level idempotency helper path: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionIdempotency.cs`.
- Concrete future route integration location: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`, inside the future `POST /conversion/submit` handler before FLOW-47 delegation and around the accepted-response writeback to `IIdempotencyStore`.

## OpenAPI / Docs / Test Project Locations Found

OpenAPI/API docs:

- Existing OpenAPI/Swagger/NSwag/Scalar/AddEndpointsApiExplorer path: none found in scoped searches.
- Existing docs root: `C:\Projects\xiigen-platform-dot-net-angular\docs`.
- Public DCM API docs `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\docs\api\dcm-conversion-submit.md`.

Test projects found:

- Auth tests: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Auth.Tests\Xiigen.Auth.Tests.csproj`
- Fabric/in-memory tests: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Fabrics.InMemory.Tests\Xiigen.Fabrics.InMemory.Tests.csproj`
- Scheduler tests: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Fabrics.Scheduler.Tests\Xiigen.Fabrics.Scheduler.Tests.csproj`
- Freedom tests: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Freedom.Tests\Xiigen.Freedom.Tests.csproj`
- Kernel tests: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Kernel.Tests\Xiigen.Kernel.Tests.csproj`
- Marketplace/service tests: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Marketplace.Tests\Xiigen.Platform.Marketplace.Tests.csproj`

Public platform API endpoint test location:

- Existing public API endpoint test project: none found in the inspected test project list.
- Public API endpoint test project `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj`.
- Public API endpoint test file `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs`.

## Required Future Code Changes

Future implementation should:

- Add public minimal API routes in `Program.cs` for `POST /conversion/submit` and `GET /conversion/jobs/{jobId}`.
- Add public DTOs for the DCM submission payload, accepted response, job status response, result body, and platform error envelope in `src\Xiigen.Platform.Api\DesignConversion\DesignConversionEndpointModels.cs`.
- Add or register a platform-facing service that validates the public request and delegates only to FLOW-47; platform code must not call the llm_mvp_core internal model API directly.
- Reuse or extend the existing role guard so DCM routes require a platform/session authorization path, not anonymous access.
- Reuse existing tenant/correlation context headers and require the DCM-specific headers named by the plan: `X-Tenant-Id`, `X-User-Id`, `X-Idempotency-Key`, `X-Correlation-Id` or `X-Request-Id`, `If-Match`, `X-Design-Tier`, and `X-Loop-Budget`.
- Add route-level idempotency handling in `src\Xiigen.Platform.Api\DesignConversion\DesignConversionIdempotency.cs` using `IIdempotencyStore`, including replay/conflict behavior for `X-Idempotency-Key`.
- Add concrete tier/budget policy code in `src\Xiigen.Platform.Api\DesignConversion\DesignConversionTierPolicy.cs` for `X-Design-Tier` and `X-Loop-Budget`.
- Add API docs at `docs\api\dcm-conversion-submit.md`.
- Add public API endpoint tests in `tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs` that cover accepted submission, job status polling, validation failures, 401/403, 404, idempotency replay/conflict, tier/budget denial, FLOW-47 unavailable mapping, and proof that platform does not call the internal model API directly.

## Gate Result

Gate result: PASS

Reason: Required Check 17 endpoint/routing/auth/docs/tests/tier/idempotency locations are now concrete as either existing platform paths or explicit `to_create` paths:

- Endpoint/routing host: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\Program.cs`.
- Auth/tenant existing paths: `Program.cs` and `TenantContext.cs`.
- Existing idempotency primitive paths: `FabricContracts.cs`, `InMemoryIdempotencyStore.cs`, and `XiigenInMemoryFabricServiceCollectionExtensions.cs`.
- Route-level idempotency `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionIdempotency.cs`, integrated from `Program.cs`.
- Tier validation `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\src\Xiigen.Platform.Api\DesignConversion\DesignConversionTierPolicy.cs`, with DTO/header handling in `DesignConversionEndpointModels.cs`.
- API docs `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\docs\api\dcm-conversion-submit.md`.
- Public endpoint tests `to_create`: `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\Xiigen.Platform.Api.Tests.csproj` and `C:\Projects\xiigen-platform-dot-net-angular\tests\Xiigen.Platform.Api.Tests\DesignConversionEndpointTests.cs`.

The evidence supports only: Platform Check 17 location discovery is concrete enough to unblock later implementation planning/execution. It does not prove endpoint implementation, runtime behavior, product tests, OpenAPI generation, or FLOW-47 integration, because those were forbidden in this task.

## RAG Registration Status

RAG registration status: NOT REGISTERED / NO SAME-FOLDER CONVENTION FOUND.

The requested target directory is `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion server`. I did not find or update an obvious same-folder RAG index/registry convention in this task. I did not invent a RAG index.

## Verification Notes

- Updated only this document artifact: `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion server\codex-dcm-platform-endpoint-location.md`.
- Required gate fields now use concrete existing paths or concrete `to_create` paths.
- Did not edit product code.
- Did not edit plans.
- Did not run product tests/builds.
- Did not run git commands.
