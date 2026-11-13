# External Sources Batch 17 — EXT-331 through EXT-350

Generated: 2026-05-04  
Ledger range: EXT-331 – EXT-350  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, VUE-DOCS, EFCORE-SOURCE, DOTNET-MICROSERVICES, MICROSOFT-LEARN-ARCH, ISTIO-DOCS, CWE-TOP25

---

## EXT-331

```json
{
  "rowId": "EXT-331",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B2-DeploymentRouting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Implement zero-downtime rolling updates for a stateless API service running 10 replicas in Kubernetes.'",
    "b": "Key mechanisms to configure: maxSurge, maxUnavailable in RollingUpdate strategy, readinessProbe, and PodDisruptionBudget.",
    "c": "Service handles 500 RPS. Each pod handles ~50 RPS. Deployment must never drop below 8 healthy replicas (80% capacity)."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Action items: (1) Set Deployment strategy: `type: RollingUpdate, maxSurge: 2, maxUnavailable: 0` — this allows 2 extra pods during rollout while keeping all existing pods until new ones are ready. (2) Configure readinessProbe: HTTP GET /health with `initialDelaySeconds: 10, periodSeconds: 5, failureThreshold: 3` — Kubernetes only routes traffic to a pod when its readiness probe passes. (3) Set PodDisruptionBudget (PDB) `minAvailable: 8` — even during concurrent drain operations, at least 8 pods must remain available. (4) Set `terminationGracePeriodSeconds: 30` — allows in-flight requests to complete before pod termination. The combination guarantees zero-downtime: maxUnavailable=0 prevents any traffic-serving pods from being removed before a healthy replacement is ready; readinessProbe ensures new pods are actually serving traffic before old pods are removed; PDB enforces the 80% floor even during node maintenance.",
  "rejectedOutput": "Set maxUnavailable: 2 and maxSurge: 0 to keep total pod count constant during rollout. This avoids over-provisioning.",
  "arbiters": ["kubernetes-rolling-update-strategy", "readiness-probe-traffic-gate"],
  "whyAccepted": "maxUnavailable=0 is the zero-downtime guarantee: no pod is terminated until a ready replacement exists. maxSurge=2 provides the replacement pods. The PDB adds a floor for concurrent maintenance.",
  "whyRejected": "maxUnavailable=2 terminates 2 pods before replacements are ready — at 50 RPS/pod this drops capacity by 100 RPS (20%) during rollout, which is downtime.",
  "sourceAnchors": [
    "https://kubernetes.io/docs/concepts/workloads/controllers/deployment/#rolling-update-deployment",
    "https://kubernetes.io/docs/tasks/run-application/configure-pdb/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-332

```json
{
  "rowId": "EXT-332",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement `merge_config_files(base_path: str, override_path: str) -> dict` that deep-merges two JSON config files, with the override file winning on conflicts.",
    "b": "Submitted implementation uses `base.update(override)` — a shallow dict update.",
    "c": "Test case: base = `{database: {host: 'localhost', port: 5432, ssl: true}}`, override = `{database: {host: 'prod-db.example.com'}}`. Expected: `{database: {host: 'prod-db.example.com', port: 5432, ssl: true}}`. Actual: `{database: {host: 'prod-db.example.com'}}` — port and ssl silently dropped."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Reject the shallow update. `dict.update()` replaces the entire `database` key with the override's `database` dict, discarding `port` and `ssl`. Implement recursive deep merge: `def deep_merge(base: dict, override: dict) -> dict: result = base.copy(); [result.update({k: deep_merge(base[k], v)}) if isinstance(base.get(k), dict) and isinstance(v, dict) else result.update({k: v}) for k, v in override.items()]; return result`. This recursively merges nested dicts while primitive values follow override-wins semantics. The test case passes: `database.port=5432` and `database.ssl=true` are preserved from base; `database.host` is overridden.",
  "rejectedOutput": "The implementation is correct. dict.update() handles the merge and the test expectation is wrong — updating a nested dict key should replace the entire sub-dict.",
  "arbiters": ["deep-merge-config-semantics", "shallow-vs-deep-update"],
  "whyAccepted": "Deep merge is the explicit requirement ('deep-merges two JSON config files'). The test case proves shallow update is incorrect: it silently discards `port` and `ssl` when `host` is overridden.",
  "whyRejected": "Claiming the test expectation is wrong contradicts the stated requirement. Config file merging semantics universally expect partial override — replacing an entire sub-object is a data loss bug.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-333

```json
{
  "rowId": "EXT-333",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A Django application returns a list of User objects in an API response. The view queries `User.objects.all()` and serializes with DRF (Django REST Framework). A load test reveals N+1 queries: for each user, a separate query fetches `user.profile` (OneToOne relation).",
    "b": "Three agent fixes: (A) `User.objects.select_related('profile').all()`. (B) `User.objects.prefetch_related('profile').all()`. (C) Cache the profile queryset in a class variable.",
    "c": "User.profile is a OneToOne relation (not a reverse FK or M2M)."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Select Agent-A: `select_related('profile')`. For OneToOne and ForeignKey relations (single object, not a collection), `select_related` performs a SQL JOIN in a single query — e.g., `SELECT users.*, profiles.* FROM users LEFT JOIN profiles ON profiles.user_id = users.id`. This eliminates the N+1 by fetching all users and their profiles in one round trip. `prefetch_related` (option B) is correct for reverse FK and M2M relations — it performs two queries with Python-side joining. For a OneToOne forward relation, prefetch_related works but is less efficient than select_related (2 queries vs 1). Option C (class-variable cache) is a global mutable state anti-pattern that breaks request isolation and returns stale data.",
  "rejectedOutput": "Use prefetch_related('profile'). It is more flexible than select_related and works for all relation types, so it is safer to use consistently.",
  "arbiters": ["django-select-related-vs-prefetch", "n-plus-one-elimination"],
  "whyAccepted": "select_related for OneToOne/FK is always more efficient than prefetch_related: 1 query with JOIN vs 2 queries. The 'use prefetch for everything' heuristic sacrifices performance for consistency.",
  "whyRejected": "prefetch_related for a OneToOne relation fires 2 queries (users, then profiles filtered by user_id list) and performs Python-side matching. select_related fires 1 JOIN query — strictly better.",
  "sourceAnchors": [
    "https://docs.djangoproject.com/en/stable/ref/models/querysets/#select-related",
    "https://www.swebench.com"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-334

```json
{
  "rowId": "EXT-334",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-LearningCycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.10 codebase uses `functools.reduce()` for building cumulative sums, products, and string joining across 23 call sites. A code reviewer suggests replacing all `functools.reduce()` calls with explicit for-loops, citing 'reduce is considered un-Pythonic by Guido van Rossum'.",
    "b": "PEP-3000 (Python 3 manifesto) moved reduce() from builtins to functools, indicating reduced prominence. However, functools.reduce() remains in Python 3.12 and has no deprecation notice.",
    "c": "Of the 23 call sites: 8 are `reduce(operator.add, nums)` (cumulative sum), 6 are `reduce(operator.mul, nums)` (product), 4 are `reduce(str.__add__, strings)` (string join), 5 are genuinely complex folds."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "SELECTIVE replacement — not a blanket migration. Replace by category: (1) 8 cumulative sums: replace with `sum(nums)` — this is idiomatic, faster (C implementation), and clearly communicates intent. (2) 6 products: replace with `math.prod(nums)` (Python 3.8+) — same reasoning. (3) 4 string joins: replace with `''.join(strings)` — `str.__add__` via reduce is O(n²) due to string immutability; join is O(n). (4) 5 genuinely complex folds: KEEP reduce() — for arbitrary accumulator functions without a built-in equivalent, reduce() is the correct tool and for-loops are more verbose without clarity benefit. Total: 18 replacements with clear equivalents, 5 kept. Do NOT treat this as a 'reduce is banned' migration.",
  "rejectedOutput": "Replace all 23 reduce() calls with explicit for-loops. This follows the Python philosophy that explicit is better than implicit and removes a function Guido himself called 'not Pythonic'.",
  "arbiters": ["python-reduce-stdlib", "pep-20-explicit-vs-implicit"],
  "whyAccepted": "Built-in replacements (sum, math.prod, str.join) are faster, clearer, and idiomatic. But for genuine folds without built-in equivalents, reduce() is correct — for-loops are not inherently more Pythonic.",
  "whyRejected": "Blanket 'no reduce' is not Python policy — functools.reduce() is not deprecated. For-loop replacements of genuine folds add lines without clarity. String join via str.__add__ reduce is actively O(n²) vs O(n).",
  "sourceAnchors": [
    "https://docs.python.org/3/library/functools.html#functools.reduce",
    "https://docs.python.org/3/library/math.html#math.prod"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-335

```json
{
  "rowId": "EXT-335",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team wants to add distributed tracing to a .NET 8 microservice. Options: (A) Use OpenTelemetry .NET SDK with OTLP (OpenTelemetry Line Protocol) exporter to an OTel Collector. (B) Use Application Insights SDK directly. (C) Use Jaeger client library directly.",
    "b": "The observability backend may change (currently Jaeger, possibly Grafana Tempo next quarter). The team uses Azure but is not Azure-locked.",
    "c": "Other services in the ecosystem already use OpenTelemetry."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option A: OpenTelemetry .NET SDK with OTLP exporter to an OTel Collector. The OTel Collector acts as an observability routing layer — the application exports via OTLP, and the Collector routes to Jaeger today and Grafana Tempo next quarter by changing a single Collector config line (no application code change). Application Insights SDK (option B) ties the application to Azure's proprietary ingestion format, requiring code changes to switch backends. Jaeger client (option C) ties to Jaeger's wire format — the explicit concern about a Grafana Tempo switch makes this a direct contraindication. The other services already use OTel, making option A also the consistency choice. The .NET 8 OTel SDK integrates with ASP.NET Core's built-in Activity API, adding minimal overhead.",
  "rejectedOutput": "Use Application Insights SDK. The team uses Azure and Application Insights provides the best Azure integration with Live Metrics, Application Map, and built-in anomaly detection.",
  "arbiters": ["otel-vendor-neutrality", "otlp-exporter-backend-agnostic"],
  "whyAccepted": "OTel + OTLP Collector provides backend portability: switch from Jaeger to Tempo by changing Collector config only. Ecosystem consistency with other OTel services is an additional benefit.",
  "whyRejected": "Application Insights SDK locks to Azure ingestion format. The team explicitly needs to support a possible Grafana Tempo migration next quarter — changing SDK when switching backends is exactly the cost OTel avoids.",
  "sourceAnchors": [
    "https://opentelemetry.io/docs/languages/dotnet/",
    "https://opentelemetry.io/docs/collector/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-336

```json
{
  "rowId": "EXT-336",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Dapper repository method performs a read and a conditional write in sequence: (1) SELECT existing record, (2) if record exists: UPDATE, if not: INSERT. This read-modify-write pattern runs 50 times/second.",
    "b": "Two implementations: (A) Two separate Dapper calls in a TransactionScope with Serializable isolation. (B) Single `MERGE INTO ... USING ... WHEN MATCHED THEN UPDATE WHEN NOT MATCHED THEN INSERT` (upsert) statement.",
    "c": "SQL Server 2019. The SELECT-then-INSERT race condition has caused duplicate rows in production."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option B: SQL Server MERGE (upsert) statement. The MERGE statement is atomic — SQL Server acquires the appropriate locks to prevent the TOCTOU (Time Of Check Time Of Use) race condition that caused duplicate rows. At 50/second with Serializable isolation (option A), TransactionScope creates lock contention — Serializable isolation holds range locks that can cause deadlocks under concurrent upsert load. The MERGE statement with a unique index on the key column is the SQL Server-idiomatic upsert pattern: single round trip, atomic, and the unique index provides the safety net if concurrent MERGEs race on the same key (one will fail with a unique constraint violation, which is safe to retry). The production duplicate row incident confirms option A's race condition is real.",
  "rejectedOutput": "Use TransactionScope with Serializable isolation (option A). Serializable ensures the SELECT and INSERT/UPDATE are atomic at the transaction level.",
  "arbiters": ["sql-server-merge-upsert", "toctou-race-condition"],
  "whyAccepted": "MERGE is atomically safe without application-level transaction management. The production duplicate incident proves Serializable TransactionScope did not prevent the race condition in practice.",
  "whyRejected": "Serializable TransactionScope did not prevent duplicates in production. At 50/second, Serializable range locks cause deadlocks. MERGE with a unique index is the correct SQL Server upsert pattern.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql",
    "https://github.com/DapperLib/Dapper"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-337

```json
{
  "rowId": "EXT-337",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-RetrainingTrigger",
  "rowClass": "migration-decision",
  "input": {
    "a": "An Angular 14 application uses `NgModule` with `CommonModule` imported in every feature module for `*ngIf` and `*ngFor`. Angular 17 introduced standalone components. Evaluate migrating to standalone components.",
    "b": "Standalone components import `NgIf`, `NgFor`, `AsyncPipe` directly instead of `CommonModule`. They do not require `NgModule` declaration.",
    "c": "Application has 67 components across 12 feature modules. The CLI provides a `ng generate --standalone` flag and a `ng generate @angular/core:standalone` schematic for migration."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "SCHEDULE migration for the next major refactor sprint using the Angular standalone migration schematic. The Angular team provides an automated migration tool: `ng generate @angular/core:standalone` converts NgModule-based components to standalone in batches, updating imports, removing module declarations, and replacing `CommonModule` with per-directive imports. This is not a manual migration for 67 components. Benefits: (1) Tree-shaking improvement — standalone components import only `NgIf`/`NgFor`/`AsyncPipe` needed, not all of `CommonModule`. (2) Simpler mental model — no NgModule ceremony. (3) Angular 17+ new features (deferred loading, server-side rendering improvements) work optimally with standalone. The migration schematic handles the mechanical transformation. Gate: all existing E2E (End-to-End) tests pass after migration.",
  "rejectedOutput": "Defer migration indefinitely. NgModule components continue to work in Angular 17 and the migration risk for 67 components is not justified by the benefits.",
  "arbiters": ["angular-standalone-schematic", "commonmodule-tree-shaking"],
  "whyAccepted": "Angular provides an automated migration schematic, reducing the 67-component migration to a CLI command. Tree-shaking and Angular 17+ feature compatibility are real benefits.",
  "whyRejected": "NgModule components work but FormsModule/CommonModule import overhead grows with application size. With automated tooling available, deferring indefinitely accumulates tech debt without valid justification.",
  "sourceAnchors": [
    "https://angular.dev/guide/standalone-components",
    "https://angular.dev/reference/migrations/standalone"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-338

```json
{
  "rowId": "EXT-338",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A React component renders a list of items with `key={Math.random()}` on each list item to ensure uniqueness.",
    "b": "Developer justification: 'Math.random() guarantees unique keys without needing to add IDs to the data model.'",
    "c": "The list re-renders when the user filters items. Components in the list have internal state (expanded/collapsed toggle)."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace Math.random() keys with stable IDs. `key={Math.random()}` generates a new random key on every render — React treats each render's keys as an entirely new set of elements, unmounting and remounting every list item on every re-render. This: (1) destroys internal component state (the expanded/collapsed toggle resets on every filter change), (2) causes unnecessary DOM mutations (full replace instead of reconciliation), (3) eliminates React's reconciliation benefit entirely. The correct fix depends on the data: if items have database IDs, use `key={item.id}`. If not, add a stable UUID to each item when the data is first received (`items.map(item => ({...item, _key: crypto.randomUUID()}))`). The key should be generated once per item, not once per render.",
  "rejectedOutput": "Math.random() keys work correctly because they guarantee uniqueness. React only needs keys to be unique within the list, not stable across renders.",
  "arbiters": ["react-reconciliation-stable-keys", "react-key-prop-semantics"],
  "whyAccepted": "React keys must be stable across renders for reconciliation to work. Random keys cause full unmount/remount on every render, destroying state and defeating the purpose of keys.",
  "whyRejected": "Uniqueness within a render is necessary but not sufficient. React uses key stability between renders to match old and new elements — random keys signal 'entirely new element every time'.",
  "sourceAnchors": [
    "https://react.dev/learn/rendering-lists#keeping-list-items-in-order-with-key",
    "https://react.dev/learn/preserving-and-resetting-state#same-component-at-same-position-preserves-state"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-339

```json
{
  "rowId": "EXT-339",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core 8 API uses JWT (JSON Web Token) Bearer authentication. Token expiry is 24 hours. A security audit finds that compromised tokens remain valid for up to 24 hours with no revocation mechanism.",
    "b": "Three proposals: (A) Reduce token lifetime to 15 minutes + refresh token rotation. (B) Add a token revocation list (Redis cache storing revoked token JTI claims). (C) Both: short-lived tokens + revocation list as defence-in-depth.",
    "c": "The API has 50,000 active users. Average session length: 4 hours. Mobile clients are included."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Implement option A (short-lived tokens + refresh token rotation) as the primary control, with option B (revocation list) only for high-privilege actions (admin endpoints, payment operations). Full revocation list (option B applied to all tokens) requires a Redis lookup on every API request — at 50,000 users this is a significant overhead for a cache miss path. Short-lived tokens (15 minutes) limit the compromise window to 15 minutes without per-request overhead. Refresh token rotation (RFC 6749 §10.4 + RFC 6819) detects token theft: if a refresh token is used twice (by both the legitimate client and an attacker), the second use triggers revocation of all tokens in the family. Mobile clients benefit from refresh tokens as they maintain long sessions without requiring re-login. The defence-in-depth argument for option C is valid for admin/payment endpoints where a 15-minute window is still too long.",
  "rejectedOutput": "Apply option B only: token revocation list. Short token lifetimes force mobile users to re-authenticate frequently, degrading UX. A revocation list provides immediate revocation on compromise detection.",
  "arbiters": ["rfc-6749-refresh-token-rotation", "jwt-revocation-tradeoffs"],
  "whyAccepted": "Refresh token rotation detects theft without per-request Redis overhead. Short-lived access tokens limit exposure window. High-privilege endpoints get additional revocation list protection.",
  "whyRejected": "Option B alone requires Redis on every request. For 50,000 users at 100 req/session-hour, that is millions of revocation-list checks daily. The refresh token rotation approach is both more scalable and detects theft proactively.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc6749#section-10.4",
    "https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-340

```json
{
  "rowId": "EXT-340",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS application needs to send transactional emails (order confirmation, password reset). Options: (A) Call SendGrid API directly from the OrderService and AuthService using HttpService. (B) Publish an EmailRequested event to a Bull queue; an EmailConsumer processes the queue and calls SendGrid.",
    "b": "Password reset emails must deliver within 30 seconds. Order confirmation can be delayed up to 5 minutes. SendGrid has 99.9% SLA but occasionally experiences 5-30 second delays.",
    "c": "Application runs on 3 instances. Bull queue backed by Redis."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option B (Bull queue) for all emails, with priority differentiation. Publish password reset emails with `priority: 1` (highest) and order confirmation with `priority: 10`. The Bull queue with 3 consumer workers processes high-priority jobs first. This decouples OrderService/AuthService from SendGrid's transient latency — the services publish the event and return; the queue handles retries on failure. Password reset emails with priority 1 process within seconds under normal load. The 30-second delivery requirement is met by queue processing time, not synchronous API call time. Option A (direct HttpService calls) ties the caller's response time to SendGrid's latency — a 30-second SendGrid delay makes AuthService's password reset endpoint wait 30 seconds. If SendGrid is down, the API call fails and the email is lost unless the caller implements retry logic (re-inventing the queue).",
  "rejectedOutput": "Use direct SendGrid API calls (option A) for password reset emails (latency-sensitive) and queue for order confirmations only. This guarantees password reset emails are sent immediately without queue processing delay.",
  "arbiters": ["bull-queue-priority", "transactional-email-decoupling"],
  "whyAccepted": "Queue with priority delivers password reset emails within seconds while providing retry durability and decoupling caller response time from SendGrid latency. Direct calls for latency-sensitive emails are false security — if SendGrid is slow, the caller is slow.",
  "whyRejected": "Direct API calls for password reset block the authentication endpoint during SendGrid delays. If SendGrid returns 500, the email is lost without retry. The queue's priority mechanism handles the latency requirement correctly.",
  "sourceAnchors": [
    "https://docs.nestjs.com/techniques/queues",
    "https://github.com/OptimalBits/bull/blob/master/REFERENCE.md#queue-add"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-341

```json
{
  "rowId": "EXT-341",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "An API returns `Cache-Control: max-age=3600` on GET /products. A product price update takes effect immediately in the database but clients see stale prices for up to 1 hour.",
    "b": "Two proposals: (A) Reduce max-age to 60 seconds. (B) Add `ETag` + `Last-Modified` response headers and support conditional GET with `If-None-Match` / `If-Modified-Since`.",
    "c": "Product prices change ~20 times per day across 5,000 products. 95% of requests hit the same 200 popular products. P99 price update propagation must be under 5 minutes."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option B (ETag + conditional GET) as the primary mechanism, with max-age reduced to 300 seconds (5 minutes). ETag-based validation: the server computes an ETag from the product's last_modified timestamp. A client with a cached response sends `If-None-Match: \"etag-value\"`. If the price hasn't changed, the server returns `304 Not Modified` with no body — saving bandwidth. If the price changed, it returns `200` with the new price. max-age=300 means: fresh responses are served from cache for 5 minutes (covering P99 propagation requirement), then clients revalidate with If-None-Match. Option A (max-age=60) causes 60x more revalidation requests than 3600s without conditional GET efficiency. Option B with max-age=300 satisfies the 5-minute propagation requirement while serving cache hits for unchanged products at zero server cost.",
  "rejectedOutput": "Reduce max-age to 60 seconds. This ensures stale prices are visible for at most 60 seconds, which is well within the 5-minute propagation requirement and requires no ETag implementation.",
  "arbiters": ["rfc-9110-etag-conditional-get", "cache-control-max-age"],
  "whyAccepted": "ETag conditional GET serves 304 for unchanged products (zero bandwidth, minimal server cost) while delivering price changes within 5 minutes. max-age=300 reduces revalidation frequency 5x vs 60s.",
  "whyRejected": "max-age=60 alone causes 60 revalidation requests per product per hour from all clients — for 200 popular products at high traffic this creates unnecessary load without the bandwidth savings of 304 responses.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc9110#section-8.8.3",
    "https://datatracker.ietf.org/doc/html/rfc9110#section-13.1"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-342

```json
{
  "rowId": "EXT-342",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B1-RoutingTopology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Design the API for listing Orders with support for filtering by status, date range, and customer ID; sorting by created_at and total_amount; and pagination.'",
    "b": "AIP-132 (List method), AIP-158 (Pagination), AIP-160 (Filtering).",
    "c": "Target: REST API, JSON response, server-side cursor pagination."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Action items per AIP standards: (1) Endpoint: `GET /orders` with query parameters. (2) Filtering (AIP-160): `filter=status=\"SHIPPED\" AND created_at>\"2024-01-01\" AND customer_id=\"cust-123\"` — a single `filter` string using the AIP-160 filter expression language. (3) Sorting (AIP-132): `order_by=created_at desc,total_amount asc` — comma-separated fields with direction. (4) Pagination (AIP-158): `page_size=50` (client-requested, server-enforced max), `page_token` (opaque cursor string from previous response's `next_page_token`). (5) Response body: `{orders: [...], next_page_token: \"...\", total_size: 1234}`. (6) Server enforces max page_size (e.g., 100); if client requests 200, return 100 and document the max. (7) `total_size` is optional and should be omitted if computing it is expensive (AIP-158 §total_size).",
  "rejectedOutput": "Use offset/limit pagination: `GET /orders?offset=100&limit=50`. Cursor pagination is harder to implement and offset/limit is more familiar to API consumers.",
  "arbiters": ["aip-158-cursor-pagination", "aip-160-filter-expression"],
  "whyAccepted": "Cursor pagination (AIP-158) is consistent across pages when data is inserted/deleted during iteration — offset pagination skips or duplicates rows when the underlying set changes during traversal.",
  "whyRejected": "Offset/limit pagination is incorrect for live data: inserting an order between page 1 and page 2 fetches causes the first item of page 2 to be skipped. Cursor pagination uses a stable position marker.",
  "sourceAnchors": [
    "https://google.aip.dev/158",
    "https://google.aip.dev/160",
    "https://google.aip.dev/132"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-343

```json
{
  "rowId": "EXT-343",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C2-ResultIntegration",
  "rowClass": "negative-example",
  "input": {
    "a": "A model fine-tuning job reports: 'Training loss decreased from 2.4 to 0.08 over 10 epochs. Validation loss: 2.1.' The final model is evaluated on the held-out test set and achieves 51% accuracy (baseline: 50%).",
    "b": "Three operator interpretations: (I1) The model trained successfully — low training loss confirms convergence. (I2) The model is severely overfit — training loss 0.08 vs validation loss 2.1 indicates the model memorised the training set. (I3) The validation loss plateau at 2.1 indicates the learning rate is too high.",
    "c": "Training set: 50,000 examples. Validation set: 5,000 examples. Test accuracy 51% vs baseline 50%."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Select I2: severe overfitting. The gap between training loss (0.08) and validation loss (2.1) is a 26x difference — a canonical overfitting signature. Test accuracy of 51% (vs 50% baseline) confirms the model learned nothing generalisable; it memorised training examples. Remediation options in priority order: (1) Add regularization: dropout, weight decay (L2). (2) Reduce model capacity: fewer layers or hidden units. (3) Data augmentation to increase effective training set diversity. (4) Early stopping: monitor validation loss and stop when it plateaus/increases — the model was at its best performance around epoch 2-3, not epoch 10. I3 (learning rate too high) would cause training loss to oscillate, not decrease to 0.08. I1 ignores the fundamental purpose of the validation set.",
  "rejectedOutput": "Interpret as I3: validation loss plateau indicates the learning rate is too high and preventing the model from converging on the validation distribution. Reduce the learning rate.",
  "arbiters": ["overfitting-diagnosis", "train-val-loss-gap"],
  "whyAccepted": "26x train-to-validation loss gap is definitive overfitting evidence. Test accuracy at baseline confirms no generalisation. High learning rate (I3) causes oscillating loss, not monotonic decrease to 0.08.",
  "whyRejected": "High learning rate causes noisy/oscillating training loss. A smooth decrease from 2.4 to 0.08 over 10 epochs is consistent with successful optimisation on the training set — the problem is what the model is optimising (memorisation, not generalisation).",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-344

```json
{
  "rowId": "EXT-344",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A multilingual corpus pipeline downloads Tatoeba sentence pairs for 12 language pairs and stores them in a flat directory: `tatoeba_en_fr.tsv`, `tatoeba_en_es.tsv`, etc. A script identifies 'low-quality' sentences by checking if `sentence.split().count > 50` (longer than 50 words) and deletes those rows.",
    "b": "The quality filter removes sentences over 50 words, assuming they are 'too long to be useful.' This removes 8% of the dataset.",
    "c": "The downstream task is machine translation. Test BLEU is measured on a held-out set that includes some long sentences."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Remove the 50-word length filter. The filter is not calibrated to a quality signal — sentence length does not correlate with translation quality. Complex sentences (legal text, technical documentation) are legitimately long and represent important translation patterns. Deleting 8% of the dataset without a quality justification harms the model's ability to handle long sentences — BLEU scores on long-sentence test examples will suffer. If quality filtering is needed, use a principled metric: language identification confidence (langdetect score), character-level entropy (low entropy = repetitive/garbled), or LASER (Language-Agnostic SEntence Representations) similarity score between source and target (detecting misaligned pairs). Replace the word-count heuristic with one of these evidence-based filters.",
  "rejectedOutput": "Keep the 50-word filter. Long sentences are harder to translate and removing them makes the training set cleaner and easier for the model to learn from.",
  "arbiters": ["data-quality-calibration", "bleu-long-sentence-coverage"],
  "whyAccepted": "Length is not a quality signal for translation data. Removing 8% of data without evidence of quality improvement is harmful. BLEU on long sentences in the test set will directly reveal the damage.",
  "whyRejected": "'Harder to translate' is a model capacity problem, not a data quality problem. Filtering hard examples makes the training set easier but reduces the model's capability on real-world long text.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://github.com/facebookresearch/LASER"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-345

```json
{
  "rowId": "EXT-345",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 application needs to share user authentication state (user object, isAuthenticated flag, login/logout actions) across all components. The application uses Vue Router with navigation guards that check authentication.",
    "b": "Options: (A) Pinia store for auth state. (B) provide/inject at App.vue root. (C) Composable `useAuth()` function using a module-level `ref` (singleton pattern).",
    "c": "Application uses SSR (Server-Side Rendering) with Nuxt 3. Multiple concurrent requests per server instance."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use Pinia store (option A). In an SSR (Server-Side Rendering) context with Nuxt 3, module-level singletons (option C) cause request state leakage between concurrent users — all requests on the server share the same module-level `ref`, meaning User A's auth state could leak to User B's request. Pinia stores are request-scoped in SSR: Nuxt 3 creates a new Pinia instance per request, ensuring isolation. `provide/inject` at App.vue root (option B) works for CSR (Client-Side Rendering) but is awkward with Router navigation guards (guards cannot inject from a component context). Pinia stores are accessible from both Vue components and Router guards via `useAuthStore()` — the same API in all contexts. SSR request isolation is the decisive factor.",
  "rejectedOutput": "Use a module-level composable (option C). The useAuth() composable with a module-level ref is simpler than Pinia and avoids the Pinia dependency.",
  "arbiters": ["pinia-ssr-request-scoping", "nuxt3-state-isolation"],
  "whyAccepted": "Pinia provides per-request store instances in Nuxt 3 SSR, preventing auth state leakage between concurrent users. Module-level refs are server-global singletons — a critical security vulnerability in SSR.",
  "whyRejected": "Module-level ref in SSR is not a per-request singleton — it is a server-process singleton. User A's login persists in the module ref and is visible to User B's concurrent request. This is a security vulnerability.",
  "sourceAnchors": [
    "https://pinia.vuejs.org/ssr/",
    "https://nuxt.com/docs/guide/features/state-management"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-346

```json
{
  "rowId": "EXT-346",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An EF Core 8 repository method: `public async Task<List<OrderDto>> GetRecentOrdersAsync(int customerId) { return await _context.Orders.Where(o => o.CustomerId == customerId).OrderByDescending(o => o.CreatedAt).Take(10).Select(o => new OrderDto { Id = o.Id, Total = o.Total, CreatedAt = o.CreatedAt }).ToListAsync(); }`. A junior developer proposes adding `.AsNoTracking()` to improve performance.",
    "b": "The method returns DTOs (Data Transfer Objects) via `.Select()` projection, not entity objects.",
    "c": "EF Core tracking behaviour: entities tracked by the change tracker have identity resolution and snapshot overhead. Projected DTOs via Select are not tracked regardless of AsNoTracking."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "The `.AsNoTracking()` addition is correct in intent but redundant in this specific case. When a LINQ query uses `.Select()` to project into a non-entity type (like `OrderDto`), EF Core does not track the projected objects — they are not entities in the DbContext model. `AsNoTracking()` affects entity tracking, not DTO projection. The method is already optimally non-tracking. However, if the developer intends to establish a team convention of always using AsNoTracking() on read queries (even though it is redundant here), it does not harm correctness. The correct guidance: add a code comment explaining why AsNoTracking is unnecessary for projected queries to educate the team, rather than either silently accepting or rejecting the change.",
  "rejectedOutput": "Accept the AsNoTracking() addition. It always improves performance on read queries and there is no downside to adding it.",
  "arbiters": ["efcore-tracking-vs-projection", "asnotracking-dto-semantics"],
  "whyAccepted": "AsNoTracking() is a no-op for Select() projections into non-entity types. Understanding when tracking applies prevents cargo-cult usage of AsNoTracking() on all queries.",
  "whyRejected": "Claiming AsNoTracking 'always improves performance' is incorrect for projected queries — it neither helps nor hurts because the projected DTO is never tracked. The explanation is more valuable than the change.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/querying/tracking",
    "https://github.com/dotnet/efcore"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-347

```json
{
  "rowId": "EXT-347",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET 8 microservice exposes a gRPC API internally and a REST API externally via an API gateway. The team debates whether to use Protobuf for internal service-to-service communication only or also as the internal data model (replacing C# classes with Protobuf-generated classes throughout the service).",
    "b": "Option A: Protobuf at the API boundary only — use Protobuf-generated request/response classes for gRPC endpoints, but use domain C# classes internally. Option B: Protobuf-generated classes as the domain model throughout the service.",
    "c": "The service has complex domain logic with behaviour methods on entities (DDD — Domain-Driven Design style)."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option A: Protobuf at the API boundary only. Protobuf-generated classes are data containers — they have no behaviour methods, no custom constructors, no encapsulation. Using them as domain model objects for DDD-style entities forces all business logic into service/handler classes instead of the domain objects, which is an anemic domain model anti-pattern. The correct architecture: Protobuf Request/Response classes at the gRPC boundary → mappers to/from domain C# classes → domain objects with behaviour → repositories. This separation also future-proofs the domain model against Protobuf schema evolution: changing a Protobuf field numbering or adding `oneof` does not ripple through domain logic if the boundary is explicit. The mapping layer is boilerplate but AutoMapper or hand-written record mappings are manageable.",
  "rejectedOutput": "Use Protobuf-generated classes as the domain model (option B). This eliminates the mapping layer between API boundary and domain, reducing code volume.",
  "arbiters": ["ddd-domain-model-behaviour", "protobuf-generated-anemic-model"],
  "whyAccepted": "Protobuf generated classes are anemic DTOs — no encapsulation, no behaviour, no invariants. DDD domain objects need behaviour. Boundary-only Protobuf + domain C# classes is the correct separation.",
  "whyRejected": "Eliminating the mapping layer trades a one-time boilerplate cost for permanent architectural coupling between wire format and domain model. Protobuf schema changes (renumbering, oneof) leak into domain logic.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/dotnet/architecture/microservices/",
    "https://protobuf.dev/programming-guides/proto3/"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-348

```json
{
  "rowId": "EXT-348",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-RetrainingTrigger",
  "rowClass": "migration-decision",
  "input": {
    "a": "A system uses a monolithic SQL Server database shared by 6 microservices. Each service has its own schema (Orders, Products, Inventory, Shipping, Payments, Customers) but they share a single database instance. The team wants to decompose to separate databases per service (Database-per-Service pattern).",
    "b": "Current cross-schema queries: Orders JOIN Customers (3 query sites), Orders JOIN Products (5 query sites), Inventory JOIN Products (8 query sites). These queries use JOINs across schema boundaries.",
    "c": "Target state: each service owns its own database; cross-service data access via API calls or events."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "PLAN migration as a 3-phase operation, not a big-bang switch: Phase 1 (schema isolation) — enforce schema boundaries in application code: remove all cross-schema JOIN queries and replace with service API calls. This is the hardest phase: 16 cross-schema JOIN sites must be replaced with async fetches or data denormalisation. Phase 2 (replication bridge) — while still on one DB, add change-data-capture (CDC) replication of denormalised fields: e.g., the Orders service stores a snapshot of Customer.name and Customer.address at order creation time rather than joining live. Phase 3 (database split) — once all cross-schema JOINs are eliminated and services are autonomous, split the physical database. The big-bang approach (split first, fix queries second) causes service outages. Each phase has a measurable gate: Phase 1 gate = zero cross-schema SQL JOINs in any service. Phase 2 gate = each service's data access passes tests with a separate DB instance. Phase 3 gate = all 6 services deployed against separate DBs with passing integration tests.",
  "rejectedOutput": "Split into separate databases immediately (big-bang). The 16 cross-schema JOIN sites can be identified and fixed as they fail in the new environment.",
  "arbiters": ["database-per-service-migration", "cdc-denormalisation-bridge"],
  "whyAccepted": "Phased migration with measurable gates prevents service outages. The 16 cross-schema JOIN sites must be fixed before the split — not after — because they will cause immediate failures in production.",
  "whyRejected": "Big-bang database split is guaranteed to cause production failures: all 16 JOIN sites break simultaneously. 'Fix as they fail' means fixing failures in a live production environment under pressure.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/azure/architecture/patterns/database-per-service",
    "https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/microservices"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-349

```json
{
  "rowId": "EXT-349",
  "sourceIds": ["ISTIO-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Istio service mesh routes traffic to a product-service with two versions (v1: stable, v2: canary). The team wants to send 10% of traffic to v2 for canary testing while routing 100% of internal test traffic to v2 regardless of the weight.",
    "b": "Istio resources: VirtualService for traffic routing, DestinationRule for subsets. Header-based routing can match `x-test-group: canary`.",
    "c": "Canary criteria: error rate < 1%, P99 latency < 200ms in v2."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Configure Istio VirtualService with header-match routing before weight-based routing. VirtualService routes in priority order (first match wins): (1) Match `headers: x-test-group: exact: canary` → route 100% to v2 subset. (2) Weight-based: 10% to v2, 90% to v1. DestinationRule defines subsets: `v1: labels: version: v1` and `v2: labels: version: v2`. The header match must appear BEFORE the weight-based route in the VirtualService spec — Istio evaluates routes in order. Internal test clients add `x-test-group: canary` header and receive v2 exclusively. Regular traffic is split 90/10. Canary graduation criteria are enforced externally (Prometheus metrics, Grafana alerts) against the v2 error rate and P99 latency.",
  "rejectedOutput": "Use two separate VirtualServices: one for header-based routing and one for weight-based routing. This keeps the routing rules independent and easier to manage.",
  "arbiters": ["istio-virtual-service-route-order", "canary-header-routing"],
  "whyAccepted": "Istio evaluates VirtualService routes in order — header match first, weight-based second. A single VirtualService with ordered rules is the correct pattern. Two VirtualServices for the same host create undefined behaviour (last-write-wins config merge).",
  "whyRejected": "Two VirtualServices for the same host are not independent — Istio merges them with unpredictable precedence. The correct approach is ordered rules within a single VirtualService.",
  "sourceAnchors": [
    "https://istio.io/latest/docs/concepts/traffic-management/#virtual-services",
    "https://istio.io/latest/docs/tasks/traffic-management/traffic-shifting/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-350

```json
{
  "rowId": "EXT-350",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Node.js API endpoint processes file uploads. The filename from the multipart form is used to construct the save path: `const savePath = path.join('/uploads', req.file.originalname)`. Files are saved at the constructed path.",
    "b": "CWE-22 Path Traversal. Evaluate the vulnerability: what inputs could exploit it and what is the correct fix.",
    "c": "Uploaded file: filename `../../etc/passwd`. Server constructs: `/uploads/../../etc/passwd` which resolves to `/etc/passwd`."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "This is a path traversal vulnerability (CWE-22). `path.join('/uploads', '../../etc/passwd')` resolves to `/etc/passwd` on Linux/macOS — `path.join` does not prevent directory traversal, it merely concatenates and normalises. An attacker can overwrite any file the Node.js process has write permission to, including `/etc/shadow`, application config files, or SSH authorized_keys. Fix in three layers: (1) Never use user-supplied filenames as filesystem paths — generate a UUID-based filename: `const saveName = `${crypto.randomUUID()}${path.extname(req.file.originalname)}`;`. (2) Validate the extension against an allowlist: `const allowed = ['.jpg', '.png', '.pdf']; if (!allowed.includes(ext)) reject`. (3) After constructing the path, verify it is within the upload directory: `const resolved = path.resolve('/uploads', saveName); if (!resolved.startsWith('/uploads/')) reject`. Layer 3 is a defence-in-depth check for any future code changes.",
  "rejectedOutput": "Use path.basename() to strip directory components: `const safeName = path.basename(req.file.originalname)`. This removes any `../` sequences from the filename.",
  "arbiters": ["cwe-22-path-traversal", "crypto-uuid-filename-safety"],
  "whyAccepted": "UUID-based filename generation eliminates user input from the filesystem path entirely — the safest approach. Extension allowlist prevents disguising executables. Path.resolve() check provides defence-in-depth.",
  "whyRejected": "path.basename() strips directory separators but the resulting filename could still be `passwd` or `.bashrc` — overwriting important files in the /uploads directory. UUID generation is the correct fix.",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/22.html",
    "https://nodejs.org/api/path.html#pathbasenamepath-suffix"
  ],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "APPROVED"
}
```
