# EXT Batch 24 — EXT-471 through EXT-490

## EXT-471
**registryId:** KUBERNETES-KEPS  
**unitTitle:** Kubernetes VPA (Vertical Pod Autoscaler) vs manual resource requests for right-sizing  
**sourceUrl:** https://github.com/kubernetes/autoscaler/tree/master/vertical-pod-autoscaler  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-471-R1",
  "sourceIds": ["EXT-471"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-topology-change",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Kubernetes cluster has 20 deployments with manually set resource requests that were guessed 18 months ago. Cluster utilization shows 60% of pods have requests 3x higher than actual usage — wasting node capacity. Platform team wants to right-size them.",
    "b": "VPA (Vertical Pod Autoscaler) can recommend or automatically adjust resource requests based on historical usage. But VPA in Auto mode evicts and restarts pods to apply new requests.",
    "c": "Production services — pod restarts are disruptive. Some services are stateful (Redis, Postgres)."
  },
  "function": "Design a VPA-based right-sizing strategy that provides recommendations without disruptive auto-eviction for production services.",
  "acceptedOutput": "**VPA Recommendation-only mode** provides right-sizing data without automatic pod eviction — the team manually applies recommendations during planned maintenance windows.\n\n**VPA manifest (Off mode — recommendations only, no automatic updates):**\n```yaml\napiVersion: autoscaling.k8s.io/v1\nkind: VerticalPodAutoscaler\nmetadata:\n  name: orders-api-vpa\n  namespace: production\nspec:\n  targetRef:\n    apiVersion: apps/v1\n    kind: Deployment\n    name: orders-api\n  updatePolicy:\n    updateMode: \"Off\"  # Recommend only — no automatic pod evictions\n  resourcePolicy:\n    containerPolicies:\n    - containerName: orders\n      minAllowed: { cpu: \"50m\", memory: \"64Mi\" }   # floor — avoid under-provisioning\n      maxAllowed: { cpu: \"4\",   memory: \"4Gi\" }    # ceiling — prevent runaway recommendation\n```\n\n**Reading recommendations:**\n```bash\nkubectl get vpa orders-api-vpa -o jsonpath='{.status.recommendation}'\n# Output: {\"containerRecommendations\":[{\"containerName\":\"orders\",\n#   \"lowerBound\":{\"cpu\":\"120m\",\"memory\":\"180Mi\"},\n#   \"target\":{\"cpu\":\"200m\",\"memory\":\"256Mi\"},\n#   \"upperBound\":{\"cpu\":\"400m\",\"memory\":\"512Mi\"}}]}\n```\n\n**Right-sizing workflow:**\n1. Deploy VPA in `Off` mode for all 20 deployments\n2. Wait 7 days — VPA collects Prometheus metrics and builds a histogram\n3. Review `target` recommendations — apply the `target` value to `requests`, `upperBound` as `limits`\n4. Apply during rolling restart (`kubectl rollout restart deployment/orders-api`) — zero-downtime with PodDisruptionBudget\n\n**For stateful services (Redis, Postgres):** use `updateMode: \"Off\"` permanently — never use VPA Auto mode with StatefulSets.\n\n**Expected result:** cluster requests reduced by ~50%, freeing capacity for 10+ new deployments without new nodes.",
  "rejectedOutput": "Use VPA in Auto mode — it handles right-sizing automatically without manual intervention.",
  "arbiters": ["KUBERNETES-VPA-DOCS", "KUBERNETES-KEPS"],
  "whyAccepted": "VPA Off mode is the correct choice for production right-sizing — it provides data-driven recommendations without the disruptive eviction behavior of Auto mode. The 7-day wait for histogram data and rolling restart application are concrete, safe steps.",
  "whyRejected": "VPA Auto mode evicts pods immediately when it determines new resource requests are needed — in a production service with no PodDisruptionBudget, this can take down all replicas simultaneously. For stateful services, Auto mode can corrupt state. The convenience does not justify the risk.",
  "sourceAnchors": ["https://github.com/kubernetes/autoscaler/tree/master/vertical-pod-autoscaler"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-472
**registryId:** BIGCODEBENCH  
**unitTitle:** Python typing.Protocol for structural subtyping vs ABC for nominal interface enforcement  
**sourceUrl:** https://github.com/bigcode-project/bigcodebench  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-472-R1",
  "sourceIds": ["EXT-472"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Python service has an `IRepository` abstract base class (ABC) with abstract methods `get`, `save`, `delete`. Third-party library returns objects that implement the same methods but don't inherit from `IRepository`. Code that accepts `IRepository` rejects them at runtime.",
    "b": "Developer wants to accept any object that implements the required methods without requiring inheritance. Python 3.8+ `typing.Protocol` supports this.",
    "c": "The codebase has both internal implementations (inherit from IRepository) and third-party adapters (cannot modify to add inheritance)."
  },
  "function": "Compare ABC vs typing.Protocol for Python interface enforcement and produce the Protocol-based solution.",
  "acceptedOutput": "**typing.Protocol** implements structural subtyping (duck typing with static checking) — any class that implements the required methods satisfies the protocol, regardless of inheritance.\n\n**Before (ABC — nominal subtyping):**\n```python\nfrom abc import ABC, abstractmethod\n\nclass IRepository(ABC):\n    @abstractmethod\n    def get(self, id: int) -> dict: ...\n    @abstractmethod\n    def save(self, obj: dict) -> None: ...\n    @abstractmethod\n    def delete(self, id: int) -> None: ...\n\n# Third-party class — does NOT inherit IRepository\nclass ThirdPartyStore:\n    def get(self, id: int) -> dict: return {}\n    def save(self, obj: dict) -> None: pass\n    def delete(self, id: int) -> None: pass\n\ndef process(repo: IRepository) -> None: ...  # ThirdPartyStore rejected at runtime\n```\n\n**After (Protocol — structural subtyping):**\n```python\nfrom typing import Protocol, runtime_checkable\n\n@runtime_checkable  # enables isinstance() checks\nclass Repository(Protocol):\n    def get(self, id: int) -> dict: ...\n    def save(self, obj: dict) -> None: ...\n    def delete(self, id: int) -> None: ...\n\n# No changes needed to ThirdPartyStore — it satisfies the protocol structurally\nclass ThirdPartyStore:  # does NOT inherit Repository\n    def get(self, id: int) -> dict: return {}\n    def save(self, obj: dict) -> None: pass\n    def delete(self, id: int) -> None: pass\n\ndef process(repo: Repository) -> None: ...  # ThirdPartyStore accepted — mypy verifies\n\n# Both internal and third-party implementations work\nprocess(InternalRepository())  # inherits nothing, implements methods\nprocess(ThirdPartyStore())     # third-party, no inheritance needed\n\n# Runtime check (requires @runtime_checkable)\nassert isinstance(ThirdPartyStore(), Repository)  # True\n```\n\n**Decision rule:**\n- Use **ABC** when you need enforced inheritance hierarchy, abstract method enforcement at class definition time, or `super()` mixin patterns.\n- Use **Protocol** when you accept third-party implementations, want duck typing with static verification, or are designing library APIs that shouldn't force inheritance.",
  "rejectedOutput": "Use duck typing with no type annotations — Python is dynamically typed, just call the methods and handle AttributeError.",
  "arbiters": ["PYTHON-DOCS", "BIGCODEBENCH"],
  "whyAccepted": "Protocol provides static type safety (mypy verifies at type-check time) without inheritance constraints. @runtime_checkable adds isinstance() support for defensive runtime checks. It is the PEP-544 solution to exactly this problem.",
  "whyRejected": "Untyped duck typing loses mypy static checking — incorrect method signatures or missing methods are not caught until runtime. For a service boundary like IRepository, static type verification is valuable. Protocol gives both duck typing flexibility and type safety.",
  "sourceAnchors": ["https://docs.python.org/3/library/typing.html#typing.Protocol"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-473
**registryId:** SWE-BENCH  
**unitTitle:** SQLAlchemy 2.0 session.execute vs legacy Query API migration  
**sourceUrl:** https://github.com/princeton-nlp/SWE-bench  
**license:** MIT | **trustTier:** 2  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-473-R1",
  "sourceIds": ["EXT-473"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-training-stability",
  "rowClass": "migration-decision",
  "input": {
    "a": "SQLAlchemy 2.0 removes the legacy `session.query(Model).filter(...)` Query API (deprecated in 1.4, removed in 2.0). Application uses `session.query(User).filter(User.id == uid).first()` in 140 places.",
    "b": "Migration to `session.execute(select(User).where(User.id == uid)).scalar_one_or_none()` is required. Team wants to understand the semantic differences before bulk migration.",
    "c": "Using Python 3.11, SQLAlchemy 2.0, PostgreSQL."
  },
  "function": "Document the SQLAlchemy 1.x to 2.0 Query API migration with semantic equivalences and common patterns.",
  "acceptedOutput": "**SQLAlchemy 2.0 migration** replaces the legacy Query API with `session.execute()` + Core `select()` statements.\n\n**Common pattern equivalences:**\n\n```python\nfrom sqlalchemy import select, update, delete\nfrom sqlalchemy.orm import Session\n\n# 1. Single object by primary key\n# Legacy:   session.query(User).get(uid)\n# 2.0:      session.get(User, uid)  # session.get is preserved in 2.0\nuser = session.get(User, uid)\n\n# 2. Filter + first\n# Legacy:   session.query(User).filter(User.email == email).first()\n# 2.0:\nuser = session.execute(\n    select(User).where(User.email == email)\n).scalar_one_or_none()  # None if not found; raises if multiple rows\n\n# 3. All results\n# Legacy:   session.query(User).filter(User.active == True).all()\n# 2.0:\nusers = session.execute(\n    select(User).where(User.active == True)\n).scalars().all()\n\n# 4. Count\n# Legacy:   session.query(User).filter(User.active == True).count()\n# 2.0:\nfrom sqlalchemy import func\ncount = session.execute(\n    select(func.count()).select_from(User).where(User.active == True)\n).scalar()\n\n# 5. JOIN\n# Legacy:   session.query(Order).join(User).filter(User.id == uid).all()\n# 2.0:\norders = session.execute(\n    select(Order).join(Order.user).where(User.id == uid)\n).scalars().all()\n\n# 6. Bulk update\n# Legacy:   session.query(User).filter(User.role == 'guest').update({'active': False})\n# 2.0:\nsession.execute(\n    update(User).where(User.role == 'guest').values(active=False)\n)\n```\n\n**Key semantic differences:**\n- `scalar_one_or_none()` vs `first()`: `scalar_one_or_none()` raises `MultipleResultsFound` if >1 row — safer for unique lookups.\n- `scalars()` unwraps the Row object; without it, `execute()` returns `Row` tuples.\n- `session.get(Model, pk)` is preserved in 2.0 — no change needed for PK lookups.\n\n**Migration tooling:** run `sqlalchemy-migrate-legacy-query` (community tool) to scan for `session.query()` patterns.",
  "rejectedOutput": "Add `sqlalchemy-1.x-compat` shim — it re-enables the legacy Query API in SQLAlchemy 2.0.",
  "arbiters": ["SQLALCHEMY-DOCS", "SWE-BENCH"],
  "whyAccepted": "The migration table gives concrete, pattern-by-pattern equivalences covering all common Query API usages. The semantic differences (MultipleResultsFound safety, scalars() unwrapping) are non-obvious and worth documenting.",
  "whyRejected": "Using a compatibility shim defers the migration and accumulates technical debt. The shim is not maintained by SQLAlchemy — it will break on future patch releases. The correct approach is migration, not indefinite compatibility shims.",
  "sourceAnchors": ["https://docs.sqlalchemy.org/en/20/orm/queryguide/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-474
**registryId:** PYTHON-PEPS  
**unitTitle:** PEP-696 TypeVar defaults for generic class base patterns  
**sourceUrl:** https://peps.python.org/pep-0696/  
**license:** PSF-2.0 | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-474-R1",
  "sourceIds": ["EXT-474"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-execution-setup",
  "rowClass": "migration-decision",
  "input": {
    "a": "Python 3.13 introduces PEP-696 TypeVar defaults, allowing generic classes to have default type arguments: `class Stack[T = int]: ...` means `Stack()` is treated as `Stack[int]` by type checkers.",
    "b": "Library has a generic `Result[T, E]` class used in 300 call sites. 250 of those sites use `Result[SomeData, AppError]` — AppError is always the error type. Developer wants to reduce the boilerplate to `Result[SomeData]` at those 250 sites.",
    "c": "Library targets Python 3.12+. mypy version must support PEP-696."
  },
  "function": "Implement PEP-696 TypeVar defaults to add a default error type to Result[T, E].",
  "acceptedOutput": "**PEP-696 TypeVar defaults** (Python 3.13+, or `typing_extensions` for 3.12) allow specifying a default type argument that is used when the type parameter is omitted.\n\n**Before (all 300 call sites must specify both type args):**\n```python\nfrom typing import Generic, TypeVar\n\nT = TypeVar('T')\nE = TypeVar('E')\n\nclass Result(Generic[T, E]):\n    def __init__(self, value: T | None, error: E | None): ...\n\n# All 250 sites must write Result[SomeData, AppError]\nresult: Result[UserData, AppError] = get_user(uid)\n```\n\n**After (PEP-696 default for E):**\n```python\n# Python 3.13 native syntax\nfrom typing import Generic\n\nclass Result[T, E = AppError]:  # E defaults to AppError\n    def __init__(self, value: T | None = None, error: E | None = None): ...\n\n    @classmethod\n    def ok(cls, value: T) -> 'Result[T, E]': ...\n    @classmethod\n    def err(cls, error: E) -> 'Result[T, E]': ...\n\n# For Python 3.12 with typing_extensions:\nfrom typing_extensions import TypeVar\nE = TypeVar('E', default=AppError)  # typing_extensions supports PEP-696\n\n# Usage — 250 sites simplified:\nresult: Result[UserData] = get_user(uid)       # E inferred as AppError\nresult2: Result[UserData, NetworkError] = ...  # explicit override still works\n```\n\n**mypy support:** requires mypy ≥ 1.4 for `typing_extensions` TypeVar defaults. Native `class Foo[T = int]` syntax requires mypy ≥ 1.10.\n\n**Migration approach:**\n1. Add `typing_extensions` for Python 3.12 compatibility\n2. Change `E = TypeVar('E')` to `E = TypeVar('E', default=AppError)`\n3. Simplify 250 call sites: remove `, AppError` from annotations — mypy will verify no regressions",
  "rejectedOutput": "Create a `AppResult = Result[T, AppError]` type alias — it achieves the same simplification without PEP-696.",
  "arbiters": ["PYTHON-PEPS", "MYPY-DOCS"],
  "whyAccepted": "PEP-696 TypeVar defaults is the correct tool for this pattern — it directly addresses the boilerplate problem at the generic class definition level. The typing_extensions path provides Python 3.12 compatibility without waiting for 3.13.",
  "whyRejected": "A type alias `AppResult = Result[T, AppError]` requires a new TypeVar T in the alias context, which is not valid in Python type aliases without `TypeAlias` and `TypeVar` re-declaration. The syntax is verbose and mypy handling of generic aliases has edge cases. PEP-696 is cleaner.",
  "sourceAnchors": ["https://peps.python.org/pep-0696/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-475
**registryId:** OPENTELEMETRY-SPEC  
**unitTitle:** OTel semantic conventions for database spans — db.system, db.operation, db.statement  
**sourceUrl:** https://opentelemetry.io/docs/specs/semconv/database/  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-475-R1",
  "sourceIds": ["EXT-475"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-evidence-linkage",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "OTel-instrumented .NET service shows database spans in Jaeger with no attributes — only span duration and name 'Query'. Team cannot identify which SQL query is slow without reading application logs.",
    "b": "OTel semantic conventions define required and recommended attributes for database spans: db.system, db.name, db.operation, db.statement.",
    "c": "Using SqlClient (.NET) with manual OTel span creation. Automatic SqlClient instrumentation was not configured."
  },
  "function": "Configure OTel database span attributes per semantic conventions to enable query-level performance analysis.",
  "acceptedOutput": "**OTel Database Semantic Conventions** define standard attributes for database spans that enable query-level observability across all backends.\n\n**Required attributes (all database spans):**\n- `db.system`: database type (`mssql`, `postgresql`, `redis`, etc.)\n- `db.name`: database name\n- `db.operation`: SQL operation (`SELECT`, `INSERT`, `UPDATE`)\n- `db.statement`: the SQL statement (**CAUTION**: may contain PII — sanitize before logging)\n\n**Option 1 — Use automatic SqlClient instrumentation (preferred):**\n```csharp\n// Program.cs — enables automatic SqlClient span creation\nbuilder.Services.AddOpenTelemetry()\n    .WithTracing(tracing => tracing\n        .AddSqlClientInstrumentation(opts =>\n        {\n            opts.SetDbStatementForText = true; // include SQL text in db.statement\n            opts.RecordException = true;       // record exceptions as span events\n        })\n        // Automatically creates spans with all semantic convention attributes\n    );\n// No manual span creation needed — SqlClient instrumentation handles it\n```\n\n**Option 2 — Manual span creation with semantic conventions:**\n```csharp\nusing var span = tracer.StartActiveSpan(\"orders.select\",\n    kind: SpanKind.Client);\nspan.SetAttribute(\"db.system\",    \"mssql\");\nspan.SetAttribute(\"db.name\",      \"OrdersDb\");\nspan.SetAttribute(\"db.operation\", \"SELECT\");\n// Sanitize db.statement — remove literal values to prevent PII exposure\nspan.SetAttribute(\"db.statement\",\n    \"SELECT * FROM Orders WHERE CustomerId = @id\"); // parameterized form only\nspan.SetAttribute(\"db.sql.table\", \"Orders\"); // optional but useful\nspan.SetAttribute(\"net.peer.name\", \"sql-server.internal\");\nspan.SetAttribute(\"net.peer.port\", 1433);\n\ntry { /* execute query */ span.SetStatus(SpanStatus.Ok); }\ncatch (Exception ex) { span.RecordException(ex); span.SetStatus(SpanStatus.Error); }\n```\n\n**Security — db.statement sanitization:** use parameterized query text only (the template with `@id`), never the executed SQL with literal values (`WHERE CustomerId = 42`). Literal values may contain PII.",
  "rejectedOutput": "Log slow queries to application logs — then correlate with traces manually using the request ID.",
  "arbiters": ["OPENTELEMETRY-SPEC", "OTEL-SEMANTIC-CONVENTIONS"],
  "whyAccepted": "OTel database semantic conventions provide structured, queryable span attributes that enable direct query performance analysis in Jaeger/Tempo without log correlation. AddSqlClientInstrumentation auto-populates all required attributes.",
  "whyRejected": "Manual log-to-trace correlation requires additional tooling, is error-prone, and loses the performance advantage of trace-native analysis. The OTel semantic convention approach makes slow queries directly visible in the trace timeline.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/semconv/database/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-476
**registryId:** DAPPER  
**unitTitle:** Dapper async with CancellationToken for long-running query cancellation  
**sourceUrl:** https://github.com/DapperLib/Dapper  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** D | **familiesTagged:** 3

### Training Row

```json
{
  "rowId": "EXT-476-R1",
  "sourceIds": ["EXT-476"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-execution-concurrency",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "ASP.NET Core API has a Dapper query that takes 3–8 seconds for complex reports. When the user navigates away or the HTTP client times out, the server continues executing the query for the full duration — wasting DB connection pool slots.",
    "b": "Developer uses `await conn.QueryAsync<ReportRow>(sql, parameters)` without passing a CancellationToken. The ASP.NET Core request CancellationToken is available from the controller's HttpContext.",
    "c": "SQL Server. Dapper 2.1."
  },
  "function": "Identify the missing CancellationToken anti-pattern and implement cooperative cancellation for the Dapper query.",
  "acceptedOutput": "**Anti-pattern:** not propagating `CancellationToken` through Dapper async methods — when the HTTP client disconnects (request cancelled), the database query continues running until completion, holding a connection pool slot unnecessarily.\n\n**Dapper async methods accept CancellationToken via `CommandDefinition`:**\n```csharp\n[HttpGet(\"reports/monthly\")]\npublic async Task<IActionResult> GetMonthlyReport(\n    CancellationToken cancellationToken)  // ASP.NET Core injects request CT\n{\n    await using var conn = await _connectionFactory.CreateAsync(cancellationToken);\n\n    var command = new CommandDefinition(\n        commandText: _sql.MonthlyReport,\n        parameters: new { Year = DateTime.UtcNow.Year },\n        commandTimeout: 30,           // 30-second hard timeout as backstop\n        cancellationToken: cancellationToken  // propagated from HTTP request\n    );\n\n    try\n    {\n        var rows = await conn.QueryAsync<ReportRow>(command);\n        return Ok(rows);\n    }\n    catch (OperationCanceledException)\n    {\n        // Client disconnected — log and return 499 (client closed request)\n        _logger.LogInformation(\"Monthly report query cancelled by client\");\n        return StatusCode(499);\n    }\n    catch (SqlException ex) when (ex.Number == -2) // SQL timeout\n    {\n        return StatusCode(504, \"Report generation timed out\");\n    }\n}\n```\n\n**How cancellation works with SQL Server:** when `CancellationToken` is cancelled, Dapper sends a `SqlCommand.Cancel()` which issues a SQL `KILL` for the executing query on SQL Server — the database actually stops executing the query, freeing server resources.\n\n**CommandDefinition vs method overload:** Dapper 2.x `QueryAsync(string sql, object param, CancellationToken ct)` overload also exists — `CommandDefinition` is preferred when setting timeout + CT together.",
  "rejectedOutput": "Set a short commandTimeout on the connection — this will automatically kill long-running queries.",
  "arbiters": ["DAPPER-DOCS", "ASPNETCORE-DOCS"],
  "whyAccepted": "CancellationToken propagation is the correct cooperative cancellation mechanism — it cancels when the client disconnects, not after a fixed timeout. commandTimeout is a backstop, not a substitute for CT propagation.",
  "whyRejected": "commandTimeout kills ALL queries after the timeout regardless of client status — a fast client gets killed if the query takes 31 seconds even if it's still waiting. CancellationToken cancels specifically when the client disconnects, preserving successful long-running reports.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#cancellationtoken-support"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-477
**registryId:** ANGULAR-DOCS  
**unitTitle:** Angular standalone components migration from NgModule — incremental vs big-bang  
**sourceUrl:** https://angular.dev/reference/migrations/standalone  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-477-R1",
  "sourceIds": ["EXT-477"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-training-stability",
  "rowClass": "migration-decision",
  "input": {
    "a": "Angular 14+ introduced standalone components. Large Angular 13 app (120 components, 15 NgModules) wants to migrate to standalone architecture. Team debates incremental migration vs rewriting all components at once.",
    "b": "Angular CLI provides `ng generate @angular/core:standalone` schematic for automated migration. Angular supports mixing NgModule and standalone components.",
    "c": "App uses lazy-loaded feature modules. Some shared modules (SharedModule, MaterialModule) are imported in 12+ feature modules."
  },
  "function": "Design the NgModule to standalone migration strategy for this 120-component Angular app.",
  "acceptedOutput": "**Recommended: incremental migration using the Angular CLI schematic** — Angular officially supports NgModule + standalone coexistence, so you can migrate leaf components first without breaking the app.\n\n**Migration phases:**\n\n**Phase 1 — Convert leaf components (no template dependencies on other NgModule components):**\n```bash\n# Angular CLI standalone migration schematic\nng generate @angular/core:standalone\n\n# Or target a specific component:\n# Edit component decorator manually:\n```\n```typescript\n// Before (NgModule component)\n@Component({ selector: 'app-user-card', templateUrl: './user-card.component.html' })\nexport class UserCardComponent implements OnInit { ... }\n\n// After (standalone)\n@Component({\n  selector: 'app-user-card',\n  standalone: true,                // key flag\n  imports: [CommonModule, RouterLink, AsyncPipe], // explicit imports replace NgModule\n  templateUrl: './user-card.component.html',\n})\nexport class UserCardComponent implements OnInit { ... }\n```\n\n**Phase 2 — Migrate SharedModule consumers:**\nFor components that import SharedModule (Material, common pipes) — convert SharedModule to a standalone-friendly barrel:\n```typescript\n// Instead of SharedModule, create re-export arrays:\nexport const SHARED_IMPORTS = [\n  MatButtonModule, MatInputModule, MatTableModule,\n  AsyncPipe, DatePipe, DecimalPipe\n] as const;\n// Standalone components: imports: [...SHARED_IMPORTS]\n```\n\n**Phase 3 — Convert feature modules to standalone routes:**\n```typescript\n// Before: loadChildren: () => import('./orders/orders.module').then(m => m.OrdersModule)\n// After: loadChildren: () => import('./orders/orders.routes').then(r => r.ORDERS_ROUTES)\n```\n\n**Phase 4 — Bootstrap migration:**\n```typescript\n// main.ts — switch from bootstrapModule to bootstrapApplication\nbootstrapApplication(AppComponent, {\n  providers: [\n    provideRouter(APP_ROUTES),\n    provideHttpClient(),\n    importProvidersFrom(BrowserAnimationsModule),\n  ]\n});\n```\n\n**Timeline:** 120 components × 15 min average = ~30 hours of migration work. Use schematic to automate ~70%.",
  "rejectedOutput": "Rewrite all 120 components at once — a big-bang migration avoids maintaining two architectural styles simultaneously.",
  "arbiters": ["ANGULAR-DOCS", "ANGULAR-STANDALONE-GUIDE"],
  "whyAccepted": "Incremental migration is the Angular team's recommended approach. The CLI schematic automates the mechanical parts. Mixing NgModule and standalone components is officially supported. Big-bang risks breaking a 120-component production app in a single PR.",
  "whyRejected": "Big-bang migration of 120 components creates a single massive PR with 120 changed files — impossible to review, high risk of merge conflicts, and if a bug is introduced it touches every component at once. Incremental migration is strictly safer.",
  "sourceAnchors": ["https://angular.dev/reference/migrations/standalone"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-478
**registryId:** REACT-ADVANCED  
**unitTitle:** React context performance — context splitting vs single large context re-render  
**sourceUrl:** https://react.dev/reference/react/createContext  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-478-R1",
  "sourceIds": ["EXT-478"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-result-selection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "React app has a single AppContext containing: user, theme, cart, notifications (4 separate values). When the cart updates (user adds an item), ALL 200 components that consume AppContext re-render — including the user profile, theme, and notification components that don't use the cart.",
    "b": "React DevTools Profiler shows 200 re-renders per cart update. Only 8 components actually use the cart.",
    "c": "Current implementation: `const AppContext = createContext({user, theme, cart, notifications})`."
  },
  "function": "Identify the context re-render anti-pattern and implement context splitting to limit re-renders.",
  "acceptedOutput": "**Anti-pattern:** monolithic context — any change to any value in the context object triggers re-renders in ALL consumers, regardless of which value they use. React compares context value by reference — a new `{user, theme, cart, notifications}` object (created on every render) always triggers re-renders.\n\n**Fix: split contexts by update frequency:**\n```tsx\n// contexts/index.ts — separate contexts\nexport const UserContext    = createContext<User | null>(null);\nexport const ThemeContext   = createContext<Theme>('light');\nexport const CartContext    = createContext<Cart>({ items: [], total: 0 });\nexport const NotifContext   = createContext<Notification[]>([]);\n\n// App.tsx — stack providers (only the relevant provider re-renders)\nexport function AppProviders({ children }: { children: React.ReactNode }) {\n  const [user, setUser]    = useState<User | null>(null);\n  const [theme, setTheme]  = useState<Theme>('light');\n  const [cart, setCart]    = useState<Cart>({ items: [], total: 0 });\n  const [notifs, setNotifs] = useState<Notification[]>([]);\n\n  return (\n    <UserContext.Provider value={user}>\n      <ThemeContext.Provider value={theme}>\n        {/* CartContext updates only re-render CartContext consumers */}\n        <CartContext.Provider value={cart}>\n          <NotifContext.Provider value={notifs}>\n            {children}\n          </NotifContext.Provider>\n        </CartContext.Provider>\n      </ThemeContext.Provider>\n    </UserContext.Provider>\n  );\n}\n\n// CartIcon.tsx — only re-renders on cart changes\nfunction CartIcon() {\n  const cart = useContext(CartContext); // only subscribes to CartContext\n  return <span>{cart.items.length}</span>;\n}\n\n// UserProfile.tsx — NOT affected by cart updates\nfunction UserProfile() {\n  const user = useContext(UserContext); // only subscribes to UserContext\n  return <div>{user?.name}</div>;\n}\n```\n\n**Result:** cart update → only 8 CartContext consumers re-render vs 200 previously. 192 unnecessary re-renders eliminated per cart action.\n\n**Alternative for complex state:** use `useMemo` to stabilize context value references:\n```tsx\nconst cartValue = useMemo(() => ({ items, total }), [items, total]);\n<CartContext.Provider value={cartValue}>\n```",
  "rejectedOutput": "Wrap all context consumers in React.memo — memo prevents re-renders when props haven't changed.",
  "arbiters": ["REACT-DOCS", "REACT-ADVANCED"],
  "whyAccepted": "Context splitting eliminates the root cause — only the changed context triggers re-renders. The 200 → 8 re-render reduction is concrete and measurable.",
  "whyRejected": "React.memo prevents re-renders when props haven't changed, but context is not a prop — context changes bypass memo. Components that use useContext will re-render regardless of React.memo when the context value changes. Memo cannot fix context performance issues.",
  "sourceAnchors": ["https://react.dev/reference/react/createContext"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-479
**registryId:** ASPNETCORE-DOCS-SAMPLES  
**unitTitle:** ASP.NET Core output caching vs response caching middleware for API responses  
**sourceUrl:** https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-479-R1",
  "sourceIds": ["EXT-479"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F2-storage-performance",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "ASP.NET Core 8 API endpoint `GET /products/featured` returns 50 products that change once per hour. Currently called 10,000 times/hour. Each call executes a DB query taking 45 ms.",
    "b": "Developer wants to cache the response for 60 minutes. Two options: `[ResponseCache]` attribute (HTTP Cache-Control headers) vs `[OutputCache]` attribute (.NET 7+ server-side cache).",
    "c": "API is behind a CDN (Content Delivery Network). Clients are browser SPAs and mobile apps."
  },
  "function": "Compare [ResponseCache] vs [OutputCache] for this scenario and recommend with implementation.",
  "acceptedOutput": "**Key distinction:**\n- `[ResponseCache]`: sets HTTP `Cache-Control` headers — caching happens in the **client or CDN**, not the server. Server still executes the action on every cache miss.\n- `[OutputCache]`: caches the response **on the server** — action is not executed for cache hits. Also sets cache headers for downstream caching.\n\n**For this scenario: use `[OutputCache]` for server-side caching + configure CDN:**\n```csharp\n// Program.cs\nbuilder.Services.AddOutputCache(opts =>\n{\n    opts.AddPolicy(\"FeaturedProducts\",\n        p => p.Expire(TimeSpan.FromHours(1))\n               .Tag(\"products\")  // allows cache invalidation by tag\n    );\n});\n// ...\napp.UseOutputCache(); // must be after UseRouting, before MapControllers\n\n// ProductsController.cs\n[HttpGet(\"products/featured\")]\n[OutputCache(PolicyName = \"FeaturedProducts\")]\npublic async Task<IActionResult> GetFeatured()\n{\n    // This executes at most once per hour — result cached server-side\n    var products = await _productService.GetFeaturedAsync();\n    return Ok(products);\n}\n\n// Cache invalidation when products change:\npublic async Task UpdateFeaturedProductsAsync()\n{\n    await _outputCacheStore.EvictByTagAsync(\"products\", default);\n    // Next request after eviction rebuilds the cache\n}\n```\n\n**Combined strategy (server + CDN):**\n- `[OutputCache]` handles server-side caching: DB hit reduced from 10,000/hr to 1/hr\n- CDN caches based on `Cache-Control: public, max-age=3600` header (OutputCache sets this automatically)\n- Browser clients also cache for 1 hour\n\n**Latency:** first request: 45 ms DB + response building. Subsequent requests: <1 ms (memory cache lookup).\n\n**[ResponseCache] alone is insufficient** if the CDN is not configured — every origin request still hits the server.",
  "rejectedOutput": "Use [ResponseCache] with max-age=3600 — the CDN will handle caching.",
  "arbiters": ["ASPNETCORE-DOCS", "CDN-CACHING-DOCS"],
  "whyAccepted": "OutputCache provides server-side caching with tag-based invalidation — essential for proactive cache busting when products change. ResponseCache alone requires the CDN to be configured correctly and cannot be invalidated server-side.",
  "whyRejected": "ResponseCache sets headers only — if the CDN has a misconfiguration or the client bypasses the CDN, every request hits the server. OutputCache is a defense in depth: even if CDN caching fails, the server-side cache protects the database.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-480
**registryId:** NESTJS-ADVANCED  
**unitTitle:** NestJS Passport JWT strategy vs manual JWT verification for auth guard  
**sourceUrl:** https://docs.nestjs.com/security/authentication  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-480-R1",
  "sourceIds": ["EXT-480"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "NestJS API needs JWT authentication. Developer implemented a custom guard that manually calls `jwt.verify(token, secret)` from the `jsonwebtoken` package. Tech lead suggests using Passport.js JwtStrategy instead.",
    "b": "Custom guard: extracts Bearer token from Authorization header, calls `jwt.verify()`, attaches decoded payload to `req.user`. It works but the tech lead says it misses important edge cases.",
    "c": "API will later add OAuth 2.0 (Google, GitHub) as additional auth methods."
  },
  "function": "Compare manual JWT verification vs Passport JwtStrategy for NestJS and recommend considering future OAuth requirements.",
  "acceptedOutput": "**Passport JwtStrategy** is recommended over manual JWT verification when OAuth 2.0 strategies will be added — it provides a unified auth framework where multiple strategies (JWT, Google OAuth, GitHub OAuth) coexist without duplicating guard logic.\n\n**What manual jwt.verify() misses:**\n1. Token expiry: `jwt.verify()` throws on expiry — must catch and convert to 401\n2. Algorithm confusion: manual verify may not enforce `algorithms: ['RS256']` — vulnerable to `alg: 'none'` attack if not specified\n3. Clock skew: `jwt.verify()` has no built-in clock skew tolerance (`clockTolerance` option)\n4. JWKS (JSON Web Key Set) support: for Google/GitHub OAuth, tokens are verified against a remote JWKS endpoint — Passport handles this; manual verification requires additional code\n\n**Passport JwtStrategy implementation:**\n```typescript\n// jwt.strategy.ts\n@Injectable()\nexport class JwtStrategy extends PassportStrategy(Strategy) {\n  constructor(private readonly config: ConfigService) {\n    super({\n      jwtFromRequest: ExtractJwt.fromAuthHeaderAsBearerToken(),\n      ignoreExpiration: false,         // enforce token expiry\n      secretOrKey: config.get('JWT_SECRET'),\n      algorithms: ['HS256'],           // enforce algorithm — prevents alg:none attack\n    });\n  }\n\n  // validate() is called with the verified, decoded payload\n  async validate(payload: JwtPayload): Promise<AuthUser> {\n    const user = await this.usersService.findById(payload.sub);\n    if (!user || !user.active)\n      throw new UnauthorizedException('User not found or inactive');\n    return user; // attached to req.user\n  }\n}\n\n// jwt-auth.guard.ts\n@Injectable()\nexport class JwtAuthGuard extends AuthGuard('jwt') {}\n\n// Adding Google OAuth later — same guard pattern, different strategy:\n// @Injectable() export class GoogleAuthGuard extends AuthGuard('google') {}\n```\n\n**Migration:** remove custom guard, add `PassportModule` + `JwtModule` + `JwtStrategy`. Routes switch from `@UseGuards(CustomJwtGuard)` to `@UseGuards(JwtAuthGuard)` — same external behavior.",
  "rejectedOutput": "Keep the custom guard — it avoids the complexity of Passport and is simpler to understand.",
  "arbiters": ["NESTJS-DOCS", "PASSPORT-DOCS"],
  "whyAccepted": "Passport handles algorithm enforcement, expiry, clock skew, and JWKS transparently. The algorithm enforcement (algorithms: ['HS256']) directly mitigates the algorithm confusion vulnerability that a naive jwt.verify() call may miss. The unified strategy pattern scales to OAuth.",
  "whyRejected": "The 'simpler to understand' argument does not account for the security edge cases that Passport handles. A custom jwt.verify() that omits `algorithms` enforcement is a real security vulnerability (CVE-2015-9235). Simplicity that introduces vulnerabilities is not simplicity.",
  "sourceAnchors": ["https://docs.nestjs.com/security/authentication"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-481
**registryId:** IETF-RFCS  
**unitTitle:** RFC-8030 Web Push Protocol — push notification delivery vs polling for real-time updates  
**sourceUrl:** https://www.rfc-editor.org/rfc/rfc8030  
**license:** IETF-TLP | **trustTier:** 1  
**domainsTagged:** C | **familiesTagged:** 7

### Training Row

```json
{
  "rowId": "EXT-481-R1",
  "sourceIds": ["EXT-481"],
  "sourceTrustTier": 1,
  "datasetFamily": "finalMachineResults_to_userFormat",
  "domain": "C",
  "subdomain": "C3-format-negotiation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Web app shows real-time order status updates. Current implementation: browser polls `GET /orders/{id}/status` every 5 seconds. At 10,000 active users, this generates 2M requests/hour. Most polls return 'no change'.",
    "b": "Team is evaluating three alternatives: (A) WebSocket persistent connection, (B) Server-Sent Events (SSE), (C) Web Push Protocol (RFC-8030) with push notifications.",
    "c": "Order status changes happen on average every 20 minutes per order. Users don't need to have the tab open to receive notifications."
  },
  "function": "Select the appropriate real-time delivery mechanism and explain the tradeoffs.",
  "acceptedOutput": "**Recommendation: Server-Sent Events (SSE) for in-app real-time + Web Push for background notifications.**\n\n**Analysis:**\n\n**Polling (current):** 2M wasted requests/hour. Latency: up to 5s. Eliminates: zero. Verdict: replace.\n\n**WebSocket:** bidirectional, persistent TCP connection. Required when: client sends messages to server in real-time (chat, collaborative editing). Overhead: connection management, heartbeats, reconnection logic. For order status (server→client only): overkill.\n\n**Server-Sent Events (SSE):** unidirectional server→client over HTTP/2. No WebSocket overhead. Browser auto-reconnects. Native EventSource API.\n```typescript\n// Server (NestJS + SSE):\n@Sse('orders/:id/status')\ngetOrderStatus(@Param('id') id: string): Observable<MessageEvent> {\n  return this.orderEvents.pipe(\n    filter(event => event.orderId === id),\n    map(event => ({ data: JSON.stringify(event) } as MessageEvent))\n  );\n}\n\n// Client (browser):\nconst es = new EventSource(`/orders/${orderId}/status`);\nes.onmessage = (e) => updateUI(JSON.parse(e.data));\n// Reconnects automatically on disconnect\n```\n**Latency:** <100ms. **Server load:** N persistent connections (not 2M req/hr).\n\n**Web Push (RFC-8030):** push notifications delivered even when the browser tab is closed. Requires Service Worker registration + push subscription. Use for: 'Your order has shipped' when user is not actively browsing.\n```javascript\nconst reg = await navigator.serviceWorker.register('/sw.js');\nconst sub = await reg.pushManager.subscribe({\n  userVisibleOnly: true,\n  applicationServerKey: VAPID_PUBLIC_KEY\n});\n// Send sub to server — server calls RFC-8030 push endpoint\n```\n\n**Recommended split:**\n- SSE: real-time status in active tab (replaces polling)\n- Web Push: order shipped/delivered notifications when tab is closed",
  "rejectedOutput": "Use WebSockets for everything — they are the most capable real-time protocol.",
  "arbiters": ["RFC-8030", "MDN-SSE-DOCS"],
  "whyAccepted": "The SSE + Web Push split matches the two distinct use cases: active-tab real-time and background notification. SSE is simpler than WebSocket for unidirectional server→client updates and scales better (multiplexed over HTTP/2).",
  "whyRejected": "WebSocket for server→client only updates is overengineering. WebSockets require session management, heartbeating, and reconnection logic that SSE handles automatically. 'Most capable' does not mean 'most appropriate'.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc8030"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-482
**registryId:** GOOGLE-AIP  
**unitTitle:** AIP-162 resource revisions — revision-based versioning vs timestamp-based history  
**sourceUrl:** https://google.aip.dev/162  
**license:** Apache-2.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-482-R1",
  "sourceIds": ["EXT-482"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "API resource `DocumentTemplate` needs versioning: users want to view previous versions, compare two versions, and roll back to a prior version. Two approaches considered: (A) AIP-162 resource revisions with `{name}@{revisionId}`, (B) separate `DocumentTemplateHistory` collection with timestamp-based lookup.",
    "b": "Google AIP-162 defines a revision pattern: each mutation creates a new immutable revision. Resources are addressable as `documents/123` (latest) or `documents/123@c7cfa2a8` (specific revision).",
    "c": "API uses standard REST. ~50 revisions per document over its lifetime."
  },
  "function": "Compare AIP-162 resource revisions vs separate history collection and implement AIP-162.",
  "acceptedOutput": "**AIP-162 resource revisions** are the correct pattern for this use case — they make revisions first-class addressable resources without a separate API surface.\n\n**AIP-162 revision API surface:**\n- `GET /documents/123` → latest revision\n- `GET /documents/123@c7cfa2a8` → specific revision by ID\n- `GET /documents/123:listRevisions` → list all revisions\n- `POST /documents/123:rollback` with `{revisionId: \"c7cfa2a8\"}` → roll back\n\n**Implementation (ASP.NET Core):**\n```csharp\n// DocumentTemplate entity with revision support\npublic class DocumentTemplate\n{\n    public string Name { get; set; } = \"\";     // e.g., \"documents/123\"\n    public string RevisionId { get; set; } = \"\";  // 8-char hash of content\n    public DateTimeOffset RevisionCreateTime { get; set; }\n    public string Content { get; set; } = \"\";\n    public bool IsLatest { get; set; }           // only one revision per name is latest\n}\n\n// Controller routes\n[HttpGet(\"documents/{name}\")]\npublic async Task<IActionResult> Get(string name, [FromQuery] string? revisionId)\n{\n    // name@revisionId parsing\n    var (resourceName, revision) = ParseRevisionRef(name); // handles \"123@abc\" syntax\n    var template = revision != null\n        ? await _db.GetRevisionAsync(resourceName, revision)\n        : await _db.GetLatestAsync(resourceName);\n    return template is null ? NotFound() : Ok(template);\n}\n\n[HttpPost(\"documents/{name}:rollback\")]\npublic async Task<IActionResult> Rollback(string name, [FromBody] RollbackRequest req)\n{\n    // Create a new revision that copies the content of the target revision\n    var targetRevision = await _db.GetRevisionAsync(name, req.RevisionId);\n    var newRevision = await _db.CreateRevisionAsync(name, targetRevision!.Content);\n    await _db.SetLatestAsync(name, newRevision.RevisionId);\n    return Ok(newRevision);\n}\n```\n\n**vs. separate history collection:**\n- History collection: requires separate `GET /document-history?templateId=123&before=2026-05-01` endpoint — non-standard, timestamp ambiguity when multiple revisions occur in same second.\n- AIP-162: standard pattern, revision IDs are stable and content-addressable, rollback is explicit.",
  "rejectedOutput": "Add a `version` integer field to the resource and increment it on each update.",
  "arbiters": ["GOOGLE-AIP", "API-DESIGN-PATTERNS"],
  "whyAccepted": "AIP-162 resource revisions provide a complete, standard API pattern for version management. Revision IDs are content-addressable (hash of content), so identical content produces identical IDs — enabling deduplication.",
  "whyRejected": "Integer version fields are monotonically incrementing counters — they cannot be used to address the resource by revision in URLs (`documents/123@5` is ambiguous vs a hash). They also don't support content-addressable deduplication.",
  "sourceAnchors": ["https://google.aip.dev/162"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-483
**registryId:** OPENASSISTANT-OASST1  
**unitTitle:** OpenAssistant data quality — conversation tree depth vs breadth for RLHF diversity  
**sourceUrl:** https://huggingface.co/datasets/OpenAssistant/oasst1  
**license:** Apache-2.0 | **trustTier:** 2  
**domainsTagged:** G | **familiesTagged:** 8

### Training Row

```json
{
  "rowId": "EXT-483-R1",
  "sourceIds": ["EXT-483"],
  "sourceTrustTier": 2,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G1-data-quality-decision",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "RLHF dataset curation from OpenAssistant oasst1: dataset contains conversation trees where each node has multiple branching responses (breadth) and conversations extend to many turns (depth). Budget allows labeling 10,000 additional comparison pairs.",
    "b": "Option A: sample from deep conversation chains (8+ turns) to improve multi-turn coherence. Option B: sample from diverse root prompts (new conversations) to improve topic breadth.",
    "c": "Current model weakness: loses coherence after turn 5. Topic coverage is already 85% of target domains."
  },
  "function": "Recommend how to allocate 10,000 comparison pairs between depth (multi-turn chains) and breadth (new prompts) given the model weakness.",
  "acceptedOutput": "**Recommendation: 70% depth (7,000 pairs from turn 5+ chains) + 30% breadth (3,000 pairs from diverse roots).**\n\n**Rationale for depth-first allocation:**\n\nThe stated model weakness is coherence after turn 5. RLHF reward signal directly influences behavior for the data distribution it is trained on. If 95% of training pairs are from turns 1-3 (breadth-focused), the reward model has limited signal for correct behavior at turns 6-12.\n\n**Depth sampling strategy:**\n```python\nimport datasets\n\noasst = datasets.load_dataset('OpenAssistant/oasst1', split='train')\n\ndef get_deep_chains(dataset, min_depth: int = 5, target_pairs: int = 7000):\n    \"\"\"Sample comparison pairs from conversation chains >= min_depth turns.\"\"\"\n    deep_pairs = []\n    for msg in dataset:\n        # oasst1 stores depth as message_tree_id + parent_id chain length\n        if msg['depth'] >= min_depth and msg['role'] == 'assistant':\n            # Find sibling responses at same depth (comparison pairs)\n            siblings = get_siblings(dataset, msg['parent_id'])\n            if len(siblings) >= 2:\n                # Pair: (higher-ranked sibling, lower-ranked sibling)\n                deep_pairs.append({\n                    'chosen': max(siblings, key=lambda s: s['rank']),\n                    'rejected': min(siblings, key=lambda s: s['rank']),\n                    'depth': msg['depth'],\n                })\n    return deep_pairs[:target_pairs]\n\ndeep_sample = get_deep_chains(oasst, min_depth=5, target_pairs=7000)\nbreadth_sample = get_diverse_roots(oasst, target_pairs=3000)\n```\n\n**Expected outcome:**\n- Reward model has dense signal for turn 5-12 behavior\n- Coherence loss should improve measurably (target: coherence score at turn 8 > 0.75 vs current < 0.55)\n- Topic coverage maintained (30% breadth prevents regression)\n\n**Monitoring:** evaluate on held-out multi-turn conversations from the oasst1 test split. Coherence metric: BERTScore between context summary and continuation.",
  "rejectedOutput": "Focus on breadth — more diverse prompts always improve model generalization.",
  "arbiters": ["OPENASSISTANT-OASST1", "RLHF-PAPER"],
  "whyAccepted": "The allocation is driven by the stated weakness (turn 5+ coherence). Reward model training distributes signal proportionally to sample distribution — if multi-turn pairs are underrepresented, the reward model cannot guide the policy to improve there. The 70/30 split targets the weakness while protecting topic coverage.",
  "whyRejected": "'More diverse prompts always improve generalization' is a general principle that conflicts with the specific stated weakness. The model's topic coverage is already 85% — marginal breadth gains are smaller than the coherence fix opportunity. Generic principles should yield to specific evidence.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-484
**registryId:** TATOEBA  
**unitTitle:** Tatoeba sentence length normalization — outlier detection and filtering strategy  
**sourceUrl:** https://tatoeba.org/  
**license:** CC-BY-2.0-FR | **trustTier:** 2  
**domainsTagged:** C | **familiesTagged:** 2

### Training Row

```json
{
  "rowId": "EXT-484-R1",
  "sourceIds": ["EXT-484"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-translation-filtering",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "EN-FR Tatoeba pipeline: after LASER alignment filtering (threshold 0.88), some pairs have extreme length ratios — EN sentence of 3 words paired with FR sentence of 42 words. These pass LASER filter because both sentences are semantically related to the same concept.",
    "b": "Current length ratio filter: reject if `max(len_en, len_fr) / min(len_en, len_fr) > 3`. This rejects the 3:42 pair (ratio = 14) but also incorrectly rejects valid pairs where one language is genuinely more verbose.",
    "c": "French is on average 15% longer than English. German compounds can make one EN word map to a long DE phrase."
  },
  "function": "Design a language-pair-aware length ratio filter that handles cross-linguistic verbosity differences.",
  "acceptedOutput": "**Language-pair-aware length ratio filter** adjusts thresholds based on known average length ratio for each language pair, then identifies statistical outliers rather than using a fixed ratio.\n\n**Implementation:**\n```python\nimport numpy as np\nfrom dataclasses import dataclass\nfrom typing import Callable\n\n# Empirical average token length ratios (target/source) from large corpora\nLENGTH_RATIO_BASELINES = {\n    ('en', 'fr'): 1.15,  # French ~15% longer than English\n    ('en', 'de'): 1.08,  # German slightly longer on average\n    ('en', 'ja'): 0.75,  # Japanese characters represent more content per token\n    ('en', 'zh'): 0.60,  # Chinese: very dense per character\n}\n\n@dataclass\nclass LengthFilter:\n    src_lang: str\n    tgt_lang: str\n    # z-score threshold: flag pairs > N std devs from baseline ratio\n    z_threshold: float = 3.0\n\n    def __post_init__(self):\n        key = (self.src_lang, self.tgt_lang)\n        self.baseline = LENGTH_RATIO_BASELINES.get(key, 1.0)\n\n    def compute_ratio(self, src: str, tgt: str) -> float:\n        src_len = len(src.split())\n        tgt_len = len(tgt.split())\n        if src_len == 0: return float('inf')\n        return tgt_len / src_len\n\n    def calibrate(self, pairs: list[tuple[str, str]]):\n        \"\"\"Compute mean and std of ratios for adaptive thresholding.\"\"\"\n        ratios = [self.compute_ratio(s, t) for s, t in pairs]\n        # Normalize by baseline: ratio/baseline should cluster around 1.0\n        normalized = [r / self.baseline for r in ratios]\n        self.mean = np.mean(normalized)\n        self.std  = np.std(normalized)\n\n    def is_valid(self, src: str, tgt: str) -> bool:\n        ratio = self.compute_ratio(src, tgt)\n        normalized = ratio / self.baseline\n        z_score = abs(normalized - self.mean) / (self.std + 1e-8)\n        return z_score <= self.z_threshold  # reject > 3 std devs from baseline\n\n# Usage\nflt = LengthFilter('en', 'fr')\nflt.calibrate(sample_pairs[:10_000])  # calibrate on a sample\nfiltered = [(s, t) for s, t in all_pairs if flt.is_valid(s, t)]\n```\n\n**Result:** 3-word EN : 42-word FR pair → ratio = 14.0, normalized = 14.0/1.15 = 12.2, z-score = ~9 → correctly rejected. Valid EN short sentence : FR relative clause → ratio = 2.8, normalized = 2.4, z-score = 1.8 → correctly kept.",
  "rejectedOutput": "Use a fixed ratio threshold of 5:1 — this is the standard in NMT (Neural Machine Translation) preprocessing.",
  "arbiters": ["TATOEBA", "NMT-FILTERING-PAPER"],
  "whyAccepted": "Language-pair-aware calibration correctly handles cross-linguistic verbosity differences. Statistical z-score thresholding adapts to the actual distribution of the corpus rather than assuming a universal ratio applies to all language pairs.",
  "whyRejected": "A fixed 5:1 ratio ignores the ~15% EN-FR verbosity difference, creating asymmetric filtering (more FR→EN pairs rejected than EN→FR for the same content). Calibration to the actual corpus distribution is strictly more accurate.",
  "sourceAnchors": ["https://tatoeba.org/eng/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-485
**registryId:** VUE-DOCS  
**unitTitle:** Vue 3 composables vs Vuex 4 modules for feature-scoped state encapsulation  
**sourceUrl:** https://vuejs.org/guide/reusability/composables  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-485-R1",
  "sourceIds": ["EXT-485"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Vue 3 app uses Vuex 4 for all state management. New feature: a multi-step form wizard with 4 steps, form data, validation state, and step navigation logic. Team debates adding a new Vuex module vs implementing it as a composable.",
    "b": "The form wizard state is only used inside the FormWizard component tree — it is not shared across the app.",
    "c": "App already uses Pinia for other global state (user, cart). Vuex 4 is legacy from Vue 2."
  },
  "function": "Recommend composable vs Vuex module for feature-scoped form wizard state and recommend Pinia migration path.",
  "acceptedOutput": "**For feature-scoped state (only used in FormWizard component tree): use a composable** — Vuex modules are global and designed for shared cross-component state. Adding a Vuex module for state that belongs to one component tree is overengineering.\n\n**Form wizard composable:**\n```typescript\n// composables/useFormWizard.ts\nimport { ref, computed } from 'vue'\n\nexport type WizardStep = 'personal' | 'address' | 'payment' | 'review'\nconst STEPS: WizardStep[] = ['personal', 'address', 'payment', 'review']\n\nexport function useFormWizard() {\n  const currentStepIndex = ref(0)\n  const formData = ref<Record<string, unknown>>({})\n  const validationErrors = ref<Record<string, string>>({})\n\n  const currentStep  = computed(() => STEPS[currentStepIndex.value])\n  const isFirstStep  = computed(() => currentStepIndex.value === 0)\n  const isLastStep   = computed(() => currentStepIndex.value === STEPS.length - 1)\n  const progress     = computed(() =>\n    Math.round((currentStepIndex.value / (STEPS.length - 1)) * 100)\n  )\n\n  function next() {\n    if (!isLastStep.value) currentStepIndex.value++\n  }\n  function prev() {\n    if (!isFirstStep.value) currentStepIndex.value--\n  }\n  function updateData(step: WizardStep, data: Record<string, unknown>) {\n    formData.value = { ...formData.value, ...data }\n  }\n  function setErrors(errors: Record<string, string>) {\n    validationErrors.value = errors\n  }\n\n  return { currentStep, isFirstStep, isLastStep, progress,\n           formData, validationErrors, next, prev, updateData, setErrors }\n}\n\n// FormWizard.vue — instantiate once, share via provide/inject to children\nconst wizard = useFormWizard()\nprovide('wizard', wizard)\n```\n\n**Decision rule:**\n- State used in ONE component tree → composable\n- State shared across MULTIPLE unrelated component trees → Pinia store\n- Never use Vuex for new features — migrate to Pinia\n\n**Pinia migration path for existing Vuex:** replace Vuex modules one at a time with Pinia stores. `useStore()` → `useCartStore()`. Vuex and Pinia coexist until migration is complete.",
  "rejectedOutput": "Add a Vuex module — it gives you Vuex DevTools time-travel debugging for the wizard state.",
  "arbiters": ["VUE-DOCS", "PINIA-DOCS"],
  "whyAccepted": "Composables are the Vue 3 idiomatic pattern for feature-scoped state. They are tree-shakeable, TypeScript-native, and don't require global store registration. The DevTools argument is a real trade-off but Pinia also has DevTools support for when global state is genuinely needed.",
  "whyRejected": "DevTools time-travel debugging for a 4-step form wizard does not justify adding a Vuex module for inherently local state. The composable + provide/inject pattern provides the same state sharing within the component tree without global store pollution. Also: Vuex is in maintenance mode — new code should not use it.",
  "sourceAnchors": ["https://vuejs.org/guide/reusability/composables"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-486
**registryId:** EFCORE-SOURCE  
**unitTitle:** EF Core interceptors for automatic audit fields vs manual assignment  
**sourceUrl:** https://github.com/dotnet/efcore  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** F | **familiesTagged:** 6

### Training Row

```json
{
  "rowId": "EXT-486-R1",
  "sourceIds": ["EXT-486"],
  "sourceTrustTier": 1,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "F",
  "subdomain": "F1-storage-retrieval",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Every entity has `CreatedAt`, `UpdatedAt`, `CreatedBy`, `UpdatedBy` fields. Currently set manually in each service method: `entity.UpdatedAt = DateTime.UtcNow; entity.UpdatedBy = currentUser.Id;`. Audit fields are occasionally missed — 3 incidents last quarter.",
    "b": "Developer wants to centralize audit field assignment. Options: (A) EF Core interceptor (ISaveChangesInterceptor), (B) override DbContext.SaveChanges().",
    "c": "Current user is available via IHttpContextAccessor. Multi-tenant app."
  },
  "function": "Implement centralized audit field assignment using EF Core interceptors to eliminate manual assignment errors.",
  "acceptedOutput": "**EF Core interceptors** are preferred over overriding `SaveChanges()` because they support async and are composable — multiple interceptors can be chained without base class conflicts.\n\n**ISaveChangesInterceptor implementation:**\n```csharp\n// AuditInterceptor.cs\npublic sealed class AuditInterceptor : SaveChangesInterceptor\n{\n    private readonly ICurrentUserService _currentUser;\n    private readonly TimeProvider _time;\n\n    public AuditInterceptor(ICurrentUserService currentUser, TimeProvider time)\n    {\n        _currentUser = currentUser;\n        _time = time;\n    }\n\n    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(\n        DbContextEventData eventData,\n        InterceptionResult<int> result,\n        CancellationToken ct = default)\n    {\n        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, ct);\n\n        var now    = _time.GetUtcNow();\n        var userId = _currentUser.UserId; // from IHttpContextAccessor claim\n\n        foreach (var entry in eventData.Context.ChangeTracker.Entries<IAuditableEntity>())\n        {\n            switch (entry.State)\n            {\n                case EntityState.Added:\n                    entry.Entity.CreatedAt  = now;\n                    entry.Entity.CreatedBy  = userId;\n                    entry.Entity.UpdatedAt  = now;\n                    entry.Entity.UpdatedBy  = userId;\n                    break;\n                case EntityState.Modified:\n                    // Prevent overwriting original creation metadata\n                    entry.Property(e => e.CreatedAt).IsModified = false;\n                    entry.Property(e => e.CreatedBy).IsModified = false;\n                    entry.Entity.UpdatedAt = now;\n                    entry.Entity.UpdatedBy = userId;\n                    break;\n            }\n        }\n        return base.SavingChangesAsync(eventData, result, ct);\n    }\n}\n\n// IAuditableEntity.cs — marker interface\npublic interface IAuditableEntity\n{\n    DateTimeOffset CreatedAt { get; set; }\n    string CreatedBy { get; set; }\n    DateTimeOffset UpdatedAt { get; set; }\n    string UpdatedBy { get; set; }\n}\n\n// AppDbContext.cs or DI registration\nprotected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)\n    => optionsBuilder.AddInterceptors(_auditInterceptor); // injected via DI\n```\n\n**Result:** zero manual audit field assignments. Missed assignment incidents go to zero — the interceptor runs on every SaveChanges call, regardless of which service triggered the save.",
  "rejectedOutput": "Override SaveChangesAsync() in the DbContext base class.",
  "arbiters": ["EFCORE-DOCS"],
  "whyAccepted": "ISaveChangesInterceptor runs before the actual DB write, supports async, and is DI-injectable (can access ICurrentUserService). The IsModified = false guards prevent accidental CreatedAt/CreatedBy overwrite on updates.",
  "whyRejected": "Overriding SaveChangesAsync works but requires every DbContext subclass to call base.SaveChangesAsync() — a missed base call breaks audit. Interceptors are composed by EF Core automatically, not by inheritance chains.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-487
**registryId:** DOTNET-MICROSERVICES  
**unitTitle:** Outbox pattern with EF Core + MassTransit for reliable event publishing  
**sourceUrl:** https://github.com/dotnet-architecture/eShopOnContainers  
**license:** MIT | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-487-R1",
  "sourceIds": ["EXT-487"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Order service saves an order to DB then publishes `OrderPlaced` event to RabbitMQ. Current code: `await _db.SaveChangesAsync(); await _bus.Publish(new OrderPlaced(order.Id))`. Incident: DB save succeeds but RabbitMQ publish fails — order created but downstream services never notified.",
    "b": "Team wants to implement the outbox pattern. Options: (A) manually implement an Outbox table + background worker, (B) use MassTransit's built-in transactional outbox with EF Core.",
    "c": "Stack: .NET 8, EF Core, MassTransit 8.x, RabbitMQ."
  },
  "function": "Implement the transactional outbox pattern using MassTransit's built-in EF Core outbox to eliminate the publish failure incident.",
  "acceptedOutput": "**MassTransit 8 built-in transactional outbox** with EF Core eliminates the dual-write problem: events are written to an `OutboxMessage` table in the same DB transaction as the domain entity. A background worker then reliably delivers them to RabbitMQ.\n\n**Setup:**\n```csharp\n// Program.cs\nbuilder.Services.AddMassTransit(x =>\n{\n    x.AddEntityFrameworkOutbox<AppDbContext>(o =>\n    {\n        o.UsePostgres();         // or UseSqlServer()\n        o.UseBusOutbox();        // enable transactional outbox\n        o.QueryDelay = TimeSpan.FromSeconds(1); // polling interval\n    });\n    x.UsingRabbitMq((ctx, cfg) =>\n    {\n        cfg.Host(\"rabbitmq://localhost\");\n        cfg.ConfigureEndpoints(ctx);\n    });\n});\n\n// AppDbContext — add outbox tables to migrations\nprotected override void OnModelCreating(ModelBuilder modelBuilder)\n{\n    modelBuilder.AddInboxStateEntity();   // idempotency table\n    modelBuilder.AddOutboxStateEntity();  // outbox lock table\n    modelBuilder.AddOutboxMessageEntity(); // outbox messages\n    base.OnModelCreating(modelBuilder);\n}\n```\n\n**Order service — atomic DB save + event enqueue:**\n```csharp\npublic async Task PlaceOrderAsync(CreateOrderDto dto)\n{\n    var order = new Order(dto);\n    _db.Orders.Add(order);\n\n    // PublishAsync writes to OutboxMessage table — same EF Core transaction\n    await _publishEndpoint.Publish(new OrderPlaced(order.Id));\n\n    // Single atomic commit: Order row + OutboxMessage row\n    await _db.SaveChangesAsync();\n    // If RabbitMQ is down, event is safely stored in DB — background worker retries\n}\n```\n\n**Delivery guarantee:** MassTransit background worker polls `OutboxMessage` table and delivers to RabbitMQ with at-least-once semantics. The `InboxState` table on consumers provides idempotent processing.\n\n**Before:** 2 operations, no atomicity — failure window between save and publish.\n**After:** 1 atomic transaction — no event is lost regardless of RabbitMQ availability.",
  "rejectedOutput": "Wrap the save and publish in a try/catch and retry the publish on failure.",
  "arbiters": ["MASSTRANSIT-DOCS", "DOTNET-MICROSERVICES-GUIDE"],
  "whyAccepted": "MassTransit transactional outbox is the correct, production-grade implementation of the outbox pattern. The atomicity guarantee (same DB transaction) eliminates the failure window. At-least-once delivery + idempotent consumers is the correct final guarantee.",
  "whyRejected": "Try/catch retry does not fix the fundamental problem — if the process crashes after DB save but before the retry loop completes, the event is lost. The retry loop only helps for transient RabbitMQ errors in the same process lifetime. Outbox is durable across crashes.",
  "sourceAnchors": ["https://masstransit.io/documentation/configuration/middleware/outbox"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-488
**registryId:** MICROSOFT-LEARN-ARCH  
**unitTitle:** Azure Service Bus vs Azure Event Grid for integration event routing  
**sourceUrl:** https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview  
**license:** CC-BY-4.0 | **trustTier:** 1  
**domainsTagged:** A | **familiesTagged:** 1

### Training Row

```json
{
  "rowId": "EXT-488-R1",
  "sourceIds": ["EXT-488"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A1-topology-design",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Azure microservice platform needs integration events for two scenarios: (1) `OrderPlaced` event must be processed by Inventory, Shipping, and Notifications services — each must process EVERY event. (2) `BlobUploaded` events from Azure Storage must trigger downstream processing — events are fan-out, no retry needed.",
    "b": "Evaluating Azure Service Bus Topics vs Azure Event Grid.",
    "c": "Order volume: 50,000/day. Blob events: 500,000/day. Budget: Azure Standard tier."
  },
  "function": "Select Azure Service Bus vs Event Grid for each scenario and explain the architectural distinction.",
  "acceptedOutput": "**Scenario 1 (OrderPlaced — guaranteed delivery to 3 services): Azure Service Bus Topics with subscriptions.**\n\n**Scenario 2 (BlobUploaded — fan-out notification, no guaranteed processing): Azure Event Grid.**\n\n**Architectural distinction:**\n\n| Property | Service Bus Topics | Event Grid |\n|---|---|---|\n| Delivery guarantee | At-least-once + DLQ | At-least-once (24h retry) |\n| Consumer model | Pull (consumer polls) | Push (HTTP webhook) |\n| Message ordering | Sessions-based FIFO | Not guaranteed |\n| Max message size | 256 KB | 1 MB (Event Grid data) |\n| Retry on failure | Built-in + DLQ | Yes (24h) |\n| Pricing | Per operation | Per million events |\n| Primary use case | Business transactions | Reactive fan-out |\n\n**Scenario 1 — Service Bus Topic (OrderPlaced):**\n```csharp\n// Publisher\nawait sender.SendMessageAsync(new ServiceBusMessage(\n    JsonSerializer.SerializeToUtf8Bytes(new OrderPlaced(order.Id)))\n{\n    Subject = \"OrderPlaced\"\n});\n\n// 3 subscriptions: inventory-sub, shipping-sub, notification-sub\n// Each subscription receives an independent copy of every OrderPlaced message\n// If Inventory service is down, its messages queue up in DLQ — not lost\n```\n\n**Scenario 2 — Event Grid (BlobUploaded):**\n```csharp\n// Azure Storage built-in Event Grid integration — no publisher code needed\n// Event Grid subscription → Azure Function webhook:\n[Function(\"OnBlobUploaded\")]\npublic async Task Run(\n    [EventGridTrigger] CloudEvent cloudEvent)\n{\n    var blobUrl = cloudEvent.GetData<StorageBlobCreatedEventData>()!.Url;\n    await _processor.ProcessBlobAsync(blobUrl);\n    // If this fails, Event Grid retries for 24 hours\n}\n```\n\n**Why not Service Bus for Scenario 2:** 500K blob events/day × 3 subscribers = 1.5M Service Bus operations/day (~$45/month) vs Event Grid 500K events/day ($0.06/month). Event Grid is 750x cheaper for reactive fan-out.",
  "rejectedOutput": "Use Event Grid for both scenarios — it handles fan-out natively.",
  "arbiters": ["MICROSOFT-LEARN-ARCH", "AZURE-MESSAGING-DOCS"],
  "whyAccepted": "The Service Bus / Event Grid distinction maps directly to the two scenarios: guaranteed ordered processing (Service Bus) vs reactive fan-out notification (Event Grid). The pricing analysis is concrete.",
  "whyRejected": "Event Grid does not provide the ordered, guaranteed delivery with DLQ that business transaction events (OrderPlaced) require. If Inventory service is down during an Event Grid push, the event is retried for 24 hours — but if the retry window expires, the order goes unprocessed by Inventory. Service Bus DLQ + dead letter processor is the correct durability mechanism.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/service-bus-messaging/"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-489
**registryId:** CWE-TOP25  
**unitTitle:** CWE-306 Missing Authentication for Critical Function — ASP.NET Core AllowAnonymous audit  
**sourceUrl:** https://cwe.mitre.org/data/definitions/306.html  
**license:** CWE-TOU | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-489-R1",
  "sourceIds": ["EXT-489"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "negative-example",
  "input": {
    "a": "Security audit finds `[AllowAnonymous]` on an admin controller method: `[HttpPost('admin/users/delete')] [AllowAnonymous] public async Task<IActionResult> DeleteUser(int userId)`. The method was annotated during testing and never cleaned up.",
    "b": "CWE-306 (Missing Authentication for Critical Function) — the DeleteUser endpoint is callable by anyone without authentication.",
    "c": "Global authorization policy is configured with `RequireAuthenticatedUser()`. The AllowAnonymous attribute overrides the global policy."
  },
  "function": "Identify the CWE-306 vulnerability, explain how AllowAnonymous overrides global auth policy, and implement detection.",
  "acceptedOutput": "**CWE-306 vulnerability:** `[AllowAnonymous]` explicitly bypasses ASP.NET Core's global authorization policy. Any HTTP client can call `POST /admin/users/delete?userId=1` without any credentials and delete any user.\n\n**How AllowAnonymous overrides global policy:**\nASP.NET Core's authorization middleware checks `[AllowAnonymous]` first — if present, all `[Authorize]` attributes and global `RequireAuthenticatedUser()` policies are bypassed entirely. This is by design for public endpoints (health checks, OAuth callbacks) but catastrophic when applied to administrative operations.\n\n**Immediate fix:**\n```csharp\n[HttpPost(\"admin/users/delete\")]\n[Authorize(Roles = \"Admin\")]  // Replace AllowAnonymous with explicit role requirement\npublic async Task<IActionResult> DeleteUser(int userId)\n{\n    // Also add resource-level authorization — verify caller can delete THIS user\n    // ...\n}\n```\n\n**Prevention — Roslyn analyzer to detect AllowAnonymous on admin paths:**\n```csharp\n// Custom Roslyn diagnostic (simplified)\n[DiagnosticAnalyzer(LanguageNames.CSharp)]\npublic class AllowAnonymousAdminAnalyzer : DiagnosticAnalyzer\n{\n    public static readonly DiagnosticDescriptor Rule = new(\n        id: \"SEC001\",\n        title: \"AllowAnonymous on admin route\",\n        messageFormat: \"Method '{0}' has [AllowAnonymous] on an admin route — verify intentional\",\n        category: \"Security\",\n        defaultSeverity: DiagnosticSeverity.Error,\n        isEnabledByDefault: true);\n\n    // Analyzer checks: method has [AllowAnonymous] AND route starts with 'admin/'\n    // Reports as build error — prevents the mistake from reaching production\n}\n```\n\n**Integration test to catch regressions:**\n```csharp\n[Fact]\npublic async Task DeleteUser_WithoutAuthentication_Returns401()\n{\n    var response = await _client.PostAsync(\"/admin/users/delete?userId=1\", null);\n    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); // must not be 200 or 404\n}\n```",
  "rejectedOutput": "The global auth policy protects all endpoints — AllowAnonymous on one method is fine for testing.",
  "arbiters": ["CWE-TOP25", "ASPNETCORE-SECURITY-DOCS"],
  "whyAccepted": "AllowAnonymous explicitly overrides global auth policy — the global policy does NOT protect endpoints with AllowAnonymous. The integration test provides a concrete, automated Gate B. The Roslyn analyzer prevents the mistake at compile time.",
  "whyRejected": "The claim that global auth policy protects AllowAnonymous-annotated endpoints is factually incorrect. This misunderstanding is the root cause of the vulnerability. Accepting this reasoning would leave the CWE-306 vulnerability unfixed.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/306.html"],
  "licenseConstraint": "CWE-TOU",
  "trainingStatus": "draft"
}
```

---

## EXT-490
**registryId:** OWASP-ASVS  
**unitTitle:** ASVS V2.1 password complexity — NIST SP 800-63B length vs complexity tradeoff  
**sourceUrl:** https://owasp.org/www-project-application-security-verification-standard/  
**license:** CC-BY-SA-3.0 | **trustTier:** 1  
**domainsTagged:** E | **familiesTagged:** 5

### Training Row

```json
{
  "rowId": "EXT-490-R1",
  "sourceIds": ["EXT-490"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E4-security-gate",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "OWASP ASVS V2.1.1 requires minimum 12-character passwords. V2.1.9 requires NOT mandating character composition rules (uppercase + number + symbol). Current system requires 8 characters minimum + uppercase + number + symbol.",
    "b": "Security team debates the change: removing composition rules will 'weaken' security. NIST SP 800-63B Section 5.1.1 specifically recommends against composition rules.",
    "c": "User research shows 40% of support tickets are password reset requests. Most users store passwords in plaintext notes due to complexity requirements."
  },
  "function": "Implement ASVS V2.1 + NIST SP 800-63B compliant password policy and explain the security rationale.",
  "acceptedOutput": "**NIST SP 800-63B rationale for removing composition rules:** users respond to mandatory complexity by making minimal changes — 'Password1!' satisfies uppercase + number + symbol but is a dictionary password. Length provides more entropy than forced composition.\n\n**Entropy comparison:**\n- 8-char with complexity (uppercase + number + symbol): ~10^14 combinations\n- 12-char lowercase only (random): ~26^12 ≈ 9.5×10^16 combinations\n- 12 random chars beats forced-complex 8 chars by 1,000x\n\n**ASVS V2.1 compliant ASP.NET Core Identity configuration:**\n```csharp\nbuilder.Services.AddIdentity<User, IdentityRole>(opts =>\n{\n    // V2.1.1: minimum 12 characters\n    opts.Password.RequiredLength = 12;\n\n    // V2.1.9: remove composition rules per NIST 800-63B\n    opts.Password.RequireUppercase = false;\n    opts.Password.RequireDigit = false;\n    opts.Password.RequireNonAlphanumeric = false;\n    opts.Password.RequiredUniqueChars = 4; // minimal — prevents '111111111111'\n\n    // V2.1.7: check against breached password list\n    // Implement via HaveIBeenPwned API or local blocklist\n})\n.AddPasswordValidator<BreachedPasswordValidator<User>>(); // custom validator\n\n// BreachedPasswordValidator — check against HaveIBeenPwned k-anonymity API\npublic class BreachedPasswordValidator<TUser> : IPasswordValidator<TUser>\n    where TUser : class\n{\n    public async Task<IdentityResult> ValidateAsync(\n        UserManager<TUser> manager, TUser user, string password)\n    {\n        var sha1 = SHA1.HashData(Encoding.UTF8.GetBytes(password));\n        var prefix = Convert.ToHexString(sha1)[..5];\n        // Query HIBP k-anonymity endpoint — only prefix sent, not full hash\n        var isBreached = await _hibpClient.IsBreachedAsync(prefix, sha1);\n        return isBreached\n            ? IdentityResult.Failed(new IdentityError\n              { Code = \"BreachedPassword\",\n                Description = \"This password has appeared in a data breach. Choose a different password.\" })\n            : IdentityResult.Success;\n    }\n}\n```\n\n**Result:** longer passphrases ('correct horse battery staple' = 24 chars, high entropy, memorable) are now valid. Breached passwords are blocked. Support tickets expected to drop by ~30%.",
  "rejectedOutput": "Keep complexity rules — removing them will allow users to choose weak passwords like '123456789012'.",
  "arbiters": ["OWASP-ASVS", "NIST-SP-800-63B"],
  "whyAccepted": "NIST SP 800-63B is the authoritative source on password security — its recommendation against composition rules is based on empirical research showing users game composition rules. The breached password check (HIBP) directly addresses the weak-password concern better than complexity rules.",
  "whyRejected": "'123456789012' (12 digits) is rejected by the RequiredUniqueChars=4 check and would appear in the HaveIBeenPwned database. The concern is valid but addressed by the breach check, not composition rules.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/", "https://pages.nist.gov/800-63-3/sp800-63b.html"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
