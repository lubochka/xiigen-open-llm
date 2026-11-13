# EXT Batch 26 — EXT-511 through EXT-530

## EXT-511
**registryId:** KUBERNETES-KEPS  
**unitTitle:** Kubernetes ephemeral containers for live pod debugging vs exec into running container  
**sourceUrl:** https://kubernetes.io/docs/concepts/workloads/pods/ephemeral-containers/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-511-R1",
  "sourceIds": ["EXT-511"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-failure-recovery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Production pod running distroless image (`gcr.io/distroless/dotnet`) is crashing with OOMKilled. No shell in the image — `kubectl exec` fails. Need to inspect memory usage, heap dumps, and running processes.",
    "b": "Kubernetes 1.23+ ephemeral containers allow attaching a debug container to a running pod without modifying the pod spec.",
    "c": "Cluster is EKS 1.29. Pod is running in production namespace."
  },
  "function": "Use ephemeral containers to debug the distroless crashing pod and collect a .NET memory dump.",
  "acceptedOutput": "**Ephemeral containers** attach a transient debug container to a running pod, sharing the pod's PID namespace — allowing inspection of running processes, memory, and filesystem without modifying the original container or pod spec.\n\n**Debug session:**\n```bash\n# Attach a .NET diagnostic-capable debug container to the crashing pod\nkubectl debug -it orders-api-7d8f9-xyz \\\n  --image=mcr.microsoft.com/dotnet/sdk:8.0 \\\n  --target=orders-api \\ # target the specific container in the pod\n  -- /bin/bash\n\n# Inside the ephemeral container — PID namespace is shared with orders-api container\n# List all .NET processes in the pod:\nps aux | grep dotnet\n# Output: 7 dotnet /app/OrdersApi.dll\n\n# Install dotnet-dump in the ephemeral container\ndotnet tool install --global dotnet-dump\nexport PATH=$PATH:$HOME/.dotnet/tools\n\n# Collect managed heap dump (process ID from ps output above)\ndotnet-dump collect -p 7 --output /tmp/orders.dmp\n\n# Copy dump out of the pod\n# (from another terminal:)\nkubectl cp orders-api-7d8f9-xyz:/tmp/orders.dmp ./orders.dmp -c debugger-ephemeral\n\n# Analyze the dump locally:\ndotnet-dump analyze orders.dmp\n> dumpheap -stat   # show heap by type — identify memory-consuming types\n> gcroot <address> # find GC roots keeping large objects alive\n```\n\n**OOMKilled diagnosis:** if heap dump shows large `byte[]` or `string` growth, investigate:\n- Connection string multiplied (IDbContextFactory not scoped)\n- Response body buffering enabled globally\n- Large in-memory cache without eviction policy\n\n**Ephemeral container lifecycle:**\n- Attaches to existing pod — does NOT restart the pod\n- Removed when the pod is deleted (not persistent)\n- Cannot be removed while the pod is running — use a new pod for clean state\n\n**Permission:** requires `pods/ephemeralcontainers` RBAC permission — typically granted to SRE (Site Reliability Engineering) role only.",
  "rejectedOutput": "Add a debug shell to the distroless image and redeploy — then use kubectl exec.",
  "arbiters": ["KUBERNETES-DOCS"],
  "whyAccepted": "Ephemeral containers are the Kubernetes-native solution for debugging distroless/minimal images without modifying the image or redeploying. They share the pod's PID namespace, enabling dotnet-dump to inspect the crashing process.",
  "whyRejected": "Adding a shell to a distroless image defeats its security purpose — distroless images have no shell precisely to reduce the attack surface. Redeploying also requires waiting for the new pod to reach the same failure state, which may not be reproducible on demand.",
  "sourceAnchors": ["https://kubernetes.io/docs/concepts/workloads/pods/ephemeral-containers/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-512
**registryId:** BIGCODEBENCH  
**unitTitle:** Python functools.lru_cache vs manual dict cache for expensive pure functions  
**sourceUrl:** https://github.com/bigcode-project/bigcodebench  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-512-R1",
  "sourceIds": ["EXT-512"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F2-storage-performance",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Python service has a function `compute_risk_score(policy_id: str, risk_factors: tuple) -> float` called 50,000 times/day with ~200 unique (policy_id, risk_factors) combinations. Each call takes 80 ms. Developer implements a manual dict cache: `_cache = {}; if key in _cache: return _cache[key]`.",
    "b": "The manual cache grows unbounded — after 30 days, the dict has 200 entries but each risk_factors tuple is large (50 floats). Memory usage is acceptable but the cache has no eviction, thread safety, or hit/miss instrumentation.",
    "c": "The function is pure (deterministic, no side effects). Python 3.11."
  },
  "function": "Identify the manual cache anti-pattern and implement the correct functools.lru_cache solution.",
  "acceptedOutput": "**Anti-pattern:** manual dict cache lacks: bounded size (unbounded growth risk), thread safety (dict reads/writes are not atomic for complex operations), hit/miss metrics, and requires manual key construction.\n\n**functools.lru_cache** (Least Recently Used (LRU) cache) provides all of these automatically:\n```python\nfrom functools import lru_cache\n\n@lru_cache(maxsize=512)  # keep at most 512 entries; evicts LRU when full\ndef compute_risk_score(policy_id: str, risk_factors: tuple[float, ...]) -> float:\n    \"\"\"Pure function — deterministic given same inputs.\"\"\"\n    # expensive computation — only runs on cache miss\n    return _run_risk_model(policy_id, risk_factors)\n\n# Cache statistics (built-in):\nstats = compute_risk_score.cache_info()\nprint(f\"hits={stats.hits}, misses={stats.misses}, \"\n      f\"maxsize={stats.maxsize}, currsize={stats.currsize}\")\n# Output: hits=49800, misses=200, maxsize=512, currsize=200\n\n# Clear cache explicitly when risk model updates:\ncompute_risk_score.cache_clear()\n```\n\n**Thread safety:** `lru_cache` uses a threading.RLock internally — safe for concurrent reads in multi-threaded services.\n\n**Key requirement:** ALL arguments must be hashable. `tuple[float, ...]` is hashable — correct. `list[float]` is NOT hashable — must convert: `compute_risk_score(policy_id, tuple(risk_factors_list))`.\n\n**functools.cache** (Python 3.9+) is `lru_cache(maxsize=None)` — unbounded but with less overhead. Use when the full working set fits in memory:\n```python\nfrom functools import cache\n\n@cache  # unbounded — for 200 stable entries, appropriate here\ndef compute_risk_score(policy_id: str, risk_factors: tuple[float, ...]) -> float:\n    return _run_risk_model(policy_id, risk_factors)\n```\n\n**Performance:** 50,000 calls/day × 80ms × (200/50000) miss rate = 320s of compute vs 50,000 calls × <1µs (cache hit) = 50ms total. ~6,400x reduction in compute time.",
  "rejectedOutput": "Keep the manual dict cache — it's simpler and gives direct control over eviction logic.",
  "arbiters": ["PYTHON-DOCS", "BIGCODEBENCH"],
  "whyAccepted": "lru_cache provides bounded size (eviction), thread safety, and built-in hit/miss instrumentation — all missing from the manual dict cache. For 200 stable entries, @cache (unbounded) is even simpler and correct.",
  "whyRejected": "Manual dict cache 'simplicity' comes at the cost of missing thread safety, no eviction policy, and no instrumentation. For a pure function called 50K times/day, lru_cache is the idiomatic Python solution with no additional complexity.",
  "sourceAnchors": ["https://docs.python.org/3/library/functools.html#functools.lru_cache"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-513
**registryId:** SWE-BENCH  
**unitTitle:** Flask Blueprint vs Application Factory pattern for testable Flask apps  
**sourceUrl:** https://github.com/princeton-nlp/SWE-bench  
**license:** MIT | **trustTier:** 2  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-513-R1",
  "sourceIds": ["EXT-513"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Flask app creates the application at module level: `app = Flask(__name__)`. Unit tests import `app` directly. Problem: tests cannot use a test database — the app is configured once at import with the production config.",
    "b": "Flask Application Factory pattern creates the `app` inside a function, allowing tests to call `create_app(config='testing')` with different configurations.",
    "c": "App has 3 Blueprints (auth, orders, products). Tests use pytest."
  },
  "function": "Refactor the module-level Flask app to use the Application Factory pattern for testable configuration.",
  "acceptedOutput": "**Application Factory pattern** creates the Flask app inside a function — tests can call `create_app('testing')` to get an isolated app instance with the test database.\n\n**Before (module-level app — untestable config):**\n```python\n# app.py — created at import time with production config\napp = Flask(__name__)\napp.config['SQLALCHEMY_DATABASE_URI'] = os.environ['DATABASE_URL']\n\n@app.route('/orders')\ndef get_orders(): ...\n```\n\n**After (Application Factory):**\n```python\n# app/__init__.py\nfrom flask import Flask\nfrom .extensions import db, migrate\n\ndef create_app(config_name: str = 'production') -> Flask:\n    app = Flask(__name__)\n\n    # Load config by name — 'production', 'testing', 'development'\n    app.config.from_object({\n        'production':  'app.config.ProductionConfig',\n        'testing':     'app.config.TestingConfig',\n        'development': 'app.config.DevelopmentConfig',\n    }[config_name])\n\n    # Initialize extensions with this app instance\n    db.init_app(app)\n    migrate.init_app(app, db)\n\n    # Register Blueprints\n    from .orders import orders_bp\n    from .auth   import auth_bp\n    from .products import products_bp\n    app.register_blueprint(orders_bp,   url_prefix='/api/orders')\n    app.register_blueprint(auth_bp,     url_prefix='/api/auth')\n    app.register_blueprint(products_bp, url_prefix='/api/products')\n\n    return app\n\n# config.py\nclass TestingConfig:\n    TESTING = True\n    SQLALCHEMY_DATABASE_URI = 'sqlite:///:memory:'  # in-memory DB for tests\n    WTF_CSRF_ENABLED = False\n\nclass ProductionConfig:\n    SQLALCHEMY_DATABASE_URI = os.environ['DATABASE_URL']\n```\n\n**pytest fixtures using Application Factory:**\n```python\n# conftest.py\nimport pytest\nfrom app import create_app\nfrom app.extensions import db as _db\n\n@pytest.fixture(scope='session')\ndef app():\n    app = create_app('testing')  # isolated test app with SQLite in-memory\n    with app.app_context():\n        _db.create_all()\n        yield app\n        _db.drop_all()\n\n@pytest.fixture\ndef client(app):\n    return app.test_client()\n```\n\n**Result:** each test run gets a fresh SQLite in-memory database. Production DB is never touched by tests.",
  "rejectedOutput": "Set DATABASE_URL environment variable to the test database in tests — the existing app will use it.",
  "arbiters": ["FLASK-DOCS", "SWE-BENCH"],
  "whyAccepted": "Application Factory is the Flask recommended pattern for testable apps. It enables config isolation without environment variable manipulation, which is fragile (environment leaks between tests) and requires careful teardown.",
  "whyRejected": "Environment variable manipulation is fragile: parallel test execution shares environment variables, tests must restore DATABASE_URL after each test, and the original production value must be saved. Application Factory creates proper isolation through Python objects, not global state.",
  "sourceAnchors": ["https://flask.palletsprojects.com/en/3.0.x/patterns/appfactories/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-514
**registryId:** PYTHON-PEPS  
**unitTitle:** PEP-612 ParamSpec for higher-order function type annotations  
**sourceUrl:** https://peps.python.org/pep-0612/  
**license:** PSF-2.0 | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-514-R1",
  "sourceIds": ["EXT-514"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Python decorator `@with_retry(max_attempts=3)` wraps any function. Current type annotation: `def with_retry(max_attempts: int) -> Callable[..., Any]`. mypy cannot infer the wrapped function's parameter types — all decorated functions lose their signatures.",
    "b": "PEP-612 ParamSpec (Python 3.10+) allows decorators to preserve the wrapped function's parameter spec.",
    "c": "Python 3.11. mypy 1.8."
  },
  "function": "Implement PEP-612 ParamSpec annotation on the retry decorator to preserve wrapped function signatures.",
  "acceptedOutput": "**PEP-612 ParamSpec** captures the parameter specification of a callable, allowing decorators to preserve the wrapped function's signature for type checkers.\n\n**Before (loses parameter types):**\n```python\nfrom typing import Callable, Any\n\ndef with_retry(max_attempts: int) -> Callable[..., Any]:  # loses parameter info\n    def decorator(func: Callable[..., Any]) -> Callable[..., Any]:\n        @functools.wraps(func)\n        def wrapper(*args: Any, **kwargs: Any) -> Any:\n            for attempt in range(max_attempts):\n                try:\n                    return func(*args, **kwargs)\n                except Exception:\n                    if attempt == max_attempts - 1:\n                        raise\n        return wrapper\n    return decorator\n\n@with_retry(max_attempts=3)\ndef fetch_user(user_id: int, include_deleted: bool = False) -> User: ...\n\n# mypy: fetch_user has signature (*args: Any, **kwargs: Any) — loses user_id, include_deleted\n```\n\n**After (PEP-612 ParamSpec — preserves parameter types):**\n```python\nfrom typing import ParamSpec, TypeVar, Callable\nimport functools\n\nP = ParamSpec('P')   # captures the parameter spec of the wrapped function\nR = TypeVar('R')     # captures the return type\n\ndef with_retry(max_attempts: int) -> Callable[[Callable[P, R]], Callable[P, R]]:\n    def decorator(func: Callable[P, R]) -> Callable[P, R]:\n        @functools.wraps(func)\n        def wrapper(*args: P.args, **kwargs: P.kwargs) -> R:\n            for attempt in range(max_attempts):\n                try:\n                    return func(*args, **kwargs)\n                except Exception:\n                    if attempt == max_attempts - 1:\n                        raise\n                    continue\n            raise RuntimeError(\"unreachable\")\n        return wrapper\n    return decorator\n\n@with_retry(max_attempts=3)\ndef fetch_user(user_id: int, include_deleted: bool = False) -> User: ...\n\n# mypy: fetch_user(user_id: int, include_deleted: bool = False) -> User\n# Correct — parameter types preserved through the decorator\nfetch_user(user_id=123)               # mypy OK\nfetch_user(user_id='wrong')           # mypy ERROR — int expected\nfetch_user(nonexistent_param=True)    # mypy ERROR — unexpected argument\n```\n\n**P.args and P.kwargs** are the magic: they match `*args` and `**kwargs` to the parameter spec `P` captures — ensuring wrapper accepts exactly the same arguments as the wrapped function.",
  "rejectedOutput": "Use `@overload` to define the decorator signature for each wrapped function type.",
  "arbiters": ["PYTHON-PEPS", "MYPY-DOCS"],
  "whyAccepted": "ParamSpec is the PEP-612 specified solution for preserving callable parameter specs through decorators. P.args/P.kwargs correctly constrain the wrapper's *args/**kwargs to the wrapped function's signature.",
  "whyRejected": "@overload requires writing a separate overload for each wrapped function type — this defeats the purpose of a generic decorator and would require overloads for every function the retry decorator is applied to. ParamSpec is the correct generic solution.",
  "sourceAnchors": ["https://peps.python.org/pep-0612/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-515
**registryId:** OPENTELEMETRY-SPEC  
**unitTitle:** OTel metrics instrument selection — Counter vs Histogram vs Gauge for business metrics  
**sourceUrl:** https://opentelemetry.io/docs/specs/otel/metrics/api/#instrument-types  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-515-R1",
  "sourceIds": ["EXT-515"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-evidence-linkage",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "Service needs to track: (1) total orders placed (monotonically increasing count), (2) HTTP request duration in milliseconds (distribution, need P50/P95/P99), (3) current active connections to the DB connection pool (can go up and down).",
    "b": "OTel provides: Counter (monotonically increasing), UpDownCounter (can increase or decrease), Histogram (records distribution of values), Gauge (reports instantaneous point-in-time value).",
    "c": "Metrics exported to Prometheus via OTel Collector."
  },
  "function": "Select the correct OTel instrument for each metric and implement in .NET.",
  "acceptedOutput": "**OTel instrument selection rules:**\n- **Counter**: monotonically increasing — use for counts that only go up (orders, requests, errors)\n- **UpDownCounter**: can increase or decrease — use for queue depth, active connections, in-flight requests\n- **Histogram**: records distribution of measurements — use for latency, request size, response size (enables P50/P95/P99)\n- **Gauge**: point-in-time snapshot — use for CPU%, memory usage, temperature (polled values, not accumulated)\n\n**Implementation (.NET OTel SDK):**\n```csharp\npublic class OrderMetrics\n{\n    private readonly Counter<long> _ordersPlaced;\n    private readonly Histogram<double> _requestDuration;\n    private readonly UpDownCounter<int> _activeDbConnections;\n\n    public OrderMetrics(IMeterFactory meterFactory)\n    {\n        var meter = meterFactory.Create(\"OrderService\", \"1.0.0\");\n\n        // (1) Total orders placed — monotonically increasing\n        _ordersPlaced = meter.CreateCounter<long>(\n            name: \"orders.placed.total\",\n            unit: \"{order}\",\n            description: \"Total number of orders placed\");\n\n        // (2) HTTP request duration — histogram for P50/P95/P99\n        _requestDuration = meter.CreateHistogram<double>(\n            name: \"http.request.duration\",\n            unit: \"ms\",\n            description: \"HTTP request duration in milliseconds\");\n\n        // (3) Active DB connections — UpDownCounter (can go up and down)\n        _activeDbConnections = meter.CreateUpDownCounter<int>(\n            name: \"db.connections.active\",\n            unit: \"{connection}\",\n            description: \"Current active database connections\");\n    }\n\n    public void RecordOrderPlaced(string region) =>\n        _ordersPlaced.Add(1, new KeyValuePair<string, object?>(\"region\", region));\n\n    public void RecordRequestDuration(double ms, string route, int statusCode) =>\n        _requestDuration.Record(ms,\n            new KeyValuePair<string, object?>(\"route\", route),\n            new KeyValuePair<string, object?>(\"status_code\", statusCode));\n\n    public void ConnectionOpened() => _activeDbConnections.Add(+1);\n    public void ConnectionClosed() => _activeDbConnections.Add(-1);\n}\n```\n\n**Prometheus output:**\n- Counter → `orders_placed_total{region=\"us-east\"}` (Prometheus counter)\n- Histogram → `http_request_duration_ms_bucket{le=\"100\"}` (enables PromQL `histogram_quantile(0.95, ...)` for P95)\n- UpDownCounter → `db_connections_active` (Prometheus gauge)\n\n**Common mistake:** using Counter for active connections — it saturates at the max value and cannot decrease, making it useless for connection pool monitoring.",
  "rejectedOutput": "Use Gauge for all three — Gauge records any numeric value and works for any use case.",
  "arbiters": ["OPENTELEMETRY-SPEC", "PROMETHEUS-DOCS"],
  "whyAccepted": "Instrument selection directly affects what queries are possible in Prometheus. Histogram enables histogram_quantile() for P95/P99. Counter enables rate() and increase() for throughput. UpDownCounter/Gauge enables monitoring of current values.",
  "whyRejected": "Gauge records instantaneous snapshots and does not accumulate — using Gauge for orders would show only the current moment's count, not the total. Gauge for latency would show only the most recent value, not the distribution needed for P95. Instrument choice determines query capability.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/metrics/api/#instrument-types"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-516
**registryId:** DAPPER  
**unitTitle:** Dapper with stored procedures — parameter mapping and output parameters  
**sourceUrl:** https://github.com/DapperLib/Dapper  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-516-R1",
  "sourceIds": ["EXT-516"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Legacy SQL Server database has stored procedures (SP) for all writes: `sp_CreateOrder(@customerId INT, @total DECIMAL(18,2), @orderId INT OUTPUT)`. Dapper is used for reads; team wants to use Dapper for SP calls too.",
    "b": "Stored procedures return the new order ID via OUTPUT parameter. Normal Dapper Execute() doesn't capture OUTPUT parameters.",
    "c": "Dapper 2.1. SQL Server 2019."
  },
  "function": "Implement Dapper stored procedure call with OUTPUT parameter capture for sp_CreateOrder.",
  "acceptedOutput": "**Dapper DynamicParameters** supports OUTPUT parameters for stored procedures:\n```csharp\npublic async Task<int> CreateOrderAsync(int customerId, decimal total)\n{\n    var parameters = new DynamicParameters();\n    // Input parameters\n    parameters.Add(\"@customerId\", customerId, DbType.Int32, ParameterDirection.Input);\n    parameters.Add(\"@total\",      total,      DbType.Decimal, ParameterDirection.Input,\n                   precision: 18, scale: 2);\n    // OUTPUT parameter — Dapper will populate this after execution\n    parameters.Add(\"@orderId\", dbType: DbType.Int32,\n                   direction: ParameterDirection.Output);\n\n    await using var conn = await _connectionFactory.CreateAsync();\n    await conn.ExecuteAsync(\n        \"sp_CreateOrder\",\n        parameters,\n        commandType: CommandType.StoredProcedure  // required for SP calls\n    );\n\n    // Read the OUTPUT parameter after execution\n    var orderId = parameters.Get<int>(\"@orderId\");\n    return orderId;\n}\n```\n\n**Return value capture (RETURN statement in SP):**\n```csharp\nparameters.Add(\"@returnValue\", dbType: DbType.Int32,\n               direction: ParameterDirection.ReturnValue);\nawait conn.ExecuteAsync(\"sp_CheckStock\", parameters,\n                        commandType: CommandType.StoredProcedure);\nvar returnCode = parameters.Get<int>(\"@returnValue\"); // SP RETURN code\n```\n\n**SP with resultset + OUTPUT parameter:**\n```csharp\n// Some SPs return a resultset AND output parameters simultaneously\nvar reader = await conn.QueryMultipleAsync(\n    \"sp_GetOrderWithDetails\",\n    new { orderId = 123 },\n    commandType: CommandType.StoredProcedure\n);\nvar order  = await reader.ReadSingleAsync<Order>();\nvar items  = (await reader.ReadAsync<OrderItem>()).ToList();\n// Note: OUTPUT params from a multi-resultset SP require SqlCommand directly — Dapper limitation\n```\n\n**When to prefer Dapper over raw ADO.NET for SPs:** Dapper handles result mapping automatically. Use raw SqlCommand only when OUTPUT + multi-resultset combination is required.",
  "rejectedOutput": "Use ADO.NET SqlCommand for stored procedures — Dapper is not designed for SP calls.",
  "arbiters": ["DAPPER-DOCS"],
  "whyAccepted": "Dapper DynamicParameters with ParameterDirection.Output is the documented Dapper API for stored procedures with output parameters. It is fully supported and idiomatic.",
  "whyRejected": "The claim that Dapper is not designed for SP calls is factually incorrect — DynamicParameters and CommandType.StoredProcedure are first-class Dapper features specifically for this use case. Using raw ADO.NET adds boilerplate without benefit for standard SP patterns.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#stored-procedures"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-517
**registryId:** ANGULAR-DOCS  
**unitTitle:** Angular httpResource vs HttpClient for declarative server-state management  
**sourceUrl:** https://angular.dev/guide/http  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-517-R1",
  "sourceIds": ["EXT-517"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "migration-decision",
  "input": {
    "a": "Angular 19 introduced `httpResource()` — a signal-based API for HTTP requests that replaces the common HttpClient + BehaviorSubject pattern for server-state management.",
    "b": "Current code: `HttpClient.get<Order[]>()` with BehaviorSubject + async pipe + loading/error state management spread across service and component.",
    "c": "Angular 19. Standalone components with signals."
  },
  "function": "Compare httpResource vs HttpClient + BehaviorSubject and implement httpResource for an orders list with loading/error states.",
  "acceptedOutput": "**httpResource()** (Angular 19) is a declarative signal-based API that manages server state (data, loading, error) with automatic refetch on signal dependency changes.\n\n**Before (HttpClient + BehaviorSubject pattern):**\n```typescript\n@Injectable({ providedIn: 'root' })\nexport class OrderService {\n  private readonly _orders$ = new BehaviorSubject<Order[]>([]);\n  private readonly _loading$ = new BehaviorSubject(false);\n  private readonly _error$ = new BehaviorSubject<string | null>(null);\n\n  readonly orders$ = this._orders$.asObservable();\n  readonly loading$ = this._loading$.asObservable();\n\n  loadOrders(status: string) {\n    this._loading$.next(true);\n    this._error$.next(null);\n    this._http.get<Order[]>(`/api/orders?status=${status}`)\n      .pipe(finalize(() => this._loading$.next(false)))\n      .subscribe({\n        next: orders => this._orders$.next(orders),\n        error: err => this._error$.next(err.message)\n      });\n  }\n}\n```\n\n**After (httpResource — Angular 19):**\n```typescript\nimport { httpResource } from '@angular/common/http';\nimport { signal, computed } from '@angular/core';\n\n@Component({\n  standalone: true,\n  template: `\n    @if (ordersResource.isLoading()) {\n      <app-spinner />\n    } @else if (ordersResource.error()) {\n      <app-error [message]=\"ordersResource.error()?.message\" />\n    } @else {\n      @for (order of ordersResource.value(); track order.id) {\n        <app-order-card [order]=\"order\" />\n      }\n    }\n  `\n})\nexport class OrderListComponent {\n  readonly status = signal('active'); // reactive filter\n\n  // httpResource automatically refetches when status() changes\n  readonly ordersResource = httpResource<Order[]>(\n    () => `/api/orders?status=${this.status()}`, // reactive URL\n    { defaultValue: [] }  // value() before first fetch\n  );\n\n  // Derived signals — no async pipe needed\n  readonly orderCount = computed(() => this.ordersResource.value()?.length ?? 0);\n\n  changeStatus(newStatus: string) {\n    this.status.set(newStatus); // triggers automatic refetch\n  }\n}\n```\n\n**Comparison:**\n| | HttpClient + BehaviorSubject | httpResource |\n|---|---|---|\n| Loading state | Manual | `.isLoading()` signal |\n| Error state | Manual | `.error()` signal |\n| Refetch on filter change | Manual subscribe | Automatic (signal deps) |\n| Template | async pipe | Direct signal read |\n| Reuse | Service injection | Inline or service |\n\n**Migration guidance:** httpResource replaces the server-state slice of BehaviorSubject — pure client state (form values, UI toggles) still uses `signal()`.",
  "rejectedOutput": "Keep HttpClient — httpResource is too new and may not be stable.",
  "arbiters": ["ANGULAR-DOCS"],
  "whyAccepted": "httpResource ships in Angular 19 stable. It eliminates the manual loading/error state management that is boilerplate in every HttpClient + BehaviorSubject pattern. The reactive refetch on signal dependency change is a concrete functional improvement.",
  "whyRejected": "httpResource is in Angular 19 stable, not experimental. 'Too new' without a specific stability concern is not a technical objection. The existing BehaviorSubject pattern has real costs (boilerplate, manual loading/error management) that httpResource directly addresses.",
  "sourceAnchors": ["https://angular.dev/guide/http"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-518
**registryId:** REACT-ADVANCED  
**unitTitle:** React 19 use() hook for async resource reading vs useEffect + useState  
**sourceUrl:** https://react.dev/reference/react/use  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-518-R1",
  "sourceIds": ["EXT-518"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "migration-decision",
  "input": {
    "a": "React 19 introduces the `use()` hook for reading async resources (Promises, Context) during render. Current pattern: `useEffect(() => { fetch().then(setData) }, [])` with separate loading and error states.",
    "b": "use() can read a Promise inside the component — React suspends the component until the Promise resolves, integrating with Suspense boundaries automatically.",
    "c": "React 19 stable. App uses React.Suspense already for other components."
  },
  "function": "Compare use() hook vs useEffect + useState for async data reading and implement the use() pattern.",
  "acceptedOutput": "**React 19 use() hook** reads a Promise during render — React suspends the component (shows the nearest Suspense fallback) until the Promise resolves, then re-renders with the resolved value.\n\n**Before (useEffect + useState — 3 state variables, loading flicker):**\n```tsx\nfunction OrderList() {\n  const [orders, setOrders] = useState<Order[]>([]);\n  const [loading, setLoading] = useState(true);\n  const [error, setError] = useState<Error | null>(null);\n\n  useEffect(() => {\n    fetchOrders()\n      .then(setOrders)\n      .catch(setError)\n      .finally(() => setLoading(false));\n  }, []);\n\n  if (loading) return <Spinner />;\n  if (error)   return <Error message={error.message} />;\n  return <ul>{orders.map(o => <li key={o.id}>{o.name}</li>)}</ul>;\n}\n```\n\n**After (use() with Suspense + ErrorBoundary — no local state):**\n```tsx\n// Create the Promise OUTSIDE the component to avoid refetch on every render\nconst ordersPromise = fetchOrders(); // created once — stable reference\n\nfunction OrderList() {\n  // use() suspends until ordersPromise resolves\n  // Suspense boundary shows fallback during suspension\n  const orders = use(ordersPromise); // resolved value — no loading state needed\n\n  return <ul>{orders.map(o => <li key={o.id}>{o.name}</li>)}</ul>;\n  // No error handling here — ErrorBoundary catches Promise rejections\n}\n\n// Parent component:\nfunction App() {\n  return (\n    <ErrorBoundary fallback={<ErrorPage />}>\n      <Suspense fallback={<Spinner />}>\n        <OrderList />\n      </Suspense>\n    </ErrorBoundary>\n  );\n}\n```\n\n**Dynamic Promise (refetch on param change):**\n```tsx\nfunction OrderList({ status }: { status: string }) {\n  // useMemo creates a new Promise only when status changes\n  const promise = useMemo(() => fetchOrders(status), [status]);\n  const orders = use(promise); // React re-suspends when promise changes\n  return <ul>{orders.map(o => <li key={o.id}>{o.name}</li>)}</ul>;\n}\n```\n\n**Limitations of use():**\n- Cannot be called conditionally (follows Rules of Hooks)\n- Promise must be stable (wrap in useMemo or create outside component)\n- ErrorBoundary required for error handling (use() throws on rejection)\n\n**When to keep useEffect:** when you need the data for side effects (not rendering), or when the data fetch depends on DOM measurements after mount.",
  "rejectedOutput": "useEffect + useState is the established pattern — use() is too new and changes the mental model too drastically.",
  "arbiters": ["REACT-DOCS"],
  "whyAccepted": "use() eliminates 3 local state variables and the loading/error conditional renders — the component body expresses only the happy path. Suspense and ErrorBoundary handle the loading and error states at the boundary level. This is the React 19 intended model for async data.",
  "whyRejected": "useEffect + useState has real costs: waterfall fetching (effect fires after first render), loading state flicker, 3 state variables for every data fetch. 'Changes the mental model' is a learning curve, not a technical objection to a stable React 19 feature.",
  "sourceAnchors": ["https://react.dev/reference/react/use"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-519
**registryId:** ASPNETCORE-DOCS-SAMPLES  
**unitTitle:** ASP.NET Core Keyed Services (DI) for named implementations vs factory pattern  
**sourceUrl:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-519-R1",
  "sourceIds": ["EXT-519"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "migration-decision",
  "input": {
    "a": "ASP.NET Core API has two implementations of INotificationService: EmailNotificationService and SmsNotificationService. A controller needs to inject both. Previous workaround: factory pattern with enum parameter `_factory.Create(NotificationType.Email)`.",
    "b": ".NET 8 introduced Keyed Services — register multiple implementations of the same interface with string or enum keys, then inject by key.",
    "c": "ASP.NET Core 8. Two notification services are used in separate controllers."
  },
  "function": "Migrate from factory pattern to .NET 8 Keyed Services for named DI implementations.",
  "acceptedOutput": "**.NET 8 Keyed Services** allow registering multiple implementations of the same interface with distinct keys, eliminating the factory pattern for named implementations.\n\n**Before (factory pattern):**\n```csharp\n// Factory-based workaround — adds INotificationServiceFactory to DI graph\npublic interface INotificationServiceFactory\n{\n    INotificationService Create(NotificationType type);\n}\n\n// Controller — awkward: injects factory, not the service directly\n[HttpPost(\"notify/email\")]\npublic async Task<IActionResult> NotifyByEmail()\n{\n    var service = _factory.Create(NotificationType.Email);\n    await service.SendAsync(...);\n}\n```\n\n**After (.NET 8 Keyed Services):**\n```csharp\n// Program.cs — register with string keys\nbuilder.Services.AddKeyedScoped<INotificationService, EmailNotificationService>(\"email\");\nbuilder.Services.AddKeyedScoped<INotificationService, SmsNotificationService>(\"sms\");\n\n// EmailController — inject by key using [FromKeyedServices]\n[ApiController]\n[Route(\"notify/email\")]\npublic class EmailNotifyController : ControllerBase\n{\n    private readonly INotificationService _emailService;\n\n    public EmailNotifyController(\n        [FromKeyedServices(\"email\")] INotificationService emailService)\n    {\n        _emailService = emailService;\n    }\n\n    [HttpPost]\n    public async Task<IActionResult> Send(NotifyDto dto)\n    {\n        await _emailService.SendAsync(dto.Recipient, dto.Message);\n        return Ok();\n    }\n}\n\n// Dynamic resolution (when key is determined at runtime):\npublic class NotificationOrchestrator(\n    IServiceProvider serviceProvider)\n{\n    public async Task SendAsync(string channel, string recipient, string message)\n    {\n        var service = serviceProvider.GetRequiredKeyedService<INotificationService>(channel);\n        await service.SendAsync(recipient, message);\n    }\n}\n```\n\n**Comparison:**\n| | Factory Pattern | Keyed Services |\n|---|---|---|\n| DI registration | INotificationServiceFactory + impls | INotificationService ×2 with keys |\n| Constructor injection | Factory, then factory.Create() | Direct service via [FromKeyedServices] |\n| Runtime resolution | factory.Create(type) | sp.GetRequiredKeyedService<T>(key) |\n| Testability | Mock factory | Mock individual keyed service |\n\n**Key type:** string keys work for most cases. For compile-time safety, use an enum or static class constants as keys.",
  "rejectedOutput": "Keep the factory pattern — it's more explicit about which implementation is being created.",
  "arbiters": ["ASPNETCORE-DOCS", "DOTNET-8-RELEASE-NOTES"],
  "whyAccepted": "Keyed Services is .NET 8's built-in solution for the factory pattern — it eliminates the INotificationServiceFactory interface and its registration. Direct [FromKeyedServices] injection is more testable (mock the specific service, not the factory).",
  "whyRejected": "The factory pattern requires a factory interface, factory registration, factory implementation, and factory resolution — 4 components to replace 1 DI registration. The factory's explicitness (factory.Create(type)) is not a significant advantage over [FromKeyedServices(\"email\")].",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection#keyed-services"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-520
**registryId:** NESTJS-ADVANCED  
**unitTitle:** NestJS ConfigModule validation with Joi vs class-validator for environment config  
**sourceUrl:** https://docs.nestjs.com/techniques/configuration  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-520-R1",
  "sourceIds": ["EXT-520"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-execution-validation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "NestJS app starts with missing environment variables (DATABASE_URL not set) and fails 30 seconds later when the first DB query is made — not at startup. Team wants to fail fast at startup if required env vars are missing.",
    "b": "NestJS ConfigModule supports validation via Joi schema or class-validator. Both validate at startup before the app begins listening.",
    "c": "Environment: DATABASE_URL (required string URL), PORT (optional int, default 3000), JWT_SECRET (required string, min 32 chars), LOG_LEVEL (optional enum: debug|info|warn|error)."
  },
  "function": "Implement ConfigModule startup validation with Joi to fail fast on missing environment variables.",
  "acceptedOutput": "**NestJS ConfigModule with Joi validation** fails the application startup if any required environment variable is missing or invalid — before any HTTP requests are served.\n\n**Implementation:**\n```typescript\n// app.module.ts\nimport { ConfigModule } from '@nestjs/config';\nimport * as Joi from 'joi';\n\n@Module({\n  imports: [\n    ConfigModule.forRoot({\n      isGlobal: true,  // no need to import ConfigModule in every feature module\n      validationSchema: Joi.object({\n        // Required — startup fails immediately if missing or invalid\n        DATABASE_URL: Joi.string().uri().required(),\n        JWT_SECRET:   Joi.string().min(32).required(),\n\n        // Optional with defaults\n        PORT:      Joi.number().integer().min(1).max(65535).default(3000),\n        LOG_LEVEL: Joi.string().valid('debug', 'info', 'warn', 'error').default('info'),\n\n        // Optional with conditional requirement\n        REDIS_URL: Joi.string().uri().when('CACHE_ENABLED', {\n          is: 'true',\n          then: Joi.required(),  // required if CACHE_ENABLED=true\n          otherwise: Joi.optional(),\n        }),\n        CACHE_ENABLED: Joi.string().valid('true', 'false').default('false'),\n      }),\n      validationOptions: {\n        allowUnknown: true,  // don't fail on extra env vars (CI_BUILD_ID etc.)\n        abortEarly: false,   // report ALL missing vars at once, not just the first\n      },\n    }),\n  ],\n})\nexport class AppModule {}\n```\n\n**Startup behavior on missing DATABASE_URL:**\n```\nError: Config validation error: \"DATABASE_URL\" is required, \"JWT_SECRET\" is required\n    at ConfigModule.forRoot ...\n// Process exits immediately — before HTTP server starts\n```\n\n**Usage in service:**\n```typescript\n@Injectable()\nexport class OrderService {\n  constructor(private readonly config: ConfigService) {}\n\n  private get dbUrl() {\n    // ConfigService is type-safe after validation — value is guaranteed non-null\n    return this.config.getOrThrow<string>('DATABASE_URL');\n  }\n}\n```\n\n**Joi vs class-validator:** Joi is simpler for env vars (string-based, no class definition needed). class-validator is better when config is complex and needs typed DTO classes with decorators.",
  "rejectedOutput": "Check env vars in the service constructor and throw if missing — services know which vars they need.",
  "arbiters": ["NESTJS-DOCS"],
  "whyAccepted": "ConfigModule Joi validation fails at startup before the NestJS module graph is initialized — the application never starts in an invalid state. abortEarly: false reports all missing variables in one error message, not one per restart.",
  "whyRejected": "Per-service env var checks fail at the first DB query (or first usage), not at startup — the application starts, serves health checks, and accepts connections before the first failure. This creates a 30-second deception window. Startup validation is strictly safer.",
  "sourceAnchors": ["https://docs.nestjs.com/techniques/configuration#schema-validation"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-521
**registryId:** IETF-RFCS  
**unitTitle:** RFC-7235 HTTP Authentication — 401 vs 403 semantic distinction  
**sourceUrl:** https://www.rfc-editor.org/rfc/rfc7235  
**license:** IETF-TLP | **trustTier:** 1  
**domainsTagged:** C | **familiesTagged:** 7

### Training Row

```json
{
  "rowId": "EXT-521-R1",
  "sourceIds": ["EXT-521"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C2-output-formatting",
  "rowClass": "negative-example",
  "input": {
    "a": "ASP.NET Core API returns 403 Forbidden when a JWT token is missing. Security audit flags this as incorrect — RFC-7235 specifies 401 Unauthorized when authentication credentials are absent.",
    "b": "Developer believes 403 is 'more secure' because it doesn't reveal that authentication is required.",
    "c": "API uses JWT Bearer authentication. Global authorization policy configured."
  },
  "function": "Explain the RFC-7235 semantic distinction between 401 and 403 and implement correct HTTP status codes.",
  "acceptedOutput": "**RFC-7235 Section 3.1 (401 Unauthorized):** 'The request has not been applied because it lacks valid authentication credentials for the target resource. The server generating a 401 response MUST send a WWW-Authenticate header field.'\n\n**RFC-7235 Section 3.2 (403 Forbidden):** 'The server understood the request but refuses to authorize it. [...] If authentication credentials were provided in the request, the server considers them insufficient to grant access.'\n\n**Correct mapping:**\n- **401 Unauthorized**: no credentials provided (missing or malformed JWT token) OR credentials are expired/invalid\n- **403 Forbidden**: valid, authenticated identity, but the identity lacks permission for this resource\n\n**Examples:**\n- No Authorization header → **401** (not authenticated)\n- Expired JWT token → **401** (authentication failed)\n- Valid JWT for `role=User` accessing `DELETE /admin/users` → **403** (authenticated but not authorized)\n\n**ASP.NET Core configuration:**\n```csharp\n// Program.cs — configure correct status codes\nbuilder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)\n    .AddJwtBearer(opts =>\n    {\n        opts.Events = new JwtBearerEvents\n        {\n            OnChallenge = ctx =>\n            {\n                // Default: 401 with WWW-Authenticate header — CORRECT per RFC-7235\n                // Do NOT override this to 403\n                return Task.CompletedTask;\n            },\n            OnForbidden = ctx =>\n            {\n                // Fires when authenticated but not authorized — 403 is correct here\n                ctx.Response.StatusCode = 403;\n                return Task.CompletedTask;\n            }\n        };\n    });\n```\n\n**Security claim refutation:** returning 403 for 'no credentials' does NOT hide that authentication is required — browsers and API clients recognize 403 for any access failure and retry without credentials anyway. The 'security through ambiguity' argument is false. Correct status codes enable proper client behavior (401 triggers credential dialogs in browsers, allows clients to obtain a token and retry).\n\n**WWW-Authenticate header** (required by RFC-7235 with 401):\n```\nWWW-Authenticate: Bearer realm=\"api\", error=\"invalid_token\"\n```\nASP.NET Core JwtBearer adds this automatically.",
  "rejectedOutput": "Return 403 for all auth failures — it's more secure to not reveal authentication requirements.",
  "arbiters": ["RFC-7235", "IANA-HTTP-STATUS"],
  "whyAccepted": "RFC-7235 explicitly defines 401 for missing/invalid credentials and 403 for insufficient authorization. The security claim (403 is 'more secure') is false — 403 for no-credentials breaks OAuth flows, browser login dialogs, and API client retry logic.",
  "whyRejected": "The 'more secure' argument conflates two different security properties. 401 with WWW-Authenticate is the specified mechanism for signaling that authentication is needed — suppressing it breaks standard HTTP client behavior without providing any actual security benefit.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc7235"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-522
**registryId:** GOOGLE-AIP  
**unitTitle:** AIP-132 standard List method — page_size, page_token, filter, order_by requirements  
**sourceUrl:** https://google.aip.dev/132  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-522-R1",
  "sourceIds": ["EXT-522"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "REST API `GET /orders` endpoint returns all orders with no pagination. For large customers with 100,000+ orders, this causes OOM errors and 30-second timeouts.",
    "b": "Google AIP-132 defines the standard List method pattern: required `page_size` and `page_token` for cursor pagination, optional `filter` (AIP-160) and `order_by`.",
    "c": "API uses HTTP/JSON. gRPC transcoding to be added later."
  },
  "function": "Implement AIP-132 compliant List method with cursor pagination for the orders endpoint.",
  "acceptedOutput": "**AIP-132 List method** defines cursor-based pagination via `page_token` — prevents offset-based pagination problems (skip N rows is O(N) in SQL) for large datasets.\n\n**AIP-132 response structure:**\n```json\n{\n  \"orders\": [...],           // list of resources\n  \"nextPageToken\": \"abc123\", // omitted on last page\n  \"totalSize\": 100421        // optional — AIP-158 says totalSize is optional\n}\n```\n\n**Implementation (ASP.NET Core 8):**\n```csharp\n[HttpGet(\"orders\")]\npublic async Task<IActionResult> ListOrders(\n    [FromQuery] int pageSize = 20,\n    [FromQuery] string? pageToken = null,\n    [FromQuery] string? filter = null,\n    [FromQuery] string? orderBy = null)\n{\n    // Validate page_size: AIP-132 requires server to cap at max_page_size\n    const int maxPageSize = 1000;\n    pageSize = Math.Clamp(pageSize, 1, maxPageSize);\n\n    // Decode cursor (opaque to client, encodes last-seen ID + sort value)\n    var cursor = pageToken is not null\n        ? _cursorService.Decode(pageToken)  // {lastId: 54321, lastCreatedAt: ...}\n        : null;\n\n    var query = _db.Orders.AsNoTracking();\n\n    // Apply AIP-160 filter\n    if (!string.IsNullOrEmpty(filter))\n        query = _filterParser.Apply(query, filter);\n\n    // Cursor-based pagination — avoids OFFSET N (expensive on large tables)\n    if (cursor is not null)\n        query = query.Where(o => o.CreatedAt < cursor.LastCreatedAt\n                              || (o.CreatedAt == cursor.LastCreatedAt\n                                  && o.Id < cursor.LastId));\n\n    // Apply order — AIP-132: default is unspecified but must be stable\n    query = query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);\n\n    // Fetch pageSize + 1 to detect if there's a next page\n    var orders = await query.Take(pageSize + 1).ToListAsync();\n\n    string? nextPageToken = null;\n    if (orders.Count > pageSize)\n    {\n        orders.RemoveAt(orders.Count - 1); // remove the extra item\n        var last = orders.Last();\n        nextPageToken = _cursorService.Encode(last.Id, last.CreatedAt);\n    }\n\n    return Ok(new\n    {\n        orders    = orders.Select(OrderDto.From),\n        nextPageToken,\n        // totalSize: omit for performance — forces sequential cursor traversal\n    });\n}\n```\n\n**Cursor encoding** (opaque to client):\n```csharp\n// Encode: Base64(JSON({lastId, lastCreatedAt})) — URL-safe\n// Never expose raw IDs in cursors — use signed/encrypted tokens for security\n```",
  "rejectedOutput": "Use `?page=2&limit=20` offset pagination — it's simpler for clients to implement.",
  "arbiters": ["GOOGLE-AIP", "AIP-132"],
  "whyAccepted": "AIP-132 cursor pagination (page_token) is O(1) for any page position — uses indexed WHERE clause instead of OFFSET N. For 100K orders, OFFSET 99980 scans 99980 rows before returning 20. Cursor pagination reads only the 20 rows needed.",
  "whyRejected": "Offset pagination (OFFSET N) is O(N) — page 5000 of 20 requires the DB to scan 99,980 rows before returning 20. For 100K+ orders, this is the root cause of 30-second timeouts. 'Simpler for clients' is outweighed by server-side O(N) cost.",
  "sourceAnchors": ["https://google.aip.dev/132"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-523
**registryId:** OPENASSISTANT-OASST1  
**unitTitle:** OpenAssistant data filtering — removing low-rank assistant turns for training quality  
**sourceUrl:** https://huggingface.co/datasets/OpenAssistant/oasst1  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-523-R1",
  "sourceIds": ["EXT-523"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-data-quality-decision",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "SFT training on OpenAssistant oasst1 uses ALL assistant messages regardless of human ranking. Dataset has messages ranked 0-4 by human annotators. Low-ranked messages (rank 0-1) include hallucinations, refusals with wrong reasons, and unhelpful responses.",
    "b": "Training on all ranks degrades model quality — the model learns from both good (rank 3-4) and bad (rank 0-1) responses equally. Human annotators agreed that ranks 0-1 are 'poor' responses.",
    "c": "oasst1 dataset has 161,443 messages; 42% are assistant messages; of those, ~35% are rank 0-1."
  },
  "function": "Design the oasst1 filtering strategy for SFT to exclude low-quality training examples.",
  "acceptedOutput": "**Anti-pattern:** training on all oasst1 assistant messages regardless of rank — rank 0-1 responses are annotator-labeled as poor quality and corrupt the SFT signal.\n\n**Filtering strategy:**\n```python\nfrom datasets import load_dataset\nimport pandas as pd\n\n# Load oasst1 with message metadata\noasst = load_dataset('OpenAssistant/oasst1', split='train')\ndf = pd.DataFrame(oasst)\n\n# Filter: keep only assistant messages with rank >= 2 (acceptable) or None (human messages)\n# oasst1 rank field: None = human turn, 0-4 = assistant quality\nfiltered = df[\n    (df['role'] == 'prompter') |  # keep all human messages\n    ((df['role'] == 'assistant') & (df['rank'] >= 2))  # keep rank 2+ assistant\n]\n\nprint(f\"Original: {len(df):,} messages\")\nprint(f\"After rank filter: {len(filtered):,} messages\")\n# Original: 161,443\n# After rank filter: ~118,000 (removes ~43K rank 0-1 assistant messages)\n\n# Reconstruct complete conversation trees (must keep conversation structure)\ndef get_complete_trees(df: pd.DataFrame) -> pd.DataFrame:\n    \"\"\"Keep only message trees where all messages pass the filter.\"\"\"\n    filtered_ids = set(df['message_id'])\n    # Keep only trees where all included messages have all ancestors in filtered_ids\n    # (conversation continuity — can't have orphaned responses)\n    valid_trees = []\n    for tree_id in df['message_tree_id'].unique():\n        tree_msgs = df[df['message_tree_id'] == tree_id]\n        parent_ids = set(tree_msgs['parent_id'].dropna())\n        if parent_ids.issubset(filtered_ids):\n            valid_trees.append(tree_id)\n    return df[df['message_tree_id'].isin(valid_trees)]\n\ncomplete_filtered = get_complete_trees(filtered)\n\n# SFT format: (prompt, response) pairs from complete conversation paths\n# Walk each tree path: human1 → assistant1 → human2 → assistant2\n```\n\n**Expected improvement:** SFT on rank 2+ messages vs all ranks shows +4-7% improvement on MT-Bench evaluation (measuring multi-turn instruction following quality).\n\n**Rank threshold tuning:** rank ≥ 2 is the recommended threshold — rank ≥ 3 filters too aggressively (removes valid but not exemplary responses), reducing dataset size excessively.",
  "rejectedOutput": "Use all messages — human annotators are inconsistent and rank scores are noisy.",
  "arbiters": ["OPENASSISTANT-OASST1", "RLHF-DATA-QUALITY-PAPER"],
  "whyAccepted": "Human annotator consistency at the aggregate level is sufficient for filtering — rank 0-1 messages have statistically agreed-upon poor quality even with individual annotator variance. The MT-Bench improvement is a concrete measurable Gate B.",
  "whyRejected": "Annotator inconsistency at the individual level does not imply that the rank signal is useless at the aggregate level. Rank 0 messages that received unanimous poor ratings are genuine low-quality training examples. Using all messages gives equal weight to known-bad responses.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-524
**registryId:** TATOEBA  
**unitTitle:** Tatoeba speaker metadata — native vs non-native contributor filtering for training quality  
**sourceUrl:** https://tatoeba.org/  
**license:** CC-BY-2.0-FR | **trustTier:** 2  
**domainsTagged:** C | **familiesTagged:** 2

### Training Row

```json
{
  "rowId": "EXT-524-R1",
  "sourceIds": ["EXT-524"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-translation-filtering",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Tatoeba EN-FR pipeline. Tatoeba contributors tag their proficiency: native speaker, advanced, intermediate, beginner. Some sentences from beginner/intermediate contributors have grammatical errors that pass LASER alignment filter because the semantic content matches.",
    "b": "Team debates: (1) filter to native speakers only — high quality but reduces EN-FR pairs from 220K to 85K. (2) include advanced non-native — 155K pairs, some quality risk.",
    "c": "Target: training data for business communication translation. Precision matters over recall."
  },
  "function": "Design a proficiency-level filtering strategy for Tatoeba EN-FR that balances quality and dataset size.",
  "acceptedOutput": "**Recommended: native + advanced non-native, with error detection filter.**\n\n**Rationale:** reducing from 220K to 85K (native-only) discards 135K potentially valid pairs. Advanced non-native speakers produce grammatically correct sentences — beginner/intermediate errors are the quality concern.\n\n**Implementation:**\n```python\nfrom language_tool_python import LanguageTool\nimport langdetect\n\n# LanguageTool grammar checker — open source, supports EN and FR\nlt_en = LanguageTool('en-US')\nlt_fr = LanguageTool('fr')\n\ndef proficiency_filter(sentence: str, lang: str, contributor_level: str) -> bool:\n    \"\"\"Accept native/advanced sentences; grammar-check beginner/intermediate.\"\"\"\n\n    if contributor_level in ('native', 'advanced'):\n        return True  # trust native/advanced — no grammar check needed\n\n    if contributor_level in ('beginner', 'intermediate'):\n        # Grammar check — reject if more than 1 significant error\n        lt = lt_en if lang == 'en' else lt_fr\n        matches = lt.check(sentence)\n        # Filter: ignore style suggestions, count only grammar/spelling errors\n        grammar_errors = [\n            m for m in matches\n            if m.ruleIssueType in ('grammar', 'misspelling')\n        ]\n        return len(grammar_errors) <= 1  # allow 1 minor error\n\n    return True  # unknown level — include (don't discard by default)\n\n# Apply to dataset\ndef filter_pair(en: str, fr: str, en_level: str, fr_level: str) -> bool:\n    return (\n        proficiency_filter(en, 'en', en_level) and\n        proficiency_filter(fr, 'fr', fr_level)\n    )\n\nfiltered = [\n    (en, fr) for en, fr, el, fl in pairs\n    if filter_pair(en, fr, el, fl)\n]\n```\n\n**Expected dataset composition:**\n- Native-only: 85K pairs (100% quality, low coverage)\n- Native + advanced: 155K pairs (98% quality estimate)\n- Native + advanced + grammar-checked intermediate: ~175K pairs (95% quality estimate)\n\n**Recommendation for business communication:** Native + advanced (155K) — grammar-checked intermediate adds 20K pairs but business translation requires high precision. The 155K dataset provides sufficient coverage for business domain fine-tuning.",
  "rejectedOutput": "Include all contributors — native speakers are not always correct and non-native can be better.",
  "arbiters": ["TATOEBA", "NLP-DATA-QUALITY-PAPER"],
  "whyAccepted": "The proficiency-stratified grammar checking correctly handles the actual quality risk (beginner errors, not advanced errors). The dataset composition analysis gives concrete size/quality trade-offs for each strategy.",
  "whyRejected": "While native speaker quality can vary, the systematic error rate for beginners/intermediate is empirically higher than for natives/advanced. 'Non-native can be better' is true for some individuals but is not a reason to ignore the proficiency signal entirely for a quality-sensitive use case.",
  "sourceAnchors": ["https://tatoeba.org/eng/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-525
**registryId:** VUE-DOCS  
**unitTitle:** Vue 3 Suspense with async setup components — data fetching coordination  
**sourceUrl:** https://vuejs.org/guide/built-ins/suspense  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-525-R1",
  "sourceIds": ["EXT-525"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Vue 3 dashboard renders 3 panels: UserProfile, OrderSummary, and RecentActivity. Each has its own `onMounted` + `ref(loading)` + fetch pattern. Loading spinners flash independently — user sees 3 separate loading states sequentially.",
    "b": "Vue 3 Suspense coordinates multiple async component resolutions — all or nothing fallback.",
    "c": "Vue 3.3+. Using Pinia for some state. Components are independent (no data dependency)."
  },
  "function": "Implement Vue 3 Suspense to coordinate dashboard panel loading with a unified skeleton fallback.",
  "acceptedOutput": "**Vue 3 Suspense** coordinates async components — shows the fallback until ALL async children have resolved, then renders all panels simultaneously (no individual loading flickers).\n\n**Async setup component (enables Suspense integration):**\n```vue\n<!-- UserProfile.vue — async setup: top-level await -->\n<script setup lang=\"ts\">\n// Top-level await makes this an async component — Suspense waits for this\nconst user = await fetchCurrentUser(); // no ref(loading) needed\n</script>\n\n<template>\n  <div class=\"profile\">\n    <img :src=\"user.avatarUrl\" :alt=\"user.name\" />\n    <h2>{{ user.name }}</h2>\n  </div>\n</template>\n```\n\n```vue\n<!-- OrderSummary.vue —  async setup -->\n<script setup lang=\"ts\">\nconst summary = await fetchOrderSummary();\n</script>\n<template>\n  <div class=\"summary\">\n    <p>Total orders: {{ summary.total }}</p>\n    <p>Revenue: {{ formatCurrency(summary.revenue) }}</p>\n  </div>\n</template>\n```\n\n```vue\n<!-- Dashboard.vue — Suspense coordinates all async children -->\n<template>\n  <!-- DashboardSkeleton shown until ALL three components resolve -->\n  <Suspense>\n    <template #default>\n      <!-- All three fetch concurrently — rendered together on resolve -->\n      <div class=\"dashboard-grid\">\n        <UserProfile />\n        <OrderSummary />\n        <RecentActivity />\n      </div>\n    </template>\n\n    <template #fallback>\n      <!-- Single unified skeleton instead of 3 independent spinners -->\n      <DashboardSkeleton />\n    </template>\n  </Suspense>\n</template>\n\n<script setup lang=\"ts\">\nimport { defineAsyncComponent } from 'vue'\n// Async components defined with defineAsyncComponent integrate with Suspense\nimport UserProfile  from './UserProfile.vue'   // has top-level await\nimport OrderSummary from './OrderSummary.vue'  // has top-level await\nimport RecentActivity from './RecentActivity.vue' // has top-level await\n</script>\n```\n\n**Error handling with Suspense:**\n```vue\n<template>\n  <!-- onError event: fires if any child async throws -->\n  <Suspense @error=\"handleError\">\n    <template #default><Dashboard /></template>\n    <template #fallback><DashboardSkeleton /></template>\n  </Suspense>\n</template>\n```\n\n**Fetch timing:** all three `await fetch()` calls in the async setups run in parallel (Vue starts resolving all children simultaneously) — same as `Promise.all` timing.\n\n**Note:** Suspense is stable for async setup components in Vue 3.3+. Nested Suspense boundaries are still marked experimental.",
  "rejectedOutput": "Use Promise.all in the parent component to fetch all data before rendering children.",
  "arbiters": ["VUE-DOCS"],
  "whyAccepted": "Suspense with async setup components is the Vue 3 idiomatic pattern for coordinated async rendering. It provides the same timing as Promise.all but with component-level encapsulation — each component owns its own data fetching without prop drilling from the parent.",
  "whyRejected": "Promise.all in the parent requires the parent to know about and orchestrate each child's data requirements — tight coupling. Any child that adds a new data requirement must modify the parent. Suspense preserves component encapsulation.",
  "sourceAnchors": ["https://vuejs.org/guide/built-ins/suspense"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-526
**registryId:** EFCORE-SOURCE  
**unitTitle:** EF Core global query filters for soft delete and multi-tenancy  
**sourceUrl:** https://github.com/dotnet/efcore  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-526-R1",
  "sourceIds": ["EXT-526"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Multi-tenant SaaS application: every entity has `TenantId` and `IsDeleted` fields. Every query must filter `WHERE TenantId = @currentTenant AND IsDeleted = 0`. Developers sometimes forget — incidents where one tenant sees another's data.",
    "b": "EF Core global query filters automatically apply a WHERE clause to every query for an entity type.",
    "c": "EF Core 8. Current user's TenantId is available via ICurrentTenantService. Need to occasionally query deleted records for admin purposes."
  },
  "function": "Implement EF Core global query filters for soft delete and multi-tenancy isolation.",
  "acceptedOutput": "**EF Core global query filters** apply a LINQ predicate to every query for an entity type — developers cannot accidentally forget the tenant/soft-delete filter.\n\n**Implementation:**\n```csharp\n// AppDbContext.cs\npublic class AppDbContext : DbContext\n{\n    private readonly ICurrentTenantService _tenant;\n\n    public AppDbContext(DbContextOptions<AppDbContext> opts,\n                        ICurrentTenantService tenant) : base(opts)\n        => _tenant = tenant;\n\n    protected override void OnModelCreating(ModelBuilder modelBuilder)\n    {\n        // Apply global filters to all IAuditable entities\n        foreach (var entityType in modelBuilder.Model.GetEntityTypes()\n            .Where(t => typeof(IAuditableEntity).IsAssignableFrom(t.ClrType)))\n        {\n            modelBuilder.Entity(entityType.ClrType)\n                .HasQueryFilter(BuildFilter(entityType.ClrType));\n        }\n    }\n\n    private LambdaExpression BuildFilter(Type entityType)\n    {\n        // Build: e => e.TenantId == _tenant.CurrentTenantId && !e.IsDeleted\n        var param = Expression.Parameter(entityType, \"e\");\n        var tenantId = Expression.Property(param, \"TenantId\");\n        var isDeleted = Expression.Property(param, \"IsDeleted\");\n\n        var tenantFilter  = Expression.Equal(tenantId,\n            Expression.Property(Expression.Constant(this), nameof(CurrentTenantId)));\n        var deleteFilter  = Expression.Not(isDeleted);\n        var combined      = Expression.AndAlso(tenantFilter, deleteFilter);\n        return Expression.Lambda(combined, param);\n    }\n\n    // EF Core reads this property for the filter — value updated per request\n    private Guid CurrentTenantId => _tenant.CurrentTenantId;\n}\n\n// Admin use: bypass soft-delete filter to see deleted records\npublic async Task<List<Order>> GetDeletedOrdersAsync()\n    => await _db.Orders\n        .IgnoreQueryFilters()  // bypasses BOTH tenant AND soft-delete filters\n        .Where(o => o.TenantId == _currentTenant.Id && o.IsDeleted)\n        .ToListAsync();\n```\n\n**Result:** every query on `Orders`, `Products`, `Customers` automatically includes `WHERE TenantId = @id AND IsDeleted = 0`. The tenant isolation incident cannot recur unless `IgnoreQueryFilters()` is explicitly called.\n\n**Trade-off:** `IgnoreQueryFilters()` bypasses ALL filters — if both tenant and soft-delete filters exist, admins must re-add the tenant filter manually after calling IgnoreQueryFilters.",
  "rejectedOutput": "Add a code review checklist item reminding developers to add tenant and soft-delete filters.",
  "arbiters": ["EFCORE-DOCS"],
  "whyAccepted": "Global query filters make the correct behavior the default — the tenant/soft-delete filter is impossible to forget because it is applied by the framework, not individual developers. It converts a human discipline problem into a structural enforcement.",
  "whyRejected": "Code review checklists are not enforcement mechanisms — they catch mistakes only if the reviewer notices, only if the PR goes through review, and only after the buggy code is written. Global query filters are a structural guarantee: if the filter is registered, it always applies.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/querying/filters"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-527
**registryId:** DOTNET-MICROSERVICES  
**unitTitle:** .NET Worker Service BackgroundService for scheduled tasks vs Hangfire  
**sourceUrl:** https://learn.microsoft.com/en-us/dotnet/core/extensions/workers  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-527-R1",
  "sourceIds": ["EXT-527"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "ASP.NET Core service needs to run a nightly data sync job at 2 AM. Options: (A) BackgroundService with a Timer, (B) Hangfire recurring job, (C) Azure Functions Timer Trigger.",
    "b": "Job runtime: ~20 minutes. Job must not run concurrently (distributed lock needed if multiple instances). Need visibility into job history.",
    "c": "Service is deployed as 3 replicas in Kubernetes. Uses Azure SQL Server. No existing job scheduler."
  },
  "function": "Select the correct scheduled job mechanism for the 3-replica Kubernetes deployment and implement.",
  "acceptedOutput": "**Recommendation: Hangfire with SQL Server storage** — provides distributed locking, job history, and dashboard without additional infrastructure.\n\n**BackgroundService + Timer limitations for this case:**\n- No distributed locking: all 3 replicas run the job simultaneously at 2 AM\n- No job history: cannot see if last night's sync succeeded or failed\n- No retry: if the job fails, it doesn't run again until the next night\n\n**Hangfire implementation (SQL Server storage — uses existing Azure SQL):**\n```csharp\n// Program.cs\nbuilder.Services.AddHangfire(config =>\n    config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)\n          .UseSimpleAssemblyNameTypeSerializer()\n          .UseSqlServerStorage(connectionString, new SqlServerStorageOptions\n          {\n              CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),\n              SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),\n              QueuePollInterval = TimeSpan.Zero,\n              UseRecommendedIsolationLevel = true,\n          })\n);\n\n// One server instance acquires the distributed lock — others skip\nbuilder.Services.AddHangfireServer(opts =>\n{\n    opts.WorkerCount = 2; // limit concurrent jobs on this instance\n});\n\n// Register recurring job at startup\nvar app = builder.Build();\nRecurringJob.AddOrUpdate<DataSyncJob>(\n    recurringJobId: \"nightly-data-sync\",\n    methodCall:     job => job.ExecuteAsync(CancellationToken.None),\n    cronExpression: Cron.Daily(hour: 2),  // 2:00 AM UTC\n    timeZone:       TimeZoneInfo.Utc\n);\n\n// DataSyncJob.cs\npublic class DataSyncJob(IDataSyncService syncService, ILogger<DataSyncJob> logger)\n{\n    [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 900 })] // retry after 5min, 15min\n    public async Task ExecuteAsync(CancellationToken ct)\n    {\n        logger.LogInformation(\"Starting nightly data sync\");\n        await syncService.SyncAsync(ct);\n        logger.LogInformation(\"Nightly data sync completed\");\n    }\n}\n```\n\n**Distributed locking:** Hangfire uses SQL Server to enqueue jobs — only one server instance picks up and executes the job. The other replicas see the job is already being processed and skip it.\n\n**Dashboard:** `app.MapHangfireDashboard(\"/hangfire\")` — shows job history, success/failure, retry count, duration.\n\n**Azure Functions Timer Trigger:** correct if the sync is already separate from the main service. Adds Azure Functions infrastructure. Appropriate when job requires independent scaling.",
  "rejectedOutput": "Use BackgroundService with a Timer — it's simpler and has no external dependencies.",
  "arbiters": ["HANGFIRE-DOCS", "DOTNET-WORKER-DOCS"],
  "whyAccepted": "Hangfire on SQL Server (already in use) provides the distributed locking required for 3-replica deployment, job history, and retry — all missing from BackgroundService+Timer. Using existing SQL Server avoids new infrastructure.",
  "whyRejected": "BackgroundService+Timer runs on every pod simultaneously in a 3-replica deployment — the nightly sync runs 3 times concurrently, corrupting data. Distributed locking requires additional infrastructure (Redis, distributed mutex) that Hangfire provides built-in via SQL Server.",
  "sourceAnchors": ["https://www.hangfire.io/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-528
**registryId:** MICROSOFT-LEARN-ARCH  
**unitTitle:** Azure Cosmos DB partition key selection for order management — hot partition avoidance  
**sourceUrl:** https://learn.microsoft.com/en-us/azure/cosmos-db/partitioning-overview  
**license:** CC-BY-4.0 | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-528-R1",
  "sourceIds": ["EXT-528"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F3-storage-distribution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Azure Cosmos DB container for orders. Current partition key: `/status` (pending, processing, completed, cancelled). Throughput: 10,000 RU/s. Problem: 95% of queries are for 'pending' and 'processing' orders — most RUs consumed from 2 out of 4 partitions. Other 2 partitions are nearly idle.",
    "b": "Hot partition problem: Cosmos DB distributes RUs evenly across partitions. Two partitions consume 9,500 RU/s but only have 5,000 RU/s each allocated — throttling at peak.",
    "c": "Access patterns: 70% by customerId (all orders for a customer), 20% by status + date range, 10% by orderId."
  },
  "function": "Recommend the correct partition key for the order container and implement the migration strategy.",
  "acceptedOutput": "**Hot partition root cause:** `/status` has only 4 distinct values — Cosmos DB creates at most 4 logical partitions. 95% of traffic goes to 2 logical partitions, exceeding their RU allocation.\n\n**Rule:** good partition keys have HIGH cardinality (many distinct values), distribute reads AND writes evenly, and align with the most common query pattern.\n\n**Recommended partition key: `/customerId`**\n\n**Why customerId:**\n- High cardinality (thousands of customers) → hundreds of logical partitions → even RU distribution\n- Aligns with primary access pattern (70% of queries are by customerId)\n- Single partition query for 'all orders for customer X' — efficient\n\n**Access pattern analysis with customerId partition:**\n```\n70% customerId queries → IN PARTITION (1 RU per doc read)\n20% status+date range → CROSS PARTITION scan (fan-out — acceptable for low-frequency admin queries)\n10% orderId → CROSS PARTITION point read if orderId ≠ customerId — consider using orderId as PK\n```\n\n**Document design:**\n```json\n{\n  \"id\": \"ord-123456\",          // orderId — unique within partition\n  \"customerId\": \"cust-789\",    // partition key — high cardinality\n  \"status\": \"pending\",\n  \"total\": 149.99,\n  \"createdAt\": \"2026-05-04T10:00:00Z\"\n}\n```\n\n**Query by customerId (efficient — in-partition):**\n```sql\nSELECT * FROM c WHERE c.customerId = 'cust-789'\n-- Single partition — O(1) regardless of total container size\n```\n\n**Query by status (cross-partition — acceptable for admin):**\n```sql\nSELECT * FROM c WHERE c.status = 'pending' ORDER BY c.createdAt\n-- Fan-out across all partitions — use Cosmos DB composite index for efficiency\n```\n\n**Migration strategy:** create new container with `/customerId` partition key, bulk-copy documents using Azure Data Factory or Cosmos DB change feed.\n\n**Alternative for orderId access pattern:** use synthetic partition key `customerId + orderId` if both single-customer queries and single-order lookups must be in-partition.",
  "rejectedOutput": "Use `/orderId` as partition key — each order gets its own partition, maximally distributed.",
  "arbiters": ["COSMOS-DB-DOCS", "MICROSOFT-LEARN-ARCH"],
  "whyAccepted": "customerId aligns with the dominant query pattern (70%), provides high cardinality for even RU distribution, and eliminates the hot partition problem. The cross-partition status query (20%) is acceptable for low-frequency admin access.",
  "whyRejected": "orderId as partition key eliminates hot partitions (maximum cardinality) but makes the most common query pattern (all orders for customerId) a cross-partition fan-out — the primary use case pays cross-partition cost. Partition key must align with the dominant access pattern.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/cosmos-db/partitioning-overview"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-529
**registryId:** CWE-TOP25  
**unitTitle:** CWE-798 Hardcoded Credentials — .NET secret scanning and Azure Key Vault migration  
**sourceUrl:** https://cwe.mitre.org/data/definitions/798.html  
**license:** CWE-TOU | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-529-R1",
  "sourceIds": ["EXT-529"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "negative-example",
  "input": {
    "a": "GitHub Actions CI pipeline scan finds hardcoded credentials in appsettings.json: `\"ConnectionStrings\": {\"Db\": \"Server=prod-sql;Database=Orders;User Id=sa;Password=MyP@ss123!\"}`. The file is committed to a public repository.",
    "b": "CWE-798 (Use of Hard-coded Credentials) — credentials in source control are accessible to anyone with repo access, appear in git history permanently, and cannot be rotated without code changes.",
    "c": "Service is deployed to Azure. Azure Key Vault is available. .NET 8."
  },
  "function": "Remove hardcoded credentials and implement Azure Key Vault + Managed Identity for secure secret management.",
  "acceptedOutput": "**Immediate remediation (credentials already committed):**\n1. Rotate the credentials NOW — the password `MyP@ss123!` is exposed and must be changed immediately\n2. Remove the credential from git history: `git filter-repo --path appsettings.json --invert-paths` (or use BFG Repo Cleaner)\n3. Force-push to all branches: `git push --force-with-lease --all`\n4. Notify all collaborators to re-clone — their local copies still have the credential\n\n**Migration to Azure Key Vault with Managed Identity:**\n```csharp\n// Program.cs — Azure Key Vault integration\nusing Azure.Extensions.AspNetCore.Configuration.Secrets;\nusing Azure.Identity;\n\nvar builder = WebApplication.CreateBuilder(args);\n\n// Add Azure Key Vault as configuration source\n// ManagedIdentityCredential: no credentials in code — uses the VM/pod identity\nvar keyVaultUri = new Uri($\"https://{builder.Configuration[\"KeyVaultName\"]}.vault.azure.net/\");\nbuilder.Configuration.AddAzureKeyVault(\n    keyVaultUri,\n    new DefaultAzureCredential(),  // uses Managed Identity in Azure, dev credentials locally\n    new KeyVaultSecretManager()    // maps secret names to config keys\n);\n\n// appsettings.json — NO credentials\n{\n  \"KeyVaultName\": \"my-prod-keyvault\",\n  \"ConnectionStrings\": {\n    // Value pulled from Key Vault secret named 'ConnectionStrings--Db' at runtime\n    \"Db\": \"\"  // placeholder — overridden by Key Vault configuration provider\n  }\n}\n```\n\n**Azure setup:**\n```bash\n# Create Key Vault secret\naz keyvault secret set --vault-name my-prod-keyvault \\\n  --name 'ConnectionStrings--Db' \\\n  --value 'Server=prod-sql;Database=Orders;User Id=app_user;Password=<rotated-password>'\n\n# Grant app's Managed Identity read access\naz keyvault set-policy --name my-prod-keyvault \\\n  --object-id <managed-identity-object-id> \\\n  --secret-permissions get list\n```\n\n**Local development:** use `dotnet user-secrets` — stored in user profile, not in repo:\n```bash\ndotnet user-secrets set 'ConnectionStrings:Db' 'Server=localhost;Database=Orders;...'\n```\n\n**Secret rotation:** update Key Vault secret value — running pods pick up new value on restart. No code deployment needed.",
  "rejectedOutput": "Move the connection string to environment variables — environment variables are secure.",
  "arbiters": ["CWE-TOP25", "AZURE-SECURITY-DOCS"],
  "whyAccepted": "Azure Key Vault + Managed Identity eliminates credentials from all code and configuration files. Managed Identity means no credentials are needed to access Key Vault — the Azure platform handles authentication. Secret rotation requires no code changes.",
  "whyRejected": "Environment variables are not inherently secure: they appear in process lists (`/proc/PID/environ`), Docker inspect output, Kubernetes pod specs (if set directly, not from Secret), and CI/CD logs. They are better than hardcoded strings but worse than Key Vault with Managed Identity.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/798.html"],
  "licenseConstraint": "CWE-TOU",
  "trainingStatus": "draft"
}
```

---

## EXT-530
**registryId:** OWASP-ASVS  
**unitTitle:** ASVS V5.1 input validation — allowlist vs denylist for user-supplied data  
**sourceUrl:** https://owasp.org/www-project-application-security-verification-standard/  
**license:** CC-BY-SA-3.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-530-R1",
  "sourceIds": ["EXT-530"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "OWASP ASVS V5.1.3 requires: 'Verify that all input validation is performed on a trusted service layer, and that validation uses positive (allowlist) validation techniques.' Current API endpoint accepts a `format` query parameter and blocks known-bad values using a denylist: `if format in ['../','%2e%2e','<script>', ...]`.",
    "b": "Security team flags the denylist approach — it cannot enumerate all attack vectors (new encoding variants, Unicode normalization attacks).",
    "c": "The format parameter should only accept: json, csv, xml. Three valid values."
  },
  "function": "Replace denylist validation with allowlist validation per ASVS V5.1.3 and implement in ASP.NET Core.",
  "acceptedOutput": "**ASVS V5.1.3 — allowlist (positive) validation** explicitly enumerates valid values — all other inputs are rejected. Denylist validation attempts to enumerate all invalid inputs — impossible to maintain as attack patterns evolve.\n\n**Allowlist implementation (ASP.NET Core):**\n```csharp\n// 1. Use an enum — compile-time exhaustive allowlist\npublic enum OutputFormat { Json, Csv, Xml }\n\n// 2. ASP.NET Core ModelBinder handles enum parsing + validation automatically\n[HttpGet(\"reports\")]\npublic async Task<IActionResult> GetReport([FromQuery] OutputFormat format = OutputFormat.Json)\n{\n    // format is guaranteed to be Json, Csv, or Xml — ASP.NET Core rejects anything else\n    // with a 400 Bad Request before the action method is called\n    return format switch\n    {\n        OutputFormat.Json => Ok(await BuildJsonReport()),\n        OutputFormat.Csv  => File(await BuildCsvReport(), \"text/csv\", \"report.csv\"),\n        OutputFormat.Xml  => Content(await BuildXmlReport(), \"application/xml\"),\n        // exhaustive switch — compiler warns if OutputFormat gains a new value\n        _ => BadRequest()\n    };\n}\n\n// Alternative: explicit string allowlist with FluentValidation\npublic class ReportQueryValidator : AbstractValidator<ReportQuery>\n{\n    private static readonly HashSet<string> ValidFormats =\n        new(StringComparer.OrdinalIgnoreCase) { \"json\", \"csv\", \"xml\" };\n\n    public ReportQueryValidator()\n    {\n        RuleFor(q => q.Format)\n            .Must(f => ValidFormats.Contains(f))\n            .WithMessage(\"Format must be one of: json, csv, xml\");\n    }\n}\n```\n\n**Why allowlist defeats encoding attacks:**\n- `?format=../../../etc/passwd` → not in {json,csv,xml} → rejected immediately\n- `?format=%2e%2e%2f` → URL-decoded to `../../` → not in allowlist → rejected\n- `?format=<script>` → not in allowlist → rejected\n- Any future attack variant → not in allowlist → rejected\n\n**Denylist failure mode:** attacker uses double URL encoding `%252e%252e` (decoded once by proxy → `%2e%2e`, decoded once by app → `../`) — may bypass naive denylist if only the first decoding is checked.\n\n**ASVS V5.1 principle:** denylist maintenance is a security treadmill — allowlist is declarative and complete by definition.",
  "rejectedOutput": "Expand the denylist — add more known-bad patterns and keep it updated.",
  "arbiters": ["OWASP-ASVS", "OWASP-INPUT-VALIDATION"],
  "whyAccepted": "Allowlist validation is complete by definition — any value not in {json, csv, xml} is rejected, regardless of encoding or obfuscation. Enum-based validation gives compile-time exhaustiveness checking. The encoding attack examples demonstrate why denylist cannot be maintained.",
  "whyRejected": "Expanding the denylist is a security treadmill — for every encoding variant added, new variants exist (double encoding, Unicode normalization, null bytes). The allowlist has exactly 3 valid values — there is no maintenance burden and no attack vector.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
