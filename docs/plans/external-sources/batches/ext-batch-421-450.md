# External Sources Batch 15 — EXT-291 through EXT-310

Generated: 2026-05-04  
Ledger range: EXT-291 – EXT-310  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, VUE-DOCS, EFCORE-SOURCE, HELM-DOCS, PROMETHEUS-DOCS, DOTNET-MICROSERVICES, SVELTE-DOCS

---

## EXT-291

```json
{
  "rowId": "EXT-291",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We need to run a one-time database schema migration job in Kubernetes before the new deployment goes live. The job must complete successfully before the application pods start.",
    "b": "Options: (1) Kubernetes Job with initContainer dependency — migration Job runs first, Deployment uses initContainer to wait for job completion before main container starts. (2) Helm pre-upgrade hook Job — migration Job annotated with helm.sh/hook: pre-upgrade runs before any Deployment rollout. (3) Application startup migration — the application pod runs migrations itself on startup before serving traffic.",
    "c": "Context: Helm-managed deployment. Rollback must be clean. Migration is idempotent."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use Helm pre-upgrade hook Job (option 2). Annotate the Job with `helm.sh/hook: pre-upgrade` and `helm.sh/hook-delete-policy: before-hook-creation`. The Job runs to completion before any Deployment update proceeds — if the Job fails, Helm aborts the upgrade and the old Deployment remains running. On rollback `helm rollback` does not re-run pre-upgrade hooks, so old pods continue unaffected. The migration must be idempotent (safe to re-run) which the context confirms. Option 1 (initContainer wait) couples migration logic to every pod restart unnecessarily and bloats pod spec. Option 3 (app-startup migration) creates a race if multiple pods start simultaneously and lacks a clean failure signal to Helm.",
  "rejectedOutput": "Run the migration inside an initContainer in each application pod. The initContainer checks whether the migration has already run and exits if so.",
  "arbiters": ["helm-docs-hook-spec", "kubernetes-job-lifecycle"],
  "whyAccepted": "Pre-upgrade hook provides clean lifecycle separation: migration runs once, Helm waits for completion, rollback does not re-trigger migration. Idempotency guarantee satisfies the re-run-safety requirement.",
  "whyRejected": "InitContainer approach re-evaluates migration on every pod restart including normal scaling events, adds latency to all pod starts, and does not give Helm a clean abort signal on failure.",
  "sourceAnchors": [
    "https://helm.sh/docs/topics/charts_hooks/",
    "https://kubernetes.io/docs/concepts/workloads/controllers/job/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-292

```json
{
  "rowId": "EXT-292",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement a function `parse_csv_with_types(path: str) -> list[dict]` that reads a CSV file and auto-converts columns: integer columns to int, float columns to float, and boolean columns (values 'true'/'false') to bool. Unrecognised values stay as str.",
    "b": "Submitted implementation uses `eval()` on each cell value to attempt type conversion, falling back to str on exception.",
    "c": "File is user-supplied. Values include strings like `__import__('os').system('rm -rf /')` in test fixtures."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Reject the eval()-based approach as a critical security flaw (CWE-94 Code Injection). Replace with explicit type-probe logic: attempt `int(val)`, then `float(val)`, then check `val.lower() in ('true','false')` for bool, else keep as str. No code execution occurs during parsing. Each probe is O(1) per cell. The function signature and output contract are satisfied without eval.",
  "rejectedOutput": "The eval() approach is acceptable because it is wrapped in a try/except block, so any malicious payload will be caught as an exception and fall through to the str branch.",
  "arbiters": ["cwe-94-code-injection", "bigcodebench-type-conversion"],
  "whyAccepted": "Explicit type probing satisfies the contract with zero code execution risk. eval() is never safe on untrusted input regardless of try/except — the exception only triggers after the code runs.",
  "whyRejected": "try/except does not prevent code execution; it only suppresses the exception after execution. `__import__('os').system(...)` runs before any exception is raised.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench",
    "https://cwe.mitre.org/data/definitions/94.html"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-293

```json
{
  "rowId": "EXT-293",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Three agents proposed fixes for a failing test that asserts `response.status_code == 200` on a POST /orders endpoint. Agent-A returned early 200 before writing to the database. Agent-B added a missing `await db.commit()` after the insert. Agent-C changed the test assertion to `== 201`.",
    "b": "Evaluate which fix is correct given: HTTP semantics (RFC 9110), POST creates a resource, commit is required for durability.",
    "c": "All three agents report their fix as 'resolving the test failure'."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Select Agent-B's fix: add `await db.commit()`. The test failure root cause is that the database transaction was never committed, so the inserted row disappeared on rollback at request end, making subsequent reads fail. RFC 9110 §9.3.3 specifies POST creating a resource should return 201 Created with a Location header — but changing the assertion (Agent-C) papers over a real bug without fixing the missing commit. Returning 200 before the write (Agent-A) creates a false success signal with no durability guarantee. Agent-B's fix is the only one that addresses root cause without violating HTTP semantics.",
  "rejectedOutput": "Agent-C's fix is pragmatic: the test was asserting the wrong status code, and 200 is a valid response for POST. Changing the assertion removes the false failure with minimal code change.",
  "arbiters": ["rfc-9110-post-semantics", "database-durability-commit"],
  "whyAccepted": "Agent-B fixes the actual bug (uncommitted transaction) while preserving correct HTTP semantics. The test failure was a symptom of missing commit, not a wrong assertion.",
  "whyRejected": "Agent-C changes the test to match broken behavior. The real issue — rows disappearing due to missing commit — remains and will cause data loss in production.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc9110#section-9.3.3",
    "https://www.swebench.com"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-294

```json
{
  "rowId": "EXT-294",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-LearningCycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.8 codebase uses `typing.Protocol` with `@runtime_checkable` decorator and `isinstance()` checks throughout. Python 3.12 introduced PEP-695 type parameter syntax and the `type` statement. Evaluate whether to migrate Protocol definitions to use PEP-695 syntax.",
    "b": "PEP-695 introduces `type X = ...` alias syntax and `class Foo[T]:` generic syntax. Protocol itself is unchanged. The `@runtime_checkable` + isinstance pattern is unchanged in 3.12.",
    "c": "Codebase has 47 Protocol definitions. CI targets Python 3.10+."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "DEFER migration. PEP-695 type parameter syntax (`class Foo[T]:`) applies to generic classes and functions — it does not change the Protocol API or runtime_checkable behaviour. Migrating 47 Protocol definitions to use `[T]` style type params would only affect generic Protocols (those with TypeVar parameters), which may be a minority. More importantly, CI targets Python 3.10+ and PEP-695 syntax requires Python 3.12 — adopting it would break the 3.10 target. Schedule for when CI minimum is raised to 3.12.",
  "rejectedOutput": "Migrate all Protocol definitions now to use PEP-695 syntax for consistency with modern Python. The migration is mechanical and low-risk.",
  "arbiters": ["pep-695-type-params", "python-version-compatibility"],
  "whyAccepted": "PEP-695 syntax requires Python 3.12. CI targets 3.10+. Migrating now would break 3.10 and 3.11 targets. The Protocol API is unchanged so there is no correctness benefit.",
  "whyRejected": "Rejected output misidentifies all Protocol definitions as candidates for PEP-695 migration and ignores the CI version constraint.",
  "sourceAnchors": [
    "https://peps.python.org/pep-0695/",
    "https://peps.python.org/pep-0544/"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-295

```json
{
  "rowId": "EXT-295",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A distributed tracing system produces spans with 200+ attributes per span for a high-throughput service (50k RPS). Storage costs are 3x over budget. Two proposals: (A) tail-based sampling — keep 100% of error traces, sample 1% of success traces. (B) attribute pruning — drop low-value attributes (http.user_agent, network.peer.address) from all spans.",
    "b": "Evaluate using OpenTelemetry Semantic Conventions and the XIIGEN arbiter framework.",
    "c": "SLO alert latency must remain under 30 seconds. Debugging requires correlating across 12 microservices."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Apply both proposals in sequence, prioritised: (1) Tail-based sampling first — reduces volume by ~99x for success paths while preserving 100% of error traces for debugging. This alone likely brings costs within budget. (2) Attribute pruning as a secondary measure — drop `http.user_agent` (browser telemetry, not useful for backend correlation), `network.peer.address` (redundant with service mesh metrics), and other low-signal fields identified per OTel Semantic Conventions stability markings. The 30-second SLO alert latency is not affected by tail sampling because error traces (the SLO trigger) are kept at 100%. Cross-service correlation is preserved via trace_id propagation regardless of sampling rate.",
  "rejectedOutput": "Apply only attribute pruning. Sampling discards data and makes post-hoc debugging impossible for sampled-out traces.",
  "arbiters": ["otel-semantic-conventions", "cost-volume-tradeoff"],
  "whyAccepted": "Tail-based sampling is the highest-leverage cost reduction: 99% volume cut on success paths with zero impact on error-path observability. Attribute pruning provides additional savings but is secondary.",
  "whyRejected": "Attribute pruning alone delivers 10-20% savings at most (dropping a few fields per span). It cannot achieve the 3x cost reduction required.",
  "sourceAnchors": [
    "https://opentelemetry.io/docs/specs/semconv/",
    "https://opentelemetry.io/docs/concepts/sampling/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-296

```json
{
  "rowId": "EXT-296",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Dapper query returns a flat result set joining Orders, OrderLines, and Products. The mapping function uses a `Dictionary<int, Order>` to group lines by OrderId and accumulate them. The existing code calls `connection.Query<Order, OrderLine, Product, Order>()` with a custom `splitOn` parameter.",
    "b": "A junior developer proposes replacing the multi-type Query with three separate queries: one for Orders, one for OrderLines filtered by OrderId list, one for Products filtered by ProductId list — claiming it is 'simpler to understand'.",
    "c": "Average order has 8 lines, 4 distinct products. Batch size is 500 orders."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Retain the multi-mapping Query with splitOn. Three separate queries for 500 orders requires: 1 query for orders + 1 query with 500-item IN clause for lines + 1 query with up to 2000-item IN clause for products. Large IN clauses hit SQL Server parameter limits (2100 params) and may require batching. The single JOIN query is 1 round trip with a deterministic execution plan the optimizer can cache. The multi-mapping splitOn pattern is Dapper's intended solution for exactly this graph-building case. Simplicity argument is valid only if maintainers are unfamiliar with Dapper — the solution is to add a comment explaining splitOn semantics, not to degrade performance.",
  "rejectedOutput": "Split into three queries. The multi-mapping API is hard to read and the separate-query approach is clearer and easier to maintain for the team.",
  "arbiters": ["dapper-multi-mapping", "sql-parameter-limits"],
  "whyAccepted": "Single JOIN query: 1 round trip, parameter-count safe, optimizer-cache friendly. The cost of learning splitOn is a one-time comment; the cost of three queries compounds with every call.",
  "whyRejected": "Three-query approach hits SQL Server 2100-parameter limit for large batches, adds 2 extra round trips, and trades a one-time learning cost for a permanent performance penalty.",
  "sourceAnchors": [
    "https://github.com/DapperLib/Dapper",
    "https://learn.microsoft.com/en-us/sql/sql-server/maximum-capacity-specifications-for-sql-server"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-297

```json
{
  "rowId": "EXT-297",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 application uses RxJS BehaviorSubject for sharing state between a parent component and three child components. A teammate suggests migrating to Angular Signals for the shared state. The parent updates state on user interaction; children subscribe and re-render on change.",
    "b": "Compare: BehaviorSubject (RxJS) vs Angular Signal for local shared state in a component tree with no async side effects.",
    "c": "No HTTP calls involved. State is synchronous. Components use OnPush change detection."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Migrate to Angular Signals. For synchronous local state shared within a component tree, Signals are the Angular 17 idiomatic choice: `signal(initialValue)` produces a reactive primitive, `computed()` derives values, and `effect()` handles side effects — all without RxJS subscription boilerplate. With OnPush change detection, Signals integrate natively: Angular's change detection understands Signal reads and marks only affected views dirty. BehaviorSubject requires `async` pipe or manual subscribe/unsubscribe in each child, adding teardown risk. The migration path is: replace `new BehaviorSubject<T>(v)` with `signal<T>(v)`, replace `.next(v)` with `set(v)`, replace `.pipe(map(...))` with `computed(() => ...)`, remove subscription teardown in ngOnDestroy.",
  "rejectedOutput": "Keep BehaviorSubject. Signals are still experimental and the team is already familiar with RxJS patterns. Consistency is more valuable than adopting new APIs.",
  "arbiters": ["angular-signals-spec", "rxjs-behavioral-subject"],
  "whyAccepted": "Signals are stable in Angular 17 (not experimental), integrate natively with OnPush, and eliminate subscription management entirely for synchronous state.",
  "whyRejected": "Signals were marked experimental in Angular 16 but are stable in 17. Keeping BehaviorSubject for synchronous local state adds unnecessary RxJS overhead and subscription teardown risk.",
  "sourceAnchors": [
    "https://angular.dev/guide/signals",
    "https://angular.dev/guide/components/advanced-configuration#onpush"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-298

```json
{
  "rowId": "EXT-298",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A React 18 application fetches user profile data on page load. Two implementations proposed: (A) useEffect + useState with fetch inside effect, dependency array [userId]. (B) React Query (TanStack Query) with useQuery hook, staleTime 60s.",
    "b": "Evaluate for: correctness (race condition safety), caching behaviour, loading/error state handling, and code volume.",
    "c": "The same profile data is displayed in 3 different components on the same page."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Select React Query (option B). React Query deduplicate the three concurrent useQuery calls for the same userId into a single network request, serves the cached result to all three components, and handles stale-while-revalidate automatically with staleTime 60s. The useEffect approach (option A) requires manual race condition handling with an `ignore` flag or AbortController, manual loading/error state with useState, and no built-in deduplication — meaning 3 components each independently fetch the same endpoint on mount. React Query's useQuery provides isLoading, isError, and data as first-class return values with zero boilerplate. For an endpoint called from 3 components, React Query reduces 3 fetch calls to 1.",
  "rejectedOutput": "Use useEffect + useState. React Query is an external dependency and useEffect is sufficient. Adding a library for a single data-fetching case is over-engineering.",
  "arbiters": ["react-query-deduplication", "use-effect-race-condition"],
  "whyAccepted": "Three simultaneous component mounts mean the useEffect approach fires 3 redundant fetch calls. React Query deduplicates to 1 and caches the result. The 'external dependency' objection is outweighed by the elimination of manual race condition handling for every fetch.",
  "whyRejected": "useEffect + useState requires AbortController race-condition fix, 3x manual loading states, and 3 separate fetch calls for the same data — all avoidable with React Query.",
  "sourceAnchors": [
    "https://tanstack.com/query/latest/docs/framework/react/guides/queries",
    "https://react.dev/reference/react/useEffect"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-299

```json
{
  "rowId": "EXT-299",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B1-RoutingTopology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Add IP-based rate limiting to the ASP.NET Core 8 API — max 100 requests per minute per IP, apply to all endpoints except /health and /metrics.'",
    "b": "Two decomposition strategies: (1) UseRateLimiter middleware with FixedWindowRateLimiter, policy applied globally with RequireRateLimiting, DisableRateLimiting on /health and /metrics. (2) Per-endpoint attribute [EnableRateLimiting(\"fixed\")] applied to every controller action individually.",
    "c": "API has 42 controllers and growing. Health check endpoints are consumed by Kubernetes probes and must never be throttled."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Use strategy 1: global middleware with selective opt-out. Action items: (1) Register FixedWindowRateLimiter in DI with Window=60s, PermitLimit=100, QueueLimit=0 (reject, not queue). (2) Call app.UseRateLimiter() in middleware pipeline after UseRouting. (3) Apply [EnableRateLimiting(\"fixed\")] as a global convention via AddControllers().Conventions or apply the policy name in MapControllers(). (4) Add [DisableRateLimiting] to the /health and /metrics minimal API endpoints. This ensures: all new controllers automatically inherit rate limiting without per-action decoration, health probe endpoints are never throttled, and Kubernetes liveness/readiness probes cannot be blocked.",
  "rejectedOutput": "Apply [EnableRateLimiting(\"fixed\")] to each controller action individually to have explicit control over which endpoints are protected.",
  "arbiters": ["aspnetcore-rate-limiting-middleware", "kubernetes-probe-safety"],
  "whyAccepted": "Global-with-opt-out scales to new controllers automatically. Per-action decoration requires updating 42+ controllers and risks missing new ones, which would leave endpoints unprotected by default.",
  "whyRejected": "Per-action decoration creates an 'allow-by-default' posture for new endpoints and requires discipline across 42 controllers — any missed action is unprotected.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit",
    "https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-300

```json
{
  "rowId": "EXT-300",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS application uses class-validator DTOs (Data Transfer Objects) for all incoming request validation. A developer proposes adding a custom ValidationPipe with `whitelist: true` and `forbidNonWhitelisted: true` globally versus applying it per-controller.",
    "b": "Evaluate: security posture, maintenance overhead, and behaviour for endpoints that legitimately accept dynamic extra fields (e.g., a metadata endpoint accepting arbitrary key-value pairs).",
    "c": "95% of endpoints use strict DTOs. 2 endpoints accept dynamic key-value metadata."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Apply ValidationPipe globally with `whitelist: true, forbidNonWhitelisted: true` as the default, and use `@UsePipes(new ValidationPipe({ whitelist: false }))` on the 2 dynamic-metadata endpoints to opt out. Global application of whitelist stripping prevents mass-assignment attacks across 95% of endpoints without per-controller ceremony. The 2 metadata endpoints are explicit exceptions — their opt-out is visible in code and easy to audit. The alternative (per-controller pipe) means any new endpoint added without a pipe decorator is unprotected by default, creating a security posture that degrades as the codebase grows.",
  "rejectedOutput": "Apply ValidationPipe per-controller to give fine-grained control. Global pipes are too blunt an instrument when some endpoints need different validation behaviour.",
  "arbiters": ["nestjs-validation-pipe", "mass-assignment-cwe-915"],
  "whyAccepted": "Secure-by-default (global whitelist) with explicit opt-out for exceptions is the correct security posture. Auditing 2 opt-out decorators is easier than auditing 42 controllers for missing pipe decorators.",
  "whyRejected": "Per-controller approach is allow-by-default for new endpoints, creating a mass-assignment risk surface that grows with the codebase.",
  "sourceAnchors": [
    "https://docs.nestjs.com/pipes#global-scoped-pipes",
    "https://cwe.mitre.org/data/definitions/915.html"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-301

```json
{
  "rowId": "EXT-301",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C3-OutputFormatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API endpoint returns a paginated list of audit log entries. The response can contain up to 10,000 entries per page. Format options: (A) JSON array in a single response body. (B) chunked transfer encoding (RFC 7230) streaming JSON array. (C) NDJSON (Newline-Delimited JSON) — one JSON object per line streamed via chunked encoding.",
    "b": "Consumers include: a CLI tool that processes entries line-by-line, a dashboard that renders a table on page load, and a compliance report generator that accumulates all entries before processing.",
    "c": "P99 entry count per request is 8,000. Average entry size is 512 bytes (~4 MB per page)."
  },
  "function": "finalMachineResults_to_userFormat",
  "acceptedOutput": "Use NDJSON (option C) streamed via chunked transfer encoding. NDJSON allows the CLI tool to process entries as they arrive without buffering 4 MB, satisfies the dashboard (4 MB JSON parse on page load is acceptable but NDJSON allows progressive rendering), and the compliance generator can accumulate lines trivially. A plain JSON array (option A) requires the server to buffer all 10,000 entries before sending the first byte and forces every client to buffer the full response before parsing. Chunked JSON array (option B) streams bytes but clients still cannot parse individual entries until the closing `]` arrives. NDJSON with chunked encoding gives the best time-to-first-entry for all three consumers and degrades gracefully if the client wants to buffer everything.",
  "rejectedOutput": "Return a standard JSON array. It is the most widely supported format and all three consumers can handle it. NDJSON requires custom parsing logic.",
  "arbiters": ["rfc-7230-chunked-encoding", "ndjson-streaming-spec"],
  "whyAccepted": "NDJSON enables streaming parse for the CLI and progressive render for the dashboard without requiring full-response buffering. Standard JSON parsers handle NDJSON line-by-line trivially.",
  "whyRejected": "Plain JSON array forces 4 MB buffering on server (before first byte sent) and client (before first entry parsed), making the CLI and progressive-render use cases unnecessarily expensive.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc7230#section-4.1",
    "https://ndjson.org/"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-302

```json
{
  "rowId": "EXT-302",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Two API designs for a resource update endpoint: (A) PUT /orders/{id} replaces the entire order resource. (B) PATCH /orders/{id} with a FieldMask body applies partial updates to specified fields only.",
    "b": "Use case: mobile clients update only the delivery_address field of an order. Clients have incomplete knowledge of all order fields (backend adds fields over time).",
    "c": "AIP-134 covers standard Update method. AIP-161 covers field masks."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use PATCH with FieldMask (option B, AIP-134 + AIP-161). Mobile clients updating only delivery_address must not be required to know all current order fields. PUT semantics require sending the complete resource representation — a mobile client omitting a new server-side field would silently overwrite it with null/zero/empty, causing data corruption. PATCH + FieldMask sends only the fields the client intends to change (`update_mask: \"delivery_address\"`) and the server applies only those. This is robust to server-side schema evolution. AIP-134 mandates FieldMask for Update methods precisely for this reason. The field mask also serves as an explicit intent declaration, enabling fine-grained audit logging.",
  "rejectedOutput": "Use PUT. FieldMask introduces API complexity and requires clients to know and send the mask string. PUT is simpler for clients that already fetch the full resource before updating.",
  "arbiters": ["aip-134-update-method", "aip-161-field-masks"],
  "whyAccepted": "PATCH + FieldMask is the only correct choice for partial updates where client has incomplete resource knowledge. PUT causes silent data loss on new fields the client doesn't know about.",
  "whyRejected": "PUT 'fetch then update' pattern requires the client to always have a fresh copy of the resource, adds a read round trip, and is still unsafe if the resource changes between the GET and PUT.",
  "sourceAnchors": [
    "https://google.aip.dev/134",
    "https://google.aip.dev/161"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-303

```json
{
  "rowId": "EXT-303",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C2-ResultIntegration",
  "rowClass": "negative-example",
  "input": {
    "a": "A GPU training node reports: 'CUDA out of memory. Tried to allocate 2.50 GiB (GPU 0; 23.69 GiB total capacity; 18.20 GiB already allocated; 2.16 GiB free; 19.34 GiB reserved by PyTorch using caching allocator).'",
    "b": "Three operator responses: (R1) Increase batch size to utilise the free memory. (R2) Reduce batch size or enable gradient checkpointing. (R3) Restart the training node to clear memory.",
    "c": "Training is at epoch 47 of 100. Checkpoint saved at epoch 45."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Select R2: reduce batch size or enable gradient checkpointing. The OOM (Out of Memory) error occurs because allocation (2.50 GiB) exceeds available free memory (2.16 GiB). Increasing batch size (R1) would request more memory, immediately causing another OOM. Restarting (R3) clears the caching allocator's reserved memory and may temporarily resolve the symptom, but the same OOM will recur at the next large allocation unless batch size or model size is reduced. Gradient checkpointing trades compute for memory by recomputing activations during backward pass — it reduces peak VRAM (Video RAM) usage by up to 60-70% at ~33% compute overhead. Resume from epoch 45 checkpoint with reduced batch size or checkpointing enabled.",
  "rejectedOutput": "Restart the training node to clear the CUDA memory cache. The reserved memory (19.34 GiB) is causing fragmentation that prevents the 2.50 GiB allocation from succeeding.",
  "arbiters": ["cuda-oom-diagnosis", "gradient-checkpointing-spec"],
  "whyAccepted": "Reducing batch size or enabling gradient checkpointing addresses the root cause: peak memory demand exceeds available capacity. The training can resume from epoch 45 with minimal work lost.",
  "whyRejected": "Restart clears fragmentation temporarily but the same allocation request will OOM again at the same point in training. This is a symptom fix, not a root cause fix.",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1",
    "https://pytorch.org/docs/stable/checkpoint.html"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-304

```json
{
  "rowId": "EXT-304",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-SemanticMapping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A translation pipeline processes Tatoeba DE-EN (German-English) sentence pairs. The pre-processing step normalises German umlauts by replacing ä→ae, ö→oe, ü→ue before feeding to a tokeniser that supports full Unicode.",
    "b": "The normalisation was added 'for compatibility' with a legacy tokeniser that was replaced 6 months ago. The current tokeniser is BPE (Byte Pair Encoding) trained on full UTF-8.",
    "c": "Test set BLEU score before normalisation: 42.1. After normalisation: 39.8."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Remove the umlaut normalisation step. The normalisation was a workaround for a legacy tokeniser that no longer exists. The BPE (Byte Pair Encoding) tokeniser is trained on full UTF-8 and handles ä, ö, ü natively. The normalisation is actively harmful: BLEU (Bilingual Evaluation Understudy) score drops 2.3 points because ä→ae conflates distinct German words (e.g., 'Ärger' anger vs 'Aere' — a rare name), reducing translation quality. Removing dead compatibility shims that degrade quality is not a risk; it is correct behaviour. Action items: (1) Remove normalisation function from preprocessing pipeline. (2) Re-run evaluation to confirm BLEU recovery to ≥42.1. (3) Add regression test asserting umlauts reach tokeniser unmodified.",
  "rejectedOutput": "Keep the normalisation for safety. It was added for a reason and removing it may break other consumers of the preprocessing pipeline.",
  "arbiters": ["bleu-metric-regression", "utf8-tokeniser-compatibility"],
  "whyAccepted": "Empirical evidence (BLEU 42.1 vs 39.8) confirms the normalisation degrades quality. The legacy justification no longer applies. Removing it is the correct action.",
  "whyRejected": "Keeping a dead compatibility shim that demonstrably reduces BLEU by 2.3 points to avoid a hypothetical risk is incorrect. The BLEU regression is concrete evidence of harm.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://github.com/google/sentencepiece"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-305

```json
{
  "rowId": "EXT-305",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 component renders a list of 5,000 items. The list uses `v-for` with no virtualisation. The component re-renders whenever the parent updates any reactive state, causing 200ms+ render times.",
    "b": "Options: (A) Add `v-memo` directive to the v-for to skip re-rendering items whose dependencies haven't changed. (B) Implement virtual scrolling with vue-virtual-scroller to render only visible items. (C) Move the list to a child component with `defineProps` and wrap with `v-once`.",
    "c": "Only ~20 items are visible at a time. Items are complex: each has 3 sub-components and event listeners."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Apply virtual scrolling (option B). With 5,000 items each containing 3 sub-components, the DOM (Document Object Model) node count exceeds 15,000 elements. `v-memo` (option A) skips re-render diffing but all 15,000+ DOM nodes still exist and are painted by the browser — scrolling performance remains poor. `v-once` (option C) would make items static and break event listeners. Virtual scrolling renders only ~25-30 visible items (plus a small buffer), keeping the DOM count under 100 nodes regardless of list size. With complex items (3 sub-components + event listeners), the memory and initial render savings are proportionally larger. This is the correct architectural choice; `v-memo` is a micro-optimisation that does not address the root cause (DOM size).",
  "rejectedOutput": "Add v-memo to each list item. This prevents unnecessary re-renders when only the parent's unrelated state changes, fixing the 200ms render time without adding a dependency.",
  "arbiters": ["vue-virtual-scroller", "dom-size-performance"],
  "whyAccepted": "Virtual scrolling reduces DOM nodes from 15,000+ to ~100, addressing both initial render time and scroll performance. v-memo leaves all 15,000 nodes in the DOM.",
  "whyRejected": "v-memo prevents re-renders but does not reduce DOM size. Browser layout and paint costs for 15,000+ nodes remain high regardless of Vue's diffing optimisation.",
  "sourceAnchors": [
    "https://vuejs.org/guide/best-practices/performance.html#virtualize-large-lists",
    "https://github.com/Akryum/vue-virtual-scroller"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-306

```json
{
  "rowId": "EXT-306",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "An EF Core 6 application uses value converters for a `Money` value object: `HasConversion<string>()` serializes Money to a decimal string column. EF Core 8 introduced native JSON column support. Evaluate migrating the Money column from string to JSON column.",
    "b": "Money has two properties: Amount (decimal) and Currency (3-char ISO 4217 code). Current schema: `Money NVARCHAR(20)` storing '19.99:USD'. Proposed: `Money NVARCHAR(MAX)` storing `{\"Amount\":19.99,\"Currency\":\"USD\"}`.",
    "c": "Column is used in ORDER BY and WHERE Amount > 10 queries. Table has 12M rows."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "DEFER JSON column migration. The current string encoding ('19.99:USD') is compact and the Amount component is extractable for range queries via LIKE or computed column. Migrating to JSON NVARCHAR(MAX) has three costs: (1) JSON values are not indexable for range queries in SQL Server without computed columns or JSON_VALUE() in indexes — WHERE Amount > 10 becomes `WHERE CAST(JSON_VALUE(Money, '$.Amount') AS decimal(18,2)) > 10`, which is non-SARGable (non-Search ARGument able) without a specific JSON index. (2) NVARCHAR(MAX) increases per-row storage vs NVARCHAR(20) for 12M rows. (3) Schema migration touching 12M rows requires careful batched execution. The correct EF Core 8 pattern for this use case is an owned entity with `OwnsOne(...).ToTable()` or a custom value converter to separate Amount and Currency into two typed columns — not JSON.",
  "rejectedOutput": "Migrate to JSON column. EF Core 8 JSON columns are the modern approach for value objects and the migration unlocks future JSON path querying.",
  "arbiters": ["efcore-json-column-indexability", "sql-server-json-sargability"],
  "whyAccepted": "JSON column migration degrades Amount range query performance (non-SARGable) on a 12M row table and increases storage for a simple two-field object. The current approach or a two-column owned entity are both better.",
  "whyRejected": "JSON column for a two-property value object with range-query requirements introduces indexability problems that outweigh the EF Core convenience.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#json-columns",
    "https://learn.microsoft.com/en-us/sql/relational-databases/json/json-data-sql-server"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-307

```json
{
  "rowId": "EXT-307",
  "sourceIds": ["HELM-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B2-DeploymentRouting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Configure Helm chart to support multi-environment deployment (dev/staging/prod) with environment-specific values without duplicating the chart.'",
    "b": "Strategies: (A) Single chart + values-dev.yaml, values-staging.yaml, values-prod.yaml overlay files passed via `helm install -f values-prod.yaml`. (B) Helmfile to orchestrate environment-specific deployments declaratively. (C) Separate charts per environment in separate directories.",
    "c": "3 environments. 12 microservices. Prod values differ from staging mainly in replica counts, resource limits, and image tags."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Use strategy A: single chart with environment overlay files. Action items: (1) Create `values.yaml` as the base with dev-appropriate defaults. (2) Create `values-staging.yaml` overriding: replicaCount, resources.limits, HPA (Horizontal Pod Autoscaler) maxReplicas. (3) Create `values-prod.yaml` overriding the same fields plus image.tag pinning and PodDisruptionBudget (PDB) settings. (4) Deploy via `helm upgrade --install myapp ./chart -f values.yaml -f values-prod.yaml` — Helm merges the files with rightmost-wins semantics. Strategy B (Helmfile) adds value for 12+ charts with cross-chart dependencies but is unnecessary overhead for a single chart family. Strategy C (separate charts) creates maintenance burden: changes to chart templates must be applied to 3 copies.",
  "rejectedOutput": "Use separate charts per environment for maximum isolation. Environment-specific logic in a shared chart adds conditional complexity to templates.",
  "arbiters": ["helm-values-overlay", "dry-principle-chart-maintenance"],
  "whyAccepted": "Single chart + overlay files is Helm's intended pattern for multi-environment deployment. Changes to chart logic apply to all environments from one edit. Helmfile adds value at 12+ chart orchestration scale, not for a single chart.",
  "whyRejected": "Separate charts per environment means template bug fixes must be applied 3 times, creating divergence risk. The claimed 'isolation' benefit is a maintenance liability.",
  "sourceAnchors": [
    "https://helm.sh/docs/chart_template_guide/values_files/",
    "https://helmfile.readthedocs.io/en/latest/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-308

```json
{
  "rowId": "EXT-308",
  "sourceIds": ["PROMETHEUS-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Prometheus alerting rule fires `HighErrorRate` when `http_requests_total{status=~'5..'}` exceeds 1% of total requests over 5 minutes. The alert is evaluated every 15 seconds. During a 10-second spike of errors, the alert fires and pages on-call.",
    "b": "The team proposes reducing the evaluation interval to 1 second to catch shorter spikes faster.",
    "c": "Prometheus scrape interval is 15 seconds. Reducing eval interval below scrape interval has no effect on data freshness."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Reject the proposal. Prometheus evaluation interval operates on scraped data — if scrape interval is 15 seconds, evaluation at 1 second re-evaluates the same data points 15 times between scrapes with no new information. The actual fix for spurious 10-second spike alerts is to add a `for: 2m` clause to the alert rule: the condition must hold continuously for 2 minutes before paging. This filters transient spikes while preserving sensitivity to sustained error rates. The anti-pattern is: reducing evaluation interval below scrape interval adds CPU load to the Prometheus server while providing zero additional signal. The correct knob is the `for` duration, not the eval interval.",
  "rejectedOutput": "Reduce evaluation interval to 5 seconds as a compromise. This gives faster detection for real incidents while not going as low as 1 second.",
  "arbiters": ["prometheus-eval-interval-scrape-interval", "alert-for-clause"],
  "whyAccepted": "Prometheus data freshness is bounded by scrape interval (15s). Sub-scrape eval intervals add CPU cost with zero benefit. The `for` clause is the correct mechanism for filtering transient spikes.",
  "whyRejected": "5-second eval interval still re-evaluates stale 15-second-old data 3x per scrape cycle. It does not address the transient spike problem — only the `for` clause does.",
  "sourceAnchors": [
    "https://prometheus.io/docs/prometheus/latest/configuration/alerting_rules/",
    "https://prometheus.io/docs/practices/alerting/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-309

```json
{
  "rowId": "EXT-309",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-RetrainingTrigger",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET 6 microservice uses Polly v7 for resilience: retry (3 attempts, exponential backoff), circuit breaker (5 failures in 30s, 15s open), and timeout (5s per call). .NET 8 introduces Polly v8 (Microsoft.Extensions.Resilience) with a unified ResiliencePipeline API. Evaluate migration.",
    "b": "Polly v8 changes: ResiliencePolicy → ResiliencePipeline, AddRetry/AddCircuitBreaker/AddTimeout on a pipeline builder, native DI integration via AddResiliencePipeline(). The v7 API is maintained but receives only security patches.",
    "c": "Service makes 8 downstream calls, each with individually configured Polly policies. 47 unit tests cover the resilience behaviour."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "Schedule migration to Polly v8 for the next planned sprint, not as emergency work. Justification: Polly v7 receives security patches only — there is no immediate security CVE (Common Vulnerabilities and Exposures) forcing urgent migration. Polly v8 provides native .NET 8 DI integration (`AddResiliencePipeline()`) and telemetry hooks out of the box, but the migration requires updating 8 downstream call configurations and revalidating 47 unit tests. The migration is mechanical (API rename + builder pattern change) but non-trivial due to test coverage. Migration is RECOMMENDED but not urgent. Gate B: all 47 tests pass with Polly v8 API before merging.",
  "rejectedOutput": "Delay migration indefinitely. Polly v7 still works on .NET 8 and the migration risk is not justified when the current implementation is stable.",
  "arbiters": ["polly-v8-migration-guide", "security-patch-only-eol"],
  "whyAccepted": "Polly v7 security-patch-only status means it will eventually stop receiving fixes. Scheduling migration before v7 reaches end-of-life avoids emergency migration under pressure. .NET 8 DI integration also reduces boilerplate.",
  "whyRejected": "Indefinite delay on a security-patch-only library is technical debt accumulation. The migration is known to be mechanical — deferring it adds risk without benefit.",
  "sourceAnchors": [
    "https://github.com/App-vNext/Polly/blob/main/docs/migration-v8.md",
    "https://learn.microsoft.com/en-us/dotnet/core/resilience/"
  ],
  "licenseConstraint": "BSD-3-Clause",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-310

```json
{
  "rowId": "EXT-310",
  "sourceIds": ["SVELTE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Svelte 4 application manages a shopping cart using a Svelte store (`writable([])`). A developer proposes migrating to Svelte 5 runes (`$state`, `$derived`, `$effect`) for the cart state. The cart store is imported by 11 components.",
    "b": "Svelte 5 runes are reactive primitives declared in `.svelte` files or `.svelte.js` modules. The `writable` store API continues to work in Svelte 5 — migration is optional.",
    "c": "11 components use the cart store. 3 use `$store` auto-subscription syntax. 8 use explicit `subscribe()` teardown."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Migrate incrementally: convert the cart store to a `.svelte.js` module using `$state` rune first, update the 3 auto-subscription components (trivial: remove `$` prefix, reference the state directly), then update the 8 explicit-subscribe components (remove subscribe/unsubscribe boilerplate, use the rune state directly). The rune-based approach eliminates explicit `subscribe()` teardown, which is a common source of memory leaks (failing to call unsubscribe in `onDestroy`). In Svelte 5, `$state` in a `.svelte.js` shared module provides the same cross-component reactivity as `writable` with zero boilerplate. The `writable` API is not deprecated — migration is RECOMMENDED for new code and components with explicit subscribe teardown, but not required for the 3 auto-subscription components which already have minimal boilerplate.",
  "rejectedOutput": "Keep the writable store. Svelte 5 runes are a breaking change and migrating 11 components carries risk. The writable store is supported and works correctly.",
  "arbiters": ["svelte5-runes-spec", "memory-leak-subscribe-teardown"],
  "whyAccepted": "The 8 explicit-subscribe components have teardown risk (memory leaks if `onDestroy` is omitted). Runes eliminate this class of bug. The migration is incremental and low-risk since writable stores continue to work during transition.",
  "whyRejected": "Describing runes as a 'breaking change' is incorrect — writable stores continue to work in Svelte 5. The 8 explicit-subscribe components carry ongoing memory leak risk that runes eliminate.",
  "sourceAnchors": [
    "https://svelte.dev/docs/svelte/what-are-runes",
    "https://svelte.dev/docs/svelte/$state"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```
