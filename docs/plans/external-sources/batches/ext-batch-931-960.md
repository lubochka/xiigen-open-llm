# External Sources Batch 32 — EXT-631 through EXT-650

---

## EXT-631 | KUBERNETES-KEPS | Family 8 | Domain G

**Unit title:** Kubernetes HPA (Horizontal Pod Autoscaler) custom metrics vs CPU-based scaling learning
**Source URL:** https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT631-F8-G",
  "sourceIds": ["EXT-631"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "autoscaling-metric-selection-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A message queue consumer deployment scales on CPU utilization (HPA target: 70% CPU). Under load, the queue depth grows to 50,000 messages while CPU stays at 40% (processing is I/O bound — waiting on downstream API). The team needs to scale on queue depth instead.",
    "b": "HPA v2 supports custom metrics via metrics.k8s.io and external metrics via external.metrics.k8s.io. KEDA (Kubernetes Event-Driven Autoscaling) provides ScaledObject with built-in scalers for RabbitMQ, Azure Service Bus, Kafka, and 50+ other sources. CPU-based HPA is appropriate for CPU-bound workloads; queue-depth scaling is appropriate for I/O-bound queue consumers.",
    "c": "KEDA ScaledObject scales to zero (no messages = 0 replicas) which CPU-based HPA cannot do (minimum replicas must be ≥ 1 for CPU metrics to be collected)."
  },
  "function": "Record autoscaling metric selection for queue consumer scaling learning",
  "acceptedOutput": "Replace CPU-based HPA with KEDA ScaledObject targeting queue depth: spec.triggers[0].type = azure-servicebus (or rabbitmq), queueLength = 100 (scale one replica per 100 messages). KEDA scales from 0 to maxReplicaCount based on actual work backlog. Scale-to-zero during idle periods reduces infrastructure cost.",
  "rejectedOutput": "Keep CPU-based HPA but lower the CPU target to 20% to over-provision replicas and indirectly keep queue depth manageable.",
  "arbiters": ["learning-arbiter", "autoscaling-metric-arbiter"],
  "whyAccepted": "Queue depth is the correct scaling signal for a queue consumer — it directly measures work backlog. KEDA provides a production-ready scaler for this pattern with scale-to-zero, min/max replica configuration, and cooldown periods.",
  "whyRejected": "Lowering the CPU target to 20% causes over-provisioning during low load, wasting resources. It also fails when processing speed changes (faster downstream API → lower CPU → fewer replicas when more are needed). The metric remains wrong regardless of threshold tuning.",
  "sourceAnchors": ["https://keda.sh/docs/2.13/concepts/scaling-deployments/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-632 | BIGCODEBENCH | Family 5 | Domain E

**Unit title:** Python secrets module vs random module for cryptographic token generation arbitration
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT632-F5-E",
  "sourceIds": ["EXT-632"],
  "sourceTrustTier": 2,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "csprng-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "A Python application generates password reset tokens using random.token_hex(32). A security audit flags this as using a non-cryptographically-secure PRNG (Pseudo-Random Number Generator) for a security-sensitive token.",
    "b": "Python's random module uses the Mersenne Twister algorithm — a deterministic PRNG seeded from system time. Given enough outputs, an attacker can reconstruct the PRNG state and predict future tokens. secrets module uses os.urandom() which reads from the OS CSPRNG (Cryptographically Secure PRNG — /dev/urandom on Linux, CryptGenRandom on Windows).",
    "c": "secrets.token_hex(32) generates 32 bytes of CSPRNG output encoded as 64 hex characters. secrets.token_urlsafe(32) generates URL-safe base64-encoded tokens. Both are the Python 3.6+ standard for security tokens."
  },
  "function": "Arbitrate PRNG selection for security-sensitive token generation",
  "acceptedOutput": "Replace random.token_hex(32) with secrets.token_hex(32). The secrets module is explicitly designed for security-sensitive applications: password reset tokens, session tokens, CSRF tokens, and API keys. No configuration required — secrets.token_hex() uses the OS CSPRNG automatically.",
  "rejectedOutput": "Seed random with os.urandom(16) to add entropy: random.seed(os.urandom(16)); token = random.token_hex(32).",
  "arbiters": ["security-arbiter", "csprng-token-arbiter"],
  "whyAccepted": "secrets.token_hex() is CSPRNG-backed by the OS. No prediction attack is feasible. It is the single correct answer for security token generation in Python — no alternatives needed.",
  "whyRejected": "Re-seeding random with os.urandom does not convert it to a CSPRNG. After seeding, the Mersenne Twister output is still predictable from the seed. An attacker who observes enough outputs can recover the seed and predict all future tokens.",
  "sourceAnchors": ["https://docs.python.org/3/library/secrets.html"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-633 | SWE-BENCH | Family 1 | Domain A

**Unit title:** Django deployment topology — Gunicorn+Nginx vs uvicorn+ASGI for async Django views
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT633-F1-A",
  "sourceIds": ["EXT-633"],
  "sourceTrustTier": 2,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "wsgi-asgi-deployment-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Django 4.2 application uses async views (async def) for WebSocket and long-polling endpoints alongside traditional sync views. The team uses Gunicorn with synchronous workers (WSGI). Async views work but run synchronously because WSGI does not support async I/O.",
    "b": "Django 4.x supports ASGI (Asynchronous Server Gateway Interface) via uvicorn or daphne. ASGI enables true async I/O in async views, Django Channels WebSocket support, and async ORM queries. WSGI (Gunicorn) wraps async views in synchronous execution — async functionality works but without the I/O concurrency benefit.",
    "c": "Gunicorn supports uvicorn workers (gunicorn -k uvicorn.workers.UvicornWorker) as a hybrid deployment — Gunicorn manages workers, uvicorn provides ASGI within each worker. Pure uvicorn is simpler. Nginx reverse proxy handles TLS, static files, and connection limiting for both."
  },
  "function": "Select WSGI vs ASGI deployment topology for async Django application",
  "acceptedOutput": "Deploy with Gunicorn + UvicornWorker: gunicorn myapp.asgi:application -k uvicorn.workers.UvicornWorker -w 4. This gives uvicorn's ASGI support within Gunicorn's process management (graceful reload, worker monitoring). Django async views get true async I/O. Nginx proxies to Gunicorn as before.",
  "rejectedOutput": "Keep Gunicorn with sync workers. Django's async views run correctly under WSGI — the async functionality is available even without ASGI.",
  "arbiters": ["topology-arbiter", "deployment-server-arbiter"],
  "whyAccepted": "ASGI with UvicornWorker enables async Django views to perform concurrent I/O without blocking OS threads. A single uvicorn worker can handle hundreds of concurrent WebSocket connections. Under WSGI, each concurrent WebSocket connection requires a separate worker thread/process.",
  "whyRejected": "WSGI wraps async views in a synchronous executor (asyncio.run()). The async view runs but occupies a Gunicorn worker thread for its entire duration, including I/O waits. Concurrent WebSocket connections require as many Gunicorn workers as concurrent connections — defeating the purpose of async.",
  "sourceAnchors": ["https://docs.djangoproject.com/en/4.2/howto/deployment/asgi/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-634 | PYTHON-PEPS | Family 5 | Domain E

**Unit title:** PEP-678 exception notes — enriching exceptions with contextual information for error arbitration
**Source URL:** https://peps.python.org/pep-0678/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT634-F5-E",
  "sourceIds": ["EXT-634"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "exception-enrichment-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A data pipeline catches exceptions from multiple processing stages and needs to add context (file name, record number, pipeline stage) to exceptions before re-raising. The team uses exception chaining (raise ProcessingError('stage 2 failed') from original_error) but loses the specific record context.",
    "b": "PEP-678 (Python 3.11+) adds exception.__notes__: a list of strings appended to the exception display. e.add_note('Processing record #4521 from file orders_2024.csv') adds context without creating a new exception or losing the original traceback. Multiple notes can be added from different catch blocks as the exception propagates.",
    "c": "Exception chaining (from e) preserves the original exception as __cause__ but requires creating a new exception type. __notes__ adds context to the existing exception without wrapping."
  },
  "function": "Arbitrate exception enrichment strategy for pipeline error context",
  "acceptedOutput": "Use exception notes for context enrichment: except ValueError as e: e.add_note(f'Failed at record #{record_num} in {filename}'); e.add_note(f'Pipeline stage: {stage_name}'); raise. The notes appear in the traceback output and are accessible via e.__notes__. Chain (raise ... from e) only when creating a conceptually different exception type.",
  "rejectedOutput": "Catch and re-raise with a new exception type at each stage: raise PipelineStageError(f'Stage {stage} failed at record {record_num}') from original.",
  "arbiters": ["error-arbitration-arbiter", "exception-enrichment-arbiter"],
  "whyAccepted": "Exception notes add context without losing the original exception type or creating a chain of wrapped exceptions. Error handling code (except ValueError) continues to work — the notes are additive. The full context (all notes + original traceback) appears in the traceback output.",
  "whyRejected": "Creating new exception types at each pipeline stage creates deep exception chains that obscure the root cause. Callers catching ValueError must now also catch PipelineStageError. Exception chains of 4+ levels are difficult to read in production logs.",
  "sourceAnchors": ["https://peps.python.org/pep-0678/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-635 | OPENTELEMETRY-SPEC | Family 3 | Domain D

**Unit title:** OTel semantic conventions for HTTP spans — correct attribute names for request/response execution
**Source URL:** https://opentelemetry.io/docs/specs/semconv/http/http-spans/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT635-F3-D",
  "sourceIds": ["EXT-635"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "http-span-attribute-execution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A team manually instruments HTTP spans with custom attribute names: http.path, http.status, http.method. The tracing backend's service topology view fails to display correct HTTP metrics because it expects OTel semantic convention attribute names.",
    "b": "OTel HTTP semantic conventions (v1.23+) define stable attribute names: http.request.method (not http.method), http.response.status_code (not http.status), url.path (not http.path), server.address, server.port. Backends like Jaeger, Grafana Tempo, and Honeycomb use these specific attribute names for auto-generated dashboards.",
    "c": "Old stable conventions (http.method, http.status_code, http.url) were deprecated in OTel semconv 1.21. New stable conventions use the namespace-prefixed form. The OTel .NET and Java auto-instrumentation SDKs emit the new conventions."
  },
  "function": "Apply OTel HTTP semantic convention attribute names for correct span execution",
  "acceptedOutput": "Replace custom attribute names with OTel HTTP semantic conventions: span.SetAttribute(\"http.request.method\", \"GET\"); span.SetAttribute(\"http.response.status_code\", 200); span.SetAttribute(\"url.path\", \"/api/orders\"); span.SetAttribute(\"server.address\", \"api.example.com\"). Use the OTelHttpTagHelper or auto-instrumentation packages to avoid manual attribute naming.",
  "rejectedOutput": "Keep custom attribute names. The team's internal dashboard is configured to use the custom names; changing them would break existing dashboards.",
  "arbiters": ["execution-applier-arbiter", "semantic-convention-compliance-arbiter"],
  "whyAccepted": "Semantic convention compliance enables OTel-aware backend auto-dashboards (Grafana Tempo, Honeycomb, Datadog) to automatically generate RED (Rate, Error, Duration) metrics without custom queries. Custom attribute names require manual dashboard configuration for every backend.",
  "whyRejected": "Migrating to semantic conventions updates, not breaks, existing dashboards — the attribute names change but the data continues to flow. The one-time dashboard migration cost is offset by compatibility with all OTel-aware backends without custom configuration.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/semconv/http/http-spans/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-636 | DAPPER | Family 8 | Domain G

**Unit title:** Dapper performance benchmarking — micro-benchmark vs realistic workload profiling for data access
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT636-F8-G",
  "sourceIds": ["EXT-636"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "orm-performance-profiling-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team benchmarks Dapper vs EF Core using BenchmarkDotNet micro-benchmarks: Dapper is 3× faster than EF Core for a single-row query. Based on this, they propose replacing all EF Core code with Dapper.",
    "b": "Micro-benchmarks measure raw mapping speed without network latency, query compilation, or result set size. In production, database network latency (1-10ms) dominates a 0.1ms difference in ORM mapping time. EF Core compiled queries eliminate query compilation overhead. Realistic workloads have different characteristics from micro-benchmarks.",
    "c": "EF Core advantages: LINQ type safety, migration management, change tracking, and compiled queries approach Dapper performance for repeated queries. Dapper advantages: explicit SQL control, no change tracking overhead for read-only queries, and better for complex raw SQL scenarios."
  },
  "function": "Record ORM performance evaluation methodology for realistic workload profiling learning",
  "acceptedOutput": "Benchmark Dapper vs EF Core under realistic conditions: actual database (not SQLite), production query complexity, connection pooling, concurrent load, and representative result set sizes. Profile the application under production-like load with Application Insights or OTel metrics to identify actual bottlenecks before committing to ORM changes.",
  "rejectedOutput": "Replace EF Core with Dapper based on the 3× micro-benchmark advantage. Faster ORM = faster application.",
  "arbiters": ["learning-arbiter", "performance-profiling-arbiter"],
  "whyAccepted": "Micro-benchmark advantages rarely translate to application-level improvements when database latency dominates. Profile production-representative workloads, identify actual hot paths, and apply Dapper selectively to those queries rather than blanket replacement of type-safe EF Core infrastructure.",
  "whyRejected": "ORM selection based on micro-benchmarks is a common performance anti-pattern. A 3× faster mapping time for a 0.1ms operation saves 0.07ms when the query itself takes 5ms. The engineering cost of replacing type-safe EF Core with manual Dapper SQL across the application exceeds the performance benefit in most cases.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-637 | ANGULAR-DOCS | Family 5 | Domain E

**Unit title:** Angular 17 route guards — CanActivateFn vs class-based CanActivate for auth arbitration
**Source URL:** https://angular.dev/guide/routing/route-guards
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT637-F5-E",
  "sourceIds": ["EXT-637"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "route-guard-auth-arbitration",
  "rowClass": "migration-decision",
  "input": {
    "a": "An Angular application uses class-based CanActivate route guards: @Injectable() class AuthGuard implements CanActivate { canActivate(): boolean { return this.authService.isLoggedIn(); } }. Angular 14.2+ deprecated class-based guards in favor of functional guards.",
    "b": "Functional guards (CanActivateFn) are plain functions: export const authGuard: CanActivateFn = (route, state) => inject(AuthService).isLoggedIn(). They use inject() for dependencies instead of constructor injection, eliminating the need for @Injectable() class boilerplate. Functional guards are simpler to compose and test.",
    "c": "Angular 14.2 deprecated CanActivate, CanDeactivate, Resolve interfaces. Angular 17 emits deprecation warnings. Class-based guards continue to work but are scheduled for removal in a future major version."
  },
  "function": "Migrate class-based CanActivate guard to functional CanActivateFn for auth arbitration",
  "acceptedOutput": "Replace class-based guard with functional guard: export const authGuard: CanActivateFn = () => { const authService = inject(AuthService); const router = inject(Router); return authService.isLoggedIn() ? true : router.createUrlTree(['/login']); }. Register in routes: { path: 'dashboard', canActivate: [authGuard] }. No @Injectable() or providers registration needed.",
  "rejectedOutput": "Keep class-based CanActivate guards. They work and the migration adds risk without functional benefit.",
  "arbiters": ["security-arbiter", "angular-migration-arbiter"],
  "whyAccepted": "Functional guards are the Angular 14.2+ canonical pattern. They eliminate class boilerplate, simplify composition (guards can be composed as arrays of functions), and improve testability (plain functions vs class instantiation). Deprecation warnings in Angular 17 indicate future removal.",
  "whyRejected": "Deferring migration of deprecated APIs accumulates technical debt. Angular deprecation cycles typically span 1-2 major versions before removal. Migrating now while deprecation warnings are clear is safer than migrating under removal pressure in a future version.",
  "sourceAnchors": ["https://angular.dev/guide/routing/route-guards"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-638 | REACT-ADVANCED | Family 5 | Domain E

**Unit title:** React Suspense boundary placement — granular vs coarse error and loading arbitration
**Source URL:** https://react.dev/reference/react/Suspense
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT638-F5-E",
  "sourceIds": ["EXT-638"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "suspense-boundary-placement-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React dashboard page has 5 independent data sections (orders, inventory, analytics, notifications, recommendations). A single Suspense boundary wraps the entire page — when any section is loading, the whole page shows a spinner, blocking all ready sections.",
    "b": "Suspense boundaries catch the nearest ancestor's loading state. A single boundary wraps all children — any loading child suspends the whole boundary. Granular boundaries per section allow each section to show its own loading state independently.",
    "c": "Error Boundaries work similarly — a single Error Boundary around all sections means one section's error hides all other sections. Granular Error Boundaries isolate failures to individual sections."
  },
  "function": "Arbitrate Suspense boundary granularity for dashboard loading state",
  "acceptedOutput": "Wrap each dashboard section in its own Suspense boundary: <Suspense fallback={<SectionSkeleton />}><OrdersSection /></Suspense>. Each section loads independently and shows its skeleton while loading. Sections that resolve first display immediately without waiting for slower sections. Add per-section Error Boundaries to isolate failures.",
  "rejectedOutput": "Keep one Suspense boundary for the whole page. Users see a consistent loading state and the page appears complete when all data is ready.",
  "arbiters": ["reactivity-arbiter", "loading-state-arbitration-arbiter"],
  "whyAccepted": "Granular Suspense boundaries allow fast sections (notifications: 100ms) to display before slow sections (analytics: 800ms). Users see progressive content rendering rather than a spinner for the entire 800ms. This directly improves perceived performance.",
  "whyRejected": "A single boundary creates an artificial synchronization point — the fastest section (100ms) waits for the slowest (800ms) before anything appears. 'Consistent loading state' is not a user benefit when it means 700ms of unnecessarily delayed content.",
  "sourceAnchors": ["https://react.dev/reference/react/Suspense"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-639 | ASPNETCORE-DOCS-SAMPLES | Family 1 | Domain A

**Unit title:** ASP.NET Core minimal API endpoint grouping — MapGroup vs individual MapGet for topology
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT639-F1-A",
  "sourceIds": ["EXT-639"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "minimal-api-endpoint-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET 9 minimal API has 20 endpoints across 4 resource types (orders, customers, products, inventory). All order endpoints share a base path (/api/orders), an [Authorize] attribute, and rate limiting policy. The team duplicates these attributes on each of the 5 order endpoints individually.",
    "b": ".NET 7+ RouteGroupBuilder (MapGroup()) allows grouping endpoints with shared prefix, middleware, filters, and metadata. var orders = app.MapGroup(\"/api/orders\").RequireAuthorization().RequireRateLimiting(\"order-policy\"). All endpoints added to orders inherit the group's configuration.",
    "c": "MapGroup() supports nested groups: var adminOrders = orders.MapGroup(\"/admin\").RequireAuthorization(\"AdminPolicy\"). The group prefix and all parent group middleware/filters compose automatically."
  },
  "function": "Apply MapGroup() for minimal API endpoint topology with shared middleware",
  "acceptedOutput": "Use MapGroup() for all resource endpoint groups: var orders = app.MapGroup(\"/api/v1/orders\").RequireAuthorization().WithOpenApi(); orders.MapGet(\"/\", GetOrders); orders.MapGet(\"/{id}\", GetOrder); orders.MapPost(\"/\", CreateOrder). Authorization and OpenAPI metadata apply to all group endpoints automatically.",
  "rejectedOutput": "Apply .RequireAuthorization() to each endpoint individually for explicit clarity on which endpoints require auth.",
  "arbiters": ["topology-arbiter", "endpoint-configuration-arbiter"],
  "whyAccepted": "MapGroup() applies shared configuration once, reducing duplication. Adding a new order endpoint automatically inherits group authorization, rate limiting, and OpenAPI configuration. Missing authorization on one endpoint (due to copy-paste omission) becomes impossible.",
  "whyRejected": "Per-endpoint authorization is 'explicit' but creates 5× the surface area for configuration drift. A developer adding a 6th order endpoint must remember to add RequireAuthorization() — forgetting creates an unauthorized endpoint. MapGroup() makes authorization structural, not optional.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-640 | NESTJS-ADVANCED | Family 8 | Domain G

**Unit title:** NestJS testing strategy — unit tests with mocks vs integration tests with TestingModule
**Source URL:** https://docs.nestjs.com/fundamentals/testing
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT640-F8-G",
  "sourceIds": ["EXT-640"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "nestjs-testing-strategy-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS service has business logic that depends on a TypeORM repository, a mailer service, and a config service. The team writes unit tests mocking all dependencies (jest.fn()) vs integration tests using Test.createTestingModule() with real dependencies.",
    "b": "Unit tests with mocks test business logic in isolation — fast (no I/O), deterministic, but test only the unit's logic assuming dependencies behave as mocked. Integration tests with TestingModule spin up the NestJS DI container with real or test-double dependencies — slower but test actual wiring and interaction.",
    "c": "NestJS Test.createTestingModule() supports overriding specific providers: .overrideProvider(MailerService).useValue(mockMailer). This enables integration-style testing (real DI container) with controlled external dependencies (mocked mailer, in-memory database)."
  },
  "function": "Record NestJS testing strategy selection for business logic learning",
  "acceptedOutput": "Use TestingModule with overridden external providers: Test.createTestingModule({ imports: [TypeOrmModule.forFeature([Order])], providers: [OrderService, { provide: MailerService, useValue: mockMailer }] }).compile(). The DI container is real (actual service wiring), external side effects are controlled (mocked mailer). Use pure unit mocks only for pure functions without DI dependencies.",
  "rejectedOutput": "Use jest.fn() mocks for all dependencies in all tests. Unit tests are faster and more focused than integration tests.",
  "arbiters": ["learning-arbiter", "testing-strategy-arbiter"],
  "whyAccepted": "TestingModule with selective overrides tests real DI wiring (catching misconfigured providers, missing injections) while controlling external side effects. This finds a class of bugs (wrong provider token, circular dependency) that pure unit mocks miss.",
  "whyRejected": "Pure unit mocks test the unit in isolation but cannot catch DI misconfiguration, wrong injection tokens, or NestJS lifecycle hook issues. A service that works in isolation can fail to instantiate in the real container — a category of bugs invisible to mock-only tests.",
  "sourceAnchors": ["https://docs.nestjs.com/fundamentals/testing"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-641 | IETF-RFCS | Family 5 | Domain E

**Unit title:** RFC-9110 content negotiation — Accept header arbitration for multi-format API responses
**Source URL:** https://www.rfc-editor.org/rfc/rfc9110
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT641-F5-E",
  "sourceIds": ["EXT-641"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "content-negotiation-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API serves both JSON (application/json) and CSV (text/csv) for report endpoints. The team implements separate endpoints (/api/reports/json and /api/reports/csv) rather than content negotiation via the Accept header.",
    "b": "RFC-9110 Section 12 defines content negotiation via Accept header: the client sends Accept: text/csv; q=1.0, application/json; q=0.8. The server selects the best available format and responds with Content-Type: text/csv. A single endpoint serves multiple formats based on client preference.",
    "c": "Separate endpoints for format variants are an anti-pattern: format is a representation concern, not a resource concern. /api/reports identifies the resource; format is a media type negotiation detail. Separate URLs break HTTP cacheability (the same logical resource has two URLs)."
  },
  "function": "Arbitrate content negotiation vs separate endpoint URLs for multi-format API",
  "acceptedOutput": "Use content negotiation on a single /api/reports endpoint. Parse the Accept header and respond with the best matching format. In ASP.NET Core: configure OutputFormatters to support application/json and text/csv. Return the same data object; the formatter selects the serialization. Include Vary: Accept in the response for correct CDN caching.",
  "rejectedOutput": "Keep separate /api/reports/json and /api/reports/csv endpoints. Content negotiation is complex to implement and the two-endpoint approach is clearer for API consumers.",
  "arbiters": ["security-arbiter", "api-design-arbitration-arbiter"],
  "whyAccepted": "Content negotiation correctly models the HTTP resource model — one URL per resource, multiple representations via Accept. CDN caches correctly with Vary: Accept. Clients can use q-values to express format preferences. ASP.NET Core's output formatter pipeline makes implementation straightforward.",
  "whyRejected": "Separate URLs for format variants create two cache entries for the same data, complicate API versioning (now N resources × M formats), and break the REST constraint that a URL identifies a resource. 'Clearer for API consumers' is negated by having to document two URLs for the same data.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc9110#section-12"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-642 | GOOGLE-AIP | Family 3 | Domain D

**Unit title:** AIP-159 reading across collections — wildcard resource IDs for cross-collection queries
**Source URL:** https://google.aip.dev/159
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT642-F3-D",
  "sourceIds": ["EXT-642"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "cross-collection-query-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API has resources at /projects/{project}/orders/{order}. A client needs to list all orders across all projects that a user has access to. The team adds a separate /orders endpoint or requires the client to call /projects/{p}/orders for each project.",
    "b": "AIP-159 specifies wildcard resource IDs: /projects/-/orders allows listing orders across all projects. The server resolves '-' as a wildcard, collects orders from all accessible projects, and returns them in a unified list. This is the AIP pattern for cross-collection reads.",
    "c": "Wildcard collection IDs require the server to enforce resource access control for all matched parent resources. The response should include the full resource name (projects/{project}/orders/{order}) so clients know which project each order belongs to."
  },
  "function": "Apply AIP-159 wildcard resource IDs for cross-collection query execution",
  "acceptedOutput": "Implement wildcard support in the orders List handler: if projectId == '-', query across all projects the user has access to. Return full resource names in each order's name field (projects/{actual_project}/orders/{order_id}). Document the wildcard behavior in the API reference.",
  "rejectedOutput": "Add a separate /orders global endpoint. Keep /projects/{project}/orders for project-scoped queries.",
  "arbiters": ["execution-applier-arbiter", "cross-collection-arbiter"],
  "whyAccepted": "AIP-159 wildcard is the standard pattern for cross-collection reads in Google AIP-compliant APIs. It avoids proliferating separate global endpoints for each resource type and keeps the resource hierarchy consistent. Full resource names in responses preserve the parent context.",
  "whyRejected": "A separate /orders global endpoint creates a parallel resource hierarchy. Clients must know two URL patterns for the same resource type. URL proliferation complicates API documentation, versioning, and access control implementation.",
  "sourceAnchors": ["https://google.aip.dev/159"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-643 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF policy collapse — KL divergence penalty calibration for controlled policy drift
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT643-F8-G",
  "sourceIds": ["EXT-643"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "rlhf-policy-collapse-prevention",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "After 3 RLHF training iterations, a language model produces repetitive, high-confidence responses on a narrow set of topics. Reward scores are high but human evaluators rate the responses as less useful than after iteration 1. This is reward model over-optimization (Goodhart's Law in RLHF).",
    "b": "Policy collapse occurs when the RL policy over-optimizes the reward model proxy, finding responses that score highly but deviate significantly from the reference model's distribution. The KL divergence penalty (β × KL(π_rl || π_ref)) controls the tradeoff between reward maximization and staying near the reference distribution.",
    "c": "Symptoms: increasing reward scores but decreasing human evaluation scores, narrowing response diversity, mode collapse to reward-maximizing response patterns. Mitigation: increase β (KL penalty), use iterative DPO (Direct Preference Optimization) instead of PPO, or retrain the reward model on fresh preferences."
  },
  "function": "Detect and mitigate RLHF policy collapse via KL divergence penalty calibration",
  "acceptedOutput": "Increase the KL penalty coefficient β from the current value. Monitor the KL divergence between the RL policy and reference model across training — if KL > 5 nats while reward continues increasing, the policy is over-optimizing. Target KL in the 2-4 nat range for balanced optimization. Add diversity metrics (distinct-n, entropy) to training monitoring alongside reward scores.",
  "rejectedOutput": "Continue training with the current β. High reward scores indicate the model is learning correctly — human evaluators may have different preferences than the reward model.",
  "arbiters": ["learning-quality-arbiter", "rlhf-policy-collapse-arbiter"],
  "whyAccepted": "Diverging reward vs human evaluation scores is the canonical signal of reward model over-optimization. Increasing β constrains the policy to stay near the reference distribution, preventing mode collapse. The KL-reward Pareto frontier should be monitored across training iterations.",
  "whyRejected": "Attributing the human evaluation decline to 'different preferences' ignores the systematic divergence signal. When reward scores increase but human scores decrease consistently across evaluation cycles, the reward model is being exploited, not learned from. Continuing training deepens the collapse.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-644 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba domain-specific translation quality — technical vs colloquial sentence stratification
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT644-F2-C",
  "sourceIds": ["EXT-644"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "domain-stratified-translation-quality",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A translation model fine-tuned on Tatoeba EN-DE (German) performs well on colloquial sentences but poorly on technical documentation translation. The team assumes more training data will fix both domains.",
    "b": "Tatoeba is predominantly a colloquial/everyday language dataset — sentences are community-contributed conversational examples. Technical documentation translation requires domain-specific vocabulary, sentence structure, and terminology that Tatoeba underrepresents. Fine-tuning on colloquial-dominated data can harm technical domain performance.",
    "c": "Domain stratification: categorize sentences by technical vocabulary density (technical terms per sentence). Train on domain-balanced datasets or use domain-specific fine-tuning after general fine-tuning. Technical domains often require specialized corpora (software docs, medical texts)."
  },
  "function": "Stratify translation training data by domain for quality-controlled model training",
  "acceptedOutput": "Categorize Tatoeba EN-DE sentences by technical vocabulary density using a domain classifier or terminology dictionary overlap. Fine-tune a general MT model on balanced colloquial + technical data. For technical documentation translation, supplement with domain-specific parallel corpora (OPUS technical subcorpus, software localization data). Evaluate domain-specific BLEU separately for colloquial and technical test sets.",
  "rejectedOutput": "Add more Tatoeba sentences of any type to improve overall translation quality. Volume compensates for domain imbalance.",
  "arbiters": ["translation-quality-arbiter", "domain-stratification-arbiter"],
  "whyAccepted": "Domain stratification reveals that colloquial training data can actively harm technical translation by reinforcing informal sentence structures and missing technical vocabulary. Separate domain evaluation catches this regression. Domain-specific supplemental data addresses the root cause.",
  "whyRejected": "Volume does not compensate for domain imbalance — adding more colloquial data further dilutes technical domain signal. A model trained on 10M colloquial sentences and 100K technical sentences will perform proportionally to the domain ratio, not improve uniformly across domains.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-645 | VUE-DOCS | Family 3 | Domain D

**Unit title:** Vue 3 component state initialization — reactive() vs ref() vs shallowRef() for execution context
**Source URL:** https://vuejs.org/guide/essentials/reactivity-fundamentals
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT645-F3-D",
  "sourceIds": ["EXT-645"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "reactive-primitive-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 component manages a complex form state object with 20 fields. The team uses reactive(formData) for the entire object. Template updates work but a developer replaces the entire formData object (formData = newFormData) and the reactivity breaks.",
    "b": "reactive() makes an object deeply reactive but loses reactivity if the reference is replaced (reactive() wraps the specific object, not the variable). ref() wraps any value (including objects) in a .value container — replacing .value with a new object maintains reactivity. shallowRef() makes only .value reactive, not nested properties.",
    "c": "For form state: reactive() is suitable if the object is never replaced (only mutated). ref() is required if the entire form may be reset to a new object. The Vue docs recommend ref() as the primary reactive primitive for its consistency."
  },
  "function": "Select Vue 3 reactive primitive for form state execution context",
  "acceptedOutput": "Use ref<FormData>(initialFormData) for the form state. Replace the entire form via formData.value = newFormData (full reset) or mutate individual fields via formData.value.email = 'x@x.com'. Template accesses formData.email (Composition API automatically unwraps in templates). Reactivity is preserved through replacement.",
  "rejectedOutput": "Keep reactive(formData). Avoid replacing the object — always mutate individual fields. Implement form reset by mutating each field individually.",
  "arbiters": ["execution-applier-arbiter", "reactive-primitive-arbiter"],
  "whyAccepted": "ref() supports both mutation (formData.value.field = x) and replacement (formData.value = newObject). The reset pattern (formData.value = { ...initialState }) is clean and maintains reactivity. Constraining the team to never replace the object is an artificial restriction that breaks the natural form reset pattern.",
  "whyRejected": "Mutating each field individually for form reset is verbose (20 individual assignments) and error-prone (missing a field leaves stale data). The restriction exists only because reactive() was chosen for its convenience — the tool dictates an awkward usage pattern.",
  "sourceAnchors": ["https://vuejs.org/guide/essentials/reactivity-fundamentals"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-646 | EFCORE-SOURCE | Family 1 | Domain A

**Unit title:** EF Core DbContext lifetime topology — Scoped vs Transient vs Singleton in ASP.NET Core
**Source URL:** https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT646-F1-A",
  "sourceIds": ["EXT-646"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "dbcontext-lifetime-topology",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An ASP.NET Core application registers DbContext as Singleton: services.AddDbContext<AppDbContext>(opts => ..., ServiceLifetime.Singleton). Under concurrent requests, developers observe race conditions in change tracking — one request's tracked entities appear in another request's context.",
    "b": "DbContext is not thread-safe. EF Core DbContext is designed as a Scoped service — one instance per HTTP request. Singleton lifetime shares one DbContext across all concurrent requests, causing change tracking conflicts. Transient creates a new context per injection, which can cause issues with transactions spanning multiple injections.",
    "c": "The default AddDbContext() registers as Scoped. Explicit ServiceLifetime.Singleton is almost always wrong. For background services (IHostedService), use IDbContextFactory<T> to create contexts on demand, since IHostedService is Singleton and cannot depend on Scoped services."
  },
  "function": "Detect and correct DbContext lifetime topology for concurrent-safe request handling",
  "acceptedOutput": "Register DbContext as Scoped (the default): services.AddDbContext<AppDbContext>(opts => opts.UseSqlServer(connStr)). Each HTTP request gets its own DbContext instance with isolated change tracking. For Singleton services needing database access: inject IDbContextFactory<AppDbContext> and create contexts with factory.CreateDbContext() within the operation.",
  "rejectedOutput": "Keep Singleton DbContext but clear the change tracker after each request: dbContext.ChangeTracker.Clear() in middleware.",
  "arbiters": ["topology-arbiter", "dbcontext-lifetime-arbiter"],
  "whyAccepted": "Scoped lifetime is the EF Core design intent. Each request has isolated change tracking, isolated transaction scope, and no cross-request state. The database connection is managed per-request via the connection pool.",
  "whyRejected": "ChangeTracker.Clear() in middleware addresses change tracking state but not thread safety. DbContext uses non-thread-safe internal state (query result cache, compiled query cache, internal identity map) that cannot be made safe by clearing the change tracker. Concurrent calls on one DbContext instance are undefined behavior.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-647 | DOTNET-MICROSERVICES | Family 7 | Domain C

**Unit title:** .NET OpenTelemetry semantic conventions for gRPC spans — service method translation
**Source URL:** https://opentelemetry.io/docs/specs/semconv/rpc/grpc/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT647-F7-C",
  "sourceIds": ["EXT-647"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "grpc-span-semantic-translation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A .NET gRPC service manually instruments spans with custom attribute names: grpc.service_name, grpc.method_name, grpc.response_code. The tracing backend cannot build a gRPC service map because it expects OTel gRPC semantic convention attribute names.",
    "b": "OTel gRPC semantic conventions: rpc.system = 'grpc', rpc.service = 'OrderService' (proto service name), rpc.method = 'CreateOrder' (proto method name), rpc.grpc.status_code = 0 (gRPC status code integer). These are the standard attributes backends use for RPC service topology visualization.",
    "c": "The .NET OTel gRPC instrumentation (OpenTelemetry.Instrumentation.GrpcNetClient, OpenTelemetry.Instrumentation.AspNetCore) emits correct semantic convention attributes automatically when added to the SDK. Manual instrumentation should use the same attribute names."
  },
  "function": "Translate manual gRPC span attributes to OTel semantic convention compliance",
  "acceptedOutput": "Use OTel auto-instrumentation packages: services.AddOpenTelemetry().WithTracing(t => t.AddAspNetCoreInstrumentation().AddGrpcClientInstrumentation()). These emit correct rpc.system, rpc.service, rpc.method, and rpc.grpc.status_code attributes automatically. Remove manual span creation for gRPC calls — the instrumentation package handles it.",
  "rejectedOutput": "Update custom attribute names to match the semantic conventions manually: grpc.service_name → rpc.service, grpc.method_name → rpc.method.",
  "arbiters": ["api-translation-arbiter", "semantic-convention-compliance-arbiter"],
  "whyAccepted": "Auto-instrumentation packages emit correct semantic conventions and handle edge cases (streaming RPCs, error status mapping). Manual attribute renaming is a fragile intermediate step — the packages already solve this correctly.",
  "whyRejected": "Manual renaming achieves the same attribute names but requires maintenance as semantic conventions evolve. Auto-instrumentation packages are updated with each OTel SDK release to track semantic convention changes. Manual instrumentation decouples from the SDK update cycle.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/semconv/rpc/grpc/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-648 | MICROSOFT-LEARN-ARCH | Family 6 | Domain F

**Unit title:** Azure Cosmos DB change feed — event-driven result storage vs polling for downstream processing
**Source URL:** https://learn.microsoft.com/en-us/azure/cosmos-db/change-feed
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT648-F6-F",
  "sourceIds": ["EXT-648"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "event-driven-result-storage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An application writes orders to Cosmos DB. Downstream services (analytics, search indexing, email notifications) need to react to new and updated orders. The team polls Cosmos DB every 30 seconds for changed records using a lastModified timestamp filter.",
    "b": "Cosmos DB change feed provides a persistent, ordered log of inserts and updates per container. Azure Functions with CosmosDbTrigger or the Change Feed Processor library subscribe to changes and process them as they occur. Changes are delivered in near-real-time with at-least-once delivery guarantees.",
    "c": "Polling limitations: 30-second latency, N×M query overhead (N services × M queries per service per interval), missed changes during downtime, and complex lastModified timestamp filtering at scale. Change feed advantages: near-real-time, single change feed per container serves multiple independent consumers (leases), no missed changes."
  },
  "function": "Select event-driven vs polling pattern for Cosmos DB downstream result processing",
  "acceptedOutput": "Use Cosmos DB change feed with the Change Feed Processor library or Azure Functions CosmosDbTrigger. Each downstream service has an independent lease container for its read position. Services process changes in near-real-time. Implement idempotent handlers for at-least-once delivery.",
  "rejectedOutput": "Keep polling with lastModified timestamp. Change feed adds operational complexity and the 30-second latency is acceptable.",
  "arbiters": ["registry-storage-arbiter", "event-driven-processing-arbiter"],
  "whyAccepted": "Change feed eliminates polling overhead, latency, and missed-change complexity. Multiple independent consumers read the same feed via separate lease containers. At-least-once delivery with idempotent handlers is simpler than implementing reliable at-most-once polling.",
  "whyRejected": "Polling with lastModified has fundamental reliability issues: clock skew can miss changes, records updated multiple times within a poll interval may only show the latest state (missed intermediate states), and service downtime during a poll interval requires gap-filling logic. Change feed is ordered and complete.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/cosmos-db/change-feed"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-649 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-611 XML External Entity (XXE) injection — XmlDocument vs XDocument secure parsing arbitration
**Source URL:** https://cwe.mitre.org/data/definitions/611.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT649-F5-E",
  "sourceIds": ["EXT-649"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "xml-injection-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core API parses XML payloads from partners using XmlDocument.Load(requestStream). A security scan detects XXE (XML eXternal Entity) injection vulnerability: a malicious XML payload with <!ENTITY xxe SYSTEM 'file:///etc/passwd'> causes the server to read and return local files.",
    "b": "XmlDocument in .NET Framework resolves external entities by default. In .NET Core 2.0+, XmlDocument uses XmlReaderSettings with DtdProcessing.Prohibit by default, making it safer. However, explicitly setting XmlReaderSettings ensures security regardless of .NET version. XDocument (LINQ to XML) does not support DTD processing and is not vulnerable to XXE.",
    "c": "Safe XML parsing: use XmlReader with DtdProcessing = DtdProcessing.Prohibit and XmlResolver = null. Or use XDocument.Parse() which does not support external entities."
  },
  "function": "Arbitrate XML parser selection for XXE injection prevention",
  "acceptedOutput": "Use XDocument for XML parsing: var doc = XDocument.Parse(xmlString). XDocument does not support DTD or external entity resolution — XXE is structurally impossible. For XmlDocument: var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }; using var reader = XmlReader.Create(stream, settings); doc.Load(reader).",
  "rejectedOutput": "Keep XmlDocument.Load(). Validate the XML payload against a schema before parsing to prevent malicious entities.",
  "arbiters": ["security-arbiter", "xml-injection-arbiter"],
  "whyAccepted": "XDocument's structural impossibility of XXE is the strongest defense. XmlReader with explicit DtdProcessing.Prohibit is the defense for cases requiring XmlDocument. Both are explicit, robust, and do not depend on schema validation catching all XXE variants.",
  "whyRejected": "Schema validation does not prevent XXE — entity declarations can appear in the instance document DTD, not just the schema. An attacker embedding an entity declaration in the XML instance bypasses schema validation while still triggering external entity resolution in a vulnerable parser.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/611.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-650 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V14.4 HTTP security headers — CSP (Content Security Policy) directive arbitration
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT650-F5-E",
  "sourceIds": ["EXT-650"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "csp-directive-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core application sets Content-Security-Policy: default-src *. A security audit flags this as an ineffective CSP that allows loading resources from any origin, providing no XSS mitigation.",
    "b": "OWASP ASVS V14.4.3 requires CSP to prevent XSS via disallowing inline scripts and restricting script sources. A wildcard default-src * allows: inline scripts (no 'unsafe-inline' restriction), arbitrary external scripts, and data: URIs. An effective CSP restricts script-src to known origins and disallows unsafe-inline.",
    "c": "CSP nonce-based approach: server generates a random nonce per request, sets script-src 'nonce-{value}', and adds nonce=\"{value}\" to each <script> tag. Only nonce-matching scripts execute. This allows inline scripts (with nonce) while blocking injected scripts (no nonce)."
  },
  "function": "Arbitrate CSP directive configuration for ASVS V14.4 XSS mitigation",
  "acceptedOutput": "Implement a nonce-based CSP: generate a cryptographically random nonce per request (128-bit, base64 encoded). Set Content-Security-Policy: default-src 'self'; script-src 'nonce-{nonce}' 'strict-dynamic'; style-src 'self' 'nonce-{nonce}'; object-src 'none'; base-uri 'none'. Apply nonce to all script tags. Use middleware to inject the nonce into the response header and template context.",
  "rejectedOutput": "Remove the CSP header entirely rather than using a weak one. A broken CSP gives false security; no CSP is more honest.",
  "arbiters": ["security-arbiter", "csp-directive-arbiter"],
  "whyAccepted": "Nonce-based CSP with 'strict-dynamic' is the modern CSP approach: it allows explicitly nonced scripts and their dynamically loaded dependencies, blocks all other inline and external scripts. It is compatible with modern frameworks and provides meaningful XSS mitigation.",
  "whyRejected": "Removing CSP entirely eliminates defense-in-depth for XSS. A weak CSP (wildcard) should be fixed, not removed. The nonce approach takes 1-2 days to implement correctly and provides browser-enforced XSS mitigation as an additional security layer beyond input sanitization.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
