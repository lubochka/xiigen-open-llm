# External Sources Batch 33 — EXT-651 through EXT-670

---

## EXT-651 | KUBERNETES-KEPS | Family 3 | Domain D

**Unit title:** Kubernetes init containers vs postStart lifecycle hook for dependency readiness execution
**Source URL:** https://kubernetes.io/docs/concepts/workloads/pods/init-containers/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT651-F3-D",
  "sourceIds": ["EXT-651"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "pod-startup-dependency-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A pod needs to wait for a database to be ready before the main application container starts. The team uses a postStart lifecycle hook to run a connectivity check, but the main container starts before postStart completes, causing database connection failures at startup.",
    "b": "postStart lifecycle hook runs concurrently with the container's ENTRYPOINT — it does not block container startup. init containers run to completion before any app containers start. An init container that exits 0 confirms the dependency is ready; the main container then starts.",
    "c": "init containers support full pod spec (resources, environment variables) and can use different images (e.g., busybox for a netcat connectivity check, or the database CLI for a schema check)."
  },
  "function": "Apply init container for blocking dependency readiness before main container execution",
  "acceptedOutput": "Use an init container for the database readiness check: initContainers: [{name: wait-for-db, image: busybox, command: ['sh', '-c', 'until nc -z db-service 5432; do sleep 2; done']}]. The main container starts only after the init container exits 0. postStart is appropriate for non-blocking post-startup tasks (e.g., registering with a service registry).",
  "rejectedOutput": "Use postStart lifecycle hook with a readiness script. Add a readinessProbe to the main container so traffic is only sent when the app is ready.",
  "arbiters": ["execution-applier-arbiter", "pod-startup-sequencing-arbiter"],
  "whyAccepted": "init containers complete before main containers start — the startup dependency is structurally enforced. The main container's ENTRYPOINT never runs until the database is reachable.",
  "whyRejected": "postStart does not block container startup. The readinessProbe prevents traffic but does not prevent the application's own startup code from trying to connect to the database. If the application's startup code (e.g., EF Core migration check) runs before the database is ready, it fails regardless of the readiness probe.",
  "sourceAnchors": ["https://kubernetes.io/docs/concepts/workloads/pods/init-containers/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-652 | BIGCODEBENCH | Family 7 | Domain C

**Unit title:** Python Pydantic v2 model serialization — model_dump() vs dict() for API response translation
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT652-F7-C",
  "sourceIds": ["EXT-652"],
  "sourceTrustTier": 2,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "pydantic-serialization-translation",
  "rowClass": "migration-decision",
  "input": {
    "a": "A FastAPI application uses Pydantic v1's .dict() and .json() methods for model serialization. After migrating to Pydantic v2, deprecation warnings appear: model.dict() is deprecated in favor of model.model_dump().",
    "b": "Pydantic v2 renamed methods: .dict() → .model_dump(), .json() → .model_dump_json(), .schema() → .model_json_schema(), .parse_obj() → .model_validate(). The old methods still work in v2 compatibility mode but are scheduled for removal. model_dump() adds new options: mode='json' (serializes dates to ISO strings), exclude_unset=True, include/exclude field patterns.",
    "c": "FastAPI uses Pydantic internally — FastAPI endpoints automatically use model_dump() for response serialization in Pydantic v2 when the response model is a Pydantic BaseModel."
  },
  "function": "Translate Pydantic v1 serialization methods to Pydantic v2 API for response generation",
  "acceptedOutput": "Replace model.dict() with model.model_dump() and model.json() with model.model_dump_json(). For API response translation: model.model_dump(mode='json') ensures datetime fields are serialized as ISO 8601 strings, Decimal as strings, and Enum as values. Use model.model_dump(exclude_unset=True) for PATCH endpoint responses that should omit unchanged fields.",
  "rejectedOutput": "Keep using .dict() until it is removed. Pydantic v2 maintains backward compatibility for v1 methods.",
  "arbiters": ["api-translation-arbiter", "pydantic-migration-arbiter"],
  "whyAccepted": "Migrating to model_dump() eliminates deprecation warnings and takes advantage of the mode='json' option which simplifies serialization of complex types (datetime, Decimal, UUID) without custom encoders.",
  "whyRejected": "Deferring migration until removal creates a forced migration under time pressure. Deprecation warnings in production logs obscure real warnings. model_dump()'s mode='json' option is a meaningful improvement for API serialization that .dict() does not support.",
  "sourceAnchors": ["https://docs.pydantic.dev/latest/migration/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-653 | SWE-BENCH | Family 6 | Domain F

**Unit title:** Python SQLAlchemy async session vs sync session for FastAPI database result storage
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT653-F6-F",
  "sourceIds": ["EXT-653"],
  "sourceTrustTier": 2,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "async-orm-result-storage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A FastAPI application uses SQLAlchemy sync Session for database access. Under load, database queries block the asyncio event loop, causing high latency for all concurrent requests even those not waiting on the database.",
    "b": "FastAPI is an async framework — all path operations run on the asyncio event loop. Sync SQLAlchemy calls block the event loop during the entire query duration, preventing other coroutines from running. SQLAlchemy 1.4+ provides AsyncSession with asyncpg or aiosqlite drivers for non-blocking async database access.",
    "c": "Using sync SQLAlchemy in FastAPI requires running queries in a thread executor: await asyncio.run_in_executor(None, sync_db_operation). AsyncSession with await session.execute() is the native async approach."
  },
  "function": "Select SQLAlchemy session mode for non-blocking FastAPI database result storage",
  "acceptedOutput": "Migrate to SQLAlchemy AsyncSession with asyncpg: engine = create_async_engine(DATABASE_URL); async_session = async_sessionmaker(engine). In FastAPI dependency: async def get_db(): async with async_session() as session: yield session. All ORM calls use await: result = await session.execute(select(Order)). The event loop is not blocked during database I/O.",
  "rejectedOutput": "Keep sync SQLAlchemy but wrap all database calls in asyncio.run_in_executor to avoid blocking the event loop.",
  "arbiters": ["registry-storage-arbiter", "async-orm-strategy-arbiter"],
  "whyAccepted": "AsyncSession with asyncpg provides native async database access — the event loop handles other requests during database I/O. The asyncpg driver is written in C with asyncio integration, providing both performance and non-blocking semantics.",
  "whyRejected": "run_in_executor offloads sync operations to a thread pool. Database queries still block a thread (from the default ThreadPoolExecutor). Under high concurrency, the thread pool saturates, causing the same blocking behavior in a different location. It is a workaround, not a solution.",
  "sourceAnchors": ["https://docs.sqlalchemy.org/en/20/orm/extensions/asyncio.html"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-654 | PYTHON-PEPS | Family 3 | Domain D

**Unit title:** PEP-544 Protocol for structural subtyping — duck typing with static verification execution
**Source URL:** https://peps.python.org/pep-0544/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT654-F3-D",
  "sourceIds": ["EXT-654"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "structural-typing-protocol-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A function accepts any object with a read(n: int) -> bytes method. Using ABC requires all implementors to explicitly inherit from the ABC. Using Any loses type information. The team needs a way to express 'any object with read()' with static type checking support.",
    "b": "PEP-544 Protocol enables structural subtyping: class Readable(Protocol): def read(self, n: int) -> bytes: .... Any class with a compatible read() method satisfies Readable without explicit inheritance. Type checkers (mypy, pyright) verify structural conformance at development time.",
    "c": "runtime_checkable Protocol supports isinstance() checks at runtime. Protocol is useful for third-party types that cannot inherit from an ABC (e.g., io.BytesIO satisfies Readable without inheriting from it)."
  },
  "function": "Apply Protocol for duck-typed interface execution with static verification",
  "acceptedOutput": "Define class Readable(Protocol): def read(self, n: int) -> bytes: .... Annotate the function: def process(source: Readable) -> None: data = source.read(1024). Any object with a compatible read() method (io.BytesIO, socket.makefile(), custom classes) satisfies the type without explicit inheritance. Type checkers flag objects missing read() at development time.",
  "rejectedOutput": "Use Union[io.BytesIO, io.FileIO, socket.SocketIO] to enumerate all accepted types.",
  "arbiters": ["execution-applier-arbiter", "protocol-typing-arbiter"],
  "whyAccepted": "Protocol expresses the intent (any readable object) without enumerating every possible type. Adding a new type that has read() works automatically. Union requires adding every new type manually — a closed set approach to an open structural requirement.",
  "whyRejected": "Union[io.BytesIO, io.FileIO, socket.SocketIO] is a closed enumeration. A caller with a custom Readable object (e.g., from a network library) cannot use the function without a Union update. Protocol captures the structural requirement without limiting the set of valid types.",
  "sourceAnchors": ["https://peps.python.org/pep-0544/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-655 | OPENTELEMETRY-SPEC | Family 7 | Domain C

**Unit title:** OTel trace context propagation formats — W3C TraceContext vs B3 for cross-service translation
**Source URL:** https://opentelemetry.io/docs/specs/otel/context/api-propagators/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT655-F7-C",
  "sourceIds": ["EXT-655"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "trace-propagation-format-translation",
  "rowClass": "migration-decision",
  "input": {
    "a": "A microservices platform mixes services using Zipkin (B3 propagation) and newer services using OTel (W3C TraceContext). Traces break at the boundary between old and new services because the propagation formats are different headers.",
    "b": "W3C TraceContext (RFC-9485): traceparent and tracestate headers. B3: X-B3-TraceId, X-B3-SpanId, X-B3-Sampled (or single X-B3 header). OTel SDK supports both formats via composite propagators.",
    "c": "OTel composite propagator: TextMapPropagator.Composite([W3cTraceContextPropagator(), B3Propagator()]). The composite propagator extracts from both formats on incoming requests and injects W3C format on outgoing requests. Zipkin-instrumented services receive W3C headers they do not understand but also receive B3 headers they do."
  },
  "function": "Translate trace context propagation format for mixed Zipkin/OTel service environment",
  "acceptedOutput": "Configure OTel services with composite propagator: new CompositeTextMapPropagator(new TraceContextPropagator(), new B3Propagator()). OTel services extract from both W3C and B3 headers. Inject both W3C and B3 on outgoing requests during migration. Gradually migrate Zipkin services to OTel; remove B3 propagation after all services are migrated.",
  "rejectedOutput": "Use only W3C TraceContext on new OTel services. Zipkin services will need to be updated to accept W3C headers before connecting.",
  "arbiters": ["api-translation-arbiter", "propagation-format-migration-arbiter"],
  "whyAccepted": "Composite propagation allows gradual migration without a flag-day cutover. New OTel services interoperate with existing Zipkin services via B3 injection. When Zipkin services migrate, B3 injection can be removed without disrupting the W3C propagation that new services use.",
  "whyRejected": "Requiring all Zipkin services to update before connecting new OTel services creates a migration dependency chain. In a large microservices environment, this blocks OTel adoption until all services are simultaneously ready — a coordination cost that composite propagation eliminates.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/context/api-propagators/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-656 | DAPPER | Family 3 | Domain D

**Unit title:** Dapper connection string security — avoiding hardcoded credentials in DbConnection factory
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT656-F3-D",
  "sourceIds": ["EXT-656"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "connection-credential-execution-security",
  "rowClass": "negative-example",
  "input": {
    "a": "A Dapper repository constructs its SqlConnection with a hardcoded connection string: new SqlConnection(\"Server=prod-db;Database=orders;User=sa;Password=P@ssw0rd\"). The connection string appears in source control history and application logs.",
    "b": "Connection strings with embedded credentials should not be hardcoded in source code (CWE-798) or logged (CWE-200). .NET configuration pattern: read from IConfiguration (which reads from environment variables, Azure Key Vault, AWS Secrets Manager, or appsettings.json without secrets in source control).",
    "c": "For Azure SQL: use Managed Identity authentication (no password in connection string): new SqlConnection(\"Server=prod-db;Database=orders;Authentication=Active Directory Default\"). The SQL connection authenticates via the pod/VM Managed Identity token without any stored credential."
  },
  "function": "Secure Dapper DbConnection factory against hardcoded credential anti-pattern",
  "acceptedOutput": "Inject IConfiguration and read the connection string from configuration: new SqlConnection(configuration.GetConnectionString(\"Orders\")). Store the actual connection string in Azure Key Vault or environment variables, not in source code. For Azure: use Managed Identity connection string: Authentication=Active Directory Default eliminates the password entirely.",
  "rejectedOutput": "Move the hardcoded connection string to appsettings.json. This removes it from source code and the compiled binary.",
  "arbiters": ["execution-applier-arbiter", "credential-security-arbiter"],
  "whyAccepted": "IConfiguration with Azure Key Vault or environment variables ensures credentials are never in source control, are rotatable without code changes, and are access-controlled via Azure RBAC. Managed Identity eliminates stored credentials entirely.",
  "whyRejected": "appsettings.json is typically committed to source control. Moving the hardcoded string to a committed JSON file relocates the credential but does not protect it. appsettings.json without credentials (using Key Vault references or environment variable overrides) is correct; appsettings.json with credentials is not.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-657 | ANGULAR-DOCS | Family 3 | Domain D

**Unit title:** Angular 17 dependency injection — inject() function vs constructor injection for service execution
**Source URL:** https://angular.dev/guide/di/dependency-injection
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT657-F3-D",
  "sourceIds": ["EXT-657"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "angular-di-execution-pattern",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 component uses constructor injection for 5 services: constructor(private authService: AuthService, private router: Router, private store: Store, private http: HttpClient, private logger: LoggerService). The team explores using the inject() function instead.",
    "b": "inject() function (Angular 14+) can be called in the injection context (constructor, field initializer, factory function, runInInjectionContext). It returns the injected service: private authService = inject(AuthService). It works in standalone functions (route guards, resolvers) without class constructors.",
    "c": "inject() enables composable injection in standalone functions and improves tree-shaking for unused injections. Constructor injection is the traditional pattern and remains supported. Both produce the same result in a component class."
  },
  "function": "Select Angular dependency injection pattern for component service execution",
  "acceptedOutput": "Use inject() for new Angular 17 components: private authService = inject(AuthService); private router = inject(Router). This pattern is required for standalone functions (CanActivateFn guards, resolvers) and is preferred in the Angular team's own docs for new components. Existing constructor-injected code does not need to be migrated.",
  "rejectedOutput": "Keep constructor injection for all components. inject() is syntactic sugar without functional benefit and unfamiliar to developers coming from Angular 2-16.",
  "arbiters": ["execution-applier-arbiter", "angular-di-pattern-arbiter"],
  "whyAccepted": "inject() is required for standalone functions (route guards, resolvers, custom hooks) without class constructors. Using the same injection pattern in both class and function contexts reduces cognitive switching. Angular official docs now demonstrate inject() as the primary pattern.",
  "whyRejected": "Constructor injection remains valid but is limited to class contexts. As Angular moves toward functional patterns (functional guards, functional resolvers), inject() becomes necessary. Avoiding it creates inconsistency between class-based and function-based patterns in the same codebase.",
  "sourceAnchors": ["https://angular.dev/guide/di/dependency-injection"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-658 | REACT-ADVANCED | Family 3 | Domain D

**Unit title:** React custom hook extraction — when to extract useState+useEffect into a composable hook
**Source URL:** https://react.dev/learn/reusing-logic-with-custom-hooks
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT658-F3-D",
  "sourceIds": ["EXT-658"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "custom-hook-extraction-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React component has 40 lines of useState/useEffect for window resize tracking (windowWidth, windowHeight, isPortrait). Three other components implement identical resize tracking logic. The team debates extracting a custom hook.",
    "b": "Custom hooks extract stateful logic (useState + useEffect combinations) into reusable functions prefixed with 'use'. They are plain JavaScript functions — no special syntax. Each component that calls a custom hook gets its own isolated state (not shared).",
    "c": "The trigger for extraction: the same useState+useEffect pattern appears in 2+ components, OR the logic is complex enough to benefit from isolation and naming, OR the component needs to be readable without understanding the implementation details."
  },
  "function": "Apply custom hook extraction for duplicated stateful logic execution",
  "acceptedOutput": "Extract useWindowSize(): function useWindowSize() { const [size, setSize] = useState({ width: window.innerWidth, height: window.innerHeight }); useEffect(() => { const handler = () => setSize({ width: window.innerWidth, height: window.innerHeight }); window.addEventListener('resize', handler); return () => window.removeEventListener('resize', handler); }, []); return { ...size, isPortrait: size.height > size.width }; }. Each component calling useWindowSize() gets its own isolated size state.",
  "rejectedOutput": "Keep the resize logic inline in each component. Extracting to a hook adds abstraction that makes the code harder to trace.",
  "arbiters": ["execution-applier-arbiter", "hook-extraction-arbiter"],
  "whyAccepted": "The same 40-line stateful pattern in 4 components is clear duplication with associated drift risk (one component fixes a bug, the others do not). useWindowSize() names the abstraction and encapsulates the cleanup logic (removeEventListener) in one place.",
  "whyRejected": "'Harder to trace' is inversed — useWindowSize() at the component call site names the concern clearly. 40 lines of useState/useEffect inline requires reading the implementation to understand 'what does this component do with events.' The hook is more readable, not less.",
  "sourceAnchors": ["https://react.dev/learn/reusing-logic-with-custom-hooks"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-659 | ASPNETCORE-DOCS-SAMPLES | Family 3 | Domain D

**Unit title:** ASP.NET Core model binding — [FromBody] vs [FromForm] vs [FromQuery] for request data execution
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT659-F3-D",
  "sourceIds": ["EXT-659"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "request-binding-source-execution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An ASP.NET Core minimal API endpoint has parameters with no binding source specified. The framework attempts to auto-bind from route values, query string, and body. A complex object parameter fails to bind when sent as JSON body because the endpoint is not attributed with [FromBody].",
    "b": "ASP.NET Core minimal APIs infer binding source: simple types (string, int) → route/query, complex types → body (JSON). ASP.NET Core MVC requires explicit [FromBody] for JSON body binding. Minimal APIs can also read from IFormCollection, IFormFile (multipart/form-data) using [FromForm].",
    "c": "Binding source attribute table: [FromBody] = JSON/XML body, [FromForm] = multipart/form-data or application/x-www-form-urlencoded, [FromQuery] = query string, [FromRoute] = route template, [FromHeader] = HTTP header, [FromServices] = DI container."
  },
  "function": "Apply explicit binding source attributes for correct request data execution",
  "acceptedOutput": "Use explicit binding source attributes: [FromBody] for JSON request body, [FromForm] for multipart form uploads, [FromQuery] for filter parameters. In minimal APIs: app.MapPost(\"/orders\", ([FromBody] CreateOrderRequest request, [FromQuery] string? source, [FromServices] IOrderService svc) => ...). Explicit attributes prevent ambiguous binding and make the API contract clear.",
  "rejectedOutput": "Rely on auto-binding inference. ASP.NET Core is smart enough to determine the correct binding source for each parameter type.",
  "arbiters": ["execution-applier-arbiter", "binding-source-arbiter"],
  "whyAccepted": "Explicit binding attributes are self-documenting (the parameter declaration states where the value comes from) and prevent unexpected behavior when parameter names collide between query string and body, or when the default inference changes between framework versions.",
  "whyRejected": "Auto-binding inference works for simple cases but fails for: complex types in query strings, simple types that should come from the body, or when the same parameter name exists in multiple sources. Explicit attributes eliminate the inference ambiguity entirely.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-660 | NESTJS-ADVANCED | Family 5 | Domain E

**Unit title:** NestJS guard vs middleware vs pipe for request validation arbitration hierarchy
**Source URL:** https://docs.nestjs.com/guards
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT660-F5-E",
  "sourceIds": ["EXT-660"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "request-validation-layer-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS API needs: (1) JWT authentication check, (2) input format validation (required fields, types), (3) business rule validation (user must own the resource). The team debates whether to put all three in middleware, a guard, or a pipe.",
    "b": "NestJS execution order: Middleware → Guards → Interceptors (pre) → Pipes → Handler → Interceptors (post) → Exception Filters. Middleware: runs first, no access to route handler metadata. Guards: access ExecutionContext with route metadata (@Roles decorator). Pipes: transform and validate request data (DTOs via class-validator). Business logic: belongs in the service layer, not HTTP concerns.",
    "c": "Authentication guard (JWT): use Guard because it needs access to @Public() decorator metadata to skip auth on public routes. Input validation: use ValidationPipe (global or per-route). Business rule check (ownership): use resource-based authorization in the service, or a custom Guard if it needs to reject early."
  },
  "function": "Arbitrate NestJS request validation layer for authentication, input, and business rule concerns",
  "acceptedOutput": "Separate by concern: (1) JWT authentication → AuthGuard (access to ExecutionContext for @Public() metadata). (2) Input validation → global ValidationPipe with whitelist:true, forbidNonWhitelisted:true. (3) Resource ownership → service layer with IAuthorizationService or custom PolicyGuard. Each layer handles what it was designed for.",
  "rejectedOutput": "Put all three validations in middleware. Middleware runs first and provides the fastest rejection path for invalid requests.",
  "arbiters": ["security-arbiter", "validation-layer-arbitration-arbiter"],
  "whyAccepted": "Separation of concerns: Guards have access to route metadata (decorators) that middleware does not. ValidationPipe is designed for DTO validation and integrates with class-validator. Service-layer ownership checks have access to the database and domain logic. Each layer is appropriate for its concern.",
  "whyRejected": "Middleware cannot access route metadata (decorators like @Public() or @Roles()). Implementing authentication in middleware requires manual route matching for public routes — brittle and not DRY. Middleware is appropriate for cross-cutting concerns that don't need route metadata.",
  "sourceAnchors": ["https://docs.nestjs.com/guards"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-661 | IETF-RFCS | Family 3 | Domain D

**Unit title:** RFC-8288 Web Linking — Link headers for pagination and resource relationship execution
**Source URL:** https://www.rfc-editor.org/rfc/rfc8288
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT661-F3-D",
  "sourceIds": ["EXT-661"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "pagination-link-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A REST API returns paginated results with next/prev/first/last page URLs embedded in the JSON response body. GitHub, GitLab, and other APIs use HTTP Link headers instead of body-embedded pagination links. The team evaluates which approach to adopt.",
    "b": "RFC-8288 Web Linking defines the Link header: Link: <https://api.example.com/orders?page=2>; rel=\"next\", <https://api.example.com/orders?page=1>; rel=\"prev\". rel values include: next, prev, first, last. HTTP clients (browsers, curl) natively understand Link headers without parsing the JSON body.",
    "c": "Body-embedded links: pagination metadata is part of the response schema, clients parse JSON to find next page URL. Both approaches work, but Link headers enable pagination without parsing the body (streaming clients, HTTP middleware that adds pagination awareness)."
  },
  "function": "Select pagination link mechanism for REST API response execution",
  "acceptedOutput": "Use RFC-8288 Link headers for pagination: Response.Headers.Append(\"Link\", $\"<{nextUrl}>; rel=\\\"next\\\", <{prevUrl}>; rel=\\\"prev\\\"\"). Include total count in X-Total-Count header. The body contains only the data array. HTTP middleware, API gateways, and generic clients can handle pagination via headers without knowing the JSON schema.",
  "rejectedOutput": "Embed pagination links in the JSON body: { \"data\": [...], \"pagination\": { \"nextUrl\": \"...\", \"prevUrl\": \"...\", \"total\": 1000 } }.",
  "arbiters": ["execution-applier-arbiter", "pagination-link-arbiter"],
  "whyAccepted": "RFC-8288 Link headers are the HTTP-standard pagination mechanism. They separate pagination metadata from resource data, allowing the body to be a pure data array. Clients that need only the data can ignore headers; clients needing pagination use the standard Link header.",
  "whyRejected": "Body-embedded pagination adds a wrapper object around the data. Clients must know the specific JSON schema to extract the data array and next page URL. Type-safe clients need generic pagination wrappers in their model classes. Link headers avoid this coupling.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc8288"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-662 | GOOGLE-AIP | Family 1 | Domain A

**Unit title:** AIP-121 resource-oriented design — resource hierarchy for multi-tenant API topology
**Source URL:** https://google.aip.dev/121
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT662-F1-A",
  "sourceIds": ["EXT-662"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "multi-tenant-resource-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A SaaS API has Organizations, Projects (within Organizations), and Resources (within Projects). The team debates URL topology: /api/resources?org=X&project=Y vs /organizations/{org}/projects/{project}/resources.",
    "b": "AIP-121 resource-oriented design: resources are identified by their place in a hierarchy. The hierarchy is expressed in the URL path: /organizations/{org}/projects/{project}/resources/{resource}. Query parameters are for filtering within a collection, not for identifying the parent resource.",
    "c": "Hierarchical URLs: authorization is expressed structurally (access to /organizations/{org} implies access to list its projects), caching is correct (ETags and caches can use the URL as the cache key), and the parent context is always explicit."
  },
  "function": "Design multi-tenant API resource hierarchy topology per AIP-121",
  "acceptedOutput": "Use hierarchical URLs: /v1/organizations/{org}/projects/{project}/resources. CRUD operations: GET (list), POST (create) on /organizations/{org}/projects/{project}/resources; GET, PATCH, DELETE on /organizations/{org}/projects/{project}/resources/{resource}. Authorization policies are path-based and composable.",
  "rejectedOutput": "Use flat URLs with query parameters: /v1/resources?orgId={org}&projectId={project}. This is simpler to implement and easier for clients to construct.",
  "arbiters": ["topology-arbiter", "resource-hierarchy-arbiter"],
  "whyAccepted": "Hierarchical URLs make the parent-child relationship structural and self-documenting. Authorization policies, CDN routing rules, and API gateway rate limiting can target the organization or project prefix. AIP-121 is the Google Cloud API design standard — following it ensures compatibility with Google Cloud tooling (API Explorer, client library generation).",
  "whyRejected": "Flat URLs with query parameters treat the organization and project as filters on a global resource collection. This loses the structural authorization expression, makes authorization policy authoring harder (cannot use path-based rules), and does not follow any standard API design guidance.",
  "sourceAnchors": ["https://google.aip.dev/121"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-663 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF evaluation — held-out human evaluation vs automated reward model scoring for pipeline
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT663-F8-G",
  "sourceIds": ["EXT-663"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "rlhf-evaluation-pipeline-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An RLHF training pipeline evaluates model quality using only the reward model score on a held-out set. After 5 training iterations, reward scores plateau but the team suspects the model has mode-collapsed (high reward, low diversity). Automated evaluation does not capture this.",
    "b": "Reward model evaluation suffers from Goodhart's Law — the policy optimizes the proxy (reward model) which diverges from the true objective (human preference). Periodic human evaluation (A/B comparison of outputs from different training iterations) provides ground truth. Automated metrics (reward score, perplexity, BLEU) are fast but can be fooled by reward hacking.",
    "c": "Evaluation cadence: automated metrics every training step (cheap), human evaluation every N iterations (expensive but necessary). oasst1 provides a held-out human evaluation set for comparing model generations."
  },
  "function": "Design RLHF evaluation pipeline with automated and human evaluation tiers",
  "acceptedOutput": "Implement two-tier evaluation: (1) Automated: reward model score on held-out pairs after each training epoch — detects reward collapse or stagnation. (2) Human: side-by-side preference evaluation every 2-3 training iterations using a small annotator panel (30-50 prompts). Stop training when human evaluation shows plateau or regression. Add diversity metrics (response length distribution, distinct-n) to automated tier.",
  "rejectedOutput": "Use only reward model scoring. Human evaluation is too expensive for frequent use and the reward model is trained to approximate human preferences.",
  "arbiters": ["learning-quality-arbiter", "evaluation-pipeline-arbiter"],
  "whyAccepted": "Human evaluation is the only ground truth for detecting reward hacking — the reward model cannot detect its own exploitation. Human evaluation every 2-3 iterations (not every step) balances cost against the risk of over-training on a exploited reward model.",
  "whyRejected": "Reward model scores plateau or increase even during reward hacking. A model producing one preferred response pattern repeatedly has high reward model scores but poor human evaluation scores. Training on reward model scores alone without human checkpoints reliably produces collapsed models.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-664 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba sentence tokenization consistency — SentencePiece vs word tokenization for multilingual MT
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT664-F2-C",
  "sourceIds": ["EXT-664"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "tokenization-consistency-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A multilingual MT model trains on Tatoeba data tokenized using word-level tokenization (whitespace split + punctuation split). For agglutinative languages (Finnish, Turkish, Hungarian), word-level tokenization produces a large vocabulary with many rare tokens, causing OOV (Out-of-Vocabulary) issues.",
    "b": "SentencePiece (BPE or unigram language model) tokenizes text into subword units without language-specific rules. It handles agglutinative languages by splitting compound words into frequent subword units. Vocabulary size is fixed (8K-64K tokens) and shared across all languages in multilingual models.",
    "c": "Shared SentencePiece vocabulary enables cross-lingual transfer: semantically similar subwords in different languages share representations. Word tokenization requires separate vocabularies per language or a large joint vocabulary."
  },
  "function": "Select tokenization strategy for multilingual Tatoeba translation dataset",
  "acceptedOutput": "Train SentencePiece with joint vocabulary (32K tokens) on the full multilingual corpus. All language pairs use the same tokenizer, enabling cross-lingual subword sharing. For CJK languages (Chinese, Japanese): add character-level fallback (character unigrams) to SentencePiece to handle rare characters. Apply consistent tokenization to training, validation, and inference.",
  "rejectedOutput": "Use language-specific tokenizers for each language pair. Finnish NLTK tokenizer for Finnish, jieba for Chinese, etc. This optimizes tokenization quality per language.",
  "arbiters": ["translation-quality-arbiter", "tokenization-strategy-arbiter"],
  "whyAccepted": "Shared SentencePiece vocabulary enables cross-lingual transfer in multilingual models and avoids OOV for agglutinative languages. Consistent tokenization across all language pairs simplifies the training pipeline and ensures evaluation metrics are computed on comparable token sequences.",
  "whyRejected": "Language-specific tokenizers require N tokenizers for N languages, prevent cross-lingual subword sharing, and complicate multi-lingual model training (different vocabularies cannot be shared in a single embedding layer). The per-language quality improvement does not compensate for the loss of cross-lingual transfer.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-665 | VUE-DOCS | Family 7 | Domain C

**Unit title:** Vue 3 SSR hydration mismatch — server vs client rendered content translation errors
**Source URL:** https://vuejs.org/guide/scaling-up/ssr#hydration-mismatch
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT665-F7-C",
  "sourceIds": ["EXT-665"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "ssr-hydration-error-translation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Nuxt 3 (Vue SSR) application shows a hydration mismatch error in the browser console: 'Hydration text content mismatch'. The server renders a timestamp (new Date().toString()) that differs from the client's timestamp during hydration.",
    "b": "SSR hydration matches server-rendered HTML with client-side Vue's virtual DOM. Any difference between server-rendered content and what the client-side Vue produces causes a mismatch — Vue warns and re-renders from scratch (defeating SSR's purpose). Sources of mismatch: time-dependent values, browser-only APIs (window, document), random values, or client-side state.",
    "c": "Solutions: (1) use ClientOnly wrapper for client-only content, (2) use useState() in Nuxt to share state between server and client, (3) avoid rendering time/random values during SSR, (4) use v-if with mounted ref for browser-specific rendering."
  },
  "function": "Translate SSR hydration mismatch cause and remediation for server-rendered content",
  "acceptedOutput": "Wrap time-dependent content in <ClientOnly>: <ClientOnly><span>{{ currentTime }}</span><template #fallback><span>Loading...</span></template></ClientOnly>. The server renders the fallback; the client renders the actual time after hydration. Alternatively, use a static server-time value from Nuxt's useFetch() or useAsyncData() which is consistent between server and client.",
  "rejectedOutput": "Suppress hydration warnings in development: app.config.warnHandler = null. The mismatch is only a warning and does not break functionality.",
  "arbiters": ["api-translation-arbiter", "ssr-hydration-arbiter"],
  "whyAccepted": "ClientOnly renders the fallback on the server (consistent HTML), then replaces with client content after hydration (no mismatch). This is the idiomatic Nuxt pattern for client-only content like timestamps, user-specific data, or browser API output.",
  "whyRejected": "Suppressing warnings hides a real problem. SSR hydration mismatches cause Vue to discard server-rendered HTML and re-render from scratch — the performance benefit of SSR is lost. The mismatch is a correctness issue (content may flash or jitter), not just a warning.",
  "sourceAnchors": ["https://vuejs.org/guide/scaling-up/ssr#hydration-mismatch"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-666 | EFCORE-SOURCE | Family 3 | Domain D

**Unit title:** EF Core interceptors — SaveChangesInterceptor for audit trail vs manual audit field execution
**Source URL:** https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT666-F3-D",
  "sourceIds": ["EXT-666"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "audit-trail-execution-pattern",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An application needs to automatically set CreatedAt, UpdatedAt, CreatedBy, and UpdatedBy on all entities before saving. The team sets these fields manually in each service method: entity.UpdatedAt = DateTime.UtcNow; entity.UpdatedBy = currentUser.Id; before every SaveChanges() call.",
    "b": "EF Core ISaveChangesInterceptor.SavingChangesAsync() intercepts before SaveChanges executes. It can inspect context.ChangeTracker.Entries<IAuditableEntity>() and set audit fields automatically for all entities marked Added or Modified. This runs once per SaveChanges, not per service call.",
    "c": "Alternative: override DbContext.SaveChangesAsync() to apply audit fields before calling base.SaveChangesAsync(). ISaveChangesInterceptor is more composable (registered via DbContextOptionsBuilder.AddInterceptors()) and works without subclassing DbContext."
  },
  "function": "Apply EF Core SaveChangesInterceptor for audit trail execution",
  "acceptedOutput": "Implement AuditInterceptor : SaveChangesInterceptor { override ValueTask<InterceptionResult<int>> SavingChangesAsync(...) { foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>()) { if (entry.State == Added) entry.Entity.CreatedAt = _clock.UtcNow; if (entry.State is Added or Modified) { entry.Entity.UpdatedAt = _clock.UtcNow; entry.Entity.UpdatedBy = _currentUser.Id; } } return base.SavingChangesAsync(...); } }. Register: services.AddDbContext<AppDbContext>(o => o.AddInterceptors(new AuditInterceptor(...))).",
  "rejectedOutput": "Create a base repository class that sets audit fields in Save() method. All repositories inherit from BaseRepository to get audit behavior.",
  "arbiters": ["execution-applier-arbiter", "audit-trail-arbiter"],
  "whyAccepted": "ISaveChangesInterceptor runs unconditionally before every SaveChanges — direct DbContext calls, repository calls, and background job calls all go through the interceptor. No repository inheritance chain can be bypassed. Audit fields are set at the persistence boundary.",
  "whyRejected": "Base repository inheritance only covers code that uses the repository. Direct DbContext.SaveChanges() calls (migration scripts, background jobs, admin tools) bypass the base repository and produce entities without audit fields. The interceptor covers all paths.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/interceptors"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-667 | DOTNET-MICROSERVICES | Family 8 | Domain G

**Unit title:** .NET container image optimization — multi-stage Dockerfile for production image size learning
**Source URL:** https://learn.microsoft.com/en-us/dotnet/core/docker/build-container
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT667-F8-G",
  "sourceIds": ["EXT-667"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "container-image-optimization-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET 9 microservice Dockerfile uses a single stage: FROM mcr.microsoft.com/dotnet/sdk:9.0; COPY . .; RUN dotnet publish -o /app; ENTRYPOINT [\"dotnet\", \"/app/myapp.dll\"]. The resulting image is 900MB (includes the full .NET SDK).",
    "b": "Multi-stage Dockerfile: Stage 1 (build): use dotnet/sdk image for compilation. Stage 2 (runtime): use dotnet/aspnet runtime-only image (~200MB) and COPY only the published output from Stage 1. The final image contains only the runtime + published app, not the SDK.",
    "c": "Further optimization: dotnet/runtime-deps image + self-contained publish (--self-contained true --runtime linux-x64) produces a single binary without requiring the .NET runtime in the image (~100MB). AOT (Ahead-of-Time) compilation further reduces startup time."
  },
  "function": "Record multi-stage Dockerfile optimization for .NET container image size learning",
  "acceptedOutput": "Use multi-stage Dockerfile: Stage 1 FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build; WORKDIR /src; COPY . .; RUN dotnet publish -c Release -o /app/publish. Stage 2 FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime; COPY --from=build /app/publish /app; ENTRYPOINT [\"dotnet\", \"/app/myapp.dll\"]. Final image: ~220MB vs 900MB single-stage.",
  "rejectedOutput": "Keep the single-stage Dockerfile. Image size does not affect runtime performance and storage is cheap.",
  "arbiters": ["learning-arbiter", "container-optimization-arbiter"],
  "whyAccepted": "75% image size reduction (900MB → 220MB) reduces: container registry egress costs, pod startup time (image pull), attack surface (SDK not in production image), and vulnerability scan surface (fewer packages). Multi-stage Dockerfiles are the .NET Docker best practice documented by Microsoft.",
  "whyRejected": "Image size affects: registry storage cost, pull bandwidth per deployment (Kubernetes pulls the image on every new node), cold start time, and security scan scope. The SDK image contains build tools (MSBuild, NuGet, compilers) that should not be in a production container and expand the attack surface.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/dotnet/core/docker/build-container"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-668 | MICROSOFT-LEARN-ARCH | Family 5 | Domain E

**Unit title:** Azure DDoS Protection Standard vs Basic — arbitration for public-facing API security
**Source URL:** https://learn.microsoft.com/en-us/azure/ddos-protection/ddos-protection-overview
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT668-F5-E",
  "sourceIds": ["EXT-668"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "ddos-protection-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A public-facing Azure API behind an Application Gateway uses Azure DDoS Basic (included free). The API handles financial transactions. The team evaluates upgrading to DDoS Protection Standard ($2,944/month per protected virtual network).",
    "b": "DDoS Basic: always-on traffic monitoring, automatic attack mitigation for Azure infrastructure. Protects Azure services globally but with limited customization. DDoS Standard: per-VNet protection, adaptive real-time tuning per protected public IP, attack analytics, attack metrics, and cost protection SLA (Azure credits if legitimate traffic is throttled). DDoS Standard also covers Application Gateway public IPs.",
    "c": "DDoS Standard is recommended for: production environments with public-facing resources, applications where downtime has significant financial impact, or regulated industries requiring DDoS protection documentation."
  },
  "function": "Arbitrate Azure DDoS protection tier for financial transaction API security",
  "acceptedOutput": "Enable DDoS Protection Standard for a financial transaction API. The cost ($2,944/month) is justified by: (1) transaction revenue protected during an attack, (2) SLA with Azure DDoS cost protection credits, (3) attack analytics for incident response, (4) regulated financial industry compliance documentation. Combine with Application Gateway WAF for L7 attack protection.",
  "rejectedOutput": "Use DDoS Basic. The cost of DDoS Standard is not justified — Azure's infrastructure already provides significant DDoS protection at no cost.",
  "arbiters": ["security-arbiter", "ddos-protection-arbitration-arbiter"],
  "whyAccepted": "Financial transaction APIs have direct revenue impact during downtime. DDoS Standard's adaptive tuning reduces false-positive throttling of legitimate traffic. Attack analytics enable rapid incident response. Compliance documentation supports financial regulatory requirements.",
  "whyRejected": "DDoS Basic protects Azure's infrastructure but not application-layer resources at the granularity needed for a financial API. Adaptive per-IP tuning in DDoS Standard prevents legitimate transaction traffic from being throttled during volumetric attacks — Basic's generic thresholds may not differentiate.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/ddos-protection/ddos-protection-overview"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-669 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-434 unrestricted file upload — content type validation and storage isolation arbitration
**Source URL:** https://cwe.mitre.org/data/definitions/434.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT669-F5-E",
  "sourceIds": ["EXT-669"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "file-upload-validation-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core file upload endpoint checks Content-Type: image/jpeg from the HTTP header and the filename extension (.jpg) before saving the file. An attacker uploads a .js file renamed to exploit.jpg with Content-Type: image/jpeg, and the file is saved to the web root and executed when accessed.",
    "b": "Content-Type header and filename extension are client-provided and untrusted. File content signature (magic bytes) validation checks the actual file content: JPEG files start with FF D8 FF. CWE-434 requires server-side content validation, not relying on client-provided metadata.",
    "c": "Defense layers for file uploads: (1) validate file content signature (magic bytes), (2) save to isolated storage outside web root (Azure Blob Storage, S3), (3) serve via signed URLs or streaming endpoint (not direct file URL), (4) rename files on upload (UUID.ext — strip original filename), (5) scan with antivirus (Azure Defender for Storage)."
  },
  "function": "Arbitrate file upload validation strategy to prevent CWE-434 unrestricted file upload",
  "acceptedOutput": "Implement defense in depth: (1) Read first 4 bytes of the uploaded stream, verify JPEG magic bytes (FF D8 FF E0 or FF D8 FF E1). (2) Rename the file to a server-generated GUID with a whitelist extension (.jpg). (3) Store in Azure Blob Storage (not web root). (4) Serve via app endpoint that sets Content-Disposition: attachment to prevent in-browser execution.",
  "rejectedOutput": "Validate the Content-Type header and file extension. Additionally, reject files larger than 10MB to reduce attack surface.",
  "arbiters": ["security-arbiter", "file-upload-validation-arbiter"],
  "whyAccepted": "Magic byte validation checks the actual file content, which the attacker cannot fake without the file being a valid JPEG. Storage outside the web root prevents direct URL execution. UUID renaming removes malicious filename payloads.",
  "whyRejected": "Content-Type header is trivially spoofed. Extension validation is trivially bypassed by renaming. Size limits prevent DoS but do not prevent malicious file content. All three rejected controls are client-provided metadata that attackers control.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/434.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-670 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V12.3 file execution prevention — MIME type sniffing and Content-Disposition arbitration
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT670-F5-E",
  "sourceIds": ["EXT-670"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "file-execution-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core file serving endpoint returns user-uploaded files with their original Content-Type header (stored at upload time). A user uploads an HTML file with Content-Type: text/html — when served, the browser renders it as HTML, enabling stored XSS.",
    "b": "OWASP ASVS V12.3.1 requires that user-supplied content is served with Content-Disposition: attachment to prevent in-browser execution. X-Content-Type-Options: nosniff prevents browsers from MIME-sniffing and executing content as a different type than declared.",
    "c": "Content-Disposition: attachment forces the browser to download the file rather than render it. Combined with X-Content-Type-Options: nosniff, uploaded HTML files cannot execute as web pages when served through the application endpoint."
  },
  "function": "Arbitrate file serving headers for ASVS V12.3 execution prevention",
  "acceptedOutput": "Set response headers for all user-uploaded file serving: Response.Headers[\"Content-Disposition\"] = \"attachment; filename=\\\"\" + Path.GetFileName(safeName) + \"\\\"\"; Response.Headers[\"X-Content-Type-Options\"] = \"nosniff\"; Response.ContentType = \"application/octet-stream\". Serve all user content as application/octet-stream with attachment disposition — browsers download rather than render.",
  "rejectedOutput": "Restore the original Content-Type from the database and trust the stored value. Users need to preview uploaded images in the browser.",
  "arbiters": ["security-arbiter", "file-execution-prevention-arbiter"],
  "whyAccepted": "Content-Disposition: attachment + application/octet-stream prevents in-browser execution of any uploaded content. For image preview: use a separate endpoint that serves only validated images (confirmed by magic bytes) with correct image Content-Type, isolated from the general file serving endpoint.",
  "whyRejected": "Restoring original Content-Type trusts client-provided metadata. A user who uploads exploit.html stored as text/html gets the browser to render it as HTML when another user 'views' the file. Previewing images and preventing XSS are separate concerns requiring separate endpoints with different security policies.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
