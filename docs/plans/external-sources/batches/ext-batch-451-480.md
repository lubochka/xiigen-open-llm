# External Sources Batch 16 — EXT-311 through EXT-330

Generated: 2026-05-04  
Ledger range: EXT-311 – EXT-330  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, EFCORE-SOURCE, DOTNET-MICROSERVICES, MICROSOFT-LEARN-ARCH, ISTIO-DOCS, CWE-TOP25, WIDEWORLDIMPORTERS

---

## EXT-311

```json
{
  "rowId": "EXT-311",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Kubernetes cluster runs a stateful application (PostgreSQL) and needs to handle node drains gracefully. Draining a node currently terminates the PostgreSQL pod immediately, causing 15-30 seconds of downtime.",
    "b": "Solutions: (A) PodDisruptionBudget (PDB) with `minAvailable: 1` + preStop lifecycle hook sleeping 10s. (B) Pod priority class `PriorityClass` set to high value preventing eviction. (C) `terminationGracePeriodSeconds: 60` on the pod spec.",
    "c": "PostgreSQL uses a primary-replica setup. The primary is the pod being drained. Failover takes ~10 seconds."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Combine option A (PDB + preStop hook) with option C (terminationGracePeriodSeconds). The PDB with `minAvailable: 1` ensures the drain blocks until a replacement pod is ready — preventing eviction while no healthy replica exists. The preStop hook sleeping 5-10 seconds gives the load balancer time to route connections away from the pod before the PostgreSQL process receives SIGTERM. `terminationGracePeriodSeconds: 60` ensures PostgreSQL has time to flush WAL (Write-Ahead Log) and close connections cleanly. Option B (high PriorityClass) prevents voluntary eviction (drain) but also prevents the node from being drained at all — defeating the purpose of maintenance. The three mechanisms are complementary: PDB controls when eviction is allowed, preStop controls connection draining, and terminationGracePeriod controls shutdown duration.",
  "rejectedOutput": "Set a high PriorityClass on the PostgreSQL pod. This prevents the scheduler from evicting it during node pressure events and drain operations, providing the strongest protection.",
  "arbiters": ["kubernetes-pdb-spec", "postgres-graceful-shutdown"],
  "whyAccepted": "PDB + preStop + terminationGracePeriod is the correct layered approach: PDB gates eviction timing, preStop drains connections, terminationGracePeriod allows clean WAL flush.",
  "whyRejected": "High PriorityClass prevents both involuntary and voluntary eviction — node drain (`kubectl drain`) will hang indefinitely if the pod refuses eviction, blocking maintenance.",
  "sourceAnchors": [
    "https://kubernetes.io/docs/tasks/run-application/configure-pdb/",
    "https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-termination"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-312

```json
{
  "rowId": "EXT-312",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement `batch_rename_files(directory: str, pattern: str, replacement: str) -> list[str]` that renames all files in a directory where the filename matches `pattern` (a regex), replacing the match with `replacement`. Returns list of new filenames.",
    "b": "Submitted implementation uses `os.system(f'mv {old_path} {new_path}')` for each rename.",
    "c": "Test inputs include filenames with spaces: 'my report 2024.pdf', and filenames with shell metacharacters: 'data$(date).csv'."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Reject the os.system() approach as a command injection vulnerability (CWE-78). Replace with `os.rename(old_path, new_path)` — a direct syscall that does not invoke a shell. The `mv` command via os.system() requires shell interpolation: a filename containing `$(date)` would execute `date` as a subcommand. Filenames with spaces require quoting which the naive f-string does not provide. The correct implementation: (1) `pathlib.Path(directory).iterdir()` to enumerate files, (2) `re.sub(pattern, replacement, filename)` for the rename transform, (3) `old_path.rename(new_path)` for the atomic rename — no shell involved. Returns the list of new names only for files that were actually renamed.",
  "rejectedOutput": "The os.system() approach works correctly for normal filenames. Add shlex.quote() around both paths to handle spaces: `os.system(f'mv {shlex.quote(old)} {shlex.quote(new)}')`.",
  "arbiters": ["cwe-78-os-command-injection", "pathlib-rename-safety"],
  "whyAccepted": "os.rename() is a direct syscall with no shell, no quoting requirements, and no injection surface. It handles all filenames including spaces and metacharacters natively.",
  "whyRejected": "shlex.quote() mitigates the quoting issue but still invokes a shell subprocess for each rename, adding process spawn overhead. os.rename() is simpler, faster, and safe without shell involvement.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench",
    "https://cwe.mitre.org/data/definitions/78.html"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-313

```json
{
  "rowId": "EXT-313",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A SWE-bench issue: a Flask application returns 500 on POST /upload when the uploaded file exceeds 16MB (Flask default MAX_CONTENT_LENGTH). Three agent fixes: (A) Increase MAX_CONTENT_LENGTH to 100MB in config. (B) Add a 413 Content Too Large error handler returning JSON `{error: 'File too large', max_size_mb: 16}`. (C) Both: increase limit to 100MB AND add the error handler.",
    "b": "The application is a document processing service. Max legitimate file size is 50MB (PDF reports). The issue reporter uploaded a 45MB file.",
    "c": "Current 16MB limit was a Flask default never explicitly configured."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Select option C: increase MAX_CONTENT_LENGTH to 50MB (not 100MB — use the documented business maximum) AND add the 413 error handler. The 16MB limit was a default, not a deliberate policy — 45MB PDF reports are legitimate use cases. Raising the limit to 50MB accommodates all legitimate files while preventing arbitrary uploads. The 413 error handler is independently valuable: without it, clients receive a generic 500 that does not communicate the actionable cause ('file too large'). RFC 9110 specifies 413 Content Too Large for this exact case. The handler should return `{error: 'file_too_large', limit_mb: 50}` with `Content-Type: application/problem+json` per RFC 9457. Option A alone (no error handler) leaves the 500 anti-pattern for future oversized uploads. Option B alone does not fix the legitimate 45MB use case.",
  "rejectedOutput": "Apply option B only: add the 413 error handler. The 16MB limit may be intentional and increasing it requires a security review. The error handler improves the API response without changing policy.",
  "arbiters": ["rfc-9110-413-status", "flask-max-content-length"],
  "whyAccepted": "The 16MB limit is demonstrably a Flask default (never explicitly configured) and the legitimate use case is 45MB. Fix both root cause (limit too low) and symptom (wrong error code).",
  "whyRejected": "Option B alone treats 45MB PDFs as invalid requests. The 'security review required' concern is valid for 100MB but 50MB matching the business document size is proportionate.",
  "sourceAnchors": [
    "https://flask.palletsprojects.com/en/latest/config/#MAX_CONTENT_LENGTH",
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.5.14"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-314

```json
{
  "rowId": "EXT-314",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-LearningCycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.9 data pipeline uses `collections.namedtuple` for intermediate data structures (DataPoint, BatchRecord, ValidationResult — 14 total namedtuples). PEP-557 dataclasses and PEP-544 Protocol are available. Evaluate migrating to dataclasses.",
    "b": "Current namedtuples are: immutable, used with tuple unpacking in several places, and passed to functions that type-check with isinstance(). 6 of 14 have default values set via `defaults=` parameter. 3 are used as dictionary keys (hashable requirement).",
    "c": "Team has Python 3.10 minimum. No third-party serialization library; JSON serialization done manually via `._asdict()`."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "SELECTIVE migration: convert 8 namedtuples that have no hashing or unpacking requirements to `@dataclass(frozen=True)` — this provides: named field access, default values via field(), type annotations, and `__repr__` without `._asdict()` hack (use `dataclasses.asdict()` instead). Keep the 3 dictionary-key namedtuples as namedtuples — `@dataclass(frozen=True)` generates `__hash__` but only if `eq=True` (default), so it would work, but namedtuple is simpler for pure-data hash keys. Keep 3 namedtuples used with tuple unpacking if the unpacking is load-bearing (dataclasses do not support positional unpacking). Migration gate: run existing test suite with zero regressions. Do not migrate all 14 at once.",
  "rejectedOutput": "Migrate all 14 namedtuples to dataclasses. Dataclasses are the modern Python idiom, support type annotations natively, and the migration is mechanical.",
  "arbiters": ["pep-557-dataclasses", "namedtuple-hashability"],
  "whyAccepted": "Selective migration preserves hashability for dict-key types and tuple-unpacking patterns. `@dataclass(frozen=True)` is correct for the majority but not a universal drop-in for all namedtuple use cases.",
  "whyRejected": "Bulk migration ignores tuple-unpacking usage (dataclasses break positional unpacking) and incorrectly assumes all namedtuple features map 1:1 to dataclasses.",
  "sourceAnchors": [
    "https://peps.python.org/pep-0557/",
    "https://docs.python.org/3/library/collections.html#collections.namedtuple"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-315

```json
{
  "rowId": "EXT-315",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservice generates spans with `http.request.body` attribute containing full request bodies for debugging. The service handles 10,000 RPS and each request body averages 2KB. Storage cost for this attribute alone is ~172 GB/day.",
    "b": "Three proposals: (A) Remove `http.request.body` entirely. (B) Truncate to first 256 bytes with a `truncated=true` marker attribute. (C) Store full body only for error spans (tail-based sampling on error traces).",
    "c": "The debug use case is: reproducing customer-reported bugs by replaying the exact request. PII (Personally Identifiable Information) may be present in request bodies."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Apply option C (error-only full body storage) with a PII scrubbing step. Storing full request bodies on error spans satisfies the debug-replay use case (bugs manifest as errors) while reducing volume by ~99% (assuming 1% error rate). Add a PII scrubbing processor before export: redact fields matching patterns for email, phone, SSN (Social Security Number), credit card using a configurable allowlist approach. Option A loses all debug value. Option B (256-byte truncation) is insufficient for 2KB bodies — most requests would be truncated beyond the useful payload boundary. The OTel (OpenTelemetry) Collector processor pipeline supports attribute transformation before export; implement scrubbing there to keep service code clean. Gate: PII scrubbing must be validated with a test corpus before enabling in production.",
  "rejectedOutput": "Remove http.request.body entirely (option A). The PII risk is too high to store any body content in traces, and the 172 GB/day cost is unacceptable.",
  "arbiters": ["otel-attribute-pii-scrubbing", "tail-sampling-error-traces"],
  "whyAccepted": "Error-only storage with PII scrubbing preserves debug-replay capability for the 1% of requests that matter while reducing storage 99x and managing PII risk through scrubbing.",
  "whyRejected": "Removing body entirely eliminates the debug-replay use case that motivated the attribute in the first place. A PII-scrubbing + error-sampling approach addresses both concerns simultaneously.",
  "sourceAnchors": [
    "https://opentelemetry.io/docs/collector/transforming-telemetry/",
    "https://opentelemetry.io/docs/specs/semconv/http/http-spans/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-316

```json
{
  "rowId": "EXT-316",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Dapper-based repository builds search queries dynamically: `var sql = \"SELECT * FROM Products WHERE 1=1\"; if (name != null) sql += \" AND Name = '\" + name + \"'\"; if (category != null) sql += \" AND Category = '\" + category + \"'\";`",
    "b": "The query is passed directly to `connection.Query<Product>(sql)` with no parameters.",
    "c": "Name and category values come from a public API endpoint query string."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Rewrite using Dapper's parameterized query with a DynamicParameters object. Replace string concatenation with: `var params = new DynamicParameters(); var conditions = new List<string>(); if (name != null) { conditions.Add(\"Name = @Name\"); params.Add(\"Name\", name); } if (category != null) { conditions.Add(\"Category = @Category\"); params.Add(\"Category\", category); } var where = conditions.Any() ? \"WHERE \" + string.Join(\" AND \", conditions) : \"\"; var sql = $\"SELECT * FROM Products {where}\"; connection.Query<Product>(sql, params);`. This eliminates SQL injection (CWE-89) by passing values as parameters, not interpolated strings. The `WHERE 1=1` hack is replaced with explicit condition accumulation. DynamicParameters handles DBNull for null values correctly.",
  "rejectedOutput": "Sanitize the input by escaping single quotes: replace `'` with `''` before concatenation. This prevents SQL injection while keeping the dynamic query construction approach.",
  "arbiters": ["cwe-89-sql-injection", "dapper-dynamic-parameters"],
  "whyAccepted": "Parameterized queries via DynamicParameters is the correct fix: values are never interpreted as SQL. The database engine receives query structure and data separately.",
  "whyRejected": "Single-quote escaping is insufficient: SQL injection has many forms (backslash escape in some databases, numeric injection without quotes, stored procedure injection). Parameterization is the only correct defense.",
  "sourceAnchors": [
    "https://github.com/DapperLib/Dapper#parameterized-queries",
    "https://cwe.mitre.org/data/definitions/89.html"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-317

```json
{
  "rowId": "EXT-317",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 application has a ProductListComponent that renders 200 product cards. Each card has a complex template with 8 bindings. The component uses default change detection. User reports sluggish UI when filtering products (200ms+ per keystroke).",
    "b": "Proposed fixes: (A) Switch to OnPush change detection strategy. (B) Use trackBy function in ngFor. (C) Use both OnPush and trackBy. (D) Virtualise the list with CDK virtual scrolling.",
    "c": "All 200 products are visible simultaneously (no pagination). Filter updates the displayed subset but list never exceeds 200 items."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Apply option C: OnPush + trackBy. OnPush (Angular's ChangeDetectionStrategy) limits re-render triggers to: input reference changes, events originating from the component, async pipe emits, and explicit markForCheck() calls. This eliminates unnecessary re-renders during unrelated parent updates. trackBy in ngFor (e.g., `trackBy: trackByProductId`) prevents Angular from destroying and recreating all 200 DOM (Document Object Model) nodes when the filtered array reference changes — instead it reconciles by ID, reusing existing DOM nodes. Option D (virtual scrolling) is unnecessary when all 200 items must be visible simultaneously; virtual scrolling only helps when items scroll off screen. 200 items × 8 bindings = 1,600 bindings — OnPush + trackBy reduces this to only changed items.",
  "rejectedOutput": "Apply virtual scrolling (CDK VirtualScrollViewport). Rendering 200 items simultaneously is the root cause; virtual scrolling limits visible DOM nodes to the viewport.",
  "arbiters": ["angular-onpush-change-detection", "angular-trackby-reconciliation"],
  "whyAccepted": "OnPush + trackBy correctly targets the actual bottleneck: unnecessary change detection cycles and full list re-creation on filter. Virtual scrolling requires items to scroll — 200 visible-all-at-once items don't scroll.",
  "whyRejected": "Virtual scrolling requires a fixed-height container with scrollable items. If all 200 products must be visible simultaneously (per the requirement), virtual scrolling cannot be applied.",
  "sourceAnchors": [
    "https://angular.dev/guide/components/advanced-configuration#onpush",
    "https://angular.dev/guide/built-in-directives#ngfor-trackby"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-318

```json
{
  "rowId": "EXT-318",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React 18 application has a SearchPage component that fetches results on query change with a useEffect. Users experience a flash of the previous results while new results load. The team wants to show previous results (greyed out) while the new fetch is in progress.",
    "b": "Options: (A) Store previous results in a separate useState and display them when loading=true. (B) useDeferredValue(query) to defer the query update while keeping previous results rendered. (C) useTransition() with startTransition to mark the query update as non-urgent.",
    "c": "Fetch is triggered by query string changes (user typing in a search box). Results fetch takes 200-800ms."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use useDeferredValue (option B). `useDeferredValue(query)` returns a deferred copy of the query that lags behind the current value — the component renders with the old query's results while the new fetch is in progress, naturally providing the 'previous results greyed out' pattern via a CSS opacity style on the results container: `style={{ opacity: deferredQuery !== query ? 0.5 : 1 }}`. useTransition (option C) is the right choice when the transition is triggered by a user action (button click) — it wraps the state setter in startTransition. For input-driven queries where the state update is automatic (controlled input), useDeferredValue is more appropriate. Option A (manual previous-state tracking) requires additional useState + useEffect and is error-prone when managing loading state transitions.",
  "rejectedOutput": "Store previous results in useState: when loading=true, display previousResults with 50% opacity. When loading=false, update previousResults and display results at full opacity.",
  "arbiters": ["react-use-deferred-value", "react-use-transition-input"],
  "whyAccepted": "useDeferredValue is React 18's idiomatic solution for showing stale content during pending updates triggered by controlled inputs. It requires zero additional state management.",
  "whyRejected": "Manual previous-results state requires synchronizing 3 state values (query, results, previousResults) and the loading flag — race conditions and stale closure bugs are likely.",
  "sourceAnchors": [
    "https://react.dev/reference/react/useDeferredValue",
    "https://react.dev/reference/react/useTransition"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-319

```json
{
  "rowId": "EXT-319",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core 8 API uses `[Authorize]` attribute on all controllers. A new endpoint `/api/public/status` must be accessible without authentication. Two implementations: (A) Add `[AllowAnonymous]` to the status endpoint action. (B) Move the status endpoint to a separate controller and add `[AllowAnonymous]` at the controller level.",
    "b": "The status endpoint is the only public endpoint. The controller currently also contains `/api/public/version` which also should be public.",
    "c": "Global AuthorizationPolicy is applied via `app.UseAuthorization()` middleware."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Option B: separate public controller with `[AllowAnonymous]` at the controller level. With two public endpoints (`/status` and `/version`) both in the same controller, applying `[AllowAnonymous]` at the controller level is cleaner than two separate action-level attributes. More importantly, a dedicated `PublicController` (or `DiagnosticsController`) makes the security posture explicit: any action added to this controller is public by default. This prevents the common mistake of adding a new action to the public controller without realising it will be unauthenticated. Document this convention in the controller's XML summary comment. If only `/status` were public and `/version` were protected, option A (action-level) would be correct.",
  "rejectedOutput": "Use option A: add [AllowAnonymous] to each public endpoint individually. This is more explicit — each public endpoint is visibly marked rather than inheriting from a controller-level attribute.",
  "arbiters": ["aspnetcore-authorization-policy", "security-by-default-controller"],
  "whyAccepted": "With multiple public endpoints in the same controller, controller-level [AllowAnonymous] plus a clear controller name/comment is more maintainable than per-action decoration.",
  "whyRejected": "Per-action [AllowAnonymous] in a mixed controller (public and protected endpoints in the same class) is error-prone: a developer adding a new action may not notice the class has protected siblings.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple",
    "https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-320

```json
{
  "rowId": "EXT-320",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B3-ServiceMeshRouting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Add request correlation ID tracking to the NestJS API — every incoming request should have a unique correlation ID in logs, and the ID should be propagated to all downstream HTTP calls.'",
    "b": "Implementation options: (A) Custom middleware generating UUID v4 per request, storing in AsyncLocalStorage, reading in LoggerService and HTTP interceptor. (B) `cls-hooked` (Continuation Local Storage) library for request context. (C) Using NestJS `AsyncLocalStorage` provider with a scoped injection token.",
    "c": "NestJS version 10. Logger is a custom Pino logger adapter. Downstream calls use HttpService (Axios wrapper)."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Use option A: custom AsyncLocalStorage-based middleware. Action items: (1) Create `CorrelationIdMiddleware` that reads `X-Correlation-ID` header (or generates UUID v4 if absent) and stores it in an `AsyncLocalStorage<string>` store. (2) Register middleware globally: `consumer.apply(CorrelationIdMiddleware).forRoutes('*')`. (3) Create `CorrelationIdService` that reads from the AsyncLocalStorage store. (4) Inject `CorrelationIdService` into the Pino logger adapter to add `correlationId` to every log line. (5) Create `CorrelationIdInterceptor` that adds `X-Correlation-ID` header to every outgoing Axios request via HttpService. (6) Return `X-Correlation-ID` in response headers for client-side correlation. Option B (`cls-hooked`) is a CLS (Continuation Local Storage) monkey-patch that predates Node.js native AsyncLocalStorage and has known issues with async/await. Option C (scoped injection) cannot propagate across singleton services like LoggerService.",
  "rejectedOutput": "Use cls-hooked library. It is battle-tested for NestJS request context propagation and the NestJS community has established patterns for it.",
  "arbiters": ["node-async-local-storage", "nestjs-request-scoping"],
  "whyAccepted": "Node.js native AsyncLocalStorage is stable since Node 16 and does not require monkey-patching. It correctly propagates context across async boundaries including Promise chains and async/await.",
  "whyRejected": "cls-hooked uses deprecated `async_hooks` API (domain-based) and has known propagation failures with async/await in modern Node.js. AsyncLocalStorage supersedes it.",
  "sourceAnchors": [
    "https://nodejs.org/api/async_context.html#class-asynclocalstorage",
    "https://docs.nestjs.com/middleware"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-321

```json
{
  "rowId": "EXT-321",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C3-OutputFormatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API endpoint returns search results with cursors for pagination. The response includes `next_cursor` and `prev_cursor` fields. A developer proposes replacing cursor-based pagination with Link header pagination (RFC 5988) — putting cursor values in `Link: <url>; rel=\"next\"` headers instead of the response body.",
    "b": "Consumers: REST API clients in JavaScript (fetch), Python (requests), and a Go SDK. The Go SDK currently reads `next_cursor` from the JSON body.",
    "c": "RFC 5988 Web Linking is the standard for Link header pagination used by GitHub API."
  },
  "function": "finalMachineResults_to_userFormat",
  "acceptedOutput": "DEFER the migration. Add Link headers as an ADDITIONAL output alongside the existing body fields rather than replacing them. Rationale: the Go SDK already reads `next_cursor` from the JSON body — removing it is a breaking API change requiring a major version bump and SDK update. RFC 5988 Link headers are useful for discoverability (HAL/hypermedia clients) but JavaScript fetch and Python requests require explicit header parsing — they do not automatically follow Link headers. The correct migration path: (1) Add `Link: <url?cursor=X>; rel=\"next\"` headers NOW (non-breaking addition). (2) In the next major API version, deprecate body cursor fields. (3) Remove body cursor fields only when SDK adoption of the new version is confirmed. This is the GitHub API migration pattern.",
  "rejectedOutput": "Replace body cursor fields with Link headers immediately. RFC 5988 is the standard and the API should follow it. SDK updates are the SDK maintainer's responsibility.",
  "arbiters": ["rfc-5988-web-linking", "api-backwards-compatibility"],
  "whyAccepted": "Adding Link headers non-breakingly alongside existing body fields allows both consumers to work simultaneously during transition. Removing body fields immediately breaks the Go SDK.",
  "whyRejected": "Immediate replacement is a breaking change to a public API. 'SDK updates are the SDK maintainer's responsibility' is technically true but creates a bad developer experience and adoption cliff.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc5988",
    "https://docs.github.com/en/rest/using-the-rest-api/using-pagination-in-the-rest-api"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-322

```json
{
  "rowId": "EXT-322",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "An API resource `Order` has a `status` field (PENDING, PROCESSING, SHIPPED, DELIVERED, CANCELLED). A client wants to transition an order from PROCESSING to CANCELLED. Two API designs: (A) PATCH /orders/{id} with `{status: 'CANCELLED'}` via field mask. (B) POST /orders/{id}:cancel — a custom method per AIP-136.",
    "b": "The cancellation has side effects: releases inventory reservation, triggers refund workflow, sends customer notification. Evaluate per AIP-136.",
    "c": "AIP-136 covers custom methods for actions that don't map cleanly to standard CRUD (Create Read Update Delete) verbs."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use POST /orders/{id}:cancel (option B, AIP-136 custom method). The cancellation is not a simple field update — it is an action with complex side effects (inventory release, refund, notification). Using PATCH to set `status=CANCELLED` implies that the client controls the status field directly, which would also allow setting `status=SHIPPED` or `status=DELIVERED` via PATCH — clearly wrong. A custom `:cancel` method encapsulates the business action, enforces state machine transitions (only PROCESSING orders can be cancelled), and makes the side effects explicit in the API contract. AIP-136 specifies: use a custom method when the operation is a specific action rather than a resource mutation. The response should be the updated Order resource.",
  "rejectedOutput": "Use PATCH with field mask. It is RESTful and requires no special client handling. The side effects are implementation details that don't affect the API contract.",
  "arbiters": ["aip-136-custom-methods", "state-machine-api-design"],
  "whyAccepted": "Custom method `:cancel` enforces state machine semantics (not all status transitions are valid) and makes side effects explicit. PATCH on status would allow invalid transitions.",
  "whyRejected": "PATCH with `{status: 'CANCELLED'}` opens the status field to arbitrary client-driven state transitions, bypassing business rules. Side effects ARE part of the API contract — clients need to know cancellation triggers a refund.",
  "sourceAnchors": [
    "https://google.aip.dev/136",
    "https://google.aip.dev/134"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-323

```json
{
  "rowId": "EXT-323",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C2-ResultIntegration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ML training job fails with: 'RuntimeError: Expected all tensors to be on the same device, but found at least two devices, cuda:0 and cpu!' at loss.backward().",
    "b": "Three operator responses: (R1) Move all tensors to CPU for compatibility. (R2) Identify which tensor is on CPU and move it to cuda:0. (R3) Add .cuda() to all tensors throughout the training loop.",
    "c": "The model was loaded with `.to(device)` where `device = torch.device('cuda:0')`. The loss function is `nn.CrossEntropyLoss()`."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Select R2: identify and move the specific CPU tensor to cuda:0. The error indicates one tensor is on CPU while others are on CUDA. Common cause: the loss function criterion (`nn.CrossEntropyLoss()`) was instantiated without `.to(device)`. Fix: `criterion = nn.CrossEntropyLoss().to(device)`. Alternatively, the labels tensor (ground truth targets) may not have been moved to device — check `labels = labels.to(device)` in the data loading loop. R1 (move all to CPU) eliminates the GPU acceleration that motivated using CUDA in the first place. R3 (add .cuda() everywhere) is brittle: it hardcodes cuda:0, breaks on CPU-only machines, and may not find all instances — the correct pattern is `.to(device)` with a configurable device variable.",
  "rejectedOutput": "Apply R3: add .cuda() to all tensors throughout the training loop. This ensures every tensor is on GPU and eliminates the device mismatch.",
  "arbiters": ["pytorch-device-management", "cuda-tensor-placement"],
  "whyAccepted": "Identifying the specific misplaced tensor (criterion or labels) and moving it with `.to(device)` is surgical and correct. The device variable pattern is robust across CUDA and CPU environments.",
  "whyRejected": "R3 adds .cuda() everywhere but hardcodes GPU — breaks on CPU-only CI/CD (Continuous Integration/Continuous Deployment) environments. Missing even one tensor still causes the error.",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1",
    "https://pytorch.org/docs/stable/tensor_attributes.html#torch-device"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-324

```json
{
  "rowId": "EXT-324",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-SemanticMapping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A multilingual NLP pipeline uses Tatoeba sentence pairs to fine-tune a translation model. The data preprocessing concatenates source and target sentences with a separator: `input = src_sentence + \" [SEP] \" + tgt_sentence`. This is used as input to a seq2seq encoder.",
    "b": "The separator `[SEP]` is not in the model's vocabulary. The tokeniser silently splits it into `[`, `SEP`, `]` as three separate tokens.",
    "c": "The model is a pre-trained MarianMT model. MarianMT uses a SentencePiece tokeniser."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Remove the [SEP] concatenation pattern entirely. For seq2seq translation models (including MarianMT), the source and target sentences are separate inputs — the encoder processes the source sentence, and the decoder auto-regressively generates the target. Concatenating source+target with [SEP] as encoder input is not the MarianMT training protocol; it introduces a spurious pattern the model was not trained to handle. For fine-tuning, provide source in `input_ids` and target in `labels` separately per the `MarianMTModel` API. If the goal is a cross-lingual embedding model (not seq2seq translation), use a model designed for that purpose (e.g., LASER, LaBSE) with the correct separator token for that model family.",
  "rejectedOutput": "Add [SEP] to the model's vocabulary and retrain the tokeniser's special token configuration. This preserves the concatenation approach while fixing the unknown-token issue.",
  "arbiters": ["marian-mt-seq2seq-protocol", "sentencepiece-tokeniser"],
  "whyAccepted": "Seq2seq models encode source and decode target — they do not take concatenated source+target as input. The [SEP] pattern is architecturally wrong, not just a tokeniser configuration issue.",
  "whyRejected": "Adding [SEP] to vocabulary treats a fundamental architecture mismatch as a tokeniser configuration problem. Even with the token added, concatenating src+tgt as encoder input diverges from the pre-training protocol.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://huggingface.co/docs/transformers/model_doc/marian"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-325

```json
{
  "rowId": "EXT-325",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An EF Core 8 application performs bulk inserts of 10,000+ records. Current implementation: loop calling `context.Add(entity)` and `context.SaveChanges()` per entity (10,000 round trips). Proposed options: (A) Batch all entities then call `context.SaveChanges()` once (EF Core 7+ batching, default batch size 42). (B) Use `context.BulkInsert()` from EFCore.BulkExtensions NuGet package. (C) Use raw SQL `INSERT INTO ... VALUES (...)` with table-valued parameter.",
    "b": "Entities have no navigation properties to include. Operation is append-only (no update). Occurs once daily during data pipeline.",
    "c": "SQL Server target. EF Core 8."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option A (EF Core SaveChanges batching) as the first choice, with option B (BulkExtensions) if benchmarking shows A is insufficient. EF Core 7+ groups consecutive INSERT statements into batched round trips (default batch size 42, configurable). For 10,000 simple entities with no navigation properties, EF Core batching reduces round trips from 10,000 to ~238 (10,000/42). This is likely sufficient for a once-daily operation. BulkExtensions (option B) uses SqlBulkCopy internally — much faster for very large batches but adds an external dependency and bypasses EF Core's change tracker (no EF events or interceptors fire). Option C (raw SQL TVP (Table-Valued Parameter)) is the lowest-level approach: correct but high implementation complexity. Recommendation: benchmark option A first; use option B only if latency SLA (Service Level Agreement) requires sub-second insertion of 10,000 records.",
  "rejectedOutput": "Use BulkExtensions immediately. EF Core's built-in batching is too slow for 10,000 records and BulkExtensions is the community standard for bulk operations in EF Core.",
  "arbiters": ["efcore-batching-savechanges", "sqlbulkcopy-performance"],
  "whyAccepted": "EF Core 7+ batching handles 10,000 records in ~238 round trips — likely sufficient for a once-daily batch operation. BulkExtensions adds a dependency and bypasses EF events; measure first.",
  "whyRejected": "BulkExtensions is correct for very high-frequency bulk operations but is premature optimisation for a once-daily pipeline without a latency SLA that rules out EF batching.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/performance/efficient-updating",
    "https://github.com/borisdj/EFCore.BulkExtensions"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-326

```json
{
  "rowId": "EXT-326",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET 6 service uses `IHostedService` with a manual `while(!cancellationToken.IsCancellationRequested)` loop for background processing. .NET 6+ introduced `BackgroundService` abstract class with a `ExecuteAsync(CancellationToken)` override pattern. Evaluate migration.",
    "b": "`BackgroundService` is a base class implementing `IHostedService` that handles Start/Stop lifecycle and exposes `ExecuteAsync`. The manual while-loop pattern is not deprecated but BackgroundService is the idiomatic approach.",
    "c": "The service processes a queue with a 500ms poll interval. Current implementation has 3 places where the while-condition is checked manually."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "MIGRATE to BackgroundService. The migration is low-risk and directly improves correctness: the current implementation with 3 manual cancellation token checks has a subtle risk — if an exception is thrown inside the loop body before the token check, the service may not stop cleanly. `BackgroundService.ExecuteAsync` is the single correct pattern: pass the cancellationToken to all async operations inside the loop (await queue.DequeueAsync(cancellationToken) naturally handles the 500ms poll with zero busy-wait). The while-condition simplifies to `while (!stoppingToken.IsCancellationRequested)` with one check. Migration: (1) Change `class QueueProcessor : IHostedService` to `: BackgroundService`. (2) Move the loop body into `ExecuteAsync(CancellationToken stoppingToken)`. (3) Remove the manual `StartAsync`/`StopAsync` implementations. (4) Pass `stoppingToken` to all awaitable calls inside the loop.",
  "rejectedOutput": "Keep the IHostedService implementation. It works correctly and BackgroundService is just syntactic sugar. Migration risk is not justified.",
  "arbiters": ["dotnet-background-service", "cancellation-token-propagation"],
  "whyAccepted": "BackgroundService eliminates the 3 manual token checks and ensures the token is propagated into async operations, enabling cooperative cancellation rather than just loop exit.",
  "whyRejected": "The claim that the current implementation 'works correctly' understates the risk: 3 manual token checks means an exception between checks can prevent clean shutdown. BackgroundService is not merely syntactic sugar.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/dotnet/core/extensions/workers",
    "https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Hosting.Abstractions/src/BackgroundService.cs"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-327

```json
{
  "rowId": "EXT-327",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservices system has 8 services that all need to call a shared 'notification service' that sends emails and SMS. Current design: each of the 8 services calls the notification service directly via HTTP. The team proposes moving to an event-driven design where services publish a NotificationRequested event to a message bus.",
    "b": "Compare: direct HTTP coupling vs event-driven decoupling for notification delivery.",
    "c": "Notification service SLA (Service Level Agreement) is 99.5% availability. Email delivery is eventually consistent (SMTP queue). SMS delivery requires < 5 second latency for OTP (One-Time Password) messages."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use event-driven design with a caveat for OTP SMS. For email (eventually consistent by nature): publish NotificationRequested event to the message bus — the notification service consumes and delivers asynchronously. This decouples the 8 calling services from notification service availability: if notification is down, events queue in the bus and deliver when it recovers. For OTP SMS (< 5 second latency requirement): direct HTTP call is necessary — event bus adds queueing latency that violates the 5-second OTP window. The notification service should expose both interfaces: an HTTP endpoint for synchronous OTP delivery and a message bus consumer for asynchronous email. This is the Competing Consumers pattern with a synchronous escape hatch for latency-sensitive paths.",
  "rejectedOutput": "Move all notification types to event-driven. The 5-second OTP requirement can be met by setting high message priority on the bus and using a dedicated OTP queue.",
  "arbiters": ["event-driven-decoupling", "otp-latency-constraint"],
  "whyAccepted": "Email is naturally async (SMTP queue); event-driven matches its semantics. OTP SMS requires guaranteed sub-5-second delivery that event bus queuing cannot guarantee under load.",
  "whyRejected": "Message priority and dedicated queues reduce OTP latency but do not eliminate it. Under bus backpressure, even high-priority messages experience latency. HTTP synchronous call is the only reliable < 5s path.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers",
    "https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/event-driven"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-328

```json
{
  "rowId": "EXT-328",
  "sourceIds": ["ISTIO-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Kubernetes cluster uses Istio service mesh. A new service (payment-service) is being deployed. Security team requires mTLS (mutual TLS) between payment-service and all other services. Two proposals: (A) Set PeerAuthentication policy to STRICT mode for the payment-service namespace. (B) Set PeerAuthentication STRICT at the cluster-wide mesh level.",
    "b": "Other services in other namespaces are currently using PERMISSIVE mode (accepts both mTLS and plain text) during migration.",
    "c": "payment-service is the only fully mTLS-compliant service. Legacy services in other namespaces still have plain-text traffic."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Apply PeerAuthentication STRICT at the payment-service namespace level only (option A). Setting STRICT at the cluster-wide mesh level would reject plain-text traffic from legacy services in other namespaces that are still in PERMISSIVE migration mode — this would break inter-service communication for those legacy services before they are ready. Namespace-scoped STRICT for payment-service achieves the security requirement (only mTLS traffic reaches payment-service) without disrupting legacy services. The migration path is: namespace-level STRICT for payment-service now → incrementally add namespace-level STRICT to other namespaces as they become mTLS-ready → eventually apply cluster-wide STRICT when all namespaces are compliant.",
  "rejectedOutput": "Apply cluster-wide STRICT immediately. All services should use mTLS and accelerating the migration forces legacy services to become compliant sooner.",
  "arbiters": ["istio-peer-authentication-scope", "mtls-migration-strategy"],
  "whyAccepted": "Namespace-scoped STRICT isolates the security requirement to payment-service while preserving PERMISSIVE mode for legacy services that are not yet mTLS-ready.",
  "whyRejected": "Cluster-wide STRICT breaks legacy services that still use plain-text inter-service traffic. 'Forces compliance sooner' is an operational risk, not a benefit, when services are unprepared.",
  "sourceAnchors": [
    "https://istio.io/latest/docs/reference/config/security/peer_authentication/",
    "https://istio.io/latest/docs/tasks/security/authentication/mtls-migration/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-329

```json
{
  "rowId": "EXT-329",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A web application generates PDF reports by passing user-supplied title and author fields into a command: `os.system(f'pdflatex -jobname={title} report.tex')`. The `title` field is validated only for length (max 50 characters).",
    "b": "CWE-77 (Command Injection). Evaluate the vulnerability and propose the correct fix.",
    "c": "pdflatex is installed on the server. The application is a SaaS (Software as a Service) report generator. Users pay for access."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "This is a command injection vulnerability (CWE-77). A title value of `foo; rm -rf /` would execute `pdflatex -jobname=foo` then `rm -rf /` because os.system() invokes a shell. Length validation (max 50 chars) does not prevent injection — `; id #` is under 50 chars. Fix: replace os.system() with `subprocess.run(['pdflatex', '-jobname', title, 'report.tex'], check=True, capture_output=True)`. Using a list argument to subprocess.run() bypasses the shell entirely — each list element is passed as a distinct argument, so shell metacharacters in `title` are treated as literal characters. Additionally, validate the title against an allowlist (alphanumeric, spaces, hyphens) before use. Sandboxing pdflatex in a container with restricted filesystem access provides defence-in-depth.",
  "rejectedOutput": "Use shlex.quote() on the title before interpolation: `os.system(f'pdflatex -jobname={shlex.quote(title)} report.tex')`. This escapes shell metacharacters and prevents injection.",
  "arbiters": ["cwe-77-command-injection", "subprocess-list-args"],
  "whyAccepted": "subprocess.run() with list arguments passes data directly to execve() without shell involvement. No quoting, no escaping, no injection surface.",
  "whyRejected": "shlex.quote() is a mitigation, not a fix. It escapes known metacharacters but depends on the shell's quoting rules being correctly applied — edge cases exist. subprocess.run() with a list is the correct solution.",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/77.html",
    "https://docs.python.org/3/library/subprocess.html#security-considerations"
  ],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-330

```json
{
  "rowId": "EXT-330",
  "sourceIds": ["WIDEWORLDIMPORTERS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "The WideWorldImporters sample database uses SQL Server temporal tables (`PERIOD FOR SYSTEM_TIME`) for Orders, Customers, and Products tables. A developer queries historical order data using `SELECT * FROM Orders FOR SYSTEM_TIME AS OF '2024-01-01'`. They propose adding an application-level audit log table as an alternative.",
    "b": "The temporal table already exists and captures all changes automatically. The proposed audit log requires: a trigger on each table, an AuditLog table, and maintenance of the audit log schema when the Orders schema changes.",
    "c": "Compliance requirement: retain 7 years of change history. Orders table has 2M rows/year.",
    "c2": "The temporal table history table has automatic compression via SQL Server's page compression."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "REJECT the application-level audit log in favour of the existing temporal table. The temporal table already satisfies all requirements: (1) Compliance: 7-year retention is configurable via a retention policy (`HISTORY_RETENTION_PERIOD = 7 YEARS`). (2) Query: `FOR SYSTEM_TIME AS OF '2024-01-01'` retrieves the exact state of any row at any point in time — no audit log reconstruction needed. (3) Maintenance: temporal tables automatically capture changes — no triggers required, and schema changes to Orders are automatically reflected in the history table. (4) Performance: history table uses page compression. Adding an application-level audit log duplicates this functionality with higher complexity, trigger overhead on every DML (Data Manipulation Language) operation, and a schema coupling problem (audit log must mirror Orders schema changes).",
  "rejectedOutput": "Add the application-level audit log for flexibility. Temporal tables only capture row snapshots; the audit log can capture business context (who changed what and why) alongside the data changes.",
  "arbiters": ["sql-server-temporal-tables", "compliance-retention-policy"],
  "whyAccepted": "Temporal tables already provide automated, schema-aware, compressed history with FOR SYSTEM_TIME querying. Adding an audit log duplicates this at higher complexity and cost.",
  "whyRejected": "The 'who changed what and why' argument is valid for business context but should be implemented as additional columns on the temporal table (changed_by, change_reason) — not as a separate audit log that duplicates the snapshot functionality.",
  "sourceAnchors": [
    "https://github.com/microsoft/sql-server-samples/tree/master/samples/databases/wide-world-importers",
    "https://learn.microsoft.com/en-us/sql/relational-databases/tables/temporal-tables"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```
