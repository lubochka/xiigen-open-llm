# External Sources Batch 34 — EXT-671 through EXT-690

---

## EXT-671 | KUBERNETES-KEPS | Family 6 | Domain F

**Unit title:** Kubernetes PersistentVolume reclaim policy — Retain vs Delete for stateful workload data storage
**Source URL:** https://kubernetes.io/docs/concepts/storage/persistent-volumes/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT671-F6-F",
  "sourceIds": ["EXT-671"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "persistent-volume-data-retention",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A StatefulSet runs a PostgreSQL database with PersistentVolumeClaims. A developer accidentally deletes the StatefulSet. The PVCs are deleted automatically, and the PersistentVolumes (backed by cloud disks) are also deleted because the StorageClass has reclaimPolicy: Delete.",
    "b": "PersistentVolume reclaim policy: Delete removes the underlying cloud storage (EBS, Azure Disk, GCE PD) when the PVC is deleted. Retain keeps the cloud storage — the PV enters Released state and the data persists. Manual re-binding is required to reuse a Retained PV.",
    "c": "For production databases: Retain prevents accidental data loss when PVCs are deleted (by kubectl delete, helm uninstall, or namespace deletion). For ephemeral workloads (CI test databases, development environments): Delete simplifies cleanup."
  },
  "function": "Select PersistentVolume reclaim policy for production database data storage",
  "acceptedOutput": "Set reclaimPolicy: Retain in the StorageClass for production database workloads. When a PVC is deleted, the PV and its cloud disk are preserved. Implement a separate cleanup process (backup verification + manual PV deletion) for decommissioned databases. Use reclaimPolicy: Delete only for non-critical ephemeral workloads.",
  "rejectedOutput": "Use reclaimPolicy: Delete for all workloads. Kubernetes cluster operators manage backup and recovery separately from PV lifecycle.",
  "arbiters": ["registry-storage-arbiter", "data-retention-arbiter"],
  "whyAccepted": "Retain provides a safety net against accidental PVC deletion cascading to data loss. The cloud disk persists and can be re-attached to a recovery pod. For production databases, accidental deletion recovery is a critical operational capability.",
  "whyRejected": "Delete policy provides no recovery window — the cloud disk is destroyed as soon as the PVC is deleted. A separate backup strategy does not prevent the 0-RPO (Recovery Point Objective) data loss between the last backup and the deletion event.",
  "sourceAnchors": ["https://kubernetes.io/docs/concepts/storage/persistent-volumes/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-672 | BIGCODEBENCH | Family 3 | Domain D

**Unit title:** Python context manager protocol — __enter__/__exit__ vs contextlib.contextmanager for resource execution
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT672-F3-D",
  "sourceIds": ["EXT-672"],
  "sourceTrustTier": 2,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "context-manager-implementation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team implements a database connection context manager as a class with __enter__ and __exit__ methods. A code reviewer suggests using @contextlib.contextmanager decorator on a generator function instead.",
    "b": "@contextlib.contextmanager converts a generator function into a context manager: code before yield runs as __enter__, code after yield runs as __exit__. Class-based context managers are appropriate when: the context manager needs to be reentrant, needs to hold state across multiple uses, or implements __aenter__/__aexit__ for async.",
    "c": "contextmanager exception handling: use try/finally around the yield to ensure cleanup even if an exception occurs in the with block. The generator approach is more concise for simple resource management."
  },
  "function": "Select context manager implementation pattern for resource lifecycle execution",
  "acceptedOutput": "Use @contextlib.contextmanager for simple resource management: @contextmanager def db_connection(url): conn = create_connection(url); try: yield conn; finally: conn.close(). Class-based __enter__/__exit__ is preferred for: async context managers (__aenter__/__aexit__), reentrant context managers, or when the manager needs to be picklable or inspectable.",
  "rejectedOutput": "Always use class-based context managers. The __enter__/__exit__ protocol is explicit and always correct; @contextmanager is syntactic sugar that obscures the protocol.",
  "arbiters": ["execution-applier-arbiter", "context-manager-selection-arbiter"],
  "whyAccepted": "@contextlib.contextmanager reduces boilerplate significantly for simple resource patterns. The generator-with-yield is more readable than a class with two methods when there is no state beyond the resource itself. For async, class-based __aenter__/__aexit__ is required.",
  "whyRejected": "'Always use class-based' ignores that @contextmanager is the standard library's own recommendation for simple context managers. It is not obscured — it is the documented Python pattern. Complexity should be introduced only when the simpler form is insufficient.",
  "sourceAnchors": ["https://docs.python.org/3/library/contextlib.html#contextlib.contextmanager"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-673 | SWE-BENCH | Family 5 | Domain E

**Unit title:** Python input validation — Pydantic validators vs manual isinstance checks for API security
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT673-F5-E",
  "sourceIds": ["EXT-673"],
  "sourceTrustTier": 2,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "input-validation-security-arbitration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A FastAPI endpoint receives a JSON body and validates it manually: if not isinstance(data.get('email'), str) or '@' not in data.get('email', ''): raise HTTPException(400). Multiple validation rules scattered across service methods.",
    "b": "Pydantic models (FastAPI's native validation) provide declarative validation: class CreateUserRequest(BaseModel): email: EmailStr; age: int = Field(ge=0, le=150); name: str = Field(min_length=1, max_length=100). FastAPI automatically validates the request body against the Pydantic model before the handler runs, returning 422 Unprocessable Entity with field-level error details.",
    "c": "class-validator (Pydantic v2 @field_validator) enables custom validation logic beyond built-in constraints. @model_validator enables cross-field validation (e.g., end_date > start_date)."
  },
  "function": "Arbitrate input validation approach for FastAPI API security",
  "acceptedOutput": "Define Pydantic models for all API request bodies. Use built-in types (EmailStr, AnyUrl, constr, conint) and Field() constraints (ge, le, min_length, regex). FastAPI validates automatically before the handler runs — invalid requests never reach business logic. Use @field_validator for custom rules.",
  "rejectedOutput": "Keep manual isinstance checks. Pydantic adds a dependency and the validation logic is clearer when written explicitly.",
  "arbiters": ["security-arbiter", "input-validation-arbitration-arbiter"],
  "whyAccepted": "Pydantic validation runs at the framework boundary — before handler code. Manual validation scattered in service methods can be bypassed by direct service calls in tests or other callers. Pydantic provides structured 422 error responses with per-field details, improving API usability.",
  "whyRejected": "Pydantic is already a FastAPI dependency — it is not an additional dependency. Manual isinstance checks are verbose, inconsistent across endpoints, and produce unstructured error responses. Security validation belongs at the API boundary (Pydantic), not scattered in business logic.",
  "sourceAnchors": ["https://docs.pydantic.dev/latest/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-674 | PYTHON-PEPS | Family 8 | Domain G

**Unit title:** PEP-695 type parameter syntax — generic class/function definition for type-safe library learning
**Source URL:** https://peps.python.org/pep-0695/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT674-F8-G",
  "sourceIds": ["EXT-674"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "generic-type-syntax-learning",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python library uses the pre-3.12 generic syntax: T = TypeVar('T'); class Repository(Generic[T]): def get(self, id: int) -> T: .... Python 3.12 introduced PEP-695 type parameter syntax.",
    "b": "PEP-695 (Python 3.12+) introduces [T] syntax for generic definitions: class Repository[T]: def get(self, id: int) -> T: .... For functions: def first[T](items: list[T]) -> T. TypeVar() is no longer needed for most cases. TypeVarTuple and ParamSpec also have new syntax: [*Ts] and [**P].",
    "c": "PEP-695 type parameters are scoped to the class/function — no global TypeVar pollution. type aliases also use the new syntax: type Vector[T] = list[tuple[T, T]]."
  },
  "function": "Record PEP-695 generic type syntax migration for type-safe library learning",
  "acceptedOutput": "For Python 3.12+ libraries: migrate to PEP-695 syntax: class Repository[T]: def get(self, id: int) -> T: .... Remove standalone TypeVar declarations. Use type aliases: type Result[T] = T | Error. The new syntax is more readable, eliminates TypeVar boilerplate, and scopes type parameters to their definition.",
  "rejectedOutput": "Keep the pre-3.12 TypeVar syntax for broader Python version compatibility.",
  "arbiters": ["learning-arbiter", "generic-type-migration-arbiter"],
  "whyAccepted": "PEP-695 syntax reduces generic class boilerplate significantly. For libraries targeting Python 3.12+ (as of 2026, Python 3.12 is the minimum supported version in many organizations), the new syntax is the canonical form. from __future__ import annotations does not backport PEP-695 syntax to older Python.",
  "whyRejected": "If the library must support Python 3.11, the old TypeVar syntax is required. For Python 3.12+-only libraries, the compatibility argument does not apply. The migration decision should be gated on actual minimum supported Python version, not blanket caution.",
  "sourceAnchors": ["https://peps.python.org/pep-0695/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-675 | OPENTELEMETRY-SPEC | Family 5 | Domain E

**Unit title:** OTel alert correlation — span status ERROR vs HTTP 4xx/5xx for alert arbitration
**Source URL:** https://opentelemetry.io/docs/specs/otel/trace/api/#set-status
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT675-F5-E",
  "sourceIds": ["EXT-675"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "error-alert-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An alerting rule fires when http.response.status_code >= 500 in spans. The alert fires for all HTTP 500 errors including validation errors that were incorrectly returning 500 (should be 400). The SRE team wants to alert on genuine server errors only.",
    "b": "OTel span status (Unset, Ok, Error) is orthogonal to HTTP status codes. OTel semantic conventions recommend setting span status to Error for HTTP 5xx server errors and leaving it Unset for 4xx client errors (which are not server errors). Alerting on span.status = ERROR is more precise than HTTP 5xx because the developer controls when to mark a span as Error.",
    "c": "span.SetStatus(StatusCode.Error, 'Database connection failed') is set by the application code. HTTP auto-instrumentation sets it based on status code convention. Custom business logic errors (payment declined) may not warrant span Error status."
  },
  "function": "Arbitrate OTel span status vs HTTP status code for error alerting",
  "acceptedOutput": "Alert on span.status = ERROR (OTel status code) rather than HTTP 5xx. Fix the validation endpoints to return HTTP 400 (client error) instead of 500. Set span status to Error only for genuine server failures: connection pool exhaustion, downstream service unavailability, unhandled exceptions. HTTP 4xx business rule rejections should not set span Error status.",
  "rejectedOutput": "Keep alerting on HTTP 5xx and fix the individual endpoints returning 500 for validation errors. The alert rule is correct; the endpoints are wrong.",
  "arbiters": ["security-arbiter", "error-alert-arbitration-arbiter"],
  "whyAccepted": "Both fixes are needed: correct the 500→400 HTTP status codes AND switch to span.status = ERROR alerting. Span status provides semantic clarity — the developer deliberately marks a span as Error for conditions worth alerting on, independent of HTTP protocol details.",
  "whyRejected": "Fixing only the HTTP status codes leaves the alert rule dependent on HTTP status interpretation. Future endpoints returning 500 for non-error conditions will re-trigger false alerts. Span status ERROR as the alert signal provides a semantically correct, developer-controlled boundary.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/trace/api/#set-status"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-676 | DAPPER | Family 5 | Domain E

**Unit title:** Dapper SQL injection prevention — parameterized queries vs string format arbitration
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT676-F5-E",
  "sourceIds": ["EXT-676"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "sql-injection-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "A Dapper query builds SQL with string interpolation: var sql = $\"SELECT * FROM Orders WHERE CustomerId = {customerId} AND Status = '{status}'\"; connection.Query<Order>(sql). A security scan flags this as SQL injection vulnerability.",
    "b": "Dapper parameterized queries: connection.Query<Order>(\"SELECT * FROM Orders WHERE CustomerId = @customerId AND Status = @status\", new { customerId, status }). Parameters are sent separately from the SQL text — the database server cannot interpret them as SQL syntax regardless of their content.",
    "c": "Dapper does not perform any SQL string building or parameterization of interpolated strings — it passes the string directly to ADO.NET. Only the @parameter syntax with an anonymous object creates actual SQL parameters."
  },
  "function": "Arbitrate Dapper query construction to prevent SQL injection",
  "acceptedOutput": "Use named parameters with anonymous objects: connection.Query<Order>(\"SELECT * FROM Orders WHERE CustomerId = @customerId AND Status = @status\", new { customerId, status }). For dynamic column lists or ORDER BY: use a whitelist to map column names: var safeColumn = allowedColumns.Contains(sortColumn) ? sortColumn : \"CreatedAt\"; — never interpolate unsanitized column names.",
  "rejectedOutput": "Sanitize string inputs before interpolation: customerId.ToString().Replace(\"'\", \"''\") and status.Replace(\"'\", \"''\") to escape single quotes.",
  "arbiters": ["security-arbiter", "sql-injection-prevention-arbiter"],
  "whyAccepted": "Parameterized queries are structurally safe — parameter values are never interpreted as SQL syntax by the database engine, regardless of content. No sanitization is needed or appropriate.",
  "whyRejected": "Quote escaping is a denylist that fails for: numeric injection (no quotes needed for integer fields), database-specific escape sequences, second-order injection (stored then re-executed), and encoding tricks. Parameterization is the only correct defense.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-677 | ANGULAR-DOCS | Family 1 | Domain A

**Unit title:** Angular 17 standalone components — NgModule vs standalone topology migration
**Source URL:** https://angular.dev/guide/components/importing
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT677-F1-A",
  "sourceIds": ["EXT-677"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "angular-module-topology-migration",
  "rowClass": "migration-decision",
  "input": {
    "a": "An Angular 14 application uses NgModules for all feature organization. Angular 17 makes standalone components the default. The team evaluates migrating from NgModule-based to standalone component topology.",
    "b": "Standalone components: @Component({ standalone: true, imports: [CommonModule, RouterModule, MatButtonModule] }) — the component declares its own dependencies. NgModule: components are declared in a module's declarations array; modules are imported by other modules. Standalone eliminates the NgModule indirection layer.",
    "c": "Angular provides a migration schematic: ng generate @angular/core:standalone. It converts: component declarations to standalone, module imports to component-level imports, and bootstrapModule to bootstrapApplication. The migration can be applied incrementally (one module at a time)."
  },
  "function": "Record standalone component topology migration decision for Angular application",
  "acceptedOutput": "Migrate to standalone components using the Angular migration schematic: ng generate @angular/core:standalone --mode=convert-to-standalone. Apply per module to avoid large diffs. New components use standalone: true by default in Angular 17. Standalone simplifies lazy loading (loadComponent instead of loadChildren with a module), improves tree-shaking, and aligns with the Angular team's direction.",
  "rejectedOutput": "Keep NgModule-based architecture. The migration risk outweighs the benefit for a large existing application.",
  "arbiters": ["topology-arbiter", "angular-standalone-migration-arbiter"],
  "whyAccepted": "Standalone is the Angular 17 default and the Angular team's strategic direction. The migration schematic handles the mechanical conversion. Incremental migration (module by module) limits risk. Benefits: better tree-shaking, simpler lazy loading, reduced boilerplate.",
  "whyRejected": "'Migration risk' is mitigated by the official migration schematic and incremental application. Deferring migration accumulates technical debt as Angular's documentation, examples, and new features increasingly target standalone components.",
  "sourceAnchors": ["https://angular.dev/guide/components/importing"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-678 | REACT-ADVANCED | Family 7 | Domain C

**Unit title:** React accessibility (a11y) — semantic HTML vs ARIA roles for screen reader translation
**Source URL:** https://react.dev/learn/accessibility
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT678-F7-C",
  "sourceIds": ["EXT-678"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "accessibility-semantic-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React component renders a clickable div: <div onClick={handleClick} className=\"btn\">Submit</div>. An accessibility audit flags: missing keyboard accessibility (not focusable, no Enter/Space handler), missing role, and missing accessible name for screen readers.",
    "b": "Native HTML button element provides built-in: keyboard focus (Tab), Enter/Space activation, role=button, and accessible name from content. ARIA role=\"button\" on a div requires manually adding tabIndex={0}, onKeyDown handler for Enter/Space, and accessible name. Semantic HTML is always preferred over ARIA role equivalents.",
    "c": "WCAG (Web Content Accessibility Guidelines) 2.1 Rule 1: use native semantics (HTML elements) before ARIA. ARIA roles supplement native semantics; they do not replace them."
  },
  "function": "Translate clickable div to accessible semantic HTML for screen reader compatibility",
  "acceptedOutput": "Replace <div onClick={handleClick}> with <button type=\"button\" onClick={handleClick}>Submit</button>. Native button provides: keyboard focus, Enter/Space activation, role=button, accessible name from content — all for free. Use <button> for all interactive controls that trigger actions. Use <a href> for navigation.",
  "rejectedOutput": "Add role=\"button\" tabIndex={0} onKeyDown={handleKeyDown} aria-label=\"Submit\" to the div. This achieves the same accessibility as a button without changing the DOM element.",
  "arbiters": ["api-translation-arbiter", "accessibility-semantic-arbiter"],
  "whyAccepted": "Native <button> requires zero additional attributes for full accessibility. ARIA role=\"button\" on a div requires 4+ additional attributes and a manually written keyboard handler — more code to achieve the same result with higher risk of gaps (missing touch support, missed key codes).",
  "whyRejected": "ARIA role=\"button\" on a div achieves the same screen reader behavior but requires manually implementing what the browser provides for free with <button>. The ARIA First Aid Kit principle: the first rule of ARIA is to not use ARIA when a native element works.",
  "sourceAnchors": ["https://react.dev/learn/accessibility"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-679 | ASPNETCORE-DOCS-SAMPLES | Family 6 | Domain F

**Unit title:** ASP.NET Core response compression — Brotli vs gzip for API response result storage
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT679-F6-F",
  "sourceIds": ["EXT-679"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "response-compression-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core API serves large JSON payloads (200KB average). The team enables gzip response compression. A performance review suggests adding Brotli compression.",
    "b": "Brotli (br) achieves 15-25% better compression than gzip for text content (JSON, HTML) at equivalent CPU cost. ASP.NET Core supports both via UseResponseCompression() with BrotliCompressionProvider and GzipCompressionProvider. Clients indicate support via Accept-Encoding: br, gzip header. The server responds with Content-Encoding: br or gzip.",
    "c": "Brotli is supported by all modern browsers (Chrome, Firefox, Safari, Edge) and most HTTP clients. HTTPS is required for Brotli in browsers (same-origin policy for compression). API-to-API calls over HTTPS can use Brotli if the client sends Accept-Encoding: br."
  },
  "function": "Select response compression algorithm for API payload result delivery",
  "acceptedOutput": "Enable both Brotli and gzip with quality-based content negotiation: services.AddResponseCompression(opts => { opts.Providers.Add<BrotliCompressionProvider>(); opts.Providers.Add<GzipCompressionProvider>(); opts.EnableForHttps = true; }). Set Brotli level 4 (CompressionLevel.Optimal) for API responses — balance between CPU cost and compression ratio. The server selects Brotli when the client supports it (Accept-Encoding: br).",
  "rejectedOutput": "Use gzip only. Brotli is newer and may not be supported by all API clients. Adding Brotli increases server CPU usage.",
  "arbiters": ["registry-storage-arbiter", "compression-selection-arbiter"],
  "whyAccepted": "Brotli's 15-25% better compression reduces bandwidth and response time for large JSON payloads. For a 200KB payload, Brotli vs gzip saves ~30-50KB per response. At high request volume, this translates to meaningful bandwidth savings. Brotli CPU cost is comparable to gzip at the same compression level.",
  "whyRejected": "Brotli has universal modern client support. ASP.NET Core's content negotiation falls back to gzip for clients that don't support Brotli (Accept-Encoding: gzip only). The CPU cost claim is not specific — at level 4 (default for ASP.NET Core), Brotli and gzip have similar throughput.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/performance/response-compression"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-680 | NESTJS-ADVANCED | Family 6 | Domain F

**Unit title:** NestJS file upload — Multer in-memory vs disk storage for multipart result storage
**Source URL:** https://docs.nestjs.com/techniques/file-upload
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT680-F6-F",
  "sourceIds": ["EXT-680"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "file-upload-storage-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS API accepts file uploads for document processing. The team uses Multer with memoryStorage() — files are held in a Buffer in memory. Under load with large files (10-50MB), the application runs out of heap memory.",
    "b": "Multer storage options: memoryStorage() buffers the file in RAM as a Buffer (req.file.buffer). diskStorage() writes to the local filesystem; the handler receives a path to the temp file. For cloud storage (Azure Blob, S3): stream the file directly from the Multer stream to the storage SDK without buffering the entire file in memory.",
    "c": "Streaming upload: pipe the req.file.stream to a cloud storage SDK upload stream. This keeps memory usage constant regardless of file size — only a small buffer window exists at any time."
  },
  "function": "Select Multer storage strategy for memory-efficient file upload result storage",
  "acceptedOutput": "Use Multer diskStorage() for temporary local file handling (predictable disk usage, stream to cloud on next step), or implement streaming upload directly: intercept the Multer stream and pipe to Azure Blob Storage uploadStream(). Memory usage stays at the streaming buffer size (~1MB) regardless of file size.",
  "rejectedOutput": "Keep memoryStorage() but limit file size to 5MB. Large files require streaming anyway.",
  "arbiters": ["registry-storage-arbiter", "file-upload-memory-arbiter"],
  "whyAccepted": "Streaming to cloud storage is the correct pattern for file uploads in containerized microservices — no disk I/O, constant memory overhead, no temp file cleanup needed. diskStorage() is a viable intermediate but requires disk space and cleanup. memoryStorage() does not scale beyond small files.",
  "whyRejected": "5MB limit with memoryStorage(): 100 concurrent uploads × 5MB = 500MB heap allocation. Under burst load this causes OOM. The file size limit does not solve the multiplied memory usage under concurrency. Streaming solves the root cause (memory buffering), not just the symptom (individual file size).",
  "sourceAnchors": ["https://docs.nestjs.com/techniques/file-upload"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-681 | IETF-RFCS | Family 6 | Domain F

**Unit title:** RFC-7234 HTTP caching — Cache-Control directives for API response result storage optimization
**Source URL:** https://www.rfc-editor.org/rfc/rfc7234
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT681-F6-F",
  "sourceIds": ["EXT-681"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "http-cache-directive-optimization",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A REST API returns product catalog data that changes daily. The API sets no Cache-Control header — every request hits the origin server. CDN pass-through rate is 100% (no cache hits). The team needs to configure caching without serving stale data to users.",
    "b": "Cache-Control directives: max-age=N (cache for N seconds), s-maxage=N (CDN cache duration, overrides max-age for shared caches), stale-while-revalidate=N (serve stale while revalidating in background), must-revalidate (no stale serving), no-cache (always revalidate with origin before serving). ETag and Last-Modified enable conditional requests for bandwidth efficiency.",
    "c": "For daily-changing catalog data: s-maxage=3600 max-age=0 (CDN caches for 1 hour, browsers always revalidate). Add ETag for conditional GET — CDN returns 304 Not Modified from cache if the resource hasn't changed, reducing origin load even during revalidation."
  },
  "function": "Configure HTTP Cache-Control directives for API response result storage optimization",
  "acceptedOutput": "Set Cache-Control: public, s-maxage=3600, stale-while-revalidate=60, max-age=0. CDN serves cached responses for 1 hour; browsers always revalidate with CDN (not origin). stale-while-revalidate=60 allows CDN to serve stale for 60s while asynchronously revalidating, eliminating thundering herd on cache expiry. Add ETag header for efficient conditional revalidation.",
  "rejectedOutput": "Set Cache-Control: no-store to prevent any caching. Data changes daily and stale data could cause user errors.",
  "arbiters": ["registry-storage-arbiter", "cache-directive-optimization-arbiter"],
  "whyAccepted": "s-maxage=3600 reduces origin requests by up to 99% (one cache fill per hour per CDN node). For daily-changing data, a 1-hour staleness window is acceptable. stale-while-revalidate prevents the cache expiry spike. ETag enables 304 responses that cost minimal bandwidth.",
  "whyRejected": "no-store for daily-changing data is overly conservative. 'Data changes daily' means maximum staleness is 24 hours without caching — a 1-hour CDN cache has maximum 1-hour staleness, significantly better than no-store's 0-second cache with 100% origin load.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc7234"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-682 | GOOGLE-AIP | Family 5 | Domain E

**Unit title:** AIP-194 declarative clients — resource locking and conflict detection arbitration
**Source URL:** https://google.aip.dev/194
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT682-F5-E",
  "sourceIds": ["EXT-682"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "resource-conflict-detection-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A declarative client (Terraform provider, kubectl) applies resource configurations. Two operators simultaneously apply different configurations to the same resource. One update overwrites the other without conflict detection.",
    "b": "AIP-194 specifies etag field on resources for optimistic concurrency. The client reads the resource (gets etag), includes the etag in the update request. The server rejects the update with HTTP 409 Conflict if the resource's current etag does not match — indicating a concurrent modification.",
    "c": "ETag-based optimistic locking prevents lost updates in declarative clients without distributed locks. The client retries: re-read, re-apply changes, re-send with new etag."
  },
  "function": "Arbitrate resource conflict detection for declarative client concurrent updates",
  "acceptedOutput": "Include etag in all resource representations. Require etag in UpdateResource requests (return HTTP 400 if missing for mutable resources). Server verifies: if resource.etag != request.etag → return HTTP 409 Conflict with current resource in response body. Declarative clients implement read-compare-apply with etag to detect concurrent modifications.",
  "rejectedOutput": "Use last-writer-wins. Declarative clients are idempotent — concurrent updates to the same desired state produce the same result regardless of order.",
  "arbiters": ["security-arbiter", "conflict-detection-arbitration-arbiter"],
  "whyAccepted": "Declarative clients apply different desired states, not the same state. Two operators applying different configurations to the same resource are in conflict. ETag detection surfaces the conflict explicitly — the losing operator sees the 409 and can review the current state before re-applying.",
  "whyRejected": "Last-writer-wins silently discards one operator's changes. For infrastructure resources (database configs, network policies, IAM bindings), silent overwrites can cause security or availability incidents. Operators need to know when their changes conflict with concurrent changes.",
  "sourceAnchors": ["https://google.aip.dev/194"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-683 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF data flywheel — model-generated vs human-authored training data quality learning
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT683-F8-G",
  "sourceIds": ["EXT-683"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "training-data-flywheel-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "After initial RLHF training on oasst1 human data, the team uses the improved model to generate training data for the next iteration (model-generated synthetic data). They observe the model starts to drift from human preferences — it improves on RLHF metrics but human evaluators rate the new responses as less natural.",
    "b": "Model-generated training data creates a self-referential loop: the model learns from itself. Early iterations amplify the model's existing biases and gradually diverge from the human preference distribution that oasst1 represents. This is sometimes called 'model collapse' in iterative training.",
    "c": "Mitigation: maintain a fixed ratio of human data to synthetic data (e.g., 70:30). Continuously collect new human preference data to ground each training iteration. Use synthetic data only for topics underrepresented in human data, not as a replacement."
  },
  "function": "Record training data flywheel quality strategy for RLHF pipeline learning",
  "acceptedOutput": "Maintain minimum 50% human preference data in each RLHF training iteration. Use synthetic data to supplement underrepresented topics but not replace human data. Continuously collect new human preference data through production feedback (thumbs up/down, correction submissions). Evaluate each iteration against a frozen human preference test set from oasst1.",
  "rejectedOutput": "Transition fully to model-generated training data after the first iteration. Model-generated data is cheaper and scalable; human data collection is the bottleneck.",
  "arbiters": ["learning-quality-arbiter", "training-data-flywheel-arbiter"],
  "whyAccepted": "Human preference data grounds the training distribution in actual human values. Model-generated data amplifies whatever the model already produces, reducing diversity and drifting from human preferences. The 50% floor maintains grounding across iterations.",
  "whyRejected": "Fully synthetic training data is scalable but accelerates model collapse. The model optimizes for patterns in its own outputs, which progressively diverge from what humans actually prefer. The cost savings are offset by the quality degradation and the eventual need to restart from human data.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-684 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba parallel corpus filtering — LASER embedding similarity for noisy pair removal
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT684-F2-C",
  "sourceIds": ["EXT-684"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "parallel-corpus-noise-filtering",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Tatoeba EN-FR parallel corpus contains 3M sentence pairs. A random sample audit reveals approximately 8% of pairs are misaligned (the source and target are unrelated sentences contributed by different users who both added translations to the same source but for different target sentences).",
    "b": "LASER (Language-Agnostic SEntence Representations) encodes sentences into a language-independent embedding space. Translation pairs should have high cosine similarity in this space (threshold ~0.7). COMET-QE (reference-free) is an alternative that uses a neural model trained to score translation quality without a reference.",
    "c": "8% noise at 3M pairs = 240,000 noisy pairs. Training on misaligned pairs teaches the model incorrect source-target correspondences, degrading translation quality for affected sentence types."
  },
  "function": "Apply LASER similarity filtering to remove noisy Tatoeba parallel pairs",
  "acceptedOutput": "Encode all EN and FR sentences with LASER. Compute cosine similarity for each pair. Filter out pairs with similarity < 0.7. Apply COMET-QE as a secondary filter for pairs near the threshold (0.65-0.75) — COMET-QE provides more nuanced quality scoring for ambiguous cases. Expected: remove ~8% of pairs while retaining high-quality alignments.",
  "rejectedOutput": "Accept the 8% noise rate. Neural MT models are robust to small amounts of training noise.",
  "arbiters": ["translation-quality-arbiter", "corpus-filtering-arbiter"],
  "whyAccepted": "8% noise is not small — it equals 240K misaligned pairs in a 3M corpus. For specific sentence types (rare vocabulary, long sentences), the misalignment rate may be higher and have disproportionate impact. LASER filtering is fast (embedding lookup + cosine similarity) and the filter is well-calibrated for parallel corpus quality.",
  "whyRejected": "MT model robustness to noise does not mean noise has no effect — it means the model compensates at the cost of additional training data needed to overcome the wrong signal. 240K misaligned pairs consume training capacity that could be used for correct alignments.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-685 | VUE-DOCS | Family 6 | Domain F

**Unit title:** Vue 3 KeepAlive — component instance caching for expensive result storage preservation
**Source URL:** https://vuejs.org/guide/built-ins/keep-alive
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT685-F6-F",
  "sourceIds": ["EXT-685"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "component-instance-caching",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 dashboard has tabs switching between a DataGrid component (500ms to render and fetch data) and a Chart component. Each tab switch destroys and recreates the DataGrid, re-fetching all data and re-rendering from scratch.",
    "b": "Vue <KeepAlive> wraps dynamic components or router-view, preserving the component instance (and its state) when switching away. The component's activated/deactivated lifecycle hooks fire instead of mounted/unmounted. The instance is kept in memory — not destroyed — and its rendered output is preserved.",
    "c": "KeepAlive includes/excludes: <KeepAlive include=\"DataGrid\"> or :max=\"5\" to limit cached instances. Overuse leads to memory bloat — only cache components with expensive initialization or valuable preserved state."
  },
  "function": "Apply KeepAlive for expensive component result preservation in tab switching",
  "acceptedOutput": "Wrap the tab router-view or dynamic component: <KeepAlive :include=\"['DataGrid']\"><component :is=\"activeTab\" /></KeepAlive>. DataGrid's data, scroll position, and filter state are preserved across tab switches. Implement onActivated() to refresh data if staleness is a concern (revalidate without full re-mount).",
  "rejectedOutput": "Use Vuex/Pinia to store the DataGrid's fetched data globally. Tab switches restore from the store rather than re-fetching.",
  "arbiters": ["registry-storage-arbiter", "component-caching-arbiter"],
  "whyAccepted": "KeepAlive preserves the complete component instance state (fetched data, rendered DOM, scroll position, user selections) without any state externalization. The 500ms re-render and re-fetch cost is eliminated entirely for cached instances.",
  "whyRejected": "Store-based caching externalizes only the fetched data — the 500ms render cost (virtual DOM diff, large table rendering) still occurs on each tab switch. KeepAlive preserves the rendered output itself, not just the data, eliminating both the fetch and render costs.",
  "sourceAnchors": ["https://vuejs.org/guide/built-ins/keep-alive"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-686 | EFCORE-SOURCE | Family 5 | Domain E

**Unit title:** EF Core query splitting for security — preventing data exfiltration via Include depth
**Source URL:** https://learn.microsoft.com/en-us/ef/core/querying/related-data/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT686-F5-E",
  "sourceIds": ["EXT-686"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "query-depth-security-arbitration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An EF Core API endpoint accepts a GraphQL-like include parameter from the client: /api/orders?include=customer.orders.items.product.supplier. The query blindly passes the includes to EF Core, loading deeply nested related data. A user queries their own order and receives all orders from their customer, all items across all orders, and all supplier details.",
    "b": "Unrestricted EF Core Include depth allows clients to traverse the entire object graph, potentially loading and returning data the user is not authorized to access (other users' orders, supplier PII). This is a data exfiltration risk via ORM query depth.",
    "c": "Defense: define allowed include paths as a whitelist per endpoint. Map client-requested includes to pre-defined projection queries. Never pass raw client-provided include strings to EF Core."
  },
  "function": "Arbitrate EF Core query depth to prevent data exfiltration via unrestricted includes",
  "acceptedOutput": "Define allowed includes per endpoint as a whitelist: var allowedIncludes = new[] { \"items\", \"items.product\" }. Map client-requested includes to these allowed paths. Reject requests including non-whitelisted paths with HTTP 400. Use explicit projections (Select()) for the allowed paths rather than Include() to control exactly which fields are returned.",
  "rejectedOutput": "Trust authenticated users to request only data they are entitled to. The API is authenticated, so clients requesting deep includes are authorized users.",
  "arbiters": ["security-arbiter", "query-depth-security-arbiter"],
  "whyAccepted": "Authentication determines who the user is; authorization determines what data they may access. An authenticated user requesting customer.orders traverses to other customers' orders, violating horizontal access control. Whitelisted includes and projections enforce the authorization boundary at the query level.",
  "whyRejected": "Authentication does not imply authorization for all related data. An authenticated customer can legitimately access their own order; they are not authorized to access all orders for their customer account, supplier data, or other related records traversable via include chains.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/querying/related-data/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-687 | DOTNET-MICROSERVICES | Family 1 | Domain A

**Unit title:** .NET service mesh — Dapr sidecar vs Envoy/Istio for microservice communication topology
**Source URL:** https://dapr.io/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT687-F1-A",
  "sourceIds": ["EXT-687"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "service-mesh-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET microservices platform evaluates adding a service mesh for: service-to-service encryption (mTLS), service discovery, retries, and distributed tracing. The team compares Dapr (Distributed Application Runtime) vs Istio/Envoy.",
    "b": "Dapr: application-level sidecar providing building blocks (service invocation, pub/sub, state management, secrets, actors) via HTTP/gRPC API. Dapr handles mTLS between sidecars. Application code uses Dapr SDK or HTTP calls to localhost. Istio/Envoy: infrastructure-level transparent proxy; application code is unchanged; handles traffic routing, mTLS, load balancing, and observability at the network layer.",
    "c": "Dapr trade-offs: application code change (use Dapr SDK/API), but provides portable building blocks (swap Redis for Azure Cache, swap RabbitMQ for Service Bus). Istio: no application change but complex K8s operator, large control plane, steep learning curve."
  },
  "function": "Select service mesh topology for .NET microservices platform",
  "acceptedOutput": "Use Dapr for .NET microservices that benefit from its building block abstractions (state management, pub/sub, actors). The Dapr SDK integrates naturally with ASP.NET Core and provides portable abstractions over cloud infrastructure. Use Istio/Envoy when: the mesh must be transparent to polyglot services with no code changes, or when advanced traffic management (canary, circuit breaking at network layer) is required.",
  "rejectedOutput": "Use Istio for all microservices. It provides transparent mTLS and observability without any application code changes.",
  "arbiters": ["topology-arbiter", "service-mesh-selection-arbiter"],
  "whyAccepted": "For .NET-native microservices, Dapr's building blocks reduce infrastructure coupling — the application uses the Dapr state API and can run against Redis locally and Azure Cosmos DB in production without code changes. The application-level integration is more natural for .NET developers than Istio's infrastructure-level transparency.",
  "whyRejected": "Istio's transparency advantage (no application code changes) comes with significant operational complexity (CRD management, control plane, pilot/citadel/mixer components). For a .NET-native platform, Dapr's application-level integration with ASP.NET Core is less complex to operate and provides richer building blocks.",
  "sourceAnchors": ["https://dapr.io/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-688 | MICROSOFT-LEARN-ARCH | Family 8 | Domain G

**Unit title:** Azure landing zone — hub-and-spoke vs flat VNet topology for enterprise network learning
**Source URL:** https://learn.microsoft.com/en-us/azure/architecture/landing-zones/
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT688-F8-G",
  "sourceIds": ["EXT-688"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "enterprise-network-topology-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An enterprise Azure deployment has 15 workload VNets (development, staging, production for 5 applications) and shared services (DNS, monitoring, Active Directory, firewall). The team evaluates hub-and-spoke vs flat VNet peering for network topology.",
    "b": "Hub-and-spoke: a central hub VNet contains shared services (firewall, DNS, VPN gateway). Spoke VNets peer to the hub, routing inter-spoke traffic through the hub firewall. Flat topology: all VNets peer directly (N×(N-1)/2 peerings for N VNets). Hub-and-spoke: centralized security policy, fewer peering connections. Flat: lower latency (no hub hop), no hub bottleneck.",
    "c": "Azure VNet peering is non-transitive — spoke A peering to hub does not enable spoke A to reach spoke B directly; traffic must route through the hub (via UDR). Azure Virtual WAN (managed hub-and-spoke) automates peering and routing management."
  },
  "function": "Record hub-and-spoke vs flat VNet topology selection for enterprise network learning",
  "acceptedOutput": "Use hub-and-spoke for enterprise deployments with shared services and security requirements. Centralize: Azure Firewall (for spoke-to-spoke traffic inspection), DNS Private Resolver, VPN/ExpressRoute gateway. Spokes contain workload VNets. Use Azure Virtual WAN to manage routing at scale. Security policy is enforced at the hub for all inter-spoke traffic.",
  "rejectedOutput": "Use flat topology with direct VNet peering between all VNets. Hub-and-spoke adds latency and the hub becomes a single point of failure.",
  "arbiters": ["learning-arbiter", "network-topology-arbiter"],
  "whyAccepted": "Hub-and-spoke provides centralized security control — all inter-spoke traffic is inspected by the hub firewall. Shared services (DNS, monitoring) are deployed once in the hub and accessible to all spokes. For 15 VNets, flat topology requires 105 peering connections; hub-and-spoke requires 15.",
  "whyRejected": "Flat topology's latency advantage (no hub hop) is typically 1-2ms — negligible for most enterprise workloads. The hub SPOF (Single Point of Failure) concern is addressed by Azure Firewall's zone redundancy. 105 peering connections in flat topology create O(N²) management complexity that makes security policy enforcement impractical.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/architecture/landing-zones/"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-689 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-918 SSRF (Server-Side Request Forgery) — URL allowlist arbitration in ASP.NET Core
**Source URL:** https://cwe.mitre.org/data/definitions/918.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT689-F5-E",
  "sourceIds": ["EXT-689"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "ssrf-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core API accepts a URL from the user and fetches the content: var response = await httpClient.GetAsync(userProvidedUrl). An attacker provides http://169.254.169.254/metadata (AWS IMDS endpoint) or http://10.0.0.1/admin and the server fetches internal network resources.",
    "b": "CWE-918 SSRF (Server-Side Request Forgery) allows attackers to make the server fetch arbitrary URLs, potentially accessing: cloud metadata services (AWS IMDS, Azure IMDS — contain IAM credentials), internal APIs not exposed externally, localhost services (Redis, internal admin panels).",
    "c": "Defense: URL allowlist (only permit known safe domains/paths), block private IP ranges (10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, 169.254.0.0/16, ::1), use a dedicated egress proxy that enforces domain allowlists."
  },
  "function": "Arbitrate SSRF prevention via URL allowlist for ASP.NET Core outbound request",
  "acceptedOutput": "Validate user-provided URLs against an allowlist: var allowedHosts = new[] { \"api.partner.com\", \"cdn.example.com\" }; var uri = new Uri(userProvidedUrl); if (!allowedHosts.Contains(uri.Host)) return BadRequest(\"URL not allowed\"). Additionally, resolve the host to IP and block private ranges. Use a dedicated egress proxy (e.g., Squid with domain allowlist) for defense in depth.",
  "rejectedOutput": "Block URLs containing '169.254' or '10.' as string patterns. This prevents the most obvious metadata service attacks.",
  "arbiters": ["security-arbiter", "ssrf-prevention-arbiter"],
  "whyAccepted": "Allowlist approach restricts to explicitly permitted hosts — all other hosts are blocked by default. Defense in depth: host allowlist + IP-after-DNS-resolution check prevents DNS rebinding attacks where a legitimate domain resolves to a private IP.",
  "whyRejected": "String pattern blocking is a denylist approach: it misses IPv6 representations of private addresses (::ffff:10.0.0.1), decimal-encoded IPs (2130706433 = 127.0.0.1), URL-encoded characters, and new private ranges. Attackers can always find bypass variants for denylist patterns.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/918.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-690 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V11.1 business logic — rate limiting and anti-automation arbitration
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT690-F5-E",
  "sourceIds": ["EXT-690"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "anti-automation-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "A ticket purchase API has no anti-automation controls. Scalpers use bots to purchase all available tickets within seconds of release, leaving no tickets for legitimate users. The team adds IP-based rate limiting (10 requests/minute per IP) but bots use residential proxies and distributed IPs to bypass it.",
    "b": "OWASP ASVS V11.1.2 requires anti-automation controls for high-value business operations. IP-based rate limiting is insufficient against distributed attacks. Effective controls: account-level rate limiting (per authenticated user), device fingerprinting, CAPTCHA for suspicious sessions, purchase quantity limits per account, randomized release time (within ±N minutes of announced time).",
    "c": "Business logic defenses layer: authenticated user purchase limits (2 tickets per account per event), per-account rate limiting (1 purchase attempt per 5 seconds), CAPTCHA triggered on velocity anomalies, purchase confirmation via email (delays automated bulk buying)."
  },
  "function": "Arbitrate anti-automation controls for high-value ticket purchase business logic",
  "acceptedOutput": "Implement layered controls: (1) per-authenticated-account purchase limit (2 tickets per event per account); (2) account-level rate limit (1 purchase attempt per 5 seconds); (3) queue system with random position for high-demand events (eliminates speed advantage); (4) CAPTCHA triggered on account velocity anomalies; (5) purchase confirmation step with 5-minute timeout (humans confirm, bots are slower to complete).",
  "rejectedOutput": "Increase IP rate limiting to 5 requests/minute. Bots making more than 5 requests/minute per IP will be blocked.",
  "arbiters": ["security-arbiter", "anti-automation-arbitration-arbiter"],
  "whyAccepted": "Account-level controls survive IP rotation — each account is limited regardless of IP. Queue systems eliminate the speed advantage entirely. Confirmation steps add friction that disproportionately affects automated buyers. Defense in depth across multiple control points requires bots to defeat all layers simultaneously.",
  "whyRejected": "IP rate limiting is a single control point that distributed bot networks trivially bypass with residential proxy pools. A botnet with 10,000 residential IPs operating at 5 requests/minute each makes 50,000 requests/minute total — far more than any legitimate user volume.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
