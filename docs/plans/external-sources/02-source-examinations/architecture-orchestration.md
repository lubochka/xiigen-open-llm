# Source Examination: Architecture and Orchestration
## Sources: KUBERNETES-KEPS, OPENTELEMETRY-SPEC, OPENAPI-SPEC, GOOGLE-AIP, MICROSOFT-LEARN-ARCH
## Priority: 2 (families 1, 2, 5, 8)
## Date examined: 2026-05-04

---

## 1. Sources Examined

| Source ID | Title | Trust tier | Why included |
|-----------|-------|-----------|-------------|
| KUBERNETES-KEPS | Kubernetes Enhancement Proposals | 1 | Apache 2.0; each KEP is a complete proposal cycle: problem → design → graduation criteria → done/deferred |
| OPENTELEMETRY-SPEC | OpenTelemetry Specification | 1 | Apache 2.0; normative spec with MUST/SHOULD requirements and semantic conventions that produce clean arbiter rules |
| OPENAPI-SPEC | OpenAPI Specification 3.1 | 1 | Apache 2.0; API contract specification with versioning, schema, and compatibility requirements |
| GOOGLE-AIP | Google API Improvement Proposals | 1 | CC-BY 4.0; each AIP is a scored decision with rationale and guidance |
| MICROSOFT-LEARN-ARCH | Azure Architecture Patterns (CC-BY 4.0 pages) | 1 | CC-BY 4.0; each pattern page has When to use / When not to use / Considerations — maps directly to accepted/rejected pairs |

---

## 2. Architecture Challenges Found

| Challenge ID | Challenge statement | Source anchor | XIIGEN domain |
|-------------|--------------------|--------------|--------------| 
| ARCH-01 | A new capability needs to be added to an existing service; the team must decide whether to extend in-place or introduce a new microservice | KEP-0 "Kubernetes Enhancement Proposals process" §Graduation criteria; MICROSOFT-LEARN-ARCH Strangler Fig pattern | A_TOPOLOGY |
| ARCH-02 | A service emits events without trace context; a downstream consumer needs to correlate the event with the originating request | OPENTELEMETRY-SPEC specification/trace/sdk.md §Propagation | A_TOPOLOGY |
| ARCH-03 | An API is versioned; a breaking change is needed in a field type; the team must decide between a new major version, a field alias, or deprecation in place | OPENAPI-SPEC 3.1.0 §4.8.24 "Schema Object" deprecation; GOOGLE-AIP-191 "Versioning" | B_ROUTING_SELECTION |
| ARCH-04 | A resource collection needs to support filtering, ordering, and pagination; the API design must decide between cursor-based and offset-based pagination | GOOGLE-AIP-158 "Pagination" | B_ROUTING_SELECTION |
| ARCH-05 | A long-running operation (database migration, report generation) is triggered via API; the caller must not block waiting for it | GOOGLE-AIP-151 "Long-running operations" | A_TOPOLOGY |
| ARCH-06 | A Kubernetes workload sometimes exits with non-zero code during startup; the platform must decide whether to classify it as a crash loop or a slow-start | KEP-1847 "Auto remove PVCs created by StatefulSet" and readiness/liveness probe design | A_TOPOLOGY |
| ARCH-07 | A circuit breaker opens after a downstream service returns 5xx responses; the orchestrator must decide whether to retry, return a cached result, or fail fast | MICROSOFT-LEARN-ARCH Circuit Breaker §When to use this pattern | A_TOPOLOGY |
| ARCH-08 | Multiple independent workloads share a single thread pool; one workload experiences a traffic spike; the arbiter must determine whether bulkhead isolation is needed | MICROSOFT-LEARN-ARCH Bulkhead §Context and problem | A_TOPOLOGY |
| ARCH-09 | An OpenAPI schema has an allOf combinator with conflicting required fields; the validator must decide whether to reject the schema or attempt reconciliation | OPENAPI-SPEC 3.1.0 §4.8.28 "Schema Composition" | B_ROUTING_SELECTION |
| ARCH-10 | A Kubernetes Deployment needs to update without downtime; the team must choose between RollingUpdate, Blue/Green, and Canary strategies | KEP-1860 "Make Kubernetes Node-Lifecycle Controller respect PodDisruptionBudget"; Deployment rolling update spec | A_TOPOLOGY |
| ARCH-11 | A service emits traces, but span attributes use non-standard naming; a downstream tool fails to correlate spans | OPENTELEMETRY-SPEC specification/common/README.md §Attribute naming convention | H_GOVERNANCE |
| ARCH-12 | An API must support a resource that is created idempotently; repeated creation attempts with the same key must not fail | GOOGLE-AIP-133 "Standard methods: Create" §Duplicate requests | D_EXECUTION_APPLIER |
| ARCH-13 | A Kubernetes StatefulSet needs to be scaled down; persistent volumes must not be deleted automatically | KEP-1847 "Auto remove PVCs created by StatefulSet" §Motivation | F_REGISTRY_STORAGE |
| ARCH-14 | An orchestrator receives a dependency cycle from a task graph; it must decide whether to fail, break the cycle heuristically, or surface the error to the caller | MICROSOFT-LEARN-ARCH §Orchestration vs. Choreography; XIIGEN DependencyGraph invariant | A_TOPOLOGY |
| ARCH-15 | An API resource needs to be deprecated; existing callers must be notified without breaking current behavior | GOOGLE-AIP-191 "Versioning" §Deprecation and GOOGLE-AIP-4 "Resource names" | H_GOVERNANCE |

---

## 3. Patterns Found

| Pattern ID | Pattern name | When to use | When NOT to use | Source anchor |
|-----------|-------------|------------|----------------|--------------|
| PAT-ARCH-01 | Circuit breaker with half-open probe | Use when a downstream dependency can become temporarily unavailable and recovery is expected; prevents thundering-herd on recovery | Do not use when failures are expected to be permanent (bad configuration, missing resource) — fail fast instead | MICROSOFT-LEARN-ARCH Circuit Breaker |
| PAT-ARCH-02 | Bulkhead thread pool isolation | Use when multiple workloads share an execution context and one workload's failure must not cascade to others | Do not use when all workloads must share state or when the overhead of pool management exceeds the isolation benefit | MICROSOFT-LEARN-ARCH Bulkhead |
| PAT-ARCH-03 | Cursor-based pagination | Use for large, frequently-modified datasets where items may be added or deleted between pages | Do not use when the caller needs to jump to an arbitrary page by number — use offset pagination for that use case | GOOGLE-AIP-158 |
| PAT-ARCH-04 | Long-running operation resource | Use for any operation that takes more than a few seconds; return an operation resource that the caller polls | Do not use when the operation completes within a single HTTP request-response cycle — return the result directly | GOOGLE-AIP-151 |
| PAT-ARCH-05 | Distributed trace context propagation | Use whenever a request crosses a process boundary; propagate W3C (World Wide Web Consortium) TraceContext headers | Do not invent custom trace ID headers; non-standard headers are invisible to standard observability tooling | OPENTELEMETRY-SPEC §Propagation |
| PAT-ARCH-06 | API versioning by URL path prefix | Use when breaking changes are needed and multiple versions must be served simultaneously | Do not use for non-breaking additions — backward-compatible changes go in the same version | GOOGLE-AIP-191; OPENAPI-SPEC §4.8.24 |
| PAT-ARCH-07 | Strangler Fig for legacy migration | Use when replacing a legacy system incrementally; route new traffic to new implementation, route old traffic to legacy until full migration | Do not use when the legacy system cannot be kept running during migration — requires a full cutover plan instead | MICROSOFT-LEARN-ARCH Strangler Fig |
| PAT-ARCH-08 | Idempotent resource creation | Use for any resource creation that may be retried; use a client-provided idempotency key or resource name | Do not ignore the duplicate request case — it is a required part of any distributed operation that uses retries | GOOGLE-AIP-133 |

---

## 4. Anti-Patterns Found

| Anti-pattern ID | Bad design | Failure mode | Better design | Source anchor |
|----------------|-----------|-------------|--------------|--------------|
| ANTI-ARCH-01 | Shared thread pool for all workloads | One slow or spike workload exhausts all threads; all other workloads stall simultaneously | Assign separate bounded thread pools per workload type; set pool size per expected concurrency level | MICROSOFT-LEARN-ARCH Bulkhead |
| ANTI-ARCH-02 | Synchronous call for long-running operation | The client times out or the connection is dropped before the operation completes; the server finishes the operation but the client never receives the result | Return an operation resource immediately; client polls the operation until done or timeout | GOOGLE-AIP-151 |
| ANTI-ARCH-03 | Non-standard trace ID headers | Downstream tools (Jaeger, Zipkin, cloud providers) do not pick up custom headers; spans from different services are never correlated | Use W3C TraceContext (traceparent header) and W3C Baggage; propagate using the SDK's built-in propagator | OPENTELEMETRY-SPEC §Propagation |
| ANTI-ARCH-04 | Breaking change in same API version | Clients that parsed the old response shape receive unexpected fields or missing required fields; deserialization failures in production | Introduce a new major API version; mark old version deprecated; serve both versions simultaneously during the migration window | OPENAPI-SPEC §4.8.24; GOOGLE-AIP-191 |
| ANTI-ARCH-05 | Missing liveness/readiness probe distinction | Kubernetes restarts a pod that is healthy but not yet ready to serve traffic; traffic is routed to an initializing pod | Use readiness probe to gate traffic routing; use liveness probe only for deadlock/crash detection; separate the two probe types | Kubernetes Deployment spec §probes |
| ANTI-ARCH-06 | Big-bang legacy replacement | All logic moves at once; rollback requires reverting the entire release; production traffic has no fallback path | Use Strangler Fig: migrate one route or feature at a time; run old and new implementations in parallel; rollback is route-level, not service-level | MICROSOFT-LEARN-ARCH Strangler Fig |
| ANTI-ARCH-07 | Offset-based pagination on a live dataset | Items added or deleted between pages cause duplicates or missed items; page N+1 returns items already returned on page N | Use cursor-based pagination with a stable sort key; the cursor encodes position in the ordered result set, not a row offset | GOOGLE-AIP-158 §Offset-based pagination |
| ANTI-ARCH-08 | Repeated creation returning 409 Conflict | The caller cannot distinguish "you already created this resource successfully" from "creation failed because of a conflict" | Return the existing resource with 200 OK when the request is idempotent; only return 409 for a genuine conflict on a different resource | GOOGLE-AIP-133 §Duplicate requests |

---

## 5. Arbiter Rules Extracted

| Arbiter ID | What it checks | Positive signal | Negative signal | Source anchor |
|-----------|--------------|----------------|----------------|--------------|
| circuitBreakerArbiter | A downstream dependency call should be wrapped in a circuit breaker | Dependency has recorded failure rate above threshold; retries are in place; fallback behavior is defined | No circuit breaker present on any external call; direct retry without backoff; no fallback for unavailable dependency | MICROSOFT-LEARN-ARCH Circuit Breaker §When to use |
| bulkheadIsolationArbiter | Workloads are isolated so that one cannot exhaust another's resources | Each distinct workload type has a bounded thread or connection pool; pool size is configured per expected concurrency | All workloads share a single pool; pool exhaustion by one workload visibly delays all others | MICROSOFT-LEARN-ARCH Bulkhead |
| tracePropagationArbiter | Distributed trace context is propagated across all process boundaries | W3C traceparent header present on all outbound requests; SDK propagator is configured; span attributes use OTel semantic conventions | Custom or absent trace headers; spans from upstream and downstream services are not correlated | OPENTELEMETRY-SPEC §Propagation; semantic conventions |
| apiVersioningArbiter | API breaking changes are introduced through a new version, not in-place | Breaking change is in a new major version path (v2/, v3/); old version continues to serve; deprecation is announced with a timeline | Breaking change in existing version path; no migration period; old clients receive unexpected schema | OPENAPI-SPEC §4.8.24; GOOGLE-AIP-191 |
| longRunningOpArbiter | Operations that take more than a few seconds expose a polling resource | POST to initiate returns 200 with an operation resource URL; client polls the operation resource; operation has a status field and a done flag | Long-running operation blocks the HTTP connection; caller must time out and retry from scratch; no operation ID to resume | GOOGLE-AIP-151 |
| idempotentCreateArbiter | Resource creation is safe to retry | Repeated creation with the same idempotency key returns the existing resource; the operation is not applied twice | Second creation attempt returns 409 or creates a duplicate; no idempotency key accepted; retry logic on the caller causes duplicates | GOOGLE-AIP-133 |
| stranglerMigrationArbiter | Legacy migration proceeds incrementally without big-bang cutover | Traffic routing is per-feature or per-route; old implementation is still active during migration; rollback is route-level | Single cutover date with full replacement; no traffic routing split; rollback requires full revert | MICROSOFT-LEARN-ARCH Strangler Fig |
| dependencyCycleArbiter | Task dependency graphs are acyclic before execution begins | Topological sort succeeds; no cycle detected; execution order is deterministic | Cycle detected in task graph; execution stalls or deadlocks waiting for a task that depends on itself | MICROSOFT-LEARN-ARCH §Orchestration; XIIGEN DependencyGraph |

---

## 6. Candidate Training Rows

| Row ID | Dataset family | Domain | Status |
|--------|--------------|--------|-------|
| EXT-ARCH-001 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — circuit breaker arbiter |
| EXT-ARCH-002 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — bulkhead isolation arbiter |
| EXT-ARCH-003 | 5 — arbitrateResultI_with_resultsContext | H_GOVERNANCE | draft — trace propagation arbiter |
| EXT-ARCH-004 | 5 — arbitrateResultI_with_resultsContext | H_GOVERNANCE | draft — API versioning arbiter |
| EXT-ARCH-005 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — long-running op arbiter |
| EXT-ARCH-006 | 5 — arbitrateResultI_with_resultsContext | D_EXECUTION_APPLIER | draft — idempotent create arbiter |
| EXT-ARCH-007 | 8 — historyAndVersions_to_reprocessingDecision | A_TOPOLOGY | draft — strangler migration rollback |
| EXT-ARCH-008 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — dependency cycle detection |
| EXT-ARCH-009 | 1 — userPrompt_to_machineCommands | A_TOPOLOGY | draft — circuit breaker vs retry vs fail-fast decision |
| EXT-ARCH-010 | 2 — machineCommand_to_machineActionItems | A_TOPOLOGY | draft — rolling update strategy decomposition |
| EXT-ARCH-011 | 8 — historyAndVersions_to_reprocessingDecision | H_GOVERNANCE | draft — API deprecation timeline decision |
| EXT-ARCH-012 | 1 — userPrompt_to_machineCommands | B_ROUTING_SELECTION | draft — pagination strategy selection |
| EXT-ARCH-013 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — thread pool exhaustion detection |
| EXT-ARCH-014 | 8 — historyAndVersions_to_reprocessingDecision | A_TOPOLOGY | draft — blue/green vs canary rollback |
| EXT-ARCH-015 | 5 — arbitrateResultI_with_resultsContext | A_TOPOLOGY | draft — offset vs cursor pagination arbiter |

---

## 7. Gaps

This examination does not cover:
- KEP graduation criteria for specific feature areas (scheduling, storage, networking) — needed for domain-specific migration rows
- OPENTELEMETRY-SPEC metrics specification — needed for SLO (Service Level Objective) arbiter rows
- OPENAPI-SPEC webhook and async API sections — needed for event-driven API arbiter rows
- GOOGLE-AIP custom methods (AIP-136) — needed for non-CRUD operation rows
- MICROSOFT-LEARN-ARCH Queue-based Load Leveling and CQRS patterns — covered in architecture-distributed-data.md
