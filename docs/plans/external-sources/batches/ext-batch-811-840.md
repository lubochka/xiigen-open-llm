# External Sources Batch 28 — EXT-551 through EXT-570

---

## EXT-551 | KUBERNETES-KEPS | Family 3 | Domain D

**Unit title:** Kubernetes PodDisruptionBudget (PDB) configuration for zero-downtime rolling updates
**Source URL:** https://kubernetes.io/docs/tasks/run-application/configure-pdb/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT551-F3-D",
  "sourceIds": ["EXT-551"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "availability-constraint-execution",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Deployment with 3 replicas has no PodDisruptionBudget (PDB). During a node drain for maintenance, Kubernetes evicts all 3 pods simultaneously, causing a service outage. The team needs to enforce that at least 2 replicas remain available during voluntary disruptions.",
    "b": "PodDisruptionBudget with minAvailable: 2 ensures the cluster refuses to evict pods if doing so would drop availability below 2. Node drain and cluster upgrades respect PDBs for voluntary disruptions. Involuntary disruptions (node crash) are not bounded by PDBs.",
    "c": "Without a PDB, kubectl drain evicts all pods on the node without availability consideration. The rolling update strategy (maxUnavailable: 1) applies only to Deployment updates, not to node drains."
  },
  "function": "Apply PDB to enforce availability during voluntary disruptions",
  "acceptedOutput": "Create a PodDisruptionBudget: spec.minAvailable: 2, spec.selector matching the Deployment's pod labels. The cluster will evict at most 1 pod at a time during drains, waiting for a replacement to become Ready before proceeding. Set maxUnavailable: 1 in the Deployment for update consistency.",
  "rejectedOutput": "Set maxUnavailable: 0 in the Deployment rolling update strategy to prevent downtime during updates.",
  "arbiters": ["execution-applier-arbiter", "availability-constraint-arbiter"],
  "whyAccepted": "PDB controls availability during node operations (drains, cluster upgrades) which are orthogonal to Deployment update strategy. maxUnavailable in the Deployment strategy only applies during kubectl rollout, not during node maintenance.",
  "whyRejected": "maxUnavailable: 0 in the Deployment strategy prevents rolling updates from terminating old pods before new ones are ready but has no effect on kubectl drain. The node drain bypasses the Deployment controller entirely.",
  "sourceAnchors": ["https://kubernetes.io/docs/tasks/run-application/configure-pdb/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-552 | BIGCODEBENCH | Family 6 | Domain F

**Unit title:** Python heapq.nlargest vs sorted()[-n:] for top-N selection performance
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT552-F6-F",
  "sourceIds": ["EXT-552"],
  "sourceTrustTier": 2,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "selection-algorithm-performance",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A scoring function needs to return the top 10 results from a list of 1 million scored items. The team uses sorted(scores, reverse=True)[:10] and observes it takes 800ms on benchmark.",
    "b": "heapq.nlargest(10, scores) uses a min-heap of size 10 (k), achieving O(n log k) complexity. For k=10 and n=1,000,000 this is ~20M operations. sorted() is O(n log n) — ~20M log(1M) ≈ 400M comparisons. For small k relative to n, heapq is significantly faster.",
    "c": "When k approaches n/2 or when the list is already nearly sorted, sorted() can outperform heapq.nlargest due to Timsort's adaptive optimization."
  },
  "function": "Select top-N selection algorithm for large dataset result retrieval",
  "acceptedOutput": "Use heapq.nlargest(10, scores, key=lambda x: x.score) for top-10 from 1M items. Expected speedup: 15-20× over sorted() for this k/n ratio. Verify with timeit for the specific data characteristics. If scores is a generator (not materialized list), heapq.nlargest streams it without full materialization.",
  "rejectedOutput": "Use sorted(scores, key=lambda x: x.score, reverse=True)[:10] for clarity and simplicity. The difference is negligible.",
  "arbiters": ["registry-storage-arbiter", "performance-selection-arbiter"],
  "whyAccepted": "heapq.nlargest is O(n log k) vs O(n log n) for sorted. At n=1M and k=10, this is approximately a 20× difference in comparisons. For a scoring hot path called thousands of times, this translates to meaningful latency reduction.",
  "whyRejected": "800ms is not negligible for a result-retrieval function called at query time. 'Clarity' arguments do not apply — both forms are one-liners of equivalent readability. Algorithmic complexity differences at this scale are observable.",
  "sourceAnchors": ["https://docs.python.org/3/library/heapq.html#heapq.nlargest"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-553 | SWE-BENCH | Family 5 | Domain E

**Unit title:** Django ORM select_related vs prefetch_related — N+1 query arbitration for related objects
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT553-F5-E",
  "sourceIds": ["EXT-553"],
  "sourceTrustTier": 2,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "orm-query-arbitration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Django view renders a list of 100 Order objects, each displaying the related Customer name. The view uses Order.objects.all() and accesses order.customer.name in the template, generating 101 queries (1 + 100 N+1).",
    "b": "select_related follows ForeignKey/OneToOne relationships using SQL JOIN — fetches all data in one query. prefetch_related uses a separate query per relationship and does the join in Python — suited for ManyToMany and reverse ForeignKey relations.",
    "c": "Customer is a ForeignKey on Order (each order has one customer). select_related is the correct primitive for ForeignKey relationships."
  },
  "function": "Arbitrate ORM fetching strategy to eliminate N+1 query pattern",
  "acceptedOutput": "Use Order.objects.select_related('customer').all(). Django executes one SQL JOIN query fetching all Orders and their related Customers. Template access to order.customer.name hits the prefetched cache, not the database.",
  "rejectedOutput": "Use Order.objects.prefetch_related('customer').all() to fetch customer data separately.",
  "arbiters": ["query-arbitration-arbiter", "performance-arbiter"],
  "whyAccepted": "select_related is optimal for ForeignKey (single-valued) relationships — one SQL JOIN delivers all data. prefetch_related for ForeignKey is valid but issues a second query and joins in Python, wasting the database's ability to perform the join efficiently.",
  "whyRejected": "prefetch_related('customer') on a ForeignKey generates two queries (SELECT orders, then SELECT customers WHERE id IN (...)) and performs the join in Python. For 100 orders it works but is semantically mismatched — the JOIN should be in SQL for ForeignKey relationships.",
  "sourceAnchors": ["https://docs.djangoproject.com/en/5.0/ref/models/querysets/#select-related"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-554 | PYTHON-PEPS | Family 1 | Domain A

**Unit title:** PEP-703 free-threaded CPython (no-GIL) — topology implications for CPU-bound parallelism
**Source URL:** https://peps.python.org/pep-0703/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT554-F1-A",
  "sourceIds": ["EXT-554"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "concurrency-topology-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Python 3.13 data processing pipeline uses multiprocessing.Pool to parallelize CPU-bound transformations, incurring serialization overhead for large numpy arrays passed between processes. The team evaluates switching to threading.Thread with Python 3.13t (free-threaded build).",
    "b": "PEP-703 removes the GIL (Global Interpreter Lock) in Python 3.13 experimental free-threaded mode (--disable-gil build). CPU-bound operations in threads can now run truly in parallel. Shared memory between threads eliminates serialization overhead.",
    "c": "Free-threaded CPython 3.13 is experimental. C extensions must be recompiled with Py_GIL_DISABLED support. NumPy has free-threaded support in 2.1+. Thread safety of shared mutable state must be managed explicitly."
  },
  "function": "Select CPU-bound parallelism topology for free-threaded CPython",
  "acceptedOutput": "Evaluate threading.Thread with free-threaded CPython 3.13t for CPU-bound numpy operations. Verify all C extensions (numpy, etc.) are compiled with free-threaded support. Replace process-based parallelism with thread-based for operations on shared large arrays. Add explicit locks for any shared mutable state that is not numpy array slice-partitioned.",
  "rejectedOutput": "Continue using multiprocessing.Pool. Free-threaded Python is experimental and not production-ready. The serialization overhead is acceptable.",
  "arbiters": ["topology-arbiter", "parallelism-strategy-arbiter"],
  "whyAccepted": "For large numpy array processing, thread-based parallelism with free-threaded CPython eliminates the pickle serialization cost that multiprocessing requires for inter-process data transfer. If the dependencies have free-threaded support, the performance gain for array-intensive work can be substantial.",
  "whyRejected": "Dismissing evaluation based on experimental status misses the opportunity to assess concrete performance gains. The recommendation should be conditional on dependency readiness, not a blanket rejection. Serialization overhead for large arrays is a real throughput bottleneck.",
  "sourceAnchors": ["https://peps.python.org/pep-0703/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-555 | OPENTELEMETRY-SPEC | Family 3 | Domain D

**Unit title:** OTel resource attributes vs span attributes — semantic convention placement for service identity
**Source URL:** https://opentelemetry.io/docs/specs/semconv/resource/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT555-F3-D",
  "sourceIds": ["EXT-555"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "telemetry-attribute-placement",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A team adds service.name, service.version, and deployment.environment as span attributes on every span. The tracing backend shows these attributes consuming significant storage across millions of spans.",
    "b": "OTel Resource represents the entity producing telemetry (service, process, host). Resource attributes are set once at SDK initialization and attached to all telemetry. Span attributes describe a specific operation. service.name, service.version, and deployment.environment are Resource semantic conventions, not span-level.",
    "c": "Resource attributes are stored once per resource, not per span. Adding them as span attributes multiplies storage by the span count."
  },
  "function": "Correct telemetry attribute placement for service identity information",
  "acceptedOutput": "Set service.name, service.version, and deployment.environment as Resource attributes at SDK initialization: ResourceBuilder.CreateDefault().AddAttributes(new[] { new KeyValuePair<string, object>(\"service.name\", \"order-service\"), ... }). Remove them from individual span attributes. The SDK automatically attaches the Resource to all telemetry.",
  "rejectedOutput": "Keep service.name as a span attribute since some backends display it more prominently on the span detail view than on the resource view.",
  "arbiters": ["execution-applier-arbiter", "telemetry-placement-arbiter"],
  "whyAccepted": "Resource attributes are the semantic convention location for service identity. Setting them at the Resource level reduces storage proportionally to span volume and ensures correct grouping in backends that use resource.service.name for service topology views.",
  "whyRejected": "Backend display preferences are not a justification for violating semantic conventions. The storage cost of duplicating resource-level information per span is measurable at scale. Backends can be configured to surface resource attributes in span views.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/semconv/resource/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-556 | DAPPER | Family 5 | Domain E

**Unit title:** Dapper transaction management — ambient TransactionScope vs explicit IDbTransaction arbitration
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT556-F5-E",
  "sourceIds": ["EXT-556"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "transaction-boundary-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Dapper-based repository needs to insert an Order and then insert OrderItems within the same transaction. The team debates using System.Transactions.TransactionScope (ambient) vs explicitly passing IDbTransaction to each Dapper query.",
    "b": "TransactionScope creates an ambient transaction that all database commands on the connection automatically enlist in. IDbTransaction requires explicit passing as a parameter to each Dapper Execute/Query call. TransactionScope can escalate to a distributed transaction (MSDTC) if multiple connections are opened.",
    "c": "The operations use a single SqlConnection opened within the scope. MSDTC escalation can occur if the connection is closed and reopened within the same TransactionScope."
  },
  "function": "Arbitrate transaction management strategy for multi-operation Dapper sequence",
  "acceptedOutput": "Use explicit IDbTransaction: open one SqlConnection, call connection.BeginTransaction(), pass the transaction to each Dapper call (connection.Execute(sql, param, transaction: tx)). Commit or rollback explicitly. This provides predictable single-connection transaction semantics with no MSDTC risk.",
  "rejectedOutput": "Use TransactionScope with the connection opened inside the scope. All Dapper operations automatically enlist.",
  "arbiters": ["transaction-arbitration-arbiter", "data-consistency-arbiter"],
  "whyAccepted": "Explicit IDbTransaction makes transaction boundaries visible in code, prevents accidental MSDTC escalation, and works correctly with connection pooling. Dapper's explicit transaction parameter ensures every operation is on the same transaction.",
  "whyRejected": "TransactionScope with a single connection works for simple cases but is fragile: if any code path opens a second connection or a library internally opens its own connection, the ambient transaction escalates to MSDTC, which may not be available and introduces significant overhead.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#transactions"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-557 | ANGULAR-DOCS | Family 8 | Domain G

**Unit title:** Angular NgRx Signal Store vs NgRx Component Store for local vs global state management
**Source URL:** https://ngrx.io/guide/signals/signal-store
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT557-F8-G",
  "sourceIds": ["EXT-557"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "state-management-pattern-learning",
  "rowClass": "migration-decision",
  "input": {
    "a": "An Angular application uses NgRx Component Store for feature-level state (per-component). The team evaluates migrating to NgRx Signal Store (introduced in NgRx 17) which uses Angular signals instead of RxJS BehaviorSubject internally.",
    "b": "NgRx Signal Store uses Angular's signal primitives for state, providing fine-grained reactivity without Zone.js. It has a functional, composable API (signalStore, withState, withMethods, withComputed). Component Store uses RxJS and requires manual subscription management.",
    "c": "Signal Store integrates with Angular 17 zoneless change detection. Component Store requires Zone.js for change detection triggers. Migration requires replacing ComponentStore<T> with signalStore() and RxJS operators with signal-based equivalents."
  },
  "function": "Record migration decision pattern from Component Store to Signal Store",
  "acceptedOutput": "Migrate feature stores to NgRx Signal Store for new Angular 17+ features. Replace ComponentStore<T> with signalStore(withState<T>({...})). Replace select() streams with computed() signals. Replace effect() with RxJS with Angular's effect() or withHooks. Keep existing Component Stores in legacy features to avoid risky rewrites without business value.",
  "rejectedOutput": "Keep NgRx Component Store for all features. Signal Store is too new and lacks the ecosystem of Component Store.",
  "arbiters": ["migration-decision-arbiter", "technology-adoption-arbiter"],
  "whyAccepted": "Signal Store is NgRx's strategic direction for Angular 17+. New features benefit from fine-grained reactivity and zoneless compatibility. The functional composition API is more testable than class-based Component Store. Existing stores can migrate incrementally.",
  "whyRejected": "Blocking adoption of Signal Store based on novelty ignores its production-ready status in NgRx 17. Component Store's Zone.js dependency becomes a liability as Angular moves toward zoneless change detection.",
  "sourceAnchors": ["https://ngrx.io/guide/signals/signal-store"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-558 | REACT-ADVANCED | Family 3 | Domain D

**Unit title:** React Server Actions vs API routes for form mutation execution in Next.js 14
**Source URL:** https://nextjs.org/docs/app/building-your-application/data-fetching/server-actions-and-mutations
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT558-F3-D",
  "sourceIds": ["EXT-558"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "mutation-execution-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Next.js 14 form submits user profile updates. The team evaluates using React Server Actions (use server directive) vs a traditional /api/profile PUT route handler.",
    "b": "Server Actions execute server-side code directly from a form action attribute or button onClick. Next.js handles serialization, CSRF protection via origin checking, and progressive enhancement (works without JavaScript). API routes require fetch() calls from client components and manual CSRF handling.",
    "c": "Server Actions colocate mutation logic with the component. The action receives FormData directly. Revalidation via revalidatePath() or revalidateTag() triggers cache invalidation after mutation."
  },
  "function": "Select mutation execution strategy for form submission in Next.js 14",
  "acceptedOutput": "Use a Server Action for the profile update form. Define async function updateProfile(formData: FormData) with 'use server'. Bind it to the form's action attribute. Use revalidatePath('/profile') after the update. Progressive enhancement works without client-side JavaScript.",
  "rejectedOutput": "Use a /api/profile PUT route. Client component submits with fetch('/api/profile', { method: 'PUT', body: JSON.stringify(data) }) from a useTransition-wrapped handler.",
  "arbiters": ["execution-applier-arbiter", "mutation-strategy-arbiter"],
  "whyAccepted": "Server Actions reduce boilerplate: no separate route file, no fetch() call, no manual JSON serialization, CSRF protection via Next.js origin checking, and native progressive enhancement. For simple form mutations, they are the Next.js 14 recommended pattern.",
  "whyRejected": "API routes add a network round-trip with manual JSON serialization, require client-side fetch orchestration, and need separate CSRF protection. They are appropriate for external API consumers but add unnecessary complexity for same-application form mutations.",
  "sourceAnchors": ["https://nextjs.org/docs/app/building-your-application/data-fetching/server-actions-and-mutations"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-559 | ASPNETCORE-DOCS-SAMPLES | Family 6 | Domain F

**Unit title:** ASP.NET Core IDistributedCache vs IMemoryCache for multi-instance deployment result storage
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT559-F6-F",
  "sourceIds": ["EXT-559"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "cache-storage-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An ASP.NET Core API caches expensive computation results using IMemoryCache. After scaling to 3 instances behind a load balancer, each instance has its own independent cache, causing cache misses on 2/3 of requests after a value is cached by one instance.",
    "b": "IMemoryCache is process-local — each process maintains its own in-memory dictionary. IDistributedCache (backed by Redis or SQL Server) is shared across all instances. Redis IDistributedCache serializes objects to byte arrays and has network round-trip overhead vs in-process access.",
    "c": "The computation takes 2 seconds. A Redis round-trip takes ~1ms. Cache hit rate with per-instance memory cache in a 3-instance deployment is approximately 33% (only hits the instance that cached it, assuming round-robin routing)."
  },
  "function": "Select cache storage for multi-instance deployment result persistence",
  "acceptedOutput": "Replace IMemoryCache with IDistributedCache backed by Redis (AddStackExchangeRedisCache). Serialize result objects to JSON byte arrays. All instances share the cache. The 1ms Redis overhead is negligible vs the 2s computation cost. Use sliding expiration appropriate to result freshness requirements.",
  "rejectedOutput": "Keep IMemoryCache and implement sticky sessions (session affinity) at the load balancer so each user always hits the same instance.",
  "arbiters": ["registry-storage-arbiter", "cache-consistency-arbiter"],
  "whyAccepted": "IDistributedCache with Redis provides cache consistency across all instances. Any instance populating the cache benefits all instances. No dependency on load balancer configuration.",
  "whyRejected": "Sticky sessions fix the cache miss problem for returning users but create uneven load distribution (hot instances accumulate many sticky users while others idle). They also fail when an instance restarts, losing all sticky sessions' cached data.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/performance/caching/distributed"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-560 | NESTJS-ADVANCED | Family 1 | Domain A

**Unit title:** NestJS module federation — Dynamic Module vs forRoot/forFeature pattern for plugin topology
**Source URL:** https://docs.nestjs.com/fundamentals/dynamic-modules
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT560-F1-A",
  "sourceIds": ["EXT-560"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "module-topology-configuration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS library module needs to be configurable at import time — different consuming applications need different Redis connection strings, prefix values, and TTL defaults. The team debates static module exports vs Dynamic Module with forRoot().",
    "b": "Static NestJS modules export providers with hardcoded configuration. Dynamic Modules return a DynamicModule object from a static forRoot(options) method, injecting the options object into providers. forFeature() is convention for module-level configuration (per-feature, lazily configured).",
    "c": "forRoot() is typically used for root-level singleton configuration (database connection). forFeature() is used for per-feature registration (specific entity repositories). The distinction is conventional, not enforced by NestJS."
  },
  "function": "Select module configuration topology for reusable configurable NestJS module",
  "acceptedOutput": "Implement a Dynamic Module with forRoot(options: CacheModuleOptions): DynamicModule. Provide the options object as a token: { provide: CACHE_MODULE_OPTIONS, useValue: options }. Inject CACHE_MODULE_OPTIONS into the service. Use forRootAsync() with useFactory for async configuration (loading from ConfigService).",
  "rejectedOutput": "Export the module as a static module with environment variable reads inside the service constructor. Each consuming app sets the appropriate env vars.",
  "arbiters": ["topology-arbiter", "module-configuration-arbiter"],
  "whyAccepted": "Dynamic Module with forRoot() makes configuration explicit at import time, enables type-safe option objects, supports async configuration via forRootAsync(), and allows the same module to be configured differently in test vs production without environment variable coupling.",
  "whyRejected": "Static modules reading environment variables directly couple the library to a specific configuration mechanism. Testing requires setting environment variables rather than passing option objects. Multiple configurations in one process are impossible.",
  "sourceAnchors": ["https://docs.nestjs.com/fundamentals/dynamic-modules"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-561 | IETF-RFCS | Family 7 | Domain C

**Unit title:** RFC-7519 JWT (JSON Web Token) claims validation — nbf, exp, iss, aud requirements
**Source URL:** https://www.rfc-editor.org/rfc/rfc7519
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT561-F7-C",
  "sourceIds": ["EXT-561"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "token-validation-translation",
  "rowClass": "negative-example",
  "input": {
    "a": "An API validates JWTs by verifying the signature and checking that exp (expiration) has not passed. It does not validate iss (issuer), aud (audience), or nbf (not before) claims. A security audit flags this as insufficient.",
    "b": "RFC-7519 Section 4.1 defines registered claims: iss (issuer), sub (subject), aud (audience), exp (expiration), nbf (not before), iat (issued at), jti (JWT ID). Section 4.1.3 specifies aud: if present in the token, the recipient MUST verify that aud contains a value that identifies the recipient.",
    "c": "Missing aud validation allows tokens issued for one service to be replayed against another service if both trust the same issuer. Missing iss validation accepts tokens from any signer whose key is in the trusted key set."
  },
  "function": "Translate JWT validation requirements for RFC-7519 compliance",
  "acceptedOutput": "Validate all security-relevant claims: exp (token not expired), nbf (token is now valid, if present), iss (matches expected issuer string), aud (contains this service's identifier). In .NET: TokenValidationParameters { ValidateIssuer = true, ValidIssuer = \"https://auth.example.com\", ValidateAudience = true, ValidAudience = \"order-service\", ValidateLifetime = true }.",
  "rejectedOutput": "Validate only exp. The iss and aud claims are informational and not security-critical for internal services on a private network.",
  "arbiters": ["security-arbiter", "token-validation-arbiter"],
  "whyAccepted": "RFC-7519 MUST language requires aud validation when present. Omitting iss and aud validation enables confused deputy attacks where a token valid for service A is replayed against service B. Private network assumptions do not prevent insider threats or compromised services.",
  "whyRejected": "Treating iss and aud as informational contradicts RFC-7519 Section 4.1.3's MUST requirement. Internal network boundaries do not prevent token replay between services that share a token issuer.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc7519#section-4.1"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-562 | GOOGLE-AIP | Family 3 | Domain D

**Unit title:** AIP-155 request ID for idempotent mutation execution in distributed APIs
**Source URL:** https://google.aip.dev/155
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT562-F3-D",
  "sourceIds": ["EXT-562"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "idempotent-mutation-execution",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A payment API's CreatePayment endpoint is called over an unreliable network. A client retries after a timeout, resulting in duplicate payments. The team needs a mechanism to make retried requests safe.",
    "b": "AIP-155 specifies a request_id field (string UUID) in the request. The server stores processed request IDs with their responses. On receiving a request with a previously-seen request_id, the server returns the cached response without re-executing the mutation.",
    "c": "The idempotency cache should be stored durably (database, not in-memory) to survive server restarts. TTL for cached request IDs should match the client's retry window (AIP-155 recommends 72 hours)."
  },
  "function": "Apply AIP-155 request ID for safe mutation retry execution",
  "acceptedOutput": "Accept request_id as a UUID in CreatePaymentRequest. Before executing, check the idempotency store (SELECT FROM idempotency_keys WHERE request_id = @id). If found, return the stored response. If not found, execute the payment, then INSERT into idempotency_keys with the request_id, response, and expiry. Use database-level unique constraint on request_id to handle race conditions.",
  "rejectedOutput": "Implement optimistic locking on the payment record using a client-provided idempotency_key field on the Payment resource itself.",
  "arbiters": ["execution-applier-arbiter", "idempotency-arbiter"],
  "whyAccepted": "AIP-155 request_id is a transport-level idempotency mechanism — it deduplicates at the request boundary before the operation executes. The database unique constraint prevents duplicate processing under concurrent retries.",
  "whyRejected": "Optimistic locking on the resource prevents concurrent writes to the same resource but does not handle the retry case where the first request succeeded but the response was lost — the client retry still creates a second payment with a different resource key.",
  "sourceAnchors": ["https://google.aip.dev/155"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-563 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** Constitutional AI (CAI) self-critique vs RLHF for alignment — training pipeline comparison
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT563-F8-G",
  "sourceIds": ["EXT-563"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "alignment-training-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team is building an alignment training pipeline. They compare Constitutional AI (CAI) — where the model critiques and revises its own outputs based on principles — against standard RLHF (Reinforcement Learning from Human Feedback) using annotator preference data.",
    "b": "CAI uses a two-phase approach: (1) SFT (Supervised Fine-Tuning) on model self-revisions guided by a constitution; (2) RLAIF (Reinforcement Learning from AI Feedback) using a reward model trained on AI-generated preference pairs. RLHF uses human-annotated preference pairs for reward model training. CAI scales better (no human annotation bottleneck) but may encode the base model's biases in the constitution evaluation.",
    "c": "oasst1 provides human preference data usable for both: RLHF uses it directly as human comparison pairs; CAI can use it to validate the constitutional principles against human judgment."
  },
  "function": "Select alignment training strategy for scalable RLHF pipeline",
  "acceptedOutput": "Use a hybrid approach: train initial reward model on oasst1 human preferences (grounding alignment in human judgment). Use CAI constitutional principles to generate additional synthetic preference pairs for topics with sparse human coverage. Validate synthetic pairs against the human-preference-trained reward model before including them in training.",
  "rejectedOutput": "Use pure CAI with no human preference data. The constitution provides sufficient alignment signal without human annotation costs.",
  "arbiters": ["learning-pipeline-arbiter", "alignment-quality-arbiter"],
  "whyAccepted": "Human preference data grounds the reward model in actual human values. CAI extends coverage to topics where human annotation is sparse or expensive. Validation of synthetic pairs against human-grounded reward model filters constitutional biases.",
  "whyRejected": "Pure CAI risks encoding the base model's existing biases into the constitution evaluation. Without human preference grounding, the model optimizes for the constitution's principles which may not fully capture the diversity of human values.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-564 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba script normalization for multilingual datasets — Unicode NFC vs NFD in text preprocessing
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT564-F2-C",
  "sourceIds": ["EXT-564"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "text-normalization-preprocessing",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A multilingual preprocessing pipeline concatenates Tatoeba sentences from different contributors without Unicode normalization. String equality checks fail for sentences that appear identical but differ in Unicode composition form (e.g., é as U+00E9 vs e + U+0301).",
    "b": "Unicode NFC (Normalization Form Canonical Composition) represents characters in composed form (é = U+00E9). NFD (Normalization Form Canonical Decomposition) decomposes to base character + combining mark (e + U+0301). Both are valid Unicode representations of the same glyph but compare as unequal byte sequences.",
    "c": "Tatoeba contributors use different operating systems and input methods that produce different normalization forms. Deduplication and exact-match filtering without normalization miss duplicates and produce false mismatches."
  },
  "function": "Apply Unicode normalization to multilingual text preprocessing pipeline",
  "acceptedOutput": "Apply unicodedata.normalize('NFC', text) to all Tatoeba sentences at ingestion before any comparison, deduplication, or tokenization. NFC is the preferred form for storage and comparison (consistent with most web standards). NFKC (Compatibility Composition) additionally normalizes compatibility characters (e.g., ﬁ → fi) — use if tokenization requires canonical character forms.",
  "rejectedOutput": "Compare sentences using case-insensitive string comparison. Unicode normalization is an edge case affecting only a small percentage of sentences.",
  "arbiters": ["translation-quality-arbiter", "preprocessing-correctness-arbiter"],
  "whyAccepted": "NFC normalization ensures that semantically identical strings compare as equal. Deduplication and quality filters operate correctly. The normalization cost is O(n) per character and negligible compared to tokenization.",
  "whyRejected": "Case-insensitive comparison does not address Unicode composition differences. é (U+00E9) and é (e + U+0301) compare as unequal even with case folding. The 'small percentage' claim is false for languages like French, German, and Arabic where diacritics are common.",
  "sourceAnchors": ["https://unicode.org/reports/tr15/"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-565 | VUE-DOCS | Family 1 | Domain A

**Unit title:** Vue 3 Pinia store composition — stores calling other stores vs cross-store subscription topology
**Source URL:** https://pinia.vuejs.org/cookbook/composing-stores.html
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT565-F1-A",
  "sourceIds": ["EXT-565"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "store-composition-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 application has a cartStore that needs the current user's shipping preferences from userStore to calculate shipping costs. The team debates: cartStore directly calls useUserStore() inside actions vs cartStore subscribes to userStore state changes and maintains a local copy.",
    "b": "Pinia stores are composable — calling useUserStore() inside a cartStore action accesses the singleton store directly. $subscribe() allows a store to react to another store's state mutations. Direct access reads current state at call time; subscription maintains a derived copy.",
    "c": "Circular store dependencies (cartStore → userStore → cartStore) cause infinite recursion and should be avoided. Direct access is simpler but requires the dependency to be explicitly imported."
  },
  "function": "Select Pinia cross-store dependency topology for reactive store composition",
  "acceptedOutput": "Use direct store access inside actions: const userStore = useUserStore() called inside the calculateShipping action. This reads the current user preferences at calculation time without maintaining a copy. Avoid $subscribe() for derived values — use computed properties on the consuming store instead: const shippingPrefs = computed(() => useUserStore().shippingPreferences).",
  "rejectedOutput": "Use $subscribe() on userStore inside cartStore's setup to copy shippingPreferences into cartStore state whenever userStore changes.",
  "arbiters": ["topology-arbiter", "store-composition-arbiter"],
  "whyAccepted": "Direct store access reads current state without synchronization lag or state duplication. computed() on cross-store state provides reactive derivation without subscription management. The Pinia documentation explicitly recommends this pattern for cross-store access.",
  "whyRejected": "$subscribe() creates state duplication between stores, adds synchronization complexity, and can cause timing issues where cartStore uses stale copied preferences if the subscription fires after the action reads. It's appropriate for side effects (logging, persistence) not for derived state.",
  "sourceAnchors": ["https://pinia.vuejs.org/cookbook/composing-stores.html"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-566 | EFCORE-SOURCE | Family 3 | Domain D

**Unit title:** EF Core value converters for domain value objects — Money, Email, strongly-typed IDs
**Source URL:** https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT566-F3-D",
  "sourceIds": ["EXT-566"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "domain-value-object-mapping",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A domain model uses strongly-typed IDs (OrderId, CustomerId as record structs wrapping Guid) and a Money value object (decimal Amount + string Currency). EF Core cannot map these to database columns without configuration.",
    "b": "EF Core value converters (HasConversion<T>()) serialize value objects to primitive database types. For strongly-typed IDs: HasConversion<OrderId, Guid>(id => id.Value, g => new OrderId(g)). For Money: requires two columns (Amount decimal, Currency string) using OwnsOne() with column configuration.",
    "c": "Primitive obsession (using Guid directly) loses domain semantics and allows passing OrderId where CustomerId is expected. Strongly-typed IDs enforce type safety at compile time."
  },
  "function": "Configure EF Core value converters for domain value object persistence",
  "acceptedOutput": "For strongly-typed IDs: entity.Property(e => e.OrderId).HasConversion(id => id.Value, g => new OrderId(g)). For Money: entity.OwnsOne(e => e.Price, m => { m.Property(p => p.Amount).HasColumnName(\"Price\"); m.Property(p => p.Currency).HasColumnName(\"Currency\"); }). Register converters in OnModelCreating.",
  "rejectedOutput": "Map all domain types to primitives in the repository layer. The domain model uses Guid and decimal directly; the repository constructor arguments perform the conversion.",
  "arbiters": ["execution-applier-arbiter", "domain-mapping-arbiter"],
  "whyAccepted": "EF Core value converters keep the domain model as-is (with strongly-typed IDs and value objects) while managing persistence transparently. Queries using strongly-typed ID properties generate correct parameterized SQL.",
  "whyRejected": "Primitive domain types defeat compile-time type safety — the compiler cannot prevent passing a CustomerId Guid where an OrderId is expected. Repository-layer conversion duplicates domain logic and leaks persistence concerns into the repository constructor.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-567 | DOTNET-MICROSERVICES | Family 7 | Domain C

**Unit title:** .NET gRPC vs REST for internal microservice communication — translation interface selection
**Source URL:** https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/direct-client-to-microservice-communication-versus-the-api-gateway-pattern
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT567-F7-C",
  "sourceIds": ["EXT-567"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "service-communication-protocol-selection",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "Two .NET microservices need to communicate: an Order service calling an Inventory service to check stock levels. The team evaluates HTTP REST with System.Net.Http.HttpClient vs gRPC with Grpc.AspNetCore.",
    "b": "gRPC uses Protocol Buffers (binary serialization) over HTTP/2, providing lower latency and smaller payloads than JSON over HTTP/1.1. It generates strongly-typed client/server stubs from .proto files. REST with JSON is text-based, human-readable, and compatible with HTTP/1.1 and browser clients.",
    "c": "Internal service-to-service communication does not require browser compatibility. The .NET gRPC client supports deadlines, cancellation, and streaming natively."
  },
  "function": "Select communication protocol for internal .NET microservice-to-microservice calls",
  "acceptedOutput": "Use gRPC for internal Inventory service calls. Define a CheckStock RPC in inventory.proto, generate C# stubs, and inject the typed InventoryClient. Benefits: binary serialization reduces payload size ~30-50%, HTTP/2 multiplexing enables concurrent requests over one connection, strongly-typed contracts prevent interface drift.",
  "rejectedOutput": "Use HTTP REST with HttpClient. JSON is easier to debug with Fiddler/Postman and does not require proto file management.",
  "arbiters": ["api-translation-arbiter", "performance-protocol-arbiter"],
  "whyAccepted": "For high-frequency internal service calls, gRPC's binary serialization and HTTP/2 multiplexing provide measurable latency and throughput advantages. Proto contracts enforce schema compatibility with compile-time checks.",
  "whyRejected": "Debuggability preference is a development-time concern solvable with gRPC reflection and tools like grpcurl. It should not determine the production protocol for a performance-sensitive internal communication path.",
  "sourceAnchors": ["https://grpc.io/docs/languages/csharp/basics/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-568 | MICROSOFT-LEARN-ARCH | Family 8 | Domain G

**Unit title:** Azure Well-Architected Framework reliability pillar — multi-region vs zone-redundant deployment learning
**Source URL:** https://learn.microsoft.com/en-us/azure/well-architected/reliability/
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT568-F8-G",
  "sourceIds": ["EXT-568"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "reliability-pattern-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A business-critical Azure application needs 99.99% availability SLA (Service Level Agreement). The architect evaluates zone-redundant deployment (3 Availability Zones in one region) vs active-active multi-region deployment.",
    "b": "Zone-redundant deployment protects against data center failures within a region (Availability Zone outage). Multi-region active-active protects against full regional outages. Azure SLA for zone-redundant services (App Service, SQL Database, etc.) is typically 99.99%. Multi-region active-active adds cross-region replication latency and conflict resolution complexity.",
    "c": "Regional outages are rare but occur. Azure Traffic Manager or Front Door provides global load balancing for active-active. Data consistency across regions requires conflict resolution strategy (last-write-wins, CRDT, etc.)."
  },
  "function": "Record reliability topology decision: zone-redundant vs multi-region for 99.99% SLA",
  "acceptedOutput": "Zone-redundant deployment achieves 99.99% SLA for most business-critical applications without the complexity of multi-region active-active. Implement multi-region only if: (1) the 99.99% SLA is insufficient (requiring 99.999%), (2) data residency requires regional failover capability, or (3) latency requirements mandate serving users from geographically proximate regions.",
  "rejectedOutput": "Deploy active-active multi-region for all business-critical applications. Any critical system must tolerate regional outages.",
  "arbiters": ["reliability-learning-arbiter", "cost-complexity-trade-off-arbiter"],
  "whyAccepted": "Zone-redundant deployment satisfies 99.99% SLA requirements at significantly lower cost and complexity than multi-region. Regional Azure outages historically occur at a frequency that places applications well within the 99.99% threshold with zone redundancy alone.",
  "whyRejected": "Blanket multi-region requirements ignore the substantial operational complexity: cross-region replication, conflict resolution, higher latency, doubled egress costs, and more complex deployment pipelines. These costs are justified only when requirements genuinely demand beyond 99.99%.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/well-architected/reliability/"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-569 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-22 Path Traversal in ASP.NET Core file serving — Path.GetFullPath canonicalization
**Source URL:** https://cwe.mitre.org/data/definitions/22.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT569-F5-E",
  "sourceIds": ["EXT-569"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "path-traversal-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core endpoint serves user-uploaded files: var filePath = Path.Combine(uploadsDir, userFileName); return File(System.IO.File.ReadAllBytes(filePath), \"application/octet-stream\"). A user submits fileName = \"../../appsettings.json\" and receives the application configuration file.",
    "b": "Path.Combine does not canonicalize paths — if userFileName begins with a directory separator or contains .., it can navigate outside the intended directory. Path.GetFullPath() resolves the canonical absolute path. Checking that the canonical path starts with the allowed base directory prevents traversal.",
    "c": "CWE-22 Path Traversal is in the OWASP (Open Web Application Security Project) Top 10 and CWE Top 25. Affected files may include appsettings.json (connection strings), private keys, or any file readable by the process."
  },
  "function": "Detect and remediate path traversal vulnerability in file serving endpoint",
  "acceptedOutput": "Canonicalize and validate: var fullPath = Path.GetFullPath(Path.Combine(uploadsDir, userFileName)); if (!fullPath.StartsWith(Path.GetFullPath(uploadsDir) + Path.DirectorySeparatorChar)) { return BadRequest(\"Invalid file path\"); } return PhysicalFile(fullPath, \"application/octet-stream\"). Additionally, store files by a server-generated GUID name rather than user-provided names.",
  "rejectedOutput": "Sanitize userFileName by replacing \"..\" with empty string: userFileName.Replace(\"..\", \"\"). This removes the traversal sequences.",
  "arbiters": ["security-arbiter", "path-validation-arbiter"],
  "whyAccepted": "Path.GetFullPath() resolves all relative components, symlinks, and separator variations to a canonical absolute path. Checking the prefix against the base directory is a structurally sound defense that handles all traversal encoding variants.",
  "whyRejected": "String replacement is a denylist approach. Encoding variants (.%2e%2f, ..%5c, URL-encoded sequences) bypass simple .. replacement. Canonicalization-then-prefix-check handles all variants without needing an exhaustive denylist.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/22.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-570 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V8.3 sensitive data in logs — PII (Personally Identifiable Information) scrubbing arbitration
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT570-F5-E",
  "sourceIds": ["EXT-570"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "sensitive-data-logging-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core API logs full request bodies for debugging: logger.LogInformation(\"Request: {Body}\", JsonSerializer.Serialize(request)). Requests contain user email, phone number, and partial credit card numbers. The logs are shipped to a centralized logging service accessible to the support team.",
    "b": "OWASP ASVS V8.3.4 requires that sensitive data (PII, credentials, payment data) is not logged. Logs accessible to support staff or log aggregators are a data breach risk — log storage is typically less secured than the primary database.",
    "c": "Structured logging with Serilog or Microsoft.Extensions.Logging supports destructuring policies that can redact specific properties before they reach any log sink."
  },
  "function": "Arbitrate PII scrubbing strategy for structured logging compliance with ASVS V8.3",
  "acceptedOutput": "Implement a Serilog destructuring policy or ILogger enricher that replaces sensitive fields with redacted markers before serialization. Use an allowlist of safe-to-log properties per request type rather than logging the full body. For debugging needs, log a correlation ID (RequestId) that can be cross-referenced with a secure audit store accessed by authorized personnel only.",
  "rejectedOutput": "Restrict log access to senior engineers only. Continue logging full request bodies since the logs are already access-controlled.",
  "arbiters": ["security-arbiter", "privacy-compliance-arbiter"],
  "whyAccepted": "Scrubbing at the logging layer prevents PII from ever entering log storage, regardless of access controls. Defense in depth: access controls can be misconfigured; scrubbing is structural. Correlation IDs satisfy debugging needs without retaining personal data.",
  "whyRejected": "Access control on logs is a compensating control, not a primary defense. Log systems are frequently misconfigured, exported to third-party SIEM (Security Information and Event Management) tools, or backed up insecurely. ASVS V8.3.4 requires that PII is not logged, not merely that log access is restricted.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
