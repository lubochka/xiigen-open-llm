# Node 04 — Domain Processor — Clean Batch 002

Node 4 receives exactly one sub-task plus its domain context.
It produces all success states, all failure states, and all wait-state outcomes for that one sub-task.
The output must contain enough information for the applier and result translator to act without re-reading the original input.

A is the exact sub-task plus the domain context as it would arrive at runtime.
B is the complete enumeration of success, failure, and wait-state outcomes — not a description of what those outcomes would address.
A B that says "the outcomes include success and failure cases" is wrong — each outcome must be named with its specific condition and specific result.

One pair covers one sub-task. Array nodes receive one element at a time.

---

Pair CB2-04-001 — Add select_related('author') to Post queryset
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Sub-task: "Add select_related('author') to the Post queryset"
Domain context: [D-database-query-efficiency] The current queryset is Post.objects.all()[:100]. It generates 101 database queries for 100 posts: 1 for posts, then 1 per post for the author. The template accesses post.author.name in a loop. The fix is to add select_related('author') so Django generates a single JOIN query instead of N+1 individual queries.
B:
Success state: Post.objects.select_related('author').all()[:100] executes one SQL query: SELECT blog_post.id, blog_post.title, blog_post.body, blog_post.author_id, auth_user.id, auth_user.username, auth_user.first_name, auth_user.last_name FROM blog_post INNER JOIN auth_user ON blog_post.author_id = auth_user.id LIMIT 100. The ORM caches the author object on each Post instance from the JOIN result. Template access to post.author.name does not trigger a database query. Total queries for the request: 1. Expected render time: under 0.5 seconds, down from 4.2 seconds.

Failure state 1 — FieldError: If Post.author is not a direct ForeignKey field (e.g., it is a reverse ForeignKey or a ManyToManyField), select_related raises FieldError: "Invalid field name(s) for model Post: 'author'". Verify that Post.author is defined as author = models.ForeignKey(User, on_delete=models.CASCADE) or equivalent. If Post.author is a ManyToManyField, use prefetch_related('authors') instead.

Failure state 2 — Over-fetching after the fix: select_related('author') joins the full auth_user row. If auth_user has 30 columns and the template uses only username, 29 columns are transferred and discarded per row. This is acceptable at 100 posts but becomes significant at higher limits. Mitigate with .only('id', 'title', 'body', 'author__username') if the query is performance-critical at scale.

Wait state: No migration required. select_related is a Python-layer query modifier. No database schema changes. No coordination with other services. The queryset change is safe to deploy as a single commit to the view function.
Reason: The success state names the specific SQL generated, the specific template access pattern that no longer triggers queries, and the target query count and render time. The failure states name the specific exceptions and their conditions. The wait state confirms no migration or service coordination is needed — the information the applier requires to proceed without further input.
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT

Pair CB2-04-001 — Phrasing B
Source: https://github.com/princeton-nlp/SWE-bench — SWE-bench task instances
License: MIT
A: Sub-task: "Replace Post.objects.all()[:100] with Post.objects.select_related('author').all()[:100]"
Domain context: [D-database-query-efficiency / E-orm-usage-pattern] Django blog view, 100 posts, 101 queries. N+1 caused by lazy author load in template. select_related resolves N+1 via JOIN.
B:
Success state: One JOIN query replaces 101 individual queries. post.author.name in the template reads from the cached join result — no database call. Query count: 1. Render time target: under 0.5s.

Failure state 1 — Wrong field type: If Post.author is not a ForeignKey, select_related('author') raises FieldError at query evaluation time. Check model definition. Use prefetch_related for ManyToMany or reverse relations.

Failure state 2 — Partial field over-fetch: The full auth_user row is loaded per post. If only a few columns are needed, add .only('id', 'title', 'body', 'author__username') to restrict transferred columns.

Wait state: No migration, no cross-service coordination required. Single view function change.

---

Pair CB2-04-002 — Switch load balancer to consistent-hash routing on traceId
Source: https://github.com/open-telemetry/opentelemetry-specification — specification/sdk-configuration.md
License: Apache 2.0
A: Sub-task: "Replace round-robin load balancer configuration with consistent-hash routing keyed on traceId"
Domain context: [A-observability-routing] Three-instance OpenTelemetry Collector deployment. Tail-based sampling processor is being added. Tail sampling buffers all spans for a trace until the decision fires. A span from the same trace arriving at a different Collector instance than other spans of that trace causes the processor to make its retention decision on incomplete data, incorrectly dropping traces that should be retained.
B:
Success state: After the routing change, all spans sharing the same TraceId are routed to the same Collector instance. A synthetic multi-span trace (minimum 5 spans across 3 services) is injected after the routing change. All 5 spans land on one Collector instance. The tail sampling processor receives the complete trace before its decision_wait window expires. Collector metrics show no "incomplete trace" or "spans from unknown trace" counters incrementing during the test window.

Failure state 1 — Load balancer does not support header-keyed consistent hash: Some load balancers (HAProxy basic mode, AWS ALB without sticky sessions) cannot route by arbitrary header value. In this case, replace the frontend load balancer with the OpenTelemetry Collector's loadbalancing exporter. Deploy a stateless Collector tier that receives all spans and routes to backend Collector instances using loadbalancing exporter with routing_key: traceId. This tier forwards via OTLP; the backend instances run the tail sampling processor.

Failure state 2 — Hash key not available at routing time: TraceId is propagated in the W3C traceparent header (format: 00-<traceId>-<spanId>-<flags>). If the load balancer cannot parse the traceparent header to extract the traceId, it cannot perform consistent routing. Observable signal: the load balancer does not produce an error log for this failure — the failure is silent at the load balancer layer and becomes visible downstream. After the routing change is deployed, Collector metrics show 'incomplete trace' or 'spans from unknown trace' counters incrementing during the synthetic trace test. If those counters increment after the routing change but the synthetic trace test was expected to pass, traceparent parsing failure is the likely cause. Confirm that the load balancer can match and route on a substring of a custom header before attempting this configuration.

Wait state: Steps 3 and 4 (tail sampling processor and memory_limiter configuration) must not be applied until the synthetic trace test confirms routing affinity on a live trace. Specific resolution check: inject a minimum 5-span synthetic trace across at least 3 services, then confirm all 5 spans appear in one Collector instance's span buffer and that the Collector metrics show no 'incomplete trace' counter increment during the test window. If any span lands on a different instance, or if the 'incomplete trace' counter increments, the routing change is incomplete. Do not proceed to processor configuration while spans can scatter across instances.
Reason: The success state names the specific test (synthetic multi-span trace, 5 spans, confirm single instance landing) and the metric to check (Collector "incomplete trace" counter). The failure states name the specific load balancer limitations and their exact workarounds. The wait state names the blocking condition for subsequent steps by name.
Source: https://github.com/open-telemetry/opentelemetry-specification — specification/sdk-configuration.md
License: Apache 2.0

Pair CB2-04-002 — Phrasing B
Source: https://github.com/open-telemetry/opentelemetry-specification — specification/sdk-configuration.md
License: Apache 2.0
A: Sub-task: "Configure consistent-hash routing on traceId for three-instance OTel Collector cluster"
Domain context: [Observability infrastructure] Current: round-robin. Required: all spans from same trace land on same instance. Tail sampling cannot function correctly on partial trace data.
B:
Success state: All spans of any given TraceId reach one Collector instance. Synthetic trace test: inject 5-span trace, all 5 arrive at one instance. No "incomplete trace" metrics increment during the test window.

Failure state 1 — Load balancer limitation: Use the Collector's loadbalancing exporter as the routing tier instead. Configure routing_key: traceId. Frontend tier is stateless; backend tier runs tail sampling.

Failure state 2 — traceparent parsing: Consistent hash requires extracting TraceId from W3C traceparent header. Observable signal: this failure is silent at the load balancer — no error is logged. It becomes visible downstream as 'incomplete trace' counters incrementing in Collector metrics after the routing change is deployed. Confirm load balancer supports header substring routing before configuration.

Wait state: Processor configuration (tail sampling and memory_limiter) blocked until synthetic trace test passes on live traffic. Specific resolution check: inject a 5-span synthetic trace across at least 3 services and confirm all 5 spans appear in one Collector instance's span buffer with no 'incomplete trace' counter increment.

---

Pair CB2-04-003 — Add EmailAddress nullable column migration
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT
A: Sub-task: "Create an EF Core migration that adds a nullable EmailAddress column to the Users table"
Domain context: [Database schema evolution] SQL Server, Users table, 5 million rows. Active read and write traffic during business hours. Three Dapper services read the Email column by name. This is Phase 1 of a 5-phase expand-contract column rename. The goal is to add the new column without acquiring a table lock and without modifying any application service.
B:
Success state: EF Core generates the migration with the SQL statement: ALTER TABLE Users ADD EmailAddress NVARCHAR(256) NULL. SQL Server executes this DDL without a full table lock — adding a nullable column with no DEFAULT value is a metadata-only operation on SQL Server 2012 and later. After the migration: all existing rows have EmailAddress = NULL; the Email column is unchanged; all three services continue querying the Email column without error; EF Core __EFMigrationsHistory contains the new migration ID.

Failure state 1 — Table lock if column has a default or NOT NULL constraint: If the migration specifies NVARCHAR(256) NOT NULL DEFAULT '' instead of NULL, SQL Server must update every existing row to set the default value, acquiring a lock for the duration. On 5 million rows this causes a blocking period that violates the zero-downtime requirement. The migration must specify NULL with no DEFAULT to remain non-blocking.

Failure state 2 — Column add blocked by concurrent schema lock: If another DDL operation (index rebuild, statistics update job) holds a schema lock at migration time, ALTER TABLE blocks. Confirm no concurrent DDL is running before executing, or schedule the migration outside peak DDL activity.

Failure state 3 — NOT NULL constraint added prematurely: If a subsequent migration immediately adds a NOT NULL constraint before the backfill runs in Phase 3, existing rows with NULL EmailAddress violate the constraint and the migration fails. The NOT NULL constraint must wait until Phase 5, after backfill and read cutover are complete.

Wait state: Do not deploy Phase 2 (dual-write application code) until __EFMigrationsHistory confirms the migration ID is present in the production database. If the migration has not run, the EmailAddress column does not exist and any dual-write code that writes to it will throw a SqlException on every INSERT and UPDATE.
Reason: The success state names the exact SQL generated, the SQL Server version requirement for non-blocking behavior, and the observable confirmation (EFMigrationsHistory). The failure states name the specific conditions that make the operation blocking or failing. The wait state names the exact blocking condition for Phase 2 — the column must exist before dual-write code is deployed. Note: Failure state 1 (NOT NULL with DEFAULT at column creation) and Failure state 3 (NOT NULL constraint added before backfill completes) share the same root cause — a NOT NULL constraint present before all rows have values — but are kept as separate entries because they occur at different phases, produce different observable signals (a table lock during the initial migration versus a constraint violation during a subsequent migration), and require different remediations (fix the column definition in the current migration versus defer the NOT NULL constraint to Phase 5).
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT

Pair CB2-04-003 — Phrasing B
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT
A: Sub-task: "EF Core migration: add nullable EmailAddress column to Users table, 5M rows, no downtime"
Domain context: [Schema evolution] SQL Server, active traffic, 3 Dapper services on Email column. Must be non-blocking DDL. Phase 1 of expand-contract rename.
B:
Success state: ALTER TABLE Users ADD EmailAddress NVARCHAR(256) NULL — metadata-only on SQL Server 2012 and later, no table lock. All rows after migration: EmailAddress = NULL. Email column unchanged. All three services unaffected. Migration ID in __EFMigrationsHistory.

Failure state 1 — Lock risk: Any NOT NULL or DEFAULT on the new column forces SQL Server to update all 5M rows under a lock. Specify NULL only, no DEFAULT, to keep the operation non-blocking.

Failure state 2 — Concurrent DDL contention: A concurrent index rebuild or statistics job can block ALTER TABLE. Confirm no concurrent DDL before running.

Failure state 3 — Premature NOT NULL: Adding NOT NULL before the Phase 3 backfill completes fails on existing NULL rows. NOT NULL constraint belongs in Phase 5 only.

Wait state: Phase 2 dual-write deployment blocked until migration ID is confirmed in __EFMigrationsHistory on production.

---

Pair CB2-04-004 — Register Redis CacheModule with 4-hour TTL
Source: https://docs.nestjs.com — NestJS caching documentation
License: MIT
A: Sub-task: "Install cache-manager-redis-yet and register CacheModule with Redis store and TTL of 14,400 seconds"
Domain context: [Caching architecture] NestJS API, three load-balanced instances. Product catalog endpoint: 50K requests/day, 800ms database query, 4-hour catalog update cycle. Shared cache required because cache invalidation must propagate immediately to all three instances. In-process cache was ruled out because it cannot guarantee simultaneous invalidation across instances.
B:
Success state: CacheModule.register({ store: redisStore, host, port, ttl: 14400 }) is called in AppModule. The catalog endpoint handler is decorated with @CacheKey('product-catalog') and @CacheTTL(14400). First request to the catalog endpoint from any instance: Redis MISS → database query (800ms) → result stored in Redis with key 'product-catalog' and TTL 14,400 seconds → response returned. All subsequent requests from all three instances within the TTL window: Redis HIT → response returned in approximately 5ms, no database query. Cache invalidation: cacheManager.del('product-catalog') called once in the admin update handler → Redis deletes the key → all three instances serve a fresh database response on their next request.

Failure state 1 — Redis unreachable at startup: CacheModule.register() throws a connection error and the application fails to start. Mitigate by registering a fallback memory store, or wrap the Redis store registration to fall back to no-cache mode (the endpoint falls through to the database on every request). Log the fallback state so operations can identify when Redis is unavailable.

Failure state 2 — TTL of 0 or omitted: The Redis key does not expire. The catalog is cached forever unless explicitly invalidated. If the catalog update path does not call cacheManager.del(), the cache serves stale data indefinitely. Confirm ttl: 14400 is present in both the CacheModule registration and the @CacheTTL decorator.

Failure state 3 — Redis unauthenticated: The default Redis configuration has no authentication. Without a password or ACL rule, any host on the same network segment can read, write, or flush the cache. Store the Redis connection string including AUTH credentials in an environment secret. Do not commit credentials to the application configuration file.

Wait state: Do not decorate the catalog endpoint with @CacheKey and @CacheTTL until Redis is confirmed reachable and responding to PING from all three instances. A cache key registered against an unreachable store causes every request to fail at the cache layer before reaching the database fallback.
Reason: The success state names the exact registration call, the exact decorator usage, and the cache hit and miss behavior per instance. The failure states name the specific conditions and their mitigations. The wait state names the exact test (PING from all three instances) required before enabling the cache decorator — without this the decorator converts a Redis outage into a full application outage.
Source: https://docs.nestjs.com — NestJS caching documentation
License: MIT

Pair CB2-04-004 — Phrasing B
Source: https://docs.nestjs.com — NestJS caching documentation
License: MIT
A: Sub-task: "Configure NestJS shared Redis cache for product catalog, TTL 4 hours, three instances"
Domain context: [Caching] Three-instance NestJS API, 50K requests/day, 800ms database query per miss. Shared cache required for consistent invalidation. In-process cache ruled out.
B:
Success state: CacheModule registered with Redis store, ttl: 14400. @CacheKey('product-catalog') and @CacheTTL(14400) on handler. First request per 4-hour window: 800ms database query plus Redis write. All subsequent requests: 5ms Redis read. Invalidation: one cacheManager.del('product-catalog') clears the key for all three instances immediately.

Failure state 1 — Startup failure: Redis unreachable at startup causes CacheModule to throw. Configure a memory-store fallback to prevent application startup failure.

Failure state 2 — No TTL expiry: Omitting ttl: 14400 causes indefinite caching. Verify TTL is set in both the CacheModule registration and the @CacheTTL decorator.

Failure state 3 — No authentication: Default Redis is open on the network. Store AUTH credentials as environment secrets.

Wait state: Confirm Redis responds to PING from all three instances before enabling the @CacheKey decorator.
