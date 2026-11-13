# External Training Rows — Batch 12
## Row IDs: EXT-231 through EXT-250
## Date: 2026-05-04
## Focus: Service mesh, gRPC, EF Core query splitting, temporal tables, blue-green deployment, TLS migration, additional CWE, families 3/4/5/6/7/8

---

## EXT-231

```json
{
  "rowId": "EXT-231",
  "sourceIds": ["ISTIO-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "service-mesh-mtls",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our microservices communicate over HTTP. The security team requires all inter-service traffic to be encrypted in transit. Should we add TLS to each service's application code or use Istio service mesh with mutual TLS (mTLS)?",
    "b": "Current setup: 12 microservices, all in Kubernetes. Some services are written in Go, some in Python, some in Java. None currently have TLS configured. The security team requires encryption for all inter-service calls, not just external traffic.",
    "c": "Context: Istio service mesh with mTLS. Istio injects sidecar proxies (Envoy) into each pod. The sidecar handles TLS termination and initiation transparently — application code communicates via unencrypted localhost. Certificates are managed by Istio's control plane (istiod) and rotated automatically. Application-level TLS: each service manages its own certificates, TLS configuration, and rotation. In a multi-language environment (Go, Python, Java), each language requires its own TLS implementation and certificate management."
  },
  "function": "Produce the command: Istio mTLS or application-level TLS.",
  "acceptedOutput": {
    "command": "Deploy Istio service mesh with STRICT mTLS mode.",
    "rationale": "Three advantages over application-level TLS in a multi-language environment: (1) Zero application code changes — Istio sidecars handle encryption transparently. Go, Python, and Java services communicate as if on HTTP; the sidecar upgrades to mTLS. (2) Centralized certificate management — istiod issues and rotates certificates automatically. No per-service certificate distribution. (3) STRICT mode enforcement — PeerAuthentication policy with mtls.mode: STRICT rejects any non-mTLS traffic to the service, preventing accidental plaintext communication.",
    "implementation": "kubectl apply -f - <<EOF\napiVersion: security.istio.io/v1beta1\nkind: PeerAuthentication\nmetadata:\n  name: default\n  namespace: production\nspec:\n  mtls:\n    mode: STRICT\nEOF"
  },
  "rejectedOutput": {
    "command": "Add TLS to each service's application code. This gives each team control over their TLS configuration and does not add service mesh complexity."
  },
  "arbiters": ["serviceMeshArbiter"],
  "whyAccepted": "12 services in 3 languages means 3 different TLS implementation stacks, 12 certificate configurations, and 12 rotation schedules. Istio centralizes all of this. Application-level TLS is appropriate when fine-grained per-service TLS configuration is required; for baseline encryption of all inter-service traffic, a service mesh is the correct tool.",
  "whyRejected": "Application-level TLS in a 12-service, 3-language environment requires 12 separate implementations. Certificate rotation must be coordinated across all 12 services. A single misconfigured service breaks the encryption requirement. Istio eliminates the per-service implementation surface entirely.",
  "sourceAnchors": [
    {
      "sourceId": "ISTIO-DOCS",
      "url": "https://github.com/istio/istio",
      "section": "PeerAuthentication STRICT mTLS — sidecar proxy transparent TLS; istiod certificate management",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-232

```json
{
  "rowId": "EXT-232",
  "sourceIds": ["ENVOY-PROXY"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "envoy-outlier-detection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We use Envoy proxy for load balancing across 5 instances of our payment service. One instance is consistently returning 5xx (server error) responses due to a memory leak that has not been restarted yet. All 5 instances receive roughly equal traffic (20% each). Should we configure Envoy outlier detection to eject the bad instance?",
    "b": "Current Envoy cluster configuration: no outlier detection. All 5 instances receive traffic via round-robin. The bad instance fails 100% of requests routed to it. Effective error rate for clients: approximately 20% (1 in 5 requests hits the bad instance).",
    "c": "Context: Envoy outlier detection. Envoy monitors per-upstream-host success rates. When a host's success rate falls below a configurable threshold for a configurable interval, Envoy ejects the host from the load balancing pool for a configurable base ejection time. Ejected hosts are periodically checked — if they recover, they are re-admitted. Outlier detection does not replace fixing the underlying issue; it reduces client error rate while the fix is applied."
  },
  "function": "Arbitrate: configure Envoy outlier detection or route around the bad instance manually?",
  "acceptedOutput": {
    "verdict": "Configure Envoy outlier detection immediately.",
    "configuration": "outlier_detection:\n  consecutive_5xx: 5\n  interval: 10s\n  base_ejection_time: 30s\n  max_ejection_percent: 50\n  success_rate_minimum_hosts: 3\n  success_rate_request_volume: 100",
    "effect": "After 5 consecutive 5xx responses from the bad instance within a 10-second interval, Envoy ejects it for 30 seconds. Remaining 4 instances serve 25% each. Effective client error rate drops from 20% to ~0% (dependent on the 5 consecutive errors threshold). After 30 seconds, the bad instance is re-probed — if still failing, ejection time doubles (exponential backoff).",
    "maxEjectionPercent": "max_ejection_percent: 50 prevents Envoy from ejecting more than 50% of the cluster (2 of 5 instances). This prevents a misconfigured detector from taking down the entire cluster."
  },
  "rejectedOutput": {
    "verdict": "Do not configure outlier detection. The bad instance should be restarted immediately — fixing the memory leak is the correct action. Outlier detection is a band-aid that adds configuration complexity."
  },
  "arbiters": ["loadBalancingArbiter", "resiliencePatternArbiter"],
  "whyAccepted": "Outlier detection and fixing the memory leak are not mutually exclusive — both should happen. While the fix is being developed and deployed (minutes to hours), outlier detection immediately reduces client error rate from 20% to ~0%. The 'band-aid' framing is incorrect: outlier detection is a standard production resilience mechanism, not a substitute for fixing defects.",
  "whyRejected": "The rejected verdict asks clients to absorb a 20% error rate while the fix is prepared. For a payment service, 20% of payment requests failing is a revenue and trust impact. Outlier detection is the correct immediate action; the fix is the correct long-term action. They are not alternatives.",
  "sourceAnchors": [
    {
      "sourceId": "ENVOY-PROXY",
      "url": "https://github.com/envoyproxy/envoy",
      "section": "Outlier detection — consecutive_5xx, base_ejection_time, max_ejection_percent for bad-instance isolation",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-233

```json
{
  "rowId": "EXT-233",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "grpc-vs-rest-internal",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We have two internal microservices: OrderService and InventoryService. OrderService calls InventoryService approximately 500 times per second to check stock availability. Both are ASP.NET Core services in the same Kubernetes cluster. Should we use REST/HTTP or gRPC for this internal communication?",
    "b": "Current setup: OrderService makes HTTP GET /inventory/products/{id} calls to InventoryService. Each call returns a JSON response. The call frequency is 500/second. Average payload size: 200 bytes. Network latency: sub-1ms (same Kubernetes cluster).",
    "c": "Context: gRPC uses HTTP/2 and Protocol Buffers (Protobuf) binary serialization. REST uses HTTP/1.1 and JSON text serialization. At 500 calls/second, serialization overhead becomes significant. JSON: text parsing, larger payload (200 bytes). Protobuf: binary, typically 30–50% smaller payload, 5–10× faster serialization. HTTP/2 multiplexing allows multiple concurrent gRPC calls on a single connection. ASP.NET Core has first-class gRPC support via Grpc.AspNetCore."
  },
  "function": "Produce the command: gRPC or REST for the internal service call.",
  "acceptedOutput": {
    "command": "Migrate to gRPC for the OrderService→InventoryService internal call.",
    "rationale": "At 500 calls/second, three gRPC advantages are quantified: (1) Payload size — Protobuf for a 200-byte JSON payload typically produces 80–120 bytes. At 500/second: 100 KB/s vs 60 KB/s — relevant at scale. (2) Serialization speed — Protobuf binary is 5–10× faster than JSON parsing. At 500/second, JSON parsing takes ~0.5ms/call CPU time; Protobuf takes ~0.05ms. Aggregate: 250ms/second CPU savings on OrderService. (3) HTTP/2 connection multiplexing — 500 calls/second on HTTP/1.1 requires many connections; HTTP/2 multiplexes all on one connection.",
    "proto": "syntax = 'proto3';\nservice InventoryService {\n  rpc CheckStock (StockRequest) returns (StockResponse);\n}\nmessage StockRequest { string product_id = 1; }\nmessage StockResponse { int32 available_quantity = 1; bool in_stock = 2; }"
  },
  "rejectedOutput": {
    "command": "Keep REST/JSON. gRPC is more complex to set up and debug. REST is universally understood and tools like curl and Postman work out of the box. At 500 calls/second, the performance difference is negligible."
  },
  "arbiters": ["serviceProtocolArbiter"],
  "whyAccepted": "Both services are ASP.NET Core — gRPC setup is two NuGet packages and a .proto file. The 'universal tooling' argument for REST applies to external APIs; for internal service-to-service calls in a controlled environment, gRPC tooling (grpcurl, Postman gRPC) is adequate. At 500/second, the serialization difference is 250ms/second aggregate CPU savings — measurable and valuable.",
  "whyRejected": "The 'negligible performance difference' claim is incorrect at 500 calls/second. 5–10× serialization speed difference × 500 calls/second = significant CPU reduction. The complexity argument is overstated for ASP.NET Core gRPC — server and client setup is comparable to REST configuration.",
  "sourceAnchors": [
    {
      "sourceId": "ASPNETCORE-DOCS-SAMPLES",
      "url": "https://github.com/dotnet/AspNetCore.Docs.Samples",
      "section": "gRPC vs REST — HTTP/2 multiplexing, Protobuf serialization speed at 500 RPS internal traffic",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-234

```json
{
  "rowId": "EXT-234",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "rolling-statistics-computation",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: compute rolling statistics (mean, max, min) over a sliding window of size W for a stream of numeric values. The window slides forward one element at a time. Memory constraint: do not store more than W values simultaneously.",
    "b": "Function signature: rolling_stats(values: Iterable[float], window: int) -> Iterator[dict]. Each yielded dict has keys 'mean', 'max', 'min'. The first result is emitted when W values have been seen. Example: values=[1,2,3,4,5], window=3 → [{mean:2.0,max:3,min:1},{mean:3.0,max:4,min:2},{mean:4.0,max:5,min:3}]",
    "c": "Implementation approach: use collections.deque with maxlen=window. A deque with maxlen automatically discards the oldest element when a new one is appended beyond capacity. Mean requires summing the deque. Max/min require scanning the deque (O(W) per step). For efficient max/min, a monotonic deque can be used, but O(W) is acceptable for window sizes < 10,000."
  },
  "function": "Implement rolling_stats as a generator using a deque.",
  "acceptedOutput": {
    "implementation": "from collections import deque\n\ndef rolling_stats(values, window):\n    buf = deque(maxlen=window)\n    for val in values:\n        buf.append(val)\n        if len(buf) == window:\n            yield {\n                'mean': sum(buf) / window,\n                'max': max(buf),\n                'min': min(buf)\n            }",
    "behavior": "For values=[1,2,3,4,5], window=3: deque fills to [1,2,3] → yield {mean:2.0,max:3,min:1}. Append 4: deque=[2,3,4] → yield {mean:3.0,max:4,min:2}. Append 5: deque=[3,4,5] → yield {mean:4.0,max:5,min:3}.",
    "memoryBound": "deque(maxlen=window) stores exactly W elements. Memory: O(W) regardless of stream length."
  },
  "rejectedOutput": {
    "implementation": "def rolling_stats(values, window):\n    values = list(values)  # materialize entire stream\n    results = []\n    for i in range(len(values) - window + 1):\n        w = values[i:i+window]\n        results.append({'mean': sum(w)/window, 'max': max(w), 'min': min(w)})\n    return results"
  },
  "arbiters": ["streamingAlgorithmArbiter"],
  "whyAccepted": "The accepted implementation uses a deque with maxlen — memory is O(W) regardless of stream length. It is a generator — it yields results one at a time without storing all input or all output. For an unbounded stream of sensor readings, the deque approach runs indefinitely; the list approach fails with OOM (Out of Memory).",
  "whyRejected": "The rejected implementation materializes the entire input stream (list(values)) — O(N) memory for N input values. It stores all results — O(N-W) memory. It returns a list, not a generator. For a 10-million-element stream with window=100, the rejected approach allocates ~80 MB; the deque approach uses ~800 bytes.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: rolling_stats — deque maxlen O(W) memory generator vs list materialize O(N) memory",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-235

```json
{
  "rowId": "EXT-235",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "test-coverage-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for a pull request that adds a new payment processing module with 450 lines of production code.",
    "b": "Agent results: Agent 1 (Coverage Agent): WARN — the new module has 62% test coverage (threshold: 80%). Lines not covered: error handling branches and the refund calculation function. Agent 2 (Test Agent): PASS — all 47 new tests pass. Agent 3 (Static Analysis Agent): PASS — no static analysis issues.",
    "c": "Coverage policy: new code submitted via PR must have ≥80% line coverage. The threshold applies to new code only (not existing code in the PR). The payment processing module is in the critical payment path. Payment processing error handling branches are the most important paths to cover — they determine what happens when the payment gateway is down or returns unexpected responses."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "INSUFFICIENT_COVERAGE",
    "summary": "The new payment processing module has 62% test coverage against an 80% requirement. The uncovered paths include error handling branches and the refund calculation function — both are in the critical payment path. Tests pass and static analysis is clean, but the coverage gate is not met.",
    "requiredActions": [
      "Add tests for error handling branches: payment gateway timeout, invalid card response, network failure",
      "Add tests for the refund calculation function covering: full refund, partial refund, refund after partial capture",
      "Re-run coverage report — must reach ≥80% before merge"
    ],
    "blockingAgent": "Coverage Agent (62% < 80% threshold)",
    "passingAgents": ["Test Agent", "Static Analysis Agent"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS_WITH_ADVISORY",
    "summary": "Tests pass and static analysis is clean. Coverage is 62% (below 80% threshold) — recommend adding coverage in a follow-up PR."
  },
  "arbiters": ["testCoverageArbiter"],
  "whyAccepted": "The coverage gate is a hard policy threshold (80%). A payment processing module at 62% with uncovered error handling paths is a concrete risk: untested error branches in a payment system may silently fail or handle edge cases incorrectly in production. BLOCK is the correct result — the coverage threshold exists precisely for this case.",
  "whyRejected": "PASS_WITH_ADVISORY with a follow-up PR recommendation allows the module to reach production at 62% coverage. The follow-up PR rarely materializes under deadline pressure. For a critical payment processing module, 'add tests later' is an unacceptable risk deferral.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Test coverage gate — BLOCK for 62% below 80% threshold on critical payment module",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-236

```json
{
  "rowId": "EXT-236",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "otel-metric-naming",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review these custom metric names a developer added to an ASP.NET Core service: 'httpRequestCount', 'dbQueryTime', 'cacheHit'.",
    "b": "OpenTelemetry Semantic Conventions (OTel SemConv) define standard metric names for common signals. The developer created custom metrics instead of using the conventions.",
    "c": "OpenTelemetry HTTP Semantic Conventions: http.server.request.duration (histogram, seconds), http.server.active_requests (updown counter). Database conventions: db.client.operation.duration (histogram, seconds). Cache conventions: cache.hits (counter). Naming conventions: dot-notation namespacing (http.server.*), snake_case within segments, unit suffix (duration in seconds, size in bytes). Non-standard metric names prevent automatic dashboard generation by OTel-aware observability backends."
  },
  "function": "Arbitrate: are the custom metric names correct? What are the correct names?",
  "acceptedOutput": {
    "verdict": "INCORRECT. The metric names violate OpenTelemetry Semantic Conventions.",
    "corrections": [
      "'httpRequestCount' → http.server.request.duration (histogram) — request count is derived from histogram count; do not create a separate counter. Use: Meter.CreateHistogram<double>('http.server.request.duration', unit: 's')",
      "'dbQueryTime' → db.client.operation.duration (histogram, seconds) — 'Time' suffix is non-standard; 'duration' is the OTel convention. Use: Meter.CreateHistogram<double>('db.client.operation.duration', unit: 's')",
      "'cacheHit' → cache.hits (counter, plural) — use the OTel experimental semconv name. Pair with cache.misses for meaningful hit rate calculation."
    ],
    "benefit": "Standard OTel metric names allow Grafana dashboards, Prometheus recording rules, and alerting policies to be shared across services without per-service customization. Observability vendors pre-build dashboards for standard metric names."
  },
  "rejectedOutput": {
    "verdict": "The custom metric names are acceptable. Teams can use any metric names they choose — the names are internal implementation details."
  },
  "arbiters": ["observabilityConventionArbiter"],
  "whyAccepted": "OpenTelemetry Semantic Conventions exist specifically to enable cross-service and cross-vendor dashboard reuse. 'httpRequestCount' as a counter misses latency distribution (no histogram); 'dbQueryTime' loses the unit convention (seconds); 'cacheHit' as a singular gauge is not comparable across services. The corrections align with published conventions and enable tooling benefits.",
  "whyRejected": "Metric names are not internal implementation details when the service exports them to a shared observability backend. Non-standard names prevent shared dashboard adoption. 'Any name' is true at the registry level but incorrect from a convention compliance perspective.",
  "sourceAnchors": [
    {
      "sourceId": "OPENTELEMETRY-SPEC",
      "url": "https://github.com/open-telemetry/opentelemetry-specification",
      "section": "Semantic Conventions — http.server.request.duration, db.client.operation.duration, cache.hits",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-237

```json
{
  "rowId": "EXT-237",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "efcore-query-splitting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An EF Core query loads Blog entities with their Posts and Tags collections. The resulting SQL uses JOINs across three tables and produces a Cartesian product warning. Should we use query splitting?",
    "b": "Current query: var blogs = await _context.Blogs.Include(b => b.Posts).ThenInclude(p => p.Tags).ToListAsync(). EF Core logs: 'Compiling a query which loads related collections for more than one collection navigation... consider using AsSplitQuery().'",
    "c": "EF Core query splitting (AsSplitQuery): instead of one SQL query with multiple JOINs producing a Cartesian product, EF Core issues multiple SQL queries — one per collection navigation. For Blogs × Posts × Tags: a single JOIN query produces Rows(Blogs) × Rows(Posts) × Rows(Tags) rows. A split query produces three queries: one for Blogs, one for Posts WHERE BlogId IN (...), one for Tags WHERE PostId IN (...). Split queries use more round trips but eliminate Cartesian explosion."
  },
  "function": "Arbitrate: use AsSplitQuery or optimize the JOIN query?",
  "acceptedOutput": {
    "verdict": "Use AsSplitQuery for this specific query.",
    "reasoning": "A Blog with 20 Posts each with 10 Tags produces: single query = 20 × 10 = 200 rows returned per Blog. For 50 Blogs: 10,000 rows. Split query: 50 rows (Blogs) + 1,000 rows (Posts) + 10,000 rows (Tags) = 11,050 rows across 3 queries — same data, no row duplication. The Cartesian product in the single query returns 50 × 20 × 10 = 10,000 rows where each Blog record is repeated 200 times. EF Core deduplicates in memory, but the network transfer and parsing of 10,000 rows vs 11,050 non-duplicated rows is comparable — at higher Blog/Post/Tag counts, the single query grows multiplicatively while the split query grows additively.",
    "configuration": "var blogs = await _context.Blogs.Include(b => b.Posts).ThenInclude(p => p.Tags).AsSplitQuery().ToListAsync();",
    "tradeoff": "Split query uses 3 database round trips. If the database is remote and latency is 50ms, 3 round trips = 150ms base vs 1 round trip = 50ms base. On a low-latency local database (< 1ms), round trip cost is negligible and AsSplitQuery is preferred."
  },
  "rejectedOutput": {
    "verdict": "Avoid AsSplitQuery. It uses multiple database round trips. Optimize the JOIN query with appropriate indexes instead."
  },
  "arbiters": ["efCoreQueryArbiter"],
  "whyAccepted": "EF Core itself recommends AsSplitQuery for multiple collection navigation includes (the Cartesian product warning is a strong signal). Index optimization cannot eliminate the fundamental Cartesian product row multiplication — it only speeds up the row retrieval. The row count explosion is a data transfer problem, not an index problem.",
  "whyRejected": "Indexes reduce scan time but do not reduce row count. A query producing 10,000 rows due to Cartesian product will still return 10,000 rows even with perfect indexes. The round-trip cost of AsSplitQuery is relevant only at high latency — in the same Kubernetes cluster, it is sub-millisecond.",
  "sourceAnchors": [
    {
      "sourceId": "EFCORE-SOURCE",
      "url": "https://github.com/dotnet/efcore",
      "section": "AsSplitQuery — multiple collection navigation Cartesian product; split vs single query row count comparison",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-238

```json
{
  "rowId": "EXT-238",
  "sourceIds": ["WIDEWORLDIMPORTERS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "temporal-table-audit-query",
  "rowClass": "migration-decision",
  "input": {
    "a": "A SQL Server database stores customer pricing data. The compliance team requires: what was the price for product X for customer Y on date D? The current schema has no historical pricing data — only the current price. Should we implement a temporal table or a manual audit log table?",
    "b": "Current schema: CustomerPricing (CustomerId, ProductId, Price, LastModified). The LastModified column tracks when the price was last changed but does not preserve the previous value. There are approximately 50,000 price updates per day.",
    "c": "SQL Server Temporal Tables (System-Versioned Temporal Tables): SQL Server automatically maintains a history table (CustomerPricingHistory) with ValidFrom and ValidTo columns. Every UPDATE or DELETE on CustomerPricing automatically inserts the previous row version into the history table. Point-in-time query syntax: SELECT Price FROM CustomerPricing FOR SYSTEM_TIME AS OF '2026-01-15' WHERE CustomerId = 123 AND ProductId = 456. Manual audit log: requires application code to INSERT into CustomerPricingHistory before every UPDATE, in a transaction."
  },
  "function": "Decide: SQL Server temporal table or manual audit log table.",
  "acceptedOutput": {
    "decision": "MIGRATE to SQL Server System-Versioned Temporal Table.",
    "rationale": "Three advantages over manual audit log: (1) Automatic history capture — SQL Server captures every row version change without application code changes. Manual audit log requires every UPDATE path (ORM, stored procedures, direct SQL, bulk operations) to also INSERT into the history table. Missing one path produces audit gaps. (2) Point-in-time query syntax — FOR SYSTEM_TIME AS OF is a first-class SQL construct. Manual audit log requires: SELECT * FROM CustomerPricingHistory WHERE CustomerId=123 AND ProductId=456 AND ValidFrom <= '2026-01-15' AND ValidTo > '2026-01-15'. (3) 50,000 updates/day = 50,000 history rows/day automatically maintained by the engine.",
    "migrationSQL": "ALTER TABLE CustomerPricing ADD ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START HIDDEN DEFAULT '2000-01-01', ValidTo DATETIME2 GENERATED ALWAYS AS ROW END HIDDEN DEFAULT '9999-12-31'; ALTER TABLE CustomerPricing ADD PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo); ALTER TABLE CustomerPricing SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.CustomerPricingHistory));",
    "gate": "Query FOR SYSTEM_TIME AS OF '2026-01-01' returns the correct price for a known historical update. Zero application code changes required."
  },
  "rejectedOutput": {
    "decision": "Create a manual audit log table. Temporal tables are SQL Server-specific. A manual log table is database-agnostic and gives explicit control over what is logged."
  },
  "arbiters": ["auditSchemaArbiter"],
  "whyAccepted": "The database is already SQL Server — database portability is not a constraint. Temporal tables require zero application code changes and provide 100% capture coverage. Manual audit logs require changes to every write path and are vulnerable to gaps when new write paths are added.",
  "whyRejected": "Database portability is a false concern for a system explicitly on SQL Server with no migration plans. 'Explicit control' over what is logged is a disadvantage for compliance — compliance requires complete capture, not selective capture. Temporal tables provide complete capture automatically.",
  "sourceAnchors": [
    {
      "sourceId": "WIDEWORLDIMPORTERS",
      "url": "https://github.com/microsoft/sql-server-samples",
      "section": "Temporal tables — FOR SYSTEM_TIME AS OF point-in-time query; automatic history vs manual audit log",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-239

```json
{
  "rowId": "EXT-239",
  "sourceIds": ["GOOD-DOCS-TEMPLATES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "doc-template-selection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A technical writer submitted a document about the /payments/charge endpoint. It includes: what parameters the endpoint accepts, what it returns, possible error codes, and example request/response payloads. Two classification agents disagreed: one classified it as a tutorial, one as API reference.",
    "b": "Agent 1 (Documentation Type Agent): TUTORIAL — it contains examples and explanations. Agent 2 (API Reference Agent): API_REFERENCE — it describes an endpoint's interface contract.",
    "c": "Good Docs Project template definitions: API Reference — describes the technical interface: parameters, return values, error codes, types. Learning-oriented: no. Task-oriented: no. Contract-oriented: yes. Tutorial — guides the reader through a learning experience with a narrative. Includes context about why steps are taken. How-to Guide — task-oriented step-by-step instructions. Explanation — conceptual background. The distinguishing factor: API reference describes WHAT the interface is. Tutorial describes HOW to learn. The presence of examples in a document does not make it a tutorial — API reference templates include example sections."
  },
  "function": "Arbitrate: tutorial or API reference?",
  "acceptedOutput": {
    "verdict": "API REFERENCE.",
    "reasoning": "The document describes the interface contract of the /payments/charge endpoint: parameters (what to send), return values (what to expect), error codes (what can go wrong), and examples (what the exchange looks like). This is the canonical content of an API reference document. Examples within an API reference are illustrative of the contract — they show what a valid request looks like. A tutorial would explain why you charge a payment, what prerequisites are needed, and guide the reader through a use case narrative (e.g., 'Build a checkout flow'). The document contains none of these tutorial elements.",
    "agentError": "Agent 1's classification error: confusing 'contains examples' with 'is a tutorial.' API reference templates explicitly include example request/response sections. The presence of examples is not a discriminating feature between tutorial and API reference."
  },
  "rejectedOutput": {
    "verdict": "TUTORIAL — the examples and explanations make it learning-oriented."
  },
  "arbiters": ["documentationClassificationArbiter"],
  "whyAccepted": "The API reference classification is correct. The document's purpose is to describe the endpoint's contract (parameters, returns, errors) — this is reference material. A tutorial's purpose is to guide learning through a narrative. Examples within API reference are part of the contract illustration, not tutorial narrative.",
  "whyRejected": "The rejected verdict conflates 'contains examples' with 'is a tutorial.' Every well-written API reference includes example payloads. If examples made a document a tutorial, all API references would be tutorials — which contradicts the definition of tutorial (learning-oriented, narrative, explains why).",
  "sourceAnchors": [
    {
      "sourceId": "GOOD-DOCS-TEMPLATES",
      "url": "https://github.com/thegooddocsproject/templates",
      "section": "API Reference template — parameters, returns, error codes, examples vs tutorial narrative structure",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-240

```json
{
  "rowId": "EXT-240",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A_TOPOLOGY",
  "subdomain": "deployment-strategy-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We're deploying a new version of our payment service to production. The new version includes a significant refactor of the payment processing logic. We need zero downtime and the ability to roll back within 5 minutes if the new version shows elevated error rates. Should we use blue-green deployment or canary deployment?",
    "b": "Current deployment setup: Kubernetes with 10 replicas of the payment service. We have one production namespace. Average traffic: 1,000 transactions/minute. The new version has been load tested at the expected traffic level.",
    "c": "Blue-green deployment: two identical environments (blue=current, green=new). Traffic switches instantaneously via load balancer (e.g., Kubernetes Service selector update). Rollback: switch selector back to blue. Canary deployment: gradually route a percentage of traffic to the new version (e.g., 5% → 25% → 100%). Rollback: route 0% to new version. Blue-green requires 2× infrastructure during transition. Canary exposes a fraction of users to the new version — if the new version has a bug, only canary users are affected."
  },
  "function": "Produce the command: blue-green or canary deployment.",
  "acceptedOutput": {
    "command": "Use blue-green deployment for this release.",
    "rationale": "The 5-minute rollback requirement and the significant refactor combination favor blue-green: (1) Blue-green rollback is instantaneous — a single Kubernetes Service selector update switches all traffic back to the current version in under 5 seconds. Canary rollback requires reducing the canary percentage to 0%, which is also fast but requires traffic percentage adjustment. (2) The significant refactor means the new version may have systemic issues — canary's gradual exposure risks exposing 5% of payment transactions to a broken refactor. For payment processing, even 5% error rate = 50 failed transactions/minute. (3) Load testing at the full traffic level has already validated the new version — canary's gradual traffic increase is the validation mechanism, which load testing has already provided.",
    "implementation": "Deploy green (new version) to a separate Deployment. Verify green health in production namespace with a subset of test requests. Switch Kubernetes Service selector from blue to green. Monitor for 5 minutes. If issues: kubectl patch service payment-service -p '{\"spec\":{\"selector\":{\"version\":\"blue\"}}}' — instant rollback."
  },
  "rejectedOutput": {
    "command": "Use canary deployment. Gradually increase traffic from 5% to 100%. This reduces blast radius if the new version has issues."
  },
  "arbiters": ["deploymentStrategyArbiter"],
  "whyAccepted": "The load testing pre-validates the new version at full traffic — the canary's progressive traffic increase is the validation mechanism, which is redundant. Blue-green instantaneous rollback is faster than adjusting canary weights. For a payment service, minimizing the duration of exposure to a bad version is more important than limiting the initial exposure percentage.",
  "whyRejected": "Canary's 'reduced blast radius' is valuable when the new version has not been tested at full traffic. Here, load testing at expected traffic has already been performed. The canary would expose 50 payment transactions/minute to potential failure during the gradual ramp — blue-green exposes all traffic but with a 5-second rollback path.",
  "sourceAnchors": [
    {
      "sourceId": "MICROSOFT-LEARN-ARCH",
      "url": "https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/app-service-web-app/blue-green-deployment",
      "section": "Blue-green vs canary — instantaneous rollback vs gradual exposure; post-load-test deployment decision",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-241

```json
{
  "rowId": "EXT-241",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C_SEMANTIC_TRANSLATION",
  "subdomain": "pod-crash-user-communication",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translate this machine result into a user-facing message: { status: 'POD_CRASH_LOOP', serviceName: 'user-auth', restartCount: 7, lastExitCode: 137, lastRestartTimestamp: '2026-05-04T16:30:00Z', podName: 'user-auth-7b9c4d5f6-xk8mn' }",
    "b": "Target audience: junior developer who owns the user-auth service but is not deeply familiar with Kubernetes internals.",
    "c": "Exit code 137: OOM (Out of Memory) kill — the container was killed by the Kubernetes OOM (Out of Memory) killer because it exceeded its memory limit. Kubernetes CrashLoopBackOff: the pod is crashing, Kubernetes restarts it, it crashes again — restart count increases with exponential backoff delay. Exit code 137 = 128 + 9, where signal 9 = SIGKILL (Linux signal for forceful termination). This is distinct from exit code 1 (application error) and exit code 0 (clean exit)."
  },
  "function": "Produce the final user-facing message for the pod crash result.",
  "acceptedOutput": {
    "userMessage": "Service user-auth is crashing repeatedly (7 restarts)",
    "detail": "The service was killed by Kubernetes because it ran out of memory (exit code 137 = memory limit exceeded). Kubernetes has restarted it 7 times but it keeps crashing.",
    "likelyCauses": [
      "The service is consuming more memory than its configured limit",
      "A memory leak is causing memory usage to grow until it hits the limit",
      "The memory limit is set too low for the current workload"
    ],
    "nextSteps": [
      "Check current memory usage before the next crash: kubectl top pod user-auth-7b9c4d5f6-xk8mn",
      "Check the configured memory limit: kubectl describe pod user-auth-7b9c4d5f6-xk8mn | grep -A5 Limits",
      "View application logs before the crash: kubectl logs user-auth-7b9c4d5f6-xk8mn --previous",
      "If the limit is too low: increase it in the deployment spec (resources.limits.memory). If there is a memory leak: profile the application with a memory profiler."
    ],
    "urgency": "HIGH — the service is unavailable during crash/restart cycles. Authentication may be failing for users."
  },
  "rejectedOutput": {
    "userMessage": "Pod user-auth-7b9c4d5f6-xk8mn is in CrashLoopBackOff. Exit code 137, 7 restarts. Check logs."
  },
  "arbiters": ["userCommunicationArbiter"],
  "whyAccepted": "The accepted message translates exit code 137 to 'memory limit exceeded', explains CrashLoopBackOff in plain language, gives actionable kubectl commands, and flags the user-facing impact (authentication may be failing). A junior developer can act on this without reading Kubernetes documentation.",
  "whyRejected": "The rejected message exposes raw Kubernetes terminology (CrashLoopBackOff, pod name format) without explanation. 'Check logs' is not actionable — which logs, with which command, for what to look for? Exit code 137 is meaningful to a Kubernetes expert, not a junior developer.",
  "sourceAnchors": [
    {
      "sourceId": "OPENASSISTANT-OASST1",
      "url": "https://huggingface.co/datasets/OpenAssistant/oasst1",
      "section": "Kubernetes pod crash communication — exit code 137 OOM kill to junior developer actionable message",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-242

```json
{
  "rowId": "EXT-242",
  "sourceIds": ["COMMON-VOICE"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "F_REGISTRY_STORAGE",
  "subdomain": "speech-corpus-quality-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three quality review agents for a batch of 500 Common Voice speech recordings submitted for the en-US training corpus.",
    "b": "Agent results: Agent 1 (Audio Quality Agent): 23 recordings rejected — SNR (Signal-to-Noise Ratio) below 20dB. 477 accepted. Agent 2 (Transcript Alignment Agent): 8 recordings rejected — audio duration mismatch with transcript (>15% deviation). 469 of 477 accepted. Agent 3 (License Verification Agent): PASS — all 500 recordings are CC-0 licensed per contributor agreement.",
    "c": "Common Voice corpus policy: a recording must pass all three quality gates to be included in the training corpus. A recording that fails any gate is excluded from the batch. License gate failure would exclude the entire batch (not individual recordings). The quality gates apply per-recording. Net accepted: 477 (audio quality) ∩ 469 (alignment) = 461 (assuming the 8 alignment failures are a subset of the 477 audio-quality passes, which is the expected case)."
  },
  "function": "Produce the aggregated result for this corpus batch.",
  "acceptedOutput": {
    "aggregatedResult": "PARTIAL_SUCCESS",
    "resultCode": "CORPUS_BATCH_PARTIALLY_ACCEPTED",
    "summary": "461 of 500 recordings accepted for the en-US training corpus. 39 recordings excluded: 23 failed audio quality (SNR < 20dB) and 8 failed transcript alignment (duration mismatch > 15%), with 8 additional recordings failing both gates (estimated). License verification passed for all 500 recordings.",
    "acceptedCount": 461,
    "rejectedCount": 39,
    "rejectionBreakdown": {
      "audioQualityFailures": 23,
      "alignmentFailures": 8,
      "bothGatesFailures": "estimated 8 (overlap)"
    },
    "action": "Include 461 accepted recordings in the training corpus. The 39 rejected recordings are not included. Provide rejection reasons to contributors for re-submission."
  },
  "rejectedOutput": {
    "aggregatedResult": "FAIL",
    "summary": "39 of 500 recordings failed quality gates. Entire batch rejected — resubmit after fixing all 39 recordings."
  },
  "arbiters": ["corpusQualityArbiter"],
  "whyAccepted": "Per-recording quality gates allow the 461 passing recordings to be included independently of the 39 failures. The license gate is the only all-or-nothing gate (a license failure would contaminate the entire batch). Individual recording quality failures are not batch-level failures. PARTIAL_SUCCESS with 461 accepted is the correct aggregation.",
  "whyRejected": "Rejecting the entire 500-recording batch because 39 failed individual quality gates discards 461 valid recordings. This is not the policy — per-recording gates apply per recording. The FAIL aggregation is appropriate only for license gate failures (entire batch contamination).",
  "sourceAnchors": [
    {
      "sourceId": "COMMON-VOICE",
      "url": "https://github.com/common-voice/common-voice",
      "section": "Speech corpus quality gates — per-recording SNR and alignment gates vs license batch gate",
      "license": "CC-0"
    }
  ],
  "licenseConstraint": "CC-0",
  "trainingStatus": "candidate"
}
```

---

## EXT-243

```json
{
  "rowId": "EXT-243",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "react-error-boundary",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React component fetches user data and renders a profile card. When the fetch fails, the component should show an error message instead of crashing the application. A developer wrapped the fetch call in a try-catch and rendered error JSX conditionally. A reviewer suggested using a React Error Boundary. Which approach is correct?",
    "b": "Component code: const [user, setUser] = useState(null); const [error, setError] = useState(null); useEffect(() => { fetch('/api/user').then(r => r.json()).then(setUser).catch(setError); }, []); if (error) return <div>Error: {error.message}</div>; if (!user) return <div>Loading...</div>; return <UserCard user={user} />;",
    "c": "React Error Boundaries: class components that catch JavaScript errors in their child component tree during rendering. They do NOT catch errors in: event handlers, asynchronous code (setTimeout, fetch), server-side rendering. The developer's try-catch in a useEffect/Promise chain is the correct pattern for async data fetching errors. Error Boundaries catch synchronous rendering errors (e.g., a component throws during render due to bad data)."
  },
  "function": "Arbitrate: try-catch in useEffect or React Error Boundary for the fetch error case?",
  "acceptedOutput": {
    "verdict": "The try-catch/catch(.setError) pattern is correct for async fetch errors.",
    "reasoning": "React Error Boundaries do not catch errors in asynchronous code — they catch errors during the React rendering phase (render methods, lifecycle methods). A rejected Promise from fetch() is an asynchronous error that occurs outside the React rendering phase. The catch handler sets the error state (setError), which triggers a re-render that shows the error UI. This is the correct React pattern for async data fetching error handling.",
    "whenToUseErrorBoundary": "Add a React Error Boundary as a parent wrapper to catch rendering errors: if the UserCard component throws during render (e.g., user.name is undefined when expected to be a string), the Error Boundary catches it. The two mechanisms are complementary: try-catch for async, Error Boundary for rendering.",
    "recommendation": "Keep the existing try-catch pattern. Optionally add an Error Boundary as a parent to catch unexpected rendering errors from UserCard. Do not replace the async error handling with an Error Boundary."
  },
  "rejectedOutput": {
    "verdict": "Use a React Error Boundary. Wrap the component in an Error Boundary to catch all errors including the fetch failure. This centralizes error handling."
  },
  "arbiters": ["reactErrorHandlingArbiter"],
  "whyAccepted": "The accepted verdict is factually correct: Error Boundaries do not catch Promise rejections. Replacing the try-catch with an Error Boundary would leave async fetch errors uncaught — they would surface as unhandled Promise rejections and potentially crash the application.",
  "whyRejected": "The rejected verdict contains a factual error: Error Boundaries do NOT catch errors in async code (fetch, setTimeout, Promises). The React documentation explicitly states this limitation. An Error Boundary cannot replace a try-catch in a useEffect — the fetch error would not be caught.",
  "sourceAnchors": [
    {
      "sourceId": "REACT-ADVANCED",
      "url": "https://github.com/facebook/react",
      "section": "Error Boundaries — catch rendering errors, NOT async/Promise errors; useEffect catch for fetch failures",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-244

```json
{
  "rowId": "EXT-244",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "B_ROUTING",
  "subdomain": "angular-lazy-loading",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Our Angular application has 8 feature modules: Dashboard, Orders, Products, Customers, Reports, Settings, Admin, and Analytics. All 8 modules are currently eagerly loaded at startup. The initial bundle size is 4.2 MB and initial load time is 6.8 seconds on a 4G connection. Should we implement lazy loading?",
    "b": "Usage data: Dashboard is loaded by 100% of users on every session. Orders and Products are used by 90% of users. Customers by 60%. Reports by 40%. Settings by 20%. Admin by 5% (admin users only). Analytics by 15%.",
    "c": "Angular lazy loading: feature modules are loaded on-demand when the user navigates to their route. The main bundle includes only the eagerly loaded modules. Lazy-loaded modules are separate chunks downloaded when first accessed. Angular Router lazy loading: { path: 'admin', loadChildren: () => import('./admin/admin.module').then(m => m.AdminModule) }. Trade-off: first navigation to a lazy route has a brief delay (chunk download). Subsequent navigations use the cached chunk."
  },
  "function": "Produce the command: which modules to lazy load and in what order of priority.",
  "acceptedOutput": {
    "command": "Lazy load 5 modules. Keep Dashboard, Orders, Products eagerly loaded.",
    "lazyLoadingPriority": [
      "Priority 1 (LAZY — highest impact): Admin module — 5% usage, all admin users vs 95% general users who download it unnecessarily. Estimated chunk: large admin feature set.",
      "Priority 2 (LAZY): Analytics — 15% usage. Only 15% of users need the analytics bundle at startup.",
      "Priority 3 (LAZY): Settings — 20% usage. Infrequently accessed, users tolerate a brief load on first navigation.",
      "Priority 4 (LAZY): Reports — 40% usage. Acceptable lazy load delay for report generation features.",
      "Priority 5 (LAZY): Customers — 60% usage. Borderline — consider preloading with PreloadAllModules or a custom preloading strategy."
    ],
    "keepEager": "Dashboard (100% usage, must be instant), Orders (90% usage, critical path), Products (90% usage, critical path).",
    "estimatedImpact": "Admin + Analytics + Settings alone likely represent 30–50% of the current bundle. Initial load time reduction: estimated 3–4 seconds for low-usage module removal from main bundle."
  },
  "rejectedOutput": {
    "command": "Lazy load all 8 modules. Every module should be lazy loaded for maximum initial performance."
  },
  "arbiters": ["angularPerformanceArbiter"],
  "whyAccepted": "Dashboard, Orders, and Products are used by 90–100% of users — lazy loading them adds a navigation delay for the majority of users without benefit (they would be loaded immediately on first navigation anyway). The accepted approach targets the low-usage modules (Admin at 5%, Analytics at 15%) for the highest impact with the lowest UX cost.",
  "whyRejected": "Lazy loading Dashboard means 100% of users experience a delay on first navigation to the most-used feature. Lazy loading Orders and Products (90% usage) converts an eager load into a lazy load that triggers for 90% of users — converting a startup delay into a navigation delay without reducing total load time for most users.",
  "sourceAnchors": [
    {
      "sourceId": "ANGULAR-DOCS",
      "url": "https://github.com/angular/angular",
      "section": "Lazy loading feature modules — loadChildren, usage-based prioritization, eager vs lazy threshold",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-245

```json
{
  "rowId": "EXT-245",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "exception-group-adoption",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.10 async task runner collects results from multiple concurrent tasks and raises the first exception it encounters. A reviewer suggested migrating to Python 3.11 ExceptionGroup (PEP 654) to propagate all exceptions simultaneously. Should we migrate?",
    "b": "Current code (Python 3.10): async def run_all(tasks): results = []; exceptions = []; for task in asyncio.as_completed(tasks): try: results.append(await task) except Exception as e: exceptions.append(e); if exceptions: raise exceptions[0]  # Only first exception propagated. Context: 5 tasks run concurrently; if 3 fail, only the first failure is visible. The other 2 failures are silently discarded.",
    "c": "PEP 654 (Python 3.11+): ExceptionGroup allows raising multiple exceptions simultaneously. 'raise ExceptionGroup(\"task failures\", exceptions)'. except* syntax handles individual exception types within the group: 'except* ValueError as eg: ...' TaskGroup (asyncio.TaskGroup) in Python 3.11 uses ExceptionGroup automatically — all task failures are collected and raised as a group."
  },
  "function": "Decide: migrate to ExceptionGroup/TaskGroup or preserve the first-exception pattern.",
  "acceptedOutput": {
    "decision": "MIGRATE to asyncio.TaskGroup (Python 3.11+) which uses ExceptionGroup automatically.",
    "rationale": "The current code silently discards 2 of 3 exceptions in the failing scenario. A task runner that hides failures is a debugging hazard — the operator sees one error, fixes it, reruns, discovers the next, repeats. TaskGroup collects all failures into an ExceptionGroup. Three improvements: (1) No silent exception discard. (2) except* syntax allows handling ValueError, TimeoutError, and NetworkError independently within one handler. (3) asyncio.TaskGroup handles cancellation correctly — if one task fails, the group cancels remaining tasks and collects all raised exceptions.",
    "migrationCode": "async def run_all(tasks):\n    async with asyncio.TaskGroup() as tg:\n        task_refs = [tg.create_task(t) for t in tasks]\n    return [t.result() for t in task_refs]  # ExceptionGroup raised automatically if any task failed",
    "gate": "Re-run the failing scenario with 3 failing tasks — ExceptionGroup contains all 3 exceptions. No exceptions silently discarded."
  },
  "rejectedOutput": {
    "decision": "DO NOT MIGRATE. ExceptionGroup is a Python 3.11 feature that requires a version upgrade. The current first-exception pattern is well-understood. Callers expect a single exception, not an ExceptionGroup."
  },
  "arbiters": ["asyncErrorHandlingArbiter"],
  "whyAccepted": "Silent exception discard is a correctness defect, not a stylistic preference. A task runner that hides 2 of 3 failures gives the operator an incomplete picture of system state. The version upgrade to Python 3.11 is the prerequisite — if the team is on 3.10, that upgrade decision should be evaluated separately.",
  "whyRejected": "The 'callers expect a single exception' argument preserves a known defect (silent discard) as a design constraint. If callers currently catch the first exception and ignore the rest, they may already be missing failures. The migration changes the exception type — callers need to update except clauses, which is a one-time migration cost.",
  "sourceAnchors": [
    {
      "sourceId": "PYTHON-PEPS",
      "url": "https://peps.python.org/pep-0654/",
      "section": "PEP 654 — ExceptionGroup; asyncio.TaskGroup; except* syntax; silent discard vs full propagation",
      "license": "CC-BY-4.0"
    }
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-246

```json
{
  "rowId": "EXT-246",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "session-fixation-detection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Review this authentication implementation in an ASP.NET Core application.",
    "b": "Implementation: [HttpPost] public async Task<IActionResult> Login(LoginRequest request) { var user = await _authService.ValidateCredentials(request.Username, request.Password); if (user == null) return Unauthorized(); HttpContext.Session.SetString('UserId', user.Id.ToString()); return Ok(); }",
    "c": "CWE-287 (Improper Authentication) — Session Fixation: an attacker obtains a valid session ID before authentication (e.g., by visiting the site as an anonymous user). If the application does not regenerate the session ID after successful login, the attacker's pre-obtained session ID is now authenticated. The attacker sends the victim a link with the session ID in the URL or cookie — after the victim logs in, the attacker's session is elevated to the victim's privileges."
  },
  "function": "Arbitrate: is this authentication implementation secure? What is missing?",
  "acceptedOutput": {
    "verdict": "VULNERABLE. Session fixation defect — CWE-287.",
    "missingControl": "The implementation does not regenerate the session ID after successful login. ASP.NET Core: HttpContext.Session does not expose a 'regenerate session ID' method directly. The fix requires clearing the old session and issuing a new one, or using the authentication middleware (Cookie Authentication) which handles session regeneration correctly.",
    "correctImplementation": "Use ASP.NET Core Cookie Authentication instead of manual session management: await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity)). Cookie Authentication issues a new authentication ticket on sign-in, preventing session fixation. If manual session management is required: HttpContext.Session.Clear(); await HttpContext.Session.CommitAsync(); — then set the user ID in the new session. The session cookie will be re-issued with a new ID.",
    "cweReference": "CWE-287: Improper Authentication — session fixation via pre-login session ID reuse after authentication."
  },
  "rejectedOutput": {
    "verdict": "The implementation is correct. The session is set after credential validation — only authenticated users can set the UserId session value."
  },
  "arbiters": ["sessionSecurityArbiter", "cweComplianceArbiter"],
  "whyAccepted": "The rejected verdict is factually incorrect about the attack. In session fixation, the attacker does not set the session value — they obtain the session ID before login and wait for the victim to authenticate using that ID. After authentication, the attacker's pre-obtained ID is now mapped to an authenticated session. The fix is session ID regeneration on login, not credential validation.",
  "whyRejected": "The rejected verdict misunderstands the attack: the attacker does not need to set the UserId — they need the server to keep using the same session ID after login. The 'only authenticated users can set UserId' statement is true but irrelevant to the fixation attack vector.",
  "sourceAnchors": [
    {
      "sourceId": "CWE-TOP25",
      "url": "https://cwe.mitre.org/data/definitions/287.html",
      "section": "CWE-287: Improper Authentication — session fixation; session ID regeneration required after login",
      "license": "MITRE notice required"
    }
  ],
  "licenseConstraint": "MITRE notice required",
  "trainingStatus": "candidate"
}
```

---

## EXT-247

```json
{
  "rowId": "EXT-247",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G_LEARNING_TRAINING",
  "subdomain": "tls-version-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "Our API gateway currently accepts TLS 1.0, 1.1, 1.2, and 1.3. A security audit flagged TLS 1.0 and 1.1 as deprecated. Should we disable TLS 1.0 and 1.1?",
    "b": "Traffic analysis: TLS 1.3: 72% of connections. TLS 1.2: 27% of connections. TLS 1.1: 0.8% of connections. TLS 1.0: 0.2% of connections. TLS 1.0 and 1.1 combined: 1% of connections. The API serves mobile apps (iOS 12+, Android 7+) and web browsers. A small number of partner integrations use legacy HTTP clients.",
    "c": "RFC 8996 (Deprecating TLS 1.0 and 1.1): published March 2021. MUST NOT use TLS 1.0 and TLS 1.1. TLS 1.2 is supported by: iOS 9+ (2015), Android 5+ (2014), all modern browsers. TLS 1.1 and 1.0 support: iOS 11 and earlier, Android 4.x and earlier. POODLE, BEAST, SWEET32 attacks affect TLS 1.0/1.1 — these protocols have known cryptographic vulnerabilities."
  },
  "function": "Decide: disable TLS 1.0 and 1.1, and what is the migration plan?",
  "acceptedOutput": {
    "decision": "MIGRATE — disable TLS 1.0 and TLS 1.1.",
    "rationale": "RFC 8996 mandates that TLS 1.0 and 1.1 MUST NOT be used. The 1% of connections on deprecated protocols are at risk for POODLE and BEAST attacks. TLS 1.2 support extends to 2014-era devices (Android 5+, iOS 9+) — the mobile app requirements (iOS 12+, Android 7+) are well within TLS 1.2 support. The 1% legacy traffic represents clients that must upgrade.",
    "migrationPlan": [
      "Step 1: Identify the 1% legacy clients — log the TLS version and User-Agent for TLS 1.0/1.1 connections for 14 days. This identifies specific partner integrations.",
      "Step 2: Notify identified partners — 90-day migration notice that TLS 1.0/1.1 will be disabled.",
      "Step 3: Configure the API gateway to return a specific error response for TLS 1.0/1.1 connections (if the gateway supports it) before full disablement.",
      "Step 4: Disable TLS 1.0 and 1.1 after the 90-day notice period. Monitor for connection failures."
    ],
    "gate": "Zero TLS 1.0/1.1 connections in access logs 14 days after disablement."
  },
  "rejectedOutput": {
    "decision": "DO NOT DISABLE. The 1% of connections on TLS 1.0/1.1 represent real users. Disabling would break their access. The security risk is theoretical — POODLE requires a specific network position."
  },
  "arbiters": ["tlsSecurityArbiter"],
  "whyAccepted": "RFC 8996 is a MUST NOT directive, not a recommendation. POODLE and BEAST are not theoretical — they are published attacks with known exploits. The 1% traffic on deprecated protocols does not represent a business case for maintaining known-vulnerable TLS versions. The 90-day migration plan provides partners time to upgrade.",
  "whyRejected": "POODLE (Padding Oracle On Downgraded Legacy Encryption) and BEAST (Browser Exploit Against SSL/TLS) require a specific network position (MITM), but 'requires a specific position' is not 'theoretical' — they are real attacks. More importantly, RFC 8996 is a standards-track MUST NOT — compliance with published security standards is not optional.",
  "sourceAnchors": [
    {
      "sourceId": "IETF-RFCS",
      "url": "https://www.rfc-editor.org/rfc/rfc8996",
      "section": "RFC 8996: Deprecating TLS 1.0 and TLS 1.1 — MUST NOT use; POODLE/BEAST vulnerabilities; migration plan",
      "license": "IETF Trust provisions"
    }
  ],
  "licenseConstraint": "IETF Trust provisions",
  "trainingStatus": "candidate"
}
```

---

## EXT-248

```json
{
  "rowId": "EXT-248",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D_EXECUTION_APPLIER",
  "subdomain": "sensor-outlier-aggregation",
  "rowClass": "negative-example",
  "input": {
    "a": "Execute: aggregate temperature sensor readings from a list. Remove outliers using the IQR (Interquartile Range) method. Return the mean, standard deviation, and the count of outliers removed.",
    "b": "Function signature: aggregate_sensor_readings(readings: list[float]) -> dict. Return: { 'mean': float, 'std_dev': float, 'outliers_removed': int }. IQR outlier rule: values below Q1 - 1.5*IQR or above Q3 + 1.5*IQR are outliers. Q1 = 25th percentile, Q3 = 75th percentile, IQR = Q3 - Q1.",
    "c": "Example: readings = [20.1, 20.5, 21.0, 20.8, 55.3, 20.2, 19.9, 20.6, -10.2, 21.1]. Q1=20.15, Q3=20.9, IQR=0.75, lower=18.275, upper=23.025. Outliers: 55.3 and -10.2. Clean readings: [20.1, 20.5, 21.0, 20.8, 20.2, 19.9, 20.6, 21.1]. Mean=20.525, std_dev=0.394."
  },
  "function": "Implement aggregate_sensor_readings with IQR outlier removal.",
  "acceptedOutput": {
    "implementation": "import statistics\n\ndef aggregate_sensor_readings(readings: list) -> dict:\n    if len(readings) < 4:\n        raise ValueError(f'Need at least 4 readings for IQR calculation, got {len(readings)}')\n    sorted_r = sorted(readings)\n    n = len(sorted_r)\n    q1 = sorted_r[n // 4]\n    q3 = sorted_r[(3 * n) // 4]\n    iqr = q3 - q1\n    lower = q1 - 1.5 * iqr\n    upper = q3 + 1.5 * iqr\n    clean = [x for x in readings if lower <= x <= upper]\n    outliers_removed = len(readings) - len(clean)\n    return {\n        'mean': statistics.mean(clean),\n        'std_dev': statistics.stdev(clean) if len(clean) > 1 else 0.0,\n        'outliers_removed': outliers_removed\n    }",
    "behavior": "For readings=[20.1,20.5,21.0,20.8,55.3,20.2,19.9,20.6,-10.2,21.1]: outliers_removed=2, mean=20.525, std_dev=0.394."
  },
  "rejectedOutput": {
    "implementation": "def aggregate_sensor_readings(readings):\n    mean = sum(readings) / len(readings)\n    variance = sum((x - mean) ** 2 for x in readings) / len(readings)\n    std_dev = variance ** 0.5\n    return {'mean': mean, 'std_dev': std_dev, 'outliers_removed': 0}"
  },
  "arbiters": ["statisticalAggregationArbiter"],
  "whyAccepted": "The accepted implementation performs IQR calculation and outlier removal as specified. For the example input, it correctly identifies 55.3 and -10.2 as outliers and returns the clean mean and standard deviation.",
  "whyRejected": "The rejected implementation computes mean and standard deviation on the full dataset including outliers. It returns outliers_removed=0 always — outlier removal is not implemented. The mean of the full dataset including 55.3 and -10.2 is (20.1+20.5+...+55.3+(-10.2))/10 = 22.95 — 2.4 degrees higher than the correct clean mean of 20.525. Standard deviation over the full set is ~17 vs the correct 0.394.",
  "sourceAnchors": [
    {
      "sourceId": "BIGCODEBENCH",
      "url": "https://github.com/bigcode-project/bigcodebench",
      "section": "Task: aggregate_sensor_readings — IQR outlier removal vs no-op outliers_removed=0",
      "license": "Apache-2.0"
    }
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "candidate"
}
```

---

## EXT-249

```json
{
  "rowId": "EXT-249",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "dependency-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Aggregate results from three analysis agents for a pull request that updates the project's npm (Node Package Manager) dependency manifest.",
    "b": "Agent results: Agent 1 (Dependency Security Agent): BLOCK — the updated manifest introduces lodash@4.17.19 which has a known prototype pollution vulnerability (CVE-2020-8203, CVSS 7.4 HIGH). Agent 2 (Test Agent): PASS — all tests pass. Agent 3 (License Agent): PASS — all new dependencies are MIT or Apache-2.0.",
    "c": "Security policy: new dependencies with CVSS >= 7.0 severity vulnerabilities MUST NOT be merged. The dependency was introduced via an indirect dependency update — a direct dependency updated its version of lodash. CVE-2020-8203 is a prototype pollution vulnerability in lodash's merge/zipObjectDeep functions. The fix is lodash@4.17.21."
  },
  "function": "Produce the aggregated result for this pull request.",
  "acceptedOutput": {
    "aggregatedResult": "BLOCK",
    "resultCode": "KNOWN_CVE_IN_NEW_DEPENDENCY",
    "summary": "The updated dependency manifest introduces lodash@4.17.19, which contains CVE-2020-8203 (prototype pollution, CVSS 7.4 HIGH). Security policy prohibits merging dependencies with CVSS >= 7.0 vulnerabilities. Tests pass and licenses are compliant, but the security gate is the blocking condition.",
    "requiredActions": [
      "Pin lodash to @4.17.21 in the project's package.json overrides or resolutions: 'overrides': { 'lodash': '^4.17.21' }",
      "Re-run 'npm audit' to confirm no HIGH or CRITICAL vulnerabilities remain",
      "Re-submit the PR after the override is added"
    ],
    "blockingAgent": "Dependency Security Agent (CVE-2020-8203, CVSS 7.4)",
    "passingAgents": ["Test Agent", "License Agent"]
  },
  "rejectedOutput": {
    "aggregatedResult": "PASS_WITH_ADVISORY",
    "summary": "Tests pass and licenses are compliant. A known CVE in an indirect dependency is noted — recommend addressing in a security-focused follow-up PR."
  },
  "arbiters": ["dependencySecurityArbiter"],
  "whyAccepted": "CVSS 7.4 exceeds the 7.0 threshold — the security gate is triggered. Whether the vulnerability is in a direct or indirect dependency is irrelevant to the policy threshold: the vulnerability exists in the deployed artifact. The npm overrides mechanism provides a surgical fix that does not require reverting the entire dependency update.",
  "whyRejected": "PASS_WITH_ADVISORY with a follow-up PR allows a known CVE-bearing dependency to reach production. The 'indirect dependency' framing does not change the runtime exposure — the lodash@4.17.19 code is in the deployed node_modules regardless of whether it was introduced directly or transitively.",
  "sourceAnchors": [
    {
      "sourceId": "SWE-BENCH",
      "url": "https://github.com/princeton-nlp/SWE-bench",
      "section": "Dependency security gate — BLOCK for CVE-2020-8203 CVSS 7.4; npm overrides for indirect dependency pin",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```

---

## EXT-250

```json
{
  "rowId": "EXT-250",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E_ARBITER_QUALITY",
  "subdomain": "nestjs-rate-limiting-placement",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We need to apply rate limiting to our NestJS API. The requirements are: 100 requests per minute for the public /api/search endpoint, 1000 requests per minute for authenticated endpoints, and no rate limiting for internal health check endpoints. Should we use a global ThrottlerGuard or per-endpoint throttle decorators?",
    "b": "Current setup: NestJS application with @nestjs/throttler installed. Global ThrottlerGuard configured in AppModule with ttl=60, limit=100.",
    "c": "NestJS @nestjs/throttler: ThrottlerModule.forRoot configures the global default. @Throttle() decorator overrides the default for a specific route or controller. @SkipThrottle() decorator exempts a route from throttling. Global guard with route-level overrides is the recommended pattern: global guard handles the default, @Throttle() increases/decreases for specific routes, @SkipThrottle() exempts internal routes."
  },
  "function": "Arbitrate: global ThrottlerGuard with decorators or per-endpoint configuration?",
  "acceptedOutput": {
    "verdict": "Global ThrottlerGuard with per-endpoint decorator overrides is correct.",
    "implementation": {
      "globalConfig": "ThrottlerModule.forRoot([{ ttl: 60000, limit: 100 }]) in AppModule — this applies 100 req/min to all routes by default.",
      "authenticatedEndpoints": "@Throttle({ default: { ttl: 60000, limit: 1000 } }) on the AuthenticatedController — overrides the global default to 1000 req/min for all endpoints in that controller.",
      "healthEndpoint": "@SkipThrottle() on the HealthController.check() method — exempts /health from throttling entirely.",
      "searchEndpoint": "No decorator needed — inherits the global 100 req/min default."
    },
    "rationale": "Global guard with decorator overrides is the idiomatic NestJS pattern. It establishes a secure default (100 req/min) that protects all endpoints unless explicitly overridden. New endpoints added to the codebase automatically inherit the rate limit. @SkipThrottle() must be applied explicitly — internal endpoints are not exempt by default unless decorated."
  },
  "rejectedOutput": {
    "verdict": "Use per-endpoint throttle decorators. Apply @Throttle() to every endpoint individually with the specific limit. This makes rate limits explicit at each endpoint."
  },
  "arbiters": ["nestjsRateLimitingArbiter"],
  "whyAccepted": "A global guard with decorator overrides ensures all new endpoints are rate-limited by default — developers cannot accidentally add an unprotected endpoint. Per-endpoint decoration requires every developer adding an endpoint to remember to apply the decorator; forgetting results in an unprotected endpoint.",
  "whyRejected": "Per-endpoint decoration scales poorly: 50 endpoints each decorated individually is 50 places to update when the rate limiting policy changes. 'Explicit' is not valuable when the default should apply to the majority of endpoints. The global guard provides the correct default with targeted exceptions.",
  "sourceAnchors": [
    {
      "sourceId": "NESTJS-ADVANCED",
      "url": "https://github.com/nestjs/nest",
      "section": "ThrottlerGuard global with @Throttle override and @SkipThrottle exemption — rate limiting hierarchy",
      "license": "MIT"
    }
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "candidate"
}
```
