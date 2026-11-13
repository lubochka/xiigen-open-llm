# External Sources Batch 30 — EXT-591 through EXT-610

---

## EXT-591 | KUBERNETES-KEPS | Family 6 | Domain F

**Unit title:** Kubernetes ConfigMap vs Secret for configuration storage — sensitivity classification
**Source URL:** https://kubernetes.io/docs/concepts/configuration/configmap/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT591-F6-F",
  "sourceIds": ["EXT-591"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "configuration-storage-classification",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Kubernetes deployment stores database connection strings (including passwords) in a ConfigMap referenced as environment variables. A security review flags this as a misclassification of sensitive data.",
    "b": "Kubernetes ConfigMaps store non-sensitive configuration. Secrets store sensitive data (passwords, tokens, keys) and have additional access controls: RBAC can restrict Secret access separately from ConfigMaps, Secrets are not logged by the API server by default, and etcd encryption at rest applies to Secrets. Secrets are base64-encoded (not encrypted) by default but can be encrypted at rest with etcd encryption providers.",
    "c": "Even with Secret limitations (base64 is not encryption), Secrets are the correct classification for credentials because: they are excluded from audit logs by default, RBAC can limit who can read Secrets, and external secret managers (External Secrets Operator, Vault) integrate via the Secret API, not ConfigMap."
  },
  "function": "Classify configuration storage correctly for sensitive credential data in Kubernetes",
  "acceptedOutput": "Move database passwords to Kubernetes Secrets. Reference via envFrom.secretRef or env.valueFrom.secretKeyRef. For production: use External Secrets Operator to sync from Azure Key Vault or AWS Secrets Manager, ensuring secrets are managed externally and not committed to source control.",
  "rejectedOutput": "Keep credentials in ConfigMaps. Restrict access via RBAC on the ConfigMap resource. Encryption is applied at the application level regardless of storage.",
  "arbiters": ["registry-storage-arbiter", "credential-classification-arbiter"],
  "whyAccepted": "Secrets are the Kubernetes-native classification for credentials. RBAC, audit log exclusion, and external secret operator integration all depend on the resource type being Secret. ConfigMap RBAC restrictions are less granular and do not benefit from etcd at-rest encryption for secrets.",
  "whyRejected": "ConfigMap RBAC restrictions apply to the entire ConfigMap, not per-key. Anyone with read access to the ConfigMap for non-sensitive config also gets the password. External encryption at the application level addresses the wrong threat — the config is already in the cluster before the application reads it.",
  "sourceAnchors": ["https://kubernetes.io/docs/concepts/configuration/secret/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-592 | BIGCODEBENCH | Family 5 | Domain E

**Unit title:** Python concurrent.futures ThreadPoolExecutor vs ProcessPoolExecutor for CPU vs I/O bound arbitration
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT592-F5-E",
  "sourceIds": ["EXT-592"],
  "sourceTrustTier": 2,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "concurrency-primitive-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Python script processes 1000 files: each file requires (1) reading from disk (I/O bound) and (2) applying a CPU-intensive compression algorithm. The team uses ThreadPoolExecutor for both phases and observes that the compression phase is not parallelizing effectively.",
    "b": "Python GIL (Global Interpreter Lock) prevents true parallelism for CPU-bound Python code in threads. ThreadPoolExecutor is effective for I/O-bound tasks (disk reads release the GIL). ProcessPoolExecutor uses separate processes (bypassing the GIL) for CPU-bound parallelism.",
    "c": "A pipeline approach: use ThreadPoolExecutor for I/O (file reading), feed results to ProcessPoolExecutor for CPU (compression). Or use a single ProcessPoolExecutor for both phases since subprocesses handle their own I/O independently."
  },
  "function": "Arbitrate concurrency primitive selection for mixed I/O and CPU-bound pipeline",
  "acceptedOutput": "Use ProcessPoolExecutor for the full pipeline (read + compress per worker process). Each subprocess handles its own file I/O independently and applies CPU compression without GIL interference. Set max_workers to CPU count (os.cpu_count()). For files too large to pickle (inter-process transfer), write to temp files and pass paths.",
  "rejectedOutput": "Use ThreadPoolExecutor with max_workers=32 for both phases. More threads will compensate for GIL contention during compression.",
  "arbiters": ["concurrency-arbitration-arbiter", "parallelism-efficiency-arbiter"],
  "whyAccepted": "ProcessPoolExecutor bypasses the GIL for CPU-bound compression. Each process has its own Python interpreter and GIL. File I/O per process is efficient since each process handles a subset of files independently.",
  "whyRejected": "More threads do not bypass the GIL. For CPU-bound Python code, only one thread runs Python bytecode at a time regardless of thread count. ThreadPoolExecutor with 32 threads for compression achieves no better parallelism than with 1 thread for the compression phase.",
  "sourceAnchors": ["https://docs.python.org/3/library/concurrent.futures.html"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-593 | SWE-BENCH | Family 3 | Domain D

**Unit title:** Python abstract base class (ABC) enforcement vs duck typing — interface contract execution
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT593-F3-D",
  "sourceIds": ["EXT-593"],
  "sourceTrustTier": 2,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "interface-contract-enforcement",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A plugin system requires that all plugins implement process(data) and validate(data) methods. Without enforcement, a plugin missing validate() causes an AttributeError at runtime when validate() is called. The team debates ABC vs duck typing.",
    "b": "ABC (Abstract Base Class) with @abstractmethod decorators raises TypeError at class instantiation if any abstract method is not implemented. Duck typing with hasattr() checks at call time raises AttributeError only when the missing method is called, and only if that code path executes.",
    "c": "typing.Protocol (PEP-544) provides structural subtyping — a class implementing the required methods satisfies the Protocol without explicit inheritance. Type checkers (mypy, pyright) validate Protocol conformance at development time."
  },
  "function": "Select interface contract enforcement mechanism for plugin system execution",
  "acceptedOutput": "Use ABC for the plugin base class: class Plugin(ABC): @abstractmethod def process(self, data): pass; @abstractmethod def validate(self, data): pass. TypeError at instantiation catches missing methods before deployment. Type checkers additionally catch non-compliance statically. Use Protocol for type checking without requiring inheritance if plugins are third-party.",
  "rejectedOutput": "Use duck typing. The plugin documentation specifies the required methods. Developers are responsible for implementing them.",
  "arbiters": ["execution-applier-arbiter", "contract-enforcement-arbiter"],
  "whyAccepted": "ABC raises TypeError at plugin class instantiation — the earliest possible detection point. Missing method errors are caught at plugin load time, not at the specific runtime call site that exercises the missing method. Documentation does not enforce contracts; ABCs do.",
  "whyRejected": "Duck typing relies on documentation discipline and runtime error discovery. A plugin missing validate() only fails when a code path calls validate() — which may be an infrequent error path, discovered in production. This converts a preventable instantiation error into a runtime production failure.",
  "sourceAnchors": ["https://docs.python.org/3/library/abc.html"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-594 | PYTHON-PEPS | Family 8 | Domain G

**Unit title:** PEP-634 structural pattern matching — match/case for command dispatch learning
**Source URL:** https://peps.python.org/pep-0634/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT594-F8-G",
  "sourceIds": ["EXT-594"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "command-dispatch-pattern-learning",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python command processor uses a chain of if/elif statements to dispatch on command type: if command['type'] == 'CREATE': ...; elif command['type'] == 'UPDATE': ...; elif command['type'] == 'DELETE': .... Python 3.10+ supports structural pattern matching (match/case).",
    "b": "PEP-634 match/case supports: literal patterns (case 'CREATE':), mapping patterns (case {'type': 'CREATE', 'id': id}:), class patterns (case CreateCommand(id=id):), OR patterns (case 'CREATE' | 'COPY':), and wildcard (case _:). It is compiled to efficient bytecode by CPython.",
    "c": "match/case improves readability for multi-branch dispatch on structure, not just equality. Type checkers understand exhaustiveness of match statements when using dataclass patterns."
  },
  "function": "Record migration from if/elif dispatch to match/case for command processor learning",
  "acceptedOutput": "Refactor command dispatch to match/case: match command: case {'type': 'CREATE', 'id': int(id)}: handle_create(id); case {'type': 'UPDATE', 'id': int(id), 'data': data}: handle_update(id, data); case {'type': 'DELETE', 'id': int(id)}: handle_delete(id); case _: raise ValueError(f\"Unknown command: {command}\"). The mapping pattern extracts and validates key presence in one step.",
  "rejectedOutput": "Keep the if/elif chain. match/case is newer syntax that some team members are unfamiliar with and adds a Python 3.10 version requirement.",
  "arbiters": ["learning-arbiter", "dispatch-pattern-arbiter"],
  "whyAccepted": "Structural pattern matching makes command structure requirements explicit in the pattern itself — {type: 'UPDATE', id: int(id), data: data} documents that UPDATE requires both id (must be int) and data. if/elif with dict.get() is equivalent in logic but less readable for multi-key structural requirements.",
  "whyRejected": "Python 3.10 was released October 2021. In 2026, requiring Python 3.10 is not a practical constraint for active projects. Familiarity concerns are resolved by code review and documentation, not by avoiding modern language features.",
  "sourceAnchors": ["https://peps.python.org/pep-0634/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-595 | OPENTELEMETRY-SPEC | Family 6 | Domain F

**Unit title:** OTel log correlation — trace_id and span_id injection into structured logs for result lookup
**Source URL:** https://opentelemetry.io/docs/specs/otel/logs/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT595-F6-F",
  "sourceIds": ["EXT-595"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "log-trace-correlation-result-storage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core service uses Serilog for structured logging and OTel for distributed tracing. Logs and traces are in separate storage (Elasticsearch + Jaeger). The SRE team cannot correlate a log error with its trace without manual search by timestamp and service name.",
    "b": "OTel log specification defines trace_id and span_id as standard log fields that correlate logs to their originating trace context. OTel .NET SDK provides OpenTelemetry.Logs and an ILogger integration that automatically injects trace_id and span_id from the current Activity into log entries.",
    "c": "Serilog supports enrichers that can read Activity.Current.TraceId and inject into log contexts. OpenTelemetry.Exporter.OpenTelemetryProtocol (OTLP) can export logs to the OTel Collector alongside traces, enabling unified backend storage."
  },
  "function": "Implement trace-log correlation for structured log result storage lookup",
  "acceptedOutput": "Add Serilog enricher for OTel trace context: .Enrich.With(new ActivityEnricher()) which reads Activity.Current.TraceId and Activity.Current.SpanId and adds them as trace_id and span_id log properties. Configure Elasticsearch index to include trace_id as a searchable field. Backend supports trace-to-log navigation via trace_id cross-reference.",
  "rejectedOutput": "Include the correlation ID (request ID) in all logs. Use timestamp ranges to manually find traces matching a log entry.",
  "arbiters": ["registry-storage-arbiter", "observability-correlation-arbiter"],
  "whyAccepted": "trace_id injection enables exact one-to-one log-to-trace lookup. Given a log entry, jump directly to the trace in Jaeger via the trace_id. Given a trace span, find all logs within that span via span_id. No timestamp approximation required.",
  "whyRejected": "Request IDs (correlation IDs) are a request-level identifier, not a span-level one. A single request may fan out to multiple spans across services. Timestamp-based lookup is imprecise under concurrent load and requires knowing the service name and approximate time.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/logs/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-596 | DAPPER | Family 1 | Domain A

**Unit title:** Dapper connection management topology — DbConnection per request vs connection pool sharing
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT596-F1-A",
  "sourceIds": ["EXT-596"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "connection-pool-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core API using Dapper creates a new SqlConnection in each repository method call and disposes it after the query. Under load, the application exhausts the connection pool (default max 100) causing SqlException: Timeout expired waiting for pooled connection.",
    "b": "ADO.NET connection pooling: SqlConnection.Open() retrieves a connection from the pool; Dispose() returns it to the pool. Opening/closing frequently within a single request is fine — the physical connection is pooled. The issue is holding too many logical connections open concurrently, each representing a pool checkout.",
    "c": "Per-request scoping: inject IDbConnection (or SqlConnection) as a Scoped DI registration that opens once per HTTP request and is disposed at request end. Repository classes receive the injected connection, reuse it across multiple queries, and return it to the pool on request completion."
  },
  "function": "Design connection pool topology for high-concurrency Dapper API",
  "acceptedOutput": "Register SqlConnection as Scoped in DI: services.AddScoped<IDbConnection>(_ => new SqlConnection(connStr)). Each request gets one connection, opened lazily on first use and returned to the pool at request scope disposal. Repositories receive IDbConnection via constructor injection and share it across queries in the request.",
  "rejectedOutput": "Increase the connection pool Max Pool Size from 100 to 500 in the connection string. This accommodates more concurrent requests.",
  "arbiters": ["topology-arbiter", "connection-management-arbiter"],
  "whyAccepted": "Scoped connection reuse reduces pool checkouts from N queries/request to 1/request, directly reducing peak pool utilization. At 50 concurrent requests × 5 queries each, pool usage drops from 250 to 50.",
  "whyRejected": "Increasing Max Pool Size addresses the symptom (pool exhaustion) but not the cause (excessive checkouts). A pool of 500 on a database with 150 max_connections would exhaust the database server. Root cause is unnecessary checkout per query, not insufficient pool size.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-597 | ANGULAR-DOCS | Family 3 | Domain D

**Unit title:** Angular 17 application initializer — APP_INITIALIZER vs provideAppInitializer for startup execution
**Source URL:** https://angular.dev/api/core/APP_INITIALIZER
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT597-F3-D",
  "sourceIds": ["EXT-597"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "application-startup-execution",
  "rowClass": "migration-decision",
  "input": {
    "a": "An Angular application loads configuration from an API before bootstrapping (fetching feature flags, tenant config). It uses APP_INITIALIZER with a factory function that returns a Promise. Angular 17 introduced provideAppInitializer().",
    "b": "APP_INITIALIZER is the traditional multi-token approach: { provide: APP_INITIALIZER, useFactory: (service: ConfigService) => () => service.loadConfig(), deps: [ConfigService], multi: true }. provideAppInitializer() (Angular 17+) is a functional wrapper: provideAppInitializer(() => inject(ConfigService).loadConfig()). It uses inject() instead of explicit deps array.",
    "c": "Both block application startup until the returned Promise or Observable resolves. provideAppInitializer() eliminates the factory-of-factory pattern and the deps array."
  },
  "function": "Migrate APP_INITIALIZER to provideAppInitializer for application startup execution",
  "acceptedOutput": "Replace the APP_INITIALIZER token with provideAppInitializer in the providers array: provideAppInitializer(async () => { const configService = inject(ConfigService); await configService.loadConfig(); }). Remove the multi: true and deps boilerplate. The application blocks bootstrapping until loadConfig() resolves.",
  "rejectedOutput": "Keep APP_INITIALIZER. provideAppInitializer() is functionally identical with less boilerplate but the team is already familiar with the token pattern.",
  "arbiters": ["execution-applier-arbiter", "angular-api-migration-arbiter"],
  "whyAccepted": "provideAppInitializer() eliminates the factory-of-factory pattern (useFactory returns a function that returns a Promise) which confuses developers new to the codebase. inject() is cleaner than deps array dependency declaration. Angular 17+ idiomatic pattern.",
  "whyRejected": "Familiarity with older API is not a reason to avoid the cleaner Angular 17 equivalent. The factory-of-factory pattern is a known Angular sharp edge that provideAppInitializer() was specifically designed to eliminate.",
  "sourceAnchors": ["https://angular.dev/api/core/provideAppInitializer"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-598 | REACT-ADVANCED | Family 7 | Domain C

**Unit title:** React internationalization — react-i18next vs FormatJS (react-intl) for i18n translation
**Source URL:** https://react.i18next.com/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT598-F7-C",
  "sourceIds": ["EXT-598"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "i18n-translation-library-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React application needs internationalization for 15 languages including Arabic (RTL, right-to-left), Chinese (no pluralization), and German (complex pluralization). The team evaluates react-i18next vs react-intl (FormatJS).",
    "b": "react-i18next uses ICU message format via i18next-icu plugin, JSON translation files, namespace separation, and lazy loading of language bundles. It has broader ecosystem (backends for remote loading, detector for language auto-detection). react-intl is built on ECMA-402 Intl APIs and provides first-class ICU message format support with type-safe message IDs via @formatjs/cli extraction.",
    "c": "RTL support requires CSS direction handling (separate from translation library). Both libraries handle Arabic pluralization via CLDR (Common Locale Data Repository) plural rules. German Schmetterling plural complexity is handled by ICU plural format in both."
  },
  "function": "Select React i18n library for multi-language translation with complex pluralization",
  "acceptedOutput": "Use react-i18next for applications needing dynamic bundle loading and multi-backend support. Use react-intl for applications needing strict type-safe message extraction (TypeScript integration), ECMA-402 native format compliance, and existing FormatJS toolchain. For a new project with 15 languages: react-i18next with i18next-icu plugin for ICU pluralization and lazy language bundle loading.",
  "rejectedOutput": "Build a custom translation system using React Context and a simple key-value JSON map. i18n libraries are overkill for most translation needs.",
  "arbiters": ["api-translation-arbiter", "i18n-library-selection-arbiter"],
  "whyAccepted": "Production i18n with 15 languages requires: CLDR plural rules (not simple singular/plural), number formatting, date formatting, and RTL layout support. react-i18next provides these with lazy loading to avoid bundling all 15 language files. A custom key-value system would require re-implementing plural rules for each language.",
  "whyRejected": "A custom JSON key-value map handles simple translations but fails at Arabic plural forms (which have 6 plural categories), German compound noun plurals, and number/date formatting. These are solved problems in i18n libraries that should not be re-implemented.",
  "sourceAnchors": ["https://react.i18next.com/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-599 | ASPNETCORE-DOCS-SAMPLES | Family 5 | Domain E

**Unit title:** ASP.NET Core health checks — liveness vs readiness probe arbitration for Kubernetes
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT599-F5-E",
  "sourceIds": ["EXT-599"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "health-probe-arbitration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An ASP.NET Core Kubernetes deployment has a single /health endpoint used for both liveness and readiness probes. The endpoint checks: (1) database connectivity and (2) external payment API availability. When the payment API is down, the liveness probe fails, causing Kubernetes to restart the pod, creating a restart loop.",
    "b": "Kubernetes liveness probe: if it fails, Kubernetes kills and restarts the pod. Readiness probe: if it fails, Kubernetes removes the pod from the Service endpoints (stops sending traffic) but does not restart it. Liveness should check only if the process is healthy enough to restart. Readiness should check if the pod is ready to serve traffic.",
    "c": "Database connectivity failure may justify a restart (if the connection pool is corrupted). Payment API unavailability should remove the pod from traffic rotation (readiness) but not restart the pod (liveness) — the payment API will recover, and a restart does not fix an external dependency."
  },
  "function": "Arbitrate health check endpoints for Kubernetes liveness vs readiness probe separation",
  "acceptedOutput": "Split into two endpoints: /health/live (liveness): checks only process-internal health (memory, thread pool, self-test). /health/ready (readiness): checks database connectivity and external dependencies. Configure Kubernetes livenessProbe to /health/live and readinessProbe to /health/ready. Use HealthCheckOptions with tag filtering: mapHealthChecks(\"/health/live\", new HealthCheckOptions { Predicate = h => h.Tags.Contains(\"live\") }).",
  "rejectedOutput": "Remove external dependency checks from health checks entirely. External dependencies failing should be handled by circuit breakers, not health probes.",
  "arbiters": ["health-probe-arbitration-arbiter", "availability-arbiter"],
  "whyAccepted": "Separate liveness/readiness endpoints prevent external dependency failures from causing restart loops. Readiness failure removes the pod from load balancing, allowing the external dependency to recover without unnecessary restarts. Circuit breakers and readiness probes are complementary, not alternative.",
  "whyRejected": "Removing external dependency checks from readiness leaves the pod accepting traffic it cannot serve (payment API down → failed transactions for all requests routed to this pod). Readiness probes exist precisely to gate traffic based on actual service capability.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-600 | NESTJS-ADVANCED | Family 7 | Domain C

**Unit title:** NestJS OpenAPI (Swagger) decorator-based documentation vs separate API spec file
**Source URL:** https://docs.nestjs.com/openapi/introduction
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT600-F7-C",
  "sourceIds": ["EXT-600"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "api-documentation-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS API team debates: (1) @nestjs/swagger decorators (@ApiProperty, @ApiOperation, @ApiResponse) embedded in DTOs and controllers vs (2) a separate openapi.yaml file maintained independently.",
    "b": "@nestjs/swagger generates OpenAPI spec from decorators at runtime. Decorators keep documentation co-located with code — when a DTO changes, the developer updates @ApiProperty in the same file. A separate openapi.yaml can drift from implementation (spec-first development requires discipline to keep in sync).",
    "c": "Spec-first development defines the API contract in openapi.yaml before implementing, generating server stubs and client SDKs from the spec. Decorator-based is code-first — implementation drives the spec. Code-first is simpler for existing APIs; spec-first is better for API-first organizations with separate API design teams.",
    "trainingStatus": "draft"
  },
  "function": "Translate API documentation strategy decision for NestJS project",
  "acceptedOutput": "Use @nestjs/swagger decorators for existing NestJS projects. Co-location of documentation with implementation reduces documentation drift. Use @ApiProperty({ description, example, required }) on DTOs and @ApiOperation, @ApiResponse on controllers. Export the generated spec (DocumentBuilder → createDocument → yaml dump) for client SDK generation.",
  "rejectedOutput": "Maintain a separate openapi.yaml. NestJS decorator-based documentation pollutes controller code with documentation concerns.",
  "arbiters": ["api-translation-arbiter", "documentation-strategy-arbiter"],
  "whyAccepted": "Decorator co-location reduces documentation drift — changes to a DTO field naturally prompt updating the adjacent @ApiProperty. A separate openapi.yaml has no enforcement mechanism to stay synchronized with the implementation.",
  "whyRejected": "Documentation 'pollution' is subjective. @ApiProperty on a DTO field is information about the field — it belongs alongside the field definition. Separating documentation into a separate file trades co-location clarity for organizational convenience that has no enforcement mechanism.",
  "sourceAnchors": ["https://docs.nestjs.com/openapi/introduction"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-601 | IETF-RFCS | Family 3 | Domain D

**Unit title:** RFC-9110 HTTP semantics — idempotency of PUT vs PATCH for resource update execution
**Source URL:** https://www.rfc-editor.org/rfc/rfc9110
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT601-F3-D",
  "sourceIds": ["EXT-601"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "http-method-semantics-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API endpoint updates user profiles. The team uses PATCH /users/{id} but sends the complete user resource in every request. A code review suggests this should be PUT since the full resource is always sent.",
    "b": "RFC-9110 Section 9.3.4: PUT replaces the entire resource. A PUT request with a partial resource replaces the stored resource with the partial one, nullifying omitted fields. RFC-9110 Section 9.3.7: PATCH applies a partial modification to the resource.",
    "c": "If the client always sends the complete resource and intends to replace all fields, PUT is semantically correct. If the client sends only fields to update (others unchanged), PATCH is correct. The idempotency property: PUT is idempotent (same request produces same state); PATCH may or may not be idempotent depending on implementation."
  },
  "function": "Select HTTP method for resource update based on full vs partial replacement semantics",
  "acceptedOutput": "If the client always sends the complete user resource intending full replacement: use PUT /users/{id}. Server replaces all fields. Response: 200 with updated resource. If the client sends only changed fields: use PATCH /users/{id} with JSON Merge Patch (RFC-7396) or JSON Patch (RFC-6902). Select based on whether the operation is full-replacement or partial-modification, not based on which fields are sent in practice.",
  "rejectedOutput": "Always use PATCH for all update operations. PATCH is more flexible and encompasses PUT semantics as a special case.",
  "arbiters": ["execution-applier-arbiter", "http-semantics-arbiter"],
  "whyAccepted": "RFC-9110 semantics are not about current payload content — they define what the server should do. PUT means 'replace the entire resource with this representation.' If that is the intent, PUT is correct. Using PATCH when PUT is meant creates ambiguity about whether omitted fields are retained or cleared.",
  "whyRejected": "PATCH does not encompass PUT semantics. A PATCH implementation that replaces all fields is functionally equivalent to PUT but semantically incorrect. API clients relying on RFC-9110 semantics may assume PATCH retains omitted fields.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc9110#section-9.3.4"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-602 | GOOGLE-AIP | Family 8 | Domain G

**Unit title:** AIP-158 resource lifecycle — soft delete vs hard delete pattern for reprocessing decisions
**Source URL:** https://google.aip.dev/158
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT602-F8-G",
  "sourceIds": ["EXT-602"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "resource-lifecycle-reprocessing",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API's DeleteUser endpoint permanently deletes user records. Support teams need a 30-day window to restore accidentally deleted users. The team evaluates soft delete (mark deleted, retain data) vs hard delete with backup restore.",
    "b": "AIP-158 specifies a soft-delete pattern: DeleteRequest sets delete_time on the resource but retains it. The resource is excluded from List results by default. UndeleteUser can restore within a retention window. After the retention period, a purge background job performs hard delete.",
    "c": "Soft delete requires: filtering deleted records from all List/Get queries (global query filter), storage cost for retained records, a purge job, and an Undelete API. Hard delete with backup restore requires: backup infrastructure, restore procedures (potentially hours), and no self-service recovery."
  },
  "function": "Record soft delete vs hard delete lifecycle decision for reprocessing capability",
  "acceptedOutput": "Implement AIP-158 soft delete: add delete_time nullable timestamp to the User entity. Set delete_time = now() on DeleteUser. Apply global EF Core query filter: modelBuilder.Entity<User>().HasQueryFilter(u => u.DeleteTime == null). Implement UndeleteUser that clears delete_time. Schedule a background purge job after 30 days.",
  "rejectedOutput": "Implement hard delete with point-in-time database backup restore. Support teams can restore individual users via DBA-assisted backup queries.",
  "arbiters": ["learning-arbiter", "lifecycle-reprocessing-arbiter"],
  "whyAccepted": "Soft delete provides self-service recovery within the retention window without DBA involvement or backup infrastructure. The global query filter ensures deleted records are invisible to normal operations. The 30-day window meets the support team's recovery requirement.",
  "whyRejected": "Backup restore requires DBA involvement, has hour-level recovery time, and may not support restoring individual records without full database restore or complex partial restore procedures. It cannot meet a self-service 30-day recovery SLA.",
  "sourceAnchors": ["https://google.aip.dev/158"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-603 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF dataset contamination — deduplication of training and evaluation splits
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT603-F8-G",
  "sourceIds": ["EXT-603"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "training-data-contamination-prevention",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An RLHF training pipeline splits oasst1 conversation trees into train (80%) and eval (20%) splits by random sampling. Post-training evaluation shows inflated scores — the model memorizes training examples that appear in the eval set due to duplicate conversation branches.",
    "b": "oasst1 contains conversation trees where multiple annotators respond to the same prompt. Random sampling can place the same prompt in both train and eval — different response branches for the same prompt contaminate evaluation. Contamination causes memorization-based eval inflation, not generalization-based performance.",
    "c": "Correct deduplication strategy: split at the conversation root (message_tree_id) level. All branches of a conversation tree go to either train or eval, never split across both. This ensures eval prompts were never seen during training."
  },
  "function": "Detect and fix RLHF dataset contamination via root-level conversation splitting",
  "acceptedOutput": "Split oasst1 by message_tree_id (root conversation ID) rather than by individual messages: train_trees, eval_trees = train_test_split(all_tree_ids, test_size=0.2, random_state=42). All message pairs from train_trees go to training; all from eval_trees go to evaluation. No prompt appears in both splits.",
  "rejectedOutput": "Increase eval set size to 30% to statistically dilute contamination effects.",
  "arbiters": ["learning-quality-arbiter", "contamination-prevention-arbiter"],
  "whyAccepted": "Root-level splitting guarantees zero prompt contamination. The evaluation set contains only conversation trees whose root prompts were never seen during training. This is the correct evaluation methodology for conversational RLHF datasets with tree structure.",
  "whyRejected": "Larger eval set dilutes but does not eliminate contamination. Contaminated examples still provide memorization-based signal that inflates eval scores, just with lower weight. The proper fix is structural deduplication, not statistical dilution.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-604 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba low-resource language pair augmentation — back-translation quality gate
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT604-F2-C",
  "sourceIds": ["EXT-604"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "low-resource-translation-augmentation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A translation model for a low-resource language pair (EN-SW, English to Swahili) has only 50,000 Tatoeba sentence pairs. The team uses back-translation (BT) to augment: translate a large EN monolingual corpus to SW using the forward model, then add (synthetic_SW, original_EN) pairs to training.",
    "b": "Back-translation quality degrades as the forward model improves — early BT iterations use a weak model producing noisy synthetic target sentences. COMET or BLEU filtering of BT pairs by quality score removes the noisiest synthetic pairs. The threshold trades augmentation volume against noise.",
    "c": "Research (Edunov et al. 2018) shows that noisy BT (with beam search output) can be beneficial if noise is added intentionally (sampling instead of beam search). However, systematic translation errors (wrong grammar, hallucinated entities) degrade training."
  },
  "function": "Apply back-translation quality gate for low-resource language pair augmentation",
  "acceptedOutput": "Generate BT pairs using sampling (temperature > 1.0) rather than beam search for diversity. Filter synthetic pairs with COMET-QE (quality estimation without reference) at threshold 0.60 — retaining higher-confidence translations. Use ratio 4:1 (BT:real) as an upper bound. Re-evaluate after each training iteration and regenerate BT with the improved model.",
  "rejectedOutput": "Use all BT pairs without quality filtering. More data always improves low-resource models, even noisy data.",
  "arbiters": ["translation-quality-arbiter", "augmentation-strategy-arbiter"],
  "whyAccepted": "COMET-QE filtering removes the worst BT pairs (hallucinations, systematic grammar errors) that degrade training more than they help. Sampling-based BT maintains diversity for the better-quality pairs retained after filtering.",
  "whyRejected": "Unfiltered BT includes hallucinated entities and systematic errors that teach the model incorrect patterns. For low-resource pairs with small real data, noisy BT can dominate the training signal, causing the model to regress on real test data.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-605 | VUE-DOCS | Family 6 | Domain F

**Unit title:** Vue 3 useAsyncState vs manual ref + watchEffect for async data result storage
**Source URL:** https://vueuse.org/core/useAsyncState/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT605-F6-F",
  "sourceIds": ["EXT-605"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "async-data-result-storage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 component fetches user data on mount: const user = ref(null); const loading = ref(false); const error = ref(null); onMounted(async () => { loading.value = true; try { user.value = await fetchUser(id); } catch(e) { error.value = e; } finally { loading.value = false; }}). The team finds this boilerplate repeated across many components.",
    "b": "VueUse useAsyncState(fetchUser(id), null, { immediate: true }) returns { state, isLoading, error, execute } — the same pattern abstracted into a composable. It handles the loading/error state machine, immediate execution, and re-execution.",
    "c": "useAsyncState is from VueUse (community utility library). For teams without VueUse, extracting a custom useAsync<T>() composable achieves the same result."
  },
  "function": "Select async data result storage pattern for Vue 3 composable",
  "acceptedOutput": "Use useAsyncState from VueUse or extract a custom composable: function useAsync<T>(asyncFn: () => Promise<T>, initialValue: T) { const state = ref<T>(initialValue); const isLoading = ref(false); const error = ref<Error | null>(null); const execute = async () => { isLoading.value = true; try { state.value = await asyncFn(); } catch(e) { error.value = e as Error; } finally { isLoading.value = false; }}; execute(); return { state, isLoading, error, execute }; }.",
  "rejectedOutput": "Keep the per-component pattern. Extracting a composable is premature abstraction when the pattern is only used in a few components.",
  "arbiters": ["registry-storage-arbiter", "composable-abstraction-arbiter"],
  "whyAccepted": "The loading/error state machine for async data is identical across all data-fetching components. Abstracting it once eliminates bugs from forgetting finally { loading.value = false } or error handling. The pattern is not 'premature' — it is the same code appearing in every component that fetches data.",
  "whyRejected": "When the same pattern appears in 5+ components, it is not premature abstraction — it is deduplication of proven logic. Per-component implementation diverges over time (different error handling, missing finally blocks) creating inconsistent loading/error behavior across the UI.",
  "sourceAnchors": ["https://vueuse.org/core/useAsyncState/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-606 | EFCORE-SOURCE | Family 5 | Domain E

**Unit title:** EF Core concurrency tokens — RowVersion vs xmin for optimistic concurrency arbitration
**Source URL:** https://learn.microsoft.com/en-us/ef/core/saving/concurrency
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT606-F5-E",
  "sourceIds": ["EXT-606"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "optimistic-concurrency-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An EF Core application using SQL Server needs optimistic concurrency: when two users update the same entity simultaneously, one should succeed and the other receive a conflict error. The team evaluates [Timestamp] (RowVersion) vs a manual application-managed version field.",
    "b": "SQL Server RowVersion ([Timestamp] attribute in EF Core) is a database-managed binary(8) column automatically incremented on every row update. EF Core includes it in UPDATE WHERE clauses (WHERE RowVersion = @original) and throws DbUpdateConcurrencyException if 0 rows are affected.",
    "c": "A manual version int field requires the application to increment it and include it in the WHERE clause — error-prone if any code path updates the entity without incrementing the version. RowVersion is database-managed and cannot be forgotten."
  },
  "function": "Arbitrate optimistic concurrency token for EF Core SQL Server application",
  "acceptedOutput": "Use [Timestamp] attribute (RowVersion) on a byte[] property: [Timestamp] public byte[] RowVersion { get; set; }. EF Core automatically includes RowVersion in UPDATE WHERE clauses and throws DbUpdateConcurrencyException on conflict. Handle the exception: catch (DbUpdateConcurrencyException ex) { // refresh entry, show conflict to user }.",
  "rejectedOutput": "Add a manual int Version property, increment it in each SaveChanges call using a custom SaveChanges override, and include it in a ConcurrencyCheck attribute.",
  "arbiters": ["registry-storage-arbiter", "concurrency-token-arbiter"],
  "whyAccepted": "RowVersion is database-managed — it cannot be forgotten, bypassed by direct SQL, or incorrectly incremented. EF Core's first-class support generates correct WHERE clauses automatically. DbUpdateConcurrencyException is thrown reliably when rows are affected by a concurrent update.",
  "whyRejected": "Manual version fields require application-level discipline. Direct SQL updates, bulk operations, or code paths that bypass the SaveChanges override leave the version stale. A stale version means concurrency conflicts go undetected — the wrong assumption of 'no conflict' persists.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/saving/concurrency"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-607 | DOTNET-MICROSERVICES | Family 6 | Domain F

**Unit title:** .NET MassTransit Saga vs manual process manager for distributed transaction result storage
**Source URL:** https://masstransit.io/documentation/patterns/saga
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT607-F6-F",
  "sourceIds": ["EXT-607"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "distributed-transaction-result-storage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An e-commerce order process involves 4 services: Order (reserve stock), Payment (charge card), Warehouse (pick items), and Shipping (dispatch). If Payment fails, Order must release stock. If Warehouse fails after Payment succeeds, Payment must be refunded. The team needs a distributed saga.",
    "b": "MassTransit SagaStateMachine defines states (Submitted, StockReserved, Paid, Picking, Dispatched, Cancelled) and transitions triggered by events. Compensating actions (ReleaseStock, RefundPayment) are defined for failure transitions. State is persisted to a database (Entity Framework saga repository).",
    "c": "A manual process manager is a service that subscribes to events and maintains its own state table. It requires implementing state machine logic, persistence, and compensating action dispatch manually — reimplementing what MassTransit provides."
  },
  "function": "Select distributed saga orchestration for multi-service order process result storage",
  "acceptedOutput": "Use MassTransit SagaStateMachine with Entity Framework saga repository. Define states and events matching the order lifecycle. Define compensating transitions: When(PaymentFailed).TransitionTo(Cancelled).Then(ctx => ctx.Publish(new ReleaseStockCommand(...))). MassTransit handles: idempotent message delivery, saga state persistence, retry on failure, and dead-letter on exhausted retries.",
  "rejectedOutput": "Implement a manual process manager service with a state table. MassTransit adds complexity and dependency overhead for a four-service workflow.",
  "arbiters": ["registry-storage-arbiter", "saga-orchestration-arbiter"],
  "whyAccepted": "MassTransit SagaStateMachine provides: idempotent state transitions (duplicate messages are safe), persistent state between service restarts, built-in retry and dead-letter, and visual state machine diagrams. These are solved problems that manual implementation consistently gets wrong (non-idempotent compensations, lost state on restart).",
  "whyRejected": "A manual process manager for a 4-service saga with compensating actions is a complex distributed systems problem. Idempotency, exactly-once state transitions, failure recovery, and compensation coordination require the same infrastructure MassTransit provides. Reimplementing it adds risk without benefit.",
  "sourceAnchors": ["https://masstransit.io/documentation/patterns/saga"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-608 | MICROSOFT-LEARN-ARCH | Family 3 | Domain D

**Unit title:** Azure Blob Storage tiering — Hot vs Cool vs Archive for cost-optimized data lifecycle execution
**Source URL:** https://learn.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT608-F3-D",
  "sourceIds": ["EXT-608"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "data-lifecycle-tier-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An application stores user-uploaded documents in Azure Blob Storage Hot tier. After 30 days, documents are rarely accessed. After 1 year, they are accessed for compliance audits only. The team needs cost-optimized tiering.",
    "b": "Azure Blob Storage tiers: Hot (highest storage cost, lowest access cost), Cool (lower storage cost, higher access cost, 30-day minimum retention), Cold (even lower storage, higher access, 90-day minimum), Archive (lowest storage cost, highest retrieval cost, hours to rehydrate, 180-day minimum retention).",
    "c": "Lifecycle management policies automate tier transitions based on last modified date. Rehydration from Archive to Hot/Cool takes 1-15 hours. Archive is suitable for compliance data accessed only on audit events (annual or less frequent)."
  },
  "function": "Apply Azure Blob Storage tiering lifecycle for cost-optimized data retention execution",
  "acceptedOutput": "Configure a lifecycle management policy: 0-30 days → Hot tier; 30-90 days → Cool tier (document is occasionally accessed in the 30-90 day window); 90-365 days → Cold tier; 365+ days → Archive tier. Set early deletion penalty awareness: avoid moving to Cool before 30 days, Cold before 90 days, Archive before 180 days.",
  "rejectedOutput": "Keep all documents in Hot tier. The storage cost difference is minimal compared to development effort to configure lifecycle policies.",
  "arbiters": ["execution-applier-arbiter", "cost-optimization-arbiter"],
  "whyAccepted": "Hot tier storage costs ~$0.018/GB/month vs Archive ~$0.00099/GB/month — an 18× cost difference. For large document stores, lifecycle tiering provides substantial savings. Azure lifecycle management policies are configuration, not code — the development effort is 1-2 hours, not weeks.",
  "whyRejected": "'Minimal cost difference' is incorrect at scale. 10TB of documents: Hot = $180/month, Archive = $9.90/month = $170/month savings. Lifecycle policy configuration is not significant development effort — it is a JSON/YAML configuration in the storage account settings.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/storage/blobs/access-tiers-overview"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-609 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-306 missing authentication for critical function — SignalR hub authorization arbitration
**Source URL:** https://cwe.mitre.org/data/definitions/306.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT609-F5-E",
  "sourceIds": ["EXT-609"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "missing-auth-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core SignalR hub allows any client to invoke SendMessage() and ReceiveOrderUpdates() without authentication. The team assumes the hub is only accessible from the authenticated frontend, but a security test shows the hub WebSocket endpoint is directly accessible via URL.",
    "b": "SignalR hubs require explicit authorization, not implicit protection via frontend routing. [Authorize] attribute on the Hub class requires an authenticated user for all hub connections. Individual hub methods can have [Authorize(Roles = \"Admin\")] for role-based access.",
    "c": "CWE-306 (Missing Authentication for Critical Function) applies when a function that modifies state or exposes sensitive data lacks authentication. SignalR hub methods that broadcast to all connected clients or access user-specific data are critical functions."
  },
  "function": "Arbitrate SignalR hub authentication to remediate CWE-306 missing authentication",
  "acceptedOutput": "Add [Authorize] to the hub class: [Authorize] public class OrderHub : Hub. Configure the SignalR endpoint to use authentication: app.MapHub<OrderHub>(\"/orderhub\").RequireAuthorization(). For role-specific methods: [Authorize(Roles = \"Admin\")] on the hub method. Unauthenticated connection attempts receive 401 before the WebSocket upgrade completes.",
  "rejectedOutput": "Add client-side JavaScript checks before allowing users to connect to the hub. The backend hub trusts that only authenticated frontend users will connect.",
  "arbiters": ["security-arbiter", "authentication-gate-arbiter"],
  "whyAccepted": "Server-side authorization on the hub class enforces authentication at the WebSocket protocol level. Any client attempting to connect receives 401 without application code executing. Defense-in-depth: server-side gates cannot be bypassed by client-side script removal or direct WebSocket connections.",
  "whyRejected": "Client-side JavaScript checks are enforced only by the browser. Any client can bypass them by connecting directly to the WebSocket endpoint. Server trust of client-enforced checks is the definition of CWE-306.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/306.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-610 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V13.2 RESTful web service security — mass assignment prevention in ASP.NET Core
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT610-F5-E",
  "sourceIds": ["EXT-610"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "mass-assignment-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core API binds the incoming request body directly to the User domain entity: [HttpPost] public async Task<IActionResult> UpdateUser([FromBody] User user). A user submits { \"id\": 1, \"email\": \"x@x.com\", \"isAdmin\": true } and gains admin privileges.",
    "b": "Mass assignment occurs when user-supplied data binds directly to domain objects with sensitive properties. OWASP ASVS V13.2.5 requires that servers only accept properties that are intended to be changeable by the user.",
    "c": "Solutions: (1) use separate DTO (Data Transfer Object) classes for request binding with only user-editable fields; (2) use [Bind(Include)] attribute listing allowed properties; (3) use model validation that rejects unexpected properties. DTOs are the most robust solution."
  },
  "function": "Arbitrate mass assignment prevention for RESTful API request binding",
  "acceptedOutput": "Define a separate UpdateUserRequest DTO: class UpdateUserRequest { public string Email { get; set; } public string DisplayName { get; set; } } — containing only user-editable properties. Bind the endpoint to the DTO: [HttpPost] public Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request). Map DTO to domain entity explicitly, ignoring IsAdmin and other server-managed properties.",
  "rejectedOutput": "Use [Bind(\"Email,DisplayName\")] on the User domain entity parameter to whitelist bindable properties.",
  "arbiters": ["security-arbiter", "mass-assignment-arbiter"],
  "whyAccepted": "Separate DTOs make the allowed input surface explicit in the type definition. The compiler enforces that unmapped properties (IsAdmin) cannot be set via the DTO. Any new sensitive property added to User is not bindable until explicitly added to the DTO.",
  "whyRejected": "[Bind] attribute strings are stringly-typed — a rename of the property does not update the Bind attribute string, silently re-enabling binding of the property with the old name. DTO separation provides compile-time safety that attribute strings cannot.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
