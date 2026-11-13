# External Sources Batch 29 — EXT-571 through EXT-590

---

## EXT-571 | KUBERNETES-KEPS | Family 5 | Domain E

**Unit title:** Kubernetes RBAC (Role-Based Access Control) least-privilege — ClusterRole vs Role scope arbitration
**Source URL:** https://kubernetes.io/docs/reference/access-authn-authz/rbac/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT571-F5-E",
  "sourceIds": ["EXT-571"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "rbac-privilege-arbitration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A developer grants a service account a ClusterRole with get/list/watch on all pod resources (resources: [\"pods\"], verbs: [\"get\",\"list\",\"watch\"]) so the service can monitor its own namespace's pods. A security review flags excessive privilege.",
    "b": "ClusterRole + ClusterRoleBinding grants permissions across all namespaces. Role + RoleBinding restricts permissions to a single namespace. A service needing to watch pods only in its own namespace requires a Role (namespace-scoped), not a ClusterRole.",
    "c": "The principle of least privilege requires granting the minimum scope necessary. A ClusterRole allows the service account to list pods in any namespace including kube-system, which may expose sensitive system pod information."
  },
  "function": "Arbitrate RBAC scope to enforce namespace-level least privilege",
  "acceptedOutput": "Create a namespace-scoped Role (not ClusterRole) in the service's namespace with get/list/watch on pods. Bind it with a RoleBinding to the service account. The service can only list pods in its own namespace. For cross-namespace monitoring, use a separate ClusterRole with explicit justification per namespace.",
  "rejectedOutput": "Keep the ClusterRole but add a ResourceNames restriction to limit it to pods with a specific label selector.",
  "arbiters": ["security-arbiter", "rbac-scope-arbiter"],
  "whyAccepted": "Namespace-scoped Role is the structurally correct primitive. It cannot be escalated to cluster-wide access regardless of future ClusterRoleBinding additions. Least privilege is enforced by the API server, not by application-level label filtering.",
  "whyRejected": "ResourceNames in RBAC refers to specific named resources, not label selectors — it is not an effective filter for dynamic pod names. ClusterRole scope remains, providing cross-namespace access that label restrictions cannot prevent at the API server level.",
  "sourceAnchors": ["https://kubernetes.io/docs/reference/access-authn-authz/rbac/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-572 | BIGCODEBENCH | Family 3 | Domain D

**Unit title:** Python dataclass inheritance — field ordering and default value constraints
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT572-F3-D",
  "sourceIds": ["EXT-572"],
  "sourceTrustTier": 2,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "dataclass-inheritance-semantics",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Python developer defines a base dataclass with a field that has a default value, then a subclass that adds a required field (no default). Python raises TypeError: non-default argument 'required_field' follows default argument.",
    "b": "Python dataclass inheritance concatenates parent and child fields in MRO (Method Resolution Order) order. If the parent has a field with a default and the child adds a required field (no default), the required field appears after the defaulted field, violating Python's function signature rule.",
    "c": "Solutions: (1) give all parent fields defaults, (2) use field(default_factory=...) for parent fields that need mutable defaults, (3) use KW_ONLY sentinel in Python 3.10+ to force all remaining fields to be keyword-only, (4) use __init_subclass__ or __post_init__ for validation instead."
  },
  "function": "Detect and resolve dataclass inheritance field ordering constraint violation",
  "acceptedOutput": "In Python 3.10+: add a KW_ONLY sentinel in the base class: @dataclass class Base: name: str = ''; _: KW_ONLY; id: int = 0. All fields after _ are keyword-only and can appear after defaults. For pre-3.10: restructure so required fields are in the subclass only, or make parent fields optional with None defaults and validate in __post_init__.",
  "rejectedOutput": "Move the required field from the subclass to the base class so it appears before the default fields.",
  "arbiters": ["execution-applier-arbiter", "dataclass-structure-arbiter"],
  "whyAccepted": "KW_ONLY is the Python 3.10+ idiomatic solution for mixed required/optional fields in dataclass hierarchies. It preserves the semantic distinction between required and optional while satisfying the interpreter's field ordering constraint.",
  "whyRejected": "Moving the required field to the base class changes the semantic design — it adds an unnecessary field to a base class that may not require it. It also does not generalize to cases where the required field is genuinely subclass-specific.",
  "sourceAnchors": ["https://docs.python.org/3/library/dataclasses.html"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-573 | SWE-BENCH | Family 6 | Domain F

**Unit title:** Python SQLAlchemy relationship lazy vs eager loading — N+1 detection in ORM queries
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT573-F6-F",
  "sourceIds": ["EXT-573"],
  "sourceTrustTier": 2,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "orm-loading-strategy-selection",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A FastAPI endpoint returns a list of orders with their line items. The SQLAlchemy model defines relationship('LineItem', lazy='select') (default). The endpoint returns 100 orders, generating 101 SQL queries (1 for orders + 1 per order for line items).",
    "b": "SQLAlchemy lazy='select' loads related objects on first access via a separate SELECT. lazy='joined' uses LEFT OUTER JOIN to fetch related objects in the same query. lazy='subquery' uses a correlated subquery. selectinload() (eager) uses IN clause to batch-load related objects for all parent rows.",
    "c": "For one-to-many relationships with a large number of related objects per parent, selectinload() is generally preferred over 'joined' to avoid Cartesian product row multiplication."
  },
  "function": "Select SQLAlchemy loading strategy to eliminate N+1 query pattern",
  "acceptedOutput": "Use selectinload for one-to-many: query = session.query(Order).options(selectinload(Order.line_items)).all(). SQLAlchemy emits one query for orders, then one IN-clause query for all line items: SELECT * FROM line_items WHERE order_id IN (1, 2, ..., 100). Total: 2 queries regardless of order count.",
  "rejectedOutput": "Change relationship to lazy='joined'. This adds a JOIN to the order query, fetching everything in one query.",
  "arbiters": ["registry-storage-arbiter", "query-loading-arbiter"],
  "whyAccepted": "selectinload produces 2 queries with no Cartesian product. 100 orders × 10 line items = 1000 line item rows across 2 queries. No row multiplication from JOIN.",
  "whyRejected": "lazy='joined' produces one query but with a Cartesian product: 100 orders × 10 line items = 1000 rows in one query. For 100 orders this is manageable but for larger sets or multiple joined relationships, the row multiplication grows multiplicatively.",
  "sourceAnchors": ["https://docs.sqlalchemy.org/en/20/orm/queryguide/relationships.html"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-574 | PYTHON-PEPS | Family 7 | Domain C

**Unit title:** PEP-484 TypedDict for structured dictionary type annotation in Python APIs
**Source URL:** https://peps.python.org/pep-0589/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT574-F7-C",
  "sourceIds": ["EXT-574"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "structured-dict-type-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Python function returns a dict with known keys: {'user_id': int, 'email': str, 'role': str}. The return type is annotated as dict[str, Any]. Callers cannot determine the structure from the type annotation and type checkers cannot validate key access.",
    "b": "TypedDict (PEP-589, typing module) defines a dictionary type with specific key-value type constraints: class UserDict(TypedDict): user_id: int; email: str; role: str. Type checkers validate that all required keys are present and values have correct types.",
    "c": "TypedDict is structurally typed — any dict with the correct keys and types satisfies the TypedDict type, enabling gradual adoption without converting existing dict returns."
  },
  "function": "Translate dict return type to TypedDict for structured API annotation",
  "acceptedOutput": "Define class UserDict(TypedDict): user_id: int; email: str; role: str. Annotate the function return type as UserDict. Callers receive type-checker validation on key access. Use total=False for optional keys or NotRequired[T] for partial TypedDicts in Python 3.11+.",
  "rejectedOutput": "Keep dict[str, Any]. Document the structure in a docstring. Type annotations are optional and the team should not over-engineer typing for internal functions.",
  "arbiters": ["api-translation-arbiter", "type-safety-arbiter"],
  "whyAccepted": "TypedDict provides static validation of dictionary structure — type checkers catch missing keys, wrong value types, and typos in key names at development time. It is zero-runtime-cost (no class instantiation) and compatible with existing dict values.",
  "whyRejected": "dict[str, Any] erases all structural type information. Callers must read documentation or source to determine keys and types. Type checkers cannot flag result['user_idd'] (typo) as an error. For a returned dictionary with known structure, TypedDict has no downside.",
  "sourceAnchors": ["https://peps.python.org/pep-0589/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-575 | OPENTELEMETRY-SPEC | Family 1 | Domain A

**Unit title:** OTel Collector pipeline topology — fan-out receivers to multiple exporters with processing
**Source URL:** https://opentelemetry.io/docs/collector/configuration/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT575-F1-A",
  "sourceIds": ["EXT-575"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "collector-pipeline-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An organization needs to send traces to both Jaeger (for developers) and an S3 bucket (for long-term archival), while sampling at 10% for Jaeger but 100% for archival. The team needs to configure the OTel Collector pipeline topology.",
    "b": "OTel Collector pipelines define receivers → processors → exporters. Multiple pipelines can share receivers. A single pipeline cannot apply different sampling rates to different exporters — the processor chain applies uniformly before fan-out to all exporters.",
    "c": "Solution: define two separate trace pipelines sharing the same OTLP receiver. Pipeline 1: receiver=otlp → processor=[probabilistic_sampler(10%)] → exporter=jaeger. Pipeline 2: receiver=otlp → processor=[] → exporter=s3."
  },
  "function": "Design OTel Collector fan-out pipeline topology for differential sampling",
  "acceptedOutput": "Configure two separate pipelines in the Collector config: traces/sampled (10% probabilistic sampler → Jaeger) and traces/full (no sampling → S3 exporter). Both pipelines use the same otlp receiver. The Collector fan-outs from the receiver to both pipelines independently, applying each pipeline's processors independently.",
  "rejectedOutput": "Configure one pipeline with both exporters and a 10% sampler. Use S3 exporter with a custom filter to re-include dropped traces.",
  "arbiters": ["topology-arbiter", "pipeline-configuration-arbiter"],
  "whyAccepted": "Separate pipelines share the receiver but apply independent processor chains. Traces reach both exporters from the common receiver; each pipeline's sampling decision is independent. This is the standard OTel Collector pattern for differential routing.",
  "whyRejected": "A single pipeline with one sampler applies the same sampling decision to all exporters. There is no mechanism to re-include traces dropped by the sampler for a specific exporter within the same pipeline.",
  "sourceAnchors": ["https://opentelemetry.io/docs/collector/configuration/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-576 | DAPPER | Family 3 | Domain D

**Unit title:** Dapper multi-mapping with custom type split — complex JOIN result mapping execution
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT576-F3-D",
  "sourceIds": ["EXT-576"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "multi-type-join-result-mapping",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A query JOINs Orders, Customers, and Addresses (3 tables). The result needs mapping to an Order object with a nested Customer (which has a nested Address). Manual mapping in a foreach loop is verbose. The team evaluates Dapper multi-mapping vs manual construction.",
    "b": "Dapper Query<T1, T2, T3, TReturn> with splitOn allows mapping multi-type JOINs. Dapper splits the result row into types based on the first column of each type (Id by default, or a custom splitOn column name). The map function receives instances of all types and returns the assembled object graph.",
    "c": "SplitOn requires that each type's Id column appears in the SELECT in the correct order. Ambiguous column names (multiple tables have 'Id') require column aliasing in the SQL."
  },
  "function": "Apply Dapper multi-mapping for 3-table JOIN result construction",
  "acceptedOutput": "Use Query<Order, Customer, Address, Order> with splitOn: connection.Query<Order, Customer, Address, Order>(sql, (order, customer, address) => { customer.Address = address; order.Customer = customer; return order; }, splitOn: \"CustomerId,AddressId\"). Alias the SELECT to ensure CustomerId and AddressId columns mark the split boundaries.",
  "rejectedOutput": "Use Query<dynamic> and manually construct objects from the dynamic result in a foreach loop.",
  "arbiters": ["execution-applier-arbiter", "result-mapping-arbiter"],
  "whyAccepted": "Dapper multi-mapping produces typed objects with correct nesting in one pass over the result set. The map lambda explicitly controls the assembly of the object graph. Type safety is preserved via generic constraints.",
  "whyRejected": "dynamic results lose all type safety. Property access typos throw at runtime. The manual foreach loop is verbose and duplicates the mapping logic that Dapper's multi-mapping handles declaratively.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper#multi-mapping"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-577 | ANGULAR-DOCS | Family 7 | Domain C

**Unit title:** Angular 17 deferrable views (@defer) for progressive loading of non-critical UI
**Source URL:** https://angular.dev/guide/defer
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT577-F7-C",
  "sourceIds": ["EXT-577"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "progressive-loading-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular dashboard page eagerly loads a heavy analytics chart component that is below the fold. Initial page load is slow because the chart's JavaScript bundle is included in the main chunk.",
    "b": "Angular 17 @defer blocks lazy-load components when a trigger condition is met (viewport, interaction, idle, timer, or immediately). @placeholder shows content before loading. @loading shows during the async load. @error shows if loading fails. The deferred component's module is code-split automatically.",
    "c": "Without @defer, lazy loading requires complex routing configuration or manual import() calls with ViewContainerRef. @defer is declarative template syntax for the same outcome."
  },
  "function": "Translate progressive loading strategy for below-fold Angular components",
  "acceptedOutput": "Wrap the analytics chart with @defer (on viewport): @defer (on viewport) { <analytics-chart /> } @placeholder { <div class=\"chart-skeleton\"></div> } @loading { <spinner /> } @error { <p>Chart unavailable</p> }. Angular code-splits the chart module and loads it when it enters the viewport.",
  "rejectedOutput": "Use Angular's lazy-loaded routing to move the chart to a separate route that loads on navigation.",
  "arbiters": ["api-translation-arbiter", "loading-strategy-arbiter"],
  "whyAccepted": "@defer is the correct tool for within-page progressive loading. It code-splits without requiring route changes, supports multiple trigger types (viewport, idle), and provides built-in loading/error states. The dashboard remains a single route.",
  "whyRejected": "Lazy-loaded routing requires creating a separate route for the chart, forcing a navigation to view it. This fundamentally changes the dashboard's UX and information architecture to solve a bundle size problem.",
  "sourceAnchors": ["https://angular.dev/guide/defer"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-578 | REACT-ADVANCED | Family 6 | Domain F

**Unit title:** React Query (TanStack Query) vs SWR for data fetching and cache management result storage
**Source URL:** https://tanstack.com/query/latest/docs/framework/react/overview
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT578-F6-F",
  "sourceIds": ["EXT-578"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "data-fetching-cache-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A React application needs data fetching with: (1) background refetch on window focus, (2) optimistic updates for mutations, (3) pagination with cursor support, (4) dependent queries (query B depends on query A's result). The team evaluates TanStack Query vs SWR.",
    "b": "TanStack Query provides: useQuery, useMutation with optimistic update helpers (onMutate, onError rollback), useInfiniteQuery for cursor pagination, enabled option for dependent queries, and a DevTools panel. SWR is simpler, with useSWR for fetching and useSWRMutation for mutations but less built-in mutation state management.",
    "c": "Both libraries handle background refetch on focus, stale-while-revalidate caching, and deduplication."
  },
  "function": "Select data fetching library for complex mutation and pagination result storage needs",
  "acceptedOutput": "Use TanStack Query for applications requiring optimistic updates with rollback, cursor-based infinite pagination, and dependent query orchestration. The useMutation optimistic update pattern (onMutate → update cache → onError → rollback) is built-in. useInfiniteQuery with getNextPageParam handles cursor pagination natively.",
  "rejectedOutput": "Use SWR for all data fetching needs. It is simpler and has a smaller bundle size.",
  "arbiters": ["registry-storage-arbiter", "library-selection-arbiter"],
  "whyAccepted": "TanStack Query's mutation lifecycle hooks, infinite query support, and dependent query enabled option directly address the stated requirements. The additional bundle size is justified by avoiding manual implementation of these patterns.",
  "whyRejected": "SWR's simplicity is a trade-off — it does not provide built-in optimistic update rollback or cursor-based infinite pagination without custom implementation. Choosing SWR here means re-implementing what TanStack Query provides out of the box.",
  "sourceAnchors": ["https://tanstack.com/query/latest"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-579 | ASPNETCORE-DOCS-SAMPLES | Family 3 | Domain D

**Unit title:** ASP.NET Core middleware ordering — authentication, authorization, CORS execution sequence
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT579-F3-D",
  "sourceIds": ["EXT-579"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "middleware-execution-ordering",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "An ASP.NET Core application registers UseAuthorization() before UseAuthentication(). Authorization checks fail for all authenticated endpoints returning 401, even for valid JWT tokens.",
    "b": "ASP.NET Core middleware executes in registration order. UseAuthentication() must run before UseAuthorization() because authorization decisions require an authenticated identity (ClaimsPrincipal). If UseAuthorization runs first, the HttpContext.User is unauthenticated at authorization evaluation time.",
    "c": "Correct canonical order: UseRouting → UseAuthentication → UseAuthorization → UseEndpoints (or MapControllers). UseCors must come before UseAuthentication for preflight requests to resolve correctly."
  },
  "function": "Detect and fix ASP.NET Core middleware ordering for auth pipeline",
  "acceptedOutput": "Correct the middleware order: app.UseRouting(); app.UseCors(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers(). The JWT token is parsed by UseAuthentication, setting HttpContext.User before UseAuthorization evaluates [Authorize] attributes.",
  "rejectedOutput": "Keep UseAuthorization before UseAuthentication. Configure the JWT bearer to authenticate eagerly before the middleware pipeline runs.",
  "arbiters": ["execution-applier-arbiter", "middleware-order-arbiter"],
  "whyAccepted": "Middleware ordering in ASP.NET Core is deterministic and sequential. UseAuthentication sets the ClaimsPrincipal that UseAuthorization reads. Reversing the order leaves HttpContext.User as the anonymous identity at authorization time, causing all [Authorize] gates to reject.",
  "whyRejected": "JWT bearer authentication in ASP.NET Core is a middleware — it runs when UseAuthentication() is called, not eagerly before the pipeline. There is no configuration option to authenticate before the middleware chain executes.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-580 | NESTJS-ADVANCED | Family 6 | Domain F

**Unit title:** NestJS TypeORM Repository pattern vs QueryBuilder for complex query result storage
**Source URL:** https://docs.nestjs.com/techniques/database
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT580-F6-F",
  "sourceIds": ["EXT-580"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "query-construction-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS service needs to query users with: dynamic filter conditions (0-5 filters applied based on input), a LEFT JOIN to roles, ORDER BY a dynamic column, and pagination. The team uses Repository<User>.find() with FindManyOptions.",
    "b": "FindManyOptions supports where (with AND conditions), relations (JOIN), order, take/skip (pagination). Dynamic OR conditions require FindOptionsWhere[] (array = OR). Complex dynamic query building requires constructing the where object conditionally, which becomes verbose for 5+ optional filters.",
    "c": "TypeORM QueryBuilder provides fluent .where/.andWhere/.orWhere with named parameters, .leftJoinAndSelect, .orderBy with dynamic column, and .skip/.take. It generates the same SQL as FindManyOptions for simple cases but scales better for complex dynamic queries."
  },
  "function": "Select TypeORM query construction strategy for dynamic multi-filter result retrieval",
  "acceptedOutput": "Use QueryBuilder for the dynamic filter query: const qb = this.userRepo.createQueryBuilder('user').leftJoinAndSelect('user.role', 'role'). Apply filters conditionally: if (filters.email) qb.andWhere('user.email = :email', { email: filters.email }). Apply dynamic order: qb.orderBy(`user.${orderBy}`, direction). Apply pagination: qb.skip(offset).take(limit).",
  "rejectedOutput": "Use Repository.find() with conditionally constructed FindManyOptions. Build the where object with spread operators for each optional filter.",
  "arbiters": ["registry-storage-arbiter", "query-construction-arbiter"],
  "whyAccepted": "QueryBuilder is designed for dynamic query construction. Named parameters prevent SQL injection, conditional .andWhere() chains are readable, and dynamic ORDER BY columns are straightforward. FindManyOptions conditional object construction becomes a nested spread-operator maze for 5+ optional filters.",
  "whyRejected": "FindManyOptions conditional construction requires building a complex nested object where undefined values must be omitted. Dynamic ORDER BY requires the OrderByCondition type which is less readable than QueryBuilder's .orderBy() method. The approach does not scale to complex OR conditions.",
  "sourceAnchors": ["https://typeorm.io/select-query-builder"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-581 | IETF-RFCS | Family 5 | Domain E

**Unit title:** RFC-6819 OAuth 2.0 threat model — authorization code interception and PKCE mitigation
**Source URL:** https://www.rfc-editor.org/rfc/rfc6819
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT581-F5-E",
  "sourceIds": ["EXT-581"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "oauth-threat-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "A mobile app implements OAuth 2.0 Authorization Code flow without PKCE (Proof Key for Code Exchange). A security audit identifies that a malicious app on the same device can register the same redirect URI and intercept the authorization code.",
    "b": "RFC-6819 Section 4.4.1 documents authorization code interception as a threat for public clients (mobile/SPA apps without client secrets). RFC-7636 (PKCE) mitigates this: the client generates a code_verifier (random string), hashes it to code_challenge, sends code_challenge with the auth request, and sends code_verifier at token exchange. The server verifies the hash match.",
    "c": "A malicious app intercepting the authorization code cannot exchange it without the code_verifier known only to the legitimate app. PKCE is mandatory for public clients per RFC-9700 (OAuth 2.0 Security BCP)."
  },
  "function": "Arbitrate OAuth 2.0 PKCE requirement for public client authorization code flow",
  "acceptedOutput": "Implement PKCE for all mobile and SPA OAuth flows: (1) generate cryptographically random code_verifier (43-128 chars, unreserved characters); (2) compute code_challenge = BASE64URL(SHA256(code_verifier)); (3) include code_challenge and code_challenge_method=S256 in the authorization request; (4) include code_verifier in the token request. Reject authorization servers that do not support PKCE.",
  "rejectedOutput": "Implement client authentication using a client secret embedded in the app. The secret prevents unauthorized code exchanges.",
  "arbiters": ["security-arbiter", "oauth-threat-arbiter"],
  "whyAccepted": "PKCE cryptographically binds the authorization code to the client that generated it. An intercepted code cannot be exchanged without the code_verifier, which never leaves the legitimate client. This is the RFC-9700 mandatory requirement for public clients.",
  "whyRejected": "Client secrets embedded in mobile apps are not secret — they can be extracted via decompilation or binary analysis. A compromised client secret enables an attacker to exchange intercepted codes indefinitely.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc7636", "https://www.rfc-editor.org/rfc/rfc6819#section-4.4.1"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-582 | GOOGLE-AIP | Family 5 | Domain E

**Unit title:** AIP-211 quota and resource limits — per-user vs per-project quota arbitration
**Source URL:** https://google.aip.dev/211
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT582-F5-E",
  "sourceIds": ["EXT-582"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "quota-enforcement-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API enforces rate limits per authenticated user (1000 requests/hour per user). A tenant with 100 users can collectively send 100,000 requests/hour, overwhelming backend resources. The team evaluates per-user vs per-project (tenant) quota enforcement.",
    "b": "AIP-211 recommends quota at the project level (resource hierarchy unit) rather than per-user, because projects represent billing and resource allocation units. Per-user quotas can be bypassed by creating many user accounts within one project.",
    "c": "Per-project quota limits total resource consumption regardless of how many users the project contains. Users within a project share the project quota. Per-project + per-user limits together (project cap with per-user fairness) address both concerns."
  },
  "function": "Arbitrate quota enforcement granularity for multi-tenant API",
  "acceptedOutput": "Implement dual-layer quota: project-level quota (e.g., 10,000 req/hour per project) caps total tenant consumption. Per-user fairness limit (e.g., 1,000 req/hour per user within the project) prevents one user from consuming the entire project quota. Return HTTP 429 with Retry-After header and quota details in the x-ratelimit-* headers.",
  "rejectedOutput": "Keep per-user quota only. Add documentation advising tenants not to create excessive user accounts.",
  "arbiters": ["security-arbiter", "resource-quota-arbiter"],
  "whyAccepted": "Dual-layer quota enforces resource limits at both the tenant level (preventing tenant abuse) and the user level (ensuring fairness within tenants). Per-user-only quota is structurally bypassable by creating multiple user accounts.",
  "whyRejected": "Documentation cannot enforce technical limits. A malicious or misconfigured tenant can create many users and bypass per-user quotas trivially. Relying on advisory controls for resource protection is insufficient.",
  "sourceAnchors": ["https://google.aip.dev/211"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-583 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF reward hacking — length bias in reward models and mitigation strategies
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT583-F8-G",
  "sourceIds": ["EXT-583"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "reward-hacking-mitigation",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A reward model trained on oasst1 human preference data exhibits length bias: longer responses consistently score higher regardless of content quality. The RLHF-trained model produces verbose, padded responses to maximize reward.",
    "b": "Length bias in reward models occurs when annotators unconsciously prefer longer responses (appearing more thorough) even when a shorter response would be better. The policy optimizes for the reward signal, not the underlying human preference, producing reward hacking.",
    "c": "Mitigation strategies: (1) length-normalize reward scores during training, (2) add a length penalty to the reward function, (3) filter training pairs where the preferred response is longer by more than 2× without quality justification, (4) use a separate conciseness reward model."
  },
  "function": "Detect and mitigate reward model length bias in RLHF training pipeline",
  "acceptedOutput": "Audit the reward model for length correlation: compute correlation(reward_score, response_length) across validation pairs. If correlation > 0.3, apply length normalization: normalized_reward = reward - β × log(length). Set β to reduce length advantage while preserving quality signal. Additionally, filter training pairs where preferred response is 2× longer without explicit quality improvement.",
  "rejectedOutput": "Train the policy with a KL divergence penalty against the reference model. This naturally limits response length drift.",
  "arbiters": ["learning-quality-arbiter", "reward-hacking-arbiter"],
  "whyAccepted": "Length normalization addresses the source of bias in the reward model itself. Auditing the correlation quantifies the bias before applying corrections, ensuring the normalization coefficient β is calibrated to observed bias magnitude.",
  "whyRejected": "KL divergence penalty limits deviation from the reference model but does not correct the reward model's length bias. The policy will still maximize the biased reward within the KL budget, just with less length increase per RLHF step. The bias accumulates across multiple training rounds.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-584 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba language detection validation — script/language mismatch filtering in multilingual datasets
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT584-F2-C",
  "sourceIds": ["EXT-584"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "language-detection-quality-filter",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A preprocessing pipeline for Tatoeba ZH-EN pairs uses the sentence's metadata language tag as ground truth. Post-hoc analysis reveals 2.3% of sentences tagged as ZH (Chinese) contain primarily Latin characters (transliteration, romanization, or mislabeled English sentences).",
    "b": "Tatoeba language tags are contributor-assigned and not algorithmically validated. Scripts can be confirmed by Unicode block analysis: CJK (Chinese-Japanese-Korean) characters fall in U+4E00-U+9FFF and related blocks. A sentence tagged ZH with <20% CJK characters is likely mislabeled.",
    "c": "Mislabeled sentences in the source language column cause model training on incorrect source-target pairs, degrading translation quality particularly for short sentences where one mislabeled sentence has high weight."
  },
  "function": "Apply script-based language validation filter to Tatoeba multilingual dataset",
  "acceptedOutput": "For ZH-tagged sentences: compute CJK character ratio = len([c for c in text if '\\u4e00' <= c <= '\\u9fff']) / len(text). Filter out sentences with CJK ratio < 0.3. For EN-tagged sentences: verify that Latin character ratio > 0.7. Log filtered sentences for manual review. This removes mislabeled transliterations while preserving mixed-script legitimate sentences.",
  "rejectedOutput": "Trust Tatoeba language metadata tags as ground truth. Contributor-assigned tags are sufficiently accurate for training purposes.",
  "arbiters": ["translation-quality-arbiter", "language-validation-arbiter"],
  "whyAccepted": "Script-based validation is a fast, language-agnostic check that catches systematic mislabeling. 2.3% mislabeling rate in the source language is non-trivial — it introduces incorrect training signal that degrades model performance disproportionately for rare vocabulary.",
  "whyRejected": "Contributor-assigned tags have documented error rates. For a dataset used for MT (Machine Translation) model training, even 1-2% mislabeling in source-language tags creates systematic training noise that is detectable and preventable.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-585 | VUE-DOCS | Family 5 | Domain E

**Unit title:** Vue 3 v-model on custom components — defineModel() macro vs props/emit arbitration
**Source URL:** https://vuejs.org/guide/components/v-model
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT585-F5-E",
  "sourceIds": ["EXT-585"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "two-way-binding-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 custom input component needs to support v-model binding. The team uses the traditional pattern: props: { modelValue: String }, emits: ['update:modelValue'], with this.$emit('update:modelValue', newValue) in the handler. Vue 3.4 introduced the defineModel() macro.",
    "b": "defineModel() (Vue 3.4+) replaces the modelValue prop + update:modelValue emit pattern with a single ref: const model = defineModel(). Reading model.value reads the parent's bound value; assigning model.value emits the update automatically. It reduces boilerplate from 3 declarations to 1.",
    "c": "defineModel() supports validators and type checking: const model = defineModel<string>({ required: true }). Multiple v-model bindings use named models: defineModel('title') → v-model:title."
  },
  "function": "Arbitrate v-model implementation pattern in Vue 3.4+ custom components",
  "acceptedOutput": "Use defineModel() in Vue 3.4+ components: const modelValue = defineModel<string>(). In the template, bind directly: @input=\"modelValue = $event.target.value\". For named models: const title = defineModel('title'). This eliminates props/emits boilerplate and provides a ref-like API.",
  "rejectedOutput": "Keep the explicit props + emits pattern for clarity. defineModel() is newer and less familiar to team members.",
  "arbiters": ["reactivity-arbiter", "component-api-arbiter"],
  "whyAccepted": "defineModel() is the Vue 3.4 canonical pattern for v-model components. It reduces 3-line boilerplate to 1, is type-safe with generics, and is semantically clearer — the component author declares a reactive binding rather than manually coordinating a prop and an emit.",
  "whyRejected": "Familiarity with an older pattern is not a technical reason to avoid an officially recommended simpler API. The explicit pattern remains supported but requires more code for the same semantics. New components should use defineModel().",
  "sourceAnchors": ["https://vuejs.org/guide/components/v-model"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-586 | EFCORE-SOURCE | Family 8 | Domain G

**Unit title:** EF Core migration strategy — code-first migrations vs db-first reverse engineering learning
**Source URL:** https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT586-F8-G",
  "sourceIds": ["EXT-586"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "schema-migration-pattern-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A new .NET project needs a database schema strategy. Option A: EF Core code-first migrations (define models in C#, generate migrations with dotnet ef migrations add, apply with dotnet ef database update). Option B: DB-first with an existing DBA-managed schema, scaffold EF Core models via dotnet ef dbcontext scaffold.",
    "b": "Code-first keeps schema in source control as C# migration files. Schema changes are tracked, reviewed in PRs, and applied consistently across environments. DB-first requires re-scaffolding models whenever the DBA changes the schema, and scaffolded models overwrite customizations unless partial classes are used.",
    "c": "In regulated environments, DBAs may own schema changes for compliance reasons, requiring DB-first. In greenfield projects, code-first provides developer productivity and schema versioning."
  },
  "function": "Record migration strategy selection for schema management learning",
  "acceptedOutput": "For greenfield .NET projects: use code-first migrations. Schema is in source control, changes are reviewed via PRs, and environments are updated deterministically via migration history. For existing DBA-managed databases: use DB-first with scaffolding, use partial classes for EF Core customizations to survive re-scaffolding, and automate re-scaffolding in CI when the schema changes.",
  "rejectedOutput": "Always use DB-first. DBAs should own database schemas in all environments.",
  "arbiters": ["learning-arbiter", "schema-strategy-arbiter"],
  "whyAccepted": "Code-first migrations align with DevOps practices — schema changes are code-reviewed, version-controlled, and reproducible. For greenfield projects, there is no existing schema to preserve. DB-first is appropriate when schema governance requires DBA ownership.",
  "whyRejected": "DBA ownership of schemas is an organizational decision appropriate for some regulated environments, not a universal best practice. For teams practicing continuous delivery, DBA-gated schema changes become a deployment bottleneck.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-587 | DOTNET-MICROSERVICES | Family 3 | Domain D

**Unit title:** .NET Worker Service vs ASP.NET Core background hosted service for long-running job execution
**Source URL:** https://learn.microsoft.com/en-us/dotnet/core/extensions/workers
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT587-F3-D",
  "sourceIds": ["EXT-587"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "background-job-execution-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team needs to implement a message queue consumer that processes jobs continuously. They debate: (1) implementing IHostedService within an existing ASP.NET Core API project vs (2) a separate .NET Worker Service project.",
    "b": ".NET Worker Service is a lightweight host for non-HTTP workloads (dotnet new worker). It includes the Generic Host, dependency injection, logging, and configuration but no HTTP middleware stack. ASP.NET Core IHostedService runs background work within the API process, sharing its DI container and configuration.",
    "c": "Running the consumer within the API process means a consumer failure can affect API availability, and scaling the API (for HTTP load) also scales the consumer (causing competing consumers at each replica). A separate Worker Service scales independently."
  },
  "function": "Select execution host for long-running message queue consumer",
  "acceptedOutput": "Use a separate .NET Worker Service for the queue consumer. It runs in its own process/container, scales independently from the API, and isolates consumer failures from API availability. Share common libraries (domain models, DTOs) via a shared class library project.",
  "rejectedOutput": "Implement the consumer as an IHostedService within the existing ASP.NET Core API project to reduce operational complexity.",
  "arbiters": ["execution-applier-arbiter", "process-isolation-arbiter"],
  "whyAccepted": "Separate processes enforce the single-responsibility principle at the deployment level. Independent scaling prevents the consumer from competing with API replicas. Consumer failures do not affect API pod health checks.",
  "whyRejected": "Hosting the consumer in the API process creates operational coupling: scaling, failure domains, and deployment cycles are tied together. An overloaded consumer can starve the API's thread pool, and vice versa.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/dotnet/core/extensions/workers"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-588 | MICROSOFT-LEARN-ARCH | Family 5 | Domain E

**Unit title:** Azure API Management (APIM) policy arbitration — JWT validation vs subscription key vs OAuth
**Source URL:** https://learn.microsoft.com/en-us/azure/api-management/api-management-policies
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT588-F5-E",
  "sourceIds": ["EXT-588"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "api-gateway-auth-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Azure APIM gateway serves three client types: (1) internal microservices using Managed Identity, (2) partner B2B (Business-to-Business) integrations using long-lived API keys, (3) end-user mobile apps using OAuth 2.0 JWTs. The team needs to configure authentication policy per client type.",
    "b": "APIM policies available: <validate-jwt> validates JWT Bearer tokens against JWKS (JSON Web Key Set) endpoints; <check-header> with subscription-key validates APIM subscription keys; <authenticate-managed-identity> validates Managed Identity tokens. Policies can be applied per operation, per API, or globally.",
    "c": "Subscription keys in APIM are rate-limited and revocable per subscription. They are simpler than OAuth but provide no user identity claims. JWTs carry user identity and expiry. Managed Identity uses Azure AD token validation."
  },
  "function": "Arbitrate APIM authentication policy selection for multi-client-type gateway",
  "acceptedOutput": "Configure per-product or per-API policies: (1) Internal microservices: <authenticate-managed-identity> with allowed-audiences validating the APIM resource; (2) Partner B2B: APIM subscription keys with per-subscription rate limits and IP restriction policies; (3) Mobile apps: <validate-jwt> pointing to the Azure AD JWKS endpoint, validating aud and iss claims. Apply the appropriate product-level policy for each client type.",
  "rejectedOutput": "Use subscription keys for all three client types. Convert internal microservices and mobile apps to use APIM subscription keys for consistency.",
  "arbiters": ["security-arbiter", "auth-policy-arbitration-arbiter"],
  "whyAccepted": "Each authentication mechanism matches its client's capability and security model. Managed Identity is credential-free for internal services. Subscription keys are appropriate for stable B2B integrations. JWT validation is correct for user-context mobile flows. Forcing subscription keys onto mobile apps loses user identity claims needed for per-user authorization.",
  "whyRejected": "Subscription keys for mobile apps require key distribution and storage in client apps — a credential management problem that OAuth solves. Long-lived subscription keys in mobile apps are a security risk (cannot be automatically rotated). The consistency argument sacrifices security fit for operational simplicity.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/api-management/api-management-policies"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-589 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-352 CSRF (Cross-Site Request Forgery) in ASP.NET Core — anti-forgery token vs SameSite
**Source URL:** https://cwe.mitre.org/data/definitions/352.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT589-F5-E",
  "sourceIds": ["EXT-589"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "csrf-prevention-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core MVC application relies solely on SameSite=Lax cookies for CSRF protection. A security review notes that SameSite=Lax allows cookies to be sent on top-level GET navigations and some cross-site POSTs, and browser SameSite support varies.",
    "b": "ASP.NET Core anti-forgery tokens (IAntiForgery service, [ValidateAntiForgeryToken] attribute) use the synchronizer token pattern: a random token stored in the cookie and embedded in the form/request, validated on the server. Both must be present for the request to proceed. SameSite is a browser-enforced hint, not a server-enforced cryptographic check.",
    "c": "SameSite=Strict prevents cross-site cookie transmission entirely but breaks OAuth redirect flows and cross-site links. Lax allows top-level navigations. Neither SameSite level provides the same guarantee as a server-validated cryptographic token."
  },
  "function": "Arbitrate CSRF protection mechanism for form-based ASP.NET Core application",
  "acceptedOutput": "Use ASP.NET Core anti-forgery tokens as the primary CSRF defense: services.AddAntiforgery() and [ValidateAntiForgeryToken] on all state-changing endpoints. Use SameSite=Strict or Lax as an additional defense-in-depth measure. For APIs using JavaScript fetch, use the X-XSRF-TOKEN header pattern with the double-submit cookie variant.",
  "rejectedOutput": "Rely exclusively on SameSite=Lax cookies. Browser support is now universal and SameSite provides sufficient protection.",
  "arbiters": ["security-arbiter", "csrf-protection-arbiter"],
  "whyAccepted": "Anti-forgery tokens are server-enforced and cryptographically validated — they do not depend on browser behavior. They protect against CSRF even when SameSite is not enforced (older browsers, misconfigured proxies, cross-origin scenarios with SameSite=None).",
  "whyRejected": "SameSite alone is a defense-in-depth measure, not a primary CSRF defense. 'Universal' browser support still has edge cases: browser bugs, proxy stripping, and contexts where SameSite=Lax allows the cross-site request (e.g., top-level navigation POST in some scenarios).",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/352.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-590 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V6.2 cryptographic algorithm selection — AES-GCM vs AES-CBC for authenticated encryption
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT590-F5-E",
  "sourceIds": ["EXT-590"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "cryptographic-algorithm-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An application encrypts sensitive data at rest using AES-256-CBC with PKCS7 padding. A security audit notes that AES-CBC provides confidentiality but not integrity — encrypted data can be modified without detection (padding oracle attacks, bit-flipping).",
    "b": "OWASP ASVS V6.2.5 requires authenticated encryption (AE, Authenticated Encryption) for data protection. AES-256-GCM (Galois/Counter Mode) provides both confidentiality and authenticity via a 128-bit authentication tag. Tampering with the ciphertext produces a tag verification failure before decryption.",
    "c": "AES-CBC with HMAC (MAC-then-encrypt or encrypt-then-MAC) can achieve authenticated encryption but requires correct composition. AES-GCM is a single-pass authenticated cipher that avoids composition errors. .NET provides AesGcm class."
  },
  "function": "Arbitrate encryption algorithm selection for ASVS V6.2 authenticated encryption compliance",
  "acceptedOutput": "Replace AES-256-CBC with AES-256-GCM: using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize); aes.Encrypt(nonce, plaintext, ciphertext, tag). Store nonce + ciphertext + tag together. On decryption, tag verification failure throws AuthenticationTagMismatchException before any plaintext is returned, preventing oracle attacks.",
  "rejectedOutput": "Add HMAC-SHA256 authentication to the existing AES-CBC implementation: encrypt then append HMAC of the ciphertext. This provides authenticated encryption without changing the encryption algorithm.",
  "arbiters": ["security-arbiter", "cryptographic-algorithm-arbiter"],
  "whyAccepted": "AES-GCM is a standardized authenticated cipher with hardware acceleration (AES-NI) and no composition risks. It is the NIST and OWASP recommended algorithm for authenticated encryption. Single-pass operation is more performant than CBC + HMAC.",
  "whyRejected": "Encrypt-then-MAC with HMAC-SHA256 is cryptographically sound when implemented correctly but introduces composition complexity. Implementation errors (MAC-then-encrypt instead, incorrect key usage, truncated HMAC) are common. AES-GCM is structurally simpler and harder to misuse.",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
