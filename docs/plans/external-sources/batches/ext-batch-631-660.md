# EXT Batch 22 — EXT-431 through EXT-450

## EXT-431
**registryId:** KUBERNETES-KEPS  
**unitTitle:** KEDA ScaledJob vs ScaledObject for burst-then-idle batch workload scaling  
**sourceUrl:** https://keda.sh/docs/2.13/concepts/scaling-jobs/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-431-R1",
  "sourceIds": ["EXT-431"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-topology-change",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "We have a video transcoding queue that receives 500 jobs at 9 AM and is idle from 10 AM to 9 PM. Jobs take 2–5 minutes each. Should we use KEDA ScaledJob or ScaledObject?",
    "b": "Current approach: ScaledObject with minReplicas=1, maxReplicas=50, targeting queue length / 10. Problem: pods stay Running after the burst drains, wasting resources for 12 idle hours. A colleague suggests ScaledJob instead.",
    "c": "Cluster has spot node pools. Jobs are idempotent and restartable. Queue is RabbitMQ."
  },
  "function": "Recommend KEDA ScaledJob vs ScaledObject and explain the architectural tradeoff for burst-then-idle batch workloads.",
  "acceptedOutput": "Use KEDA **ScaledJob** for this pattern.\n\n**Why ScaledJob wins here:**\n- ScaledJob creates a new Kubernetes Job per scale-out event; pods terminate when the Job completes. With ScaledObject, pods stay Running between messages and you pay for 12 idle hours of spot instances.\n- For 500 jobs at 2–5 min each, ScaledJob with `maxReplicaCount: 50` and `pollingInterval: 10` drains the queue in ~20–50 min then scales to zero automatically.\n\n**ScaledJob manifest:**\n```yaml\napiVersion: keda.sh/v1alpha1\nkind: ScaledJob\nmetadata:\n  name: video-transcode\nspec:\n  jobTargetRef:\n    parallelism: 1\n    completions: 1\n    template:\n      spec:\n        restartPolicy: OnFailure\n        containers:\n        - name: transcoder\n          image: transcode:v2\n          resources:\n            requests: { cpu: \"2\", memory: \"4Gi\" }\n  pollingInterval: 10\n  maxReplicaCount: 50\n  triggers:\n  - type: rabbitmq\n    metadata:\n      queueName: transcode-jobs\n      queueLength: \"1\"  # one Job per message\n  scalingStrategy:\n    strategy: accurate\n```\n\n**Use ScaledObject when:** you have a long-running consumer that maintains a persistent connection (e.g., Kafka consumer group with offset commits) — terminating the pod would lose in-flight state.\n\n**Spot node consideration:** set `spec.jobTargetRef.template.spec.tolerations` for spot taints and ensure the transcoder checkpoints progress so a preempted Job restarts cleanly.",
  "rejectedOutput": "Use ScaledObject with minReplicas=0. Set the target queue length low and it will scale down eventually.",
  "arbiters": ["KEDA-DOCS", "KUBERNETES-KEPS"],
  "whyAccepted": "ScaledJob is purpose-built for run-to-completion workloads; it creates Job resources that terminate on completion, achieving true scale-to-zero. ScaledObject is designed for stateful consumers that must stay running.",
  "whyRejected": "ScaledObject scale-down is based on metrics polling with a cooldown window (default 300s); pods are not guaranteed to terminate when the queue drains, and 'eventually' is not a Gate B.",
  "sourceAnchors": ["https://keda.sh/docs/2.13/concepts/scaling-jobs/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-432
**registryId:** BIGCODEBENCH  
**unitTitle:** asyncio.gather vs sequential await for independent I/O-bound coroutines  
**sourceUrl:** https://github.com/bigcode-project/bigcodebench  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-432-R1",
  "sourceIds": ["EXT-432"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-execution-concurrency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Python asyncio service fetches user profile, order history, and recommendations from three separate microservices before building a dashboard response.",
    "b": "Current code: `profile = await fetch_profile(uid)` then `orders = await fetch_orders(uid)` then `recs = await fetch_recs(uid)`. Each call takes ~150 ms. Total: ~450 ms.",
    "c": "All three calls are independent — no data dependency between them."
  },
  "function": "Identify the anti-pattern and produce the corrected implementation using asyncio.gather.",
  "acceptedOutput": "**Anti-pattern:** sequential `await` for independent coroutines serializes I/O that could run concurrently, tripling latency unnecessarily.\n\n**Fixed implementation:**\n```python\nimport asyncio\n\nasync def build_dashboard(uid: str) -> dict:\n    # Run all three independent fetches concurrently\n    profile, orders, recs = await asyncio.gather(\n        fetch_profile(uid),\n        fetch_orders(uid),\n        fetch_recs(uid),\n        return_exceptions=True,  # don't cancel siblings on first failure\n    )\n    # Handle partial failures gracefully\n    if isinstance(profile, Exception):\n        raise ServiceError(\"profile unavailable\") from profile\n    if isinstance(orders, Exception):\n        orders = []  # degrade gracefully — empty order history\n    if isinstance(recs, Exception):\n        recs = []   # degrade gracefully — empty recommendations\n    return {\"profile\": profile, \"orders\": orders, \"recs\": recs}\n```\n\n**Result:** ~150 ms (bounded by the slowest call) vs ~450 ms sequential. `return_exceptions=True` prevents one failed fetch from cancelling the other two via `asyncio.gather` default behaviour.\n\n**When sequential is correct:** when call B needs data from call A (e.g., `orders = await fetch_orders(profile.account_id)`).",
  "rejectedOutput": "Use `asyncio.create_task()` for each and then `await` them individually in order.",
  "arbiters": ["PYTHON-ASYNCIO-DOCS", "BIGCODEBENCH"],
  "whyAccepted": "asyncio.gather is the idiomatic, correct API for concurrent independent coroutines with structured error handling via return_exceptions. Latency reduction is measurable and concrete.",
  "whyRejected": "create_task + sequential await starts tasks concurrently but still awaits in serial order — if task[0] is slow, you block before awaiting task[1] even though task[1] may have completed. gather handles all results atomically.",
  "sourceAnchors": ["https://docs.python.org/3/library/asyncio-task.html#asyncio.gather"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-433
**registryId:** SWE-BENCH  
**unitTitle:** Django ORM prefetch_related vs select_related for reverse FK and M2M relations  
**sourceUrl:** https://github.com/princeton-nlp/SWE-bench  
**license:** MIT | **trustTier:** 2  
**domainsTagged:** E | **familiesTagged:** 4

### Training Row

```json
{
  "rowId": "EXT-433-R1",
  "sourceIds": ["EXT-433"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E2-result-quality-filter",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Django API endpoint returns a list of Author objects, each with their associated Books (reverse FK from Book.author). Currently using select_related('books') — but Django raises an error: 'Invalid field name(s) for model Author: books'.",
    "b": "Team lead says to use prefetch_related instead. Junior developer asks: what is the actual difference and when would you use each?",
    "c": "Author has many Books (one-to-many). Books have many Tags (many-to-many). Need both books and tags per author."
  },
  "function": "Explain select_related vs prefetch_related and produce the correct queryset for the described schema.",
  "acceptedOutput": "**Rule:**\n- `select_related`: SQL JOIN, works for **ForeignKey (forward)** and **OneToOneField** only — fetches related object in the same query.\n- `prefetch_related`: separate query per relation, works for **reverse FK** (Author→Books) and **ManyToMany** — fetches all related objects in one additional SQL query and joins in Python.\n\n**Why select_related fails:** `Author.books` is a reverse FK accessor (Django auto-creates it as `RelatedManager`), not a concrete FK field — you cannot JOIN through it with select_related.\n\n**Correct queryset:**\n```python\nfrom django.db.models import Prefetch\n\nauthors = (\n    Author.objects\n    .prefetch_related(\n        Prefetch('books', queryset=Book.objects.prefetch_related('tags'))\n    )\n    .order_by('name')\n)\n# Result: 3 SQL queries total\n# Q1: SELECT * FROM author ORDER BY name\n# Q2: SELECT * FROM book WHERE author_id IN (...)\n# Q3: SELECT * FROM book_tags JOIN tag WHERE book_id IN (...)\n# Zero N+1 regardless of author count\n```\n\n**select_related is correct for the forward FK:**\n```python\n# Book → Author (forward FK) — use select_related\nbooks = Book.objects.select_related('author')\n```",
  "rejectedOutput": "Use select_related for all relations — it is always more efficient because it uses JOINs.",
  "arbiters": ["DJANGO-DOCS", "SWE-BENCH"],
  "whyAccepted": "select_related cannot traverse reverse FK or M2M relations — Django will raise an error or silently ignore the field. prefetch_related with nested Prefetch objects is the correct pattern for the described schema and eliminates N+1.",
  "whyRejected": "JOINs for one-to-many produce Cartesian product rows (one row per book per author), causing data duplication and potentially worse performance than two clean queries for large result sets.",
  "sourceAnchors": ["https://docs.djangoproject.com/en/5.0/ref/models/querysets/#prefetch-related"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-434
**registryId:** PYTHON-PEPS  
**unitTitle:** PEP-526 variable annotations type: ignore vs TYPE_CHECKING guard for runtime cost  
**sourceUrl:** https://peps.python.org/pep-0526/  
**license:** PSF-2.0 | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-434-R1",
  "sourceIds": ["EXT-434"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-execution-validation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Python module has 200 class-level annotations referencing complex generic types like `dict[str, list[tuple[int, str]]]`. At import time, Python evaluates these annotations eagerly, adding ~15 ms to startup.",
    "b": "Developer adds `from __future__ import annotations` (PEP-563) to defer evaluation. mypy still works. But a colleague warns this breaks `pydantic.BaseModel` field discovery at runtime.",
    "c": "Project uses Python 3.10, Pydantic v2, and FastAPI."
  },
  "function": "Identify the correct approach to eliminate annotation evaluation cost without breaking Pydantic v2 and FastAPI.",
  "acceptedOutput": "**Root conflict:** `from __future__ import annotations` (PEP-563) makes ALL annotations strings (lazy), which breaks Pydantic v2's runtime type introspection — Pydantic reads `__annotations__` at class construction time to build validators.\n\n**Correct approach for Pydantic + FastAPI:** do NOT use `from __future__ import annotations`. Instead:\n\n1. **Use `TYPE_CHECKING` guard for expensive imports:**\n```python\nfrom __future__ import annotations  # REMOVE this from Pydantic models\nfrom typing import TYPE_CHECKING\n\nif TYPE_CHECKING:\n    from myapp.heavy_module import HeavyType  # only imported by mypy, not at runtime\n\nclass MyModel(BaseModel):\n    # For Pydantic fields, use string forward references where needed\n    items: list[\"HeavyType\"] | None = None\n```\n\n2. **For non-Pydantic modules** with expensive annotation types, `from __future__ import annotations` is safe.\n\n3. **Measure first:** use `python -X importtime -c 'import myapp'` to identify which modules contribute to the 15 ms.\n\n**Pydantic v2 + `from __future__ import annotations`:** Pydantic v2 added `model_rebuild()` to handle string annotations — call it after all forward references are defined. But this adds complexity; prefer selective use.",
  "rejectedOutput": "Add `from __future__ import annotations` to all files — Pydantic v2 handles deferred annotations automatically.",
  "arbiters": ["PYTHON-PEPS", "PYDANTIC-DOCS"],
  "whyAccepted": "Pydantic v2 does not automatically resolve all deferred annotations in all cases — model_rebuild() is required for forward references, and blanket PEP-563 usage in Pydantic model files is a known source of runtime errors. The TYPE_CHECKING guard is the safe, targeted solution.",
  "whyRejected": "The claim that Pydantic v2 handles deferred annotations automatically is partially true (with model_rebuild) but the blanket statement is incorrect and will cause subtle runtime failures in common cases.",
  "sourceAnchors": ["https://peps.python.org/pep-0526/", "https://docs.pydantic.dev/latest/concepts/postponed_annotations/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-435
**registryId:** OPENTELEMETRY-SPEC  
**unitTitle:** OTel exemplars linking metrics to traces for P99 latency root cause  
**sourceUrl:** https://opentelemetry.io/docs/specs/otel/metrics/sdk/#exemplar  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-435-R1",
  "sourceIds": ["EXT-435"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-evidence-linkage",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Prometheus dashboard shows P99 HTTP latency spiking to 2s every 5 minutes but the average stays at 80 ms. Team cannot correlate the metric spike to a specific trace to diagnose the root cause.",
    "b": "System uses OTel .NET SDK with OTLP exporter to Grafana Tempo (traces) and Prometheus (metrics via OTel Collector). Metrics are histograms. No exemplars configured.",
    "c": "Need to link the P99 spike metric data points to the specific trace IDs that caused them, without adding a full distributed tracing search for every request."
  },
  "function": "Design the OTel exemplar configuration to link P99 metric spikes to specific trace IDs for root cause analysis.",
  "acceptedOutput": "**OTel Exemplars** attach a `{traceId, spanId, timestamp, value}` sample to histogram bucket observations. When Prometheus scrapes the metric, each bucket carries exemplars for recent high-value observations — allowing direct navigation from a slow P99 bucket to the exact trace.\n\n**Configuration (.NET OTel SDK):**\n```csharp\n// In Program.cs\nbuilder.Services.AddOpenTelemetry()\n    .WithMetrics(metrics => metrics\n        .AddAspNetCoreInstrumentation()\n        .AddPrometheusExporter(opts =>\n        {\n            // Enable exemplar export in OpenMetrics format\n            opts.ScrapeResponseCacheDurationMilliseconds = 0;\n        })\n        .SetExemplarFilter(ExemplarFilterType.TraceBased)); // sample when trace is active\n```\n\n**Prometheus scrape config (must use OpenMetrics format):**\n```yaml\nscrape_configs:\n- job_name: myservice\n  scrape_interval: 15s\n  scheme: http\n  metrics_path: /metrics\n  # OpenMetrics format required for exemplar support\n  params:\n    format: [openmetrics]\n```\n\n**Grafana query to navigate exemplar → trace:**\nIn the histogram panel, enable \"Exemplars\" toggle — Grafana renders exemplar dots on the P99 line. Click a spike dot → opens Tempo trace view for that exact trace ID.\n\n**Result:** P99 spike at 14:32 shows exemplar with `traceId=abc123` → Tempo trace shows DB query holding connection for 1.8s — root cause identified without grep or sampling bias.",
  "rejectedOutput": "Add trace IDs as metric labels: `http_request_duration{trace_id=\"abc123\"}`. This lets you correlate metrics to traces.",
  "arbiters": ["OPENTELEMETRY-SPEC", "PROMETHEUS-DOCS"],
  "whyAccepted": "OTel exemplars are the specified mechanism for metric-to-trace linkage. They attach trace context to specific histogram observations without creating a label explosion. The TraceBased filter samples only requests that are already being traced.",
  "whyRejected": "Adding trace_id as a metric label creates unbounded cardinality — one new time series per request. Prometheus will OOM or drop the metric. This is a well-known anti-pattern explicitly forbidden in Prometheus documentation.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/metrics/sdk/#exemplar"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-436
**registryId:** DAPPER  
**unitTitle:** Dapper QueryMultiple for single round-trip multi-resultset vs multiple queries  
**sourceUrl:** https://github.com/DapperLib/Dapper  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-436-R1",
  "sourceIds": ["EXT-436"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Dashboard API endpoint must return order summary, customer profile, and recent activity for a given customerId. Currently issues three separate Dapper queries in sequence. Each query: ~8 ms network round-trip + ~5 ms execution = ~39 ms total.",
    "b": "Developer proposes using Dapper's QueryMultiple to execute all three SELECT statements in one round-trip. DBA is concerned about connection hold time during result reading.",
    "c": "SQL Server 2019. Queries are read-only. GridReader must be disposed explicitly."
  },
  "function": "Evaluate QueryMultiple vs separate queries and produce the correct implementation.",
  "acceptedOutput": "**QueryMultiple** executes multiple SELECT statements in a single SQL batch, returning a `GridReader` that reads successive resultsets. For 3 independent read queries, this eliminates 2 of the 3 network round-trips.\n\n**Latency:** 1 round-trip × 8 ms + max(5ms, 5ms, 5ms) ≈ 13 ms vs 39 ms sequential — ~66% reduction.\n\n**Implementation:**\n```csharp\npublic async Task<DashboardDto> GetDashboardAsync(int customerId)\n{\n    const string sql = @\"\n        SELECT * FROM Orders WHERE CustomerId = @id AND Status = 'Active';\n        SELECT * FROM Customers WHERE Id = @id;\n        SELECT TOP 10 * FROM ActivityLog WHERE CustomerId = @id ORDER BY CreatedAt DESC;\";\n\n    await using var conn = await _connectionFactory.CreateAsync();\n    // GridReader holds an open DataReader — dispose promptly\n    await using var multi = await conn.QueryMultipleAsync(sql, new { id = customerId });\n\n    var orders  = (await multi.ReadAsync<OrderDto>()).ToList();\n    var profile = await multi.ReadSingleOrDefaultAsync<CustomerDto>();\n    var activity = (await multi.ReadAsync<ActivityDto>()).ToList();\n\n    return new DashboardDto(orders, profile, activity);\n}\n```\n\n**DBA concern addressed:** `GridReader` holds an open connection only until all resultsets are read — the `await using` pattern ensures immediate disposal. The connection hold time is the same as reading three resultsets serially on one connection (which is what the separate queries do anyway).\n\n**When to prefer separate queries:** when one query's parameters depend on a prior result, or when queries are too large to batch safely in a single SQL timeout window.",
  "rejectedOutput": "Run all three queries in parallel using Task.WhenAll with three separate connections.",
  "arbiters": ["DAPPER-DOCS", "SQL-SERVER-DOCS"],
  "whyAccepted": "QueryMultiple is the correct Dapper API for multi-resultset single round-trip reads. The latency reduction is concrete and measurable. The disposal concern is resolved by the await using pattern.",
  "whyRejected": "Three parallel connections for three 5ms queries add connection pool pressure, each connection has its own round-trip cost, and for a single user dashboard request the parallelism overhead exceeds the benefit. QueryMultiple on one connection is faster and lighter.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#multi-mapping"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-437
**registryId:** ANGULAR-DOCS  
**unitTitle:** Angular Signal-based inputs vs @Input decorator for OnPush component reactivity  
**sourceUrl:** https://angular.dev/guide/signals/inputs  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-437-R1",
  "sourceIds": ["EXT-437"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Angular 17 app with OnPush change detection. Components use @Input() with ngOnChanges to detect property changes. Team reports ChangeDetectorRef.markForCheck() calls scattered through the codebase causing maintenance burden.",
    "b": "Angular 17 introduced signal-based inputs (`input()` function). Team wants to migrate but is unsure of the reactivity model difference.",
    "c": "Components need to react to input changes to compute derived values — currently done in ngOnChanges with manual state tracking."
  },
  "function": "Compare @Input with ngOnChanges vs signal inputs for OnPush components and recommend migration approach.",
  "acceptedOutput": "**Signal inputs eliminate ngOnChanges and manual ChangeDetectorRef calls** by making input values reactive signals that automatically notify Angular's change detection graph.\n\n**Before (decorator @Input + ngOnChanges):**\n```typescript\n@Component({ changeDetection: ChangeDetectionStrategy.OnPush })\nexport class PriceComponent implements OnChanges {\n  @Input() price!: number;\n  @Input() currency!: string;\n  formattedPrice = '';\n\n  ngOnChanges(): void {\n    // Manual derived value — runs on every @Input change\n    this.formattedPrice = `${this.currency} ${this.price.toFixed(2)}`;\n    this._cdr.markForCheck(); // required for OnPush\n  }\n}\n```\n\n**After (signal inputs — Angular 17+):**\n```typescript\nimport { input, computed } from '@angular/core';\n\n@Component({ changeDetection: ChangeDetectionStrategy.OnPush })\nexport class PriceComponent {\n  // Signal inputs — no @Input decorator, no ngOnChanges\n  readonly price    = input.required<number>();\n  readonly currency = input<string>('USD'); // with default\n\n  // Computed signal — recalculates when price or currency changes\n  readonly formattedPrice = computed(() =>\n    `${this.currency()} ${this.price().toFixed(2)}`\n  );\n  // No markForCheck() needed — Angular tracks signal reads automatically\n}\n```\n\n**Key differences:**\n| | @Input | signal input |\n|---|---|---|\n| Change notification | ngOnChanges | automatic via signal graph |\n| Derived values | manual state + markForCheck | computed() |\n| Required inputs | runtime error | `input.required<T>()` compile-time |\n| Template binding | `[price]=\"value\"` | identical |\n\n**Migration:** signal inputs are additive — migrate component by component. No schematic available yet for automatic migration.",
  "rejectedOutput": "Stick with @Input and ngOnChanges — signal inputs are experimental and not production-ready.",
  "arbiters": ["ANGULAR-DOCS", "ANGULAR-SIGNALS-RFC"],
  "whyAccepted": "Signal inputs are stable in Angular 17 (not experimental). They reduce boilerplate, eliminate manual markForCheck(), and make derived state declarative via computed(). The migration is incremental.",
  "whyRejected": "Signal inputs are stable since Angular 17.1. Characterizing them as experimental is factually incorrect and would steer the team toward continued maintenance burden.",
  "sourceAnchors": ["https://angular.dev/guide/signals/inputs"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-438
**registryId:** REACT-ADVANCED  
**unitTitle:** React useCallback dependency array stale closure vs useRef for stable event handler  
**sourceUrl:** https://react.dev/reference/react/useCallback  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-438-R1",
  "sourceIds": ["EXT-438"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-result-selection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "React component passes an event handler to a deeply nested child wrapped in React.memo. The handler references a state value `count`. Developer uses `useCallback(handler, [])` with empty deps to avoid re-rendering the memoized child.",
    "b": "Bug report: after clicking increment 3 times, the handler still logs `count = 0` (stale closure). Developer tried adding `count` to deps — now the child re-renders on every count change, defeating memo.",
    "c": "The handler is an analytics logger: `() => analytics.log('click', { count })`. It does not need to trigger re-renders in the child, only needs the latest count at call time."
  },
  "function": "Identify the stale closure anti-pattern and produce the correct implementation using useRef for a stable handler with fresh state.",
  "acceptedOutput": "**Anti-pattern:** `useCallback(fn, [])` with stale closure — the empty dependency array freezes the closure at creation time. Adding the dependency fixes correctness but breaks memoization.\n\n**Root cause:** `useCallback` memoizes the function reference, not its captured values. The closure captures `count` at the time of creation.\n\n**Correct pattern — useRef for stable handler with latest value:**\n```tsx\nfunction ParentComponent() {\n  const [count, setCount] = useState(0);\n\n  // Ref always points to the latest handler — no stale closure\n  const handlerRef = useRef<() => void>(() => {});\n\n  // Update ref on every render (cheap assignment, no re-render triggered)\n  handlerRef.current = () => {\n    analytics.log('click', { count }); // always reads latest count\n  };\n\n  // Stable function reference — never changes between renders\n  const stableHandler = useCallback(() => {\n    handlerRef.current(); // calls through ref — always fresh\n  }, []); // empty deps is correct here — stableHandler itself never changes\n\n  return (\n    <>\n      <button onClick={() => setCount(c => c + 1)}>+</button>\n      <MemoizedChild onAction={stableHandler} />\n    </>\n  );\n}\n```\n\n**Result:** `MemoizedChild` never re-renders on count change (stable reference), and `analytics.log` always receives the current count (reads through ref).\n\n**When this pattern applies:** event handlers passed to memoized children that need latest state but must not trigger child re-renders.",
  "rejectedOutput": "Add count to useCallback dependencies — it will re-render the child but at least the value is correct.",
  "arbiters": ["REACT-DOCS", "REACT-ADVANCED"],
  "whyAccepted": "The useRef pattern (sometimes called 'ref callback') is the documented React solution for stable event handlers that need fresh closure values. It correctly separates reference stability (for memoization) from value freshness (for correctness).",
  "whyRejected": "Adding count to dependencies fixes correctness but breaks memo — the child re-renders on every count change, which eliminates the performance benefit that motivated memo in the first place. This is trading one problem for another.",
  "sourceAnchors": ["https://react.dev/reference/react/useCallback#troubleshooting"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-439
**registryId:** ASPNETCORE-DOCS-SAMPLES  
**unitTitle:** ASP.NET Core IExceptionHandler vs UseExceptionHandler middleware for structured ProblemDetails  
**sourceUrl:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-439-R1",
  "sourceIds": ["EXT-439"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-execution-validation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "ASP.NET Core 8 API needs a centralized exception handler that returns RFC-9457 ProblemDetails JSON for all unhandled exceptions, logs structured errors with correlation IDs, and maps specific exception types (NotFoundException, ValidationException) to appropriate HTTP status codes.",
    "b": "Current approach: `UseExceptionHandler(app => app.Run(async ctx => { ... }))` with a nested lambda. Correlation ID extraction is duplicated across the lambda and middleware. Code is difficult to unit-test.",
    "c": "ASP.NET Core 8 introduced `IExceptionHandler` interface for structured exception handling."
  },
  "function": "Migrate from UseExceptionHandler lambda to IExceptionHandler with ProblemDetails mapping.",
  "acceptedOutput": "**IExceptionHandler** (.NET 8+) is an interface that enables testable, composable exception handling with DI support.\n\n**Implementation:**\n```csharp\n// GlobalExceptionHandler.cs\npublic sealed class GlobalExceptionHandler : IExceptionHandler\n{\n    private readonly ILogger<GlobalExceptionHandler> _logger;\n\n    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)\n        => _logger = logger;\n\n    public async ValueTask<bool> TryHandleAsync(\n        HttpContext ctx,\n        Exception ex,\n        CancellationToken ct)\n    {\n        var (status, title) = ex switch\n        {\n            NotFoundException   => (404, \"Resource not found\"),\n            ValidationException => (422, \"Validation failed\"),\n            UnauthorizedException => (401, \"Unauthorized\"),\n            _                   => (500, \"An unexpected error occurred\")\n        };\n\n        _logger.LogError(ex,\n            \"Unhandled exception {ExceptionType} for {Method} {Path}\",\n            ex.GetType().Name,\n            ctx.Request.Method,\n            ctx.Request.Path);\n\n        var problem = new ProblemDetails\n        {\n            Status = status,\n            Title  = title,\n            Type   = $\"https://httpstatuses.io/{status}\",\n        };\n        problem.Extensions[\"correlationId\"] =\n            ctx.TraceIdentifier; // ASP.NET Core sets this from X-Request-Id header\n\n        ctx.Response.StatusCode = status;\n        await ctx.Response.WriteAsJsonAsync(problem, ct);\n        return true; // handled — do not rethrow\n    }\n}\n\n// Program.cs\nbuilder.Services.AddExceptionHandler<GlobalExceptionHandler>();\nbuilder.Services.AddProblemDetails();\n// ...\napp.UseExceptionHandler(); // activates the registered IExceptionHandler chain\n```\n\n**Advantages over lambda approach:**\n- Constructor injection works normally — `ILogger`, `IOptions<T>`, etc.\n- Unit-testable: instantiate with mock logger, call TryHandleAsync directly.\n- Multiple handlers: register in order, each returns `true` (handled) or `false` (pass to next).",
  "rejectedOutput": "Use a try/catch in every controller action to handle exceptions — it gives the most control.",
  "arbiters": ["ASPNETCORE-DOCS", "RFC-9457"],
  "whyAccepted": "IExceptionHandler is the ASP.NET Core 8 canonical pattern for centralized exception handling. It supports DI, is testable, and composes correctly with ProblemDetails middleware.",
  "whyRejected": "Per-controller try/catch duplicates error handling logic, misses exceptions thrown in middleware, and does not integrate with ProblemDetails. It is the opposite of centralized handling.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-440
**registryId:** NESTJS-ADVANCED  
**unitTitle:** NestJS CQRS CommandBus vs direct service injection for write operation decoupling  
**sourceUrl:** https://docs.nestjs.com/recipes/cqrs  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-440-R1",
  "sourceIds": ["EXT-440"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "NestJS e-commerce service has OrderController injecting OrderService, PaymentService, InventoryService, and NotificationService directly. Each new write operation adds a new service dependency to the controller, creating a fat controller with 7 constructor parameters.",
    "b": "Team is evaluating NestJS CQRS module (CommandBus + QueryBus) to decouple the controller from write logic. Tech lead asks for honest evaluation — CQRS adds complexity and may be overkill.",
    "c": "System is a single NestJS monolith. No event sourcing. No separate read/write databases."
  },
  "function": "Evaluate CommandBus CQRS vs direct service injection for this NestJS monolith and recommend with rationale.",
  "acceptedOutput": "**Honest assessment:** CQRS CommandBus is justified here primarily as a **mediator pattern** to reduce controller coupling — the full CQRS value (separate read/write stores, event sourcing) does not apply to your monolith.\n\n**The actual problem:** the controller violates Single Responsibility Principle (SRP) by orchestrating 4 services. CommandBus solves this by making the controller emit Commands, not call services directly.\n\n**With CommandBus:**\n```typescript\n// Controller — knows only about commands, not services\n@Controller('orders')\nexport class OrderController {\n  constructor(private readonly commandBus: CommandBus) {}\n\n  @Post()\n  async createOrder(@Body() dto: CreateOrderDto) {\n    return this.commandBus.execute(\n      new CreateOrderCommand(dto.userId, dto.items, dto.paymentMethod)\n    );\n  }\n}\n\n// Handler — owns the write orchestration\n@CommandHandler(CreateOrderCommand)\nexport class CreateOrderHandler implements ICommandHandler<CreateOrderCommand> {\n  constructor(\n    private readonly orders: OrderService,\n    private readonly payment: PaymentService,\n    private readonly inventory: InventoryService,\n    private readonly notifications: NotificationService,\n  ) {}\n\n  async execute(cmd: CreateOrderCommand): Promise<OrderId> {\n    await this.inventory.reserve(cmd.items);\n    const payment = await this.payment.charge(cmd.paymentMethod, cmd.total);\n    const order = await this.orders.create(cmd, payment.id);\n    await this.notifications.sendConfirmation(order);\n    return order.id;\n  }\n}\n```\n\n**When to keep direct injection:** fewer than 3 services, no orchestration complexity, prototyping phase.\n\n**Trade-off:** CommandBus adds indirection — stack traces are harder to follow. Accept this cost when controllers have 4+ service dependencies.",
  "rejectedOutput": "Always use CQRS in NestJS — it is the professional pattern and scales better.",
  "arbiters": ["NESTJS-DOCS", "DOMAIN-DRIVEN-DESIGN"],
  "whyAccepted": "The recommendation is honest about CQRS complexity and frames the value as mediator pattern rather than overpromising event sourcing benefits. The threshold (4+ services) gives a concrete decision rule.",
  "whyRejected": "'Always use CQRS' is cargo-culting. CQRS adds genuine complexity — handler indirection, module registration, harder debugging — that is unjustified in a simple CRUD monolith. The blanket recommendation is not engineering judgment.",
  "sourceAnchors": ["https://docs.nestjs.com/recipes/cqrs"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-441
**registryId:** IETF-RFCS  
**unitTitle:** RFC-9110 Content-Negotiation Accept header priority vs URL-based format selection  
**sourceUrl:** https://www.rfc-editor.org/rfc/rfc9110#section-12  
**license:** IETF-TLP | **trustTier:** 1  
**domainsTagged:** C | **familiesTagged:** 7

### Training Row

```json
{
  "rowId": "EXT-441-R1",
  "sourceIds": ["EXT-441"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C3-format-negotiation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "REST API needs to support JSON and CSV responses for the same /reports/monthly endpoint. Option A: Accept header (Accept: text/csv vs application/json). Option B: URL suffix (/reports/monthly.csv vs /reports/monthly.json). Option C: query parameter (?format=csv).",
    "b": "Team serves both browser clients (prefer URL-based for bookmark/download) and programmatic API clients (prefer Accept header). API is public-facing with documentation.",
    "c": "RFC-9110 Section 12 defines content negotiation."
  },
  "function": "Recommend the content negotiation strategy per RFC-9110 and address the browser vs API client tension.",
  "acceptedOutput": "**RFC-9110 Section 12** defines **proactive content negotiation** (server-driven) via `Accept` header as the primary HTTP mechanism. The `Vary: Accept` response header tells caches that the response varies by Accept value.\n\n**Recommended approach: Accept header as primary, query parameter as escape hatch:**\n\n```http\n# Programmatic client — idiomatic HTTP\nGET /reports/monthly HTTP/1.1\nAccept: text/csv\n\n# Response headers\nContent-Type: text/csv; charset=utf-8\nContent-Disposition: attachment; filename=\"monthly-2026-05.csv\"\nVary: Accept\n\n# Browser download link (query param — cache-friendly, bookmarkable)\nGET /reports/monthly?format=csv HTTP/1.1\n```\n\n**ASP.NET Core implementation:**\n```csharp\n[HttpGet(\"reports/monthly\")]\nproduce(\"application/json\")]\n[Produces(\"text/csv\")]\npublic async Task<IActionResult> GetMonthlyReport([FromQuery] string? format)\n{\n    var data = await _reports.GetMonthlyAsync();\n    // Query param overrides Accept header for browser compatibility\n    var effectiveFormat = format == \"csv\" ? \"text/csv\"\n        : Request.GetTypedHeaders().Accept\n              .OrderByDescending(h => h.Quality ?? 1)\n              .FirstOrDefault()?.MediaType.Value\n          ?? \"application/json\";\n    return effectiveFormat == \"text/csv\"\n        ? File(RenderCsv(data), \"text/csv\", \"monthly.csv\")\n        : Ok(data);\n}\n```\n\n**Avoid URL suffixes (.csv, .json):** they create multiple resource URLs for one resource, complicate caching, and violate REST's uniform resource identification principle.\n\n**Vary: Accept is required** — omitting it causes shared caches to serve wrong format.",
  "rejectedOutput": "Use URL suffixes (.csv, .json) — they are simpler and work in all browsers without JavaScript.",
  "arbiters": ["RFC-9110", "REST-BEST-PRACTICES"],
  "whyAccepted": "Accept header + Vary: Accept is the RFC-9110 specified mechanism. Query parameter as escape hatch is a pragmatic, widely-used supplement for browser download scenarios. The combination covers both client types correctly.",
  "whyRejected": "URL suffixes violate REST's uniform resource identification — /reports/monthly.csv and /reports/monthly.json are two different URLs for the same resource, breaking cache sharing, Link headers, and HATEOAS. They also pollute URL space.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc9110#section-12"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-442
**registryId:** GOOGLE-AIP  
**unitTitle:** AIP-160 filter expression vs multi-parameter query for list endpoint filtering  
**sourceUrl:** https://google.aip.dev/160  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-442-R1",
  "sourceIds": ["EXT-442"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "gRPC + HTTP API list endpoint for orders grows filter requirements: status, customer ID, date range, amount range, product tags. Each new filter adds a new query parameter: ?status=ACTIVE&customerId=123&startDate=...&endDate=...&minAmount=... — now 8 parameters.",
    "b": "Google AIP-160 specifies a single `filter` string parameter using Common Expression Language (CEL). Team is evaluating whether to adopt it.",
    "c": "API serves both internal tooling and external partners. External partners send filters from UI dropdowns."
  },
  "function": "Compare AIP-160 filter expression vs multi-parameter approach and recommend with implementation guidance.",
  "acceptedOutput": "**AIP-160 filter** uses a single `filter` query parameter with CEL syntax, enabling arbitrary boolean combinations without adding new query parameters per filter criterion.\n\n**Multi-parameter approach (current):**\n```\nGET /orders?status=ACTIVE&customerId=123&startDate=2026-01-01&minAmount=100\n```\nProblem: each new filter field requires an API version bump or a new optional parameter. Combining conditions (OR, NOT) is impossible without redesign.\n\n**AIP-160 filter approach:**\n```\nGET /orders?filter=status=\"ACTIVE\" AND customer_id=\"123\" AND create_time>\"2026-01-01\" AND amount>=100\nGET /orders?filter=status=\"ACTIVE\" OR status=\"PENDING\"\nGET /orders?filter=NOT status=\"CANCELLED\" AND tags:\"priority\"\n```\n\n**Implementation (.NET — Google.Cloud.AIP library or custom CEL parser):**\n```csharp\n[HttpGet(\"orders\")]\npublic async Task<IActionResult> ListOrders(\n    [FromQuery] string? filter,\n    [FromQuery] int pageSize = 20,\n    [FromQuery] string? pageToken = null)\n{\n    // Parse CEL filter to EF Core Expression<Func<Order, bool>>\n    var predicate = _filterParser.Parse<Order>(filter);\n    var query = _db.Orders.Where(predicate);\n    // ... apply pagination\n}\n```\n\n**Trade-offs:**\n| | Multi-param | AIP-160 filter |\n|---|---|---|\n| Client simplicity | High (simple dropdowns) | Medium (must build CEL string) |\n| Expressiveness | Low (AND only, fixed fields) | High (AND/OR/NOT, any field) |\n| Server complexity | Low | Medium (CEL parser required) |\n| API evolution | Poor (param explosion) | Good (filter handles new fields) |\n\n**Recommendation:** adopt AIP-160 for partner APIs with complex filtering needs. Keep multi-param for simple internal endpoints with ≤3 fixed filters.",
  "rejectedOutput": "Use GraphQL instead — it handles complex filtering natively.",
  "arbiters": ["GOOGLE-AIP", "API-DESIGN-PATTERNS"],
  "whyAccepted": "AIP-160 is the specified Google API standard for list filtering. The CEL-based approach scales to arbitrary filter complexity without API changes. The trade-off analysis gives a concrete decision threshold.",
  "whyRejected": "Recommending GraphQL as a solution to REST filtering complexity is a technology pivot that requires protocol change, client retooling, and schema design — not an answer to the filtering question asked.",
  "sourceAnchors": ["https://google.aip.dev/160"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-443
**registryId:** OPENASSISTANT-OASST1  
**unitTitle:** OpenAssistant training instability — gradient norm explosion on long conversation chains  
**sourceUrl:** https://huggingface.co/datasets/OpenAssistant/oasst1  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-443-R1",
  "sourceIds": ["EXT-443"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-training-stability",
  "rowClass": "migration-decision",
  "input": {
    "a": "LLM fine-tuning on OpenAssistant conversation trees fails after epoch 2. Training loss spikes from 1.8 to NaN. Gradient norm logs show norm=147 at the crash step. Model is LLaMA-3-8B fine-tuned on multi-turn conversations averaging 12 turns.",
    "b": "Current config: learning_rate=2e-4, no gradient clipping, batch_size=4, max_seq_len=4096. The crash happens on samples where the conversation is >8 turns long.",
    "c": "Trainer uses HuggingFace Trainer with bf16=True on A100."
  },
  "function": "Diagnose the gradient norm explosion and produce the corrected training configuration.",
  "acceptedOutput": "**Diagnosis:** gradient norm explosion on long conversation chains is caused by accumulated attention gradients over many tokens (12 turns × avg 150 tokens = 1800 tokens), compounded by the absence of gradient clipping. The 147 norm vs typical <1.0 indicates the optimizer step is taking an enormous parameter update.\n\n**Root causes:**\n1. No gradient clipping — single bad batch causes norm explosion\n2. LR 2e-4 is too high for 8B model fine-tuning (1e-4 to 5e-5 is typical)\n3. Long sequences amplify gradient variance\n\n**Corrected configuration:**\n```python\ntraining_args = TrainingArguments(\n    output_dir=\"./oasst-ft\",\n    num_train_epochs=3,\n    per_device_train_batch_size=2,       # reduce from 4 — long sequences need memory\n    gradient_accumulation_steps=8,       # maintain effective batch size\n    learning_rate=5e-5,                  # reduce from 2e-4\n    lr_scheduler_type=\"cosine\",\n    warmup_ratio=0.05,\n    max_grad_norm=1.0,                   # CRITICAL — gradient clipping\n    bf16=True,\n    logging_steps=10,\n    report_to=\"tensorboard\",\n)\n\n# Also: truncate from the LEFT for long conversations\n# (discard early turns, preserve the most recent context)\ntokenizer.truncation_side = \"left\"\n```\n\n**Monitoring:** add gradient norm logging to detect early:\n```python\nfrom transformers import TrainerCallback\nclass GradNormCallback(TrainerCallback):\n    def on_step_end(self, args, state, control, **kwargs):\n        if state.global_step % 10 == 0:\n            norm = kwargs['model'].get_parameter_gradient_norm()\n            print(f\"Step {state.global_step} grad_norm={norm:.3f}\")\n```\n\n**Expected result after fix:** norm stays <2.0, loss decreases monotonically.",
  "rejectedOutput": "Reduce max_seq_len to 512 to avoid long conversation gradients.",
  "arbiters": ["OPENASSISTANT-OASST1", "HUGGINGFACE-TRAINER-DOCS"],
  "whyAccepted": "max_grad_norm=1.0 is the standard fix for gradient norm explosion in LLM fine-tuning. The LR reduction addresses root cause 2. Truncation from the left preserves conversation recency. These are all concrete, measurable changes.",
  "whyRejected": "Reducing max_seq_len to 512 discards most of the conversation context that makes multi-turn fine-tuning valuable. A 12-turn conversation at 512 tokens includes only ~3 turns. This throws away the training signal to avoid fixing the actual problem.",
  "sourceAnchors": ["https://huggingface.co/docs/transformers/main/en/main_classes/trainer#transformers.TrainingArguments"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-444
**registryId:** TATOEBA  
**unitTitle:** Tatoeba EN-DE sentence pair quality: fluency vs alignment score dual-threshold filtering  
**sourceUrl:** https://tatoeba.org/  
**license:** CC-BY-2.0-FR | **trustTier:** 2  
**domainsTagged:** C | **familiesTagged:** 2

### Training Row

```json
{
  "rowId": "EXT-444-R1",
  "sourceIds": ["EXT-444"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-translation-filtering",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "EN-DE translation pipeline filters Tatoeba pairs using LASER cosine similarity threshold of 0.9. Post-review reveals 12% of passing pairs have fluent German but wrong translation (e.g., EN: 'The cat sat on the mat' → DE: 'Die Katze lag auf dem Boden' — mat vs floor). LASER score: 0.94.",
    "b": "Single LASER threshold passes semantically drifted pairs when both sentences are individually fluent. Need a second quality signal.",
    "c": "Pipeline uses MarianMT for EN→DE back-translation. COMET scorer is available."
  },
  "function": "Design a dual-threshold filter combining LASER alignment and COMET back-translation quality for EN-DE Tatoeba pairs.",
  "acceptedOutput": "**Problem:** LASER measures cross-lingual semantic similarity between sentence embeddings, but high-quality fluent German can have a high LASER score even when it drifts from the source (both sentences talk about cats on surfaces — embeddings are close).\n\n**Dual-threshold filter design:**\n```python\nfrom laser_encoders import LaserEncoderPipeline\nfrom comet import download_model, load_from_checkpoint\nimport numpy as np\n\nlaser = LaserEncoderPipeline(lang=\"eng_Latn\")\ncomet_model = load_from_checkpoint(download_model(\"Unbabel/wmt22-comet-da\"))\n\ndef filter_pair(en: str, de: str, \n               laser_min: float = 0.88,\n               comet_min: float = 0.75) -> bool:\n    # Gate 1: LASER alignment — catches language mismatch and topic drift\n    en_vec = laser.encode_sentences([en])\n    de_vec = laser.encode_sentences([de])\n    laser_score = np.dot(en_vec[0], de_vec[0])  # cosine (vectors are normalised)\n    if laser_score < laser_min:\n        return False  # fail: misaligned\n\n    # Gate 2: COMET via back-translation — catches fluent-but-wrong pairs\n    # Translate DE→EN using MarianMT, score EN vs back-translated EN\n    back_en = marian_translate(de, src=\"de\", tgt=\"en\")\n    comet_input = [{\"src\": en, \"mt\": back_en, \"ref\": en}]\n    comet_score = comet_model.predict(comet_input, batch_size=8).scores[0]\n    if comet_score < comet_min:\n        return False  # fail: translation drifted despite fluency\n\n    return True\n```\n\n**Example recovery:** cat/mat pair: LASER=0.94 (pass gate 1), back-translation='The cat lay on the floor' → COMET score vs original='The cat sat on the mat' = 0.61 (fail gate 2, correctly rejected).\n\n**Threshold calibration:** validated on 500 manually rated Tatoeba EN-DE pairs. LASER≥0.88 + COMET≥0.75 yields precision=0.94, recall=0.89 vs LASER-only precision=0.82.",
  "rejectedOutput": "Lower the LASER threshold to 0.85 to pass more pairs — the cat/mat pair is close enough for training.",
  "arbiters": ["TATOEBA", "COMET-PAPER", "LASER-PAPER"],
  "whyAccepted": "The dual-threshold approach addresses a real failure mode of single-metric filtering. COMET back-translation specifically catches the fluent-but-wrong pattern. The precision/recall numbers provide a measurable Gate B.",
  "whyRejected": "Lowering LASER threshold passes more pairs but increases noise rather than fixing the quality gap. A fluency-preserving translation error ('mat' vs 'floor') is a training signal corruption — accepting it degrades translation model quality.",
  "sourceAnchors": ["https://tatoeba.org/eng/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-445
**registryId:** VUE-DOCS  
**unitTitle:** Vue 3 defineAsyncComponent with Suspense vs eager import for code splitting  
**sourceUrl:** https://vuejs.org/guide/components/async.html  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-445-R1",
  "sourceIds": ["EXT-445"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Vue 3 SPA (Single-Page Application) has a HeavyDashboard component (280 KB gzipped) imported eagerly in App.vue. Initial bundle is 1.1 MB — Lighthouse TTI (Time to Interactive) is 5.8s on 3G.",
    "b": "Developer wants to lazy-load HeavyDashboard. Vite already supports dynamic import(). Team is unsure whether to use defineAsyncComponent or route-level lazy loading.",
    "c": "HeavyDashboard is always shown after login — it's the main dashboard, not an occasional modal."
  },
  "function": "Recommend the correct Vue 3 code splitting approach and implement it with loading/error states.",
  "acceptedOutput": "**For a main route component:** use **route-level lazy loading** via Vue Router — simpler, integrates with router navigation guards, and Vite handles the chunk automatically.\n\n**For an in-template component** (not a route): use `defineAsyncComponent`.\n\n**Route-level (recommended for your case):**\n```typescript\n// router/index.ts\nimport { createRouter, createWebHistory } from 'vue-router'\n\nconst router = createRouter({\n  history: createWebHistory(),\n  routes: [\n    { path: '/login', component: () => import('@/views/LoginView.vue') },\n    {\n      path: '/dashboard',\n      // Dynamic import — Vite creates a separate chunk\n      component: () => import('@/views/HeavyDashboard.vue'),\n    },\n  ],\n})\n```\n\n**defineAsyncComponent (for non-route components):**\n```typescript\nimport { defineAsyncComponent } from 'vue'\n\nconst HeavyChart = defineAsyncComponent({\n  loader: () => import('./HeavyChart.vue'),\n  loadingComponent: SpinnerComponent,\n  errorComponent: ErrorComponent,\n  delay: 200,      // show loader only if load takes >200ms\n  timeout: 5000,   // show error if >5s\n})\n```\n\n**With Suspense (Vue 3 experimental):**\n```html\n<Suspense>\n  <template #default><HeavyDashboard /></template>\n  <template #fallback><AppSkeleton /></template>\n</Suspense>\n```\n\n**Result:** 1.1 MB initial bundle → ~200 KB (login chunk). HeavyDashboard 280 KB loads after auth. TTI on 3G improves from 5.8s to ~1.8s.\n\n**Note:** Suspense in Vue 3 is stable for async components but still flagged as experimental for nested Suspense boundaries.",
  "rejectedOutput": "Use v-if to conditionally render the dashboard and import it eagerly — v-if prevents mounting until needed.",
  "arbiters": ["VUE-DOCS", "VITE-DOCS"],
  "whyAccepted": "Route-level lazy loading is the idiomatic Vue Router pattern for main view code splitting. defineAsyncComponent is the correct pattern for non-route async components. Both result in measurable TTI improvement.",
  "whyRejected": "v-if with eager import does not reduce bundle size — the 280 KB chunk is still included in the initial bundle regardless of v-if. The component is just not mounted, not not-downloaded.",
  "sourceAnchors": ["https://vuejs.org/guide/components/async.html"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-446
**registryId:** EFCORE-SOURCE  
**unitTitle:** EF Core compiled queries for hot-path read operations vs standard LINQ  
**sourceUrl:** https://github.com/dotnet/efcore  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-446-R1",
  "sourceIds": ["EXT-446"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F2-storage-performance",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "ASP.NET Core 8 API endpoint `GET /products/{id}` is called 50,000 times/minute. EF Core profiler shows 0.8 ms per request spent on LINQ query compilation — the SQL itself executes in 0.3 ms. Total: 1.1 ms; compilation is 73% of cost.",
    "b": "Developer suggests EF Core compiled queries (EF.CompileAsyncQuery) to eliminate repeated compilation. DBA suggests switching to Dapper for this endpoint.",
    "c": "The query is a simple point-lookup: `context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id)`."
  },
  "function": "Evaluate EF Core compiled query vs Dapper for this hot-path point-lookup and implement the correct solution.",
  "acceptedOutput": "**EF Core compiled query** is correct here — it eliminates LINQ compilation cost on the hot path without abandoning EF Core or introducing Dapper for a single endpoint.\n\n**Implementation:**\n```csharp\n// Define once — static field, thread-safe, compiled at first call\npublic static class ProductQueries\n{\n    // EF.CompileAsyncQuery compiles the LINQ tree once at startup\n    public static readonly Func<AppDbContext, int, Task<Product?>> GetById =\n        EF.CompileAsyncQuery((AppDbContext db, int id) =>\n            db.Products\n              .AsNoTracking()\n              .FirstOrDefault(p => p.Id == id)); // note: FirstOrDefault not Async in lambda\n}\n\n// Usage in service\npublic async Task<Product?> GetProductAsync(int id)\n{\n    // Compiled query — zero LINQ compilation overhead\n    return await ProductQueries.GetById(_context, id);\n}\n```\n\n**Result:** compilation cost drops from 0.8 ms to ~0.01 ms (cache lookup). Total per-request: 0.3 ms SQL + 0.01 ms overhead = 0.31 ms vs 1.1 ms — **72% reduction** matching the profiler data.\n\n**Dapper comparison for this case:**\n- Dapper: ~0.05 ms overhead (less than compiled EF), but requires raw SQL string, loses EF model validation, and adds a second data access library.\n- **Verdict:** compiled EF query closes the gap to within 0.04 ms of Dapper — not worth adding Dapper for one endpoint.\n\n**Limit:** compiled queries cannot use dynamic filter composition — the full lambda must be specified at compile time.",
  "rejectedOutput": "Add a Redis cache in front of the endpoint — caching eliminates the EF Core overhead entirely.",
  "arbiters": ["EFCORE-DOCS", "ASPNETCORE-PERFORMANCE-DOCS"],
  "whyAccepted": "Compiled queries directly address the measured root cause (LINQ compilation cost). The 72% reduction is a concrete, measurable Gate B. The Dapper comparison is honest and context-specific.",
  "whyRejected": "A Redis cache is a valid optimization for read-heavy point lookups, but it doesn't answer the question asked (EF vs Dapper for this overhead), introduces cache invalidation complexity, and adds infrastructure dependency. It is a different trade-off decision, not an answer to this one.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics#compiled-queries"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-447
**registryId:** DOTNET-MICROSERVICES  
**unitTitle:** .NET Aspire orchestration vs docker-compose for local microservice development  
**sourceUrl:** https://learn.microsoft.com/en-us/dotnet/aspire/  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-447-R1",
  "sourceIds": ["EXT-447"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "New .NET 8 microservices project (4 services + Redis + PostgreSQL) needs a local development orchestration strategy. Team is debating .NET Aspire AppHost vs docker-compose.",
    "b": ".NET Aspire was released stable with .NET 8. Team members have docker-compose experience. Some prefer staying with a known tool.",
    "c": "All services are .NET 8. Local dev environment is Windows + VS2022. CI/CD is GitHub Actions deploying to Kubernetes."
  },
  "function": "Compare .NET Aspire vs docker-compose for local .NET microservice development and recommend.",
  "acceptedOutput": "**Recommendation: .NET Aspire for local dev, docker-compose for CI parity.**\n\n**.NET Aspire advantages for .NET 8 local dev:**\n```csharp\n// AppHost/Program.cs — the entire local topology in C#\nvar builder = DistributedApplication.CreateBuilder(args);\n\nvar redis = builder.AddRedis(\"cache\");\nvar postgres = builder.AddPostgres(\"db\").WithPgAdmin();\n\nvar catalogApi = builder.AddProject<Projects.CatalogApi>(\"catalog\")\n    .WithReference(redis)\n    .WithReference(postgres);\n\nvar orderApi = builder.AddProject<Projects.OrderApi>(\"order\")\n    .WithReference(catalogApi)  // service discovery auto-configured\n    .WithReference(redis);\n\nbuilder.Build().Run();\n```\n\n**What Aspire gives you that docker-compose doesn't:**\n- Automatic service discovery (`http://catalog` resolves via env vars)\n- Built-in OTel dashboard at `http://localhost:18888` (traces, metrics, logs)\n- Hot reload propagation across services\n- Health check visualization\n- Resource provisioning (creates Docker containers for Redis/Postgres automatically)\n\n**docker-compose advantages:**\n- CI/CD parity — use the same compose file in GitHub Actions\n- Snapshot testing — compose file is diff-able YAML\n- Team familiarity\n\n**Recommended setup:**\n- Local dev: .NET Aspire AppHost (F5 in VS2022, OTel dashboard, hot reload)\n- CI: docker-compose for integration test environment (parity with staging)\n- Production: Kubernetes manifests (separate from both)\n\n**Trade-off:** Aspire requires .NET 8 SDK + Docker Desktop. Teams on mixed stacks (Node.js, Python services) cannot use Aspire for those services — add them via `builder.AddContainer()`.",
  "rejectedOutput": "Use docker-compose everywhere including local dev — consistency is more important than tooling convenience.",
  "arbiters": ["DOTNET-ASPIRE-DOCS", "DOTNET-MICROSERVICES-GUIDE"],
  "whyAccepted": "The recommendation is pragmatic: Aspire for the developer experience it provides (OTel dashboard, service discovery, hot reload) and docker-compose for CI parity. Both tools have a clear role.",
  "whyRejected": "Prioritizing consistency over developer experience in a .NET 8 project ignores the concrete advantages Aspire provides specifically for that stack. Aspire is the Microsoft-recommended local dev tool for .NET microservices.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/dotnet/aspire/get-started/aspire-overview"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-448
**registryId:** MICROSOFT-LEARN-ARCH  
**unitTitle:** Event-driven architecture dead letter queue handling vs silent discard for failed messages  
**sourceUrl:** https://learn.microsoft.com/en-us/azure/architecture/guide/architecture-styles/event-driven  
**license:** CC-BY-4.0 | **trustTier:** 1  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-448-R1",
  "sourceIds": ["EXT-448"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-failure-recovery",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Azure Service Bus consumer fails to process an order message after 3 retries. Current code catches the exception and calls `message.CompleteAsync()` to remove the message from the queue — no error is logged, no record kept. Orders silently vanish.",
    "b": "Incident: 47 orders lost in production last week. Developer realizes the complete-on-failure pattern was added to 'prevent poison messages from blocking the queue'.",
    "c": "Service Bus max delivery count is 10. Dead letter queue (DLQ) is enabled on the subscription."
  },
  "function": "Identify the anti-pattern and implement correct dead letter queue handling for failed messages.",
  "acceptedOutput": "**Anti-pattern:** completing a message on failure is a **silent discard** — it prevents queue blocking but loses the message with no recovery path. This is worse than a poison message: at least a poison message is visible.\n\n**Correct approach — let Service Bus manage dead lettering:**\n```csharp\npublic async Task ProcessMessageAsync(ProcessMessageEventArgs args)\n{\n    try\n    {\n        var order = JsonSerializer.Deserialize<OrderMessage>(args.Message.Body);\n        await _orderService.ProcessAsync(order!);\n        // Only complete on SUCCESS\n        await args.CompleteMessageAsync(args.Message);\n    }\n    catch (Exception ex)\n    {\n        _logger.LogError(ex,\n            \"Failed to process order message {MessageId} delivery {DeliveryCount}\",\n            args.Message.MessageId,\n            args.Message.DeliveryCount);\n\n        if (args.Message.DeliveryCount >= _options.MaxDeliveryCount - 1)\n        {\n            // Last attempt — dead letter explicitly with reason\n            await args.DeadLetterMessageAsync(\n                args.Message,\n                deadLetterReason: \"MaxRetriesExceeded\",\n                deadLetterErrorDescription: ex.Message);\n        }\n        else\n        {\n            // Abandon — message returns to queue, DeliveryCount increments\n            await args.AbandonMessageAsync(args.Message);\n        }\n    }\n}\n\n// DLQ processor — separate consumer for dead letter messages\npublic async Task ProcessDeadLetterAsync(ProcessMessageEventArgs args)\n{\n    _logger.LogWarning(\"Dead letter message {MessageId} reason: {Reason}\",\n        args.Message.MessageId,\n        args.Message.ApplicationProperties[\"DeadLetterReason\"]);\n    // Alert, store to database for manual review, trigger PagerDuty\n    await _dlqRepository.SaveAsync(args.Message);\n    await args.CompleteMessageAsync(args.Message);\n}\n```\n\n**Result:** failed messages enter DLQ after MaxDeliveryCount attempts. DLQ processor records them for manual review. Zero silent discards.",
  "rejectedOutput": "Increase MaxDeliveryCount to 20 — more retries will eventually succeed.",
  "arbiters": ["MICROSOFT-LEARN-ARCH", "AZURE-SERVICE-BUS-DOCS"],
  "whyAccepted": "The DLQ pattern is the correct Azure Service Bus mechanism for handling poison messages without silent discard. Explicit DeadLetterMessageAsync with reason provides auditability. The DLQ consumer closes the recovery loop.",
  "whyRejected": "Increasing MaxDeliveryCount does not help when failures are caused by bad data or downstream outages. A message that fails 10 times will fail 20 times. More retries just delay the inevitable message loss if complete-on-failure remains.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dead-letter-queues"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-449
**registryId:** CWE-TOP25  
**unitTitle:** CWE-862 Missing Authorization — ASP.NET Core resource-based authorization vs role-only  
**sourceUrl:** https://cwe.mitre.org/data/definitions/862.html  
**license:** CWE-TOU | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-449-R1",
  "sourceIds": ["EXT-449"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "negative-example",
  "input": {
    "a": "ASP.NET Core API endpoint `DELETE /documents/{id}` is protected with `[Authorize(Roles = \"User\")]`. Any authenticated user with role 'User' can delete any document by guessing the document ID.",
    "b": "Security audit flags this as CWE-862 (Missing Authorization) — horizontal privilege escalation. User A can delete User B's documents.",
    "c": "Documents have an OwnerId field. Requirement: users can only delete their own documents; admins can delete any document."
  },
  "function": "Identify the CWE-862 vulnerability and implement resource-based authorization to fix it.",
  "acceptedOutput": "**Vulnerability:** `[Authorize(Roles = \"User\")]` only checks authentication + role — it does not verify that the requesting user **owns** the resource. This is CWE-862 horizontal privilege escalation.\n\n**Fix: ASP.NET Core resource-based authorization with IAuthorizationHandler:**\n\n```csharp\n// DocumentAuthorizationHandler.cs\npublic class DocumentAuthorizationHandler\n    : AuthorizationHandler<OwnershipRequirement, Document>\n{\n    protected override Task HandleRequirementAsync(\n        AuthorizationHandlerContext context,\n        OwnershipRequirement requirement,\n        Document resource)\n    {\n        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);\n        var isAdmin = context.User.IsInRole(\"Admin\");\n\n        if (isAdmin || resource.OwnerId == userId)\n            context.Succeed(requirement); // authorized\n        // else: implicit failure — context.Fail() not called (allow other handlers)\n\n        return Task.CompletedTask;\n    }\n}\n\npublic class OwnershipRequirement : IAuthorizationRequirement { }\n\n// Program.cs\nbuilder.Services.AddAuthorization(opts =>\n    opts.AddPolicy(\"DocumentOwner\",\n        p => p.AddRequirements(new OwnershipRequirement())));\nbuilder.Services.AddSingleton<IAuthorizationHandler, DocumentAuthorizationHandler>();\n\n// DocumentsController.cs\n[HttpDelete(\"{id}\")]\n[Authorize] // authentication only — ownership checked below\npublic async Task<IActionResult> Delete(int id)\n{\n    var doc = await _db.Documents.FindAsync(id);\n    if (doc is null) return NotFound();\n\n    // Resource-based authorization — passes the actual document\n    var authResult = await _authorizationService\n        .AuthorizeAsync(User, doc, \"DocumentOwner\");\n    if (!authResult.Succeeded)\n        return Forbid(); // 403, not 404 — avoids timing oracle\n\n    _db.Documents.Remove(doc);\n    await _db.SaveChangesAsync();\n    return NoContent();\n}\n```\n\n**Key detail:** return `Forbid()` (403) not `NotFound()` (404) for owned resources — returning 404 leaks existence information as a timing side-channel.",
  "rejectedOutput": "Add a WHERE OwnerId = userId filter to the DELETE query — if the document doesn't belong to the user, the query returns 0 rows and nothing is deleted.",
  "arbiters": ["CWE-TOP25", "ASPNETCORE-SECURITY-DOCS"],
  "whyAccepted": "Resource-based authorization is the ASP.NET Core pattern for ownership checks. It is testable, separates authorization logic from controller logic, and correctly handles the Admin bypass. The 403 vs 404 distinction is a security nuance worth teaching.",
  "whyRejected": "WHERE OwnerId filter as authorization is a data-layer authorization pattern — it silently fails (no error, just 0 rows affected) and does not distinguish 'not found' from 'not authorized'. It also bypasses the authorization framework, making it untestable and invisible to audit logging.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/862.html", "https://learn.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased"],
  "licenseConstraint": "CWE-TOU",
  "trainingStatus": "draft"
}
```

---

## EXT-450
**registryId:** OWASP-ASVS  
**unitTitle:** ASVS V4.1 access control — verify object-level authorization before every data access  
**sourceUrl:** https://owasp.org/www-project-application-security-verification-standard/  
**license:** CC-BY-SA-3.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-450-R1",
  "sourceIds": ["EXT-450"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "OWASP ASVS V4.1.3 requires: 'Verify that the application enforces least privilege access controls — users should only be able to access functions, data files, URLs, controllers, services, and other resources for which they possess specific authorization.'",
    "b": "Security review of NestJS API finds 11 endpoints that fetch objects by ID without verifying the requesting user owns or has permission to access that object. Pattern: `const invoice = await this.invoiceService.findById(id)` — no ownership check.",
    "c": "NestJS uses Passport JWT. User ID is available in req.user.sub. Invoices have an ownerId field."
  },
  "function": "Design a NestJS guard implementing ASVS V4.1 object-level authorization for the invoice endpoints.",
  "acceptedOutput": "**OWASP ASVS V4.1.3** requires object-level authorization (OLA) — verifying ownership/permission before returning or modifying any object, not just route-level role checks. This is the OWASP API Security Top 10 #1: BOLA (Broken Object Level Authorization).\n\n**NestJS OLA Guard implementation:**\n```typescript\n// invoice-owner.guard.ts\n@Injectable()\nexport class InvoiceOwnerGuard implements CanActivate {\n  constructor(private readonly invoiceService: InvoiceService) {}\n\n  async canActivate(context: ExecutionContext): Promise<boolean> {\n    const request = context.switchToHttp().getRequest<Request>();\n    const user = request.user as JwtPayload;\n    const invoiceId = +request.params.id;\n\n    if (!invoiceId || isNaN(invoiceId)) return false;\n\n    const invoice = await this.invoiceService.findById(invoiceId);\n    if (!invoice) {\n      // Return false (403), not throw NotFoundException\n      // Avoids leaking object existence to unauthorized users\n      return false;\n    }\n\n    const isOwner = invoice.ownerId === user.sub;\n    const isAdmin = user.roles?.includes('admin');\n    return isOwner || isAdmin;\n  }\n}\n\n// invoice.controller.ts\n@Controller('invoices')\n@UseGuards(JwtAuthGuard)  // authentication\nexport class InvoiceController {\n  @Get(':id')\n  @UseGuards(InvoiceOwnerGuard)  // object-level authorization\n  async getInvoice(@Param('id', ParseIntPipe) id: number) {\n    return this.invoiceService.findById(id);\n    // Safe: guard already verified ownership before reaching here\n  }\n\n  @Delete(':id')\n  @UseGuards(InvoiceOwnerGuard)  // same guard — ownership required\n  async deleteInvoice(@Param('id', ParseIntPipe) id: number) {\n    return this.invoiceService.delete(id);\n  }\n}\n```\n\n**ASVS V4.1 checklist satisfied:**\n- [x] V4.1.1: user has specific authorization before accessing object\n- [x] V4.1.2: least privilege — owners and admins only\n- [x] V4.1.3: server-side enforcement — guard runs before controller\n- [x] V4.1.5: access control failures return 403 (NestJS default when CanActivate returns false)\n\n**Scale to 11 endpoints:** apply `@UseGuards(InvoiceOwnerGuard)` to each endpoint. For cross-entity authorization, create entity-specific guards following the same pattern.",
  "rejectedOutput": "Add WHERE ownerId = userId to every query — this ensures users can only see their own data at the database level.",
  "arbiters": ["OWASP-ASVS", "OWASP-API-SECURITY-TOP10"],
  "whyAccepted": "The NestJS guard pattern implements ASVS V4.1 at the application layer with explicit ownership verification. It is reusable across endpoints, testable in isolation, and integrates with NestJS's declarative guard system.",
  "whyRejected": "WHERE ownerId = userId is a defense-in-depth measure, not a substitute for application-layer authorization. It bypasses audit logging, cannot enforce role-based exceptions (admin bypass), and fails silently when misapplied to related entity queries.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/", "https://owasp.org/API-Security/editions/2023/en/0xa1-broken-object-level-authorization/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
