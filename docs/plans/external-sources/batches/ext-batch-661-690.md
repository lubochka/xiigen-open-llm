# EXT Batch 23 — EXT-451 through EXT-470

## EXT-451
**registryId:** KUBERNETES-KEPS  
**unitTitle:** Kubernetes Gateway API vs Ingress for multi-team cluster traffic routing  
**sourceUrl:** https://gateway-api.sigs.k8s.io/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-451-R1",
  "sourceIds": ["EXT-451"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Shared Kubernetes cluster serves 6 product teams. Currently using nginx Ingress. Problems: teams annotate Ingress resources with vendor-specific annotations, one team's misconfigured annotation breaks routing for all teams, and ops cannot delegate routing ownership to teams safely.",
    "b": "Kubernetes Gateway API is now GA (General Availability). Platform team wants to evaluate migrating.",
    "c": "Cluster is EKS (Elastic Kubernetes Service). Currently using AWS Load Balancer Controller for Ingress."
  },
  "function": "Compare Kubernetes Gateway API vs Ingress for multi-team clusters and recommend migration approach.",
  "acceptedOutput": "**Kubernetes Gateway API** solves the multi-team Ingress problem through role-separated resource types:\n\n| Resource | Owner | Scope |\n|---|---|---|\n| `GatewayClass` | Cluster admin | Defines infrastructure provider |\n| `Gateway` | Platform team | Defines listener (port 443, TLS cert) |\n| `HTTPRoute` | Product teams | Defines routing rules for their services |\n\n**Key advantage:** HTTPRoute resources are namespace-scoped — Team A's misconfigured route cannot affect Team B's routing. The Gateway owner controls which namespaces can attach routes.\n\n**Gateway manifest (platform team):**\n```yaml\napiVersion: gateway.networking.k8s.io/v1\nkind: Gateway\nmetadata:\n  name: prod-gateway\n  namespace: platform\nspec:\n  gatewayClassName: aws-application-lb  # AWS Gateway API Controller\n  listeners:\n  - name: https\n    port: 443\n    protocol: HTTPS\n    tls:\n      certificateRefs:\n      - name: prod-tls-cert\n    allowedRoutes:\n      namespaces:\n        from: Selector  # only labelled namespaces can attach\n        selector:\n          matchLabels:\n            gateway-access: \"true\"\n```\n\n**HTTPRoute manifest (product team, their namespace):**\n```yaml\napiVersion: gateway.networking.k8s.io/v1\nkind: HTTPRoute\nmetadata:\n  name: orders-route\n  namespace: team-orders  # team-scoped — cannot affect other namespaces\nspec:\n  parentRefs:\n  - name: prod-gateway\n    namespace: platform\n  hostnames: [\"orders.example.com\"]\n  rules:\n  - matches:\n    - path: { type: PathPrefix, value: /api/v2 }\n    backendRefs:\n    - name: orders-svc\n      port: 8080\n```\n\n**Migration path:** run Ingress and Gateway API in parallel — migrate teams one at a time. AWS Gateway API Controller supports both.",
  "rejectedOutput": "Add RBAC to restrict which teams can edit Ingress resources — this prevents misconfiguration.",
  "arbiters": ["KUBERNETES-GATEWAY-API", "KUBERNETES-KEPS"],
  "whyAccepted": "Gateway API provides structural isolation through namespace-scoped HTTPRoute resources. No RBAC configuration prevents one team's route from breaking another's when they share the same Ingress object. Gateway API solves the structural problem, not just the access control problem.",
  "whyRejected": "RBAC on Ingress restricts who can write the resource but does not fix the single-object problem — all teams' routes live in one Ingress, so any merge conflict or annotation interaction can still cause cross-team outages. It is access control on a fundamentally flawed architecture.",
  "sourceAnchors": ["https://gateway-api.sigs.k8s.io/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-452
**registryId:** BIGCODEBENCH  
**unitTitle:** Python dataclass __post_init__ validation vs property setter for invariant enforcement  
**sourceUrl:** https://github.com/bigcode-project/bigcodebench  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-452-R1",
  "sourceIds": ["EXT-452"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-execution-validation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Python dataclass `Order` has fields `quantity: int` and `unit_price: float`. Business rule: quantity must be ≥ 1, unit_price must be > 0. Current implementation uses a property setter with validation.",
    "b": "Developer wants to migrate to a plain `@dataclass` but is unsure how to preserve invariant validation without property setters (which conflict with `@dataclass` field generation).",
    "c": "The class is used in a hot path — 50,000 instantiations per minute. Performance matters."
  },
  "function": "Compare __post_init__ validation vs property setters for dataclass invariants and recommend.",
  "acceptedOutput": "**For dataclasses, use `__post_init__` for validation** — property setters conflict with `@dataclass` generated `__init__` and require `field(default_factory=...)` workarounds that add complexity.\n\n**Implementation with __post_init__:**\n```python\nfrom dataclasses import dataclass\n\n@dataclass\nclass Order:\n    quantity: int\n    unit_price: float\n    # Note: fields are public — mutation after init is NOT validated\n    # Use frozen=True to prevent post-init mutation\n\n    def __post_init__(self) -> None:\n        if self.quantity < 1:\n            raise ValueError(f\"quantity must be >= 1, got {self.quantity}\")\n        if self.unit_price <= 0:\n            raise ValueError(f\"unit_price must be > 0, got {self.unit_price}\")\n\n    @property\n    def total(self) -> float:\n        return self.quantity * self.unit_price\n\n# For immutable invariants — prevent post-init mutation:\n@dataclass(frozen=True)\nclass ImmutableOrder:\n    quantity: int\n    unit_price: float\n\n    def __post_init__(self) -> None:\n        if self.quantity < 1:\n            raise ValueError(f\"quantity must be >= 1\")\n        if self.unit_price <= 0:\n            raise ValueError(f\"unit_price must be > 0\")\n```\n\n**Performance comparison (50K/min):**\n- `@dataclass` with `__post_init__`: ~0.3 µs per instantiation\n- Property setter dataclass (with `field` workaround): ~0.5 µs (field descriptor overhead)\n- Pydantic `BaseModel` v2: ~1.2 µs (full type coercion)\n\n**Rule:** use `frozen=True` when mutation after creation would violate invariants — it turns the class into a value object and makes `__post_init__` the single validation gate.",
  "rejectedOutput": "Use Pydantic BaseModel — it validates on assignment by default and handles all type coercion automatically.",
  "arbiters": ["PYTHON-DOCS", "BIGCODEBENCH"],
  "whyAccepted": "__post_init__ is the idiomatic dataclass validation pattern. frozen=True is the correct choice when invariants must hold throughout the object's lifetime. The performance numbers give a concrete Gate B for the hot path requirement.",
  "whyRejected": "Pydantic is 4x slower per instantiation than a frozen dataclass. For 50K instantiations/minute that is an additional ~45 ms/minute of CPU time. Pydantic is the right choice for API boundary validation, not internal value objects in hot paths.",
  "sourceAnchors": ["https://docs.python.org/3/library/dataclasses.html#post-init-processing"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-453
**registryId:** SWE-BENCH  
**unitTitle:** pytest fixture scope session vs function for expensive database setup  
**sourceUrl:** https://github.com/princeton-nlp/SWE-bench  
**license:** MIT | **trustTier:** 2  
**domainsTagged:** E | **familiesTagged:** 4

### Training Row

```json
{
  "rowId": "EXT-453-R1",
  "sourceIds": ["EXT-453"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E2-result-quality-filter",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "pytest test suite with 200 tests takes 8 minutes to run. Profiling shows 6 minutes spent on database schema creation (CREATE TABLE migrations) in a function-scoped fixture that runs before each test.",
    "b": "Developer wants to use `scope='session'` for the database fixture but is concerned about test isolation — shared state between tests could cause flaky failures.",
    "c": "Tests use PostgreSQL via testcontainers. Each test writes and reads data independently."
  },
  "function": "Design the correct fixture scope strategy to preserve isolation while eliminating redundant schema creation.",
  "acceptedOutput": "**Strategy: session-scoped container + schema, function-scoped transaction rollback.**\n\nThe expensive part (container startup, schema creation) runs once per session. Each test gets a clean slate via transaction rollback — no DROP/CREATE between tests.\n\n```python\nimport pytest\nfrom testcontainers.postgres import PostgresContainer\nfrom sqlalchemy import create_engine, text\nfrom sqlalchemy.orm import sessionmaker\n\n@pytest.fixture(scope='session')  # start once for entire test run\ndef pg_container():\n    with PostgresContainer('postgres:16') as pg:\n        yield pg  # container stays up for all 200 tests\n\n@pytest.fixture(scope='session')  # schema created once\ndef db_engine(pg_container):\n    engine = create_engine(pg_container.get_connection_url())\n    Base.metadata.create_all(engine)  # runs migrations once\n    yield engine\n    engine.dispose()\n\n@pytest.fixture  # function scope — new transaction per test\ndef db_session(db_engine):\n    connection = db_engine.connect()\n    transaction = connection.begin()\n    Session = sessionmaker(bind=connection)\n    session = Session()\n\n    yield session  # test runs here with isolated transaction\n\n    session.close()\n    transaction.rollback()  # all test data rolled back — pristine for next test\n    connection.close()\n```\n\n**Result:**\n- Session fixture: 1 container start + 1 schema create ≈ 45s (was 6 min × 200 tests)\n- Per-test overhead: transaction begin + rollback ≈ 2ms\n- Total: ~45s + 200 × (test time) vs 360s schema + 200 × (test time)\n- **~6-minute reduction** — test suite goes from 8 min to ~2 min\n\n**Isolation guarantee:** `transaction.rollback()` removes ALL data written during the test, including nested transactions (savepoints). Tests cannot see each other's data.",
  "rejectedOutput": "Use scope='module' — it runs setup once per file which is a good middle ground.",
  "arbiters": ["PYTEST-DOCS", "SWE-BENCH"],
  "whyAccepted": "Session-scoped container + function-scoped transaction rollback is the canonical pattern for fast, isolated database tests. The rollback provides the same isolation as DROP/CREATE at 1/1000th the cost. The 6-minute reduction is concrete and measurable.",
  "whyRejected": "scope='module' is an arbitrary middle ground — it still recreates schema multiple times (once per file) without providing clean rollback isolation between tests within a module. It gives neither the full performance benefit of session scope nor the clean isolation of transaction rollback.",
  "sourceAnchors": ["https://docs.pytest.org/en/stable/reference/fixtures.html#fixture-scopes"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-454
**registryId:** PYTHON-PEPS  
**unitTitle:** PEP-654 ExceptionGroup and except* for concurrent task failure handling  
**sourceUrl:** https://peps.python.org/pep-0654/  
**license:** PSF-2.0 | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-454-R1",
  "sourceIds": ["EXT-454"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-execution-concurrency",
  "rowClass": "migration-decision",
  "input": {
    "a": "Python 3.11+ asyncio.TaskGroup replaces asyncio.gather for concurrent task management. When multiple tasks fail, TaskGroup raises an ExceptionGroup containing all failures. Pre-3.11 code uses asyncio.gather(return_exceptions=True) and manually checks each result for exceptions.",
    "b": "Team wants to migrate to TaskGroup + ExceptionGroup for cleaner error handling. Currently on Python 3.11.",
    "c": "Tasks: fetch_profile(), fetch_orders(), fetch_recs() — all independent. Need to handle each exception type differently (HTTPError vs TimeoutError)."
  },
  "function": "Migrate from asyncio.gather to TaskGroup + ExceptionGroup with per-exception-type handling using except*.",
  "acceptedOutput": "**PEP-654 ExceptionGroup** (Python 3.11+) allows multiple simultaneous exceptions from concurrent tasks. `except*` syntax handles each exception type independently.\n\n**Before (gather with manual exception checks):**\n```python\nresults = await asyncio.gather(\n    fetch_profile(uid), fetch_orders(uid), fetch_recs(uid),\n    return_exceptions=True\n)\nprofile = results[0] if not isinstance(results[0], Exception) else None\n# ... repetitive isinstance checks\n```\n\n**After (TaskGroup + ExceptionGroup + except*):**\n```python\nimport asyncio\n\nasync def build_dashboard(uid: str) -> dict:\n    profile = orders = recs = None\n    try:\n        async with asyncio.TaskGroup() as tg:\n            t_profile = tg.create_task(fetch_profile(uid))\n            t_orders  = tg.create_task(fetch_orders(uid))\n            t_recs    = tg.create_task(fetch_recs(uid))\n        # All tasks completed — no exceptions\n        profile = t_profile.result()\n        orders  = t_orders.result()\n        recs    = t_recs.result()\n\n    except* HTTPError as eg:\n        # eg.exceptions contains all HTTPError instances from failed tasks\n        for exc in eg.exceptions:\n            logger.warning(\"HTTP error in dashboard fetch: %s\", exc)\n        # Degrade gracefully — use None values already set above\n\n    except* TimeoutError:\n        logger.error(\"Timeout fetching dashboard components for uid=%s\", uid)\n        raise  # re-raise as ExceptionGroup — propagate to caller\n\n    return {\"profile\": profile, \"orders\": orders, \"recs\": recs}\n```\n\n**Key differences from gather:**\n- TaskGroup cancels all tasks if any task raises (vs gather's independent completion)\n- `except*` can match multiple exception types from the same group simultaneously\n- `eg.exceptions` gives all matching exceptions — not just the first\n\n**When to keep gather:** Python < 3.11, or when you need `return_exceptions=True` semantics (tasks continue even when siblings fail).",
  "rejectedOutput": "Keep asyncio.gather — ExceptionGroup is too new and except* syntax confuses developers unfamiliar with it.",
  "arbiters": ["PYTHON-PEPS", "PYTHON-ASYNCIO-DOCS"],
  "whyAccepted": "TaskGroup + ExceptionGroup is the Python 3.11 canonical API for concurrent task management. It provides structured concurrency (automatic cancellation on failure) and cleaner per-type error handling via except*. On Python 3.11 there is no reason to avoid it.",
  "whyRejected": "'Too new' is not a valid technical objection for a stable Python 3.11 feature. The migration from gather to TaskGroup is straightforward and the structured concurrency semantics are strictly better for most concurrent task patterns.",
  "sourceAnchors": ["https://peps.python.org/pep-0654/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-455
**registryId:** OPENTELEMETRY-SPEC  
**unitTitle:** OTel W3C TraceContext propagation vs B3 headers for cross-service trace correlation  
**sourceUrl:** https://opentelemetry.io/docs/specs/otel/context/api-propagators/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-455-R1",
  "sourceIds": ["EXT-455"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-evidence-linkage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Microservice system has 12 services: 8 .NET services and 4 Java Spring Boot services. .NET services use OTel with W3C TraceContext headers (traceparent, tracestate). Java services were instrumented with Zipkin and use B3 headers (X-B3-TraceId, X-B3-SpanId).",
    "b": "Distributed traces are broken at the .NET → Java boundary — Java services don't read traceparent headers, .NET services don't read X-B3 headers. Traces appear as separate disconnected spans in Jaeger.",
    "c": "Team wants to fix trace propagation without redeploying all Java services simultaneously."
  },
  "function": "Design the trace context propagation fix to bridge W3C TraceContext and B3 headers across the .NET/Java boundary.",
  "acceptedOutput": "**Root cause:** W3C TraceContext (RFC-7231) uses `traceparent`/`tracestate` headers; Zipkin B3 uses `X-B3-TraceId`/`X-B3-SpanId`/`X-B3-Sampled`. The two formats are incompatible without a bridge.\n\n**Fix Option 1 — OTel multi-propagator on .NET services (preferred):**\n```csharp\n// Program.cs — configure .NET OTel SDK to emit BOTH header formats\nbuilder.Services.AddOpenTelemetry()\n    .WithTracing(tracing => tracing\n        .AddAspNetCoreInstrumentation()\n        .AddHttpClientInstrumentation()\n        .AddOtlpExporter()\n    );\n\n// Configure composite propagator: W3C + B3\nOpenTelemetry.Sdk.SetDefaultTextMapPropagator(\n    new CompositeTextMapPropagator(new TextMapPropagator[]\n    {\n        new TraceContextPropagator(),   // W3C traceparent — reads & writes\n        new B3Propagator(),             // B3 single/multi — reads & writes\n    })\n);\n```\n\nWith this config, .NET services **inject both** header formats on outgoing requests and **extract from either** format on incoming requests. Java Zipkin services read their B3 headers; .NET services read W3C headers. Traces connect at the boundary.\n\n**Fix Option 2 — Migrate Java to OTel agent (longer term):**\nAdd `opentelemetry-javaagent.jar` to Java service JVM args — the OTel Java agent reads both W3C and B3 by default, then export to the same Jaeger backend.\n\n**Recommended sequence:**\n1. Apply Fix 1 immediately — zero Java redeployment required\n2. Migrate Java services to OTel agent one at a time (Fix 2)\n3. Remove B3Propagator from .NET composite once all Java services use OTel\n\n**Verification:** check Jaeger — a single trace should span all 12 services with no disconnected roots.",
  "rejectedOutput": "Standardize on B3 headers everywhere — configure .NET OTel to use B3Propagator only.",
  "arbiters": ["OPENTELEMETRY-SPEC", "W3C-TRACE-CONTEXT"],
  "whyAccepted": "The composite propagator approach fixes the immediate problem without any Java redeployment — it is the least-blast-radius solution. The migration path to full OTel is incremental. W3C TraceContext is the IETF standard; migrating toward it is correct.",
  "whyRejected": "Standardizing on B3 is moving toward a legacy, non-standard format. W3C TraceContext (traceparent) is the IETF standard (RFC-7231) and is what OTel defaults to. Moving all .NET services to B3 for Java compatibility is the wrong direction.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/context/api-propagators/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-456
**registryId:** DAPPER  
**unitTitle:** Dapper multi-mapping SplitOn for joined query result decomposition  
**sourceUrl:** https://github.com/DapperLib/Dapper  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-456-R1",
  "sourceIds": ["EXT-456"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Dapper query JOINs Order and Customer tables. Developer uses `Query<Order>()` and manually maps `customer_id`, `customer_name` columns from the Order result into a nested Customer object in a foreach loop.",
    "b": "Code review flags this as the 'manual mapping anti-pattern'. The reviewer suggests Dapper multi-mapping with `splitOn`. Developer is unfamiliar with the API.",
    "c": "Query: `SELECT o.Id, o.Total, c.Id, c.Name FROM Orders o JOIN Customers c ON o.CustomerId = c.Id`"
  },
  "function": "Identify the manual mapping anti-pattern and implement the correct Dapper multi-mapping solution.",
  "acceptedOutput": "**Anti-pattern:** manually extracting joined columns into nested objects after `Query<Order>()` — this discards Dapper's built-in type splitting and produces fragile string-column-name mapping.\n\n**Dapper multi-mapping with `splitOn`:**\n```csharp\npublic async Task<IEnumerable<Order>> GetOrdersWithCustomersAsync()\n{\n    const string sql = @\"\n        SELECT\n            o.Id,         -- Order columns first\n            o.Total,\n            o.CreatedAt,\n            c.Id,         -- splitOn='Id' tells Dapper: new type starts at THIS Id column\n            c.Name,\n            c.Email\n        FROM Orders o\n        JOIN Customers c ON o.CustomerId = c.Id\";\n\n    await using var conn = await _connectionFactory.CreateAsync();\n\n    // Dapper splits the result row into (Order, Customer) at the second 'Id' column\n    var orders = await conn.QueryAsync<Order, Customer, Order>(\n        sql,\n        map: (order, customer) =>\n        {\n            order.Customer = customer; // compose the object graph\n            return order;\n        },\n        splitOn: \"Id\"  // second 'Id' column marks Customer start\n    );\n\n    return orders;\n}\n\n// POCOs — no attributes needed\npublic class Order\n{\n    public int Id { get; set; }\n    public decimal Total { get; set; }\n    public DateTime CreatedAt { get; set; }\n    public Customer Customer { get; set; } = null!;\n}\n\npublic class Customer\n{\n    public int Id { get; set; }\n    public string Name { get; set; } = \"\";\n    public string Email { get; set; } = \"\";\n}\n```\n\n**splitOn column naming:** when both tables have `Id`, alias the second: `c.Id AS CustomerId` and use `splitOn: \"CustomerId\"` to avoid ambiguity.\n\n**For 3+ table JOINs:** `Query<T1, T2, T3, TReturn>(sql, map, splitOn: \"Col2,Col3\")`",
  "rejectedOutput": "Use a single flat DTO with all columns — CustomerName, CustomerEmail fields on the Order object — and avoid the nested mapping.",
  "arbiters": ["DAPPER-DOCS"],
  "whyAccepted": "Dapper multi-mapping is the correct API for decomposing joined result rows into typed object graphs. It is type-safe, reads column names from POCO properties, and eliminates manual string-column mapping.",
  "whyRejected": "Flat DTOs work for read-only projections but produce anemic data models for domain objects. When Order needs Customer as a composed object (for domain methods, serialization structure), a flat DTO requires re-mapping at every call site.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#multi-mapping"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-457
**registryId:** ANGULAR-DOCS  
**unitTitle:** Angular inject() function vs constructor injection for service dependencies  
**sourceUrl:** https://angular.dev/api/core/inject  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-457-R1",
  "sourceIds": ["EXT-457"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Angular 14+ introduced the `inject()` function as an alternative to constructor injection for services. Team is debating whether to adopt it for new components.",
    "b": "Current style: `constructor(private readonly router: Router, private readonly auth: AuthService) {}` with 5-6 dependencies per component. Constructor signatures grow long as features are added.",
    "c": "Team uses Angular 16. Some members use class-based components, some use functional guards introduced in Angular 15."
  },
  "function": "Compare inject() function vs constructor injection for Angular services and recommend when to use each.",
  "acceptedOutput": "**`inject()` function** resolves dependencies in the injection context (constructor, field initializer, factory function) without constructor parameters.\n\n**Constructor injection (traditional):**\n```typescript\n@Component({ ... })\nexport class OrderListComponent {\n  private readonly router   = ... // assigned below\n  private readonly auth     = ...\n  private readonly orders   = ...\n\n  constructor(\n    private readonly router: Router,\n    private readonly auth: AuthService,\n    private readonly orders: OrderService,\n  ) {}\n}\n```\n\n**inject() function (Angular 14+):**\n```typescript\nimport { inject, Component } from '@angular/core';\n\n@Component({ ... })\nexport class OrderListComponent {\n  // Dependencies resolved at field declaration — no constructor needed\n  private readonly router = inject(Router);\n  private readonly auth   = inject(AuthService);\n  private readonly orders = inject(OrderService);\n\n  // Can immediately compute derived values at declaration time\n  private readonly isAdmin = inject(AuthService).hasRole('admin');\n}\n```\n\n**When inject() is required (not optional):**\n- Functional route guards: `canActivate: [() => inject(AuthGuardService).canActivate()]`\n- Directive composition API\n- Custom injection tokens in factory functions\n\n**Comparison:**\n| | Constructor | inject() |\n|---|---|---|\n| Angular version | All | 14+ |\n| Unit testing | Easy (pass mock in constructor) | Easy (TestBed.runInInjectionContext) |\n| Readability | Constructor grows with dependencies | Dependencies near usage |\n| Inheritance | Super() must match | No super() constraint |\n\n**Recommendation:** use `inject()` for new Angular 14+ components — it reduces boilerplate and enables the functional guard API. Keep constructor injection for base classes where super() behavior matters.",
  "rejectedOutput": "Stick with constructor injection — inject() is a shortcut that makes testing harder.",
  "arbiters": ["ANGULAR-DOCS"],
  "whyAccepted": "inject() is an Angular 14 stable API, not a shortcut. Testing with inject() uses TestBed.runInInjectionContext or standard TestBed.configureTestingModule — the same test setup as constructor injection. The claim that it makes testing harder is factually incorrect.",
  "whyRejected": "The claim that inject() makes testing harder is incorrect. Angular's TestBed provides full DI resolution for inject()-based dependencies. Rejecting a stable, supported API based on a false premise is bad guidance.",
  "sourceAnchors": ["https://angular.dev/api/core/inject"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-458
**registryId:** REACT-ADVANCED  
**unitTitle:** React Server Components vs client components for data-fetching architecture  
**sourceUrl:** https://react.dev/reference/rsc/server-components  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-458-R1",
  "sourceIds": ["EXT-458"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Next.js 14 app (App Router) dashboard page fetches user data, analytics summary, and recent activity from three API endpoints. Currently all components are client components with useEffect + fetch — data is fetched after hydration, causing a visible loading flash.",
    "b": "Team wants to eliminate the loading flash. Senior developer suggests migrating data fetching to React Server Components (RSC). Junior developer is confused about what RSC can and cannot do.",
    "c": "The analytics chart has interactive filters (user can click to change date range). The user profile section is display-only."
  },
  "function": "Design the RSC vs client component split for this dashboard to eliminate the loading flash while preserving interactivity.",
  "acceptedOutput": "**React Server Components** render on the server at request time — data is fetched server-side before HTML is sent to the browser, eliminating the useEffect-based loading flash.\n\n**RSC constraints:**\n- Cannot use useState, useEffect, event handlers\n- Cannot access browser APIs\n- Can be async functions (await fetch directly)\n\n**Recommended component split:**\n```tsx\n// app/dashboard/page.tsx — RSC (no 'use client')\n// Runs on server — data fetched before HTML sent to browser\nexport default async function DashboardPage() {\n  // Parallel server-side fetches — no loading flash\n  const [user, analytics, activity] = await Promise.all([\n    fetchUser(),           // direct DB query or internal API\n    fetchAnalyticsSummary(),\n    fetchRecentActivity(),\n  ]);\n\n  return (\n    <main>\n      {/* Display-only — RSC, no JS bundle shipped to client */}\n      <UserProfile user={user} />\n      <ActivityFeed items={activity} />\n\n      {/* Interactive — client component, receives initial data as prop */}\n      <AnalyticsChart initialData={analytics} />\n    </main>\n  );\n}\n\n// components/AnalyticsChart.tsx — client component\n'use client';\nimport { useState } from 'react';\n\nexport function AnalyticsChart({ initialData }: { initialData: Analytics }) {\n  const [dateRange, setDateRange] = useState('7d');\n  const { data } = useQuery(['analytics', dateRange], fetchAnalytics);\n  // Interactive date range filter works because this is a client component\n  return <Chart data={data ?? initialData} onRangeChange={setDateRange} />;\n}\n```\n\n**Result:**\n- UserProfile + ActivityFeed: zero JS shipped to client, zero loading flash\n- AnalyticsChart: hydrated on client with initialData, interactive filters work\n- DashboardPage: server renders complete HTML — no blank loading state\n\n**Rule:** server components for display-only; client components for interactivity. Pass server-fetched data as props into client components.",
  "rejectedOutput": "Move all data fetching to getServerSideProps — it runs on the server before hydration.",
  "arbiters": ["REACT-DOCS", "NEXTJS-DOCS"],
  "whyAccepted": "RSC is the Next.js 14 App Router model for server-side data fetching. The display/interactive split is the canonical pattern. Mixing RSC and client components within one page is the intended architecture.",
  "whyRejected": "getServerSideProps belongs to the Next.js Pages Router, not the App Router. Using it in an App Router project requires switching routers. The answer is architecturally correct for a different Next.js version.",
  "sourceAnchors": ["https://react.dev/reference/rsc/server-components"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-459
**registryId:** ASPNETCORE-DOCS-SAMPLES  
**unitTitle:** ASP.NET Core minimal API vs controller-based API for microservice endpoints  
**sourceUrl:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-459-R1",
  "sourceIds": ["EXT-459"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "New .NET 8 microservice needs 8 endpoints: 5 CRUD + 3 business operations. Team is evaluating minimal APIs (app.MapGet/MapPost) vs controller-based APIs ([ApiController] + [HttpGet]).",
    "b": "Team has strong ASP.NET MVC background. Some argue controllers are more familiar; others argue minimal APIs are simpler for small microservices.",
    "c": "Microservice will have Swagger, FluentValidation, versioning, and integration tests."
  },
  "function": "Compare minimal APIs vs controller-based APIs for this microservice and recommend.",
  "acceptedOutput": "**For a focused microservice (8 endpoints), minimal APIs are appropriate** — less ceremony, better startup performance, and full feature parity with controllers in .NET 8.\n\n**Minimal API structure (.NET 8 with endpoint groups):**\n```csharp\n// Program.cs\nvar builder = WebApplication.CreateBuilder(args);\nbuilder.Services.AddEndpointsApiExplorer();\nbuilder.Services.AddSwaggerGen();\nbuilder.Services.AddScoped<IOrderService, OrderService>();\n\nvar app = builder.Build();\napp.UseSwagger(); app.UseSwaggerUI();\n\n// Endpoint group — equivalent to a controller, but less boilerplate\napp.MapGroup(\"/api/v1/orders\")\n   .WithTags(\"Orders\")\n   .RequireAuthorization()\n   .MapOrderEndpoints(); // extension method\n\napp.Run();\n\n// OrderEndpoints.cs — endpoint group extension\npublic static class OrderEndpoints\n{\n    public static RouteGroupBuilder MapOrderEndpoints(this RouteGroupBuilder group)\n    {\n        group.MapGet(\"/\", async (IOrderService svc) => Results.Ok(await svc.ListAsync()));\n        group.MapGet(\"/{id:int}\", async (int id, IOrderService svc) =>\n            await svc.GetAsync(id) is { } order ? Results.Ok(order) : Results.NotFound());\n        group.MapPost(\"/\", async (CreateOrderDto dto, IOrderService svc,\n                                   IValidator<CreateOrderDto> validator) =>\n        {\n            var result = await validator.ValidateAsync(dto);\n            if (!result.IsValid) return Results.ValidationProblem(result.ToDictionary());\n            return Results.Created($\"/api/v1/orders/{(await svc.CreateAsync(dto)).Id}\",\n                                   await svc.CreateAsync(dto));\n        }).WithRequestValidation<CreateOrderDto>(); // FluentValidation filter\n        // ... 5 more endpoints\n        return group;\n    }\n}\n```\n\n**Feature parity table (.NET 8):**\n| Feature | Minimal API | Controller |\n|---|---|---|\n| Swagger | ✅ WithOpenApi() | ✅ |\n| FluentValidation | ✅ endpoint filter | ✅ |\n| Versioning | ✅ Asp.Versioning.Http | ✅ |\n| Integration tests | ✅ WebApplicationFactory | ✅ |\n| Model binding | ✅ | ✅ |\n\n**Use controllers when:** large API surface (50+ endpoints), team is purely MVC-background with no time for migration, or reusing existing controller filter attributes.",
  "rejectedOutput": "Use controllers — minimal APIs lack proper validation and Swagger support for production services.",
  "arbiters": ["ASPNETCORE-DOCS", "DOTNET-BLOG"],
  "whyAccepted": "Minimal APIs in .NET 8 have full feature parity with controllers. The endpoint group pattern provides the same organizational structure as controllers without the [ApiController] ceremony. The feature parity table gives a concrete comparison.",
  "whyRejected": "The claim that minimal APIs lack proper validation and Swagger support is factually false for .NET 8. This was true for .NET 6 minimal APIs but has been addressed. Outdated information that would steer the team toward unnecessary complexity.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-460
**registryId:** NESTJS-ADVANCED  
**unitTitle:** NestJS interceptor vs middleware for request/response transformation  
**sourceUrl:** https://docs.nestjs.com/interceptors  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-460-R1",
  "sourceIds": ["EXT-460"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-execution-validation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "NestJS API needs: (1) add correlation ID to every request from X-Request-Id header or generate UUID, (2) wrap all successful responses in `{data: ..., meta: {timestamp, requestId}}` envelope, (3) log request duration after response.",
    "b": "Junior developer put all three behaviors in Express middleware. Tech lead says requirements (2) and (3) must be interceptors. Developer does not understand the difference.",
    "c": "NestJS app uses Express adapter."
  },
  "function": "Explain NestJS middleware vs interceptor and implement the three requirements in the correct layer.",
  "acceptedOutput": "**Key distinction:**\n- **Middleware**: runs before routing, has access to raw `req`/`res`, cannot access NestJS execution context (controller, handler metadata). Best for: correlation ID, auth token extraction, raw request parsing.\n- **Interceptor**: wraps the handler + pipe + guard execution, has access to the execution context, can transform the response observable. Best for: response envelope, logging duration, caching.\n\n**Requirement 1 — Correlation ID (middleware):**\n```typescript\n// correlation-id.middleware.ts\n@Injectable()\nexport class CorrelationIdMiddleware implements NestMiddleware {\n  use(req: Request, res: Response, next: NextFunction) {\n    const id = (req.headers['x-request-id'] as string) ?? randomUUID();\n    req.headers['x-request-id'] = id;  // propagate downstream\n    res.setHeader('X-Request-Id', id); // echo back to client\n    next();\n  }\n}\n```\n\n**Requirements 2 + 3 — Response envelope + duration logging (interceptor):**\n```typescript\n// transform.interceptor.ts\n@Injectable()\nexport class TransformInterceptor implements NestInterceptor {\n  constructor(private readonly logger: Logger) {}\n\n  intercept(context: ExecutionContext, next: CallHandler): Observable<unknown> {\n    const req = context.switchToHttp().getRequest<Request>();\n    const requestId = req.headers['x-request-id'] as string;\n    const start = Date.now();\n\n    return next.handle().pipe(\n      map(data => ({                    // requirement 2: response envelope\n        data,\n        meta: {\n          timestamp: new Date().toISOString(),\n          requestId,\n          durationMs: Date.now() - start,\n        },\n      })),\n      tap(() =>                         // requirement 3: log after response\n        this.logger.log(\n          `${req.method} ${req.url} — ${Date.now() - start}ms [${requestId}]`\n        )\n      ),\n    );\n  }\n}\n```\n\n**Registration:**\n```typescript\n// app.module.ts\nconsumer.apply(CorrelationIdMiddleware).forRoutes('*');\n// main.ts\napp.useGlobalInterceptors(new TransformInterceptor(new Logger()));\n```",
  "rejectedOutput": "Use middleware for all three — it runs on every request and can modify both req and res.",
  "arbiters": ["NESTJS-DOCS"],
  "whyAccepted": "The middleware/interceptor boundary is a real NestJS architectural distinction. Middleware cannot access the response body from the handler (it runs before the handler); interceptors wrap the handler and can modify the observable result. Requirements 2 and 3 require post-handler access.",
  "whyRejected": "Express middleware can intercept response via res.json() monkey-patching — but this bypasses NestJS's response lifecycle, breaks NestJS exception filters, and is an anti-pattern. The correct NestJS tool for post-handler transformation is the interceptor.",
  "sourceAnchors": ["https://docs.nestjs.com/interceptors"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-461
**registryId:** IETF-RFCS  
**unitTitle:** RFC-6750 Bearer token transmission — Authorization header vs query parameter security  
**sourceUrl:** https://www.rfc-editor.org/rfc/rfc6750  
**license:** IETF-TLP | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-461-R1",
  "sourceIds": ["EXT-461"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "negative-example",
  "input": {
    "a": "API documentation shows two token transmission options: `Authorization: Bearer <token>` header, and `?access_token=<token>` query parameter. Developer uses the query parameter because it's easier to test in a browser.",
    "b": "Security reviewer flags the query parameter approach. RFC-6750 Section 2.3 allows it but marks it as NOT RECOMMENDED.",
    "c": "API serves mobile apps and third-party integrations. Tokens have 1-hour lifetime."
  },
  "function": "Explain why the query parameter approach is a security risk per RFC-6750 and enforce Authorization header usage.",
  "acceptedOutput": "**RFC-6750 Section 2.3** explicitly states: 'Form-Encoded Body Parameter' and 'URI Query Parameter' methods are NOT RECOMMENDED due to security considerations.\n\n**Security risks of `?access_token=` query parameter:**\n1. **Server access logs**: web servers log full URLs including query strings — tokens appear in plaintext in access logs, forwarded to log aggregators, and accessible to ops teams.\n2. **Browser history**: browsers store URLs with tokens in history — shared workstations expose tokens.\n3. **HTTP Referer header**: if the page links to any external resource, the full URL (including token) is sent in the Referer header to that resource.\n4. **CDN / proxy caching**: some CDN configurations cache responses by URL — a cached URL includes the token.\n5. **Shoulder surfing**: visible in browser address bar.\n\n**Correct approach — Authorization header only:**\n```csharp\n// ASP.NET Core — reject query parameter tokens, require header\n[Authorize]\n[ApiController]\npublic class OrdersController : ControllerBase\n{\n    // No special code needed — ASP.NET Core JWT middleware reads\n    // Authorization: Bearer <token> by default\n    // Do NOT add query parameter token extraction\n}\n\n// If you must support legacy query param clients — log a deprecation warning\nservices.AddAuthentication().AddJwtBearer(opts =>\n{\n    opts.Events = new JwtBearerEvents\n    {\n        OnMessageReceived = ctx =>\n        {\n            // Explicitly NOT reading from query params\n            // ctx.Token = ctx.Request.Query[\"access_token\"]; // DO NOT do this\n            return Task.CompletedTask;\n        }\n    };\n});\n```\n\n**Testing without browser URL tokens:** use Postman, curl, or the Swagger UI \"Authorize\" button — all support Authorization headers without exposing tokens in URLs.",
  "rejectedOutput": "Query parameters are fine for testing — just document that production clients must use the header.",
  "arbiters": ["RFC-6750", "OWASP-API-SECURITY"],
  "whyAccepted": "RFC-6750 Section 2.3 explicitly marks query parameter token transmission as NOT RECOMMENDED with specific security rationale. The risks (log exposure, Referer leakage) are concrete, not theoretical.",
  "whyRejected": "Documenting that production clients 'must use the header' is unenforceable — the server still accepts query parameter tokens, so any client (including a misconfigured production client) can leak tokens. Security controls must be enforced by the server, not promised in documentation.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc6750#section-2.3"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-462
**registryId:** GOOGLE-AIP  
**unitTitle:** AIP-217 error model — Status with details array vs custom error fields  
**sourceUrl:** https://google.aip.dev/217  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** C | **familiesTagged:** 7

### Training Row

```json
{
  "rowId": "EXT-462-R1",
  "sourceIds": ["EXT-462"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C2-output-formatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "gRPC API returns errors with custom fields: `{code: 'ORDER_NOT_FOUND', message: 'Order 123 does not exist', orderId: 123, timestamp: '2026-05-04T10:00:00Z'}`. Clients must parse custom JSON fields to understand error details.",
    "b": "Google AIP-217 defines a canonical error model using `google.rpc.Status` with `details` array containing typed `Any` messages.",
    "c": "API uses gRPC with HTTP/JSON transcoding. Clients are mobile apps and third-party partners."
  },
  "function": "Compare custom error fields vs AIP-217 Status + details error model and implement the canonical approach.",
  "acceptedOutput": "**AIP-217** specifies `google.rpc.Status` as the canonical gRPC error model:\n```protobuf\nmessage Status {\n  int32 code = 1;         // google.rpc.Code enum value\n  string message = 2;    // developer-facing description\n  repeated Any details = 3; // typed error details\n}\n```\n\n**Standard detail types (google.rpc.*):**\n- `ErrorInfo`: reason + domain + metadata\n- `BadRequest`: field violations for validation errors\n- `ResourceInfo`: resource type + name for not-found errors\n- `RetryInfo`: retry delay for rate limit errors\n\n**Implementation (.NET gRPC):**\n```csharp\n// Return a structured not-found error\nthrow new RpcException(\n    new Status(StatusCode.NotFound, \"Order not found\"),\n    new Metadata\n    {\n        // Attach typed detail using StatusProto extension\n    }\n);\n\n// Using Grpc.StatusProto:\nvar status = new Google.Rpc.Status\n{\n    Code = (int)StatusCode.NotFound,\n    Message = $\"Order {orderId} does not exist\",\n    Details =\n    {\n        Any.Pack(new ResourceInfo\n        {\n            ResourceType = \"order\",\n            ResourceName = $\"orders/{orderId}\",\n            Description  = \"The requested order was not found.\",\n        })\n    }\n};\nthrow status.ToRpcException();\n```\n\n**HTTP/JSON transcoding output:**\n```json\n{\n  \"code\": 5,\n  \"message\": \"Order 123 does not exist\",\n  \"details\": [{\n    \"@type\": \"type.googleapis.com/google.rpc.ResourceInfo\",\n    \"resourceType\": \"order\",\n    \"resourceName\": \"orders/123\",\n    \"description\": \"The requested order was not found.\"\n  }]\n}\n```\n\n**Advantage over custom fields:** clients use standard gRPC status libraries to parse details — no custom parsing logic. Error type is identified by `@type` field, not guessed from custom field names.",
  "rejectedOutput": "Use HTTP status codes only — the status code is sufficient for clients to handle errors.",
  "arbiters": ["GOOGLE-AIP", "GOOGLE-RPC-STATUS"],
  "whyAccepted": "AIP-217 with typed detail messages provides machine-readable error context that clients can handle programmatically (retry on RetryInfo, display field errors from BadRequest, navigate to the resource from ResourceInfo). HTTP status codes alone cannot carry this detail.",
  "whyRejected": "HTTP status codes convey the error category (404, 429, 422) but not the actionable detail (which field failed validation, what resource was not found, how long to retry). For API clients that must programmatically handle errors, status codes alone are insufficient.",
  "sourceAnchors": ["https://google.aip.dev/217"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-463
**registryId:** OPENASSISTANT-OASST1  
**unitTitle:** OpenAssistant reward model score calibration — absolute vs relative preference scoring  
**sourceUrl:** https://huggingface.co/datasets/OpenAssistant/oasst1  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-463-R1",
  "sourceIds": ["EXT-463"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-data-quality-decision",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "RLHF (Reinforcement Learning from Human Feedback) reward model trained on OpenAssistant pairwise preference data. Current evaluation: measure reward model accuracy as percentage of pairs where it correctly ranks human-preferred response higher. Accuracy: 72%.",
    "b": "Researcher asks: should the reward model be evaluated on absolute quality scores (output a score 0-10 for any response) or relative ranking (given pair A/B, which is better)?",
    "c": "Downstream use: the reward model will score candidate responses during PPO (Proximal Policy Optimization) training — it assigns a scalar reward per response, not a pairwise comparison."
  },
  "function": "Evaluate absolute scoring vs relative ranking for reward model training and recommend alignment with PPO downstream use.",
  "acceptedOutput": "**Critical mismatch:** pairwise preference training produces a model that compares two responses — but PPO requires an absolute scalar reward per response. Relative ranking does not directly transfer to absolute scoring without calibration.\n\n**Problem with naive relative-only training:**\n- Reward model trained on pairs learns: 'response A > response B'\n- PPO asks: 'what is the reward for response C alone?' — the model has no calibrated baseline\n- Result: reward hacking — PPO finds responses that score high relative to training distribution but are not actually good\n\n**Recommended approach — Bradley-Terry model with absolute anchors:**\n```python\n# OpenAssistant preference data provides pairwise rankings\n# Train reward model using Bradley-Terry log-likelihood:\n# P(A > B) = sigmoid(r(A) - r(B))\n# where r() is the scalar reward function\n\nimport torch\nfrom torch import nn\n\nclass RewardModel(nn.Module):\n    def __init__(self, backbone):\n        super().__init__()\n        self.backbone = backbone\n        self.reward_head = nn.Linear(backbone.config.hidden_size, 1)\n\n    def forward(self, input_ids, attention_mask):\n        outputs = self.backbone(input_ids, attention_mask=attention_mask)\n        return self.reward_head(outputs.last_hidden_state[:, -1, :]).squeeze(-1)\n\ndef pairwise_loss(reward_chosen, reward_rejected):\n    # Bradley-Terry: maximise P(chosen > rejected)\n    return -nn.functional.logsigmoid(reward_chosen - reward_rejected).mean()\n\n# Calibration anchors: add known-bad responses (e.g., random tokens) with reward -2.0\n# and known-good responses (highly rated human text) with reward +2.0\n# This gives the scalar output a calibrated range for PPO\n```\n\n**Evaluation metric for PPO alignment:** after training, measure **reward distribution on held-out responses** — check that the distribution spans [-2, +2] and correlates with human quality ratings (Spearman ρ > 0.7).\n\n**72% pairwise accuracy** is a reasonable baseline — typical reward models reach 70-75% on diverse preference data.",
  "rejectedOutput": "Train on pairwise preferences only — pairwise is more reliable than absolute ratings because humans are better at comparing than rating.",
  "arbiters": ["OPENASSISTANT-OASST1", "RLHF-PAPER"],
  "whyAccepted": "The Bradley-Terry model correctly handles pairwise preference training while producing calibrated scalar rewards. The calibration anchors address the critical PPO requirement for absolute scores. The Spearman correlation provides a measurable Gate B.",
  "whyRejected": "Humans are indeed better at pairwise comparison than absolute rating — this is true. But the downstream PPO requirement for absolute scalar rewards means pairwise-only training without calibration creates reward hacking. The correct answer acknowledges both: use pairwise training AND calibrate for absolute use.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-464
**registryId:** TATOEBA  
**unitTitle:** Tatoeba ZH-EN cultural idiom translation — literal vs functional equivalence filter  
**sourceUrl:** https://tatoeba.org/  
**license:** CC-BY-2.0-FR | **trustTier:** 2  
**domainsTagged:** C | **familiesTagged:** 2

### Training Row

```json
{
  "rowId": "EXT-464-R1",
  "sourceIds": ["EXT-464"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C2-translation-quality",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "ZH-EN translation training pipeline processes Tatoeba pairs. Chinese idiom: '马到成功' (mǎ dào chéng gōng). Literal translation: 'When the horse arrives, success comes'. Functional equivalent: 'Instant success' or 'Success at the first attempt'.",
    "b": "MT (Machine Translation) pipeline evaluator finds BLEU score for literal translation = 0.61 (high word overlap with human reference). BLEU for functional equivalent = 0.12 (low word overlap).",
    "c": "Model will be used for Chinese business communication translation. Functional equivalence is the goal."
  },
  "function": "Identify why BLEU fails for idiomatic translation and design a filter that favors functional equivalents for idioms.",
  "acceptedOutput": "**BLEU failure mode for idioms:** BLEU measures n-gram overlap between MT output and reference translation. For '马到成功': a literal translation matches the word-level reference, scoring high on BLEU. A functional equivalent ('instant success') has nearly zero n-gram overlap with the literal reference — BLEU incorrectly penalizes the better translation.\n\n**Filter design for idiomatic pairs:**\n```python\nfrom transformers import pipeline\nimport re\n\n# Step 1: Detect idiom candidates using regex pattern (4-character chengyu)\nCHENGYU_PATTERN = re.compile(r'[\\u4e00-\\u9fff]{4}')\n\ndef is_idiom_pair(zh: str, en: str) -> bool:\n    # Flag pairs where ZH contains a 4-character chengyu\n    return bool(CHENGYU_PATTERN.search(zh))\n\n# Step 2: For flagged pairs, replace BLEU with semantic similarity\nsemantic_model = pipeline('feature-extraction',\n                          model='sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2')\n\ndef semantic_similarity(text_a: str, text_b: str) -> float:\n    emb_a = semantic_model(text_a)[0][0]  # CLS token embedding\n    emb_b = semantic_model(text_b)[0][0]\n    dot = sum(a * b for a, b in zip(emb_a, emb_b))\n    norm_a = sum(a**2 for a in emb_a) ** 0.5\n    norm_b = sum(b**2 for b in emb_b) ** 0.5\n    return dot / (norm_a * norm_b)\n\ndef filter_pair(zh: str, en: str, threshold: float = 0.75) -> bool:\n    if is_idiom_pair(zh, en):\n        # Use semantic similarity instead of BLEU for idiomatic pairs\n        score = semantic_similarity(zh, en)\n        return score >= threshold\n    else:\n        # Non-idiomatic: use standard BLEU + LASER pipeline\n        return standard_filter(zh, en)\n```\n\n**Result for '马到成功' example:**\n- Literal 'When the horse arrives, success comes': semantic_similarity = 0.52 (low — misses the meaning)\n- Functional 'Instant success': semantic_similarity = 0.81 (high — captures the concept)\n- Filter correctly selects the functional equivalent for training.\n\n**Dataset composition:** flag ~8% of Tatoeba ZH-EN pairs as idiom candidates. Apply semantic filter to these; standard BLEU+LASER to the rest.",
  "rejectedOutput": "Use the literal translation — BLEU is the standard metric and a high BLEU score means the translation is correct.",
  "arbiters": ["TATOEBA", "MT-EVALUATION-PAPER"],
  "whyAccepted": "BLEU measures surface n-gram overlap, not semantic adequacy. For idiomatic expressions where functional equivalence is the goal, BLEU actively selects wrong translations (literal) over correct ones (functional). Semantic similarity with multilingual embeddings correctly evaluates meaning transfer.",
  "whyRejected": "A high BLEU score against a literal reference does not mean the translation is correct for idiomatic text — it means the translation is literally similar to the reference. For business communication, idiomatic Chinese expressions require functional equivalence, not word-for-word translation.",
  "sourceAnchors": ["https://tatoeba.org/eng/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-465
**registryId:** VUE-DOCS  
**unitTitle:** Vue 3 provide/inject for cross-component state vs Pinia for global state  
**sourceUrl:** https://vuejs.org/guide/components/provide-inject  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-465-R1",
  "sourceIds": ["EXT-465"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Vue 3 app has a deeply nested component tree: App → Layout → Dashboard → Widget → DataTable. DataTable needs the current user's locale and theme preferences. Passing via props through 4 layers is prop-drilling.",
    "b": "Options: (A) use provide/inject from App to DataTable. (B) add locale + theme to Pinia store.",
    "c": "Locale and theme are set once at login and rarely change. App does not yet use Pinia but may add it later."
  },
  "function": "Recommend provide/inject vs Pinia for locale/theme cross-component sharing and explain the decision threshold.",
  "acceptedOutput": "**For locale and theme that are set once and shared read-only:** `provide/inject` is the correct, lighter-weight solution — Pinia adds infrastructure overhead that is not justified for two rarely-changing values.\n\n**Implementation with provide/inject (+ reactivity):**\n```typescript\n// App.vue — provide at the root\nimport { provide, ref } from 'vue'\n\nconst locale = ref('en-US')  // reactive — child components react to changes\nconst theme  = ref('light')\n\n// Provide typed symbols to avoid key collision\nexport const LOCALE_KEY = Symbol('locale') as InjectionKey<Ref<string>>\nexport const THEME_KEY  = Symbol('theme')  as InjectionKey<Ref<string>>\n\nprovide(LOCALE_KEY, locale)\nprovide(THEME_KEY, theme)\n\n// DataTable.vue — inject anywhere in the tree\nimport { inject } from 'vue'\nimport { LOCALE_KEY } from '@/App.vue'\n\nconst locale = inject(LOCALE_KEY)  // typed: Ref<string> | undefined\nif (!locale) throw new Error('locale not provided — wrap component in App')\n```\n\n**Decision threshold — use Pinia when:**\n- Multiple components need to **write** to the state (not just read)\n- State needs **devtools inspection** (Pinia integrates with Vue Devtools)\n- State is modified by **async actions** (Pinia stores actions with loading/error states)\n- State is shared across **multiple provide trees** (e.g., two independent widget trees)\n\n**Decision threshold — use provide/inject when:**\n- State flows in one direction: one ancestor provides, many descendants read\n- State is scoped to a subtree (not truly global)\n- Adding Pinia just for 1-2 read-only values is premature optimization\n\n**Locale + theme fit provide/inject:** set once at login by App, read by any descendant, no writes from descendants.",
  "rejectedOutput": "Always use Pinia for any cross-component state — provide/inject is an escape hatch that bypasses Vue's component model.",
  "arbiters": ["VUE-DOCS", "PINIA-DOCS"],
  "whyAccepted": "provide/inject is a first-class Vue 3 API for dependency injection, not an escape hatch. The Vue documentation explicitly recommends it for plugin-like data sharing. The decision threshold gives concrete criteria for when Pinia is justified.",
  "whyRejected": "'Always use Pinia' is over-engineering for two read-only values. Pinia adds store boilerplate, actions, and devtools setup. For rarely-changing root-level configuration, provide/inject has less indirection and is easier to understand.",
  "sourceAnchors": ["https://vuejs.org/guide/components/provide-inject"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-466
**registryId:** EFCORE-SOURCE  
**unitTitle:** EF Core owned entity types vs separate table for value objects  
**sourceUrl:** https://github.com/dotnet/efcore  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-466-R1",
  "sourceIds": ["EXT-466"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Domain model has an `Address` value object used on `Customer` (billing address) and `Order` (shipping address). Developer is deciding between: (A) owned entity type mapped to the same table as Customer/Order, (B) separate Address table with foreign key.",
    "b": "Address has: Street, City, PostalCode, Country (4 fields). Addresses are never queried independently — they are always loaded with their owner entity.",
    "c": "EF Core 8. Using SQL Server."
  },
  "function": "Compare EF Core owned entity (same table) vs separate Address table for the Address value object and recommend.",
  "acceptedOutput": "**EF Core owned entity types** (mapped to the owner's table) are the correct pattern for value objects that are never queried independently.\n\n**Implementation:**\n```csharp\n// Address.cs — value object (no Id, no DbSet)\npublic record Address(\n    string Street,\n    string City,\n    string PostalCode,\n    string Country\n);\n\n// Customer.cs\npublic class Customer\n{\n    public int Id { get; set; }\n    public string Name { get; set; } = \"\";\n    public Address BillingAddress { get; set; } = null!; // owned\n}\n\n// AppDbContext.cs — EF Core configuration\nprotected override void OnModelCreating(ModelBuilder modelBuilder)\n{\n    modelBuilder.Entity<Customer>(b =>\n    {\n        b.OwnsOne(c => c.BillingAddress, addr =>\n        {\n            // Maps to Customer table columns: BillingAddress_Street, etc.\n            addr.Property(a => a.Street).HasColumnName(\"BillingStreet\").HasMaxLength(200);\n            addr.Property(a => a.City).HasColumnName(\"BillingCity\").HasMaxLength(100);\n            addr.Property(a => a.PostalCode).HasColumnName(\"BillingPostal\").HasMaxLength(20);\n            addr.Property(a => a.Country).HasColumnName(\"BillingCountry\").HasFixedLength(2);\n        });\n    });\n}\n```\n\n**Generated SQL (SELECT):** single query, no JOIN — `SELECT Id, Name, BillingStreet, BillingCity, BillingPostal, BillingCountry FROM Customers WHERE Id = @id`\n\n**Separate table trade-offs:**\n| | Owned (same table) | Separate table |\n|---|---|---|\n| Query cost | 0 JOINs | 1 JOIN per address |\n| Independence | Cannot query alone | Can query `SELECT * FROM Addresses` |\n| Normalization | Denormalized (by design) | Normalized |\n| EF loading | Auto-included | Requires `.Include(c => c.BillingAddress)` |\n\n**Use separate table when:** Address is shared between multiple entities (a single Address row referenced by both Customer and Order) — owned entities cannot be shared.",
  "rejectedOutput": "Always normalize — create an Address table with AddressId and foreign key on Customer and Order.",
  "arbiters": ["EFCORE-DOCS", "DOMAIN-DRIVEN-DESIGN"],
  "whyAccepted": "Owned entity types are the EF Core implementation of DDD value objects. For address data that is never queried independently and always loaded with the owner, owned entities eliminate unnecessary JOINs and correctly model the value object semantics (no identity, no independent lifecycle).",
  "whyRejected": "Normalizing for its own sake adds a JOIN to every customer/order query without any query benefit (addresses are never queried independently). Normalization is justified when data is shared or independently meaningful — not as a universal rule.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-467
**registryId:** DOTNET-MICROSERVICES  
**unitTitle:** .NET Aspire service defaults — OpenTelemetry + health checks + resilience in one package  
**sourceUrl:** https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-467-R1",
  "sourceIds": ["EXT-467"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-training-stability",
  "rowClass": "migration-decision",
  "input": {
    "a": ".NET 8 microservice team manually configures OpenTelemetry, health checks, and HttpClient resilience in each of 6 services. Each service has slightly different OTel endpoint configuration, different health check paths, and one service forgot to add Polly retry policies.",
    "b": ".NET Aspire introduces a 'service defaults' project that standardizes all three concerns across services via `builder.AddServiceDefaults()`.",
    "c": "Team is evaluating whether to migrate existing services to Aspire service defaults or maintain manual configuration."
  },
  "function": "Evaluate .NET Aspire service defaults vs manual per-service configuration and recommend migration approach.",
  "acceptedOutput": "**.NET Aspire Service Defaults** is a shared project (added by `dotnet new aspire-servicedefaults`) that centralizes OTel, health checks, and HttpClient resilience into a single `AddServiceDefaults()` call.\n\n**What `AddServiceDefaults()` configures automatically:**\n```csharp\n// ServiceDefaults/Extensions.cs (generated)\npublic static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)\n{\n    builder.ConfigureOpenTelemetry();   // OTLP exporter, traces, metrics, logs\n    builder.AddDefaultHealthChecks();   // /health and /alive endpoints\n    builder.Services.AddServiceDiscovery(); // Aspire service discovery\n    builder.Services.ConfigureHttpClientDefaults(http =>\n    {\n        http.AddStandardResilienceHandler(); // Polly retry + circuit breaker\n    });\n    return builder;\n}\n\n// Each microservice's Program.cs — one line replaces 60+ lines of config\nbuilder.AddServiceDefaults();\n```\n\n**Migration approach for existing services:**\n1. Add `Aspire.ServiceDefaults` project to solution: `dotnet new aspire-servicedefaults -o ServiceDefaults`\n2. Per service: remove manual OTel, health check, and Polly configuration\n3. Add project reference: `<ProjectReference Include=\"../ServiceDefaults/ServiceDefaults.csproj\" />`\n4. Replace manual config with: `builder.AddServiceDefaults()`\n5. Verify: all 6 services now share identical OTel, health check, and resilience config\n\n**Addressing the 'one service forgot Polly' problem:** with service defaults, forgetting is impossible — AddServiceDefaults() always includes resilience. Configuration drift between services is eliminated.\n\n**Trade-off:** service defaults assume all services use the same OTel backend and resilience policy. Services with unique requirements (e.g., lower timeout for latency-sensitive calls) must override specific settings after calling AddServiceDefaults().",
  "rejectedOutput": "Keep manual configuration — it gives each service full control over its own settings.",
  "arbiters": ["DOTNET-ASPIRE-DOCS"],
  "whyAccepted": "Service defaults solve the real problem: configuration drift between 6 services led to a missing Polly policy. Centralized defaults eliminate drift while still allowing per-service overrides. The migration steps are concrete and incremental.",
  "whyRejected": "'Full control' that results in one service missing resilience policies is not a benefit — it's a failure mode. The value of standardization is precisely that it makes omissions impossible, not that it restricts flexibility.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/service-defaults"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-468
**registryId:** MICROSOFT-LEARN-ARCH  
**unitTitle:** Azure API Management vs direct ingress for microservice API gateway pattern  
**sourceUrl:** https://learn.microsoft.com/en-us/azure/api-management/  
**license:** CC-BY-4.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-468-R1",
  "sourceIds": ["EXT-468"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Azure microservice platform (8 services on AKS) needs an API gateway for: JWT validation, rate limiting, request transformation, API versioning, and developer portal with API keys for external partners.",
    "b": "Option A: Azure API Management (APIM). Option B: nginx Ingress + custom JWT validation middleware in each service. Option C: Kong Gateway on AKS.",
    "c": "Platform team is small (3 engineers). External partners number 20. Budget allows for APIM Standard tier (~$700/month)."
  },
  "function": "Compare APIM vs nginx Ingress vs Kong for this scenario and recommend.",
  "acceptedOutput": "**Recommendation: Azure API Management (APIM) Standard tier.**\n\n**Decision factors:**\n- External partners requiring API keys → APIM has a built-in developer portal with self-service API key provisioning. Building this with nginx or Kong requires significant custom work.\n- Small platform team (3 engineers) → APIM is fully managed; no patching, upgrades, or capacity planning.\n- 20 partners → APIM subscription management, usage reporting, and throttling per-partner are built in.\n\n**APIM policy for JWT validation + rate limiting:**\n```xml\n<!-- APIM inbound policy applied to all APIs -->\n<inbound>\n    <!-- JWT validation — no code in services needed -->\n    <validate-jwt header-name=\"Authorization\" failed-validation-httpcode=\"401\">\n        <openid-config url=\"https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration\" />\n        <audiences><audience>api://myapp</audience></audiences>\n    </validate-jwt>\n\n    <!-- Rate limit per partner subscription key -->\n    <rate-limit-by-key calls=\"1000\" renewal-period=\"60\"\n                       counter-key=\"@(context.Subscription.Id)\" />\n\n    <!-- Route to AKS service via private endpoint -->\n    <set-backend-service base-url=\"http://orders-svc.default.svc.cluster.local\" />\n</inbound>\n```\n\n**Trade-offs:**\n| | APIM Standard | nginx + middleware | Kong |\n|---|---|---|---|\n| Developer portal | Built-in | Build from scratch | Plugin (extra cost) |\n| JWT validation | Policy (no code) | Per-service middleware | Plugin |\n| Monthly cost | ~$700 | ~$50 (nginx pods) | ~$200 (OSS) |\n| Ops burden | Low (managed) | Medium | Medium |\n| Latency added | ~5ms | ~1ms | ~2ms |\n\n**Verdict:** for 20 external partners, the developer portal alone justifies APIM over the alternatives. The $700/month cost is offset by avoiding custom portal development.",
  "rejectedOutput": "Use nginx Ingress — it is open source, runs in your AKS cluster, and is far cheaper.",
  "arbiters": ["MICROSOFT-LEARN-ARCH", "AZURE-APIM-DOCS"],
  "whyAccepted": "APIM's managed developer portal is the decisive feature for a 20-partner external API program. The cost comparison is honest and the latency difference is acknowledged. The policy example is concrete.",
  "whyRejected": "nginx Ingress is cheap for infrastructure but the hidden cost is building API key management, a developer portal, per-partner usage reporting, and rate limiting from scratch — on a 3-engineer team. TCO (Total Cost of Ownership) favors APIM for this scenario.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/api-management/"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-469
**registryId:** CWE-TOP25  
**unitTitle:** CWE-200 Exposure of Sensitive Information — ASP.NET Core response header audit  
**sourceUrl:** https://cwe.mitre.org/data/definitions/200.html  
**license:** CWE-TOU | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-469-R1",
  "sourceIds": ["EXT-469"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "negative-example",
  "input": {
    "a": "Security scan of ASP.NET Core 8 API reveals response headers: `Server: Microsoft-IIS/10.0`, `X-Powered-By: ASP.NET`, `X-AspNet-Version: 4.0.30319`. These headers expose technology stack information.",
    "b": "CWE-200 (Exposure of Sensitive Information to Unauthorized Actor) — attacker uses version information to target known vulnerabilities. Example: IIS 10.0 + ASP.NET 4.0 indicates the server may be vulnerable to specific CVEs.",
    "c": "ASP.NET Core 8 hosted on IIS. Also needs: X-Content-Type-Options, X-Frame-Options, and Content-Security-Policy headers added."
  },
  "function": "Remove information-exposure headers and add security headers per CWE-200 mitigation.",
  "acceptedOutput": "**CWE-200 mitigation** requires removing server technology disclosure headers and adding protective security headers.\n\n**Step 1 — Remove disclosure headers:**\n```csharp\n// Program.cs — custom middleware to strip disclosure headers\napp.Use(async (context, next) =>\n{\n    context.Response.OnStarting(() =>\n    {\n        context.Response.Headers.Remove(\"Server\");           // removes IIS version\n        context.Response.Headers.Remove(\"X-Powered-By\");    // removes ASP.NET disclosure\n        context.Response.Headers.Remove(\"X-AspNet-Version\"); // removes framework version\n        return Task.CompletedTask;\n    });\n    await next();\n});\n```\n\n**Step 2 — IIS web.config for complete removal:**\n```xml\n<!-- web.config — remove at IIS level (more reliable than middleware) -->\n<system.webServer>\n  <httpProtocol>\n    <customHeaders>\n      <remove name=\"X-Powered-By\" />\n    </customHeaders>\n  </httpProtocol>\n  <security>\n    <requestFiltering removeServerHeader=\"true\" /> <!-- removes Server header -->\n  </security>\n</system.webServer>\n```\n\n**Step 3 — Add protective security headers:**\n```csharp\n// In the middleware or UseSecurityHeaders NuGet package\napp.Use(async (context, next) =>\n{\n    var headers = context.Response.Headers;\n    headers[\"X-Content-Type-Options\"] = \"nosniff\";      // prevent MIME sniffing\n    headers[\"X-Frame-Options\"]        = \"DENY\";          // prevent clickjacking\n    headers[\"Referrer-Policy\"]        = \"strict-origin-when-cross-origin\";\n    headers[\"Permissions-Policy\"]     = \"camera=(), microphone=(), geolocation=()\"; // deny unneeded APIs\n    // CSP should be tuned per-application — this is a starting point\n    headers[\"Content-Security-Policy\"] =\n        \"default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:\";\n    await next();\n});\n```\n\n**Verification:** `curl -I https://api.example.com` — confirm Server, X-Powered-By, X-AspNet-Version are absent.",
  "rejectedOutput": "Server version headers are harmless — attackers already know what software is common. Security through obscurity doesn't help.",
  "arbiters": ["CWE-TOP25", "OWASP-SECURITY-HEADERS"],
  "whyAccepted": "While security through obscurity is not a defense in itself, removing version disclosure reduces the signal-to-noise ratio for automated vulnerability scanners that target specific version ranges. The protective headers (X-Content-Type-Options, CSP) provide real protection, not obscurity.",
  "whyRejected": "The claim that version headers are 'harmless' is incorrect — automated scanners use exact version strings to target known CVEs. Removing them is a defense-in-depth measure that requires zero effort. The claim also conflates security through obscurity (which is insufficient) with information hygiene (which is necessary).",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/200.html"],
  "licenseConstraint": "CWE-TOU",
  "trainingStatus": "draft"
}
```

---

## EXT-470
**registryId:** OWASP-ASVS  
**unitTitle:** ASVS V7.1 log content — structured logging vs free-text for security event detection  
**sourceUrl:** https://owasp.org/www-project-application-security-verification-standard/  
**license:** CC-BY-SA-3.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-470-R1",
  "sourceIds": ["EXT-470"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "OWASP ASVS V7.1.1 requires: 'Verify that the application does not log credentials or payment details.' V7.1.2 requires: 'Verify that the application does not log other sensitive information as determined by local privacy laws or relevant security policy.'",
    "b": "Current ASP.NET Core API logs: `_logger.LogInformation($\"User {email} authenticated with password {password}\")`. SIEM (Security Information and Event Management) team cannot detect brute force attacks from free-text logs.",
    "c": "Need logs that: (1) never include credentials, (2) are structured for SIEM query, (3) include security event classification."
  },
  "function": "Design OWASP ASVS V7.1 compliant structured logging for authentication events.",
  "acceptedOutput": "**ASVS V7.1 compliance requires:** no credential logging + structured format for SIEM detection.\n\n**Serilog structured logging implementation:**\n```csharp\n// VIOLATION — current code logs password in plaintext\n_logger.LogInformation($\"User {email} authenticated with password {password}\");\n\n// CORRECT — structured logging, no credentials, security event classification\n_logger.LogInformation(\n    \"SecurityEvent: {EventType} {Outcome} for {UserId} from {IpAddress}\",\n    \"Authentication\",  // EventType — SIEM can query by event type\n    \"Success\",         // Outcome\n    userId,            // UserId — never email (PII), use internal ID\n    httpContext.Connection.RemoteIpAddress?.ToString()\n);\n\n// Failed attempt — structured for brute force detection\n_logger.LogWarning(\n    \"SecurityEvent: {EventType} {Outcome} for {Username} from {IpAddress} attempt {AttemptCount}\",\n    \"Authentication\",\n    \"Failure\",\n    usernameHash,  // hash the username — identifies the account without storing PII\n    ipAddress,\n    failedAttemptCount\n);\n```\n\n**Serilog configuration (appsettings.json):**\n```json\n{\n  \"Serilog\": {\n    \"Destructure\": [\n      // Prevent accidental credential logging via object destructuring\n      { \"Name\": \"ByTransforming\",\n        \"Args\": { \"type\": \"LoginRequest\",\n                  \"transform\": { \"Password\": \"[REDACTED]\",\n                                 \"ConfirmPassword\": \"[REDACTED]\" } } }\n    ]\n  }\n}\n```\n\n**SIEM detection query (Kusto / KQL for Azure Sentinel):**\n```kql\nSecurityLogs\n| where EventType == \"Authentication\" and Outcome == \"Failure\"\n| summarize FailCount = count() by IpAddress, bin(TimeGenerated, 5m)\n| where FailCount > 10  // brute force threshold: 10 failures in 5 minutes\n| project IpAddress, FailCount, TimeGenerated\n```\n\n**ASVS V7.1 checklist:**\n- [x] V7.1.1: no credentials in logs (password removed, username hashed)\n- [x] V7.1.2: no PII in logs (email replaced with userId/hash)\n- [x] V7.1.3: log security events with sufficient detail for SIEM detection",
  "rejectedOutput": "Use log levels correctly: log credentials at Debug level — production log level is Information so credentials won't appear in production logs.",
  "arbiters": ["OWASP-ASVS", "SERILOG-DOCS"],
  "whyAccepted": "Structured logging with explicit field names (EventType, Outcome, IpAddress) enables SIEM queries that free-text logs cannot support. Hashing the username satisfies both ASVS V7.1.2 (no PII) and brute force detection (same hash = same account). The Serilog destructure config prevents accidental credential logging via object logging.",
  "whyRejected": "Log level filtering is not a security control — anyone with access to set log level to Debug (in an incident investigation, for example) would expose credentials. ASVS V7.1.1 requires that credentials are NEVER logged at any level. The debug-level approach violates the requirement.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
