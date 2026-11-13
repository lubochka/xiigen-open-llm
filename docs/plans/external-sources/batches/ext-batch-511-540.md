# External Sources Batch 18 — EXT-351 through EXT-370

Generated: 2026-05-04  
Ledger range: EXT-351 – EXT-370  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, VUE-DOCS, EFCORE-SOURCE, DOTNET-MICROSERVICES, MICROSOFT-LEARN-ARCH, CWE-TOP25, OWASP-ASVS

---

## EXT-351

```json
{
  "rowId": "EXT-351",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Kubernetes cluster runs a batch processing workload that consumes jobs from a queue. The number of running jobs varies from 0 to 500 throughout the day. Evaluate: (A) Horizontal Pod Autoscaler (HPA) scaling on CPU utilisation. (B) KEDA ScaledObject scaling on queue depth (number of unprocessed jobs). (C) Cluster Autoscaler only with fixed pod count.",
    "b": "Each pod processes one job at a time. Job processing takes 30-120 seconds. Queue latency SLA: all jobs processed within 10 minutes of arrival.",
    "c": "Queue backend: Azure Service Bus. Peak arrival rate: 50 jobs/minute."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use KEDA (Kubernetes Event-Driven Autoscaling) ScaledObject on Azure Service Bus queue depth (option B). CPU-based HPA (option A) is incorrect for queue processing: pods sit at low CPU utilisation while waiting for jobs, then spike during processing — CPU lag means HPA scales up reactively after the queue has already grown. KEDA scales proactively based on queue depth: `queueLength: 5` means KEDA targets 1 pod per 5 unprocessed jobs. At 50 jobs/minute and 1-minute processing time, queue depth reaches 50 in steady state — KEDA scales to 10 pods (50/5). At peak (500 jobs queued) KEDA scales to 100 pods. With Cluster Autoscaler (option C) at a fixed pod count, queue depth grows without bound at peak. KEDA + Cluster Autoscaler together handle both pod count and node scaling.",
  "rejectedOutput": "Use HPA on CPU utilisation. CPU is a universal metric and does not require a KEDA installation. When pods are actively processing, CPU will be high and HPA will scale up.",
  "arbiters": ["keda-queue-depth-scaling", "hpa-cpu-lag-for-queue-workloads"],
  "whyAccepted": "KEDA queue-depth scaling is proactive: it sees the backlog growing and scales before CPU rises. HPA on CPU is reactive: CPU rises only after pods are processing, which is after the queue has already built up.",
  "whyRejected": "CPU utilisation for queue workers is low between jobs (waiting) and high during processing — HPA scales based on average CPU, not queue size. The 10-minute SLA requires proactive scaling that CPU cannot provide.",
  "sourceAnchors": [
    "https://keda.sh/docs/latest/scalers/azure-service-bus/",
    "https://kubernetes.io/docs/tasks/run-application/horizontal-pod-autoscale/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-352

```json
{
  "rowId": "EXT-352",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement `validate_and_transform_records(records: list[dict]) -> list[dict]` that validates each record has required fields (`id`, `name`, `email`) and transforms valid records: uppercase `name`, normalize `email` to lowercase. Invalid records are silently skipped.",
    "b": "Submitted implementation uses a bare `except:` clause around each record's validation and transformation.",
    "c": "Test inputs include: a record with a missing `id` key (KeyError), a record where `name` is None (AttributeError on .upper()), and a record where `email` has an integer type (AttributeError on .lower())."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace bare `except:` with explicit validation and specific exception handling. Bare `except:` catches all exceptions including SystemExit, KeyboardInterrupt, and GeneratorExit — it can silently swallow a Ctrl-C during a long batch operation, making the process impossible to interrupt. Replace with: (1) Explicit field presence check: `if not all(k in record for k in ('id', 'name', 'email')): continue`. (2) Type check before transformation: `if not isinstance(record['name'], str) or not isinstance(record['email'], str): continue`. (3) If try/except is used for additional errors, use `except (TypeError, AttributeError, ValueError)` — never bare except. The 'silently skip' requirement is correctly implemented with `continue` after validation failure, but the exception handling must be specific.",
  "rejectedOutput": "Bare except is acceptable here because the intent is to skip any invalid record regardless of the error type. The broad catch prevents any single bad record from crashing the batch.",
  "arbiters": ["python-bare-except-antipattern", "keyboard-interrupt-swallowing"],
  "whyAccepted": "Explicit field and type validation catches the stated error cases without swallowing SystemExit or KeyboardInterrupt. Bare except is an anti-pattern in all Python code.",
  "whyRejected": "'Skip any error regardless of type' does not justify swallowing SystemExit/KeyboardInterrupt. Explicit validation is always preferable to catching everything silently.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench",
    "https://docs.python.org/3/library/exceptions.html#BaseException"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-353

```json
{
  "rowId": "EXT-353",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A FastAPI endpoint performs a database query and returns results. A performance test reveals the endpoint takes 800ms at P99. Three agents investigate: (A) Identifies N+1 queries — 1 query for the main entity + 1 query per related entity (50 related entities = 51 queries). (B) Identifies missing index on the WHERE clause column. (C) Identifies that the database connection pool is exhausted (maxconn=5, 20 concurrent requests).",
    "b": "All three agents claim their finding is the 'root cause' of the 800ms P99.",
    "c": "Each fix is independently measurable. The team can only implement one fix per sprint."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Prioritise Agent-C's fix (connection pool exhaustion) as the immediate blocker, then Agent-A's fix (N+1), then Agent-B's fix (missing index). Reasoning: connection pool exhaustion causes requests to queue waiting for a connection — this adds unpredictable latency spikes at P99 that swamp all other optimisations. With maxconn=5 and 20 concurrent requests, 15 requests wait for a connection before any query runs. Fix: increase pool size to match concurrent request count (e.g., maxconn=25). The N+1 problem (Agent-A) is the next highest impact: 51 queries vs 1-2 queries with eager loading is a large multiplier. The missing index (Agent-B) affects query execution time but is only meaningful after the pool and N+1 are fixed. Sprint 1: pool size. Sprint 2: N+1 eager loading. Sprint 3: index if still needed.",
  "rejectedOutput": "Fix the N+1 queries first (Agent-A). N+1 is the most common performance anti-pattern and fixing it typically yields the largest improvement.",
  "arbiters": ["connection-pool-exhaustion-priority", "performance-fix-sequencing"],
  "whyAccepted": "Connection pool exhaustion is a prerequisite blocker: requests cannot even start until a connection is available. N+1 is high-impact but only measurable after the pool bottleneck is removed.",
  "whyRejected": "N+1 is the most impactful at steady state, but with 15 requests queued waiting for 5 connections, fixing N+1 first reduces query time from 51 to 2 queries — but requests still queue, so P99 improvement is minimal.",
  "sourceAnchors": [
    "https://www.swebench.com",
    "https://docs.sqlalchemy.org/en/20/core/pooling.html"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-354

```json
{
  "rowId": "EXT-354",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.9 codebase uses `threading.Thread` for concurrent I/O operations: fetching 20 external APIs, writing to 5 different databases, and reading from 3 file systems simultaneously. A developer proposes migrating to `asyncio` for all concurrent operations.",
    "b": "Current thread count: 28 threads peak. All I/O libraries used are synchronous (requests, psycopg2, standard file I/O). asyncio requires async-compatible libraries (httpx, asyncpg, aiofiles).",
    "c": "Application is a data pipeline running every 10 minutes. Not a long-running server."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "EVALUATE migration cost vs benefit before committing. The synchronous library problem is the decisive factor: migrating to asyncio requires replacing requests→httpx, psycopg2→asyncpg, and file I/O→aiofiles across the entire codebase. This is not a mechanical migration — each library has different APIs, connection pool semantics, and error handling. For a 10-minute batch pipeline (not a long-running high-concurrency server), 28 threads with ThreadPoolExecutor is a correct and low-overhead approach. asyncio's advantage is in high-concurrency scenarios (thousands of concurrent connections) — 28 concurrent I/O operations is well within threading's sweet spot. RECOMMENDATION: Use `concurrent.futures.ThreadPoolExecutor` with `as_completed()` for explicit concurrency management (structured vs ad-hoc threading). Defer asyncio migration until a library forcing async is adopted (e.g., if an API client is async-only).",
  "rejectedOutput": "Migrate to asyncio immediately. asyncio is faster than threading for I/O-bound workloads and is the modern Python concurrency model.",
  "arbiters": ["asyncio-vs-threading-io-bound", "synchronous-library-migration-cost"],
  "whyAccepted": "28 concurrent I/O operations is within threading's efficient range. asyncio's speedup is marginal at this scale. The synchronous-to-async library migration cost is high for a batch pipeline that works correctly today.",
  "whyRejected": "asyncio is faster for thousands of concurrent connections, not dozens. The migration requires replacing every I/O library — for a working 10-minute batch pipeline, this cost is not justified by a marginal speed improvement.",
  "sourceAnchors": [
    "https://docs.python.org/3/library/concurrent.futures.html",
    "https://peps.python.org/pep-0492/"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-355

```json
{
  "rowId": "EXT-355",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B3-ServiceMeshRouting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Add distributed tracing to an existing .NET 8 gRPC service that calls three downstream services: OrderService (HTTP REST), InventoryService (gRPC), and NotificationService (Azure Service Bus message).'",
    "b": "OpenTelemetry .NET SDK. W3C TraceContext propagation (RFC 9457). Each downstream call type requires different propagation mechanism.",
    "c": "The service already uses the OTel SDK for metrics. Traces should be correlated with existing metrics via exemplars."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Action items: (1) Enable tracing in AddOpenTelemetry(): `.WithTracing(builder => builder.AddAspNetCoreInstrumentation().AddGrpcClientInstrumentation().AddHttpClientInstrumentation())`. (2) HTTP REST to OrderService: HttpClientInstrumentation automatically injects `traceparent` and `tracestate` W3C headers into outgoing HttpClient requests — no manual code. (3) gRPC to InventoryService: GrpcClientInstrumentation automatically propagates trace context via gRPC metadata — no manual code. (4) Azure Service Bus to NotificationService: manually inject trace context into message properties: `message.ApplicationProperties[\"traceparent\"] = Activity.Current?.Id`. On the consumer side: `var parentId = message.ApplicationProperties[\"traceparent\"]; using var activity = source.StartActivity(\"ProcessNotification\", ActivityKind.Consumer, parentId)`. (5) Enable exemplars: `.SetExemplarFilter(ExemplarFilterType.TraceBased)` to link metric data points to active traces. (6) Export via OTLP to existing OTel Collector.",
  "rejectedOutput": "Add tracing manually to each downstream call using Activity.StartActivity() and manually set headers in each HTTP request, gRPC call, and Service Bus message.",
  "arbiters": ["otel-dotnet-auto-instrumentation", "w3c-tracecontext-propagation"],
  "whyAccepted": "OTel instrumentation libraries handle W3C header injection automatically for HTTP and gRPC. Only Service Bus requires manual propagation (no OTel instrumentation library for ASB messaging in .NET). Manual code for HTTP/gRPC duplicates what the library does and is error-prone.",
  "whyRejected": "Manual Activity.StartActivity() for HTTP/gRPC duplicates automatic instrumentation. Missing headers on even one call breaks the distributed trace. Auto-instrumentation is more reliable and handles edge cases (redirects, retries).",
  "sourceAnchors": [
    "https://opentelemetry.io/docs/languages/dotnet/instrumentation/",
    "https://www.w3.org/TR/trace-context/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-356

```json
{
  "rowId": "EXT-356",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Dapper query maps a flat result set to a nested object: `Order` with a list of `OrderLine` items. The result set has columns: `OrderId, OrderDate, CustomerId, LineId, ProductId, Quantity, UnitPrice`. A dictionary is used to accumulate OrderLines per Order.",
    "b": "Current code: `connection.Query<Order, OrderLine, Order>(sql, (order, line) => { /* accumulate */ }, splitOn: \"LineId\")`. A junior developer proposes using `QueryMultiple` with two separate SELECT statements instead.",
    "c": "Orders in the result set have an average of 5 OrderLines each. 100 orders per batch."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Retain the multi-mapping query with splitOn. For a simple parent-child relationship with known columns, `Query<Order, OrderLine, Order>` with splitOn is Dapper's idiomatic pattern. It performs one round trip, returns a flat result set, and the dictionary accumulation pattern is O(n) — correct for 100 orders × 5 lines = 500 rows. `QueryMultiple` with two SELECTs is appropriate for sibling queries (queries that are not parent-child related) or when the parent query's results need to be used as input to the child query. For a single JOIN-based parent-child query, QueryMultiple adds a second round trip without benefit. The junior developer should add a code comment explaining the splitOn column boundary and the dictionary accumulation pattern rather than changing the approach.",
  "rejectedOutput": "Switch to QueryMultiple with two separate SELECT statements. This is cleaner than the multi-mapping accumulation dictionary pattern and separates concerns.",
  "arbiters": ["dapper-querymultiple-vs-multimapping", "parent-child-accumulation"],
  "whyAccepted": "Multi-mapping with splitOn is Dapper's designed solution for JOIN-based parent-child results. One round trip, O(n) accumulation, correct for the use case. QueryMultiple adds unnecessary round trip.",
  "whyRejected": "QueryMultiple sends two queries where one JOIN handles the same data. 'Cleaner' at the cost of an extra network round trip is not a net improvement.",
  "sourceAnchors": [
    "https://github.com/DapperLib/Dapper#multi-mapping",
    "https://github.com/DapperLib/Dapper#multiple-results"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-357

```json
{
  "rowId": "EXT-357",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 application has a data table that displays 10,000 rows fetched from an API. The table supports column sorting and filtering. When a sort is applied, the entire dataset is re-sorted client-side and the table re-renders.",
    "b": "Options: (A) Implement virtual scrolling with CDK VirtualScrollViewport + server-side sort/filter. (B) Implement client-side pagination (50 rows/page) + client-side sort/filter. (C) CDK virtual scrolling with client-side sort/filter.",
    "c": "Sort and filter latency requirement: results visible within 200ms. Network round-trip to API is 150-300ms."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option B: client-side pagination with client-side sort/filter. The 200ms latency requirement for sort/filter is incompatible with option A (server-side sort): 150-300ms network round trip alone exceeds the budget. Fetching all 10,000 rows once and sorting/filtering in memory (typically < 10ms for 10,000 rows in JavaScript) satisfies the 200ms requirement. Virtual scrolling (options A and C) requires a fixed-height container and is ideal for infinite scroll patterns — for a data table with explicit pagination, it adds complexity without user-experience benefit. Client-side pagination with 50 rows/page renders only 50 DOM (Document Object Model) nodes at a time, which is performant. The initial fetch of 10,000 rows is a one-time cost amortised across all subsequent sorts/filters.",
  "rejectedOutput": "Use server-side sort/filter with virtual scrolling (option A). Sorting 10,000 rows client-side is expensive and virtual scrolling handles large datasets natively.",
  "arbiters": ["client-side-sort-latency-budget", "virtual-scroll-pagination-tradeoff"],
  "whyAccepted": "200ms sort/filter requirement cannot be met by server-side sort (150-300ms network alone). Client-side sort of 10,000 rows takes <10ms. Pagination limits DOM size to 50 rows.",
  "whyRejected": "Server-side sort exceeds the 200ms latency budget. 'Sorting 10,000 rows is expensive' is incorrect for modern JavaScript — Array.sort() on 10,000 objects takes <5ms.",
  "sourceAnchors": [
    "https://material.angular.io/components/table/overview#pagination",
    "https://material.angular.io/cdk/scrolling/overview"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-358

```json
{
  "rowId": "EXT-358",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React 18 application has a complex form with 20 fields. Each keystroke in any field triggers a re-render of all 20 fields because they share a single useState object. A developer reports typing lag.",
    "b": "Options: (A) Split into 20 separate useState calls (one per field). (B) Use useReducer for the form state. (C) Use a library: React Hook Form with uncontrolled inputs. (D) Use Zustand for form state.",
    "c": "Form fields have cross-field validation (e.g., endDate must be after startDate). The form submits to an API."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use React Hook Form (option C). React Hook Form uses uncontrolled inputs (refs, not state) — each keystroke does NOT trigger a React re-render. The form state lives outside React's rendering cycle. Cross-field validation is natively supported via `validate` functions with access to `getValues()`. On submit, `handleSubmit(onSubmit)` serialises all field values synchronously. This is the most performant pattern for complex forms: zero re-renders per keystroke. Option A (20 separate useState) reduces re-render scope per field but still triggers 1 re-render per keystroke. Option B (useReducer) dispatches an action per keystroke — same re-render count as useState. Option D (Zustand) externalises state but is overkill for form state that doesn't need to be shared globally.",
  "rejectedOutput": "Split into 20 separate useState calls (option A). This limits each keystroke re-render to only the changed field's component if they are split into child components with their own state.",
  "arbiters": ["react-hook-form-uncontrolled", "form-rerender-per-keystroke"],
  "whyAccepted": "React Hook Form's uncontrolled approach achieves zero re-renders per keystroke — the root cause of typing lag. 20 separate useState calls still re-render once per keystroke.",
  "whyRejected": "20 separate useState calls still trigger re-renders per keystroke — reducing the scope (1 field instead of 20) helps but does not eliminate the re-render. React Hook Form eliminates re-renders entirely during typing.",
  "sourceAnchors": [
    "https://react-hook-form.com/get-started",
    "https://react.dev/reference/react/useReducer"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-359

```json
{
  "rowId": "EXT-359",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B2-DeploymentRouting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Add health check endpoints to an ASP.NET Core 8 API that reports individual dependency health: SQL Server database, Redis cache, and an external payment gateway (HTTP).'",
    "b": "ASP.NET Core health checks: IHealthCheck interface, MapHealthChecks() endpoint routing, HealthCheckService.",
    "c": "Kubernetes liveness and readiness probes will consume these endpoints. Liveness probe failure restarts the pod. Readiness probe failure stops routing traffic to the pod."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Action items: (1) Register health checks in DI: `.AddSqlServer(connectionString, tags: new[]{\"ready\"})`, `.AddRedis(redisConnectionString, tags: new[]{\"ready\"})`, `.AddUrlGroup(new Uri(paymentGatewayHealthUrl), tags: new[]{\"ready\"})`. (2) Map TWO health check endpoints: `app.MapHealthChecks(\"/health/live\", new HealthCheckOptions { Predicate = _ => false })` — liveness: always returns Healthy unless the app has crashed (no dependency checks). `app.MapHealthChecks(\"/health/ready\", new HealthCheckOptions { Predicate = check => check.Tags.Contains(\"ready\") })` — readiness: checks SQL Server, Redis, payment gateway. (3) Kubernetes liveness probe: HTTP GET /health/live. (4) Kubernetes readiness probe: HTTP GET /health/ready. The critical design decision: liveness probe must NOT check external dependencies — if SQL Server is temporarily unavailable, Kubernetes restarting the pod does not fix the database. A pod restart loop (CrashLoopBackOff) is worse than a pod that is temporarily not-ready.",
  "rejectedOutput": "Map a single /health endpoint that checks all dependencies. Kubernetes liveness and readiness probes can both use this endpoint.",
  "arbiters": ["aspnetcore-health-checks-liveness-readiness", "kubernetes-probe-separation"],
  "whyAccepted": "Separate liveness/readiness endpoints are required because dependency failures should make the pod NotReady (stop traffic) not trigger a restart. A single health endpoint used for both causes restart loops on database outages.",
  "whyRejected": "If a single health endpoint checks all dependencies and Kubernetes uses it for liveness, a database outage triggers pod restarts. Restarting the pod does not fix the database — the pod enters CrashLoopBackOff.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks",
    "https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-360

```json
{
  "rowId": "EXT-360",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS application uses TypeORM for database access. The team debates between Repository pattern (TypeORM `@InjectRepository(Entity)`) vs raw EntityManager injection for database operations.",
    "b": "Repository pattern: each entity has a typed repository with `find`, `findOne`, `save`, `delete`. EntityManager: a single service injected with the full ORM API for all entities.",
    "c": "Application has 15 entities. Unit tests mock the database layer."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use the Repository pattern per entity. Injecting `@InjectRepository(Order)` into `OrderService` makes the dependency explicit: OrderService only touches the Orders table. EntityManager injection gives OrderService access to all 15 entities — violating the single responsibility principle and making it difficult to determine from the class signature which tables a service touches. For unit testing, mocking `getRepositoryToken(Order)` is straightforward: `{ provide: getRepositoryToken(Order), useValue: { find: jest.fn(), save: jest.fn() } }`. Mocking EntityManager requires a more complex mock surface. The Repository pattern also aligns with NestJS's DI conventions and TypeORM's recommended usage. EntityManager is appropriate for use cases requiring cross-entity transactions within a single service (use `DataSource.transaction(manager => ...)` explicitly for those cases).",
  "rejectedOutput": "Use EntityManager injection. Having one EntityManager per service reduces the number of injected dependencies and provides flexibility to query any entity as needs evolve.",
  "arbiters": ["typeorm-repository-per-entity", "nestjs-di-explicit-dependencies"],
  "whyAccepted": "Repository per entity makes dependencies explicit, simplifies unit test mocking, and enforces single-responsibility. EntityManager gives unlimited table access — a hidden dependency surface.",
  "whyRejected": "EntityManager 'flexibility' means any service can accidentally touch any table — this is a hidden coupling that becomes a maintenance problem as the codebase grows. Explicit is better than implicit.",
  "sourceAnchors": [
    "https://docs.nestjs.com/techniques/database#repository-pattern",
    "https://typeorm.io/working-with-repository"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-361

```json
{
  "rowId": "EXT-361",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C3-OutputFormatting",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "An API returns error responses in different formats depending on the endpoint: some return `{error: 'message'}`, others return `{message: 'error', code: 123}`, others return plain text error strings.",
    "b": "RFC 9457 (Problem Details for HTTP APIs) provides a standard error format. Evaluate migrating to a uniform error format.",
    "c": "API has 89 endpoints across 12 services. Client SDK generates typed error objects from the response."
  },
  "function": "finalMachineResults_to_userFormat",
  "acceptedOutput": "Migrate to RFC 9457 Problem Details format across all 89 endpoints. The RFC 9457 format: `{type: \"about:blank\", title: \"Validation Failed\", status: 400, detail: \"The 'email' field is not a valid email address.\", instance: \"/orders/123\"}`. Benefits: (1) The client SDK can have a single `ProblemDetails` typed error class instead of 3 different error shapes. (2) `Content-Type: application/problem+json` signals to generic API clients that this is a structured error. (3) `type` URI allows machine-readable error categorisation. Migration plan: add an RFC 9457 error formatter to the API gateway/middleware layer — all error responses pass through it and are reformatted. This centralises the change: services don't need individual updates if the gateway handles formatting. Gate: Client SDK updated to parse `ProblemDetails` type and all 89 endpoints return `application/problem+json` on error paths.",
  "rejectedOutput": "Keep the existing error formats. Migrating 89 endpoints across 12 services is a large effort for a cosmetic formatting change. Client SDKs can handle multiple formats with type narrowing.",
  "arbiters": ["rfc-9457-problem-details", "api-error-format-consistency"],
  "whyAccepted": "RFC 9457 standardisation reduces SDK complexity from 3 error shapes to 1. API gateway centralisation means services need not be individually updated. This is an infrastructure change, not 89 individual endpoint changes.",
  "whyRejected": "'Cosmetic formatting change' understates the SDK maintenance burden of 3 incompatible error shapes. Type narrowing on every catch block is error-prone and degrades as new shapes are added.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc9457",
    "https://datatracker.ietf.org/doc/html/rfc7807"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-362

```json
{
  "rowId": "EXT-362",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API resource `Document` can be in states: DRAFT, REVIEW, PUBLISHED, ARCHIVED. State transitions are: DRAFT→REVIEW, REVIEW→DRAFT (reject), REVIEW→PUBLISHED, PUBLISHED→ARCHIVED. Design the API for state transitions.",
    "b": "AIP-136 custom methods for actions. AIP-216 for state and lifecycle management.",
    "c": "Clients need to know which transitions are valid from the current state. The API should not allow invalid transitions (e.g., DRAFT→PUBLISHED directly)."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Design using AIP-136 custom methods for each transition: `POST /documents/{id}:submitForReview` (DRAFT→REVIEW), `POST /documents/{id}:reject` (REVIEW→DRAFT), `POST /documents/{id}:publish` (REVIEW→PUBLISHED), `POST /documents/{id}:archive` (PUBLISHED→ARCHIVED). Each endpoint enforces state machine validity — the server returns 400 with a Problem Details error if the transition is invalid for the current state. Include allowed_transitions in the GET /documents/{id} response: `{status: \"REVIEW\", allowed_transitions: [\"reject\", \"publish\"]}` per AIP-216. This approach: (1) Makes valid transitions discoverable without clients hard-coding state machine logic. (2) Enforces transitions server-side. (3) Each custom method can have different authorization requirements (e.g., only editors can publish). NEVER use PATCH status for state transitions (as discussed in EXT-322).",
  "rejectedOutput": "Use a single PATCH /documents/{id} endpoint with a `status` field and rely on the server to reject invalid status values. This is simpler and follows standard REST conventions.",
  "arbiters": ["aip-136-state-transitions", "aip-216-lifecycle"],
  "whyAccepted": "Custom methods per transition enforce state machine semantics, allow per-transition authorization, and provide discoverable allowed_transitions. PATCH on status allows any status value — the client must know the state machine, not the server.",
  "whyRejected": "PATCH status requires clients to know valid transitions. A DRAFT document PATCH to PUBLISHED would need to be blocked by business logic in the generic update handler — muddling concerns. Custom methods make transitions first-class API concepts.",
  "sourceAnchors": [
    "https://google.aip.dev/136",
    "https://google.aip.dev/216"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-363

```json
{
  "rowId": "EXT-363",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C1-ResultSynthesis",
  "rowClass": "negative-example",
  "input": {
    "a": "A model serving system reports: 'TOKENIZERS_PARALLELISM warning: Hugging Face tokenizers detected parallel processes. Set TOKENIZERS_PARALLELISM=false to suppress.' The warning appears in production logs 10,000 times per day.",
    "b": "Two operator responses: (R1) Set `TOKENIZERS_PARALLELISM=false` environment variable. (R2) Suppress the warning with `warnings.filterwarnings('ignore', category=UserWarning, module='transformers')`. (R3) Investigate why the tokeniser is called in a multiprocessing context and fix the root cause.",
    "c": "The system uses DataLoader workers (num_workers=4) to preprocess batches. Tokenisation happens inside the DataLoader worker processes."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Select R1 as the immediate fix (set TOKENIZERS_PARALLELISM=false), and separately apply R3 as the architectural improvement. The warning fires because Rust-based HuggingFace tokenisers are not fork-safe — they detect forked processes and warn. TOKENIZERS_PARALLELISM=false disables the tokeniser's internal thread pool (which conflicts with the outer multiprocessing), resolving the warning and preventing potential deadlocks. R2 (suppress the warning) hides a real issue without fixing it — the tokeniser deadlock risk persists. R3 (architectural fix): move tokenisation out of DataLoader workers (into the main process or a preprocessing step) so tokenisers run in a single process only. This is the correct long-term fix. The two fixes are complementary: R1 is safe and immediate; R3 is correct and architectural.",
  "rejectedOutput": "Apply R2: suppress the warning. The system functions correctly despite the warning and suppressing it reduces log noise without changing behaviour.",
  "arbiters": ["huggingface-tokeniser-parallelism", "multiprocessing-fork-safety"],
  "whyAccepted": "TOKENIZERS_PARALLELISM=false resolves the root cause of the warning (thread pool conflict in forked process). It also prevents potential deadlocks. Suppressing the warning hides both the log noise and the deadlock risk.",
  "whyRejected": "Warning suppression removes the signal without fixing the Rust tokeniser thread-safety issue. Under high load, forked processes with active tokeniser thread pools can deadlock — this is not cosmetic noise.",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1",
    "https://huggingface.co/docs/tokenizers/en/api/tokenizers#tokenizers.Tokenizer"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-364

```json
{
  "rowId": "EXT-364",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A multilingual corpus pipeline aligns Tatoeba sentence pairs for 20 language pairs. The alignment script uses a hash map to deduplicate exact duplicate sentence pairs (same source + target text). A developer proposes adding fuzzy deduplication: pairs where source Levenshtein distance < 5 AND target Levenshtein distance < 5 are considered duplicates.",
    "b": "Dataset size: 8 million sentence pairs. Fuzzy deduplication at O(n²) Levenshtein comparisons is computationally infeasible.",
    "c": "The goal is to remove near-duplicates that differ only in punctuation or whitespace."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace the O(n²) fuzzy deduplication proposal with a normalise-then-hash approach: (1) Normalise each sentence: strip leading/trailing whitespace, collapse internal whitespace to single space, remove terminal punctuation (period, exclamation, question mark), lowercase. (2) Hash the normalised source+target pair. (3) Deduplicate by hash. This is O(n) and handles the stated near-duplicate cases (punctuation and whitespace differences) without quadratic complexity. For 8 million pairs, O(n²) Levenshtein would require ~64 trillion comparisons — computationally infeasible. MinHash LSH (Locality-Sensitive Hashing) is an alternative if more aggressive fuzzy matching is needed, but normalise-then-hash covers the stated use case at O(n).",
  "rejectedOutput": "Implement the fuzzy deduplication with Levenshtein distance < 5. This is more principled than normalisation heuristics and correctly identifies near-duplicates.",
  "arbiters": ["o-n-squared-infeasibility", "normalise-hash-deduplication"],
  "whyAccepted": "Normalise-then-hash is O(n) and handles punctuation/whitespace near-duplicates — the stated use case. O(n²) Levenshtein on 8M pairs is computationally infeasible (64T comparisons).",
  "whyRejected": "O(n²) Levenshtein is mathematically correct but computationally infeasible at 8M pairs. A principled algorithm that cannot run is not a viable solution.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://ekzhu.com/datasketch/lsh.html"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-365

```json
{
  "rowId": "EXT-365",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Vue 3 component uses `watch` to synchronize two reactive values: `watch(formData, () => { processedData.value = transform(formData.value) }, { immediate: true })`. The developer asks if this is the correct approach.",
    "b": "The relationship between `formData` and `processedData` is purely deterministic: `processedData` is always `transform(formData)` with no side effects.",
    "c": "transform() is a pure function with no async operations."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Replace with `computed`: `const processedData = computed(() => transform(formData.value))`. For a deterministic, synchronous, side-effect-free transformation, `computed` is the correct reactive primitive: (1) It is lazy — computed only re-evaluates when its dependencies change. (2) It is cached — multiple reads of `processedData` do not re-run `transform()`. (3) It is declarative — the relationship between input and output is explicit. `watch` with `{ immediate: true }` is equivalent but has drawbacks: it is eager (runs immediately even if nothing reads `processedData`), it mutates a separate ref (adds boilerplate), and it obscures the input→output relationship. `watch` is correct when the transformation has side effects (e.g., API calls, localStorage writes, emitting events) — for pure transformations, always use `computed`.",
  "rejectedOutput": "The watch approach is correct. watch with immediate:true and computed are functionally equivalent for this use case and watch is more explicit about when the transformation runs.",
  "arbiters": ["vue3-computed-vs-watch", "reactive-primitive-selection"],
  "whyAccepted": "computed is the correct primitive for deterministic derived state: lazy, cached, declarative. watch is correct for side effects. This is a pure transformation — computed is unambiguously better.",
  "whyRejected": "watch with immediate:true is functionally similar but: eager evaluation even when result is unread, requires a separate mutable ref, and obscures the derived-value relationship. 'More explicit' is a false benefit here — computed's declaration IS the explicit relationship.",
  "sourceAnchors": [
    "https://vuejs.org/guide/essentials/computed.html",
    "https://vuejs.org/guide/essentials/watchers.html#when-to-use-watchers"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-366

```json
{
  "rowId": "EXT-366",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-LearningCycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "An EF Core 6 application uses `DbContext.Database.ExecuteSqlRaw()` for bulk delete operations: `context.Database.ExecuteSqlRaw($\"DELETE FROM Orders WHERE Status = {status} AND CreatedAt < {cutoffDate}\")`. EF Core 7 introduced `ExecuteDeleteAsync()`. Evaluate migration.",
    "b": "EF Core 7's `ExecuteDeleteAsync`: `await context.Orders.Where(o => o.Status == status && o.CreatedAt < cutoffDate).ExecuteDeleteAsync()`. This generates a single DELETE ... WHERE statement without loading entities into memory.",
    "c": "The current ExecuteSqlRaw uses string interpolation — EF Core protects against injection by converting interpolated strings to parameterized queries via FormattableString."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "MIGRATE to ExecuteDeleteAsync. EF Core 7's ExecuteDeleteAsync is type-safe, composable with LINQ expressions, and does not require SQL string construction. The current ExecuteSqlRaw with string interpolation is safe (EF Core's FormattableString handling parameterizes values), but it is fragile: if a developer changes the interpolation to regular string formatting (dropping the $), it becomes a SQL injection vulnerability (CWE-89). ExecuteDeleteAsync eliminates this risk: the WHERE clause is expressed as a LINQ predicate, not a SQL string. Additionally, ExecuteDeleteAsync works across database providers (SQL Server, PostgreSQL, SQLite) without SQL dialect concerns. Migration is one-line per call site: replace `ExecuteSqlRaw($\"DELETE FROM ... WHERE ...\")` with `.Where(predicate).ExecuteDeleteAsync()`.",
  "rejectedOutput": "Keep ExecuteSqlRaw. The current implementation is safe due to EF Core's FormattableString parameterization and migrating is unnecessary churn.",
  "arbiters": ["efcore7-execute-delete", "sql-injection-latent-risk"],
  "whyAccepted": "ExecuteDeleteAsync is type-safe LINQ without SQL strings — eliminates the latent injection risk from FormattableString being accidentally replaced with string.Format. One-line migration per call site.",
  "whyRejected": "'Safe due to FormattableString' is correct today but fragile under future code changes. The latent CWE-89 risk from accidental string formatting is real and eliminable at minimal migration cost.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#executeupdate-and-executedelete-bulk-updates",
    "https://github.com/dotnet/efcore"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-367

```json
{
  "rowId": "EXT-367",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET 8 service receives messages from a RabbitMQ queue. Message processing takes 2-10 seconds. The service currently sets `prefetchCount=1` (process one message at a time). Ops team asks whether to increase prefetchCount to 10 to improve throughput.",
    "b": "Increasing prefetchCount=10 means the broker delivers up to 10 unacknowledged messages to the consumer. If the consumer crashes with 10 in-flight messages, all 10 are redelivered on restart.",
    "c": "Messages are idempotent (safe to process twice). Consumer has 4 CPU cores. Processing is CPU-bound (not I/O-bound). Throughput target: 5 messages/second."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Increase prefetchCount to 4 (matching CPU core count) and run 4 concurrent consumers. For CPU-bound processing on a 4-core machine, the optimal concurrency is 4 (one per core). prefetchCount=10 with a single consumer does not increase throughput for CPU-bound work — processing is bottlenecked by CPU, not message delivery. The correct pattern: prefetchCount=1 per consumer + 4 consumers (each consuming from the same queue), OR prefetchCount=4 on a single multi-threaded consumer using Parallel.ForEachAsync with DegreeOfParallelism=4. At 2-10 second processing time and 4 cores, this achieves 0.4-2 messages/second per core × 4 = 1.6-8 messages/second total, meeting the 5 msg/s target. The idempotency guarantee makes the redelivery-on-crash acceptable.",
  "rejectedOutput": "Increase prefetchCount to 10. More prefetch means faster message delivery and higher throughput.",
  "arbiters": ["rabbitmq-prefetch-cpu-bound", "consumer-concurrency-core-count"],
  "whyAccepted": "CPU-bound processing bottleneck is CPU cores, not message delivery rate. prefetchCount beyond core count delivers messages faster than they can be processed, increasing redelivery count on crash without throughput benefit.",
  "whyRejected": "prefetchCount=10 for CPU-bound work delivers 10 messages to a consumer that can only process one at a time per thread. Increasing prefetch without parallel consumers does not increase CPU-bound throughput.",
  "sourceAnchors": [
    "https://www.rabbitmq.com/docs/consumer-prefetch",
    "https://learn.microsoft.com/en-us/dotnet/core/parallel-programming/how-to-implement-a-producer-consumer-dataflow-pattern"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-368

```json
{
  "rowId": "EXT-368",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservices system needs to implement search across multiple services: product catalogue, user profiles, and order history. Options: (A) Each service exposes a /search endpoint; the API gateway fans out requests and merges results. (B) All searchable data is indexed in a centralised Elasticsearch cluster; a single search service queries it.",
    "c": "Search queries combine fields from multiple domains: 'find orders by user email AND product category'. Cross-service join is required.",
    "c2": "Product catalogue: 2M products. User profiles: 500K users. Order history: 20M orders."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use centralised Elasticsearch (option B). The cross-service join requirement is the decisive factor: a fan-out to 3 service /search endpoints cannot perform a join across services — the API gateway would receive 3 independent result sets and cannot correlate 'orders where user.email = X AND product.category = Y'. Elasticsearch with a denormalised index (combining order, user, and product fields at index time) handles this as a single query. The indexing architecture: each service publishes change events (order created/updated, product updated, user updated) to a message bus; an indexer service consumes events and updates the Elasticsearch index. This is the CQRS (Command Query Responsibility Segregation) + read model pattern. Option A (fan-out) is correct only when search is within a single domain and result merging (not joining) is acceptable.",
  "rejectedOutput": "Fan out to each service's /search endpoint and merge results at the gateway. This preserves service autonomy and avoids centralised search infrastructure.",
  "arbiters": ["elasticsearch-cross-service-join", "cqrs-read-model-search"],
  "whyAccepted": "Cross-service join (user email AND product category in an order search) requires a denormalised index. Fan-out returns 3 independent result sets that cannot be joined at the API gateway.",
  "whyRejected": "Fan-out merge is correct for union (combine results from services) but incorrect for join (filter results by conditions across services). The requirement explicitly states a cross-domain join.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs",
    "https://www.elastic.co/guide/en/elasticsearch/reference/current/index.html"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-369

```json
{
  "rowId": "EXT-369",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A .NET 8 API endpoint accepts a file path parameter and returns the file contents: `app.MapGet(\"/files/{*path}\", (string path) => File.ReadAllText(path))`. The endpoint is intended for reading application log files from a known logs directory.",
    "b": "CWE-22 Path Traversal. Evaluate: what inputs exploit this and what is the correct fix.",
    "c": "Intended path: 'logs/app-2024-01-15.log'. Exploit input: '../../../../etc/passwd' or 'C:\\Windows\\System32\\drivers\\etc\\hosts'."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "This is a path traversal vulnerability (CWE-22). `File.ReadAllText(path)` with an unsanitized user-supplied path allows reading any file the application process can access. Fix: (1) Define the allowed base directory: `var baseDir = Path.GetFullPath(\"/app/logs\");`. (2) Resolve the user-supplied path relative to the base: `var fullPath = Path.GetFullPath(Path.Combine(baseDir, path));`. (3) Verify the resolved path starts with the base directory: `if (!fullPath.StartsWith(baseDir + Path.DirectorySeparatorChar)) return Results.Forbid();`. (4) Verify the file exists: `if (!File.Exists(fullPath)) return Results.NotFound();`. (5) Read the file: `return Results.Ok(File.ReadAllText(fullPath))`. The `Path.GetFullPath` call normalises `..` sequences — if the attacker supplies `../../../etc/passwd`, GetFullPath resolves it to `/etc/passwd` which does NOT start with `/app/logs/`, so the check catches it.",
  "rejectedOutput": "Validate that the path does not contain '..' before processing: `if (path.Contains(\"..\")) return Results.BadRequest()`. This blocks directory traversal.",
  "arbiters": ["cwe-22-path-traversal-dotnet", "path-getfullpath-normalisation"],
  "whyAccepted": "Path.GetFullPath normalises all traversal sequences (including encoded %2F, URL-decoded sequences) before the prefix check. This is bypass-resistant.",
  "whyRejected": "Checking for '..' in the raw path string is bypassable: URL-encoded `%2e%2e%2f`, `..%2f`, or Windows `..\\` variants may not contain literal '..'. Path.GetFullPath normalises all these before comparison.",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/22.html",
    "https://learn.microsoft.com/en-us/dotnet/api/system.io.path.getfullpath"
  ],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-370

```json
{
  "rowId": "EXT-370",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A web application stores user passwords using MD5 hashing: `passwordHash = md5(password + salt)` where salt is a 4-character random string stored alongside the hash.",
    "b": "OWASP ASVS V2.4 Password Storage requirements. Evaluate the vulnerability and correct fix.",
    "c": "OWASP ASVS requires an approved adaptive hashing algorithm (bcrypt, scrypt, Argon2) with a work factor tuned to make brute-force infeasible."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "The password storage is critically vulnerable. MD5 is a cryptographic hash, not a password hashing function — it is designed to be fast (billions of hashes/second on modern GPUs), making brute-force and rainbow table attacks trivially fast. A 4-character salt is insufficient: at 62 possible characters (a-z, A-Z, 0-9), 62⁴ = ~14.8 million possible salts, which an attacker can enumerate. OWASP ASVS V2.4.1 requires: Argon2id (preferred), bcrypt (minimum cost factor 10), scrypt, or PBKDF2-HMAC-SHA256 with ≥600,000 iterations. Fix: (1) Migrate to Argon2id using .NET's `Konscious.Security.Cryptography.Argon2` package. (2) On next login, re-hash the password with Argon2id and update the stored hash. (3) Force password reset for users who do not log in within the migration window. Never decrypt or re-hash without the plaintext password (hashes cannot be migrated without user interaction).",
  "rejectedOutput": "Upgrade to SHA-256 with a longer salt (32 bytes). SHA-256 is a stronger algorithm than MD5 and a longer salt prevents rainbow tables.",
  "arbiters": ["owasp-asvs-v2-4-password-storage", "adaptive-hashing-work-factor"],
  "whyAccepted": "Argon2id is designed for password hashing: memory-hard, configurable cost factor, immune to GPU acceleration by design. SHA-256 (fast hash) with a long salt is still breakable at billions of attempts/second.",
  "whyRejected": "SHA-256 is a general-purpose cryptographic hash — fast by design. A 32-byte salt prevents rainbow tables but does not slow brute-force. Modern GPUs compute SHA-256 at 10+ billion hashes/second. bcrypt at cost-10 is ~100 hashes/second — 100 million times slower.",
  "sourceAnchors": [
    "https://owasp.org/www-project-application-security-verification-standard/",
    "https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html"
  ],
  "licenseConstraint": "CC-BY-3.0",
  "trainingStatus": "APPROVED"
}
```
