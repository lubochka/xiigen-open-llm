# External Sources Batch 21 — EXT-411 through EXT-430

Generated: 2026-05-04  
Ledger range: EXT-411 – EXT-430  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, VUE-DOCS, EFCORE-SOURCE, DOTNET-MICROSERVICES, MICROSOFT-LEARN-ARCH, CWE-TOP25, OWASP-ASVS

---

## EXT-411

```json
{
  "rowId": "EXT-411",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Kubernetes cluster needs to run a privileged init job that initialises a network device driver. This job requires Linux capabilities: `NET_ADMIN` and `SYS_ADMIN`. The security team wants the rest of the cluster pods to run without any capabilities.",
    "b": "Options: (A) Set `securityContext.capabilities.add: [NET_ADMIN, SYS_ADMIN]` on the init job pod spec and use PodSecurityPolicy (deprecated) or Pod Security Admission to restrict all other pods. (B) Use a DaemonSet with a privileged init container, scoped to specific nodes via nodeSelector. (C) Use OPA (Open Policy Agent) Gatekeeper to allow the capability only for pods in the `infra-system` namespace.",
    "c": "Kubernetes 1.28+. Pod Security Policy is removed. Pod Security Admission (PSA) is the replacement."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option C with Pod Security Admission: (1) Label the `infra-system` namespace with `pod-security.kubernetes.io/enforce: privileged` to allow privileged pods in that namespace. (2) Label all other namespaces with `pod-security.kubernetes.io/enforce: restricted` (no capabilities). (3) Deploy the privileged init job in the `infra-system` namespace only. Pod Security Admission is the Kubernetes 1.25+ replacement for PSP (Pod Security Policy). PSA uses namespace labels to enforce three built-in profiles: `privileged` (no restrictions), `baseline` (blocks most privilege escalation), `restricted` (heavily restricted). This approach requires no OPA/Gatekeeper installation (option C overhead), and namespace-scoped enforcement prevents privilege escalation in tenant namespaces. OPA Gatekeeper (option C) provides more granular policy but requires a controller installation and is more complex to configure.",
  "rejectedOutput": "Use OPA Gatekeeper to allow capabilities only for specific pods (option C). This provides the most fine-grained control.",
  "arbiters": ["pod-security-admission-profiles", "kubernetes-capability-namespace-isolation"],
  "whyAccepted": "Pod Security Admission is built-in (no additional controller needed), namespace-scoped, and enforces the principle of least privilege for all non-infra namespaces without OPA complexity.",
  "whyRejected": "OPA Gatekeeper provides more granularity but adds a controller, admission webhook, and ConstraintTemplate complexity. PSA's built-in profiles cover this use case with less operational overhead.",
  "sourceAnchors": [
    "https://kubernetes.io/docs/concepts/security/pod-security-admission/",
    "https://kubernetes.io/docs/concepts/security/pod-security-standards/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-412

```json
{
  "rowId": "EXT-412",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement `process_records_concurrently(records: list[dict], processor: Callable) -> list[dict]` that processes records concurrently using Python's `concurrent.futures.ThreadPoolExecutor` with `max_workers=len(records)`.",
    "b": "Test input: records list with 10,000 elements.",
    "c": "Creating 10,000 threads causes: OS thread limit exceeded (typically 1,000-10,000 depending on system), excessive context switching overhead, and memory pressure (~8MB stack per thread on Linux)."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace `max_workers=len(records)` with a bounded thread count. `ThreadPoolExecutor(max_workers=len(records))` attempts to create as many threads as records — for 10,000 records this is 10,000 threads, which exhausts OS thread limits and adds ~80GB of stack memory pressure. The correct pattern: `max_workers = min(32, (os.cpu_count() or 1) + 4)` — this is the `ThreadPoolExecutor` default formula (as of Python 3.8). For CPU-bound processing: `max_workers = os.cpu_count()`. For I/O-bound processing: `max_workers = min(32, os.cpu_count() * 4)` — threads spend time waiting, so more threads than CPUs makes sense but not unbounded. The pool queues work internally — 10,000 work items submitted to a 32-thread pool processes them in batches without spawning 10,000 threads.",
  "rejectedOutput": "Use max_workers=len(records) to process all records truly in parallel. More threads = more parallelism.",
  "arbiters": ["threadpoolexecutor-bounded-workers", "os-thread-limit-10k"],
  "whyAccepted": "Bounded thread pool (32 workers) queues 10,000 records and processes them correctly. 10,000 threads exceeds OS limits and adds 80GB stack overhead at 8MB/thread.",
  "whyRejected": "Thread parallelism is bounded by CPU cores, not thread count. 10,000 threads on a 16-core machine means 9,984 threads context-switching — overhead dominates throughput. OS thread limits are also typically below 10,000.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench",
    "https://docs.python.org/3/library/concurrent.futures.html#concurrent.futures.ThreadPoolExecutor"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-413

```json
{
  "rowId": "EXT-413",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A Django REST Framework serializer uses `many=True` to serialize a list of objects: `OrderSerializer(queryset, many=True).data`. A load test reveals the serialization takes 1,200ms for 500 objects.",
    "b": "Three agents investigate: (A) The serializer has N+1 queries — each object serializes a nested FK (ForeignKey) relation without select_related. (B) The serializer calls a custom `get_status_label()` method that makes a regex match on every object. (C) The queryset is not evaluated until serialization — calling `.data` triggers evaluation, losing the query optimization window.",
    "c": "The queryset was constructed as `Order.objects.filter(status='active')`. No select_related or prefetch_related applied."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Select Agent-A: N+1 queries from FK serialization. The 1,200ms for 500 objects is consistent with N+1 at ~2ms/query × 500 objects = 1,000ms overhead. Without `select_related`, each object accessing `order.customer` fires a separate `SELECT customer WHERE id=X` query. Fix: add `select_related('customer')` to the queryset before passing to the serializer. Agent-B (regex on every object) would contribute but typically adds <1ms/object = <500ms — not the 1,200ms primary driver. Agent-C (queryset evaluation) is not a performance issue — DRF serializers evaluate the queryset by iterating it, which is correct. The queryset evaluation timing does not affect the query execution plan. After fixing select_related, measure again for Agent-B's regex contribution.",
  "rejectedOutput": "Agent-C identified the root cause. Forcing queryset evaluation with list(queryset) before serialization allows the optimizer to prepare the complete result set.",
  "arbiters": ["drf-serializer-n-plus-one", "select-related-serializer-fk"],
  "whyAccepted": "N+1 at ~2ms/query × 500 = 1,000ms matches the observed 1,200ms. select_related eliminates N+1 by JOINing the FK in the original query.",
  "whyRejected": "Agent-C is incorrect: `list(queryset)` and `.data` evaluate the queryset identically — iterating and executing the SQL. Pre-evaluating with list() does not change the query plan or execution cost.",
  "sourceAnchors": [
    "https://www.django-rest-framework.org/api-guide/serializers/#dealing-with-nested-objects",
    "https://www.swebench.com"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-414

```json
{
  "rowId": "EXT-414",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Python 3.11 data pipeline uses `match/case` (PEP-634) for command routing: `match command: case 'start': start_pipeline(); case 'stop': stop_pipeline(); case _: handle_unknown(command)`. A developer extends this with 47 case branches and argues that match/case is cleaner than if/elif chains.",
    "b": "The match statement with 47 string literal cases is functionally equivalent to an if/elif chain. No structural pattern matching (tuple unpacking, class patterns, guard clauses) is used.",
    "c": "All 47 cases are simple string equality checks mapping to function calls."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Refactor the 47-case match statement to a dispatch dictionary: `HANDLERS = {'start': start_pipeline, 'stop': stop_pipeline, ...}; handler = HANDLERS.get(command, handle_unknown); handler(command)`. The dispatch dictionary is: (1) O(1) lookup vs O(n) sequential case matching, (2) Directly data-driven — adding a new command requires one line in the dict, not inserting a new case branch in sorted order, (3) Testable independently — the HANDLERS dict can be inspected and patched in tests. Match/case is the correct tool for structural pattern matching (matching on object shapes, tuple structures, class types) — not for 47 string equality dispatches. The pattern for string-to-function dispatch in Python is the dictionary dispatch pattern, not match/case.",
  "rejectedOutput": "Keep the match statement. Match/case is the modern Python control flow for this pattern and is more readable than a dictionary dispatch.",
  "arbiters": ["python-dispatch-dict-vs-match-case", "o-n-match-vs-o-1-dict-lookup"],
  "whyAccepted": "Dictionary dispatch is O(1) lookup and directly maps to the 'string → function' pattern. 47-case match is O(n) and uses structural pattern matching syntax for what is semantically a hash map lookup.",
  "whyRejected": "match/case for 47 string equality cases misuses structural pattern matching. Python's match is not a hash table — it evaluates cases sequentially. Dictionary dispatch is both more efficient and more idiomatic for string routing.",
  "sourceAnchors": [
    "https://peps.python.org/pep-0634/",
    "https://docs.python.org/3/reference/compound_stmts.html#match"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-415

```json
{
  "rowId": "EXT-415",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservice has traces where the same operation appears in both the parent span and a child span with different `http.method` attribute values. Parent span shows `http.method: GET`, child span shows `http.method: POST`. Both claim to be the same HTTP request.",
    "b": "OpenTelemetry Semantic Conventions: `http.method` must reflect the actual HTTP method of the request represented by the span. Parent span represents the inbound request; child span represents an outbound downstream call.",
    "c": "The service receives a GET request from the client and makes a POST request to a downstream data service."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Both spans are correct — they represent different HTTP requests. The trace structure is: (1) Parent span (server span): represents the inbound GET /data request from the client. `http.method: GET` is correct. (2) Child span (client span): represents the outbound POST /downstream request the service makes to the data service. `http.method: POST` is correct. OTel Semantic Conventions distinguish server spans (inbound) from client spans (outbound) via `span.kind: SERVER` and `span.kind: CLIENT`. The 'same operation' claim is incorrect — they are causally related operations (the GET triggers the POST) but distinct HTTP requests with distinct HTTP methods. The correct diagnostic: verify `span.kind` attributes. If both are `SERVER` spans with different methods, that indicates a bug. If one is `CLIENT` and one is `SERVER`, the trace is correct.",
  "rejectedOutput": "One of the spans has the wrong http.method attribute. Both spans represent the same HTTP request and must have the same method value.",
  "arbiters": ["otel-server-vs-client-span-kind", "http-method-semantic-convention"],
  "whyAccepted": "Parent SERVER span (inbound GET) and child CLIENT span (outbound POST) correctly show different methods because they represent different HTTP requests in a causal chain.",
  "whyRejected": "The two spans do not represent the same HTTP request — they represent two different requests in a causal chain. Conflating them misunderstands the OTel span kind model.",
  "sourceAnchors": [
    "https://opentelemetry.io/docs/specs/semconv/http/http-spans/",
    "https://opentelemetry.io/docs/concepts/signals/traces/#span-kind"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-416

```json
{
  "rowId": "EXT-416",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET 6 application uses Dapper 1.50.7 for all data access. Dapper 2.0 was released with breaking changes: `IDataReader` mapping improvements, async API changes, and removal of some dynamic query overloads.",
    "b": "The application has 120 Dapper call sites. Dapper 1.x is no longer receiving security patches (only Dapper 2.x is actively maintained). No known CVE against Dapper 1.x.",
    "c": "Migration effort estimate: 2 developer-days to update 120 call sites and run regression tests."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "MIGRATE to Dapper 2.x in the next sprint. Dapper 1.x is no longer actively maintained — no new features and no security patches. Although there is no current CVE, a future SQL Server driver or .NET runtime change could create a compatibility issue that will not be fixed in 1.x. At 2 developer-days for 120 call sites, the migration cost is low relative to the risk of accumulating technical debt on an unmaintained library. The migration is also low-risk: the breaking changes are documented and testable. Gate: all existing integration tests pass with Dapper 2.x. Do not defer this migration — the longer it waits, the more call sites accumulate.",
  "rejectedOutput": "Defer migration. There is no active security vulnerability in Dapper 1.x and the 2 developer-days cost is not justified without a concrete risk.",
  "arbiters": ["library-maintenance-status-migration", "unmaintained-dependency-accumulation"],
  "whyAccepted": "2 developer-days for 120 call sites is low-cost migration. Unmaintained libraries accumulate risk over time. Migrate proactively before a future incompatibility forces an emergency migration.",
  "whyRejected": "'No current CVE' does not mean 'no risk'. .NET runtime changes, SQL Server driver changes, or compiler updates may break Dapper 1.x behaviour in ways that will not be fixed in 1.x. Emergency migration is more expensive than planned migration.",
  "sourceAnchors": [
    "https://github.com/DapperLib/Dapper/releases",
    "https://github.com/DapperLib/Dapper/blob/main/Changelog.md"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-417

```json
{
  "rowId": "EXT-417",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 application needs internationalization (i18n) support: English and French, with future plans for Arabic (RTL). Two approaches: (A) Angular's built-in i18n with `ng extract-i18n` and XLIFF files. (B) ngx-translate library with JSON translation files.",
    "b": "Angular built-in i18n: compile-time translation — separate build artifacts per locale, no runtime switching. ngx-translate: runtime translation switching without rebuilding.",
    "c": "The application will be deployed as a single SPA (Single Page Application). Users cannot be redirected to locale-specific URLs. RTL (Right-to-Left) support for Arabic is a future requirement."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use ngx-translate (option B). The 'single SPA with no locale-specific URLs' constraint is the decisive factor: Angular's built-in i18n produces separate compiled bundles per locale and routes users to locale-specific base paths (e.g., `/en/`, `/fr/`). If users cannot be redirected to locale-specific URLs and a single deployment is required, Angular built-in i18n cannot switch locales at runtime without a page reload. ngx-translate loads JSON translation files at runtime and supports `TranslateService.use('fr')` for seamless switching. For future Arabic RTL: add `dir` attribute toggling on the root element and `@angular/cdk/bidi` for programmatic LTR/RTL detection. Both approaches support RTL but ngx-translate's runtime switching makes locale toggling more fluid.",
  "rejectedOutput": "Use Angular built-in i18n. It is the official Angular approach, produces optimally compiled bundles for each locale, and has better performance than runtime translation.",
  "arbiters": ["ngx-translate-runtime-switching", "angular-i18n-compile-time-limitation"],
  "whyAccepted": "ngx-translate's runtime locale switching satisfies the 'single SPA, no locale URL redirects' constraint. Angular built-in i18n requires separate builds and locale-specific URLs — architecturally incompatible with the requirement.",
  "whyRejected": "Angular built-in i18n's compile-time approach is correct for multi-URL deployments but incompatible with the stated single-SPA constraint. 'Better performance' from compiled bundles is outweighed by the architectural incompatibility.",
  "sourceAnchors": [
    "https://angular.dev/guide/i18n",
    "https://github.com/ngx-translate/core"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-418

```json
{
  "rowId": "EXT-418",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React 18 application has a data visualisation dashboard that renders 50 charts, each fetching its own data. The initial page load takes 8 seconds because all 50 API calls fire simultaneously. Two proposals: (A) Render charts incrementally — each chart loads independently as data arrives (current behaviour, just parallelised). (B) Use React Suspense with `startTransition` to defer non-critical charts.",
    "b": "Charts are prioritised: 5 'key metrics' charts are critical (must show first), 45 'detail' charts are non-critical. Network bandwidth is not the constraint — the backend can handle 50 simultaneous requests.",
    "c": "Users report the dashboard 'feels slow' because no content appears for 8 seconds."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use React Suspense with startTransition for deferred non-critical charts (option B). The perceived slowness — 'nothing for 8 seconds' — is caused by all 50 charts rendering simultaneously with skeleton states. The fix: (1) Render the 5 key metrics charts with normal Suspense (high priority — show content as soon as their data arrives). (2) Wrap the 45 detail charts in `startTransition(() => setShowDetails(true))` — React defers their rendering until the critical charts are interactive. (3) Each chart uses `<Suspense fallback={<ChartSkeleton/>}>` — users see 5 real charts within 1-2 seconds of load, then detail charts progressively appear. This is exactly the React 18 Concurrent Features use case. Progressive disclosure (key metrics first) matches how users actually consume dashboards.",
  "rejectedOutput": "Option A: keep all 50 charts loading independently but add skeleton loaders to each. Users will see content appear progressively as each chart's data arrives.",
  "arbiters": ["react-suspense-start-transition-deferred", "perceived-performance-progressive-disclosure"],
  "whyAccepted": "startTransition defers the 45 detail charts, unblocking the browser to render and interact with the 5 key metrics charts. Skeleton loaders alone (option A) do not change the order in which charts become interactive.",
  "whyRejected": "Option A with skeletons shows 50 skeleton states simultaneously — users still wait for content in no particular order. startTransition prioritises the 5 critical charts and makes them interactive first.",
  "sourceAnchors": [
    "https://react.dev/reference/react/startTransition",
    "https://react.dev/reference/react/Suspense"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-419

```json
{
  "rowId": "EXT-419",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An ASP.NET Core 8 API action method returns `IActionResult` and constructs error responses by concatenating user input into an error message string: `return BadRequest($\"Invalid order ID: {orderId} provided by user {username}\")`.",
    "b": "The error message is logged server-side and also returned to the client in the response body.",
    "c": "CWE-209 (Information Exposure Through Error Messages) and CWE-116 (Improper Encoding or Escaping of Output). `username` and `orderId` are from request parameters. A malicious username could include: HTML tags, JSON control characters, or log injection sequences (newlines + fake log entries)."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Fix three separate concerns: (1) Log injection: log structured fields separately — `_logger.LogWarning(\"Invalid order ID {OrderId} for user {Username}\", orderId, username)` — Serilog/structured logging ensures username newlines do not create fake log entries. (2) Response body: return a structured error without user-supplied values — `return BadRequest(new ProblemDetails { Title = \"Invalid order ID\", Detail = \"The provided order ID is not valid.\", Status = 400 })`. Do not echo user-supplied input in error responses — even if safe today, it creates an XSS vector in browser contexts and an information leakage pathway. (3) If the orderId must appear in the response for debugging, validate it first: if orderId matches the expected format (UUID, integer), it is safe to echo; if it contains unexpected characters, log only and return a generic error.",
  "rejectedOutput": "HtmlEncode the username before including it in the error message: `return BadRequest($\"Invalid order ID: {orderId} provided by user {HttpUtility.HtmlEncode(username)}\")`. This prevents XSS in the response.",
  "arbiters": ["cwe-209-error-message-information-exposure", "log-injection-structured-logging"],
  "whyAccepted": "Structured logging prevents log injection; generic error response prevents information leakage and XSS without depending on correct encoding in every call site.",
  "whyRejected": "HtmlEncode prevents XSS but not log injection (newlines in username create fake log entries), not information leakage (username revealed in error response), and not all clients interpret HTML encoding (JSON APIs don't apply HTML encoding).",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/209.html",
    "https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-420

```json
{
  "rowId": "EXT-420",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS service needs to call an external REST API that has rate limits: 100 requests/minute. The service can receive bursts of 500 requests in a 10-second window. Options: (A) Implement a token bucket rate limiter in the NestJS service using an in-memory counter. (B) Use a Redis-backed rate limiter (rate-limiter-flexible library). (C) Add a dedicated rate-limiting proxy (Nginx or API gateway rate limiting).",
    "b": "The NestJS service runs on 4 instances (Kubernetes, 4 pods). Each pod has its own in-memory counter.",
    "c": "The external API enforces 100 req/min at the IP level (all 4 pods share the same egress IP)."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option B: Redis-backed rate limiter (rate-limiter-flexible). Option A (in-memory counter per pod) is incorrect for 4 pods sharing an egress IP: each pod allows up to 100 req/min independently, meaning all 4 pods together allow 400 req/min — 4x the external limit. The external API sees 400 requests from the shared IP and rate-limits all 4 pods after 100 requests. Redis-backed rate limiting uses a shared counter: all 4 pods decrement the same Redis key, enforcing a true 100 req/min aggregate limit. Option C (Nginx/API gateway proxy) is correct but adds infrastructure complexity — rate-limiter-flexible with Redis is a code-level solution that runs within the existing pod architecture. Rate-limiter-flexible uses Redis atomic increment + expire to implement token bucket or leaky bucket algorithms without race conditions.",
  "rejectedOutput": "Use in-memory rate limiting (option A). It is simpler and avoids a Redis dependency. Each pod limits itself to 25 req/min (100 ÷ 4 pods) to stay within the aggregate limit.",
  "arbiters": ["distributed-rate-limiting-shared-counter", "redis-rate-limiter-flexible"],
  "whyAccepted": "Redis-backed shared counter enforces the aggregate 100 req/min limit across all 4 pods. In-memory per-pod counters allow 4x the limit regardless of how the per-pod limit is calculated.",
  "whyRejected": "25 req/min per pod assumes exactly 4 pods always. If Kubernetes scales to 5 or 6 pods (HPA), the calculation is wrong and rate limit violations occur. Redis-backed limiting is correct regardless of pod count.",
  "sourceAnchors": [
    "https://github.com/animir/node-rate-limiter-flexible",
    "https://docs.nestjs.com/security/rate-limiting"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-421

```json
{
  "rowId": "EXT-421",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C3-OutputFormatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API endpoint `GET /users/{id}` returns user data including date of birth and registration date. Format options: (A) ISO 8601 strings: `\"date_of_birth\": \"1990-03-15\"`. (B) Unix timestamps: `\"date_of_birth\": 636566400`. (C) Custom format: `\"date_of_birth\": \"15/03/1990\"`.",
    "b": "RFC 3339 (profile of ISO 8601) is the IETF-recommended format for date/time in JSON APIs. Unix timestamps are unambiguous but require the consumer to know the epoch reference and handle milliseconds vs seconds.",
    "c": "Consumers: JavaScript frontend (Date parsing), Python data pipeline (datetime parsing), Java backend (LocalDate parsing)."
  },
  "function": "finalMachineResults_to_userFormat",
  "acceptedOutput": "Use ISO 8601 / RFC 3339 strings (option A). `\"date_of_birth\": \"1990-03-15\"` (date-only, RFC 3339 full-date format) is parseable by `new Date('1990-03-15')` in JavaScript, `datetime.fromisoformat('1990-03-15')` in Python 3.7+, and `LocalDate.parse('1990-03-15')` in Java. Unix timestamps (option B) for dates (not datetimes) introduce unnecessary complexity: timestamps have implicit timezone semantics and may represent midnight UTC or midnight local time — ambiguous for date-only values. Custom formats (option C) require bespoke parsing logic in every consumer. For datetime values (with time component), use RFC 3339 full format: `\"created_at\": \"2024-01-15T14:30:00Z\"` — always with timezone offset (Z for UTC). Never return timezone-naive datetime strings.",
  "rejectedOutput": "Use Unix timestamps (option B). They are unambiguous integers with no timezone ambiguity and are natively supported by all languages.",
  "arbiters": ["rfc-3339-datetime-format", "iso-8601-api-dates"],
  "whyAccepted": "ISO 8601 / RFC 3339 is human-readable, natively parseable in all three consumer languages, and standard per IETF. Unix timestamps for date-only values introduce midnight timezone ambiguity.",
  "whyRejected": "Unix timestamp for a date-only value (birthdate) is ambiguous: `636566400` is midnight UTC on 1990-03-15 but represents March 14 in UTC-5 — consumers in different timezones interpret the date differently. ISO 8601 date strings are unambiguous.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc3339",
    "https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Date/parse"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-422

```json
{
  "rowId": "EXT-422",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API has a long-running operation: exporting 10,000 user records. The operation takes 30-120 seconds. Design the API for this operation.",
    "b": "AIP-151: Long-running operations. The standard pattern: return an `Operation` resource immediately, client polls for completion, client fetches result when done.",
    "c": "The export result is a downloadable file (CSV). The polling endpoint must return meaningful progress: records exported vs total."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Implement per AIP-151: (1) `POST /users:export` returns `202 Accepted` with an `Operation` resource body: `{name: 'operations/export-abc123', done: false, metadata: {type: 'ExportUsersMetadata', recordsExported: 0, totalRecords: 10000}}`. (2) `GET /operations/export-abc123` returns the current state: `{name: ..., done: false, metadata: {recordsExported: 4200, totalRecords: 10000}, progress: 42}`. (3) When complete: `{done: true, metadata: {recordsExported: 10000, totalRecords: 10000}, response: {downloadUrl: 'https://storage.../export-abc123.csv', expiresAt: '2024-01-16T00:00:00Z'}}`. (4) `DELETE /operations/export-abc123` cancels the operation if supported. The client polls GET /operations/{name} with exponential backoff (1s, 2s, 4s, 8s, max 60s). The `progress` field (0-100) is optional per AIP-151 but strongly recommended for UX.",
  "rejectedOutput": "Return the file directly in the POST /users:export response body with a long timeout (120s). This is simpler: one request, one response.",
  "arbiters": ["aip-151-long-running-operations", "202-accepted-polling-pattern"],
  "whyAccepted": "AIP-151 long-running operations pattern: immediate 202 + polling prevents HTTP timeout, provides progress visibility, and allows cancellation. A 120-second synchronous response blocks the client thread and hits proxy/load balancer timeouts.",
  "whyRejected": "A 120-second synchronous response blocks client connections, fails behind proxies with 60-second timeouts, and provides no progress information. Clients cannot cancel or retry without starting over.",
  "sourceAnchors": [
    "https://google.aip.dev/151",
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.3.3"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-423

```json
{
  "rowId": "EXT-423",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C2-ResultIntegration",
  "rowClass": "negative-example",
  "input": {
    "a": "A model serving cluster reports: 'GPU memory usage is at 95% (23.8 GB / 25 GB). New inference requests are failing with RuntimeError: CUDA out of memory.' The model is a 13B parameter model loaded in BF16 precision.",
    "b": "Three operators: (R1) Reduce batch size from 8 to 4. (R2) Enable CUDA memory caching flush: `torch.cuda.empty_cache()` called after each request. (R3) Enable 4-bit quantisation (bitsandbytes NF4) to reduce model footprint from ~26GB to ~8GB.",
    "c": "Peak hour: 50 concurrent inference requests. Requests arrive faster than they complete. The model footprint itself is ~26GB (13B params × 2 bytes/param BF16) — exceeding the 25GB GPU."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Apply R3: 4-bit quantisation (NF4). The root cause is that the model footprint (26GB) exceeds the GPU VRAM (25GB) before any inference occurs. R1 (reduce batch size) reduces activation memory per batch but does nothing about the 26GB model weights — the OOM occurs during model loading, not during inference activation. R2 (`torch.cuda.empty_cache()`) flushes PyTorch's memory cache but does not free model weights — the 26GB model remains loaded regardless. 4-bit NF4 quantisation reduces model footprint from ~26GB to ~8GB, leaving 17GB for KV (Key-Value) cache and activations. R1 (batch size reduction) is useful after R3 as a secondary measure if activation memory is still tight. R3 is the prerequisite fix: the model cannot run at all on a 25GB GPU in BF16 without quantisation.",
  "rejectedOutput": "Apply R1 and R2 together: reduce batch size to 4 and flush the CUDA cache after each request. This reduces peak memory usage and frees cached allocations.",
  "arbiters": ["cuda-model-footprint-vs-activation-memory", "bnb-4bit-quantisation"],
  "whyAccepted": "Model footprint (26GB) > GPU VRAM (25GB) — the model cannot be loaded without quantisation. Batch size and cache flushing address activation memory, not model weight storage.",
  "whyRejected": "R1 + R2 address activation and cache memory but the 26GB BF16 model weights are fixed — they occupy 26GB regardless of batch size or cache flushes. The model cannot be loaded on a 25GB GPU in BF16.",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1",
    "https://huggingface.co/blog/4bit-transformers-bitsandbytes"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-424

```json
{
  "rowId": "EXT-424",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-SemanticMapping",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Build a sentence alignment quality filter for Tatoeba EN-JA (English-Japanese) pairs. Filter out pairs where the translation is likely misaligned or low quality.'",
    "b": "Available signals: (1) Length ratio (EN tokens / JA characters as proxy for JA tokens). (2) Language identification confidence (fastText LID). (3) LASER cross-lingual embedding cosine similarity.",
    "c": "Dataset: 200,000 EN-JA pairs. Target: filter to 150,000 high-quality pairs (25% rejection rate)."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Action items for multi-signal quality filtering: (1) Language ID filter: run fastText lid.176 on both sides; reject pairs where EN side is not `en` (confidence > 0.8) or JA side is not `ja` (confidence > 0.8). Expected: ~2% rejection. (2) Length ratio filter: compute `en_token_count / ja_char_count`. For EN-JA, expect ratio 0.2-0.8 (Japanese uses fewer characters per concept). Reject pairs outside the 2nd-98th percentile of the ratio distribution. Expected: ~4% rejection. (3) LASER similarity filter: compute LASER embeddings for both sides; compute cosine similarity; reject pairs below threshold (typically 0.8 for EN-JA). Expected: ~20% rejection. Apply filters sequentially (LID first — cheapest; LASER last — most expensive). The combination yields approximately 25% rejection as targeted. Gate: manually inspect 100 random rejected pairs to confirm filter accuracy before production use.",
  "rejectedOutput": "Use only LASER similarity as a single filter at a threshold that rejects 25% of pairs. It is the strongest signal and using multiple filters is overly complex.",
  "arbiters": ["laser-embedding-alignment-filter", "multi-signal-quality-pipeline"],
  "whyAccepted": "Multi-signal filtering applies cheap filters (LID, length ratio) before expensive LASER computation. LID catches language errors that LASER might score highly (same-language pairs can have high similarity).",
  "whyRejected": "LASER alone cannot detect all misalignment types: same-language errors (EN paired with non-JA) may have moderate LASER scores. LID is necessary as a prerequisite. Single-signal filtering is more likely to have systematic false-reject or false-pass biases.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://github.com/facebookresearch/LASER"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-425

```json
{
  "rowId": "EXT-425",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 application uses Nuxt 3 for SSR. A page component fetches product data with `useFetch('/api/products')` in the `<script setup>`. The data is used for both the initial server render and client-side navigation.",
    "b": "Options: (A) Use `useFetch` (Nuxt composable) — fetches on server and hydrates on client. (B) Use `useAsyncData` with a custom fetch function. (C) Use the native `fetch` API inside `onMounted()` — client-only fetch.",
    "c": "SEO is a requirement: product data must be in the initial HTML. Products are cached for 60 seconds on the server."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use `useFetch` (option A). `useFetch('/api/products')` in Nuxt 3 executes on the server during SSR and the result is serialized into the page HTML — the client hydrates from the server-fetched data without a duplicate network request. This satisfies the SEO requirement (data in initial HTML) and is the Nuxt 3 idiomatic approach. `useAsyncData` (option B) is appropriate when the fetch logic is more complex (multiple sources, custom key, conditional fetching) — `useFetch` is syntactic sugar for `useAsyncData` + `$fetch`. Option C (`onMounted` + native fetch) runs only on the client after hydration — the initial HTML has no product data, breaking SEO. Configure server-side caching with `useFetch('/api/products', { server: true, lazy: false, getCachedData: key => nuxtApp.payload.data[key] || nuxtApp.static.data[key] })` or via a server-side cache layer.",
  "rejectedOutput": "Use fetch inside onMounted() (option C) with explicit loading states. Server-side fetching in Nuxt adds complexity and onMounted gives full control over the fetch lifecycle.",
  "arbiters": ["nuxt3-use-fetch-ssr", "seo-server-side-data-requirement"],
  "whyAccepted": "useFetch executes on server, serializes data into HTML, and hydrates without a duplicate request — satisfying SEO and performance requirements. onMounted fetching produces empty HTML, breaking SEO.",
  "whyRejected": "onMounted fetch runs client-side after hydration. The initial HTML has no product data — web crawlers see empty content, breaking SEO. 'Full control over fetch lifecycle' does not justify breaking the core SEO requirement.",
  "sourceAnchors": [
    "https://nuxt.com/docs/api/composables/use-fetch",
    "https://nuxt.com/docs/getting-started/data-fetching"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-426

```json
{
  "rowId": "EXT-426",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An EF Core 8 repository method loads an entity, modifies it, and saves it: `var order = await context.Orders.FindAsync(id); order.Status = newStatus; await context.SaveChangesAsync()`. A developer adds `context.ChangeTracker.Clear()` before the find to 'improve performance'.",
    "b": "ChangeTracker.Clear() detaches all tracked entities. If `FindAsync` is called after Clear(), EF Core cannot use the identity map to serve the entity from the cache — it must hit the database.",
    "c": "The context is scoped (one per HTTP request). No other entities are tracked when this method runs."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Remove the `ChangeTracker.Clear()` call. With no other tracked entities and a scoped context, calling Clear() before FindAsync has no performance benefit: there is nothing to clear. Furthermore, if this pattern were used in a method that IS called with other tracked entities, Clear() would detach them, causing the subsequent SaveChangesAsync to fail to save those entities (they are no longer tracked). `ChangeTracker.Clear()` is appropriate in two scenarios: (1) After processing a large batch to release memory held by tracked entities. (2) Before beginning an isolated unit of work that should not be affected by previously tracked entities. In a per-request scoped context with a simple find-update-save pattern, it is unnecessary and potentially harmful if copy-pasted into a context with other tracked entities.",
  "rejectedOutput": "Keep ChangeTracker.Clear(). It prevents stale tracked entities from interfering with the FindAsync and ensures a fresh load from the database.",
  "arbiters": ["efcore-change-tracker-clear-semantics", "scoped-context-identity-map"],
  "whyAccepted": "Clear() is a no-op and potentially harmful pattern for a scoped context with no other tracked entities. The performance argument is invalid — there is nothing to clear.",
  "whyRejected": "'Ensures fresh load from database' is incorrect — FindAsync already hits the database if the entity is not in the identity map. If it IS in the identity map, that means it was loaded this request and Clear() forces an unnecessary extra database round trip.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/change-tracking/",
    "https://github.com/dotnet/efcore"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-427

```json
{
  "rowId": "EXT-427",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET 8 microservice processes orders. Processing an order involves: (1) Validate order, (2) Reserve inventory, (3) Process payment (Stripe), (4) Send confirmation email, (5) Update order status. All steps must succeed atomically or the order must be rolled back.",
    "b": "Options: (A) Single synchronous method with try/catch and manual rollback. (B) Saga orchestration with a dedicated OrderSaga service. (C) Temporal workflow (Temporal.io .NET SDK) for durable execution.",
    "c": "Payment (step 3) is non-idempotent without Stripe's idempotency key. Steps 2 and 3 are external service calls that cannot be wrapped in a database transaction."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use Temporal workflow (option C) for production-grade durable execution. The manual try/catch approach (option A) has a critical flaw: if the process crashes between step 3 (payment succeeded) and step 5 (order status updated), the customer is charged but the order is not confirmed — a money-lost scenario with no automatic recovery. Saga orchestration (option B) handles this correctly but requires implementing saga state persistence, compensation logic, and retry logic manually. Temporal workflow provides durable execution out of the box: if the worker crashes mid-workflow, Temporal replays from the last completed activity with full state recovery. Stripe idempotency key handling: pass a deterministic key (`order_id + '_payment'`) as the Stripe idempotency key — Temporal replays the same activity call with the same key, preventing double charges. This is the canonical use case for Temporal: complex multi-step workflows that must be durable across failures.",
  "rejectedOutput": "Use a single synchronous method with try/catch and manual rollback. This is simpler and keeps the logic in one place.",
  "arbiters": ["temporal-durable-execution", "saga-vs-temporal-orchestration"],
  "whyAccepted": "Temporal provides crash-safe durable execution with automatic replay from last checkpoint. Manual try/catch cannot recover from process crashes between external service calls.",
  "whyRejected": "Manual try/catch rollback fails on process crash between step 3 and step 4 — payment charged, order not confirmed, no automatic recovery. This is a money-lost scenario that manual try/catch cannot handle.",
  "sourceAnchors": [
    "https://docs.temporal.io/develop/dotnet",
    "https://stripe.com/docs/api/idempotent_requests"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-428

```json
{
  "rowId": "EXT-428",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET 6 monolith exposes all functionality via a single ASP.NET Core API project. The team wants to extract the 'Reporting' bounded context (12 controllers, 45 services) into a separate microservice. The reporting controllers are currently called by 8 other controllers in the monolith via direct C# method calls.",
    "b": "Two approaches: (A) Extract to a separate service, convert inter-controller calls to HTTP service calls immediately. (B) Modular monolith first: extract Reporting into a separate C# project (class library) with a defined interface, keep all in-process. Later, split to a separate service if needed.",
    "c": "Team has 4 developers. Reporting module has 200 unit tests. No current performance or scaling problem with reporting."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "Use option B: modular monolith first. The team has no current performance or scaling problem — the motivation is code organisation, not operational necessity. Extracting to a microservice immediately (option A) adds: network latency on 8 call sites (previously in-process), distributed tracing setup, service discovery, separate CI/CD pipeline, separate deployment, and inter-service authentication. For a 4-developer team, this operational overhead is significant relative to the value gained. Modular monolith (separate C# project with an IReportingService interface) achieves the bounded context boundary enforcement within the same process: changes to Reporting internals don't ripple into callers (interface boundary), tests remain fast (in-process, no mock servers), and deployment remains simple. The microservice split is enabled later by swapping the in-process interface implementation for an HTTP client implementation — a one-day change when scaling actually demands it.",
  "rejectedOutput": "Extract to a microservice immediately (option A). Microservices are the correct architecture for bounded contexts and the sooner the extraction happens, the less coupling accumulates.",
  "arbiters": ["modular-monolith-vs-microservice-extraction", "premature-microservices-overhead"],
  "whyAccepted": "Modular monolith enforces bounded context boundaries without operational overhead. The microservice split is a one-day change when actually needed. 4-developer teams rarely have the operational capacity for premature microservice extraction.",
  "whyRejected": "'Microservices are correct for bounded contexts' conflates architectural pattern with operational necessity. The team has no scaling problem. The network, tracing, and deployment overhead of a new service is real cost for a hypothetical future benefit.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/microservices",
    "https://www.martinfowler.com/bliki/MonolithFirst.html"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-429

```json
{
  "rowId": "EXT-429",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A .NET 8 API deserializes user-supplied XML: `var doc = XDocument.Load(new StringReader(userXml))`. The XML parser is the default .NET XML parser.",
    "b": "CWE-611 XML External Entity (XXE) Injection. Test payload: `<?xml version='1.0'?><!DOCTYPE foo [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]><root>&xxe;</root>`.",
    "c": "The API processes XML configuration files uploaded by users. XDocument.Load() in .NET uses XmlReader internally."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Add explicit XXE protection. .NET's XDocument.Load() with default settings is safe against XXE in .NET 4.5.2+ and .NET Core — the default XmlResolver is null, which prevents external entity resolution. However, explicitly setting this is defensive best practice: `var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null }; using var reader = XmlReader.Create(new StringReader(userXml), settings); var doc = XDocument.Load(reader)`. `DtdProcessing.Prohibit` (not just Ignore) throws an XmlException if a DOCTYPE declaration is present — the correct response for user-supplied XML that should not contain DTDs. If DTD processing is required for legitimate use, use `DtdProcessing.Ignore` (strips DTD without processing) and `XmlResolver = null` (no external resolution). The default .NET behaviour is safe but the explicit settings document intent and guard against future version changes.",
  "rejectedOutput": "The .NET default XDocument.Load() is safe against XXE since .NET 4.5.2. No changes are needed.",
  "arbiters": ["cwe-611-xxe-dotnet", "xml-reader-settings-dtd-prohibit"],
  "whyAccepted": "Explicit DtdProcessing.Prohibit + XmlResolver=null is defence-in-depth: documents security intent, guards against future defaults, and throws a clear exception for malicious payloads rather than silently ignoring them.",
  "whyRejected": "'Default is safe' is true today but undocumented intent — a developer unfamiliar with the historical change may 'fix' the default in the future. Explicit settings prevent regression and document the security requirement.",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/611.html",
    "https://learn.microsoft.com/en-us/dotnet/standard/data/xml/xml-reader-settings"
  ],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-430

```json
{
  "rowId": "EXT-430",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core 8 application sets session cookies. Current cookie configuration: `options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Lax`. A security review recommends adding `Secure = true` and changing SameSite to `Strict`.",
    "b": "OWASP ASVS V3.4.1-3.4.5 cookie security requirements. `Secure = true`: cookie only sent over HTTPS. `SameSite = Strict`: cookie not sent with cross-site requests, including navigation from external links.",
    "c": "The application is a web app that: users access via HTTPS only, may receive traffic via links from partner websites, uses OAuth for login (OAuth callback is a cross-site redirect)."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Set `Secure = true` unconditionally. Add `SameSite = Lax` (keep current, do not change to Strict). `Secure = true` is unconditionally correct: the application is HTTPS-only and the Secure flag prevents cookie transmission over HTTP (which would expose session tokens if a user somehow accesses an HTTP URL). `SameSite = Strict` would break the OAuth callback flow: OAuth login involves a redirect from the identity provider back to the application — this is a cross-site navigation and `Strict` prevents the session cookie from being sent with the callback, breaking login. `SameSite = Lax` sends the cookie for top-level navigations (including OAuth callbacks) but blocks it for cross-site subrequests (fetch, XHR, form POSTs from other sites) — this is the correct CSRF protection level for an OAuth-integrated application. Add the `__Host-` cookie prefix for defence-in-depth: `__Host-Session` requires Secure=true, Path=/, no Domain attribute.",
  "rejectedOutput": "Set both Secure=true and SameSite=Strict. Strict is the most secure option and the OAuth callback can be handled by redirecting to a login page if the session cookie is missing.",
  "arbiters": ["owasp-asvs-v3-4-cookie-security", "samesite-strict-oauth-callback-break"],
  "whyAccepted": "Lax SameSite allows OAuth callback redirect to include the session cookie (top-level navigation). Strict blocks it, breaking login for OAuth flows. The CSRF protection of Lax is sufficient for the partner-link use case.",
  "whyRejected": "'Redirect to login page if session missing' changes the user experience: clicking a partner link goes to the application page, then immediately redirects to login because the session cookie was not sent. This is a UX regression caused by Strict. Lax provides CSRF protection without this regression.",
  "sourceAnchors": [
    "https://owasp.org/www-project-application-security-verification-standard/",
    "https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie/SameSite"
  ],
  "licenseConstraint": "CC-BY-3.0",
  "trainingStatus": "APPROVED"
}
```
