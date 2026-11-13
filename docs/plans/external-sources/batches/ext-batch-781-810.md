# External Sources Batch 27 — EXT-531 through EXT-550

---

## EXT-531 | KUBERNETES-KEPS | Family 1 | Domain A

**Unit title:** Kubernetes StatefulSet vs Deployment for stateful workload topology decisions
**Source URL:** https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT531-F1-A",
  "sourceIds": ["EXT-531"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A distributed caching layer needs 3 Redis nodes with stable network identities so cluster nodes can discover each other after restarts. The team asks whether to use Deployment or StatefulSet.",
    "b": "StatefulSet provides stable pod names (redis-0, redis-1, redis-2), stable DNS entries via headless Service, and ordered pod creation/deletion. Deployment provides no ordering or identity guarantees.",
    "c": "Redis Cluster requires each node to know its own stable address to maintain cluster membership. A Deployment would assign ephemeral pod names and IPs, breaking cluster topology on any restart."
  },
  "function": "Select workload controller for stateful distributed system",
  "acceptedOutput": "Use StatefulSet with a headless Service (clusterIP: None). The stable DNS entries (redis-0.redis-headless.default.svc.cluster.local) allow Redis Cluster to maintain membership topology across restarts. Set podManagementPolicy: Parallel only if ordered startup is not required for initialization.",
  "rejectedOutput": "Use Deployment with 3 replicas and a ClusterIP Service. Redis nodes can re-discover each other via the Service IP after restarts.",
  "arbiters": ["topology-arbiter", "state-management-arbiter"],
  "whyAccepted": "StatefulSet provides the stable network identity that Redis Cluster requires for membership tracking. Headless Service gives per-pod DNS entries that survive restarts with the same address.",
  "whyRejected": "Deployment assigns random pod names and IPs on restart. Redis Cluster nodes lose membership records and cannot rejoin the cluster, causing split-brain or full cluster failure.",
  "sourceAnchors": ["https://kubernetes.io/docs/concepts/workloads/controllers/statefulset/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-532 | BIGCODEBENCH | Family 3 | Domain D

**Unit title:** Python itertools.groupby requires pre-sorting — common grouping anti-pattern
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT532-F3-D",
  "sourceIds": ["EXT-532"],
  "sourceTrustTier": 2,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "iteration-semantics",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A developer uses itertools.groupby(records, key=lambda r: r['department']) on an unsorted list expecting all records per department to be grouped together.",
    "b": "itertools.groupby groups only consecutive elements with the same key. If the list is not sorted by the grouping key, the same department appears in multiple groups.",
    "c": "The developer observes that some departments appear twice in the result with split record sets, causing under-counts in aggregate reports."
  },
  "function": "Detect groupby pre-sort requirement violation",
  "acceptedOutput": "Sort before grouping: sorted_records = sorted(records, key=lambda r: r['department']); for dept, group in itertools.groupby(sorted_records, key=lambda r: r['department']). Alternatively, use collections.defaultdict(list) if order is irrelevant and a single pass is preferred.",
  "rejectedOutput": "Apply itertools.groupby directly on the unsorted list. The groupby function handles grouping internally without needing prior sorting.",
  "arbiters": ["execution-correctness-arbiter"],
  "whyAccepted": "groupby is a streaming iterator that compares only adjacent elements. Pre-sorting ensures all identical keys are consecutive, producing correct single groups per key.",
  "whyRejected": "groupby does not sort internally. Applying it to an unsorted list produces multiple incomplete groups for the same key, silently corrupting aggregation results.",
  "sourceAnchors": ["https://docs.python.org/3/library/itertools.html#itertools.groupby"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-533 | SWE-BENCH | Family 8 | Domain G

**Unit title:** pytest fixture scope mismatch — function-scoped fixture inside session-scoped fixture
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT533-F8-G",
  "sourceIds": ["EXT-533"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "test-infrastructure-learning",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A test suite has a session-scoped database fixture that calls a function-scoped user fixture. pytest raises ScopeMismatch: function-scoped fixture 'user' cannot be used in session-scoped fixture 'db_session'.",
    "b": "pytest fixture scopes: function (default, per test), class, module, package, session (once per run). A wider-scoped fixture cannot depend on a narrower-scoped one because the narrower fixture's lifetime is shorter.",
    "c": "The team needs the database connection to persist across tests (session scope) but wants fresh user data per test (function scope)."
  },
  "function": "Detect and resolve pytest fixture scope mismatch in test pipeline",
  "acceptedOutput": "Separate concerns: keep db_session at session scope for the connection only. Create a function-scoped db fixture that receives db_session and manages per-test transaction rollback. The user fixture uses db (function scope) to insert and clean up test users per test.",
  "rejectedOutput": "Elevate the user fixture to session scope so it matches db_session. This removes the ScopeMismatch error.",
  "arbiters": ["test-quality-arbiter", "learning-pipeline-arbiter"],
  "whyAccepted": "Separating connection lifetime (session) from data lifetime (function) via transaction rollback gives test isolation without reconnection overhead. Each test sees a clean state while sharing the connection pool.",
  "whyRejected": "Session-scoped user fixture creates shared mutable state across tests. Tests that modify the user record corrupt each other's preconditions, causing order-dependent failures.",
  "sourceAnchors": ["https://docs.pytest.org/en/stable/how-to/fixtures.html#fixture-scopes"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-534 | PYTHON-PEPS | Family 3 | Domain D

**Unit title:** PEP-563 postponed evaluation of annotations vs PEP-649 lazy evaluation trade-offs
**Source URL:** https://peps.python.org/pep-0563/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT534-F3-D",
  "sourceIds": ["EXT-534"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "annotation-evaluation-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Python 3.12 library uses from __future__ import annotations (PEP-563) for forward references and performance. Users report that runtime annotation inspection via typing.get_type_hints() breaks with NameError for locally defined types.",
    "b": "PEP-563 stores all annotations as strings (lazy string). Runtime inspection requires eval() in the correct namespace, which fails for types defined in local scopes. PEP-649 (accepted for Python 3.14) uses lazy descriptors that evaluate annotations on access with access to the defining scope.",
    "c": "Pydantic, dataclasses, and attrs use get_type_hints() at class construction time. PEP-563 breaks these when local-scope types are used as annotations."
  },
  "function": "Select annotation evaluation strategy for runtime-introspectable library",
  "acceptedOutput": "Remove from __future__ import annotations in library code that uses runtime introspection. Use string literals ('MyType') only for specific forward references. Document that PEP-649 in Python 3.14 will restore lazy evaluation with correct scoping semantics.",
  "rejectedOutput": "Keep from __future__ import annotations and wrap all get_type_hints() calls in try/except to suppress NameError failures silently.",
  "arbiters": ["compatibility-arbiter", "execution-applier-arbiter"],
  "whyAccepted": "Removing the future import preserves runtime introspection correctness for Pydantic and dataclasses. String literals handle the specific forward-reference cases without breaking the entire module.",
  "whyRejected": "Suppressing NameError hides real type errors. Models with unresolvable annotations silently have no validation, defeating the purpose of runtime type checking.",
  "sourceAnchors": ["https://peps.python.org/pep-0563/", "https://peps.python.org/pep-0649/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-535 | OPENTELEMETRY-SPEC | Family 5 | Domain E

**Unit title:** OTel sampling strategies — head-based vs tail-based sampling for high-volume services
**Source URL:** https://opentelemetry.io/docs/concepts/sampling/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT535-F5-E",
  "sourceIds": ["EXT-535"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "sampling-strategy-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A payment service processes 50,000 requests/second. Head-based sampling at 1% is configured. The SRE team discovers that error traces (0.01% of traffic) are nearly never captured, making incident diagnosis impossible.",
    "b": "Head-based sampling decides at trace start before outcome is known. Tail-based sampling buffers complete traces and samples based on outcome (e.g., always keep errors, sample success at 1%).",
    "c": "The Collector supports tail-based sampling via the tail_sampling processor. It requires buffering traces in memory for the tail_latency window (e.g., 10s) before the sampling decision."
  },
  "function": "Arbitrate sampling strategy for error-visibility vs cost trade-off",
  "acceptedOutput": "Implement tail-based sampling in the OTel Collector: always sample traces with error status codes (status.code == ERROR), HTTP 5xx, or latency > P99 threshold. Apply 1% probabilistic sampling to successful fast traces. Accept increased Collector memory usage as the cost for error completeness.",
  "rejectedOutput": "Increase head-based sampling rate to 10% across all traces to capture more errors by volume.",
  "arbiters": ["observability-arbiter", "cost-quality-arbiter"],
  "whyAccepted": "Tail-based sampling retains 100% of error traces (highest diagnostic value) while controlling storage costs by downsampling healthy traces. Error capture rate goes from ~0.01% to ~100% of actual errors.",
  "whyRejected": "10× head-based sampling increases storage cost 10× while still randomly dropping 90% of errors. A service with 0.01% error rate at 10% sampling still misses 89.999% of errors.",
  "sourceAnchors": ["https://opentelemetry.io/docs/concepts/sampling/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-536 | DAPPER | Family 6 | Domain F

**Unit title:** Dapper buffered vs unbuffered queries for large result set memory management
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT536-F6-F",
  "sourceIds": ["EXT-536"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "query-execution-mode",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A report query returns 2 million rows. Using connection.Query<ReportRow>(sql) causes an OutOfMemoryException on the application server. The team needs to process rows as they stream from the database.",
    "b": "Dapper's Query<T> is buffered by default — it reads the entire result set into a List<T> before returning. Query<T>(sql, buffered: false) returns IEnumerable<T> that streams rows one at a time, keeping the connection open until enumeration completes.",
    "c": "Unbuffered queries require the connection to stay open for the full enumeration duration. Callers must enumerate completely or explicitly dispose the connection."
  },
  "function": "Select query buffering mode for large result set processing",
  "acceptedOutput": "Use connection.Query<ReportRow>(sql, buffered: false) inside a using block. Process each row immediately (write to stream, aggregate, or batch-insert elsewhere) without materializing the full list. Ensure the IEnumerable is fully consumed before the connection closes.",
  "rejectedOutput": "Increase the application server memory limit to 8 GB to accommodate the full 2M-row buffer.",
  "arbiters": ["registry-storage-arbiter", "resource-efficiency-arbiter"],
  "whyAccepted": "Unbuffered streaming processes rows at O(1) memory regardless of result set size. The database cursor manages pagination server-side, and the application processes and discards each row.",
  "whyRejected": "Increasing memory handles today's 2M rows but fails at 4M. It masks a design problem and increases infrastructure cost proportionally to data growth.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#buffered-vs-unbuffered-readers"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-537 | ANGULAR-DOCS | Family 5 | Domain E

**Unit title:** Angular 17 signal-based computed() vs derived state in RxJS for reactive arbitration
**Source URL:** https://angular.dev/guide/signals
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT537-F5-E",
  "sourceIds": ["EXT-537"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "reactive-state-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 component needs to display a filtered and sorted product list derived from two signals: searchTerm and sortOrder. The team debates whether to use computed() signals or combineLatest with RxJS operators.",
    "b": "computed() automatically tracks signal dependencies and re-evaluates synchronously when any dependency changes. It integrates natively with Angular's new signal-based change detection (no Zone.js needed). combineLatest requires Observable subscriptions and manual unsubscription.",
    "c": "The component uses no Observables for its local state — all inputs are signals. Introducing RxJS would add subscription management complexity for a purely synchronous derivation."
  },
  "function": "Arbitrate reactive derivation strategy in signal-based Angular component",
  "acceptedOutput": "Use computed(() => products().filter(p => p.name.includes(searchTerm())).sort(sortFn(sortOrder()))). Angular's reactivity graph tracks both signal dependencies automatically. No subscription, no cleanup required. Change detection fires only when searchTerm or sortOrder actually changes.",
  "rejectedOutput": "Use combineLatest([searchTerm$, sortOrder$]).pipe(map(([term, order]) => ...)) to derive the filtered list, subscribing with async pipe in the template.",
  "arbiters": ["reactivity-arbiter", "complexity-cost-arbiter"],
  "whyAccepted": "computed() is the native derivation primitive for signal graphs. It composes synchronously, requires no cleanup, and integrates with Angular 17's zoneless change detection. The derivation logic is identical in complexity.",
  "whyRejected": "Converting signals to Observables solely to use combineLatest adds two signal-to-Observable bridges, subscription management, and Zone.js dependency for a synchronous operation. It mixes two reactive paradigms unnecessarily.",
  "sourceAnchors": ["https://angular.dev/guide/signals#computed-signals"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-538 | REACT-ADVANCED | Family 1 | Domain A

**Unit title:** React concurrent features — useDeferredValue vs useTransition for UI responsiveness topology
**Source URL:** https://react.dev/reference/react/useDeferredValue
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT538-F1-A",
  "sourceIds": ["EXT-538"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "concurrent-rendering-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A search component renders a large results list that causes UI jank when the user types. The team considers useDeferredValue on the query string vs useTransition wrapping the state update.",
    "b": "useDeferredValue accepts a value and returns a deferred version that lags behind when the component is busy. It is used when you receive a value you cannot control (e.g., from a parent prop). useTransition wraps state setter calls you own, marking them as non-urgent.",
    "c": "The search query state is owned by the current component (useState). The results list renders based on that state."
  },
  "function": "Select concurrent React primitive for typing responsiveness",
  "acceptedOutput": "Use useTransition: const [isPending, startTransition] = useTransition(); wrap the setQuery call: startTransition(() => setQuery(e.target.value)). Render a loading indicator using isPending. The input stays responsive because the query update is marked non-urgent.",
  "rejectedOutput": "Use useDeferredValue on the query: const deferredQuery = useDeferredValue(query). Pass deferredQuery to the results list.",
  "arbiters": ["ui-topology-arbiter", "responsiveness-arbiter"],
  "whyAccepted": "useTransition is the correct primitive when you own the state setter. It marks the state update as non-urgent and provides isPending for loading UI. The input field updates immediately while the results re-render deferred.",
  "whyRejected": "useDeferredValue is for values received from outside (props, context). Using it on state you own bypasses the transition's isPending signal and is semantically incorrect — though it works, it provides no pending indicator.",
  "sourceAnchors": ["https://react.dev/reference/react/useDeferredValue", "https://react.dev/reference/react/useTransition"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-539 | ASPNETCORE-DOCS-SAMPLES | Family 7 | Domain C

**Unit title:** ASP.NET Core OpenAPI (Swashbuckle vs Microsoft.AspNetCore.OpenApi) in .NET 9
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT539-F7-C",
  "sourceIds": ["EXT-539"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "api-documentation-translation",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET 9 minimal API project currently uses Swashbuckle.AspNetCore for OpenAPI document generation. The team evaluates migrating to Microsoft.AspNetCore.OpenApi (the new built-in package).",
    "b": "Microsoft.AspNetCore.OpenApi generates OpenAPI documents natively without reflection-heavy scanning. It supports document transformers and schema transformers via IOpenApiDocumentTransformer. It does not include a built-in Swagger UI (requires scalar or redoc separately).",
    "c": "Swashbuckle provides Swagger UI out of the box and has a large ecosystem of extension packages (annotations, filters). The built-in package has less ecosystem depth but tighter integration with ASP.NET Core route metadata."
  },
  "function": "Translate migration decision criteria for OpenAPI provider switch",
  "acceptedOutput": "Migrate to Microsoft.AspNetCore.OpenApi for new .NET 9 projects: add app.MapOpenApi() and use Scalar.AspNetCore for UI. The built-in package has first-party support guarantees and generates schemas directly from route endpoint metadata without Swashbuckle's reflection overhead. Keep Swashbuckle for projects requiring its annotation ecosystem.",
  "rejectedOutput": "Continue using Swashbuckle in all .NET 9 projects. The built-in package is missing Swagger UI and annotation support, making it incomplete.",
  "arbiters": ["api-translation-arbiter", "migration-readiness-arbiter"],
  "whyAccepted": "Microsoft.AspNetCore.OpenApi is the long-term direction for ASP.NET Core. For minimal API projects, its route-metadata-based schema generation is more accurate than Swashbuckle's reflection scanning. Scalar provides a comparable UI experience.",
  "whyRejected": "Claiming incompleteness ignores that Swagger UI is a separate concern replaceable by Scalar or Redoc. Avoiding migration delays adoption of a package with guaranteed first-party maintenance.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-540 | NESTJS-ADVANCED | Family 5 | Domain E

**Unit title:** NestJS custom exception filters vs global exception filter for error arbitration
**Source URL:** https://docs.nestjs.com/exception-filters
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT540-F5-E",
  "sourceIds": ["EXT-540"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "error-arbitration-layer",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS API needs to: (1) return RFC-9457 Problem Details format for all HTTP errors, (2) send domain-specific errors from a payments module to a separate error tracking queue, (3) suppress internal error details in production responses.",
    "b": "A global exception filter registered via app.useGlobalFilters() catches all unhandled exceptions. Module-specific filters registered via @UseFilters() on controllers override or extend the global filter for specific scopes.",
    "c": "The team has a single AllExceptionsFilter that handles cases 1 and 3 but needs payments-specific routing without polluting the global filter."
  },
  "function": "Arbitrate exception filter layering for multi-concern error handling",
  "acceptedOutput": "Register AllExceptionsFilter globally for Problem Details formatting and detail suppression. Create PaymentsExceptionFilter extending AllExceptionsFilter, adding queue publication logic, and apply it with @UseFilters(PaymentsExceptionFilter) on the PaymentsController. The module filter runs first; unhandled cases bubble to the global filter.",
  "rejectedOutput": "Add payments queue logic directly into AllExceptionsFilter with a module-type check: if (error instanceof PaymentsException) { await queue.publish(error); }.",
  "arbiters": ["error-arbitration-arbiter", "module-isolation-arbiter"],
  "whyAccepted": "Layered filters keep cross-cutting concerns (formatting, suppression) in the global filter while module-specific behavior stays scoped. The payments filter is independently testable and does not require the global filter to know about payments logic.",
  "whyRejected": "Adding instanceof checks in a global filter violates the open/closed principle. Every new module requiring special error handling requires modifying the global filter, increasing its complexity and test surface indefinitely.",
  "sourceAnchors": ["https://docs.nestjs.com/exception-filters"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-541 | IETF-RFCS | Family 7 | Domain C

**Unit title:** RFC-8446 TLS 1.3 vs TLS 1.2 — handshake round-trip reduction and 0-RTT trade-offs
**Source URL:** https://www.rfc-editor.org/rfc/rfc8446
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT541-F7-C",
  "sourceIds": ["EXT-541"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "tls-protocol-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A high-frequency trading API uses TLS 1.2 with session resumption. The team evaluates upgrading to TLS 1.3 and enabling 0-RTT (Zero Round Trip Time) resumption for latency-sensitive reconnections.",
    "b": "TLS 1.3 reduces the full handshake to 1-RTT (from 2-RTT in TLS 1.2) by moving cipher negotiation to the first message. 0-RTT allows clients to send data in the first flight using a pre-shared key, reducing to 0-RTT for resumed sessions. 0-RTT data is not forward-secret and is vulnerable to replay attacks.",
    "c": "The API endpoints receiving 0-RTT data must be idempotent — RFC-8446 Section 8 explicitly warns that 0-RTT does not prevent replay. Non-idempotent endpoints (order submission) receiving replayed 0-RTT data could execute duplicate trades."
  },
  "function": "Translate TLS 1.3 0-RTT trade-off for latency vs security decision",
  "acceptedOutput": "Enable TLS 1.3 (1-RTT improvement is safe). Disable 0-RTT for all mutation endpoints (order submission, cancellation). Enable 0-RTT only for read-only endpoints (market data queries) that are naturally idempotent. Implement server-side anti-replay via the session ticket age check.",
  "rejectedOutput": "Enable TLS 1.3 with 0-RTT for all endpoints including order submission. The performance gain justifies the risk for a trusted API with authenticated clients.",
  "arbiters": ["security-arbiter", "protocol-translation-arbiter"],
  "whyAccepted": "1-RTT TLS 1.3 provides latency improvement without replay risk. Selective 0-RTT on idempotent endpoints captures additional gains while protecting mutation endpoints from replay-based duplicate execution.",
  "whyRejected": "RFC-8446 Section 8 explicitly states 0-RTT cannot prevent replay. A replayed order submission would execute a duplicate trade. Client authentication does not prevent replay of legitimate authenticated requests.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc8446#section-8"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-542 | GOOGLE-AIP | Family 7 | Domain C

**Unit title:** AIP-148 field masks for partial resource updates vs full resource replacement
**Source URL:** https://google.aip.dev/148
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT542-F7-C",
  "sourceIds": ["EXT-542"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "api-update-semantics-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A gRPC API uses UpdateUser with a full UserResource message. Mobile clients updating only a user's display_name must send the entire resource including fields they did not intend to change, causing accidental overwrites of fields changed concurrently by other clients.",
    "b": "AIP-148 specifies UpdateUserRequest with a FieldMask update_mask field. Only fields listed in update_mask are updated; unlisted fields retain their current values. The server applies only the masked fields to the stored resource.",
    "c": "Without a field mask, a mobile client that reads a stale resource and sends it back with one field changed will overwrite any concurrent changes to other fields made by other clients between the read and write."
  },
  "function": "Translate field mask pattern for partial update API design",
  "acceptedOutput": "Implement UpdateUserRequest with FieldMask update_mask. Clients send only the fields they intend to change and list them in update_mask. Server-side: apply only masked fields to the stored entity. Validate that update_mask contains only valid field paths for the resource.",
  "rejectedOutput": "Use HTTP PATCH with JSON Merge Patch (RFC-7396) instead of field masks, sending only the changed fields in the request body.",
  "arbiters": ["api-translation-arbiter", "concurrency-safety-arbiter"],
  "whyAccepted": "AIP-148 field masks are the canonical pattern for gRPC partial updates. They explicitly declare intent, preventing accidental overwrites. The mask is type-safe and can be validated against the proto schema.",
  "whyRejected": "JSON Merge Patch is HTTP-specific and not applicable to gRPC. It also cannot distinguish between 'set field to null' and 'don't change field' without field mask semantics.",
  "sourceAnchors": ["https://google.aip.dev/148"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-543 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF (Reinforcement Learning from Human Feedback) label noise: annotator disagreement handling
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT543-F8-G",
  "sourceIds": ["EXT-543"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "reward-model-training-quality",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A reward model training dataset has pairs where 3 annotators rated response A vs B. In 30% of pairs, annotators disagree (2-1 splits). The team debates whether to drop disagreement pairs, use majority vote, or model annotator uncertainty.",
    "b": "Majority vote (2-1 → A wins) treats the losing annotator's signal as noise. Dropping all disagreement pairs loses 30% of data. Soft labels (e.g., 0.67 confidence for A) preserve the uncertainty signal and can be used with a distributional loss function.",
    "c": "The oasst1 dataset provides per-annotator scores and inter-annotator agreement metrics. High-disagreement pairs often correspond to cases where quality criteria are genuinely ambiguous (e.g., conciseness vs completeness trade-offs)."
  },
  "function": "Select label aggregation strategy for RLHF reward model with annotator disagreement",
  "acceptedOutput": "Use soft labels: compute confidence as (votes_for_A / total_votes). Train the reward model with a KL divergence loss against the soft label distribution rather than binary cross-entropy. This preserves the uncertainty signal and prevents overconfident reward models on ambiguous pairs.",
  "rejectedOutput": "Apply majority vote to all pairs. 2-1 splits resolve to the majority label. Drop pairs with exactly 50-50 disagreement if any exist.",
  "arbiters": ["learning-quality-arbiter", "label-calibration-arbiter"],
  "whyAccepted": "Soft labels encode genuine human disagreement as a calibration signal. Reward models trained on soft labels produce better-calibrated scores on ambiguous pairs, reducing over-optimization in RLHF loops.",
  "whyRejected": "Majority vote discards the minority annotator's information. For genuinely ambiguous pairs, the 'correct' answer may not exist — forcing a binary label trains the model to be overconfident on cases where humans themselves are uncertain.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-544 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba sentence alignment quality — detecting translation drift in long sentences
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT544-F2-C",
  "sourceIds": ["EXT-544"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "translation-alignment-quality",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A translation pipeline using Tatoeba EN-FR pairs discovers that sentences longer than 25 tokens have significantly lower COMET scores on held-out evaluation despite passing word-count parity filters.",
    "b": "Translation drift occurs when a translator rephrases, omits clauses, or adds explanatory content rather than translating directly. Short sentences have fewer opportunities for drift. Word-count parity filters (accepting pairs within ±50% length ratio) miss drift in long sentences where both the source and paraphrase can have similar word counts.",
    "c": "COMET (Crosslingual Optimized Metric for Evaluation of Translation) is a neural evaluation metric that scores semantic similarity between source, hypothesis, and reference using multilingual embeddings."
  },
  "function": "Detect translation drift in long-sentence alignment from Tatoeba",
  "acceptedOutput": "Apply COMET-DA (direct assessment) scoring to all pairs with source length > 20 tokens. Filter out pairs with COMET < 0.75. Additionally, use ChrF++ at the sentence level as a complementary lexical check. Long pairs surviving both filters have acceptable alignment quality for training.",
  "rejectedOutput": "Rely on word-count parity (source_length / target_length between 0.5 and 2.0) as the sole quality gate for all sentence lengths.",
  "arbiters": ["translation-quality-arbiter", "alignment-filter-arbiter"],
  "whyAccepted": "COMET captures semantic drift that lexical length ratios miss. Long sentences with paraphrased translations pass length filters but score low on COMET due to missing clauses or added explanations not in the original.",
  "whyRejected": "Word-count parity detects gross length mismatches only. A 30-token sentence translated as a 28-token paraphrase passes the parity filter but may omit a negation or reverse the meaning of a subordinate clause.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-545 | VUE-DOCS | Family 3 | Domain D

**Unit title:** Vue 3 watchEffect vs watch for reactive side effect execution strategy
**Source URL:** https://vuejs.org/guide/essentials/watchers
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT545-F3-D",
  "sourceIds": ["EXT-545"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "reactive-watcher-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 component needs to fetch data whenever userId or filters (a reactive object) change. The team debates between watchEffect(() => fetchData(userId.value, filters)) and watch([userId, filters], ([newId, newFilters]) => fetchData(newId, newFilters)).",
    "b": "watchEffect automatically tracks reactive dependencies accessed during its synchronous execution and re-runs when any change. watch requires explicit source declaration and provides old/new values. watchEffect runs immediately on setup; watch runs only on change by default.",
    "c": "The fetch function accesses userId.value and filters.someField — watchEffect would track all accessed reactive properties including nested ones of filters."
  },
  "function": "Select reactive watcher primitive for data fetch side effect",
  "acceptedOutput": "Use watch([userId, filters], fetchData, { immediate: true }) for explicit dependency declaration. This makes dependencies visible in code review, provides old values if needed to compare and cancel previous requests, and avoids tracking unintended reactive properties accessed inside fetchData's implementation.",
  "rejectedOutput": "Use watchEffect(() => { fetchData(userId.value, filters) }) for automatic dependency tracking.",
  "arbiters": ["execution-applier-arbiter", "reactive-dependency-arbiter"],
  "whyAccepted": "Explicit watch sources prevent accidental over-triggering if fetchData internally accesses other reactive state. Old/new values enable request cancellation patterns (cancel previous AbortController on each trigger). Dependencies are readable without tracing the function body.",
  "whyRejected": "watchEffect's implicit tracking is convenient but fragile: if fetchData later accesses a reactive loading flag or other state, it creates an unintended re-fetch loop. Dependency tracking becomes invisible to future maintainers.",
  "sourceAnchors": ["https://vuejs.org/guide/essentials/watchers"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-546 | EFCORE-SOURCE | Family 6 | Domain F

**Unit title:** EF Core split queries vs single query for Include() with collection navigations
**Source URL:** https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT546-F6-F",
  "sourceIds": ["EXT-546"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "query-execution-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An EF Core query loads Orders with Include(o => o.Items).Include(o => o.Payments). With 1000 orders averaging 10 items and 3 payments each, the single query produces a Cartesian product of 30,000 rows (1000 × 10 × 3) causing high memory pressure and slow query plans.",
    "b": "EF Core split queries (AsSplitQuery()) execute one SQL query per Include, then merge results in memory. This eliminates the Cartesian product at the cost of multiple round-trips. EF Core 5+ makes AsSplitQuery() available per-query or as a global default.",
    "c": "Split queries read data in separate statements without a transaction by default, creating a potential for inconsistency if records change between the queries. UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery) sets the global default."
  },
  "function": "Select EF Core query splitting strategy for collection navigation performance",
  "acceptedOutput": "Use AsSplitQuery() for queries with multiple collection Includes that produce large Cartesian products. Wrap in a transaction if consistency between the split reads is required. Set the global default for applications with predominantly collection-heavy queries: options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery).",
  "rejectedOutput": "Keep the single query. Use pagination (.Take(100)) to reduce the result set size and avoid the Cartesian product problem.",
  "arbiters": ["registry-storage-arbiter", "query-performance-arbiter"],
  "whyAccepted": "Split queries reduce a 30,000-row result to three queries of 1,000, 10,000, and 3,000 rows respectively — each independently optimized by the query planner. Memory pressure drops proportionally.",
  "whyRejected": "Pagination addresses data volume but not the Cartesian product. Even loading 100 orders with 10 items and 3 payments per order produces 3,000 rows. The fundamental multiplication is not solved by limiting the primary entity count.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/querying/single-split-queries"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-547 | DOTNET-MICROSERVICES | Family 5 | Domain E

**Unit title:** .NET Circuit Breaker pattern — Polly v8 ResiliencePipeline vs manual state machine
**Source URL:** https://learn.microsoft.com/en-us/dotnet/architecture/microservices/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT547-F5-E",
  "sourceIds": ["EXT-547"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "resilience-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservice calls an external payment gateway. Under load, the gateway becomes unavailable. Without circuit breaking, each request waits for the timeout (30s), exhausting the thread pool and causing cascading failures across dependent services.",
    "b": "Polly v8 ResiliencePipeline with AddCircuitBreaker() implements the three-state circuit breaker (Closed → Open → Half-Open). In Open state, calls fail immediately without attempting the network request. After a sampling window, it transitions to Half-Open to test recovery.",
    "c": "A manual state machine would require thread-safe state transitions, a timer for the Open→Half-Open delay, and retry coordination — all solved problems in Polly."
  },
  "function": "Arbitrate circuit breaker implementation for cascading failure prevention",
  "acceptedOutput": "Use Polly v8 ResiliencePipeline: services.AddResiliencePipeline(\"payment-gateway\", builder => builder.AddCircuitBreaker(new CircuitBreakerStrategyOptions { FailureRatio = 0.5, MinimumThroughput = 10, SamplingDuration = TimeSpan.FromSeconds(30), BreakDuration = TimeSpan.FromSeconds(60) })). Inject IResiliencePipelineProvider<string> and execute calls through the pipeline.",
  "rejectedOutput": "Implement a manual circuit breaker using a static volatile bool isOpen field and a Timer to reset it after 60 seconds.",
  "arbiters": ["resilience-arbiter", "dependency-isolation-arbiter"],
  "whyAccepted": "Polly v8 provides thread-safe state transitions, configurable failure ratio sampling, half-open probe requests, and integration with .NET metrics (System.Diagnostics.Metrics). It handles race conditions in state transitions that manual implementations frequently get wrong.",
  "whyRejected": "A static volatile bool is not a circuit breaker — it's a flag. It has no half-open state, no failure ratio calculation, no sampling window, and no thread-safe transition logic. Under concurrent load, multiple threads can simultaneously test a 'recovering' dependency.",
  "sourceAnchors": ["https://www.pollydocs.org/strategies/circuit-breaker"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-548 | MICROSOFT-LEARN-ARCH | Family 1 | Domain A

**Unit title:** Azure Event Hubs vs Azure Service Bus for event streaming topology selection
**Source URL:** https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-about
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT548-F1-A",
  "sourceIds": ["EXT-548"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "messaging-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A telemetry ingestion system needs to process 100,000 events/second from IoT devices. Multiple consumer groups (analytics, alerting, archival) must independently read all events. Messages must be retained for 7 days for replay. The team evaluates Azure Event Hubs vs Azure Service Bus.",
    "b": "Event Hubs is a partitioned log (like Apache Kafka) supporting multiple independent consumer groups reading from the same offset independently. It supports retention up to 90 days and Event Hubs Capture for archival. Service Bus is a message broker with competing consumers — each message is delivered to one consumer and deleted after acknowledgment.",
    "c": "Service Bus Topics with Subscriptions allow multiple subscribers but each subscription gets a copy of the message, not shared log reads. At 100K events/second, Service Bus Premium tier is required and costs significantly more than Event Hubs."
  },
  "function": "Select event streaming topology for high-throughput multi-consumer IoT ingestion",
  "acceptedOutput": "Use Azure Event Hubs with partition count matching peak parallelism needs. Configure consumer groups for analytics, alerting, and archival independently. Enable Event Hubs Capture to Azure Blob Storage for archival. Each consumer group maintains its own checkpoint offset and can replay independently.",
  "rejectedOutput": "Use Azure Service Bus Premium with Topics and Subscriptions. Create three subscriptions for analytics, alerting, and archival.",
  "arbiters": ["topology-arbiter", "throughput-capacity-arbiter"],
  "whyAccepted": "Event Hubs is designed for exactly this pattern: high-throughput partitioned log with independent consumer group offsets. Multiple readers get full event streams without message duplication cost. Native Kafka protocol support allows existing Kafka consumers without code changes.",
  "whyRejected": "Service Bus creates three message copies per event for three subscriptions — tripling storage and egress cost at 100K events/second. Service Bus is designed for command/task queuing with competing consumers, not broadcast streaming to independent consumers.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/event-hubs/event-hubs-about"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-549 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-89 SQL Injection in EF Core — raw SQL with user input vs parameterized FromSqlRaw
**Source URL:** https://cwe.mitre.org/data/definitions/89.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT549-F5-E",
  "sourceIds": ["EXT-549"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "sql-injection-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core controller accepts a search term from a query string and uses it in an EF Core raw SQL query: context.Database.ExecuteSqlRaw($\"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%'\").",
    "b": "String interpolation in ExecuteSqlRaw/FromSqlRaw does NOT parameterize the query — it concatenates the string directly into SQL. The overload accepting FormattableString (FromSqlInterpolated or $\"\" with FromSqlRaw parameter syntax) does parameterize.",
    "c": "A user submitting searchTerm = \"'; DROP TABLE Products; --\" would execute the DROP TABLE statement on databases where stacked queries are supported."
  },
  "function": "Detect and remediate SQL injection in EF Core raw query usage",
  "acceptedOutput": "Use FromSqlInterpolated which converts C# string interpolation to parameterized queries: context.Products.FromSqlInterpolated($\"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%'\"). EF Core converts {searchTerm} to a SQL parameter, not a string concatenation. Alternatively use LINQ .Where(p => EF.Functions.Like(p.Name, $\"%{searchTerm}%\")).",
  "rejectedOutput": "Sanitize the searchTerm by removing single quotes before passing it to ExecuteSqlRaw: searchTerm.Replace(\"'\", \"''\").",
  "arbiters": ["security-arbiter", "injection-prevention-arbiter"],
  "whyAccepted": "FromSqlInterpolated sends the search term as a SQL parameter (e.g., @p0) preventing any SQL interpretation of its contents. The query plan cache also benefits from parameterization.",
  "whyRejected": "Quote escaping is a denylist approach — it handles the most obvious attack but misses database-specific injection vectors, Unicode escaping, and encoding tricks. Parameterization makes injection structurally impossible regardless of input content.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/89.html", "https://learn.microsoft.com/en-us/ef/core/querying/raw-sql"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-550 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V3.4 Cookie security attributes — Secure, HttpOnly, SameSite for session tokens
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT550-F5-E",
  "sourceIds": ["EXT-550"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "session-cookie-security-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core application sets session cookies without explicit attribute configuration: services.AddSession(). The default cookie is accessible via JavaScript, sent over HTTP, and has SameSite=Lax.",
    "b": "OWASP ASVS V3.4.1 requires session cookies to have Secure (HTTPS only), HttpOnly (no JavaScript access), and SameSite=Strict or Lax. Missing HttpOnly enables XSS (Cross-Site Scripting)-based session token theft. Missing Secure transmits tokens over HTTP. SameSite=None without Secure enables CSRF (Cross-Site Request Forgery).",
    "c": "ASP.NET Core's default session cookie name is .AspNetCore.Session and is not HttpOnly or Secure by default in development mode — developers often deploy this default configuration to production."
  },
  "function": "Arbitrate session cookie security attribute compliance against ASVS V3.4",
  "acceptedOutput": "Configure session cookie with all required security attributes: services.AddSession(options => { options.Cookie.HttpOnly = true; options.Cookie.SecurePolicy = CookieSecurePolicy.Always; options.Cookie.SameSite = SameSiteMode.Strict; options.Cookie.Name = \"__Host-Session\"; options.IdleTimeout = TimeSpan.FromMinutes(20); }). The __Host- prefix additionally enforces Secure, no Domain attribute, and Path=/.",
  "rejectedOutput": "Accept the default ASP.NET Core session cookie configuration. HttpOnly and Secure settings are handled by the reverse proxy (nginx) which strips HTTP connections and blocks JavaScript cookie access.",
  "arbiters": ["security-arbiter", "session-management-arbiter"],
  "whyAccepted": "Defense in depth: cookie security attributes must be set at the application layer regardless of infrastructure. A misconfigured proxy, a developer testing without the proxy, or a proxy bypass exposes unprotected cookies. The __Host- prefix provides additional binding guarantees.",
  "whyRejected": "Relying solely on infrastructure for security attribute enforcement creates a single point of failure. If the proxy is misconfigured, bypassed, or absent in a test environment, session tokens are exposed. ASVS V3.4.1 requires application-level attribute configuration.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
