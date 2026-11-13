# Node 03 — Scheme Planner — Clean Batch 001

Node 3 receives all translated domain descriptions together and produces an ordered set of sub-tasks.
Each sub-task is paired with a transformational prompt that tells the applier exactly how to approach it.
The plan must include: ordered steps, parallel steps where allowed, wait states, recovery paths, and review points.
A B that says "create a plan with phases" is wrong — the actual plan with actual steps must appear as B.

---

Pair CB1-03-001 — OTel tail sampling deployment plan
Source: https://github.com/open-telemetry/opentelemetry-specification — specification/sdk-configuration.md and Collector processor docs
License: Apache 2.0
A: [Infrastructure/Observability] A high-traffic HTTP API emits 10,000 trace spans per second and exports 100% of them to Jaeger at a cost of $4,000/month. The team needs to reduce exported trace volume by 90% without losing visibility into error traces or requests that exceed 500ms end-to-end. The current Collector deployment uses round-robin load balancing across three instances.
[Sampling Strategy] Decision context: head-based sampling discards 90% of spans at ingress before the trace outcome is known. Tail-based sampling buffers the complete trace and applies retention policies after the outcome is visible. Available retention policies: retain all traces with ERROR-status spans; retain all traces exceeding 500ms; retain 1% of healthy-fast traces as baseline.
B:
Step 1 — Topology audit [prerequisite; no dependencies; blocks all subsequent steps]
Transformational prompt: Enumerate every Collector instance. Record its address, current processor chain, and routing mode. Determine whether spans from the same trace can arrive at different instances under the current load balancer. The tail sampling processor makes its retention decision only after all spans for a trace have arrived — if spans for one trace can land on different instances, the processor will make its decision on incomplete data, and some traces will be incorrectly dropped. This audit must confirm routing mode before any processor configuration begins.
Wait state: Hold at Step 1 until audit is complete and all three instances are accounted for. Do not proceed if any instance is unreachable.

Step 2 — Enable trace-affine routing [depends on Step 1; blocks Steps 3 and 4]
Transformational prompt: Replace the round-robin load balancer configuration with consistent-hash routing keyed on traceId. After the change, generate a multi-span synthetic trace and confirm that all spans from that trace arrive at the same Collector instance. Consistent-hash routing is a hard prerequisite: without it, the tail sampling decision fires on partial data.
Wait state: Hold at Step 2 until the routing change is confirmed by the synthetic trace test. Do not proceed if any span from the synthetic trace arrives at a different instance.

Step 3 — Configure tail sampling processor [depends on Step 2; runs in parallel with Step 4]
Transformational prompt: Add the tailsampling processor to the Collector pipeline. In the final configured pipeline, the tailsampling processor appears after the memory_limiter — Step 4 configures the memory_limiter concurrently, and both processors are applied together at deployment time. Configure the tailsampling processor independently of Step 4; do not wait for Step 4 before beginning this step. Define three policies in priority order: first, a status_code policy that retains all traces containing a span with ERROR status; second, a latency policy that retains all traces with total end-to-end duration greater than 500ms; third, a probabilistic policy that retains 1% of traces matching neither prior policy. Set decision_wait to 10 seconds. The priority order matters — a trace that is both an error and slow must not be counted twice against the 1% sample.

Step 4 — Configure memory_limiter processor [depends on Step 2; runs in parallel with Step 3]
Transformational prompt: Add the memory_limiter processor to the Collector pipeline, positioned before the tail sampling processor in the chain. Set the soft limit to 75% of the instance's allocated RAM and the hard limit to 85%. The memory_limiter must precede tail sampling in the chain order — tail sampling buffers all spans for a trace in memory until the decision fires, and a burst of high-cardinality traces can exhaust memory without this guard. If the hard limit is reached, the Collector drops new incoming spans rather than crashing.

Step 5 — Staged deployment and acceptance verification [depends on Steps 3 and 4]
Transformational prompt: Deploy the updated configuration to one Collector instance first, keeping the other two on the old configuration. Over 30 minutes: (a) confirm that outbound trace volume from the updated instance has dropped by at least 85%; (b) inject a synthetic error span and confirm it appears in Jaeger within 5 minutes; (c) inject a synthetic slow request held above 500ms and confirm it appears in Jaeger within 5 minutes; (d) confirm that healthy fast requests appear at approximately 1% of their original rate. If all four checks pass, roll out to the remaining two instances. If any check fails, roll back the single instance, diagnose, and do not proceed to the other instances.
Recovery path: If the memory_limiter evicts spans before the tail sampling decision fires (visible as "spans dropped" in Collector metrics), reduce decision_wait from 10s to 5s to shorten the buffer window. If OOM still occurs, reduce batch_size on the OTLP receiver to limit concurrent span load entering the Collector.

Review point: After full rollout, compare the previous month's Jaeger storage cost against the new rate. The target is under $500/month. If the reduction is insufficient, examine which policy is retaining more traces than expected.
Reason: The step order reflects tail sampling's hard prerequisite: consistent-hash routing before processor configuration. memory_limiter before tailsampling in the chain is an OTel Collector invariant — documented in the Collector processing pipeline specification. The parallel step (3 and 4) is valid because processor configuration files are independent; they are applied together at deployment time.
Source: https://github.com/open-telemetry/opentelemetry-specification — specification/sdk-configuration.md
License: Apache 2.0

Pair CB1-03-001 — Phrasing B
Source: https://github.com/open-telemetry/opentelemetry-specification — specification/sdk-configuration.md
License: Apache 2.0
A: [Observability cost reduction] Production API: 10K spans/second, 100% exported to Jaeger, $4K/month cost target reduced to $400/month. Constraint: all error traces and all traces over 500ms must remain fully visible. Collector topology: three instances, currently round-robin load balanced.
[Retention policy selection] Options compared: discard 90% at ingress (head-based) versus buffer complete traces and retain based on outcome (tail-based). Tail-based policy set: keep all ERROR-status traces, keep all traces > 500ms, keep 1% of remaining traces.
B:
Step 1 — Collector topology audit [no dependencies; all subsequent steps wait on this]
Transformational prompt: Identify all three Collector instances. Record address, processor chain, and load balancing mode. Test whether spans from the same trace can reach different instances. Tail-based sampling requires complete traces on one instance — if spans scatter across instances, the decision fires on partial data and drops traces that should be retained. Audit must complete and confirm affinity is absent before any configuration changes begin.

Step 2 — Switch to trace-affine routing [depends on Step 1; gates Steps 3 and 4]
Transformational prompt: Change load balancer configuration to consistent-hash routing using traceId as the hash key. Validate with a synthetic multi-span trace: emit it and confirm all spans land on one instance. Do not advance until this test passes on a live trace.

Step 3 — Tail sampling processor setup [depends on Step 2; parallel with Step 4]
Transformational prompt: Insert tailsampling processor into the pipeline. In the final configured pipeline, the tailsampling processor appears after the memory_limiter — Step 4 configures the memory_limiter concurrently; both steps are applied together at deployment time. Configure this step independently without waiting for Step 4. Configure policies in order: (1) status_code — keep traces with any ERROR span; (2) latency — keep traces with duration > 500ms; (3) probabilistic — keep 1% of all others. Set decision_wait: 10s. Policy order determines priority when a trace matches multiple policies.

Step 4 — Memory limiter setup [depends on Step 2; parallel with Step 3]
Transformational prompt: Insert memory_limiter before the tailsampling processor in the pipeline chain. Set soft_limit_mib to 75% of instance RAM, hard_limit_mib to 85%. Without this, tail sampling's in-memory trace buffer grows unbounded during load spikes. The Collector drops new spans when the hard limit is reached rather than crashing.

Step 5 — Single-instance rollout and acceptance test [depends on Steps 3 and 4]
Transformational prompt: Apply the new configuration to one instance. Run for 30 minutes. Verify: volume drops ≥ 85%; a synthetic error trace appears in Jaeger; a synthetic slow trace (held > 500ms) appears in Jaeger; healthy-fast trace rate is approximately 1% of baseline. All four must pass before rolling out to the remaining two instances. Roll back and diagnose if any check fails.

Recovery path: Spans dropped before decision_wait expires means the buffer window is too long. Reduce decision_wait to 5s. If OOM persists, reduce the OTLP receiver batch_size to lower concurrent span load.

---

Pair CB1-03-002 — EF Core column rename: zero-downtime expand-contract migration
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT
A: [Database schema evolution] A SQL Server table has 5 million rows and a column named Email. The column must be renamed to EmailAddress. The table is queried continuously during business hours. Three separate services access the column by name via Dapper queries. An EF Core migration with RenameColumn would lock the table and break all three services simultaneously at the moment of deployment.
[Safety constraint] The team requires zero downtime and no cross-service deployment coordination. Each service must be deployable independently.
B:
Phase 1 — Add EmailAddress column [independent of service deployments; runs first]
Transformational prompt: Create an EF Core migration that adds a nullable EmailAddress column to the Users table. SQL Server adds a nullable column without a table lock on large tables — this is a non-blocking DDL operation. The migration adds the column; no data is moved yet. Deploy the migration before any application code change.
Wait state: Confirm the migration applied successfully and EmailAddress column is present before proceeding.

Phase 2 — Dual-write application deployment [depends on Phase 1; deploy to all three services before Phase 3]
Transformational prompt: Update all three services to write to both Email and EmailAddress on every INSERT and UPDATE. Continue reading from Email. This phase is backward-compatible: if any service reverts, data written by the new version is still in the Email column that the old version reads. All three services must be on the dual-write version before the backfill runs.
Wait state: Confirm all three services are on the dual-write version. Do not run the backfill while any service is on the old single-write version.

Phase 3 — Backfill [depends on Phase 2]
Transformational prompt: Batch-update existing rows to copy Email values into EmailAddress for rows where EmailAddress is still NULL. Keep batches small enough to avoid acquiring a table lock — a batch that is too large will block concurrent reads and writes during the backfill window. Pause between batches to allow replication lag to recover. Monitor lock wait times and replication lag throughout. Continue until no rows with NULL EmailAddress remain. After the backfill, every row in the table has a value in both Email and EmailAddress.

Phase 4 — Read cutover [depends on Phase 3]
Transformational prompt: Update all three services to read from EmailAddress. Continue writing to both columns. This phase can also be deployed per-service since all three services still read a correct value from either column.

Phase 5 — Make EmailAddress NOT NULL and drop Email [depends on Phase 4; all services must be on read-from-EmailAddress before this runs]
Transformational prompt: Create an EF Core migration that: (1) alters EmailAddress to NOT NULL; (2) drops the Email column. Drop the Email column only after all three services have been confirmed on the Phase 4 read-cutover version. Dropping Email while any service still reads it causes an immediate error on every query from that service.
Wait state: Confirm all three services are on Phase 4 before running this migration.

Review point: After Phase 5, confirm that no query in any of the three services references the Email column name. A grep across all service repositories for the column name confirms the cleanup is complete.
Reason: Expand-contract is the standard zero-downtime column rename pattern for SQL Server tables with active query traffic. The phase ordering is mandatory: column addition before dual-write, dual-write before backfill, backfill before read cutover, read cutover before drop. Any other order creates a window where a service reads from a column that does not yet have data.
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT

Pair CB1-03-002 — Phrasing B
Source: https://github.com/dotnet/efcore — EF Core migration documentation
License: MIT
A: [Schema migration] SQL Server, 5 million rows, column Email must become EmailAddress. Continuous business-hours traffic. Three Dapper services reference the column by name. A direct RenameColumn migration would lock the table and break all three services at once. Constraint: each service deploys independently, no maintenance window.
[Zero-downtime requirement] The rename must happen without coordinating deployments across services and without any query returning an error at any point during the migration.
B:
Phase 1 — Add the new column [runs first, before any application change]
Transformational prompt: EF Core migration: add nullable EmailAddress column. Non-blocking on SQL Server for a nullable column add. No data movement. Deploy migration only — no application code changes yet.

Phase 2 — Dual-write in all three services [depends on Phase 1]
Transformational prompt: Each service writes Email and EmailAddress on every write operation. Reads come from Email. Deploy independently to each service — any deployment order is safe because old code reads Email, new code reads Email, and both write Email. All three must reach this version before the backfill.

Phase 3 — Backfill existing rows [depends on Phase 2, all services on dual-write]
Transformational prompt: Batch-update rows where EmailAddress is still NULL, copying Email values into EmailAddress. Size batches to avoid acquiring a table lock. Pause between batches for replication lag recovery. Continue until no NULL EmailAddress rows remain.

Phase 4 — Switch reads to EmailAddress [depends on Phase 3]
Transformational prompt: Update each service to read from EmailAddress. Still write to both. Can deploy per-service in any order.

Phase 5 — Drop Email column [depends on Phase 4, all services confirmed on read-from-EmailAddress]
Transformational prompt: Migration: set EmailAddress NOT NULL, then drop Email. Do not run until every service is confirmed on Phase 4. Grep all service codebases for the Email column name before running this migration.

---

Pair CB1-03-003 — Atomic order and inventory update: local transaction versus saga
Source: https://github.com/DapperLib/Dapper — Dapper patterns documentation
License: Apache 2.0
A: [Data consistency] An order service must insert an order record and decrement the matching inventory count in the same operation. If the inventory decrement fails — because insufficient stock exists — the order insert must be rolled back. Both the order and inventory data live in the same SQL Server database. The team has three developers and no message bus infrastructure.
[Architecture option analysis] Option A: wrap both database operations in a SqlTransaction, commit if both succeed, roll back if either fails. Option B: implement a saga with compensating transactions — publish an OrderPlaced event, subscribe in the inventory service, publish InventoryFailed if the decrement fails, and handle compensation in the order service.
B:
Step 1 — Confirm same-database boundary [prerequisite; no dependencies]
Transformational prompt: Verify that the Orders table and the Inventory table are in the same SQL Server database. A local SqlTransaction can span multiple tables in the same database but cannot span tables in separate databases or separate services. If they are in separate databases, skip to the saga path. If they are in the same database, proceed to Step 2.
Wait state: Confirm database boundary before choosing an implementation approach.

Step 2 — Implement SqlTransaction wrapping [depends on Step 1 confirming same-database; selected approach is local transaction]
Transformational prompt: Open one SqlConnection. Begin a transaction. Execute the INSERT into Orders. Execute the UPDATE to decrement Inventory where StockAvailable > 0 and capture the rows-affected count. If rows-affected is zero, the decrement failed because stock is insufficient — roll back the transaction and return a clear error to the caller. If rows-affected is one, commit the transaction. Both operations commit atomically or neither does. No saga, no event bus, no compensating transaction — ACID guarantees this at the database level.

Step 3 — Handle the insufficient-stock case explicitly [depends on Step 2]
Transformational prompt: When the inventory decrement returns rows-affected = 0, roll back and return an explicit structured error (not a generic exception): order_failed_reason = INSUFFICIENT_STOCK, item_id, quantity_requested, quantity_available. The caller needs to distinguish this failure from a database failure so it can surface the right message to the user or calling system.

Review point: Confirm that the SqlTransaction is disposed correctly in both the success and failure paths. A transaction left open blocks other database operations. Use a try/catch/finally or a using block to guarantee disposal.
Reason: Same-database operations are solved by SQL ACID transactions. The saga pattern adds a message bus, event handlers, idempotency requirements, and eventual consistency for a problem that a local transaction solves in 20 lines. The saga is the correct choice when order and inventory are in separate databases or separate services; it is not the correct choice here.
Source: https://github.com/DapperLib/Dapper — Dapper transaction documentation
License: Apache 2.0

Pair CB1-03-003 — Phrasing B
Source: https://github.com/DapperLib/Dapper — Dapper transaction documentation
License: Apache 2.0
A: [Atomic operation] Order service: insert order row, decrement inventory row. If the decrement fails (insufficient stock), the insert must not persist. Same SQL Server database. Team of 3, no message broker. Two candidate approaches: SqlTransaction or saga with compensating events.
[Decision context] Saga is appropriate when services are separate. Local transaction is appropriate when tables are co-located. The question is which pattern fits this case.
B:
Step 1 — Confirm database co-location [prerequisite]
Transformational prompt: Check that Orders and Inventory tables are in the same SQL Server instance and database. If yes, use a local SqlTransaction — ACID guarantees atomicity within a database. If they are in different databases or services, switch to the saga path and stop here.

Step 2 — SqlTransaction implementation [depends on co-location confirmed]
Transformational prompt: Open SqlConnection, begin transaction. Execute order INSERT. Execute inventory UPDATE with WHERE StockAvailable >= requested_quantity. Capture rows-affected. If 0: roll back, return INSUFFICIENT_STOCK error with item and quantity details. If 1: commit. The transaction guarantees that the INSERT and UPDATE either both persist or neither does.

Step 3 — Disposal guarantee [depends on Step 2]
Transformational prompt: Wrap transaction in a using block or ensure Dispose() is called in the finally branch. An open transaction blocks other connections. Confirm that both the success path and the rollback path call Dispose before returning.

---

Pair CB1-03-004 — Redis distributed cache versus in-process cache for a three-instance API
Source: https://docs.nestjs.com — NestJS caching documentation
License: MIT
A: [Caching architecture] A NestJS API serves a product catalog endpoint that is called 50,000 times per day. The catalog changes every 4 hours. The database query generating the catalog takes 800ms. The API runs as three instances behind a load balancer. The required cache TTL is 4 hours.
[Option analysis] Option A: in-process memory cache on each instance using @nestjs/cache-manager with the default memory store. Option B: Redis cache shared across all three instances via cache-manager-redis-yet.
B:
Step 1 — Confirm the catalog update handler can invoke CacheManager [prerequisite; determines whether the invalidation path is reachable]
Transformational prompt: Confirm that the handler or service responsible for updating the product catalog has an injection point where cacheManager.del('product-catalog') can be called through the NestJS DI (dependency injection) container. The Redis shared cache requires exactly one del() call per catalog update to clear the shared key across all three instances. If the update path is a background job, a cron task, or an external webhook handler that runs outside the NestJS application context, CacheManager is not injectable there and the invalidation path must be designed before Step 2 proceeds. Shared cache without a reachable invalidation point serves stale data indefinitely when the catalog changes faster than the TTL.

Step 2 — Configure Redis shared cache [depends on Step 1; selected when consistency or shared invalidation is required]
Transformational prompt: Install cache-manager-redis-yet. Register CacheModule with the Redis store, host, port, and TTL of 14,400 seconds (4 hours). Decorate the catalog endpoint handler with @CacheKey('product-catalog') and @CacheTTL(14400). The first request from any instance that reaches a cache miss fetches from the database and populates the shared Redis key. All subsequent requests from all three instances hit Redis (approximately 5ms) rather than the database (800ms). Cache invalidation: when the catalog is updated, call cacheManager.del('product-catalog') once — all three instances immediately serve fresh data on the next request.

Step 3 — Handle cache warm-up window [depends on Step 2]
Transformational prompt: The Redis cache starts cold on first deployment. The first request to the catalog endpoint will experience the 800ms database latency. This is acceptable for a TTL of 4 hours: it happens once every 4 hours per cache key, not once per instance. Document this warm-up period in the API's operational runbook so it is not misread as a performance regression.

Review point: Confirm that the Redis connection string is stored in a secret (not in the application configuration file). A Redis instance accessible without authentication is an open cache that any host on the network can read or flush.
Reason: In-process cache on three instances creates three independent caches. A cache invalidation must be sent to all three instances separately, and there is no guarantee all three receive it. Shared Redis cache eliminates this: one write to Redis, all three instances see the updated (or invalidated) value. The 50,000 requests/day volume makes the Redis round-trip (5ms) versus database (800ms) difference significant — the cache hit rate after warm-up is approximately 99.9% at this request volume and TTL.
Source: https://docs.nestjs.com — NestJS caching documentation
License: MIT

Pair CB1-03-004 — Phrasing B
Source: https://docs.nestjs.com — NestJS caching documentation
License: MIT
A: [API caching] NestJS API, three load-balanced instances. Product catalog endpoint: 50K requests/day, 800ms database query, catalog updates every 4 hours, cache TTL 4 hours. Two options: in-process memory cache per instance, or shared Redis cache across all instances.
[Consistency question] With in-process cache: each instance warms independently and invalidation must reach all three. With Redis: one shared cache, one invalidation clears all instances.
B:
Step 1 — Confirm catalog update handler can invoke CacheManager [prerequisite]
Transformational prompt: Confirm that the service handling catalog updates can call cacheManager.del('product-catalog') through the NestJS DI container. Shared Redis invalidation requires this call at each catalog update — if updates arrive through a background process or cron job that runs outside the NestJS application context, CacheManager cannot be injected there and the invalidation path must be designed before Step 2 proceeds. An invalidation-unreachable cache serves stale data after the TTL window when catalog updates are faster than TTL expiry.

Step 2 — Configure Redis shared cache [selected approach when invalidation is needed or when cache consistency matters]
Transformational prompt: Register CacheModule with Redis store, TTL 14,400s. Decorate the catalog handler with @CacheKey and @CacheTTL. First cache miss populates the Redis key once — all three instances serve from that key until TTL expires. For admin updates: inject CacheManager and call del('product-catalog') in the update handler.

Step 3 — Secure the Redis connection [depends on Step 2]
Transformational prompt: Redis connection string must include authentication credentials stored as environment secrets. Default Redis configuration has no authentication — without credentials, the cache is readable and writable by any host on the same network segment, including a compromised instance.
