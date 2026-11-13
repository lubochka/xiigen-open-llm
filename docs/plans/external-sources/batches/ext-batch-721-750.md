# EXT Batch 25 — EXT-491 through EXT-510

## EXT-491
**registryId:** KUBERNETES-KEPS  
**unitTitle:** Kubernetes NetworkPolicy default-deny vs allow-all for service-to-service security  
**sourceUrl:** https://kubernetes.io/docs/concepts/services-networking/network-policies/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-491-R1",
  "sourceIds": ["EXT-491"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Kubernetes cluster has 8 microservices with no NetworkPolicy resources. By default, all pods can communicate with all other pods in the cluster — a compromised pod can reach any database, internal API, or message broker.",
    "b": "Security audit recommends implementing default-deny NetworkPolicy as the baseline, then allowing only required traffic.",
    "c": "CNI (Container Network Interface) plugin is Cilium. Services: orders-api, payment-api, inventory-api, postgres, redis, rabbitmq."
  },
  "function": "Implement default-deny NetworkPolicy baseline and explicit allow rules for the described services.",
  "acceptedOutput": "**Default-deny NetworkPolicy** closes all pod-to-pod communication by default, then explicit policies open only required connections — implementing least-privilege networking.\n\n**Step 1 — Default deny all ingress and egress per namespace:**\n```yaml\n# applied to each namespace containing microservices\napiVersion: networking.k8s.io/v1\nkind: NetworkPolicy\nmetadata:\n  name: default-deny-all\n  namespace: production\nspec:\n  podSelector: {}  # matches ALL pods in namespace\n  policyTypes:\n  - Ingress\n  - Egress\n  # No ingress/egress rules = deny all\n```\n\n**Step 2 — Allow required traffic explicitly:**\n```yaml\n# orders-api: accept traffic from ingress controller, reach postgres and rabbitmq\napiVersion: networking.k8s.io/v1\nkind: NetworkPolicy\nmetadata:\n  name: orders-api-policy\n  namespace: production\nspec:\n  podSelector:\n    matchLabels:\n      app: orders-api\n  policyTypes: [Ingress, Egress]\n  ingress:\n  - from:\n    - namespaceSelector:\n        matchLabels:\n          kubernetes.io/metadata.name: ingress-nginx\n    ports:\n    - port: 8080\n  egress:\n  - to:\n    - podSelector: { matchLabels: { app: postgres } }\n    ports: [{ port: 5432 }]\n  - to:\n    - podSelector: { matchLabels: { app: rabbitmq } }\n    ports: [{ port: 5672 }]\n  - to:  # allow DNS resolution\n    - namespaceSelector:\n        matchLabels:\n          kubernetes.io/metadata.name: kube-system\n    ports: [{ port: 53, protocol: UDP }]\n```\n\n**Payment-api → only reaches payment-gateway (external) and postgres:**\n```yaml\n  egress:\n  - to: [{ podSelector: { matchLabels: { app: postgres } } }]\n    ports: [{ port: 5432 }]\n  - ports: [{ port: 443 }]  # HTTPS to external payment gateway\n```\n\n**Result:** compromised orders-api pod cannot reach payment-api, redis, or inventory-api — lateral movement is blocked. Blast radius of any pod compromise is limited to its explicitly allowed connections.\n\n**Cilium note:** Cilium supports L7 NetworkPolicy (allow specific HTTP methods/paths) for finer-grained control beyond port-level.",
  "rejectedOutput": "Use Istio mTLS instead of NetworkPolicy — it provides stronger security at the service mesh level.",
  "arbiters": ["KUBERNETES-DOCS", "CIS-BENCHMARK"],
  "whyAccepted": "NetworkPolicy default-deny is the Kubernetes-native, CNI-level defense-in-depth layer. It operates at L3/L4 and blocks TCP connections regardless of application behavior. The explicit egress rules implement least-privilege networking.",
  "whyRejected": "Istio mTLS and NetworkPolicy are complementary, not alternatives. mTLS provides mutual authentication between services (L7); NetworkPolicy provides network-level access control (L3/L4). A compromised pod with a valid mTLS certificate can still reach unauthorized services if NetworkPolicy is absent.",
  "sourceAnchors": ["https://kubernetes.io/docs/concepts/services-networking/network-policies/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-492
**registryId:** BIGCODEBENCH  
**unitTitle:** Python contextlib.contextmanager vs class-based context manager for resource cleanup  
**sourceUrl:** https://github.com/bigcode-project/bigcodebench  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-492-R1",
  "sourceIds": ["EXT-492"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Python service creates a temporary directory for file processing, must clean it up on success AND failure, and needs to track the directory path during processing. Developer is choosing between `@contextlib.contextmanager` and a class with `__enter__`/`__exit__`.",
    "b": "The cleanup logic must run even if an exception is raised inside the `with` block.",
    "c": "Python 3.11. The context manager will be used in 15 places across the codebase."
  },
  "function": "Compare @contextmanager vs class-based context manager and implement the correct choice for temporary directory management.",
  "acceptedOutput": "**Use `@contextlib.contextmanager` for simple linear setup/teardown** — it is more readable and less boilerplate than a class when the context manager logic flows naturally as a generator.\n\n**@contextmanager implementation:**\n```python\nimport shutil\nimport tempfile\nfrom contextlib import contextmanager\nfrom pathlib import Path\n\n@contextmanager\ndef temp_processing_dir(prefix: str = 'proc_'):\n    \"\"\"Create temp directory, yield path, clean up on exit regardless of exceptions.\"\"\"\n    tmpdir = Path(tempfile.mkdtemp(prefix=prefix))\n    try:\n        yield tmpdir          # execution enters the `with` block here\n    finally:\n        shutil.rmtree(tmpdir, ignore_errors=True)  # runs on success AND exception\n\n# Usage — identical to class-based interface\nwith temp_processing_dir('order_') as tmpdir:\n    process_files(tmpdir)  # exception here → finally still runs → dir cleaned up\n    move_to_output(tmpdir)\n```\n\n**Class-based context manager (equivalent, more verbose):**\n```python\nclass TempProcessingDir:\n    def __init__(self, prefix: str = 'proc_'):\n        self._prefix = prefix\n        self.path: Path | None = None\n\n    def __enter__(self) -> Path:\n        self.path = Path(tempfile.mkdtemp(prefix=self._prefix))\n        return self.path\n\n    def __exit__(self, exc_type, exc_val, exc_tb) -> bool:\n        if self.path:\n            shutil.rmtree(self.path, ignore_errors=True)\n        return False  # don't suppress exceptions\n```\n\n**Decision rule:**\n- Use `@contextmanager` when: linear setup → yield → teardown, no need to store state between `__enter__` and `__exit__`, no inheritance.\n- Use class when: context manager needs to be reusable (call `__enter__`/`__exit__` multiple times), needs to inherit from a base class, or has complex state that benefits from clear attribute naming.\n\n**Note:** `tempfile.TemporaryDirectory()` is already a class-based context manager in the standard library — for most cases, `with tempfile.TemporaryDirectory() as tmpdir:` is sufficient.",
  "rejectedOutput": "Use a try/finally block directly — context managers add unnecessary abstraction for simple file cleanup.",
  "arbiters": ["PYTHON-DOCS", "BIGCODEBENCH"],
  "whyAccepted": "@contextmanager encapsulates the try/finally cleanup pattern in a reusable, named abstraction. Used in 15 places, centralizing cleanup logic eliminates 15 separate try/finally blocks that could each independently miss cleanup on a code path.",
  "whyRejected": "A direct try/finally block in each of 15 call sites duplicates cleanup logic — any change to cleanup behavior (e.g., adding logging) requires 15 updates. Abstraction is justified when there are 3+ identical patterns.",
  "sourceAnchors": ["https://docs.python.org/3/library/contextlib.html#contextlib.contextmanager"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-493
**registryId:** SWE-BENCH  
**unitTitle:** FastAPI dependency injection for request-scoped resources vs global singleton  
**sourceUrl:** https://github.com/princeton-nlp/SWE-bench  
**license:** MIT | **trustTier:** 2  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-493-R1",
  "sourceIds": ["EXT-493"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "FastAPI service uses a global SQLAlchemy Session created at module load time: `db = SessionLocal()`. All request handlers use this single session. Under load, concurrent requests corrupt each other's database state — phantom reads, wrong data returned.",
    "b": "Root cause: SQLAlchemy Session is not thread-safe. A single session shared across requests causes state corruption.",
    "c": "FastAPI. SQLAlchemy 2.0 async. PostgreSQL."
  },
  "function": "Identify the global session anti-pattern and implement per-request session using FastAPI dependency injection.",
  "acceptedOutput": "**Anti-pattern:** global SQLAlchemy Session — sessions are not thread-safe and track object state internally. A shared session causes:\n- Transaction contamination: request A's uncommitted changes visible to request B\n- Object identity conflicts: same DB row loaded twice as different Python objects\n- Connection pool exhaustion: one session holds a connection for the app lifetime\n\n**Fix: FastAPI dependency injection for per-request async session:**\n```python\n# database.py\nfrom sqlalchemy.ext.asyncio import AsyncSession, create_async_engine, async_sessionmaker\n\nengine = create_async_engine(\n    \"postgresql+asyncpg://user:pass@localhost/db\",\n    pool_size=10,\n    max_overflow=20,\n)\nAsyncSessionLocal = async_sessionmaker(\n    engine, expire_on_commit=False, class_=AsyncSession\n)\n\n# Dependency — yields a new session per request, closes on completion\nasync def get_db() -> AsyncGenerator[AsyncSession, None]:\n    async with AsyncSessionLocal() as session:\n        try:\n            yield session         # request handler runs here\n            await session.commit()  # auto-commit on clean exit\n        except Exception:\n            await session.rollback()  # auto-rollback on exception\n            raise\n\n# routers/orders.py\nfrom fastapi import APIRouter, Depends\n\nrouter = APIRouter()\n\n@router.get('/orders/{order_id}')\nasync def get_order(\n    order_id: int,\n    db: AsyncSession = Depends(get_db)  # new session per request\n):\n    result = await db.execute(select(Order).where(Order.id == order_id))\n    return result.scalar_one_or_none()\n\n@router.post('/orders')\nasync def create_order(\n    dto: CreateOrderDto,\n    db: AsyncSession = Depends(get_db)  # same request = same session\n):\n    order = Order(**dto.model_dump())\n    db.add(order)\n    # session.commit() called automatically in get_db on clean exit\n    return order\n```\n\n**Result:** each request gets an isolated session. Transactions commit on clean exit, rollback on exception. Connection returned to pool immediately after request completes.",
  "rejectedOutput": "Add threading.Lock() around all session operations to make the global session thread-safe.",
  "arbiters": ["SQLALCHEMY-DOCS", "FASTAPI-DOCS"],
  "whyAccepted": "Per-request session injection is the FastAPI + SQLAlchemy canonical pattern. Each request has transaction isolation, automatic commit/rollback, and returns the connection to the pool promptly.",
  "whyRejected": "A lock around all session operations serializes all database access — concurrent requests queue behind the lock, eliminating the performance benefit of async. It also does not fix the transaction isolation problem (request A's data still visible to request B within the same session).",
  "sourceAnchors": ["https://fastapi.tiangolo.com/tutorial/sql-databases/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-494
**registryId:** PYTHON-PEPS  
**unitTitle:** PEP-557 dataclass field() factory vs default for mutable default values  
**sourceUrl:** https://peps.python.org/pep-0557/  
**license:** PSF-2.0 | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-494-R1",
  "sourceIds": ["EXT-494"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-execution-validation",
  "rowClass": "negative-example",
  "input": {
    "a": "Python dataclass with a mutable default: `@dataclass class Order: tags: list = []`. Python raises `ValueError: mutable default <class 'list'> for field tags is not allowed: use default_factory`. Developer doesn't understand why.",
    "b": "In a regular class, `tags = []` as a class attribute would share the same list across all instances — a classic Python bug. Dataclass explicitly disallows this pattern.",
    "c": "Need: each Order instance has its own independent tags list, starting empty."
  },
  "function": "Explain the mutable default bug and implement the correct dataclass field() with default_factory.",
  "acceptedOutput": "**Root cause:** mutable defaults are shared across all instances in Python class definitions — not just dataclasses. This is a Python language behavior.\n\n**The bug (without dataclass protection):**\n```python\n# Regular class — BUG: all instances share the same list object\nclass BuggyOrder:\n    tags: list = []  # ONE list object, shared by ALL instances\n\na = BuggyOrder()\nb = BuggyOrder()\na.tags.append('urgent')\nprint(b.tags)  # ['urgent'] — b sees a's data!\n```\n\n**Dataclass raises ValueError explicitly** to prevent this bug at class definition time — one of the safety features of `@dataclass`.\n\n**Fix: field() with default_factory:**\n```python\nfrom dataclasses import dataclass, field\n\n@dataclass\nclass Order:\n    id: int\n    # field() calls list() for EACH new instance — unique list per instance\n    tags: list[str] = field(default_factory=list)\n    metadata: dict[str, str] = field(default_factory=dict)\n\n    # For complex defaults, use a lambda or factory function:\n    config: dict = field(default_factory=lambda: {'priority': 'normal', 'retry': 3})\n\na = Order(id=1)\nb = Order(id=2)\na.tags.append('urgent')\nprint(b.tags)  # [] — b has its own list, unaffected\n```\n\n**Immutable defaults are fine without field():**\n```python\n@dataclass\nclass Order:\n    status: str = 'pending'   # OK — strings are immutable\n    priority: int = 1          # OK — ints are immutable\n    tags: tuple = ()           # OK — tuples are immutable\n```\n\n**Rule:** any mutable default (list, dict, set, custom object) must use `field(default_factory=...)`.",
  "rejectedOutput": "Use `tags: list = None` as the default and set it to `[]` in `__post_init__`.",
  "arbiters": ["PYTHON-PEPS", "PYTHON-DOCS"],
  "whyAccepted": "field(default_factory=list) is the PEP-557 specified solution. It is the idiomatic, readable approach that eliminates the shared-mutable-default bug with zero boilerplate.",
  "whyRejected": "`None` default + `__post_init__` assignment works but requires the type annotation to be `list | None` (or Optional[list]), which is semantically incorrect — Order.tags is always a list, never None. It also adds boilerplate that `default_factory` makes unnecessary.",
  "sourceAnchors": ["https://peps.python.org/pep-0557/#mutable-default-values"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-495
**registryId:** OPENTELEMETRY-SPEC  
**unitTitle:** OTel baggage propagation for cross-service request context vs custom headers  
**sourceUrl:** https://opentelemetry.io/docs/specs/otel/baggage/api/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-495-R1",
  "sourceIds": ["EXT-495"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-evidence-linkage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Multi-service system needs to propagate `userId` and `tenantId` across service boundaries for: (1) logging correlation — each downstream service logs with the userId, (2) feature flag evaluation — each service checks feature flags for the tenant.",
    "b": "Current approach: each HTTP client adds `X-User-Id` and `X-Tenant-Id` custom headers manually. Some services forget to forward them, breaking the correlation.",
    "c": "System uses OTel for distributed tracing. W3C TraceContext propagation is configured."
  },
  "function": "Compare OTel Baggage vs custom headers for cross-service context propagation and implement Baggage-based solution.",
  "acceptedOutput": "**OTel Baggage** is the standardized mechanism for propagating application-level key-value pairs alongside trace context — automatically forwarded by OTel instrumentation on every outbound HTTP call.\n\n**Advantage over custom headers:** Baggage is automatically forwarded by OTel HttpClient instrumentation — services don't manually add headers. OTel adds them as part of the W3C `baggage` header.\n\n**Implementation (.NET OTel SDK):**\n```csharp\n// At the API gateway / entry point — set Baggage from JWT claims:\napp.Use(async (ctx, next) =>\n{\n    if (ctx.User.Identity?.IsAuthenticated == true)\n    {\n        var userId   = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);\n        var tenantId = ctx.User.FindFirstValue(\"tenant_id\");\n\n        // OTel Baggage — propagated automatically to all downstream services\n        Baggage.SetBaggage(\"user.id\",    userId ?? \"\");\n        Baggage.SetBaggage(\"tenant.id\",  tenantId ?? \"\");\n    }\n    await next();\n});\n\n// In downstream services — read Baggage without any manual header extraction:\npublic class OrderService\n{\n    private readonly ILogger<OrderService> _logger;\n\n    public async Task ProcessAsync()\n    {\n        var userId   = Baggage.GetBaggage(\"user.id\");\n        var tenantId = Baggage.GetBaggage(\"tenant.id\");\n\n        // Log with correlation — userId is available without header extraction\n        _logger.LogInformation(\n            \"Processing order for user {UserId} tenant {TenantId}\",\n            userId, tenantId);\n\n        // Feature flag evaluation\n        var showNewUI = _featureFlags.IsEnabled(\"new-checkout\", tenantId);\n    }\n}\n```\n\n**W3C baggage header format (auto-injected by OTel):**\n`baggage: user.id=abc123,tenant.id=tenant456`\n\n**Security consideration:** Baggage is readable by all services in the call chain and is visible in HTTP logs. Do NOT propagate secrets (passwords, tokens) in Baggage — only identifiers.\n\n**When to keep custom headers:** services not in the OTel ecosystem (legacy, non-.NET) that cannot read Baggage automatically.",
  "rejectedOutput": "Add userId and tenantId to the trace span attributes — they will be visible in Jaeger.",
  "arbiters": ["OPENTELEMETRY-SPEC", "W3C-BAGGAGE"],
  "whyAccepted": "OTel Baggage is the specified mechanism for cross-service propagation of application context. It is automatically forwarded by OTel instrumentation, eliminating the manual forwarding problem. The security note about not propagating secrets is essential.",
  "whyRejected": "Span attributes are stored in the trace backend (Jaeger) but not propagated to downstream services at runtime. Service B cannot read Service A's span attributes during request processing — they are only visible in post-hoc trace analysis. Baggage propagates at request time.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/baggage/api/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-496
**registryId:** DAPPER  
**unitTitle:** Dapper custom type handler for value objects — Money type SQL mapping  
**sourceUrl:** https://github.com/DapperLib/Dapper  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-496-R1",
  "sourceIds": ["EXT-496"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Domain model has a `Money` value object: `record Money(decimal Amount, string Currency)`. Database stores money as two columns: `amount DECIMAL(18,4)` and `currency CHAR(3)`. Dapper cannot map a single SQL column to a composite C# type.",
    "b": "Current workaround: query maps to a flat DTO then manually constructs Money objects. This adds mapping code at every call site.",
    "c": "Dapper supports `SqlMapper.TypeHandler<T>` for custom type mapping."
  },
  "function": "Implement a Dapper TypeHandler for the Money value object to eliminate per-call-site mapping.",
  "acceptedOutput": "**Dapper TypeHandler** is designed for mapping between SQL column values and C# types. However, Money maps FROM two columns — TypeHandler handles one column. The correct Dapper approach for multi-column value objects is multi-mapping.\n\n**Option 1 — Dapper multi-mapping (recommended for 2-column value object):**\n```csharp\n// SQL: SELECT o.Id, o.Amount, o.Currency FROM Orders o\n// Map Amount + Currency → Money in the multi-map callback\n\npublic async Task<IEnumerable<Order>> GetOrdersAsync()\n{\n    const string sql = @\"\n        SELECT o.Id, o.CustomerId,\n               o.Amount, o.Currency  -- Money columns last, splitOn='Amount'\n        FROM Orders o\";\n\n    await using var conn = await _connectionFactory.CreateAsync();\n    return await conn.QueryAsync<OrderRow, Money, Order>(\n        sql,\n        map: (row, money) => new Order(row.Id, row.CustomerId, money),\n        splitOn: \"Amount\"  // Dapper starts new Money type at Amount column\n    );\n}\n\n// Money as a mappable POCO (for Dapper multi-mapping):\npublic record Money(decimal Amount, string Currency);\n```\n\n**Option 2 — TypeHandler for JSON-encoded Money (single column approach):**\n```csharp\n// If Money is stored as JSON: '{\"Amount\":99.99,\"Currency\":\"USD\"}'\npublic class MoneyTypeHandler : SqlMapper.TypeHandler<Money>\n{\n    public override Money Parse(object value)\n        => JsonSerializer.Deserialize<Money>((string)value)!\n           ?? throw new InvalidCastException(\"Cannot parse Money from null\");\n\n    public override void SetValue(IDbDataParameter parameter, Money value)\n    {\n        parameter.Value = JsonSerializer.Serialize(value);\n        parameter.DbType = DbType.String;\n    }\n}\n\n// Register once at startup:\nSqlMapper.AddTypeHandler(new MoneyTypeHandler());\n```\n\n**Recommendation:** Use multi-mapping (Option 1) for two-column storage — preserves queryability of Amount and Currency columns for SQL-level filtering. Use TypeHandler (Option 2) only if schema stores Money as JSON.",
  "rejectedOutput": "Use a flat DTO everywhere — Dapper is for simple mapping, complex value objects belong in EF Core.",
  "arbiters": ["DAPPER-DOCS"],
  "whyAccepted": "Both approaches eliminate per-call-site mapping. Multi-mapping is preferred for two-column storage because it keeps Amount and Currency as queryable SQL columns. TypeHandler is correct for single-column JSON storage.",
  "whyRejected": "Flat DTOs require conversion at every call site — the original problem. The tool choice (Dapper vs EF Core) should be driven by requirements, not by the complexity of value object mapping. Dapper's multi-mapping and TypeHandler are designed exactly for this.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#type-handlers"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-497
**registryId:** ANGULAR-DOCS  
**unitTitle:** Angular control flow @if @for vs *ngIf *ngFor structural directives  
**sourceUrl:** https://angular.dev/guide/templates/control-flow  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-497-R1",
  "sourceIds": ["EXT-497"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "migration-decision",
  "input": {
    "a": "Angular 17 introduced built-in control flow syntax: `@if`, `@for`, `@switch` as template-native alternatives to `*ngIf`, `*ngFor`, `*ngSwitch` structural directives. Team is migrating from Angular 15 to 17.",
    "b": "Old code: `<div *ngIf=\"user; else loading\">`, `<li *ngFor=\"let item of items; trackBy: trackById\">`. Developer wants to know if migration is required and what changes.",
    "c": "Angular 17. Standalone components."
  },
  "function": "Compare new @if/@for control flow vs *ngIf/*ngFor and provide migration patterns.",
  "acceptedOutput": "**Angular 17 built-in control flow** is a template syntax improvement — not a breaking change. `*ngIf`/`*ngFor` continue to work but are recommended to migrate for performance (no directive overhead) and readability.\n\n**Migration patterns:**\n\n**@if / @else (replaces *ngIf with else template):**\n```html\n<!-- Before (*ngIf with else template ref) -->\n<div *ngIf=\"user; else loadingTemplate\">\n  <h1>{{ user.name }}</h1>\n</div>\n<ng-template #loadingTemplate>\n  <app-spinner />\n</ng-template>\n\n<!-- After (@if @else — no ng-template needed) -->\n@if (user) {\n  <h1>{{ user.name }}</h1>\n} @else {\n  <app-spinner />\n}\n\n<!-- @else if also supported: -->\n@if (status === 'loading') { <app-spinner /> }\n@else if (status === 'error') { <app-error /> }\n@else { <app-content /> }\n```\n\n**@for (replaces *ngFor — track is REQUIRED, not optional):**\n```html\n<!-- Before (*ngFor with trackBy — optional but recommended) -->\n<li *ngFor=\"let item of items; trackBy: trackById\">{{ item.name }}</li>\n\n<!-- After (@for — track expression is REQUIRED) -->\n@for (item of items; track item.id) {\n  <li>{{ item.name }}</li>\n} @empty {\n  <li>No items found</li>  <!-- @empty: no *ngIf needed for empty state -->\n}\n```\n\n**@switch (replaces [ngSwitch]):**\n```html\n<!-- After -->\n@switch (color) {\n  @case ('red')   { <app-red-theme /> }\n  @case ('blue')  { <app-blue-theme /> }\n  @default        { <app-default-theme /> }\n}\n```\n\n**Performance:** `@for` requires `track` — enforces the pattern that was optional in `*ngFor`. The built-in control flow avoids creating a structural directive component wrapper, reducing overhead.\n\n**Migration CLI:**\n```bash\nng generate @angular/core:control-flow  # automated migration for the project\n```\n\n**Standalone note:** new syntax does NOT require `CommonModule` import — `*ngIf`/`*ngFor` require `CommonModule` or `NgIf`/`NgFor` standalone imports. Built-in control flow is template-native.",
  "rejectedOutput": "Stay with *ngIf/*ngFor — they are stable and the new syntax is just syntactic sugar.",
  "arbiters": ["ANGULAR-DOCS"],
  "whyAccepted": "The new control flow is not just syntactic sugar — it requires `track` in @for (enforcing the performance-critical trackBy pattern), has a built-in @empty block (eliminating a common *ngIf pattern), and removes the CommonModule import requirement. These are functional improvements.",
  "whyRejected": "While *ngIf/*ngFor continue to work, characterizing the new control flow as 'just syntactic sugar' understates the differences. The `track` requirement and @empty block are semantically meaningful, not cosmetic.",
  "sourceAnchors": ["https://angular.dev/guide/templates/control-flow"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-498
**registryId:** REACT-ADVANCED  
**unitTitle:** React error boundaries vs try/catch for async rendering error handling  
**sourceUrl:** https://react.dev/reference/react/Component#catching-rendering-errors-with-an-error-boundary  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-498-R1",
  "sourceIds": ["EXT-498"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-result-selection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "React component fetches user data and renders it. When the API returns an error, the component throws during render, crashing the entire React tree — white screen of death. Developer adds a try/catch around the render return statement.",
    "b": "The try/catch approach doesn't work — React rendering errors still propagate up. Developer is confused about why try/catch fails in React render.",
    "c": "React 18. Using React.Suspense for data loading."
  },
  "function": "Explain why try/catch cannot catch React render errors and implement the correct Error Boundary pattern.",
  "acceptedOutput": "**Why try/catch doesn't work in React render:** React calls component functions synchronously and propagates errors through its own internal fiber tree — NOT through JavaScript's call stack that try/catch intercepts. Errors thrown during render are caught by React's reconciler, not by any try/catch in the render function itself.\n\n**Error Boundaries** are class components that implement `componentDidCatch` — React's dedicated mechanism for catching render errors in a component subtree.\n\n**Error Boundary implementation (class component — required by React):**\n```tsx\ninterface ErrorBoundaryProps {\n  children: React.ReactNode;\n  fallback: React.ReactNode;\n  onError?: (error: Error, info: React.ErrorInfo) => void;\n}\n\ninterface ErrorBoundaryState {\n  hasError: boolean;\n  error: Error | null;\n}\n\nexport class ErrorBoundary extends React.Component<ErrorBoundaryProps, ErrorBoundaryState> {\n  constructor(props: ErrorBoundaryProps) {\n    super(props);\n    this.state = { hasError: false, error: null };\n  }\n\n  static getDerivedStateFromError(error: Error): ErrorBoundaryState {\n    return { hasError: true, error };\n  }\n\n  componentDidCatch(error: Error, info: React.ErrorInfo): void {\n    // Log to error tracking (Sentry, etc.)\n    this.props.onError?.(error, info);\n    console.error('Render error:', error, info.componentStack);\n  }\n\n  render() {\n    if (this.state.hasError) {\n      return this.props.fallback;\n    }\n    return this.props.children;\n  }\n}\n\n// Usage — wrap component trees that might fail:\nfunction App() {\n  return (\n    <ErrorBoundary\n      fallback={<ErrorPage message=\"Something went wrong. Please refresh.\" />}\n      onError={(err) => Sentry.captureException(err)}\n    >\n      <Suspense fallback={<LoadingSpinner />}>\n        <UserDashboard />\n      </Suspense>\n    </ErrorBoundary>\n  );\n}\n```\n\n**react-error-boundary library** provides a functional wrapper:\n```tsx\nimport { ErrorBoundary } from 'react-error-boundary';\n<ErrorBoundary FallbackComponent={ErrorFallback} onError={logError}>\n  <UserDashboard />\n</ErrorBoundary>\n```\n\n**Limitation:** Error Boundaries do NOT catch: event handler errors (use try/catch in handlers), async errors (use try/catch in async functions), server-side rendering errors.",
  "rejectedOutput": "Wrap the async fetch in try/catch and set an error state — then conditionally render an error message.",
  "arbiters": ["REACT-DOCS"],
  "whyAccepted": "Error Boundary is the React-specified mechanism for catching render errors. The explanation of WHY try/catch fails (React fiber tree vs JS call stack) is the non-obvious insight that prevents future misapplication.",
  "whyRejected": "try/catch in async fetch + error state correctly handles fetch errors, but does NOT handle render errors — if UserDashboard throws during render (e.g., accessing undefined.name), try/catch in the fetch function doesn't help. Error Boundary is needed for render-time failures.",
  "sourceAnchors": ["https://react.dev/reference/react/Component#catching-rendering-errors-with-an-error-boundary"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-499
**registryId:** ASPNETCORE-DOCS-SAMPLES  
**unitTitle:** ASP.NET Core rate limiting by IP vs by user for API abuse prevention  
**sourceUrl:** https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-499-R1",
  "sourceIds": ["EXT-499"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "ASP.NET Core 8 API needs rate limiting. Two strategies: (A) limit by IP address — 100 requests/minute per IP, (B) limit by authenticated user ID — 1000 requests/hour per user. Some users are behind NAT (Network Address Translation) sharing one IP.",
    "b": ".NET 7+ includes `Microsoft.AspNetCore.RateLimiting` middleware with FixedWindow, SlidingWindow, TokenBucket, and Concurrency policies.",
    "c": "API serves both anonymous (login endpoint) and authenticated (data endpoints) traffic."
  },
  "function": "Design a combined rate limiting strategy: IP-based for anonymous endpoints, user-based for authenticated endpoints.",
  "acceptedOutput": "**Combined strategy:** IP-based rate limiting for unauthenticated endpoints (prevents credential stuffing), user-based for authenticated endpoints (prevents API abuse per customer, solves NAT problem).\n\n```csharp\n// Program.cs\nbuilder.Services.AddRateLimiter(opts =>\n{\n    // Policy 1: IP-based for login/register (anonymous)\n    opts.AddPolicy(\"login-policy\", context =>\n        RateLimitPartition.GetFixedWindowLimiter(\n            // Use X-Forwarded-For if behind reverse proxy; fall back to RemoteIpAddress\n            partitionKey: context.Connection.RemoteIpAddress?.ToString()\n                          ?? context.Request.Headers[\"X-Forwarded-For\"].ToString(),\n            factory: _ => new FixedWindowRateLimiterOptions\n            {\n                PermitLimit = 10,              // 10 login attempts\n                Window = TimeSpan.FromMinutes(15), // per 15-minute window\n                QueueLimit = 0,                // no queuing — reject immediately\n            }));\n\n    // Policy 2: user-based for authenticated endpoints\n    opts.AddPolicy(\"api-policy\", context =>\n    {\n        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);\n        if (userId is null)\n        {\n            // Unauthenticated request to authenticated endpoint — reject\n            return RateLimitPartition.GetNoLimiter(string.Empty); // auth middleware handles this\n        }\n        return RateLimitPartition.GetTokenBucketLimiter(\n            partitionKey: userId,\n            factory: _ => new TokenBucketRateLimiterOptions\n            {\n                TokenLimit = 1000,                     // burst up to 1000\n                ReplenishmentPeriod = TimeSpan.FromHours(1),\n                TokensPerPeriod = 1000,                // replenish 1000/hour\n                QueueLimit = 0,\n            });\n    });\n\n    // Default response for rate limit exceeded\n    opts.OnRejected = async (ctx, ct) =>\n    {\n        ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;\n        ctx.HttpContext.Response.Headers.RetryAfter =\n            ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)\n                ? retryAfter.TotalSeconds.ToString(\"0\")\n                : \"60\";\n        await ctx.HttpContext.Response.WriteAsync(\"Too Many Requests\", ct);\n    };\n});\n\n// Apply policies per endpoint\napp.MapPost(\"/auth/login\", LoginHandler).RequireRateLimiting(\"login-policy\");\napp.MapGroup(\"/api\").RequireAuthorization().RequireRateLimiting(\"api-policy\");\n```\n\n**NAT problem solved:** authenticated user ID partitioning means all NAT-shared users have independent limits — user A's 1000 requests don't consume user B's quota.",
  "rejectedOutput": "Use IP-based rate limiting for everything — it's simpler and IP is always available.",
  "arbiters": ["ASPNETCORE-DOCS", "OWASP-API-SECURITY"],
  "whyAccepted": "The combined strategy addresses both use cases correctly. IP-based for login prevents credential stuffing (where IP is the right signal). User-based for API prevents per-customer abuse without penalizing shared-IP corporate networks.",
  "whyRejected": "IP-based only fails for NAT: a corporate network where 500 employees share 1 IP would hit rate limits constantly while a single abusive user with a dedicated IP could send 100 req/min continuously. IP is the wrong partition key for authenticated API usage.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-500
**registryId:** NESTJS-ADVANCED  
**unitTitle:** NestJS microservice transport — TCP vs RabbitMQ vs gRPC for internal service communication  
**sourceUrl:** https://docs.nestjs.com/microservices/basics  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-500-R1",
  "sourceIds": ["EXT-500"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "NestJS microservice platform needs internal service-to-service communication for: (1) synchronous request/reply (orders-api queries inventory availability before checkout), (2) async events (order placed → notifications, shipping, accounting).",
    "b": "NestJS supports multiple transports: TCP (built-in), RabbitMQ, Redis, gRPC. Team is building from scratch.",
    "c": "Expected load: 500 synchronous calls/sec for inventory checks, 200 events/sec for order events. SLA: inventory check <100ms."
  },
  "function": "Select the correct NestJS microservice transport for each communication pattern and implement.",
  "acceptedOutput": "**Pattern → Transport mapping:**\n- Synchronous request/reply with <100ms SLA → **gRPC** (HTTP/2 multiplexed, binary Protocol Buffers, low latency)\n- Async events with guaranteed delivery → **RabbitMQ** (message durability, DLQ, consumer groups)\n\n**gRPC for inventory check (synchronous):**\n```typescript\n// inventory.proto\nsyntax = \"proto3\";\nservice InventoryService {\n  rpc CheckAvailability (CheckRequest) returns (CheckResponse);\n}\nmessage CheckRequest { string productId = 1; int32 quantity = 2; }\nmessage CheckResponse { bool available = 1; int32 inStock = 2; }\n\n// inventory.service.ts (server)\n@GrpcMethod('InventoryService', 'CheckAvailability')\nasync checkAvailability(data: CheckRequest): Promise<CheckResponse> {\n  const stock = await this.stockRepo.getStock(data.productId);\n  return { available: stock >= data.quantity, inStock: stock };\n}\n\n// orders-api (client — calling inventory via gRPC)\nconst inventory = this.client.getService<InventoryServiceClient>('InventoryService');\nconst result = await firstValueFrom(\n  inventory.checkAvailability({ productId: item.id, quantity: item.qty })\n);\nif (!result.available) throw new ConflictException('Out of stock');\n```\n\n**RabbitMQ for order events (async):**\n```typescript\n// orders-api — emit event after successful checkout\nawait this.client.emit('order.placed', { orderId: order.id, total: order.total });\n\n// notifications-service — consume independently\n@MessagePattern('order.placed')\nasync onOrderPlaced(@Payload() data: { orderId: string; total: number }) {\n  await this.emailService.sendConfirmation(data.orderId);\n}\n// shipping-service and accounting-service consume the same event independently\n// via separate RabbitMQ queue bindings\n```\n\n**Transport comparison:**\n| | TCP (NestJS built-in) | gRPC | RabbitMQ |\n|---|---|---|---|\n| Latency | ~5ms | ~2ms | ~20ms (queue overhead) |\n| Pattern | Request/reply | Request/reply | Events + queuing |\n| Schema | None (JSON) | Protobuf (typed) | JSON/Protobuf |\n| Durability | None | None | Yes (durable queues) |\n| Fan-out | No | No | Yes (exchange bindings) |\n\n**Recommendation:** NestJS TCP is fine for prototyping. For production with 500 req/sec and <100ms SLA, gRPC's binary serialization and HTTP/2 multiplexing are necessary.",
  "rejectedOutput": "Use TCP transport for everything — it's built into NestJS and requires no additional infrastructure.",
  "arbiters": ["NESTJS-DOCS", "GRPC-DOCS"],
  "whyAccepted": "The pattern-to-transport mapping is driven by concrete requirements: SLA drives gRPC (low latency, typed schema), reliability drives RabbitMQ (durable queues, fan-out). The NestJS TCP transport is correctly positioned as prototyping-only.",
  "whyRejected": "NestJS TCP transport has no message durability, no fan-out to multiple consumers, and JSON serialization overhead for 500 req/sec. It is not production-grade for these requirements — the convenience of no-infrastructure-overhead is outweighed by these gaps.",
  "sourceAnchors": ["https://docs.nestjs.com/microservices/basics"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-501
**registryId:** IETF-RFCS  
**unitTitle:** RFC-7807 Problem Details (superseded by RFC-9457) — structured error response format  
**sourceUrl:** https://www.rfc-editor.org/rfc/rfc9457  
**license:** IETF-TLP | **trustTier:** 1  
**domainsTagged:** C | **familiesTagged:** 7

### Training Row

```json
{
  "rowId": "EXT-501-R1",
  "sourceIds": ["EXT-501"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C2-output-formatting",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "REST API currently returns custom error JSON: `{error: 'NOT_FOUND', message: 'Order 123 not found', code: 404}`. Different endpoints use different field names — some use `error`, some use `errorMessage`, some use `msg`. Clients must parse each endpoint's schema separately.",
    "b": "RFC-9457 (Problem Details for HTTP APIs, supersedes RFC-7807) defines a standard error response format.",
    "c": "ASP.NET Core 8 has built-in ProblemDetails support."
  },
  "function": "Implement RFC-9457 Problem Details for all API error responses in ASP.NET Core 8.",
  "acceptedOutput": "**RFC-9457 Problem Details** standardizes error responses with a defined JSON schema — clients learn one format for all API errors.\n\n**Standard fields:**\n- `type`: URI identifying the problem type (not required to dereference)\n- `title`: short human-readable summary\n- `status`: HTTP status code (duplicated in body for easy parsing)\n- `detail`: human-readable explanation specific to this occurrence\n- `instance`: URI of the specific occurrence (optional)\n\n**ASP.NET Core 8 configuration:**\n```csharp\n// Program.cs\nbuilder.Services.AddProblemDetails(opts =>\n{\n    // Customize which exceptions map to which status codes\n    opts.CustomizeProblemDetails = ctx =>\n    {\n        ctx.ProblemDetails.Extensions[\"correlationId\"] = ctx.HttpContext.TraceIdentifier;\n        ctx.ProblemDetails.Extensions[\"timestamp\"] =\n            DateTimeOffset.UtcNow.ToString(\"O\");\n    };\n});\n\nbuilder.Services.AddExceptionHandler<GlobalExceptionHandler>();\n\n// app.UseExceptionHandler() activates ProblemDetails for unhandled exceptions\napp.UseExceptionHandler();\napp.UseStatusCodePages(); // converts 404/405 to ProblemDetails automatically\n```\n\n**Throwing domain errors with typed Problem Details:**\n```csharp\n// In controller or service\npublic async Task<Order> GetOrderAsync(int id)\n{\n    var order = await _db.Orders.FindAsync(id)\n        ?? throw new ProblemDetailsException(new ProblemDetails\n        {\n            Type   = \"https://api.example.com/errors/order-not-found\",\n            Title  = \"Order not found\",\n            Status = 404,\n            Detail = $\"Order {id} does not exist or has been deleted.\",\n        });\n    return order;\n}\n\n// RFC-9457 JSON response:\n// {\n//   \"type\": \"https://api.example.com/errors/order-not-found\",\n//   \"title\": \"Order not found\",\n//   \"status\": 404,\n//   \"detail\": \"Order 123 does not exist or has been deleted.\",\n//   \"correlationId\": \"0HN3Q7R2S:00000001\",\n//   \"timestamp\": \"2026-05-04T10:00:00.000Z\"\n// }\n```\n\n**Extension fields** for domain-specific context (allowed by RFC-9457):\n```json\n{\n  \"type\": \"https://api.example.com/errors/validation-failed\",\n  \"title\": \"Validation Failed\",\n  \"status\": 422,\n  \"errors\": { \"Email\": [\"Email is required\"], \"Amount\": [\"Amount must be > 0\"] }\n}\n```",
  "rejectedOutput": "Use HTTP status codes only — the status code is the standard error response.",
  "arbiters": ["RFC-9457", "ASPNETCORE-DOCS"],
  "whyAccepted": "RFC-9457 is the IETF standard for HTTP API error responses. It provides a machine-readable `type` URI for error classification, human-readable `detail`, and allows extensions for domain-specific context. ASP.NET Core 8 has built-in support.",
  "whyRejected": "HTTP status codes convey the category (404, 422, 500) but not the actionable detail (which field failed, which resource was not found, what the client should do). RFC-9457 adds the structured context that programmatic error handling requires.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc9457"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-502
**registryId:** GOOGLE-AIP  
**unitTitle:** AIP-193 errors — using google.rpc.Status for consistent API error responses  
**sourceUrl:** https://google.aip.dev/193  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** C | **familiesTagged:** 7

### Training Row

```json
{
  "rowId": "EXT-502-R1",
  "sourceIds": ["EXT-502"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C2-output-formatting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "gRPC API has inconsistent error handling: some methods return custom error codes in the response body, some throw gRPC StatusCode.Unknown for all errors, and some return empty responses on failure. Clients cannot distinguish error types.",
    "b": "Google AIP-193 mandates using google.rpc.Status with canonical error codes for all API errors.",
    "c": "API is gRPC with HTTP/JSON transcoding. Clients are Google Cloud-style APIs."
  },
  "function": "Implement AIP-193 consistent error handling using google.rpc.Status canonical codes.",
  "acceptedOutput": "**AIP-193** requires using `google.rpc.Status` with canonical `google.rpc.Code` values — NOT custom error codes in response bodies, NOT StatusCode.Unknown for all errors.\n\n**Canonical code mapping:**\n```\ngoogle.rpc.Code.NOT_FOUND        → HTTP 404 — resource does not exist\ngoogle.rpc.Code.ALREADY_EXISTS   → HTTP 409 — resource already exists\ngoogle.rpc.Code.INVALID_ARGUMENT → HTTP 400 — client-provided argument is invalid\ngoogle.rpc.Code.PERMISSION_DENIED → HTTP 403 — authenticated but not authorized\ngoogle.rpc.Code.UNAUTHENTICATED  → HTTP 401 — not authenticated\ngoogle.rpc.Code.RESOURCE_EXHAUSTED → HTTP 429 — rate limited\ngoogle.rpc.Code.INTERNAL         → HTTP 500 — unexpected server error\n```\n\n**Implementation (.NET gRPC with Grpc.StatusProto):**\n```csharp\npublic override async Task<Order> GetOrder(GetOrderRequest request, ServerCallContext ctx)\n{\n    var order = await _db.Orders.FindAsync(request.OrderId);\n    if (order is null)\n    {\n        // AIP-193: NOT_FOUND with ResourceInfo detail\n        throw new RpcException(new Grpc.Core.Status(\n            StatusCode.NotFound,\n            $\"Order {request.OrderId} not found\"),\n            new Metadata { { \"x-goog-request-params\", $\"order_id={request.OrderId}\" } }\n        );\n        // OR using google.rpc.Status for richer detail:\n    }\n\n    if (!_authz.CanRead(ctx.GetHttpContext().User, order))\n        throw new RpcException(new Grpc.Core.Status(\n            StatusCode.PermissionDenied, \"Insufficient permissions to read order\"));\n\n    return MapToProto(order);\n}\n\n// Validation errors — INVALID_ARGUMENT with BadRequest detail:\nif (request.PageSize is < 1 or > 1000)\n{\n    var status = new Google.Rpc.Status\n    {\n        Code = (int)StatusCode.InvalidArgument,\n        Message = \"page_size must be between 1 and 1000\",\n        Details =\n        {\n            Any.Pack(new BadRequest\n            {\n                FieldViolations =\n                {\n                    new BadRequest.Types.FieldViolation\n                    {\n                        Field       = \"page_size\",\n                        Description = \"Must be between 1 and 1000\",\n                    }\n                }\n            })\n        }\n    };\n    throw status.ToRpcException();\n}\n```\n\n**Rule:** NEVER use `StatusCode.Unknown` — it means 'I don't know what went wrong'. Every error has a canonical code. `StatusCode.Internal` is the correct code for unexpected server errors.",
  "rejectedOutput": "Return error details in the response message body — it's easier for clients to parse than gRPC status metadata.",
  "arbiters": ["GOOGLE-AIP", "GRPC-STATUS-CODES"],
  "whyAccepted": "AIP-193 canonical codes enable client-side error handling by error type without parsing response bodies. The BadRequest FieldViolations detail provides actionable field-level validation feedback in a standard structure.",
  "whyRejected": "Error details in response bodies break gRPC's streaming semantics (errors mid-stream cannot be in the body), are not visible to gRPC interceptors, and bypass the standard status code mechanism that load balancers and service meshes use for circuit breaking and retry decisions.",
  "sourceAnchors": ["https://google.aip.dev/193"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-503
**registryId:** OPENASSISTANT-OASST1  
**unitTitle:** OpenAssistant DPO vs RLHF training pipeline for alignment fine-tuning  
**sourceUrl:** https://huggingface.co/datasets/OpenAssistant/oasst1  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-503-R1",
  "sourceIds": ["EXT-503"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-training-stability",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Team wants to fine-tune LLaMA-3-8B on OpenAssistant pairwise preference data for alignment. Options: (A) DPO (Direct Preference Optimization — trains directly on preference pairs without a separate reward model), (B) full RLHF pipeline (SFT → reward model → PPO).",
    "b": "Team has 2 A100 GPUs, 3 engineers, 4-week timeline. DPO requires only SFT + DPO training steps; RLHF requires SFT + reward model training + PPO.",
    "c": "Dataset: OpenAssistant oasst1 (~160K messages, pairwise preferences available)."
  },
  "function": "Evaluate DPO vs RLHF for this team's constraints and recommend with implementation approach.",
  "acceptedOutput": "**Recommendation: DPO for this team's constraints.**\n\n**Why DPO wins given 2 A100s, 3 engineers, 4 weeks:**\n- DPO eliminates the reward model training step — saves ~1 week of training + validation\n- DPO has no PPO instability (KL divergence tuning, reward hacking) — simpler to debug\n- DPO training is one step: SFT → DPO on preference pairs\n- Memory: DPO needs model + reference model in memory (2× model size) — fits on 2×A100 80GB\n\n**RLHF is better when:** you need the reward model for inference-time reranking, you have >3 months and dedicated infra team, you need fine-grained reward shaping beyond pairwise preferences.\n\n**DPO training pipeline with TRL:**\n```python\nfrom datasets import load_dataset\nfrom trl import DPOTrainer, DPOConfig\nfrom transformers import AutoModelForCausalLM, AutoTokenizer\n\n# Step 1: Load SFT-trained base model\nmodel = AutoModelForCausalLM.from_pretrained(\n    \"meta-llama/Meta-Llama-3-8B-Instruct\",  # or your SFT checkpoint\n    torch_dtype=torch.bfloat16,\n)\ntokenizer = AutoTokenizer.from_pretrained(\"meta-llama/Meta-Llama-3-8B-Instruct\")\n\n# Step 2: Prepare oasst1 preference pairs\ndataset = load_dataset(\"OpenAssistant/oasst1\", split=\"train\")\n\n# Convert oasst1 tree structure to DPO format (chosen/rejected pairs)\n# oasst1 has rank field — higher rank = preferred\ndpo_data = prepare_dpo_pairs(dataset)  # custom function: pairs by rank\n\n# Step 3: DPO training — directly optimizes preference probability\ntraining_args = DPOConfig(\n    output_dir=\"./dpo-output\",\n    num_train_epochs=1,\n    per_device_train_batch_size=2,\n    gradient_accumulation_steps=8,\n    learning_rate=5e-7,        # DPO needs much lower LR than SFT\n    beta=0.1,                  # DPO beta: KL constraint strength\n    bf16=True,\n    logging_steps=25,\n)\n\ntrainer = DPOTrainer(\n    model=model,\n    ref_model=None,  # TRL creates reference model copy automatically\n    args=training_args,\n    train_dataset=dpo_data,\n    tokenizer=tokenizer,\n)\ntrainer.train()\n```\n\n**Expected timeline:** SFT (3 days) → DPO (4 days) → evaluation (3 days) = 10 days, well within 4 weeks.",
  "rejectedOutput": "Use full RLHF — DPO is newer and less proven than PPO-based RLHF.",
  "arbiters": ["OPENASSISTANT-OASST1", "DPO-PAPER", "TRL-DOCS"],
  "whyAccepted": "DPO is proven (published 2023, widely adopted in Llama-2, Mistral fine-tuning) and specifically designed for teams without the infrastructure to run PPO. Given the resource constraints, DPO is the pragmatic and technically sound choice.",
  "whyRejected": "RLHF being 'more proven' refers to its longer history, not necessarily better outcomes — DPO achieves comparable or better alignment on standard benchmarks with less infrastructure. 'Less proven' does not mean 'worse' for a team with these specific constraints.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-504
**registryId:** TATOEBA  
**unitTitle:** Tatoeba multilingual pivot translation quality — EN-ZH via EN-FR-ZH vs direct  
**sourceUrl:** https://tatoeba.org/  
**license:** CC-BY-2.0-FR | **trustTier:** 2  
**domainsTagged:** C | **familiesTagged:** 2

### Training Row

```json
{
  "rowId": "EXT-504-R1",
  "sourceIds": ["EXT-504"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C2-translation-quality",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Translation pipeline needs EN-ZH pairs. Tatoeba has 40K direct EN-ZH pairs and 500K EN-FR pairs and 200K FR-ZH pairs. Team considers using pivot translation (EN→FR→ZH) to augment the 40K direct pairs.",
    "b": "Pivot translation introduces error compounding — errors in EN→FR are amplified in FR→ZH. BLEU score for pivot translations is 18.4 vs direct translations at 24.1.",
    "c": "Target use: training data for a business-domain ZH translation model."
  },
  "function": "Evaluate pivot translation augmentation vs direct pair quality for EN-ZH training data and design the filtering strategy.",
  "acceptedOutput": "**Pivot translation is viable as augmentation with strict quality filtering** — used directly without filtering, it degrades model performance (BLEU 18.4 vs 24.1 direct). With quality filtering, pivot pairs can supplement the 40K direct pairs for low-frequency domains.\n\n**Quality-filtered pivot augmentation strategy:**\n```python\nfrom sacrebleu.metrics import BLEU, CHRF\nfrom comet import load_from_checkpoint, download_model\n\ncomet = load_from_checkpoint(download_model('Unbabel/wmt22-comet-da'))\nbleu = BLEU(effective_order=True)\n\ndef filter_pivot_pair(en: str, zh_pivot: str, zh_direct_ref: str | None = None) -> bool:\n    \"\"\"Accept pivot EN-ZH pair only if quality meets direct pair standard.\"\"\"\n\n    # Gate 1: COMET quality estimate (reference-free, no direct ref needed)\n    comet_input = [{\"src\": en, \"mt\": zh_pivot}]\n    comet_score = comet.predict(comet_input, batch_size=16).scores[0]\n    if comet_score < 0.72:  # empirically: direct pairs average 0.78\n        return False  # reject: below direct pair quality floor\n\n    # Gate 2: if direct reference available, check BLEU floor\n    if zh_direct_ref:\n        bleu_score = bleu.sentence_score(zh_pivot, [zh_direct_ref]).score\n        if bleu_score < 15:  # rough floor — catches major divergence\n            return False\n\n    return True\n\n# Apply: keep pivot pairs with COMET >= 0.72\nfiltered_pivot = [\n    (en, zh) for en, zh in pivot_pairs\n    if filter_pivot_pair(en, zh)\n]\n\nprint(f\"Pivot pairs after filtering: {len(filtered_pivot):,} / {len(pivot_pairs):,}\")\n# Expected: ~35% pass rate → ~245K filtered pairs from 700K pivot candidates\n\n# Training mix: 40K direct + 245K filtered pivot\n# Direct pairs weighted 3:1 over pivot to preserve quality signal\n```\n\n**Training mix recommendation:** 3:1 direct:pivot weighting — direct pairs contain higher-quality signal; over-weighting pivot degrades performance.\n\n**Validation:** measure COMET and BLEU on held-out direct test set after each training configuration. Target: ≥ 90% of direct-only baseline score.",
  "rejectedOutput": "Use all pivot pairs without filtering — more data always helps translation models.",
  "arbiters": ["TATOEBA", "PIVOT-MT-PAPER"],
  "whyAccepted": "The COMET quality gate correctly identifies pivot translations that meet direct pair quality. The 3:1 weighting preserves the quality advantage of direct pairs while leveraging pivot augmentation for rare domains. The validation metric (90% of direct-only baseline) provides a concrete Gate B.",
  "whyRejected": "'More data always helps' is false for noisy data — adding 700K unfiltered pivot pairs (BLEU 18.4) to 40K direct pairs (BLEU 24.1) degrades model performance. Neural MT models learn from signal in the data; noisy signal corrupts learned representations.",
  "sourceAnchors": ["https://tatoeba.org/eng/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-505
**registryId:** VUE-DOCS  
**unitTitle:** Vue 3 Teleport for modal/tooltip rendering outside component hierarchy  
**sourceUrl:** https://vuejs.org/guide/built-ins/teleport  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-505-R1",
  "sourceIds": ["EXT-505"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Vue 3 app has a modal dialog component nested inside a CSS `overflow: hidden` container. The modal is clipped — only part of it is visible because of the ancestor's CSS. Developer is considering using `position: fixed` with high z-index.",
    "b": "Even with `position: fixed`, if any ancestor has `transform`, `perspective`, or `filter` CSS, the fixed positioning is relative to that ancestor, not the viewport — the modal is still clipped.",
    "c": "Vue 3 has the built-in `<Teleport>` component."
  },
  "function": "Identify the CSS stacking context problem and implement the correct Vue 3 Teleport solution.",
  "acceptedOutput": "**Root cause:** CSS `overflow: hidden` on an ancestor clips ALL descendants regardless of position. `position: fixed` is trapped by any ancestor with `transform`, `perspective`, or `filter` — these create a new stacking context that contains fixed positioning.\n\n**Vue 3 Teleport** moves the modal's DOM node to a different location in the HTML document (typically `<body>`) while keeping the component logic and state in its original place in the Vue component tree.\n\n**Implementation:**\n```vue\n<!-- ParentComponent.vue — modal declared inside overflowed container -->\n<template>\n  <div class=\"dashboard\" style=\"overflow: hidden; position: relative;\">\n    <DashboardContent />\n\n    <!-- Teleport renders the modal DOM into <body>, not inside .dashboard -->\n    <Teleport to=\"body\">\n      <AppModal v-if=\"showModal\" @close=\"showModal = false\">\n        <template #header>Confirm Delete</template>\n        <template #body>This action cannot be undone.</template>\n      </AppModal>\n    </Teleport>\n  </div>\n</template>\n\n<script setup lang=\"ts\">\nimport { ref } from 'vue'\nimport { Teleport } from 'vue'  // built-in — no import needed in Vue 3\nconst showModal = ref(false)\n</script>\n```\n\n**What Teleport does and doesn't change:**\n- DOM position: moved to `<body>` — no longer inside the clipped ancestor\n- Vue component tree: stays in ParentComponent — `v-if`, `v-model`, `$emit` all work normally\n- CSS: no ancestor `overflow: hidden` affects the modal DOM node\n- Scoped styles: `<style scoped>` from AppModal still applies correctly\n\n**Multiple Teleport targets:** `to=\"#modals\"` targets a specific portal div:\n```html\n<!-- index.html -->\n<div id=\"app\"></div>\n<div id=\"modals\"></div>  <!-- dedicated portal for modals -->\n```\n\n**Accessibility:** teleported modals should set `aria-modal=\"true\"` and trap focus within the modal for WCAG compliance.",
  "rejectedOutput": "Use `position: fixed` with `z-index: 9999` — it will appear above all other elements.",
  "arbiters": ["VUE-DOCS"],
  "whyAccepted": "Teleport is the correct Vue 3 solution for the CSS stacking context trap. It moves the DOM node to body, completely escaping the ancestor CSS constraints. The component tree relationship is preserved.",
  "whyRejected": "position: fixed is trapped by any ancestor with transform/perspective/filter CSS — a common pattern in animated components, card hover effects, and Material Design components. z-index cannot escape a transformed ancestor. Teleport is the structurally correct solution.",
  "sourceAnchors": ["https://vuejs.org/guide/built-ins/teleport"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-506
**registryId:** EFCORE-SOURCE  
**unitTitle:** EF Core raw SQL with FromSqlRaw vs ExecuteSqlRaw — when to use each  
**sourceUrl:** https://github.com/dotnet/efcore  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-506-R1",
  "sourceIds": ["EXT-506"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "EF Core service needs two operations: (1) a complex analytical query returning Product entities with window functions that EF Core LINQ cannot express, (2) a bulk status update `UPDATE Orders SET Status = 'Expired' WHERE ExpiresAt < @now`.",
    "b": "EF Core provides `FromSqlRaw` (returns IQueryable<T>) and `ExecuteSqlRaw` (returns rows affected count). Developer is not sure which to use for each case.",
    "c": "EF Core 8. SQL Server. Both operations must be parameterized to prevent SQL injection."
  },
  "function": "Explain the correct use of FromSqlRaw vs ExecuteSqlRaw for each operation with safe parameterization.",
  "acceptedOutput": "**Rule:**\n- `FromSqlRaw`: use when returning entities from a SELECT — returns `IQueryable<T>` that can be composed with LINQ\n- `ExecuteSqlRaw`: use for DML (INSERT, UPDATE, DELETE) or DDL — returns row count\n\n**Operation 1 — Complex SELECT with window functions (FromSqlRaw):**\n```csharp\npublic async Task<List<Product>> GetTopProductsByCategory(int topN)\n{\n    // FormattableString overload (FromSql) automatically parameterizes\n    // For raw SQL with FormattableString:\n    return await _db.Products\n        .FromSql(\n            // $\"\" creates FormattableString — values auto-parameterized\n            $\"\"\"\n            SELECT p.*, RANK() OVER (PARTITION BY p.CategoryId ORDER BY p.Sales DESC) AS Rank\n            FROM Products p\n            WHERE Rank <= {topN}\n            \"\"\"\n        )\n        .AsNoTracking()  // read-only — no change tracking overhead\n        .ToListAsync();\n    // CAUTION: FromSqlRaw (vs FromSql) does NOT auto-parameterize:\n    // .FromSqlRaw(\"SELECT ... WHERE Rank <= {0}\", topN) -- correct\n    // .FromSqlRaw($\"SELECT ... WHERE Rank <= {topN}\") -- SQL INJECTION RISK\n}\n```\n\n**Operation 2 — Bulk UPDATE (ExecuteSqlRaw):**\n```csharp\npublic async Task<int> ExpireOrdersAsync()\n{\n    // ExecuteSqlRaw: use @param syntax with SqlParameter for SQL Server\n    var now = DateTime.UtcNow;\n    return await _db.Database.ExecuteSqlRawAsync(\n        \"UPDATE Orders SET Status = 'Expired' WHERE ExpiresAt < @now\",\n        new SqlParameter(\"@now\", now)  // safe — parameterized\n    );\n    // Returns: count of rows updated\n    // CAUTION: Do NOT use string interpolation here:\n    // ExecuteSqlRaw($\"...WHERE ExpiresAt < {now}\") -- SQL INJECTION RISK\n}\n\n// Safer alternative — ExecuteSql with FormattableString (auto-parameterized):\nreturn await _db.Database.ExecuteSqlAsync(\n    $\"UPDATE Orders SET Status = 'Expired' WHERE ExpiresAt < {now}\"\n);\n// ExecuteSql (not ExecuteSqlRaw) treats the $\"\" as a parameterized template\n```\n\n**Key distinction:** `FromSql` / `ExecuteSql` (no 'Raw') accept `FormattableString` — values are automatically parameterized. `FromSqlRaw` / `ExecuteSqlRaw` treat the string literally — manual parameterization required.",
  "rejectedOutput": "Use ExecuteSqlRaw for all raw SQL — it's the more direct API.",
  "arbiters": ["EFCORE-DOCS"],
  "whyAccepted": "FromSqlRaw and ExecuteSqlRaw serve distinct purposes — SELECT with entity tracking vs DML without. The FormattableString vs raw string distinction is the critical security differentiator that prevents SQL injection.",
  "whyRejected": "ExecuteSqlRaw cannot return a typed IQueryable<T> for SELECT operations — it only returns a row count. Using it for SELECT would require abandoning EF Core entity mapping and falling back to raw ADO.NET.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/querying/sql-queries"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-507
**registryId:** DOTNET-MICROSERVICES  
**unitTitle:** .NET Aspire dashboard for local distributed tracing vs running Jaeger locally  
**sourceUrl:** https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-507-R1",
  "sourceIds": ["EXT-507"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-failure-recovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Development team runs 4 microservices locally. Currently using Jaeger (docker-compose) for distributed tracing during development. Setup requires: `docker-compose up jaeger`, configure each service's OTLP_ENDPOINT, restart services after config change.",
    "b": ".NET Aspire includes a built-in dashboard with traces, metrics, and logs — zero configuration for Aspire-managed services.",
    "c": "Team uses .NET Aspire for local orchestration. Production uses Azure Monitor + Grafana."
  },
  "function": "Compare .NET Aspire dashboard vs Jaeger for local development tracing and recommend.",
  "acceptedOutput": "**Recommendation: use .NET Aspire dashboard for local development, keep Jaeger/Grafana for production.**\n\n**.NET Aspire dashboard advantages for local dev:**\n- Zero configuration — all Aspire-managed services export OTLP to the dashboard automatically\n- No Docker container to start — dashboard runs as part of AppHost\n- Unified view: traces, metrics, AND structured logs in one UI\n- Real-time updates — no polling delay\n- Service dependency graph: visualizes which service called which\n- Available at `http://localhost:18888` by default when running AppHost\n\n**Accessing the dashboard:**\n```csharp\n// AppHost/Program.cs — no configuration needed\nvar builder = DistributedApplication.CreateBuilder(args);\n\nvar catalogApi = builder.AddProject<Projects.CatalogApi>(\"catalog\");\nvar orderApi   = builder.AddProject<Projects.OrderApi>(\"order\")\n    .WithReference(catalogApi);\n\nbuilder.Build().Run();\n// Dashboard: http://localhost:18888 — automatically captures all OTel from both services\n```\n\n**What Jaeger has that Aspire dashboard doesn't:**\n- Trace search by arbitrary tags\n- Service dependency graph for large (10+) service topologies\n- Long-term trace storage (Aspire dashboard is in-memory, lost on restart)\n- Production-grade scalability\n\n**Recommended split:**\n| Environment | Tool | Reason |\n|---|---|---|\n| Local dev | Aspire dashboard | Zero config, unified traces/metrics/logs |\n| Staging | Jaeger or Tempo | Persistent storage, team-shared |\n| Production | Azure Monitor + Grafana | Long-term retention, alerting |\n\n**Migration:** remove Jaeger from docker-compose for local dev. Keep docker-compose for services not managed by Aspire (legacy Java service, etc.).",
  "rejectedOutput": "Keep Jaeger — Aspire dashboard is just a developer preview without production features.",
  "arbiters": ["DOTNET-ASPIRE-DOCS"],
  "whyAccepted": "Aspire dashboard is stable in .NET Aspire GA (not a preview). Zero-configuration unified observability is a concrete developer experience improvement over running a separate Jaeger container. The limitation (in-memory, no long-term storage) is correctly identified and addressed by the split recommendation.",
  "whyRejected": "Characterizing the Aspire dashboard as 'developer preview' is factually incorrect — it shipped as GA with .NET Aspire. The relevant question is not production-readiness but fit-for-purpose: for local dev, Aspire dashboard is superior. For production, Azure Monitor is the correct tool.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-508
**registryId:** MICROSOFT-LEARN-ARCH  
**unitTitle:** Azure Durable Functions vs Azure Logic Apps for long-running workflow orchestration  
**sourceUrl:** https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview  
**license:** CC-BY-4.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-508-R1",
  "sourceIds": ["EXT-508"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Order fulfillment workflow: payment → inventory reserve → shipping label → email notification. Steps take 2s to 3 minutes each. Payment and shipping call external APIs. If any step fails, need to compensate (refund payment, release inventory).",
    "b": "Evaluating: (A) Azure Durable Functions (code-based orchestration), (B) Azure Logic Apps (designer-based workflow).",
    "c": "Team: 4 .NET developers, no integration specialist. Workflow has complex compensation logic (saga pattern)."
  },
  "function": "Compare Azure Durable Functions vs Logic Apps for this workflow and recommend.",
  "acceptedOutput": "**Recommendation: Azure Durable Functions** for this team and workflow.\n\n**Decision factors:**\n- Complex compensation logic (saga) → requires code-level control over error handling\n- .NET developer team → Durable Functions uses C# — same language, same IDE, same test tooling\n- External API retries with backoff → Durable Functions has built-in retry policies\n- Logic Apps designer cannot express complex conditional compensation chains without JSON ARM template editing\n\n**Durable Functions saga orchestration:**\n```csharp\n[FunctionName(\"OrderFulfillmentOrchestrator\")]\npublic static async Task RunOrchestrator(\n    [OrchestrationTrigger] IDurableOrchestrationContext ctx)\n{\n    var order = ctx.GetInput<Order>();\n    string? paymentId = null;\n    string? reservationId = null;\n\n    try\n    {\n        // Step 1 — charge payment\n        paymentId = await ctx.CallActivityWithRetryAsync<string>(\n            \"ChargePayment\", new RetryOptions(TimeSpan.FromSeconds(5), 3), order);\n\n        // Step 2 — reserve inventory\n        reservationId = await ctx.CallActivityWithRetryAsync<string>(\n            \"ReserveInventory\", new RetryOptions(TimeSpan.FromSeconds(2), 5), order);\n\n        // Step 3 — create shipping label (external API)\n        var label = await ctx.CallActivityAsync<ShippingLabel>(\"CreateShipment\", order);\n\n        // Step 4 — send confirmation email\n        await ctx.CallActivityAsync(\"SendConfirmation\", (order, label));\n    }\n    catch (Exception)\n    {\n        // Saga compensation — reverse successful steps in order\n        if (reservationId is not null)\n            await ctx.CallActivityAsync(\"ReleaseInventory\", reservationId);\n        if (paymentId is not null)\n            await ctx.CallActivityAsync(\"RefundPayment\", paymentId);\n        throw;\n    }\n}\n```\n\n**Logic Apps advantages (when to use instead):**\n- No-code/low-code team\n- Workflow primarily connects SaaS systems (Salesforce, Dynamics, SharePoint) via pre-built connectors\n- Business analysts need to modify the workflow without developer involvement\n\n**Cost:** Durable Functions: consumption-based (~$0.00001 per orchestration step). Logic Apps Standard: ~$0.00008/step. For 10K orders/day × 5 steps = ~$0.005/day (Durable) vs ~$0.04/day (Logic Apps).",
  "rejectedOutput": "Use Logic Apps — it has a visual designer that makes workflows easier to understand.",
  "arbiters": ["MICROSOFT-LEARN-ARCH", "AZURE-DURABLE-FUNCTIONS-DOCS"],
  "whyAccepted": "The compensation logic (if inventory fails, refund payment) requires code-level conditional logic that Logic Apps cannot express cleanly. Durable Functions is the correct choice for a .NET team building complex saga patterns.",
  "whyRejected": "Visual designer is a benefit for no-code teams and SaaS integration, not for a .NET team building saga compensation chains. Expressing conditional compensation in Logic Apps requires JSON template editing — less readable than C# for a developer team.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/azure-functions/durable/durable-functions-overview"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-509
**registryId:** CWE-TOP25  
**unitTitle:** CWE-502 Deserialization of Untrusted Data — .NET BinaryFormatter removal  
**sourceUrl:** https://cwe.mitre.org/data/definitions/502.html  
**license:** CWE-TOU | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-509-R1",
  "sourceIds": ["EXT-509"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "negative-example",
  "input": {
    "a": "Legacy .NET service deserializes session state from Redis using `BinaryFormatter`. Security scan flags CWE-502 (Deserialization of Untrusted Data). The Redis data could be tampered by an attacker with Redis access.",
    "b": ".NET 5+ marked BinaryFormatter as obsolete. .NET 9 removed it entirely. Any deserialization of untrusted binary data using BinaryFormatter allows arbitrary code execution via gadget chains.",
    "c": "Session data structure: `UserSession { UserId, Roles, Preferences }` — no circular references, straightforward POCO."
  },
  "function": "Identify the CWE-502 vulnerability and migrate from BinaryFormatter to a safe serializer.",
  "acceptedOutput": "**CWE-502 vulnerability:** BinaryFormatter deserializes type metadata from the binary stream — an attacker who can write to Redis can inject a malicious binary payload that executes arbitrary code during deserialization (gadget chain attack). This is a Remote Code Execution (RCE) vulnerability.\n\n**Why BinaryFormatter is unsafe:** it deserializes the type name from the stream and instantiates it via reflection — no type allowlist by default. Known gadget chains in .NET's own base class library enable RCE.\n\n**Migration to System.Text.Json:**\n```csharp\n// Before — BinaryFormatter (UNSAFE)\npublic UserSession? GetSession(byte[] data)\n{\n    using var ms = new MemoryStream(data);\n    var formatter = new BinaryFormatter(); // CWE-502 — REMOVE\n    return (UserSession?)formatter.Deserialize(ms); // arbitrary code exec risk\n}\n\n// After — System.Text.Json (safe)\nprivate static readonly JsonSerializerOptions _jsonOpts = new()\n{\n    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,\n};\n\npublic UserSession? GetSession(byte[] data)\n{\n    var json = Encoding.UTF8.GetString(data);\n    return JsonSerializer.Deserialize<UserSession>(json, _jsonOpts);\n    // System.Text.Json deserializes to the explicitly specified type only\n    // No reflection-based type instantiation from stream content\n}\n\npublic byte[] SerializeSession(UserSession session)\n    => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(session, _jsonOpts));\n```\n\n**Migration steps:**\n1. Replace BinaryFormatter serialization with System.Text.Json\n2. Clear existing Redis session keys (binary format is incompatible with JSON)\n3. Users re-authenticate to regenerate sessions in new format\n4. Add `<EnableUnsafeBinaryFormatterSerialization>false</EnableUnsafeBinaryFormatterSerialization>` to project file — throws if BinaryFormatter is used\n\n**Alternative for complex object graphs:** `MessagePack-CSharp` with explicit type registration is safe and 3× faster than JSON.\n\n**Test:** `dotnet test` with a security test that attempts to deserialize a malicious payload — should throw `NotSupportedException` or `JsonException`, not execute the payload.",
  "rejectedOutput": "Add `[Serializable]` attribute only to allowed types — this restricts what BinaryFormatter can deserialize.",
  "arbiters": ["CWE-TOP25", "DOTNET-SECURITY-DOCS"],
  "whyAccepted": "System.Text.Json is the correct, safe replacement — it deserializes only to the explicitly specified CLR type, with no reflection-based type lookup from stream content. The migration steps are concrete and include the cache-clearing step that prevents broken sessions.",
  "whyRejected": "`[Serializable]` attribute is a marker attribute, not a security control. BinaryFormatter does NOT enforce an allowlist based on `[Serializable]` — it deserializes any type that appears in the stream. The attribute does not prevent gadget chain attacks.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/502.html", "https://learn.microsoft.com/en-us/dotnet/standard/serialization/binaryformatter-security-guide"],
  "licenseConstraint": "CWE-TOU",
  "trainingStatus": "draft"
}
```

---

## EXT-510
**registryId:** OWASP-ASVS  
**unitTitle:** ASVS V9.1 data classification — PII handling and data minimization in API responses  
**sourceUrl:** https://owasp.org/www-project-application-security-verification-standard/  
**license:** CC-BY-SA-3.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-510-R1",
  "sourceIds": ["EXT-510"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "OWASP ASVS V9.1 requires data classification and data minimization. API endpoint `GET /users/{id}` returns the full User entity including: hashedPassword, salt, resetToken, internalNotes, createdByAdminId. Clients only need: name, email, role.",
    "b": "Developer serializes the EF Core entity directly to JSON. All entity fields are returned regardless of client need.",
    "c": "ASP.NET Core 8 API. Entity framework User entity has 15 fields. Client needs 4."
  },
  "function": "Implement ASVS V9.1 data minimization with response DTOs and field-level classification.",
  "acceptedOutput": "**ASVS V9.1 data minimization** requires returning only the fields the client legitimately needs — never serializing internal or sensitive fields to API responses.\n\n**Root problem:** returning EF Core entities directly auto-includes all fields, including sensitive internal data (hashedPassword, salt, resetToken). This violates data minimization and creates unnecessary PII exposure.\n\n**Fix: dedicated response DTO with explicit field selection:**\n```csharp\n// UserResponseDto.cs — ONLY the fields clients legitimately need\npublic record UserResponseDto(\n    Guid   Id,\n    string Name,\n    string Email,\n    string Role\n    // hashedPassword, salt, resetToken, internalNotes: NEVER included\n);\n\n// UserController.cs\n[HttpGet(\"{id:guid}\")]\n[Authorize]\npublic async Task<IActionResult> GetUser(Guid id)\n{\n    var user = await _db.Users\n        .Where(u => u.Id == id)\n        // Project to DTO at the DB query level — sensitive fields never loaded\n        .Select(u => new UserResponseDto(u.Id, u.Name, u.Email, u.Role))\n        .FirstOrDefaultAsync();\n\n    if (user is null) return NotFound();\n\n    // Resource-level authorization — user can only see their own profile\n    // (or admin can see any)\n    var requestingUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);\n    if (user.Id.ToString() != requestingUserId && !User.IsInRole(\"Admin\"))\n        return Forbid();\n\n    return Ok(user);\n}\n```\n\n**Data classification tags (code comment approach):**\n```csharp\npublic class User\n{\n    public Guid   Id           { get; set; }  // Public\n    public string Name         { get; set; } = \"\"; // Public\n    public string Email        { get; set; } = \"\"; // PII-Contact\n    public string Role         { get; set; } = \"\"; // Public\n\n    // [DataClassification: Secret] — NEVER in API response\n    public string HashedPassword { get; set; } = \"\";\n    public string Salt           { get; set; } = \"\";\n    public string? ResetToken    { get; set; }  // Secret\n\n    // [DataClassification: Internal] — NEVER in API response\n    public string? InternalNotes    { get; set; };\n    public Guid?   CreatedByAdminId { get; set; };\n}\n```\n\n**Additional protection:** configure JSON serializer to ignore null and use `[JsonIgnore]` as a fallback:\n```csharp\nbuilder.Services.AddControllers().AddJsonOptions(opts =>\n    opts.JsonSerializerOptions.DefaultIgnoreCondition =\n        JsonIgnoreCondition.WhenWritingNull);\n```\n\n**Projection at DB query level** is critical — it means sensitive fields are never loaded from the database, not just excluded from the JSON output.",
  "rejectedOutput": "Add `[JsonIgnore]` to sensitive fields on the entity class — they won't be serialized.",
  "arbiters": ["OWASP-ASVS", "ASPNETCORE-SECURITY-DOCS"],
  "whyAccepted": "DTO projection at the DB query level (SELECT only the needed columns) is the correct approach — sensitive data is never loaded into memory. `[JsonIgnore]` is a defense-in-depth measure but not the primary control.",
  "whyRejected": "`[JsonIgnore]` only prevents JSON serialization — the field is still loaded from the DB into the entity object in memory. If logging, debugging tools, or other middleware accesses the entity object, the sensitive field is exposed. Defense must happen at the data access layer.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
