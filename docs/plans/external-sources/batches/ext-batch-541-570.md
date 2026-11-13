# External Sources Batch 19 — EXT-371 through EXT-390

Generated: 2026-05-04  
Ledger range: EXT-371 – EXT-390  
Source groups: KUBERNETES-KEPS, BIGCODEBENCH, SWE-BENCH, PYTHON-PEPS, OPENTELEMETRY-SPEC, DAPPER, ANGULAR-DOCS, REACT-ADVANCED, ASPNETCORE-DOCS-SAMPLES, NESTJS-ADVANCED, IETF-RFCS, GOOGLE-AIP, OPENASSISTANT-OASST1, TATOEBA, VUE-DOCS, EFCORE-SOURCE, DOTNET-MICROSERVICES, MICROSOFT-LEARN-ARCH, CWE-TOP25, OWASP-ASVS

---

## EXT-371

```json
{
  "rowId": "EXT-371",
  "sourceIds": ["KUBERNETES-KEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "B",
  "subdomain": "B3-ServiceMeshRouting",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Implement secrets management for a Kubernetes-deployed .NET 8 microservice that needs database connection strings and API keys. Do not store secrets in ConfigMaps or environment variables in plaintext.'",
    "b": "Options: (A) Kubernetes Secrets (base64 encoded, not encrypted at rest by default). (B) External Secrets Operator syncing from Azure Key Vault. (C) CSI Secrets Store Driver with Azure Key Vault provider, mounted as files.",
    "c": "Cluster runs on AKS (Azure Kubernetes Service). Secrets must be auditable. Rotation must propagate to pods within 5 minutes."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Use option B (External Secrets Operator) or option C (CSI Secrets Store Driver). Kubernetes Secrets (option A) are base64 encoded, not encrypted — any cluster user with RBAC permission to get Secrets can read the value. AKS with Azure Key Vault provides encryption at rest and RBAC at the Key Vault level. Action items for option B: (1) Install External Secrets Operator via Helm. (2) Create SecretStore referencing Azure Key Vault with Workload Identity (no client secret required). (3) Create ExternalSecret resources that sync Key Vault secrets to Kubernetes Secrets with `refreshInterval: 5m`. (4) Reference the synced Kubernetes Secret in the pod env or volume. Rotation propagation: ESO (External Secrets Operator) polls Key Vault every 5 minutes (refreshInterval) and updates the Kubernetes Secret; pods reading from volumes get updates automatically; pods reading env vars require a restart. For env var rotation, configure `reloader` sidecar or Argo Rollouts. Gate: `kubectl get externalsecret` shows `Synced` status for all secrets.",
  "rejectedOutput": "Use Kubernetes Secrets with envelope encryption enabled at the etcd level. This encrypts secrets at rest and avoids additional infrastructure.",
  "arbiters": ["aks-key-vault-integration", "external-secrets-operator-rotation"],
  "whyAccepted": "External Secrets Operator with Azure Key Vault provides: encryption at rest in Key Vault, RBAC audit logging, automatic rotation propagation within 5 minutes. etcd envelope encryption alone does not provide Key Vault-level audit trails.",
  "whyRejected": "etcd envelope encryption encrypts at rest but secrets are still stored in etcd, RBAC to `get secrets` still exposes values, and rotation requires manual secret updates. Azure Key Vault provides centralised management and audit logging beyond etcd encryption.",
  "sourceAnchors": [
    "https://external-secrets.io/latest/provider/azure-key-vault/",
    "https://learn.microsoft.com/en-us/azure/aks/csi-secrets-store-driver"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-372

```json
{
  "rowId": "EXT-372",
  "sourceIds": ["BIGCODEBENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "Implement `load_configuration(config_path: str) -> dict` that reads a YAML configuration file and returns its contents as a dict.",
    "b": "Submitted implementation uses `yaml.load(file, Loader=None)` without specifying a Loader.",
    "c": "The configuration file is user-supplied (the path comes from a command-line argument). YAML deserialization with the default (unsafe) Loader allows arbitrary Python object instantiation."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace with `yaml.safe_load(file)`. PyYAML's `yaml.load()` with `Loader=None` (or `Loader=yaml.FullLoader` without restriction) can instantiate arbitrary Python objects via YAML tags like `!!python/object/apply:os.system ['rm -rf /']`. This is a code execution vulnerability (CWE-94 via YAML deserialization). `yaml.safe_load()` only deserialises standard YAML types (str, int, float, list, dict, bool, None) — no Python object instantiation. For a configuration file reading use case (key-value pairs, lists, nested dicts), safe_load satisfies all requirements. PyYAML emits a warning when `Loader=None` is used in newer versions — the warning is the library's own indication that this usage is unsafe.",
  "rejectedOutput": "Use `yaml.load(file, Loader=yaml.FullLoader)`. FullLoader is safer than the default and is PyYAML's recommended loader for untrusted input.",
  "arbiters": ["pyyaml-safe-load", "yaml-deserialization-cwe-94"],
  "whyAccepted": "yaml.safe_load() is the correct loader for configuration files: handles all config-relevant types, no Python object instantiation, no code execution surface.",
  "whyRejected": "yaml.FullLoader is not safe for untrusted YAML: it can still instantiate some Python objects. PyYAML's documentation recommends safe_load for untrusted input. FullLoader is for trusted content that uses YAML tags.",
  "sourceAnchors": [
    "https://github.com/bigcode-project/bigcodebench",
    "https://pyyaml.org/wiki/PyYAMLDocumentation#loading-yaml"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-373

```json
{
  "rowId": "EXT-373",
  "sourceIds": ["SWE-BENCH"],
  "sourceTrustTier": 2,
  "datasetFamily": "domainResultsArray_to_resultI",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "arbiter-construction",
  "input": {
    "a": "A SWE-bench issue: a Python library's test suite fails with `DeprecationWarning: pkg_resources is deprecated as an API` in Python 3.12. The warning causes test failures because the test runner is configured with `filterwarnings = error`. Three agents propose: (A) Suppress the DeprecationWarning with `warnings.filterwarnings('ignore', category=DeprecationWarning, module='pkg_resources')`. (B) Replace `pkg_resources` usage with `importlib.metadata` (the modern replacement). (C) Change the test runner config to not treat DeprecationWarnings as errors.",
    "b": "The library uses `pkg_resources.get_distribution('some-package').version` in one place.",
    "c": "Python 3.12 is the target runtime. pkg_resources (setuptools) is deprecated."
  },
  "function": "domainResultsArray_to_resultI",
  "acceptedOutput": "Select option B: replace `pkg_resources.get_distribution('some-package').version` with `importlib.metadata.version('some-package')`. `importlib.metadata` is in the Python standard library since 3.8 and is the official replacement for `pkg_resources` version querying. Option A (suppress the warning) hides a real deprecation — pkg_resources will eventually be removed. Option C (change filterwarnings config) degrades the test suite's sensitivity to other deprecation warnings, hiding future issues. The replacement is one line: `from importlib.metadata import version; pkg_version = version('some-package')`. No additional dependencies required. This is the correct fix: replace the deprecated API with its standard library successor.",
  "rejectedOutput": "Apply option C: change the test runner config to not treat DeprecationWarnings as errors. The library cannot control what setuptools does with pkg_resources and the warning is not in the library's own code.",
  "arbiters": ["importlib-metadata-pkg-resources", "deprecation-warning-suppress-vs-fix"],
  "whyAccepted": "importlib.metadata is the standard library replacement available since Python 3.8. One-line change, zero new dependencies, eliminates the deprecated API usage.",
  "whyRejected": "Relaxing filterwarnings config degrades test suite sensitivity. Other deprecation warnings from the library's own code would now be silently ignored. The root cause (deprecated API usage) is fixable in one line.",
  "sourceAnchors": [
    "https://www.swebench.com",
    "https://docs.python.org/3/library/importlib.metadata.html"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-374

```json
{
  "rowId": "EXT-374",
  "sourceIds": ["PYTHON-PEPS"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G2-VersionComparison",
  "rowClass": "migration-decision",
  "input": {
    "a": "A Python 3.10 library uses `Union[str, int]` type annotations throughout (PEP-484 style). Python 3.10 introduced PEP-604 union syntax: `str | int`. Evaluate migrating to PEP-604 syntax.",
    "b": "PEP-604 union syntax (`X | Y`) is available for type annotations in Python 3.10+. It is also usable at runtime for isinstance checks: `isinstance(x, str | int)`. `Union[X, Y]` continues to work and is not deprecated.",
    "c": "Library supports Python 3.9+ (per pyproject.toml). 847 occurrences of Union[] in the codebase."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "DEFER migration. The library declares Python 3.9+ support. PEP-604 `str | int` syntax in annotations requires Python 3.10+ at runtime — using it without `from __future__ import annotations` breaks Python 3.9. With `from __future__ import annotations`, the union syntax is stored as a string and not evaluated at import time, making it compatible with 3.9. However, `isinstance(x, str | int)` at runtime still requires 3.10+. Migration requires: (1) Adding `from __future__ import annotations` to all 847-occurrence files, (2) Auditing for runtime isinstance uses of Union, (3) Dropping 3.9 support OR maintaining 3.9 compatibility workarounds. Until the minimum Python version is raised to 3.10, migration is BLOCKED. Schedule when 3.9 reaches end-of-life (2025-10-05) and the minimum is raised.",
  "rejectedOutput": "Migrate to PEP-604 syntax now. PEP-604 is cleaner and Python 3.9 is approaching end-of-life.",
  "arbiters": ["pep-604-union-syntax", "python-version-min-compatibility"],
  "whyAccepted": "PEP-604 `|` syntax requires Python 3.10 at runtime. The library supports 3.9. Migration without dropping 3.9 requires from __future__ annotations in all files AND no runtime isinstance(x, X | Y) — a complex migration for 847 occurrences.",
  "whyRejected": "'Python 3.9 approaching end-of-life' is not the same as 'end-of-life'. Library users may still target 3.9. The migration is blocked until the minimum version is officially raised in pyproject.toml.",
  "sourceAnchors": [
    "https://peps.python.org/pep-0604/",
    "https://peps.python.org/pep-0563/"
  ],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-375

```json
{
  "rowId": "EXT-375",
  "sourceIds": ["OPENTELEMETRY-SPEC"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A microservice generates custom business metrics using OpenTelemetry: `order_processing_duration_seconds`, `payment_failure_count`, `inventory_reservation_count`. The team debates: (A) Use OpenTelemetry Metrics API with a Prometheus exporter. (B) Expose a `/metrics` endpoint manually using the `prometheus-net` library directly.",
    "b": "The service already uses the OTel SDK for tracing. Other services use OTel for metrics. The Prometheus scrape config is already in place.",
    "c": "The team wants to correlate metrics with traces using exemplars."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use OpenTelemetry Metrics API with Prometheus exporter (option A). The OTel Metrics API provides backend portability: if the team switches from Prometheus to Azure Monitor or Grafana Cloud in the future, only the exporter changes. The OTel SDK also enables exemplars natively: `SetExemplarFilter(ExemplarFilterType.TraceBased)` attaches the current trace_id and span_id to metric data points, enabling drill-down from metric anomalies to the specific trace. prometheus-net (option B) does not support exemplars and creates a split observability stack: OTel for traces, prometheus-net for metrics — two separate instrumentation APIs to maintain. Since the service already uses the OTel SDK for tracing, adding OTel metrics is a single `.WithMetrics(builder => builder.AddMeter(\"my.service\"))` addition to the existing OTel setup.",
  "rejectedOutput": "Use prometheus-net directly. It is the most widely used .NET Prometheus library and has mature ASP.NET Core integration. OTel Metrics is an additional abstraction layer.",
  "arbiters": ["otel-metrics-exemplars", "otel-prometheus-exporter"],
  "whyAccepted": "OTel Metrics + Prometheus exporter provides exemplar support (trace correlation) that prometheus-net cannot. Single OTel SDK for both traces and metrics reduces instrumentation surface.",
  "whyRejected": "prometheus-net lacks exemplar support, splitting traces (OTel) and metrics (prometheus-net) into two instrumentation systems. The 'additional abstraction layer' cost is offset by exemplars and backend portability.",
  "sourceAnchors": [
    "https://opentelemetry.io/docs/specs/otel/metrics/",
    "https://opentelemetry.io/docs/languages/dotnet/instrumentation/#metrics"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-376

```json
{
  "rowId": "EXT-376",
  "sourceIds": ["DAPPER"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Dapper-based data access layer needs to handle an optional filter: the query should return all orders if `customerId` is null, or only orders for that customer if provided. Current implementation: two separate methods `GetAllOrders()` and `GetOrdersByCustomer(int customerId)`.",
    "b": "Proposed: single method `GetOrders(int? customerId)` with SQL: `SELECT * FROM Orders WHERE (@CustomerId IS NULL OR CustomerId = @CustomerId)`.",
    "c": "Orders table has 5M rows. CustomerId is an indexed column."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Keep the two separate methods (`GetAllOrders` and `GetOrdersByCustomer`). The proposed `@CustomerId IS NULL OR CustomerId = @CustomerId` pattern is a 'catch-all query' anti-pattern that prevents index usage. SQL Server cannot optimise this because the query plan must handle both the 'return all rows' and 'return indexed rows' cases with a single compiled plan. For 5M rows, a full table scan every time CustomerId IS NULL is the result — even when the caller always provides a value, the query plan may not use the index due to parameter sniffing or plan cache pollution. The two-method approach generates two distinct, cacheable query plans: `GetAllOrders()` gets a full-scan plan (correct), `GetOrdersByCustomer()` gets an index-seek plan (correct). Alternatively, if a single method is preferred for DRY (Don't Repeat Yourself) reasons, build the query conditionally: `var sql = customerId.HasValue ? \"SELECT * FROM Orders WHERE CustomerId = @CustomerId\" : \"SELECT * FROM Orders\";`.",
  "rejectedOutput": "Use the single method with @CustomerId IS NULL OR CustomerId = @CustomerId. It is cleaner and avoids code duplication.",
  "arbiters": ["catch-all-query-antipattern", "sql-server-index-seek-parameter"],
  "whyAccepted": "Two separate queries generate two distinct, optimisable query plans. The catch-all pattern forces a plan that cannot use the index for the common case.",
  "whyRejected": "The @CustomerId IS NULL OR pattern is a documented SQL Server anti-pattern that prevents index utilisation. 'Cleaner' code that causes full table scans on 5M rows is not a valid tradeoff.",
  "sourceAnchors": [
    "https://github.com/DapperLib/Dapper",
    "https://use-the-index-luke.com/sql/where-clause/obfuscation/smart-logic"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-377

```json
{
  "rowId": "EXT-377",
  "sourceIds": ["ANGULAR-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular 17 application uses HttpClient to call an API that occasionally returns 429 Too Many Requests or 503 Service Unavailable. The team wants to add automatic retry with exponential backoff.",
    "b": "Options: (A) Use RxJS `retryWhen` operator with a custom delay strategy. (B) Use RxJS `retry({count: 3, delay: exponentialBackoffFn})` (RxJS 7+ API). (C) Implement retry in the HttpInterceptor with a recursive retry function.",
    "c": "Angular 17 uses RxJS 7.8. Retries should only apply to 429 and 503 responses, not to 400, 401, 404, or 5xx other than 503."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option B: RxJS `retry({count: 3, delay: (error, retryCount) => ...})` in an HttpInterceptor. The retry configuration: `delay: (error, retryCount) => { if (error.status !== 429 && error.status !== 503) return throwError(() => error); const backoff = Math.pow(2, retryCount) * 1000; return timer(backoff); }`. This applies exponential backoff (1s, 2s, 4s) only for 429/503 responses, passes through all other errors immediately. `retryWhen` (option A) is deprecated in RxJS 7 — the replacement is `retry({delay:...})`. Option C (recursive interceptor function) reinvents RxJS retry logic with observable subscription complexity — harder to maintain and test. Implement in a dedicated HttpRetryInterceptor provided globally, keeping retry logic separate from feature-specific interceptors.",
  "rejectedOutput": "Use retryWhen (option A). It provides the most explicit control over retry logic and is widely documented in Angular tutorials.",
  "arbiters": ["rxjs-retry-delay-api", "http-interceptor-retry"],
  "whyAccepted": "RxJS 7's `retry({delay})` is the current API, correctly handles status-specific retry, and is composable. `retryWhen` is deprecated in RxJS 7 — Angular 17 uses RxJS 7.8.",
  "whyRejected": "retryWhen is deprecated in the version of RxJS Angular 17 uses. Using deprecated APIs creates a migration burden and may produce deprecation warnings in tests.",
  "sourceAnchors": [
    "https://rxjs.dev/api/operators/retry",
    "https://angular.dev/guide/http/interceptors"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-378

```json
{
  "rowId": "EXT-378",
  "sourceIds": ["REACT-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D1-DomainExecution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A React component uses `useEffect` with `[]` dependency array (runs once on mount) to fetch initial data: `useEffect(() => { fetchData(userId).then(setData) }, [])`. A linter warning fires: 'React Hook useEffect has missing dependency: userId'.",
    "b": "The developer suppresses the linter warning with `// eslint-disable-next-line react-hooks/exhaustive-deps` to silence the warning while keeping the effect run-once behaviour.",
    "c": "The component receives `userId` as a prop. The component is reused in a list where the same component instance is reused with different userIds (React virtualised list or keyed list)."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Remove the eslint-disable and add `userId` to the dependency array: `useEffect(() => { fetchData(userId).then(setData) }, [userId])`. The linter warning is correct: `userId` is a dependency of the effect and must be in the array. If the component is reused with a different userId (as stated), omitting userId causes stale data — the effect only runs once with the first userId, and subsequent userId prop changes don't trigger a re-fetch. The `// eslint-disable` suppresses a real bug. The correct fix is `[userId]` in the dependency array, which re-fetches when userId changes. If the 'run only once' requirement is truly intentional and userId never changes, the component should have a `key={userId}` prop at the usage site to force component remount on userId change rather than re-rendering with a new prop.",
  "rejectedOutput": "The eslint-disable is acceptable. The developer intentionally wants the effect to run only on mount. The linter rule is overly strict in this case.",
  "arbiters": ["react-use-effect-dependencies", "eslint-exhaustive-deps-rule"],
  "whyAccepted": "The dependency array with userId re-fetches on prop change, which is the correct behaviour for a component reused with different userIds. Suppressing the lint warning hides a stale-data bug.",
  "whyRejected": "The eslint-disable suppresses a real bug: the effect captures the initial userId in a closure and never updates when userId changes. 'Intentionally run once' is only correct if userId is guaranteed to never change — the problem statement states it can change.",
  "sourceAnchors": [
    "https://react.dev/reference/react/useEffect#specifying-reactive-dependencies",
    "https://react.dev/learn/removing-effect-dependencies"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-379

```json
{
  "rowId": "EXT-379",
  "sourceIds": ["ASPNETCORE-DOCS-SAMPLES"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core 8 API needs to validate incoming JSON request bodies. Two approaches: (A) Data Annotations on the DTO class: `[Required]`, `[Range(1, 1000)]`, `[StringLength(100)]`, validated automatically by model binding. (B) FluentValidation library with a dedicated validator class per DTO.",
    "b": "The API has 30 DTO types. Some validation rules are complex: cross-field validation (endDate > startDate), conditional validation (if Type == 'priority' then Description is required), and database lookups (validate that a referenced CustomerId exists).",
    "c": "Team prefers testable validation logic."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use FluentValidation (option B) for complex validation cases, with Data Annotations for simple constraints that the team chooses to keep as documentation on the DTO. FluentValidation's RuleFor chain natively handles: cross-field validation (`RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)`), conditional validation (`When(x => x.Type == \"priority\", () => RuleFor(x => x.Description).NotEmpty())`), and async database lookups (`MustAsync(async (id, ct) => await repo.CustomerExistsAsync(id, ct))`). Data Annotations cannot express these cases without custom attributes that are harder to test. FluentValidation validators are plain C# classes — unit testable with `new OrderValidator().Validate(dto)` without an HTTP pipeline. Register FluentValidation globally: `services.AddFluentValidationAutoValidation()`. For the 30 DTOs, simple ones can retain `[Required]`/`[Range]` if the team prefers, but all cross-field and conditional rules must be in FluentValidation.",
  "rejectedOutput": "Use Data Annotations only. FluentValidation is an additional dependency and Data Annotations with custom validation attributes can handle all the described cases.",
  "arbiters": ["fluentvalidation-cross-field", "data-annotations-testability"],
  "whyAccepted": "FluentValidation natively handles cross-field, conditional, and async validation with plain C# testable classes. Custom validation attributes for Data Annotations require attribute infrastructure and are harder to test in isolation.",
  "whyRejected": "Data Annotations custom attributes for complex validation require implementing IValidatableObject or custom ValidationAttribute — harder to test, more boilerplate, and cannot do async (database lookup) validation without framework tricks.",
  "sourceAnchors": [
    "https://docs.fluentvalidation.net/en/latest/",
    "https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-380

```json
{
  "rowId": "EXT-380",
  "sourceIds": ["NESTJS-ADVANCED"],
  "sourceTrustTier": 1,
  "datasetFamily": "historyAndVersions_to_reprocessingDecision",
  "domain": "G",
  "subdomain": "G3-RetrainingTrigger",
  "rowClass": "migration-decision",
  "input": {
    "a": "A NestJS application uses Mongoose ODM (Object Document Mapper) for MongoDB access. The team considers migrating to Prisma ORM which added MongoDB support in Prisma 3.12. Evaluate the migration.",
    "b": "Current Mongoose usage: 23 schemas, 8 with discriminators (polymorphic documents), 4 with custom middleware (pre/post save hooks), and 2 with virtual fields. Prisma MongoDB: supports basic CRUD but has limited discriminator support and no virtual fields.",
    "c": "Primary motivation: team prefers Prisma's type-safe query builder over Mongoose's runtime type checking."
  },
  "function": "historyAndVersions_to_reprocessingDecision",
  "acceptedOutput": "DEFER migration. Prisma's MongoDB support has significant feature gaps for this use case: (1) Discriminators (polymorphic documents via `__t` field) — Prisma MongoDB does not natively support Mongoose-style discriminators as of Prisma 5. (2) Virtual fields — Prisma has no virtual/computed field equivalent for MongoDB. (3) Pre/post hooks — Prisma has middleware but with different semantics than Mongoose hooks. The 8 discriminator schemas and 4 hook schemas are high-risk migration targets. The type-safety motivation is valid — Prisma's generated client is strongly typed. Alternative: add `mongoose-lean-types` or `typegoose` to Mongoose to get TypeScript type safety without migration. Revisit Prisma MongoDB migration when discriminator support is confirmed stable.",
  "rejectedOutput": "Migrate to Prisma. The type safety benefits outweigh the migration complexity and Prisma is the more modern ORM.",
  "arbiters": ["prisma-mongodb-discriminator-support", "typegoose-type-safety-alternative"],
  "whyAccepted": "Prisma MongoDB's discriminator and virtual field gaps make 8 + 2 = 10 of the 23 schemas high-risk migrations. The type-safety goal is achievable with typegoose without a high-risk migration.",
  "whyRejected": "'More modern ORM' is not a sufficient reason to migrate when the new tool lacks features the current system uses. Discriminators and virtual fields are load-bearing features, not edge cases.",
  "sourceAnchors": [
    "https://www.prisma.io/docs/orm/overview/databases/mongodb",
    "https://github.com/typegoose/typegoose"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-381

```json
{
  "rowId": "EXT-381",
  "sourceIds": ["IETF-RFCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API endpoint creates a new resource (POST /orders) and returns the created order. Two response designs: (A) Return `201 Created` with the full order object in the body and `Location: /orders/123` header. (B) Return `200 OK` with the full order object in the body.",
    "b": "RFC 9110 §9.3.3 specifies POST creating a resource should return 201 with Location header.",
    "c": "Client SDK auto-follows Location headers to fetch the canonical resource representation."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option A: `201 Created` with the full body AND Location header. RFC 9110 §9.3.3 is clear: a 201 response SHOULD include a Location header pointing to the created resource. Including the full body in the 201 response is a common extension that avoids a follow-up GET request — the client has the created resource immediately. The Location header provides the canonical resource URL for caching and future reference. The client SDK auto-following Location is orthogonal — the 201 + Location + body combination satisfies both: SDK knows the canonical URL, client has the resource without a follow-up GET. Using 200 for a resource creation response is semantically incorrect — 200 means 'the request was fulfilled', 201 means 'a resource was created'. This distinction matters for HTTP caches and monitoring systems that parse status codes.",
  "rejectedOutput": "Use 200 OK. Most clients treat 200 and 201 identically and the Location header is redundant when the full body is returned. Simplicity is preferable.",
  "arbiters": ["rfc-9110-201-created", "location-header-canonical-url"],
  "whyAccepted": "201 + Location + body is correct per RFC 9110: semantic accuracy, canonical URL in Location, full body avoids follow-up GET. HTTP caches and monitoring tools parse status codes.",
  "whyRejected": "'Clients treat 200 and 201 identically' is true for application code but false for HTTP caches and API monitoring systems. RFC-compliant status codes are not optional cosmetics.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc9110#section-9.3.3",
    "https://datatracker.ietf.org/doc/html/rfc9110#section-15.3.2"
  ],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-382

```json
{
  "rowId": "EXT-382",
  "sourceIds": ["GOOGLE-AIP"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D3-ResultShaping",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API resource `Order` has a `total_amount` field (decimal, monetary value). The API currently returns `total_amount: 19.99` as a JSON number. A client reports precision issues: `19.99` is represented as `19.990000000000001` in JavaScript after JSON.parse().",
    "b": "RFC 8259 §6 specifies JSON numbers do not have precision limits but implementations vary. JavaScript's Number type uses IEEE 754 double-precision floating-point — it cannot represent all decimal values exactly.",
    "c": "Monetary values must be represented exactly. The API serves JavaScript, Python, and Java clients."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Return monetary values as strings in JSON: `total_amount: \"19.99\"`. JavaScript's JSON.parse() converts JSON numbers to IEEE 754 doubles — `19.99` cannot be represented exactly and becomes `19.990000000000001`. Returning as a string `\"19.99\"` preserves the exact decimal representation. Python and Java JSON parsers handle string-encoded decimals correctly (parse to Decimal/BigDecimal). Include API documentation specifying that monetary fields are string-encoded decimals. Alternatively, return the value in the smallest currency unit (integer cents): `total_amount_cents: 1999` — integers within the safe integer range (Number.MAX_SAFE_INTEGER = 2^53-1) are represented exactly in IEEE 754. The cents approach is unambiguous but requires client-side conversion for display. Both approaches are correct — document the choice explicitly.",
  "rejectedOutput": "Keep returning total_amount as a JSON number. JavaScript clients should use toFixed(2) to round the display value. This is a display issue, not an API issue.",
  "arbiters": ["rfc-8259-json-number-precision", "ieee-754-monetary-values"],
  "whyAccepted": "String-encoded decimals or integer cents are the correct approaches for monetary values in JSON APIs. The precision loss is in the data type, not in display — toFixed(2) rounds for display but the stored JavaScript value is already imprecise.",
  "whyRejected": "toFixed(2) rounds the display string but the underlying JavaScript Number is still imprecise. Arithmetic on `19.990000000000001 + 0.01` gives `20.000000000000004`. This is a data correctness issue, not cosmetic.",
  "sourceAnchors": [
    "https://datatracker.ietf.org/doc/html/rfc8259#section-6",
    "https://google.aip.dev/143"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-383

```json
{
  "rowId": "EXT-383",
  "sourceIds": ["OPENASSISTANT-OASST1"],
  "sourceTrustTier": 2,
  "datasetFamily": "resultI_to_finalMachineResults",
  "domain": "C",
  "subdomain": "C2-ResultIntegration",
  "rowClass": "negative-example",
  "input": {
    "a": "A model inference service reports: 'Inference latency increased from 45ms to 380ms after deploying the new model version (4.2B → 7B parameters).' GPU utilisation is at 90%.",
    "b": "Three operators: (R1) Roll back to the 4.2B model. (R2) Enable model quantisation (INT8 or INT4). (R3) Scale out to 4 GPU instances.",
    "c": "The 7B model produces measurably better outputs (BLEU +3.2, win rate +12% in A/B test). Rolling back loses these quality gains. Budget: 2x current GPU spend allowed."
  },
  "function": "resultI_to_finalMachineResults",
  "acceptedOutput": "Apply R2 (quantisation) first, then evaluate if R3 (scale-out) is needed. INT8 quantisation of the 7B model typically reduces memory bandwidth requirements by ~50% and inference latency by 30-50% with <1% quality degradation on most benchmarks. Target: bring latency from 380ms back towards the 45-100ms range without losing the +3.2 BLEU quality gain. If quantisation brings latency to ~150ms and budget allows, add a second GPU instance (scale-out to 2). R1 (rollback) is a last resort: it loses 12% win rate improvement from the A/B test — a real user experience regression. R3 (scale-out to 4 GPUs) is the maximum spend allowed (2x budget = 2 GPUs) and should be applied only after quantisation narrows the gap.",
  "rejectedOutput": "Apply R3: scale out to 4 GPU instances immediately. This directly addresses throughput and latency by distributing load, and the budget allows it.",
  "arbiters": ["model-quantisation-latency", "scale-out-vs-optimise-first"],
  "whyAccepted": "Quantisation targets the root cause (7B model memory bandwidth) and recovers latency without rollback or full scale-out cost. Scale-out to 4 GPUs exceeds the 2x budget cap.",
  "whyRejected": "Scale-out to 4 GPUs is 4x current spend — beyond the 2x budget limit. Quantisation should be applied first as it may bring latency within acceptable range at 1x current GPU count.",
  "sourceAnchors": [
    "https://huggingface.co/datasets/OpenAssistant/oasst1",
    "https://huggingface.co/docs/transformers/quantization/overview"
  ],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-384

```json
{
  "rowId": "EXT-384",
  "sourceIds": ["TATOEBA"],
  "sourceTrustTier": 2,
  "datasetFamily": "machineCommand_to_machineActionItems",
  "domain": "C",
  "subdomain": "C1-SemanticMapping",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Decompose the command: 'Build a language identification classifier for Tatoeba corpus sentences. The classifier must distinguish 10 languages: EN, FR, ES, DE, ZH, JA, AR, RU, PT, IT. Accuracy target: 99% on single sentences.'",
    "b": "Options: (A) Train a character n-gram logistic regression model from scratch on Tatoeba data. (B) Use fastText's pre-trained LID (Language IDentification) model (`lid.176.bin`). (C) Use langdetect library (port of Google's language-detection).",
    "c": "Sentences average 8 words / 45 characters. Some sentences as short as 2 words."
  },
  "function": "machineCommand_to_machineActionItems",
  "acceptedOutput": "Use fastText's pre-trained LID model (option B). Action items: (1) Install fastText Python binding: `pip install fasttext-wheel`. (2) Download lid.176.bin (pre-trained on 176 languages). (3) Load: `model = fasttext.load_model('lid.176.bin')`. (4) Predict: `lang, confidence = model.predict(sentence.replace('\\n', ' '))`. (5) Extract the language code: `lang[0].replace('__label__', '')`. fastText lid.176 achieves >99% accuracy on 8-word sentences for all 10 target languages. Option A (train from scratch) requires significant training data curation and hyperparameter tuning to reach 99% — the fastText pre-trained model already exceeds this. Option C (langdetect) uses a probabilistic model with known issues on short texts (2-word sentences can produce wrong classifications) and is slower than fastText. Gate: evaluate on 1,000 held-out sentences per language, confirm >99% accuracy before production use.",
  "rejectedOutput": "Train a character n-gram logistic regression from scratch (option A). This gives full control over the model and avoids external model dependencies.",
  "arbiters": ["fasttext-lid-model", "short-sentence-langdetect-accuracy"],
  "whyAccepted": "fastText lid.176 is pre-trained on 176 languages at >99% accuracy, handles short sentences, and runs in microseconds. Training from scratch provides no benefit over a pre-trained model that already exceeds the accuracy target.",
  "whyRejected": "Training from scratch requires weeks of data curation and experimentation to reach 99% accuracy that a pre-trained model already provides. 'Full control' is a process benefit that does not outweigh the accuracy and effort cost.",
  "sourceAnchors": [
    "https://tatoeba.org",
    "https://fasttext.cc/docs/en/language-identification.html"
  ],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-385

```json
{
  "rowId": "EXT-385",
  "sourceIds": ["VUE-DOCS"],
  "sourceTrustTier": 1,
  "datasetFamily": "machineActionItem_to_domainResult",
  "domain": "D",
  "subdomain": "D2-ExecutionPlan",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Vue 3 component directly mutates a prop: `props.items.push(newItem)` inside a button click handler. The component is in a parent-child hierarchy where the parent owns the items array.",
    "b": "The mutation works in practice because JavaScript objects/arrays passed as props share the same reference.",
    "c": "Vue 3's reactivity system warns: 'Avoid mutating a prop directly since the value will be overwritten whenever the parent component re-renders'."
  },
  "function": "machineActionItem_to_domainResult",
  "acceptedOutput": "Replace the direct prop mutation with an emit event. The correct Vue 3 pattern: (1) Remove `props.items.push(newItem)`. (2) Emit an event: `emit('update:items', [...props.items, newItem])`. (3) In the parent template: `<ChildComponent v-model:items=\"parentItems\" />` or `@update:items=\"parentItems = $event\"`. Direct prop mutation violates the one-way data flow principle: props flow from parent to child; events flow from child to parent. Although the mutation 'works' because of reference sharing, Vue will overwrite the mutation on the next parent re-render. This creates subtle bugs: the mutation appears to work, then silently reverts when the parent re-renders for unrelated reasons. The Vue 3 warning is not cosmetic — it documents a real bug class.",
  "rejectedOutput": "The prop mutation is acceptable if it works reliably in practice. Vue's reactivity system will track the push() and trigger reactivity without issues.",
  "arbiters": ["vue3-one-way-data-flow", "prop-mutation-overwrite-risk"],
  "whyAccepted": "emit('update:items') is the correct Vue 3 pattern for child-to-parent data flow. Prop mutation is overwritten on parent re-render — a real bug that the Vue warning accurately describes.",
  "whyRejected": "Prop mutation 'works' until the parent re-renders for any unrelated reason and overwrites the mutated value. Vue's reactivity tracking does not prevent the overwrite — it only makes the mutation visible to the current render cycle.",
  "sourceAnchors": [
    "https://vuejs.org/guide/components/props.html#one-way-data-flow",
    "https://vuejs.org/guide/components/events.html#usage-with-v-model"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-386

```json
{
  "rowId": "EXT-386",
  "sourceIds": ["EFCORE-SOURCE"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E3-ConsensusFormation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An EF Core 8 application has a `Product` entity with a `Description` property (NVARCHAR(MAX)). Queries filter by `Name` and `Category` columns but never filter on `Description`. A developer proposes adding a `[NotMapped]` attribute to `Description` to avoid loading it in queries.",
    "b": "Option A: `[NotMapped]` on Description — EF Core excludes the column from all queries and migrations. Option B: Lazy loading via `virtual` property and navigation property configuration. Option C: Use `Select()` projection in read queries to exclude Description.",
    "c": "Description averages 2KB. Table has 500K rows. Read queries outnumber write queries 100:1."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "Use option C: `Select()` projection in read queries. `[NotMapped]` (option A) is incorrect — it removes the column from the schema, meaning Description can never be persisted to the database. Lazy loading (option B) adds N+1 query risk and requires a virtual property with a configured proxy — significant complexity for a simple column exclusion. Option C is surgical: `context.Products.Where(filter).Select(p => new ProductSummaryDto { Id = p.Id, Name = p.Name, Category = p.Category }).ToListAsync()` — the SELECT query never touches the Description column. For the 1% of queries that need Description, use `context.Products.Where(filter).Select(p => new ProductDetailDto { ... Description = p.Description ... }).FirstOrDefaultAsync()`. This is the EF Core-idiomatic solution: projection controls which columns are fetched without schema changes or proxy magic.",
  "rejectedOutput": "Add [NotMapped] to Description. This is a one-line fix that prevents Description from ever being loaded.",
  "arbiters": ["efcore-projection-column-exclusion", "not-mapped-removes-persistence"],
  "whyAccepted": "Select() projection excludes columns from read queries without removing persistence. NotMapped removes the column from the database schema — Description can never be saved or loaded again.",
  "whyRejected": "[NotMapped] removes the column from EF Core's schema mapping, meaning migrations would drop the column and Description cannot be persisted. This is irreversible data loss for existing rows.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/ef/core/querying/related-data/explicit",
    "https://github.com/dotnet/efcore"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-387

```json
{
  "rowId": "EXT-387",
  "sourceIds": ["DOTNET-MICROSERVICES"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A2-ComponentOrchestration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A .NET 8 microservice needs to implement an event-driven inbox pattern: incoming domain events from a message bus must be idempotently processed exactly-once, even if the message is delivered multiple times.",
    "b": "Options: (A) Store processed event IDs in a Redis SET with TTL; skip if ID already present. (B) Store processed event IDs in a SQL Server Inbox table with a unique constraint on event_id; use a transaction to check-then-insert.",
    "c": "Message broker: Azure Service Bus with at-least-once delivery. Event volume: 500 events/day. Each event triggers a database write."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option B: SQL Server Inbox table with unique constraint. At 500 events/day, a Redis SET with TTL (option A) has a correctness risk: if the TTL expires before a delayed redelivery arrives, the event is processed twice — exactly the scenario the inbox pattern must prevent. Service Bus supports redelivery of messages up to 10 days (dead-letter queue). A Redis TTL shorter than 10 days creates a window for duplicate processing. The SQL Server Inbox table with `event_id NVARCHAR(100) PRIMARY KEY` provides permanent deduplication: even if a message is redelivered after weeks, the unique constraint rejects the duplicate insert. The transaction pattern: `BEGIN TRANSACTION; INSERT INTO Inbox (event_id, received_at) VALUES (@id, SYSUTCDATETIME()); -- process event; COMMIT;` — if the insert fails with a unique violation, skip processing. This is atomic with the business logic write in the same database transaction.",
  "rejectedOutput": "Use Redis SET with TTL=7 days (option A). Redis is faster than SQL Server for key lookups and 7 days covers the typical redelivery window.",
  "arbiters": ["inbox-pattern-sql-unique-constraint", "redis-ttl-redelivery-window"],
  "whyAccepted": "SQL Server unique constraint on event_id provides permanent deduplication with no TTL expiry risk. The inbox check and business logic write are in the same database transaction — atomic.",
  "whyRejected": "Redis TTL=7 days covers typical redelivery but not the 10-day Service Bus maximum. Events redelivered after 7 days would be processed twice. SQL Server provides exact-once semantics without TTL expiry.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/",
    "https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-expiration"
  ],
  "licenseConstraint": "MIT",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-388

```json
{
  "rowId": "EXT-388",
  "sourceIds": ["MICROSOFT-LEARN-ARCH"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E1-ConflictResolution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A distributed system uses optimistic concurrency: each resource has a `version` field (integer). Updates require sending the current version; the server rejects updates where `version` doesn't match the stored version (409 Conflict). A load test reveals 30% of updates fail with 409 under high concurrency.",
    "b": "Three proposals: (A) Switch to pessimistic locking (database row lock). (B) Implement retry with exponential backoff on 409. (C) Add client-side conflict resolution: when a 409 is received, fetch the latest version, merge changes, and retry.",
    "c": "Conflict rate is 30% because multiple clients update the same resource simultaneously. The resource is a shared counter (inventory level)."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "For a shared counter (inventory level), neither optimistic concurrency nor pessimistic locking is the correct pattern. Use a SQL atomic increment: `UPDATE Inventory SET quantity = quantity - @decrBy WHERE id = @id AND quantity >= @decrBy` — a single atomic SQL statement that decrements only if sufficient inventory exists. This eliminates both the 30% conflict rate (no version check needed) and the database lock contention of pessimistic locking. The optimistic concurrency + retry pattern (option B) works but at 30% conflict rate requires an average of 1.4 round trips per update. Option C (client merge) is unnecessary for a counter: there is no 'merge' for counters — the server-side atomic decrement is always correct. If optimistic concurrency is required for other fields on the same resource, use conditional UPDATE for the counter field specifically and a version check for the other fields.",
  "rejectedOutput": "Implement retry with exponential backoff on 409 (option B). This handles the conflict gracefully without changing the locking strategy, and 30% conflict rate is acceptable at the current scale.",
  "arbiters": ["atomic-sql-increment-counter", "optimistic-concurrency-counter-antipattern"],
  "whyAccepted": "Atomic SQL UPDATE for a shared counter is the correct pattern: single round trip, no conflicts, no locks. Optimistic concurrency with version fields is designed for general-purpose resources, not shared counters.",
  "whyRejected": "Retry on 409 at 30% conflict rate means 30% extra round trips under load. At higher concurrency, the conflict rate and retry count increase super-linearly. Atomic SQL eliminates the conflict entirely.",
  "sourceAnchors": [
    "https://learn.microsoft.com/en-us/azure/architecture/patterns/optimistic-concurrency",
    "https://learn.microsoft.com/en-us/sql/relational-databases/sql-server-transaction-locking-and-row-versioning-guide"
  ],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-389

```json
{
  "rowId": "EXT-389",
  "sourceIds": ["CWE-TOP25"],
  "sourceTrustTier": 1,
  "datasetFamily": "arbitrateResultI_with_resultsContext",
  "domain": "E",
  "subdomain": "E2-ArbiterScoring",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A React application renders user-supplied content using `dangerouslySetInnerHTML`: `<div dangerouslySetInnerHTML={{__html: userComment}} />`. Users can submit HTML-formatted comments.",
    "b": "CWE-79 Cross-Site Scripting (XSS). Evaluate and propose the correct fix.",
    "c": "User comments are stored in the database and displayed to other users. A test payload: `<script>document.cookie</script>` or `<img src=x onerror=alert(1)>`."
  },
  "function": "arbitrateResultI_with_resultsContext",
  "acceptedOutput": "This is a Stored XSS vulnerability (CWE-79). `dangerouslySetInnerHTML` with unsanitised user input allows arbitrary HTML/JavaScript injection. Fix: (1) Use DOMPurify to sanitise HTML before rendering: `import DOMPurify from 'dompurify'; <div dangerouslySetInnerHTML={{__html: DOMPurify.sanitize(userComment)}} />`. DOMPurify removes script tags, event handlers, and other XSS vectors while preserving safe formatting (bold, italic, links). (2) Configure DOMPurify allowlist if needed: `DOMPurify.sanitize(html, {ALLOWED_TAGS: ['b', 'i', 'a', 'p'], ALLOWED_ATTR: ['href']})`. (3) Also sanitise on the server before storage (defence-in-depth) — client-side sanitisation alone is insufficient because attackers bypass the client. Server-side sanitisation with DOMPurify (via JSDOM) or HtmlSanitizer (.NET) before storing the comment prevents XSS even for direct API attacks that bypass the React client.",
  "rejectedOutput": "Escape the content using React's default text rendering instead of dangerouslySetInnerHTML. Remove the dangerouslySetInnerHTML prop and render as plain text.",
  "arbiters": ["cwe-79-xss-stored", "dompurify-sanitisation"],
  "whyAccepted": "DOMPurify sanitisation preserves safe HTML formatting while removing XSS vectors. Server-side sanitisation adds defence-in-depth against direct API attacks.",
  "whyRejected": "Removing dangerouslySetInnerHTML renders HTML tags as literal text — if users legitimately use formatting (bold, links), this degrades the user experience. DOMPurify allows safe formatting while eliminating XSS.",
  "sourceAnchors": [
    "https://cwe.mitre.org/data/definitions/79.html",
    "https://github.com/cure53/DOMPurify"
  ],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "APPROVED"
}
```

---

## EXT-390

```json
{
  "rowId": "EXT-390",
  "sourceIds": ["OWASP-ASVS"],
  "sourceTrustTier": 1,
  "datasetFamily": "userPrompt_to_machineCommands",
  "domain": "A",
  "subdomain": "A3-TopologyLifecycle",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API uses API keys for authentication (no user identity — machine-to-machine). API keys are stored in the database as plaintext strings. A security review flags this. Options: (A) Hash API keys with SHA-256 before storage. (B) Hash API keys with bcrypt before storage. (C) Store API keys plaintext but encrypt the column with AES-256.",
    "b": "API key validation on every request: client sends the key in Authorization header; server looks up the key in the database to identify the caller.",
    "c": "OWASP ASVS V2.10 covers machine credentials. API key lookup is in the hot path: 10,000 requests/second."
  },
  "function": "userPrompt_to_machineCommands",
  "acceptedOutput": "Use option A: SHA-256 hash of the API key. bcrypt (option B) is correct for passwords but incorrect for API keys: bcrypt is intentionally slow (100ms at cost-10) — hashing 10,000/second API key lookups adds 1,000 seconds of bcrypt computation/second, which is impossible. API key lookup pattern: (1) Client sends the full key in the header. (2) Server computes `SHA-256(key)` and looks up the hash in the database. (3) SHA-256 for a randomly generated API key (32+ bytes of entropy) is secure: the key space (2^256 possibilities from random bytes) is too large for brute-force regardless of hash speed. SHA-256 is fast enough for 10,000 req/sec. Column encryption (option C) adds AES overhead on every lookup without addressing the plaintext storage risk if the database is compromised. Store the SHA-256 hash only — never store the plaintext API key after generation. Display the key once at generation time.",
  "rejectedOutput": "Use bcrypt (option B). bcrypt is the correct password hashing algorithm and API keys should be treated like passwords.",
  "arbiters": ["owasp-asvs-machine-credentials", "sha256-api-key-lookup-performance"],
  "whyAccepted": "SHA-256 for high-entropy API keys is cryptographically secure and performant at 10,000 req/sec. bcrypt's intentional slowness makes it unsuitable for a hot-path lookup at this scale.",
  "whyRejected": "bcrypt is designed for passwords (low entropy, user-generated) to resist offline brute-force. API keys with 32+ bytes of entropy have no brute-force risk regardless of hash speed. bcrypt's cost is prohibitive at 10,000 lookups/second.",
  "sourceAnchors": [
    "https://owasp.org/www-project-application-security-verification-standard/",
    "https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html#api-keys"
  ],
  "licenseConstraint": "CC-BY-3.0",
  "trainingStatus": "APPROVED"
}
```
