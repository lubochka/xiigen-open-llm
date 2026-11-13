# External Sources Batch 20 — EXT-391 through EXT-410

Generated: 2026-05-04  
Ledger range: EXT-391 – EXT-410  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, VUE-DOCS, EFCORE-SOURCE, DOTNET-MICROSERVICES, MICROSOFT-LEARN-ARCH, CWE-TOP25, OWASP-ASVS

---

## EXT-391

```json
{
  "rowId": "EXT-391",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Kubernetes cluster has a Java microservice experiencing intermittent OOMKilled (Out Of Memory Killed) events. The JVM (Java Virtual Machine) is configured with `-Xmx512m` but the container memory limit is `512Mi`. The pod restarts repeatedly.",
    "b": "Diagnosis: JVM heap is 512MB but the container memory limit is also 512MB, leaving no room for JVM off-heap memory (code cache, Metaspace, thread stacks, DirectByteBuffers).",
    "c": "Off-heap JVM overhead is typically 100-250MB depending on application. Container memory limit must account for both heap and off-heap."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Increase the container memory limit to `memory: 768Mi` and set JVM heap to `512m` OR reduce JVM heap to `384m` with limit `512Mi`. The container memory limit must exceed the JVM max heap by at least 150-200MB to accommodate: JVM Metaspace (class metadata, ~100-200MB for large apps), JVM code cache (~60-100MB), thread stacks (1MB × thread count), and DirectByteBuffer off-heap. With `-Xmx512m` and `memory: 512Mi`, the off-heap demand causes the Linux OOM (Out Of Memory) killer to terminate the container. The correct formula: `container memory limit ≥ Xmx + 150-200MB overhead`. Also add `-XX:+ExitOnOutOfMemoryError` so the JVM exits cleanly on heap exhaustion rather than thrashing. Add a readiness probe to detect unhealthy state before the OOM killer acts.",
  "rejectedOutput": "Set -Xmx to 480m to leave 32MB headroom within the 512Mi limit. This gives the off-heap memory some room while keeping the memory limit unchanged.",
  "arbiters": ["jvm-off-heap-container-limit", "kubernetes-oomkilled-diagnosis"],
  "whyAccepted": "JVM off-heap overhead is 150-250MB — 32MB headroom is insufficient. Container limit must be increased or heap must be substantially reduced to leave proper overhead.",
  "whyRejected": "32MB headroom is too small for JVM Metaspace alone (which can grow to 100-200MB). OOMKilled events will continue with only 32MB margin.",
  "sourceAnchors": [
    "https://kubernetes.io/docs/concepts/configuration/manage-resources-containers/",
    "https://docs.oracle.com/en/java/javase/17/gctuning/factors-affecting-garbage-collection-performance.html"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-392

```json
{
  "rowId": "EXT-392",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement `calculate_statistics(data: list[float]) -> dict` returning `{mean, median, std_dev, percentile_95}` for a list of floating-point numbers.",
    "b": "Submitted implementation uses Python's `statistics` module for mean, median, and standard deviation but implements percentile_95 manually: `sorted_data[int(len(sorted_data) * 0.95)]`.",
    "c": "Test case: data = [1.0, 2.0, 3.0, ..., 100.0] (100 values). Expected percentile_95: 95.05 (linear interpolation). Actual with index method: 95.0."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace the manual index method with `statistics.quantiles(data, n=100)[94]` (Python 3.8+) or `sorted_data[int(len(sorted_data) * 0.95 - 0.5)]` with rounding. The simple index approach `sorted_data[int(0.95 * len)]` is the 'nearest rank' method — correct for exact-rank percentiles but does not match the standard linear interpolation (IEEE) method used by NumPy, Pandas, and most statistical tools. For `[1..100]`, nearest-rank gives index 94 = value 95.0; linear interpolation gives 95.05 (interpolated between 95 and 96). If the standard statistics.quantiles is unavailable, implement linear interpolation: `h = 0.95 * (n-1); floor_h = int(h); percentile = data[floor_h] + (h - floor_h) * (data[floor_h+1] - data[floor_h])`. Use whichever method matches the documented contract, but document it explicitly.",
  "rejectedOutput": "The index method is acceptable. The difference between 95.0 and 95.05 is negligible for practical purposes.",
  "arbiters": ["percentile-linear-interpolation", "statistics-quantile-method"],
  "whyAccepted": "The test expects 95.05 (linear interpolation). The submitted implementation returns 95.0 (nearest rank). The method must match the specified contract — 'negligible' is irrelevant when the test is explicit.",
  "whyRejected": "The difference between methods is systematic, not random. If the consumer uses SLA (Service Level Agreement) thresholds at specific percentile values, the 0.05 difference matters. The test is the specification.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench",
    "https://docs.python.org/3/library/statistics.html#statistics.quantiles"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-393

```json
{
  "rowId": "EXT-393",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A FastAPI application has a background task that reads from a database connection created at startup. In production, the connection occasionally times out. Three agents investigate: (A) The connection is created once at startup and held open — TCP keepalive is not configured, causing idle timeout after 4 hours. (B) The database pool exhaustion causes timeout errors for the background task. (C) The background task runs in a thread without a transaction context and gets a different connection from the pool on each retry.",
    "b": "Evaluate which finding best explains the symptom: 'background task fails with OperationalError: server closed the connection unexpectedly after running correctly for several hours.'",
    "c": "Postgres database. Background task runs every 30 minutes. Startup connection held open for hours between task runs."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Select Agent-A's finding: TCP idle timeout causing connection closure. The symptom — 'fails after running correctly for several hours' — is the classic signature of an idle TCP connection being dropped by a network device (firewall, NAT gateway, load balancer) after an inactivity timeout. A startup-time single connection held open between 30-minute tasks is idle for 29.5 minutes between runs. After 4-8 hours, the connection is silently dropped at the network layer; the application does not know until it tries to use it. Fix: (1) Use connection pooling (SQLAlchemy pool or asyncpg pool) with `pool_recycle=1800` (recycle connections every 30 minutes). (2) Enable `pool_pre_ping=True` — SQLAlchemy issues a `SELECT 1` before each connection use to detect stale connections. Agent-B (pool exhaustion) would manifest as timeout errors at high load, not specifically 'after several hours'. Agent-C (no transaction context) would cause immediate errors, not delayed ones.",
  "rejectedOutput": "Select Agent-C's finding. Background threads without proper transaction context cause intermittent failures that appear unpredictable.",
  "arbiters": ["tcp-idle-timeout-db-connection", "sqlalchemy-pool-pre-ping"],
  "whyAccepted": "The 'fails after several hours' timing is a textbook TCP idle timeout pattern. pool_recycle + pool_pre_ping addresses the root cause without architecture changes.",
  "whyRejected": "Agent-C (transaction context) would cause immediate failures, not time-delayed ones. The 4-hour symptom onset is diagnostic evidence pointing to TCP idle timeout, not a threading issue.",
  "sourceAnchors": [
    "https://www.swebench.com",
    "https://docs.sqlalchemy.org/en/20/core/pooling.html#dealing-with-disconnects"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-394

```json
{
  "rowId": "EXT-394",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-LearningCycle",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.10 data processing library uses `__init_subclass__` hooks and `__class_getitem__` for a generic base class pattern. A developer proposes migrating to use `dataclasses.dataclass` with `__init_subclass__` replaced by dataclass field inheritance.",
    "b": "The current pattern: `class Processor(Generic[T]): def __init_subclass__(cls, **kwargs): super().__init_subclass__(**kwargs); cls._registry[cls.__name__] = cls`. Subclasses register themselves on definition. The `_registry` enables a factory method: `Processor.create('ConcreteProcessor', data)`.",
    "c": "7 concrete subclasses. The registry-based factory is the primary consumer of __init_subclass__."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "DEFER migration. `__init_subclass__` is not replaceable by dataclasses — they address different concerns. `@dataclass` provides auto-generated `__init__`, `__repr__`, `__eq__` for data container classes. The `__init_subclass__` pattern provides automatic subclass registration into `_registry` — a metaclass alternative. Migrating to `@dataclass` would require moving the registration logic to a class decorator or explicit registration call in each subclass. Neither is cleaner than the current `__init_subclass__`. The factory method `Processor.create()` based on string name lookup in `_registry` is a valid plugin/strategy pattern. The developer's proposal conflates two orthogonal patterns: data storage (`@dataclass`) and subclass registration (`__init_subclass__`). If the goal is type-safe generic syntax, apply PEP-695 syntax WHEN Python minimum is 3.12 (current is 3.10 — blocked).",
  "rejectedOutput": "Migrate to dataclasses with explicit registration. It is more explicit and avoids metaclass magic.",
  "arbiters": ["init-subclass-registry-pattern", "dataclass-vs-metaclass-registration"],
  "whyAccepted": "dataclasses and __init_subclass__ solve different problems. __init_subclass__ provides automatic subclass auto-registration; dataclasses provide field declaration. They are orthogonal and can be combined.",
  "whyRejected": "The proposal incorrectly treats __init_subclass__ as 'metaclass magic' to be eliminated. Explicit registration in each subclass is more verbose and fragile — adding a new subclass requires remembering to call register().",
  "sourceAnchors": [
    "https://peps.python.org/pep-0487/",
    "https://docs.python.org/3/reference/datamodel.html#object.__init_subclass__"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-395

```json
{
  "rowId": "EXT-395",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-TopologyDiscovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team wants to add SLO (Service Level Objective) alerting to a microservice. Options: (A) Prometheus alerting rules with static thresholds (e.g., alert if error rate > 1% over 5 minutes). (B) Sloth — an SLO-based alerting framework that generates multi-window multi-burn-rate alerts from an SLO spec. (C) OpenTelemetry SLO recording rules.",
    "b": "The SLO is: 99.9% of requests complete successfully within 200ms over a 30-day window. The team wants to be paged early when the error budget is being consumed too fast.",
    "c": "Prometheus and Alertmanager are already deployed."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use Sloth (option B) for multi-window multi-burn-rate SLO alerting. Static threshold alerts (option A) — e.g., 'error rate > 1% for 5 minutes' — are inadequate for SLO-based alerting: they do not account for burn rate. A 2% error rate for 30 minutes consumes more error budget than a 10% error rate for 2 minutes, but a static threshold might only page for the 10% case. Multi-burn-rate alerts (Google SRE Book, Chapter 5) page when the budget is being consumed at 14.4x normal rate (fast burn — page now) or 6x rate (slow burn — page within an hour). Sloth takes an SLO spec (99.9%, 30-day window) and generates the correct Prometheus recording rules and alert rules automatically. The SLO window and target drive the burn rate thresholds mathematically. Option C (OTel SLO recording rules) is not a standardised framework — use Sloth with Prometheus which is already deployed.",
  "rejectedOutput": "Use static Prometheus threshold alerts (option A). They are simpler, easier to understand, and the team already knows Prometheus alerting rule syntax.",
  "arbiters": ["sloth-slo-alerting", "multi-burn-rate-alerting"],
  "whyAccepted": "Multi-burn-rate SLO alerting (Sloth) mathematically derives alert thresholds from the SLO target and window. Static thresholds miss slow burns that silently exhaust error budgets.",
  "whyRejected": "Static threshold alerts miss slow-burn error budget depletion. A 0.2% error rate for 12 hours exhausts the weekly error budget but never triggers a '> 1% for 5 minutes' alert.",
  "sourceAnchors": [
    "https://sloth.dev/",
    "https://sre.google/workbook/alerting-on-slos/"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-396

```json
{
  "rowId": "EXT-396",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Dapper-based application needs to handle database connection resilience: transient failures (network glitch, SQL Server failover) should be retried automatically. Two implementations: (A) Wrap all Dapper calls in Polly retry policies (3 attempts, exponential backoff). (B) Use SqlConnection with `ConnectRetryCount` and `ConnectRetryInterval` in the connection string.",
    "b": "SQL Server connection string retry settings: `ConnectRetryCount=3;ConnectRetryInterval=10` — retries only during the initial connection phase, not during query execution.",
    "c": "The target scenario: query fails mid-execution during a SQL Server failover (always-on availability group failover). The connection is already established when the failover occurs."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use Polly retry policies (option A) for query-time transient failures. The SQL Server `ConnectRetryCount` setting (option B) only retries during the initial connection establishment — it does not handle failures that occur during query execution. An always-on failover that occurs after the connection is established will cause the query to fail with a `SqlException` that `ConnectRetryCount` does not catch. Polly with a `HandleTransientErrors()` policy (checking `SqlException.Number` for transient SQL Server error codes: 4060, 40613, 40501, 40197, 49918, 49919, 49920) retries at the query level. Combine both: connection string retry for initial connection + Polly for query-time transient failures. The Polly policy should be non-retryable for non-transient errors (1205 deadlock: retryable, 8152 string truncation: not retryable).",
  "rejectedOutput": "Use connection string retry settings (option B). They are built into the SQL Server driver and handle retries transparently without adding a library dependency.",
  "arbiters": ["polly-sql-transient-retry", "sql-connect-retry-scope"],
  "whyAccepted": "ConnectRetryCount only handles initial connection failures. Always-on failover during query execution requires Polly at the query level. Both layers are complementary.",
  "whyRejected": "ConnectRetryCount is scoped to connection establishment only. A query that fails after connection is established (always-on failover) is not retried by ConnectRetryCount — this is the explicit stated scenario.",
  "sourceAnchors": [
    "https://github.com/App-vNext/Polly",
    "https://learn.microsoft.com/en-us/sql/connect/ado-net/sql/sqlclient-support-high-availability-disaster-recovery"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-397

```json
{
  "rowId": "EXT-397",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An Angular 17 service uses a Subject to expose data: `private dataSubject = new Subject<Data[]>(); public data$ = this.dataSubject.asObservable();`. Components subscribe to `data$`. A new component subscribes after the data has been emitted and receives no data.",
    "b": "The developer changes Subject to ReplaySubject(1) to fix the late-subscriber problem. A colleague suggests using BehaviorSubject instead.",
    "c": "The service loads data once at startup and emits it. Late-subscribing components need the current value immediately on subscription."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Use BehaviorSubject, not ReplaySubject(1). Both solve the late-subscriber problem, but they differ in semantics and API: BehaviorSubject requires an initial value at construction (`new BehaviorSubject<Data[]>([])`), exposes `.getValue()` for synchronous access to the current value, and always emits the current value to new subscribers. ReplaySubject(1) replays the last emitted value to late subscribers but has no synchronous `.getValue()` — components can only receive the value asynchronously. For a service that loads data once at startup and components need the current value immediately, BehaviorSubject is semantically correct: the 'current state' model matches BehaviorSubject's design. ReplaySubject is appropriate when there is no meaningful default value before the first emission. Initialise with `[]` (empty array) as the before-load state.",
  "rejectedOutput": "Use ReplaySubject(1). It solves the late-subscriber problem equivalently to BehaviorSubject without requiring an initial value, which avoids rendering with empty data before the load completes.",
  "arbiters": ["behavior-subject-vs-replay-subject", "rxjs-initial-value-semantics"],
  "whyAccepted": "BehaviorSubject is the correct model for 'current state that components need immediately': synchronous getValue(), typed initial value, clear 'current state' semantics.",
  "whyRejected": "Avoiding an initial value to prevent 'rendering with empty data' is handled by the component: check `data.length > 0` before rendering. ReplaySubject's lack of synchronous getValue() is a capability loss without benefit here.",
  "sourceAnchors": [
    "https://rxjs.dev/api/BehaviorSubject",
    "https://rxjs.dev/api/ReplaySubject"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-398

```json
{
  "rowId": "EXT-398",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React 18 application has a modal dialog that traps focus when open. The team implements focus trapping manually with event listeners. A colleague points to the `@radix-ui/react-dialog` or `react-focus-trap` library instead.",
    "b": "Requirements: focus returns to the trigger element on close, Escape key closes the modal, screen reader announces the dialog role, background content is inert when modal is open.",
    "c": "WCAG 2.1 AA compliance is required. The manual focus trap implementation fails screen reader testing."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Replace the manual focus trap with `@radix-ui/react-dialog` (or `react-aria-components` from Adobe). Manual focus trapping correctly is significantly harder than it appears: (1) Focus management across portals, (2) Screen reader `aria-modal` attribute support, (3) `inert` attribute on background content (or aria-hidden), (4) Focus return to trigger on close, (5) Escape key handler that works across nested modals. All four requirements (focus return, Escape, role announcement, background inert) are implemented correctly in Radix UI Dialog — each is a separately documented ARIA authoring practice. The manual implementation failing screen reader testing is evidence that the complexity is not trivial. The library adds ~7KB gzip but eliminates a class of accessibility regressions. WCAG 2.1 AA compliance requires correct screen reader behaviour — a passing visual test is insufficient.",
  "rejectedOutput": "Fix the manual implementation. Adding a dependency for a focus trap is over-engineering when the specific ARIA issues can be addressed with targeted fixes.",
  "arbiters": ["wcag-aria-dialog-pattern", "radix-ui-accessibility-primitives"],
  "whyAccepted": "ARIA dialog patterns have 5+ correctness requirements that interact in non-obvious ways (portals, nested modals, screen reader quirks). Radix UI implements the ARIA APG dialog pattern specification correctly and is maintained for compatibility.",
  "whyRejected": "'Targeted fixes' to a failing screen reader implementation require expert ARIA knowledge, produce fragile code, and may introduce new failures when fixing the reported ones. The library is a 7KB investment in accessibility correctness.",
  "sourceAnchors": [
    "https://www.radix-ui.com/primitives/docs/components/dialog",
    "https://www.w3.org/WAI/ARIA/apg/patterns/dialog-modal/"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-399

```json
{
  "rowId": "EXT-399",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core 8 Minimal API endpoint deserializes a JSON body using `System.Text.Json`. A security review finds that the endpoint accepts any JSON property — including properties not in the DTO class — without error. This allows property injection if the DTO has settable properties that are not intended to be client-controlled.",
    "b": "Options: (A) Add `[JsonIgnore]` to sensitive properties. (B) Configure `JsonSerializerOptions` with `UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow`. (C) Add `[FromBody]` with a record type (immutable) — records only accept properties defined in the constructor.",
    "c": "The DTO has 5 properties the client should set and 2 properties set by the server (CreatedAt, UpdatedBy). The risk is CWE-915 mass assignment."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option C: record type DTOs with constructor-only properties. Record types enforce that only constructor parameters can be populated during deserialization — server-only properties (`CreatedAt`, `UpdatedBy`) are not in the constructor and cannot be set via JSON. This is the most idiomatic .NET 8 approach for immutable DTOs. Additionally apply option B (`JsonUnmappedMemberHandling.Disallow`) globally — this causes deserialization to fail with a JsonException if any unknown property is present, providing defence-in-depth and making API misuse explicit. Option A (`[JsonIgnore]`) prevents serialization of the property in responses but does not prevent it from being deserialized from the request body — `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]` is not the same as 'cannot be set by client'.",
  "rejectedOutput": "Add [JsonIgnore] to CreatedAt and UpdatedBy. This prevents these properties from appearing in responses and they cannot be round-tripped from the client.",
  "arbiters": ["cwe-915-mass-assignment-dotnet", "record-type-immutable-dto"],
  "whyAccepted": "Record type DTOs with constructor-only properties structurally prevent mass assignment — server-only properties are not deserializable. JsonIgnore prevents serialization but not deserialization.",
  "whyRejected": "[JsonIgnore] on sensitive properties prevents them from appearing in responses but does NOT prevent a client from sending `{CreatedAt: '2000-01-01'}` — System.Text.Json ignores unknown properties by default, but properties with [JsonIgnore] are still parsed from the input if present without the correct condition.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/immutability",
    "https://cwe.mitre.org/data/definitions/915.html"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-400

```json
{
  "rowId": "EXT-400",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS application needs to implement multi-tenancy: each tenant has isolated data. Options: (A) Single database with a `tenant_id` column on all tables and row-level security via RLS (Row-Level Security) policies. (B) Separate schema per tenant in a shared PostgreSQL instance. (C) Separate database per tenant.",
    "b": "Scale: 500 tenants at launch, 5,000 in 2 years. Tenant data isolation is a contractual requirement. Average tenant has 10,000 rows per major table.",
    "c": "Largest tenants have 1M rows. Budget constraint: cannot afford 5,000 separate database instances."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option B: separate schema per tenant in a shared PostgreSQL instance. Option A (shared schema with tenant_id + RLS) carries a catastrophic risk: a bug bypassing the tenant_id WHERE clause leaks all tenants' data simultaneously. PostgreSQL RLS provides a mitigation but it is a single layer of defence for a contractual isolation requirement. Option C (separate database per tenant) is correct for isolation but 5,000 databases exceeds the budget constraint. Option B (schema-per-tenant) provides structural isolation: each tenant's tables are in a separate schema (`tenant_abc.orders`, `tenant_xyz.orders`) — a query against tenant_abc's schema cannot access tenant_xyz's tables. NestJS with TypeORM or Prisma can be configured to use schema-based multi-tenancy with a request-scoped schema switcher (`SET search_path = tenant_abc`). PostgreSQL handles hundreds of schemas in a single instance efficiently.",
  "rejectedOutput": "Use option A: shared schema with tenant_id and RLS. RLS is designed for exactly this use case and avoids the complexity of schema management.",
  "arbiters": ["postgres-schema-per-tenant", "rls-single-defence-layer-risk"],
  "whyAccepted": "Schema-per-tenant provides structural data isolation within PostgreSQL capacity limits. A bug in tenant_id filtering in option A exposes all 500 tenants' data simultaneously — unacceptable for a contractual isolation requirement.",
  "whyRejected": "RLS is one layer of defence. A misconfigured policy, a direct connection bypassing RLS, or a framework bug omitting the WHERE clause exposes all tenant data. Single-layer defence for a contractual isolation requirement is insufficient.",
  "sourceAnchors": [
    "https://www.postgresql.org/docs/current/ddl-schemas.html",
    "https://www.postgresql.org/docs/current/ddl-rowsecurity.html"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-401

```json
{
  "rowId": "EXT-401",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API allows clients to download large reports (up to 500MB). Currently the server buffers the entire report in memory before sending. Clients report timeouts on large reports. Options: (A) Increase server timeout to 5 minutes. (B) Stream the response using chunked transfer encoding. (C) Generate the report asynchronously: POST /reports (returns 202 Accepted with a job ID), GET /reports/{jobId}/status (poll), GET /reports/{jobId}/download (when ready).",
    "b": "Report generation takes 30-120 seconds depending on size. Clients are both browser and programmatic.",
    "c": "RFC 9110 202 Accepted is the standard for async operations. RFC 7230 chunked transfer for streaming."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Implement option C: asynchronous report generation with 202 Accepted. Streaming (option B) solves the timeout problem but creates a long-lived HTTP connection (30-120 seconds) that holds a server thread/connection, limiting concurrency. At 100 concurrent report downloads, 100 threads are blocked for 1-2 minutes each. Option A (increase timeout) moves the timeout ceiling but does not solve the thread-blocking problem. Option C decouples report generation from download: POST /reports returns 202 immediately with `Location: /reports/{id}/status`; a background job generates the report; the client polls for completion; GET /reports/{id}/download streams the pre-generated file. The download is fast (pre-generated), streaming (no buffering), and does not block long-lived server threads. Browser clients: implement a polling UI. Programmatic clients: poll until status == 'READY'. The 202 Accepted pattern is RFC 9110 §15.3.3.",
  "rejectedOutput": "Increase the timeout to 5 minutes (option A) and stream the response. This is the simplest change that fixes the immediate timeout problem.",
  "arbiters": ["rfc-9110-202-async-operation", "streaming-vs-async-report-generation"],
  "whyAccepted": "Async report generation (202 Accepted) decouples generation time from HTTP connection duration. 100 concurrent reports do not block 100 server threads for 2 minutes each.",
  "whyRejected": "Option A + streaming still ties a server thread to each active report generation (30-120 seconds each). At scale, this exhausts thread pools. Option C reduces the download thread hold time to seconds (pre-generated file).",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.3.3",
    "https://datatracker.ietf.org/doc/html/rfc7230#section-4.1"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-402

```json
{
  "rowId": "EXT-402",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An API returns a paginated list with `total_count: 9847123` on every page response. The total count is computed with `SELECT COUNT(*) FROM orders WHERE ...` before the paginated SELECT. At 10,000 concurrent users this query runs 10,000 times simultaneously.",
    "b": "AIP-158 specifies that `total_size` in List responses is optional and SHOULD be omitted if it is expensive to compute.",
    "c": "The COUNT(*) query takes 800ms on the orders table (no index on the filter columns). This doubles the latency of every list request."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Remove the total_count from the paginated response. AIP-158 explicitly states: 'Services SHOULD NOT include total_size if it would be prohibitively expensive to compute.' An 800ms COUNT(*) that doubles list request latency for all 10,000 concurrent users is prohibitively expensive. Replace with: (1) Cursor pagination only — clients continue fetching until `next_page_token` is absent (last page). (2) If an approximate count is needed for UI display, use a stale estimate: PostgreSQL's `pg_stat_user_tables.n_live_tup` or SQL Server's `sys.partitions.row_count` — updated by AUTOVACUUM/statistics, not exact but instant. (3) For exact counts, move to an async endpoint: POST /orders/count-request (returns 202 Accepted with a job ID; the count is computed in the background and returned via GET /count-requests/{id}).",
  "rejectedOutput": "Cache the total_count with a Redis TTL of 60 seconds. This limits the COUNT(*) to once per minute per filter combination rather than once per request.",
  "arbiters": ["aip-158-total-size-optional", "count-star-performance-antipattern"],
  "whyAccepted": "AIP-158 explicitly authorises omitting total_size when expensive. Cursor pagination without total count is the correct pattern. Stale statistics or async count serve UI needs without 800ms synchronous queries.",
  "whyRejected": "Redis caching limits COUNT(*) frequency but still runs 800ms queries for new filter combinations. With 10,000 concurrent users using diverse filter combinations, cache hit rate may be low. The root cause (expensive COUNT*) is not addressed.",
  "sourceAnchors": [
    "https://google.aip.dev/158",
    "https://www.postgresql.org/docs/current/catalog-pg-stat-user-tables.html"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-403

```json
{
  "rowId": "EXT-403",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C1-ResultSynthesis",
  "rowClass": "negative-example",
  "input": {
    "a": "A model training job reports: `loss=nan` after epoch 3. The model trained normally in epochs 1-2 (loss decreased from 2.3 to 1.8).",
    "b": "Three operators: (R1) Reduce learning rate by 10x and resume from epoch 2 checkpoint. (R2) Check for NaN values in the training data and remove affected batches. (R3) Add gradient clipping (`torch.nn.utils.clip_grad_norm_(model.parameters(), max_norm=1.0)`) and resume from epoch 2 checkpoint.",
    "c": "The model uses an Adam optimiser with lr=1e-3. No gradient clipping. Training data was not checked for NaN values."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Apply R2 first (check data for NaN), then R3 (gradient clipping) as a preventive measure. NaN loss that appears suddenly after normal training is typically caused by: (1) NaN values in a training batch (most common — a batch with an invalid label or feature propagates NaN through the forward pass), (2) Exploding gradients (gradient norm → ∞, causing Adam's adaptive learning rate to produce NaN updates). Data NaN is the most likely cause when training was normal for 2 epochs then suddenly failed — it suggests a specific corrupted batch was encountered at epoch 3. Check: `torch.isnan(batch_data).any()` and `torch.isnan(batch_labels).any()` in the data loader. Gradient clipping (R3) is always a good practice and should be added after the root cause is identified. R1 (reduce learning rate) is unlikely to help if the cause is NaN data — the NaN batch will NaN the loss regardless of learning rate.",
  "rejectedOutput": "Apply R1: reduce learning rate by 10x. Exploding gradients are the most common cause of NaN loss and a lower learning rate stabilises training.",
  "arbiters": ["nan-loss-diagnosis-data-vs-gradient", "gradient-clipping-nan-prevention"],
  "whyAccepted": "Sudden NaN after 2 normal epochs is more likely caused by a corrupted batch than by gradient explosion (which tends to appear earlier during training). Data validation is the correct first step.",
  "whyRejected": "Gradient explosion typically manifests earlier in training when the model's weights are less stable. Reducing learning rate does not fix NaN data — the corrupted batch will still propagate NaN regardless.",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1",
    "https://pytorch.org/docs/stable/generated/torch.nn.utils.clip_grad_norm_.html"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-404

```json
{
  "rowId": "EXT-404",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A translation quality evaluation pipeline uses BLEU score as the sole metric for model selection. The team always promotes the model with the highest BLEU score on the Tatoeba EN-DE test set.",
    "b": "BLEU (Bilingual Evaluation Understudy) correlates with human judgement at the corpus level but can be gamed: a model that produces grammatically correct but semantically empty translations can achieve high BLEU by matching n-gram patterns.",
    "c": "The team has budget for one additional automatic metric. Human evaluation is available but expensive (2 weeks turnaround)."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Add COMET (Crosslingual Optimized Metric for Evaluation of Translation) as a second metric alongside BLEU. COMET is a neural evaluation metric trained on human judgement scores — it correlates significantly better with human ratings than BLEU, especially for meaning preservation vs surface-form matching. Model selection rule: promote only if BOTH BLEU improves AND COMET improves. If BLEU improves but COMET regresses, block the promotion and trigger human evaluation. This 'dual threshold' approach prevents BLEU gaming while keeping automated evaluation for the 95% case. COMET can be run with `pip install unbabel-comet` and runs on CPU (slower) or GPU (fast). For the EN-DE pair, COMET-22 or COMET-kiwi (reference-free) are appropriate. The combination of BLEU (surface fidelity) + COMET (semantic fidelity) provides better model discrimination than either alone.",
  "rejectedOutput": "Use BLEU alone with a higher threshold. Raising the BLEU threshold from 35 to 38 filters out lower-quality models without adding metric complexity.",
  "arbiters": ["comet-metric-human-correlation", "bleu-gaming-detection"],
  "whyAccepted": "COMET detects semantic quality that BLEU misses. Dual-threshold promotion (BLEU + COMET must both improve) prevents BLEU-gaming while keeping automated evaluation for routine model updates.",
  "whyRejected": "A higher BLEU threshold filters out worse models on average but does not detect models that achieve high BLEU with semantically poor translations. The gaming risk is not addressed by raising the threshold.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://github.com/Unbabel/COMET"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-405

```json
{
  "rowId": "EXT-405",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 application renders a form with dynamic field visibility: some fields appear or disappear based on other field values. Options: (A) Use `v-if` to conditionally render fields. (B) Use `v-show` to toggle field visibility.",
    "b": "Fields that are hidden must NOT submit their values. Hidden fields include sensitive inputs (payment details for non-paying users). v-if removes the element from the DOM; v-show uses `display:none` but the element remains in the DOM.",
    "c": "Form has 20 fields. 8 are conditionally visible. Toggling happens on every keystroke in a trigger field."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use `v-if` for fields that must not submit hidden values, and `v-show` for performance-sensitive frequent toggling. For the security requirement — hidden payment detail fields must not submit their values — `v-if` is mandatory: it removes the input element from the DOM entirely, preventing form submission of its value. `v-show` leaves the element in the DOM with `display:none` — the value is still present in the form and will be submitted (and accessible via JavaScript). For fields that toggle frequently on every keystroke (8 fields, 20 total) and do not have submission-safety requirements, `v-show` is more performant: it avoids DOM mount/unmount cycles. Rule: if the hidden field's value must not be submitted → v-if. If only visual toggling → v-show.",
  "rejectedOutput": "Use v-show for all conditional fields. It is more performant than v-if because it avoids component re-mounting on every toggle.",
  "arbiters": ["vue3-v-if-vs-v-show", "form-hidden-field-security"],
  "whyAccepted": "v-if removes the element from the DOM — hidden payment fields cannot be submitted or accessed. v-show leaves the element: accessible via JavaScript, submittable as hidden input. The security requirement mandates v-if.",
  "whyRejected": "v-show for payment detail fields is a security issue: the field value persists in the DOM even when hidden and may be submitted with the form or read by XSS scripts.",
  "sourceAnchors": [
    "https://vuejs.org/guide/essentials/conditional.html#v-if-vs-v-show",
    "https://vuejs.org/guide/best-practices/performance.html"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-406

```json
{
  "rowId": "EXT-406",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An EF Core 8 application uses a `DbContext` registered as a scoped service in ASP.NET Core. A developer injects the DbContext into a singleton service to cache database results.",
    "b": "DI scoping rule: a scoped service cannot be consumed by a singleton — the singleton holds the scoped service for its entire lifetime, which in ASP.NET Core DI causes a `InvalidOperationException: Cannot consume scoped service 'AppDbContext' from singleton 'CacheService'` at startup (with ValidateScopes=true).",
    "c": "ASP.NET Core 8 has ValidateScopes=true by default in development. Production default is false (does not throw but silently shares the DbContext across requests — causing threading and state issues)."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Fix by injecting `IDbContextFactory<AppDbContext>` into the singleton service instead of the DbContext directly. `IDbContextFactory<T>` is a singleton-safe factory that creates a short-lived DbContext for each operation: `using var context = _factory.CreateDbContext(); var result = await context.Products.FindAsync(id);`. The DbContext is created, used, and disposed within the singleton method — no shared state between calls. The alternative (injecting `IServiceScopeFactory` and creating a scope per operation) is also correct but more verbose. The root cause — `ValidateScopes=false` in production silently sharing a DbContext across concurrent requests — is a threading bug: DbContext is not thread-safe and sharing it causes data corruption and connection leaks.",
  "rejectedOutput": "Register the DbContext as a singleton instead of scoped. This allows it to be safely consumed by the singleton cache service.",
  "arbiters": ["efcore-dbcontext-factory-singleton", "aspnetcore-di-scope-rules"],
  "whyAccepted": "IDbContextFactory<T> creates a short-lived DbContext per operation inside the singleton — correct lifetime management without scope violations. DbContext is not thread-safe and must not be shared across concurrent operations.",
  "whyRejected": "Registering DbContext as a singleton makes it shared across all requests and threads. DbContext is not thread-safe — concurrent queries from multiple requests will cause data corruption, connection pool exhaustion, and hard-to-debug exceptions.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory",
    "https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-guidelines"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-407

```json
{
  "rowId": "EXT-407",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-RetrainingTrigger",
  "rowClass": "migration-decision",
  "input": {
    "a": "A .NET 6 service uses `HttpClient` created with `new HttpClient()` directly in service constructors. Each constructor call creates a new HttpClient instance. A load test reveals: 'System.Net.Sockets.SocketException: Only one usage of each socket address is normally permitted' — socket exhaustion from too many connections.",
    "b": "The root cause: `new HttpClient()` creates a new connection pool per instance. When many short-lived HttpClient instances are created and garbage collected, their underlying sockets enter TIME_WAIT and are unavailable for minutes. Fix: use `IHttpClientFactory` to reuse HTTP connections.",
    "c": ".NET 6. The service creates ~200 HttpClient instances per minute (short-lived request handlers)."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "MIGRATE to IHttpClientFactory immediately — this is a production incident, not routine maintenance. Register named or typed clients in DI: `services.AddHttpClient<OrderServiceClient>(client => client.BaseAddress = new Uri(baseUrl))`. `IHttpClientFactory` manages a pool of HttpMessageHandler instances — the underlying connections are shared across all HttpClient instances from the factory, solving socket exhaustion. The factory also rotates handlers every 2 minutes to respect DNS TTL changes. Migration steps: (1) Register all HttpClient usages via `services.AddHttpClient()` in Program.cs. (2) Inject the typed client or use `IHttpClientFactory.CreateClient(name)` in constructors. (3) Remove all `new HttpClient()` direct instantiations. This is not optional — socket exhaustion at 200 instances/minute causes service outages. Gate: zero SocketException errors in logs after deployment.",
  "rejectedOutput": "Keep new HttpClient() but implement a static singleton HttpClient shared across all request handlers. A single long-lived HttpClient reuses the connection pool.",
  "arbiters": ["httpclient-factory-socket-exhaustion", "dns-ttl-handler-rotation"],
  "whyAccepted": "IHttpClientFactory is the .NET 6+ idiomatic solution: shared connection pools, DI integration, Polly policy attachment, DNS TTL rotation via 2-minute handler cycling.",
  "whyRejected": "A static singleton HttpClient avoids socket exhaustion but does not respect DNS TTL changes (handler never rotates) — if the backend DNS record changes (e.g., Kubernetes service IP changes), the static HttpClient continues using the stale IP until restart.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory",
    "https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-408

```json
{
  "rowId": "EXT-408",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservices system needs to implement distributed transactions across 3 services: OrderService (SQL Server), InventoryService (SQL Server), and PaymentService (Stripe). The team debates: (A) 2PC (Two-Phase Commit) distributed transaction. (B) Saga pattern (choreography with compensating transactions).",
    "b": "2PC requires all participants to be in the prepared state before commit. Stripe is an external SaaS — it does not participate in a database 2PC protocol. Sagas use compensating transactions: if InventoryService succeeds but PaymentService fails, a compensation reverses the inventory reservation.",
    "c": "External dependency (Stripe) cannot be included in a 2PC transaction."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use Saga pattern (option B). 2PC (option A) is architecturally impossible for this system: Stripe is an external SaaS that does not support database 2PC. Even if Stripe were replaced with an internal service, 2PC at scale has known availability problems: if any participant is unavailable during the prepare phase, all participants are blocked until it recovers. Saga choreography: (1) OrderService creates an order and publishes OrderCreated event. (2) InventoryService reserves inventory and publishes InventoryReserved event. (3) PaymentService charges Stripe and publishes PaymentCompleted or PaymentFailed event. (4) If PaymentFailed: InventoryService publishes an InventoryReleased compensating event. Each step has a compensating transaction. Stripe provides idempotency keys (AIP-148 pattern) to handle duplicate charge attempts during saga retries. The saga must handle all failure scenarios with compensating transactions documented before implementation.",
  "rejectedOutput": "Use 2PC with a distributed transaction coordinator that wraps all three services. Modern distributed transaction managers can coordinate SQL Server and HTTP-based services.",
  "arbiters": ["saga-pattern-external-saas", "two-phase-commit-availability-problem"],
  "whyAccepted": "Stripe cannot participate in database 2PC — this is a hard technical constraint. Saga is the only viable pattern. 2PC availability problems (coordinator blocking) are an additional reason to avoid it.",
  "whyRejected": "HTTP-based distributed transaction coordinators exist (e.g., Seata) but require all participants to implement their specific protocol. Stripe does not implement any distributed transaction protocol — 2PC is impossible regardless of the coordinator choice.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/azure/architecture/reference-architectures/saga/saga",
    "https://stripe.com/docs/api/idempotent_requests"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-409

```json
{
  "rowId": "EXT-409",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A REST API accepts a `redirect_url` parameter after successful OAuth login: `GET /callback?code=auth_code&redirect_url=https://app.example.com/dashboard`. The server validates the auth code and redirects the user to `redirect_url`.",
    "b": "CWE-601 Open Redirect. Test payloads: `redirect_url=https://evil.com/phishing`, `redirect_url=//evil.com`, `redirect_url=/\\evil.com`.",
    "c": "The API currently validates that redirect_url is not empty but does not validate the domain."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Fix with an allowlist of permitted redirect origins. CWE-601 Open Redirect allows attackers to use a trusted domain's redirect endpoint to send victims to phishing sites. The user sees `app.example.com/callback` in the URL (trusted) before being redirected to `evil.com`. Fix: (1) Maintain a server-side allowlist of permitted redirect domains: `['app.example.com', 'app2.example.com']`. (2) Parse the redirect_url with `Uri.TryCreate()` or `URL()` constructor and check the host against the allowlist. (3) For relative URLs only (no host), validate with `Uri.IsWellFormedUriString(url, UriKind.Relative)`. Do NOT validate by string prefix `redirect_url.StartsWith('https://app.example.com')` — `https://app.example.com.evil.com` passes this check. Use URI parsing, not string matching.",
  "rejectedOutput": "Validate that redirect_url starts with 'https://app.example.com'. This ensures only URLs on the trusted domain are permitted.",
  "arbiters": ["cwe-601-open-redirect", "uri-allowlist-vs-string-prefix"],
  "whyAccepted": "URI parsing + host allowlist is bypass-resistant. The host component is unambiguously parsed regardless of path, query, or subdomain tricks.",
  "whyRejected": "String prefix `StartsWith('https://app.example.com')` is bypassable: `https://app.example.com.evil.com` starts with the trusted prefix but redirects to evil.com. URI parsing extracts the host component unambiguously.",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/601.html",
    "https://cheatsheetseries.owasp.org/cheatsheets/Unvalidated_Redirects_and_Forwards_Cheat_Sheet.html"
  ],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-410

```json
{
  "rowId": "EXT-410",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B1-RoutingTopology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Implement Content Security Policy (CSP) headers for an ASP.NET Core 8 web application to mitigate XSS. The application uses React with inline styles and a CDN for fonts.'",
    "b": "OWASP ASVS V14.4 HTTP Security Headers. CSP directives: `default-src`, `script-src`, `style-src`, `font-src`, `connect-src`, `img-src`.",
    "c": "The application uses: React bundle from /static/js, Google Fonts from fonts.googleapis.com, Stripe.js from js.stripe.com, inline styles from a CSS-in-JS library (emotion)."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Action items for CSP implementation: (1) Use `Content-Security-Policy-Report-Only` first (report mode) to audit violations without breaking the site: `Content-Security-Policy-Report-Only: default-src 'self'; report-uri /csp-report`. (2) Implement a /csp-report endpoint to collect violation reports. (3) Analyse reports for 1 week to identify all legitimate sources. (4) Build the production policy: `Content-Security-Policy: default-src 'self'; script-src 'self' js.stripe.com; style-src 'self' 'nonce-{nonce}' fonts.googleapis.com; font-src 'self' fonts.gstatic.com; connect-src 'self' api.stripe.com; img-src 'self' data:`. (5) For emotion (CSS-in-JS) inline styles: use `'nonce-{nonce}'` in style-src (generate a random nonce per request and inject into the emotion theme provider). Do NOT use `'unsafe-inline'` in script-src — this defeats XSS protection. Do NOT use `'unsafe-eval'` unless absolutely required. (6) Set CSP via middleware: `app.Use(async (ctx, next) => { ctx.Response.Headers.Append(\"Content-Security-Policy\", policy); await next(); })`.",
  "rejectedOutput": "Add `'unsafe-inline'` to script-src for simplicity. React's event handlers require inline scripts and removing unsafe-inline breaks the application.",
  "arbiters": ["owasp-asvs-v14-csp", "csp-nonce-vs-unsafe-inline"],
  "whyAccepted": "CSP-Report-Only audit → nonce-based inline styles is the correct migration path. React does not require unsafe-inline — modern React bundles are self-contained JS files, not inline scripts.",
  "whyRejected": "unsafe-inline in script-src completely defeats CSP's XSS protection — it allows any inline script to execute, including attacker-injected scripts. React's event handlers in JSX are compiled to bundle JS, not inline scripts.",
  "sourceAnchors": [
    "https://owasp.org/www-project-application-security-verification-standard/",
    "https://content-security-policy.com/nonce/"
  ],
  "licenseConstraint": "CC-BY-3.0",
  "trainingStatus": "APPROVED"
}
```
