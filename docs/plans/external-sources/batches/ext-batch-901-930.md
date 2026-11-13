# External Sources Batch 31 — EXT-611 through EXT-630

---

## EXT-611 | KUBERNETES-KEPS | Family 7 | Domain C

**Unit title:** Kubernetes admission webhooks — MutatingAdmissionWebhook vs ValidatingAdmissionWebhook for policy translation
**Source URL:** https://kubernetes.io/docs/reference/access-authn-authz/extensible-admission-controllers/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT611-F7-C",
  "sourceIds": ["EXT-611"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "admission-policy-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A platform team needs to enforce two policies: (1) all Deployments must have resource limits set (CPU/memory), (2) Deployments without resource limits should have sensible defaults injected automatically. They evaluate Mutating vs Validating admission webhooks.",
    "b": "MutatingAdmissionWebhook runs before ValidatingAdmissionWebhook and can modify the incoming object. ValidatingAdmissionWebhook can only approve or reject. For auto-injection of resource defaults: Mutating. For hard enforcement that rejects non-compliant resources: Validating.",
    "c": "Using both: Mutating injects defaults for missing limits; Validating then confirms limits are present (catches cases where explicit limits were set but invalid). Order: Mutating runs first, then Validating sees the mutated object."
  },
  "function": "Translate admission webhook policy selection for Kubernetes resource compliance",
  "acceptedOutput": "Implement both webhooks: (1) MutatingAdmissionWebhook that injects default resource limits (cpu: 100m/500m, memory: 128Mi/256Mi) when limits are absent. (2) ValidatingAdmissionWebhook that rejects Deployments with limits exceeding cluster maximums or with invalid values. The mutating webhook handles the developer experience; the validating webhook enforces hard constraints.",
  "rejectedOutput": "Use only a ValidatingAdmissionWebhook that rejects Deployments without resource limits. Developers must always specify limits explicitly.",
  "arbiters": ["api-translation-arbiter", "admission-policy-arbiter"],
  "whyAccepted": "Mutating + Validating in sequence provides both developer ergonomics (defaults injected) and hard policy enforcement. Developers are not rejected for routine omissions; they are only rejected for policy violations that defaults cannot correct.",
  "whyRejected": "Reject-only validation creates friction for all developers without providing guidance on correct values. A Mutating webhook converts a compliance failure into a success for the common case (absent limits) while still allowing Validation to catch invalid explicit limits.",
  "sourceAnchors": ["https://kubernetes.io/docs/reference/access-authn-authz/extensible-admission-controllers/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-612 | BIGCODEBENCH | Family 1 | Domain A

**Unit title:** Python asyncio event loop topology — single vs multiple event loops in multi-service architecture
**Source URL:** https://bigcode-bench.github.io/
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT612-F1-A",
  "sourceIds": ["EXT-612"],
  "sourceTrustTier": 2,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "async-event-loop-topology",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A Python application creates multiple asyncio event loops using asyncio.new_event_loop() in different threads to run independent async services. The team discovers that asyncio primitives (Queue, Event) created in one loop cannot be safely used from another loop.",
    "b": "asyncio is single-threaded per event loop — all coroutines within an event loop run on the same OS thread. Cross-loop communication requires thread-safe primitives (queue.Queue, threading.Event) or asyncio.run_coroutine_threadsafe(). Multiple event loops in one process are valid but complex.",
    "c": "The recommended Python asyncio architecture: one event loop per process (or one per thread if necessary). Use asyncio.TaskGroup or asyncio.gather for concurrency within one loop. Use multiprocessing or separate processes for true CPU parallelism with independent event loops."
  },
  "function": "Design asyncio event loop topology for multi-service Python architecture",
  "acceptedOutput": "Use a single asyncio event loop per process. Run independent services as asyncio Tasks within the same loop: async def main(): async with asyncio.TaskGroup() as tg: tg.create_task(service_a()); tg.create_task(service_b()). For cross-thread communication, use asyncio.Queue with loop.call_soon_threadsafe() or asyncio.run_coroutine_threadsafe().",
  "rejectedOutput": "Create one event loop per service in a separate thread. Each service runs independently without sharing loop state.",
  "arbiters": ["topology-arbiter", "concurrency-design-arbiter"],
  "whyAccepted": "Single event loop per process is the asyncio design intent. Tasks within one loop cooperate on one thread with shared state safety. Cross-task communication via asyncio.Queue is straightforward and does not require thread-safe wrappers.",
  "whyRejected": "Multiple loops in separate threads require thread-safe bridges for any communication between services. asyncio.Queue is not thread-safe across loops. The complexity of cross-loop communication eliminates the simplicity that asyncio provides.",
  "sourceAnchors": ["https://docs.python.org/3/library/asyncio-eventloop.html"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-613 | SWE-BENCH | Family 5 | Domain E

**Unit title:** Python rate limiting — token bucket vs fixed window algorithm arbitration for API protection
**Source URL:** https://github.com/princeton-nlp/SWE-bench
**License:** MIT | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT613-F5-E",
  "sourceIds": ["EXT-613"],
  "sourceTrustTier": 2,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "rate-limiting-algorithm-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Python API needs rate limiting at 100 requests/minute per client. The team implements fixed window counting: reset counter every 60 seconds. A client sending 100 requests at 0:59 and 100 requests at 1:01 effectively sends 200 requests in a 2-second window.",
    "b": "Fixed window: count resets at interval boundaries, allowing 2× bursts at window edges. Sliding window log: tracks exact request timestamps, prevents boundary bursts but requires O(n) memory per client. Token bucket: clients accrue tokens at a steady rate (100/60 = 1.67/sec), can burst up to bucket capacity. Leaky bucket: processes requests at steady rate, queuing excess.",
    "c": "Token bucket is the most common algorithm for API rate limiting: it allows controlled bursting (up to bucket capacity) while maintaining the average rate constraint. Redis with INCR + EXPIRE or the sliding window algorithm with Redis sorted sets are common implementations."
  },
  "function": "Arbitrate rate limiting algorithm for API burst protection",
  "acceptedOutput": "Implement token bucket with Redis: bucket capacity = 100 tokens, refill rate = 100 tokens/60 seconds. Use Redis Lua script for atomic check-and-decrement. Clients can burst up to 100 requests instantly but cannot exceed the average 100/min rate. Return X-RateLimit-Remaining and Retry-After headers on 429 responses.",
  "rejectedOutput": "Keep fixed window. The boundary burst case is statistically rare and acceptable for the use case.",
  "arbiters": ["security-arbiter", "rate-limiting-arbitration-arbiter"],
  "whyAccepted": "Token bucket prevents the 2× boundary burst while allowing legitimate short bursts (clients sending occasional concentrated requests stay within the bucket capacity). The boundary burst in fixed window is exploitable by clients timing requests to the window boundary.",
  "whyRejected": "'Statistically rare' understates the problem: a client aware of the window reset time can reliably exploit the boundary burst, effectively doubling their allowed rate. For security-sensitive APIs, this is an exploitable vulnerability, not an edge case.",
  "sourceAnchors": ["https://redis.io/commands/incr/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-614 | PYTHON-PEPS | Family 6 | Domain F

**Unit title:** PEP-525 asynchronous generators for streaming result production in Python
**Source URL:** https://peps.python.org/pep-0525/
**License:** PSF-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT614-F6-F",
  "sourceIds": ["EXT-614"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "streaming-result-production",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A FastAPI endpoint streams large database result sets to the client. The current approach fetches all rows into a list, then returns them. Memory pressure causes OOM (Out of Memory) errors for large result sets.",
    "b": "PEP-525 async generators: async functions with yield statements. FastAPI supports StreamingResponse with an async generator body. The generator yields JSON-encoded rows as they are fetched from the database cursor, never materializing the full result set in memory.",
    "c": "Streaming requires the client to handle a streaming response (JSON Lines or Server-Sent Events format) rather than a single JSON array. Connection must remain open for the full stream duration."
  },
  "function": "Apply async generator for streaming large result set production",
  "acceptedOutput": "Implement async generator: async def generate_rows(cursor): async for row in cursor: yield json.dumps(row.dict()) + '\\n'. Return StreamingResponse(generate_rows(cursor), media_type='application/x-ndjson'). Database cursor is iterated asynchronously, each row yielded immediately, connection held open for stream duration.",
  "rejectedOutput": "Implement server-side pagination: return pages of 1000 rows with a nextPageToken cursor. Clients make multiple requests to retrieve all data.",
  "arbiters": ["registry-storage-arbiter", "streaming-production-arbiter"],
  "whyAccepted": "Async generator streaming processes rows at O(1) memory regardless of result size. The client receives results progressively. For large exports (millions of rows), streaming is the only feasible approach.",
  "whyRejected": "Pagination is correct for interactive UIs but inappropriate for bulk data export — it requires N round trips for N pages, each with HTTP overhead. A client downloading 1M rows in 1000-row pages makes 1000 HTTP requests. Streaming is one connection, zero round trips.",
  "sourceAnchors": ["https://peps.python.org/pep-0525/"],
  "licenseConstraint": "PSF-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-615 | OPENTELEMETRY-SPEC | Family 8 | Domain G

**Unit title:** OTel metrics SDK — push vs pull model for learning pipeline observability decisions
**Source URL:** https://opentelemetry.io/docs/specs/otel/metrics/sdk/
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT615-F8-G",
  "sourceIds": ["EXT-615"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "metrics-collection-model-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A team deploys OTel metrics for their microservices. They evaluate: (1) push model — services push metrics to the Collector via OTLP at regular intervals; (2) pull model — Prometheus scrapes metrics from a /metrics endpoint on each service.",
    "b": "Push (OTLP): services control when metrics are sent; Collector aggregates. Suitable for batch jobs and containers with dynamic IPs. Pull (Prometheus): Prometheus controls scrape schedule; service exposes endpoint. Requires stable scrape target addresses (Service DNS in Kubernetes). OTel SDK supports both via OTLP exporter (push) and Prometheus exporter (pull).",
    "c": "In Kubernetes: Prometheus pull works well with Service discovery. For batch/job workloads that terminate before being scraped: push is required. OTel Collector can bridge push to Prometheus pull via the prometheusremotewrite exporter."
  },
  "function": "Record metrics collection model selection for learning pipeline observability",
  "acceptedOutput": "Use push (OTLP) for batch/ML training jobs that terminate before Prometheus can scrape them. Use pull (Prometheus) for long-running services in Kubernetes where scrape targets are stable. Use OTel Collector as the collection hub: services push OTLP to Collector; Collector exposes Prometheus endpoint for existing Prometheus infrastructure.",
  "rejectedOutput": "Use only Prometheus pull for all services. Batch jobs should write metrics to a file that is read by a Pushgateway.",
  "arbiters": ["learning-arbiter", "observability-model-arbiter"],
  "whyAccepted": "Hybrid: push for ephemeral jobs (no scrape target), pull for long-running services (Prometheus ecosystem). OTel Collector as a bridge enables mixing without changing existing Prometheus infrastructure.",
  "whyRejected": "Pushgateway is a known Prometheus anti-pattern for batch jobs — it does not handle job restarts correctly (stale metrics remain after job completion) and is a single point of failure. OTLP push to the Collector is the OTel-native solution for ephemeral jobs.",
  "sourceAnchors": ["https://opentelemetry.io/docs/specs/otel/metrics/sdk/"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-616 | DAPPER | Family 7 | Domain C

**Unit title:** Dapper query result mapping to API response DTOs — AutoMapper vs manual projection translation
**Source URL:** https://github.com/DapperLib/Dapper
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT616-F7-C",
  "sourceIds": ["EXT-616"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "result-to-dto-translation-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Dapper repository returns mapped domain objects (Order, Customer). The API layer needs to translate these to response DTOs (OrderResponse, CustomerSummary) for the client. The team evaluates AutoMapper vs manual mapping.",
    "b": "AutoMapper uses convention-based mapping (same-name properties map automatically) with explicit configuration for deviations. Manual mapping is explicit: new OrderResponse { Id = order.Id, CustomerName = order.Customer.Name, TotalAmount = order.Items.Sum(i => i.Price) }. AutoMapper adds a dependency and 'magic' that can silently map wrong properties; manual mapping is verbose but explicit.",
    "c": "AutoMapper's ProjectTo() integrates with IQueryable for SQL-level projection. For Dapper (which already executes SQL), this integration does not apply — AutoMapper provides only in-memory mapping over Dapper results."
  },
  "function": "Translate Dapper query results to API response DTOs",
  "acceptedOutput": "Use manual mapping for Dapper results to response DTOs. The absence of IQueryable projection means AutoMapper provides no Dapper-specific benefit. Create explicit mapping: static OrderResponse ToResponse(Order o) => new OrderResponse { Id = o.Id, CustomerName = o.Customer.Name, ... }. Mapping logic is visible, testable, and not subject to AutoMapper convention magic.",
  "rejectedOutput": "Use AutoMapper. Convention-based mapping reduces boilerplate for the many properties that have the same name.",
  "arbiters": ["api-translation-arbiter", "mapping-strategy-arbiter"],
  "whyAccepted": "For Dapper projects, AutoMapper's primary advantage (IQueryable projection to SQL) is unavailable. The remaining value — convention-based property mapping — trades explicitness for convenience. Manual mapping makes the translation visible at the call site and avoids AutoMapper's silent mapping of wrong-type properties with the same name.",
  "whyRejected": "AutoMapper reduces boilerplate for same-name properties but introduces silent failure modes: a property renamed in the domain type stops mapping without compile-time error (if not using ForMember). The reduced boilerplate is not worth the reduced explicitness for a Dapper project without IQueryable.",
  "sourceAnchors": ["https://github.com/DapperLib/Dapper"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-617 | ANGULAR-DOCS | Family 6 | Domain F

**Unit title:** Angular 17 NgOptimizedImage for Core Web Vitals LCP (Largest Contentful Paint) optimization
**Source URL:** https://angular.dev/guide/image-optimization
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT617-F6-F",
  "sourceIds": ["EXT-617"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "image-optimization-result-delivery",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An Angular application has poor Core Web Vitals scores, particularly LCP (Largest Contentful Paint). The hero image loads slowly. The team uses <img [src]=\"heroUrl\"> without optimization.",
    "b": "Angular 17 NgOptimizedImage directive (NgOptimizedImage) adds: automatic lazy loading for below-fold images, priority loading (preload link) for LCP images, intrinsic size enforcement (prevents layout shift), automatic srcset for responsive images, and broken src warnings. Applied via ngSrc instead of src.",
    "c": "LCP images should have fetchpriority=high and be preloaded. Angular's NgOptimizedImage generates the correct link rel=preload header for priority images automatically."
  },
  "function": "Apply NgOptimizedImage for LCP image result delivery optimization",
  "acceptedOutput": "Replace <img [src]=\"heroUrl\" width=\"1200\" height=\"600\"> with <img ngSrc=\"heroUrl\" width=\"1200\" height=\"600\" priority>. The priority attribute generates a preload link, sets fetchpriority=high, and disables lazy loading for the LCP image. For below-fold images: use ngSrc without priority (auto-lazy loads). Import NgOptimizedImage from @angular/common.",
  "rejectedOutput": "Manually add <link rel=\"preload\" as=\"image\" href=\"heroUrl\"> in index.html. This achieves the same preloading without migrating to NgOptimizedImage.",
  "arbiters": ["registry-storage-arbiter", "image-delivery-arbiter"],
  "whyAccepted": "NgOptimizedImage provides preloading, responsive srcset generation, and intrinsic size enforcement in one directive. The manual preload link only covers the static URL — for dynamic hero images (CMS-driven), a template-generated preload link requires additional Angular server-side rendering integration. NgOptimizedImage handles dynamic URLs natively.",
  "whyRejected": "Manual link rel=preload in index.html is static — it cannot reference dynamic image URLs determined at component render time. For CMS-driven images, the preload URL must match the runtime image URL exactly. NgOptimizedImage generates the preload at the template level, matching the rendered src.",
  "sourceAnchors": ["https://angular.dev/guide/image-optimization"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-618 | REACT-ADVANCED | Family 8 | Domain G

**Unit title:** React micro-frontend architecture — Module Federation vs iframe vs web components for learning
**Source URL:** https://module-federation.io/
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT618-F8-G",
  "sourceIds": ["EXT-618"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "micro-frontend-architecture-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A large organization wants to decompose a monolithic React frontend into micro-frontends owned by separate teams. Each team has an independent deployment pipeline. The team evaluates: Module Federation (Webpack 5), iframes, and Web Components.",
    "b": "Module Federation: shares runtime between host and remotes, supports shared dependencies (single React instance), enables seamless component integration but requires Webpack 5 and coordinated version management. iframes: complete isolation, separate origins, no shared state or styles, poor UX for integrated experiences. Web Components: framework-agnostic, shadow DOM isolation, but React integration requires wrapper components and complex event communication.",
    "c": "Module Federation is the industry standard for React micro-frontends. Vite-based projects use the @originjs/vite-plugin-federation plugin. Native federation (without Webpack) is possible via importmap-based approaches."
  },
  "function": "Record micro-frontend architecture selection for large React platform learning",
  "acceptedOutput": "Use Module Federation for React micro-frontends with teams sharing a React codebase. Configure shared: { react: { singleton: true }, 'react-dom': { singleton: true } } to prevent multiple React instances. Each team exposes components via remoteEntry.js and deploys independently. The host app loads remote components at runtime without a build-time dependency.",
  "rejectedOutput": "Use iframes for complete team isolation. Each team deploys independently without coordination on shared libraries.",
  "arbiters": ["learning-arbiter", "micro-frontend-architecture-arbiter"],
  "whyAccepted": "Module Federation provides independent deployment with shared React runtime — teams deploy their remote independently; the host loads updates at runtime without rebuilding. Singleton shared React prevents version conflicts and duplicate React instances (which break hooks).",
  "whyRejected": "iframes provide isolation but poor integration: cross-iframe communication requires postMessage, styles cannot be shared, the user experience has visible boundaries (scroll, focus), and responsive design is complex. For a product that should feel integrated, iframes create a disjointed UX.",
  "sourceAnchors": ["https://module-federation.io/"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-619 | ASPNETCORE-DOCS-SAMPLES | Family 8 | Domain G

**Unit title:** ASP.NET Core SignalR hub lifetime management — per-hub vs singleton state for persistent connections
**Source URL:** https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT619-F8-G",
  "sourceIds": ["EXT-619"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "persistent-connection-state-learning",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A developer stores connected client IDs in a List<string> field on the SignalR Hub class to track online users. The list appears empty despite clients being connected. The developer observes that each hub method invocation creates a new hub instance.",
    "b": "SignalR Hubs are transient — a new Hub instance is created for each method invocation, then disposed. State stored in Hub instance fields is lost between method calls. For persistent state (connected users, room membership), use: IHubContext<T> with a singleton service, or IMemoryCache, or a database.",
    "c": "The Hub's context provides stable identifiers: Context.ConnectionId (unique per connection), Clients.All, Groups. Persistent group membership (group tracking) is built into SignalR via Groups.AddToGroupAsync()."
  },
  "function": "Record SignalR hub lifetime anti-pattern for connection state management learning",
  "acceptedOutput": "Move connected client tracking to a singleton service: class ConnectionTracker { private readonly ConcurrentDictionary<string, UserInfo> _connections = new(); public void AddConnection(string connectionId, UserInfo user) ... }. Inject ConnectionTracker into the hub via constructor DI. The singleton persists across hub invocations. Use OnConnectedAsync/OnDisconnectedAsync to add/remove entries.",
  "rejectedOutput": "Use a static Dictionary on the Hub class to share state across instances: private static Dictionary<string, UserInfo> _connections = new().",
  "arbiters": ["learning-arbiter", "hub-lifetime-arbiter"],
  "whyAccepted": "Singleton service injected via DI is the correct lifetime for cross-request state. It is testable (injectable mock), thread-safe with ConcurrentDictionary, and clearly scoped (not shared via static state across all application domains).",
  "whyRejected": "Static Dictionary is not thread-safe under concurrent SignalR connections. Multiple clients connecting simultaneously cause race conditions. Static state also cannot be reset between test runs and leaks across AppDomain boundaries in some hosting scenarios.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/aspnet/core/signalr/hubs"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-620 | NESTJS-ADVANCED | Family 3 | Domain D

**Unit title:** NestJS event emitter vs RxJS Subject for in-process domain event execution
**Source URL:** https://docs.nestjs.com/techniques/events
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT620-F3-D",
  "sourceIds": ["EXT-620"],
  "sourceTrustTier": 1,
  "datasetFamily": 3,
  "domain": "D",
  "subdomain": "domain-event-execution-strategy",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A NestJS service emits domain events (OrderPlaced, PaymentProcessed) to trigger side effects (send email, update analytics, notify warehouse). The team debates: @nestjs/event-emitter vs RxJS Subject for in-process event dispatch.",
    "b": "@nestjs/event-emitter provides decorator-based subscription (@OnEvent('order.placed')), async handler support, wildcard patterns, and integration with NestJS DI. RxJS Subject is a push-based Observable — subscribers receive values pushed to the Subject, with full RxJS operator support (filter, debounceTime, mergeMap).",
    "c": "Both are in-process (not distributed). For cross-service events, use a message broker. @nestjs/event-emitter is simpler for fire-and-forget domain events. RxJS Subject is better when operators (buffer, throttle, flatMap) are needed on the event stream."
  },
  "function": "Select in-process domain event dispatch mechanism for NestJS application",
  "acceptedOutput": "Use @nestjs/event-emitter for domain events requiring fire-and-forget dispatch to multiple handlers: eventEmitter.emit('order.placed', new OrderPlacedEvent(orderId)). Handlers use @OnEvent('order.placed') with async support. Use RxJS Subject when event stream processing (buffering, throttling, reactive composition) is required.",
  "rejectedOutput": "Use RxJS Subject for all domain events. It provides more power and flexibility for future stream processing needs.",
  "arbiters": ["execution-applier-arbiter", "event-dispatch-arbiter"],
  "whyAccepted": "@nestjs/event-emitter matches the domain event pattern: fire-and-forget, multiple independent subscribers, NestJS DI integration. The decorator-based subscription is explicit and readable. RxJS Subject is appropriate when the event stream needs reactive operators.",
  "whyRejected": "Using RxJS Subject for simple fire-and-forget events adds unnecessary complexity. 'Future needs' is not a justification for current complexity. If stream processing is needed, refactor then. Premature Subject introduces Subscription management overhead in each handler.",
  "sourceAnchors": ["https://docs.nestjs.com/techniques/events"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-621 | IETF-RFCS | Family 1 | Domain A

**Unit title:** RFC-7540 HTTP/2 multiplexing vs HTTP/1.1 connection topology for API gateway
**Source URL:** https://www.rfc-editor.org/rfc/rfc7540
**License:** IETF-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT621-F1-A",
  "sourceIds": ["EXT-621"],
  "sourceTrustTier": 1,
  "datasetFamily": 1,
  "domain": "A",
  "subdomain": "http-connection-topology",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API gateway proxies requests to 10 backend microservices. Under load, the gateway exhausts its HTTP/1.1 connection pools (6 connections per host limit in browsers, unlimited for server-to-server but with overhead). The team evaluates upgrading upstream connections to HTTP/2.",
    "b": "HTTP/2 multiplexes multiple requests over a single TCP connection using streams. HTTP/1.1 requires one connection per concurrent request (or pipelining, which is fragile). HTTP/2 eliminates the connection pool overhead: one connection to each upstream handles thousands of concurrent streams.",
    "c": "HTTP/2 requires TLS in practice (h2 via ALPN). HTTP/2 can cause head-of-line blocking at the TCP level (all streams on one connection stall on packet loss). HTTP/3 (QUIC) eliminates TCP HOL (Head-of-Line Blocking)."
  },
  "function": "Select HTTP connection topology for API gateway upstream connections",
  "acceptedOutput": "Upgrade upstream connections from the API gateway to backend services to HTTP/2. Configure the gateway to maintain one persistent HTTP/2 connection per upstream service, multiplexing all concurrent requests as streams. This eliminates connection pool management overhead and reduces TLS handshake costs.",
  "rejectedOutput": "Keep HTTP/1.1 and increase the connection pool size from default to 200 connections per upstream. This accommodates more concurrent requests.",
  "arbiters": ["topology-arbiter", "connection-pool-efficiency-arbiter"],
  "whyAccepted": "HTTP/2 multiplexing eliminates the fundamental N-connections-for-N-concurrent-requests constraint of HTTP/1.1. One connection per upstream handles unlimited concurrent streams. Connection overhead (TLS handshake, TCP slow start) is paid once per upstream, not per request.",
  "whyRejected": "Increasing HTTP/1.1 pool size increases connection overhead proportionally. 200 connections per upstream × 10 upstreams = 2000 connections with TLS state, buffers, and TCP windows. HTTP/2 achieves the same concurrency with 10 connections total.",
  "sourceAnchors": ["https://www.rfc-editor.org/rfc/rfc7540"],
  "licenseConstraint": "IETF-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-622 | GOOGLE-AIP | Family 6 | Domain F

**Unit title:** AIP-136 custom methods — when to use custom vs standard CRUD for resource operations
**Source URL:** https://google.aip.dev/136
**License:** Apache-2.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT622-F6-F",
  "sourceIds": ["EXT-622"],
  "sourceTrustTier": 1,
  "datasetFamily": 6,
  "domain": "F",
  "subdomain": "api-operation-classification-storage",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An API has an Order resource with operations: CreateOrder, GetOrder, UpdateOrder, CancelOrder, RefundOrder. The team debates whether CancelOrder and RefundOrder should be custom methods or modeled as UpdateOrder with a status field.",
    "b": "AIP-136: use standard methods (Create, Get, Update, Delete) when they naturally fit the operation semantics. Use custom methods when the operation: (1) has side effects beyond resource state change (external payment system call), (2) changes state in ways not expressible as a field update (transition that can't be reversed), (3) requires different validation than Update.",
    "c": "CancelOrder triggers a cancellation workflow (warehouse hold release, refund initiation, email notification). RefundOrder calls the payment gateway. These are not simple field updates — they have side effects and are not reversible via another Update."
  },
  "function": "Classify API operations as standard vs custom method for resource storage design",
  "acceptedOutput": "Use standard UpdateOrder for simple field changes (delivery address, notes). Use custom methods for operations with external side effects: POST /orders/{id}:cancel and POST /orders/{id}:refund. Custom methods use the AIP-136 colon syntax, accept a dedicated request message, and return the updated Order.",
  "rejectedOutput": "Model all operations as UpdateOrder with a status field: PATCH /orders/{id} with {\"status\": \"CANCELLED\"} or {\"status\": \"REFUNDED\"}. Status-driven state machines keep the API surface minimal.",
  "arbiters": ["registry-storage-arbiter", "api-operation-classification-arbiter"],
  "whyAccepted": "Custom methods make external side effects explicit in the API contract. A client calling :cancel knows it triggers a workflow, not just a field update. A status PATCH that triggers payment gateway calls is a misleading contract — it looks like a simple update but has significant external side effects.",
  "whyRejected": "Status-field updates hide side effects behind a simple PATCH. Clients cannot distinguish a side-effect-free status change from one that calls external systems. Validation, authorization, and retry semantics differ between a simple update and a workflow-triggering operation — merging them into one endpoint increases complexity without benefit.",
  "sourceAnchors": ["https://google.aip.dev/136"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-623 | OPENASSISTANT-OASST1 | Family 8 | Domain G

**Unit title:** RLHF preference data quality — inter-annotator agreement threshold for reward model training
**Source URL:** https://huggingface.co/datasets/OpenAssistant/oasst1
**License:** Apache-2.0 | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT623-F8-G",
  "sourceIds": ["EXT-623"],
  "sourceTrustTier": 2,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "preference-data-quality-threshold",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A reward model training pipeline uses oasst1 preference pairs. The team sets an inter-annotator agreement threshold: only include pairs where all annotators agree (100% agreement). This discards 45% of available pairs.",
    "b": "100% agreement threshold gives highest-confidence training signal but discards nearly half the data. 66% agreement (2/3 majority) keeps more data but includes noisier pairs. Research shows reward models benefit from data quantity up to a noise threshold — moderate noise with high volume can outperform clean signal with low volume.",
    "c": "The optimal threshold depends on annotator count, domain specificity, and base model size. oasst1 has 3-5 annotators per comparison. Cohen's kappa or Fleiss' kappa measures actual agreement beyond chance."
  },
  "function": "Set inter-annotator agreement threshold for RLHF preference data quality filter",
  "acceptedOutput": "Use 66% agreement threshold (majority): include pairs where ≥ 2/3 annotators agree on the preferred response. Train two reward models: one with 66% threshold (larger dataset) and one with 100% threshold (cleaner data). Compare validation set Pearson correlation with held-out human preferences to select the better model empirically rather than assuming either threshold is optimal.",
  "rejectedOutput": "Use 100% agreement only. Training on disagreed pairs introduces noise that degrades reward model calibration.",
  "arbiters": ["learning-quality-arbiter", "data-threshold-calibration-arbiter"],
  "whyAccepted": "Empirical comparison of both thresholds determines the actual quality/quantity trade-off for this dataset and model size. The 100%-only assumption is not validated — smaller but cleaner datasets can produce worse reward models if the volume is insufficient for the model to generalize.",
  "whyRejected": "Assuming 100% agreement always produces better reward models conflates data cleanliness with model quality. Insufficient training data causes underfitting that degrades model quality more than moderate noise. The optimal threshold is empirically determined, not a priori assumed.",
  "sourceAnchors": ["https://huggingface.co/datasets/OpenAssistant/oasst1"],
  "licenseConstraint": "Apache-2.0",
  "trainingStatus": "draft"
}
```

---

## EXT-624 | TATOEBA | Family 2 | Domain C

**Unit title:** Tatoeba sentence pair deduplication — exact vs near-duplicate detection for translation datasets
**Source URL:** https://tatoeba.org/en/downloads
**License:** CC-BY-2.0-FR | **Trust tier:** 2

**Training row:**
```json
{
  "rowId": "ROW-EXT624-F2-C",
  "sourceIds": ["EXT-624"],
  "sourceTrustTier": 2,
  "datasetFamily": 2,
  "domain": "C",
  "subdomain": "dataset-deduplication-quality",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A translation dataset built from Tatoeba uses exact string matching for deduplication. Post-training analysis reveals the model memorizes near-duplicate sentences (e.g., 'Hello world.' vs 'Hello, world!' vs 'Hello World') as separate training examples, inflating training diversity metrics without adding genuine diversity.",
    "b": "Exact deduplication misses near-duplicates: punctuation variations, capitalization differences, trailing whitespace, and contributor paraphrases of the same sentence. MinHash LSH (Locality-Sensitive Hashing) detects near-duplicates at configurable Jaccard similarity thresholds.",
    "c": "For translation pairs, deduplication should consider source and target jointly — same source with different valid translations should be retained (genuine diversity). Exact-source + different-target pairs are valid. Near-duplicate source pairs should be deduplicated even if targets differ."
  },
  "function": "Apply near-duplicate detection for Tatoeba translation dataset deduplication",
  "acceptedOutput": "Apply MinHash LSH at Jaccard similarity 0.85 on the source sentences after normalization (lowercase, strip punctuation). Cluster near-duplicates and retain one representative per cluster. Retain multiple translations for genuinely unique source sentences. This removes paraphrase clusters while preserving translational diversity.",
  "rejectedOutput": "Use exact string deduplication on the source sentences. Near-duplicates are genuinely different sentences and should be retained.",
  "arbiters": ["translation-quality-arbiter", "dataset-deduplication-arbiter"],
  "whyAccepted": "MinHash LSH at 0.85 Jaccard removes punctuation variants and minor paraphrases while preserving genuinely distinct sentences. Normalization before hashing ensures 'Hello world.' and 'Hello, world!' cluster together. Retaining multiple translations per unique source preserves genuine diversity.",
  "whyRejected": "Exact deduplication leaves near-duplicates that overrepresent common sentence patterns. 'Hello world' in 5 punctuation variants trains the model 5 times on essentially the same example, over-weighting trivial variation relative to genuine linguistic diversity.",
  "sourceAnchors": ["https://tatoeba.org/en/downloads"],
  "licenseConstraint": "CC-BY-2.0-FR",
  "trainingStatus": "draft"
}
```

---

## EXT-625 | VUE-DOCS | Family 8 | Domain G

**Unit title:** Vue 3 SSR (Server-Side Rendering) vs SSG (Static Site Generation) — rendering mode learning
**Source URL:** https://vuejs.org/guide/scaling-up/ssr
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT625-F8-G",
  "sourceIds": ["EXT-625"],
  "sourceTrustTier": 1,
  "datasetFamily": 8,
  "domain": "G",
  "subdomain": "rendering-mode-selection-learning",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A Vue 3 marketing website with 200 pages needs good SEO (Search Engine Optimization) and fast initial load. The content changes infrequently (product descriptions, blog posts updated weekly). The team evaluates: CSR (Client-Side Rendering), SSR (Server-Side Rendering) with Nuxt, and SSG (Static Site Generation) with Nuxt generate.",
    "b": "CSR: JS bundle rendered in browser, poor SEO without SSR. SSR: server renders HTML per request, good SEO, requires Node.js server, higher TTFB (Time To First Byte) under load. SSG: HTML pre-rendered at build time, served as static files from CDN, best performance, no server required. SSG requires rebuild on content changes.",
    "c": "Nuxt 3 supports ISR (Incremental Static Regeneration) and hybrid rendering: routes can be configured as static, SSR, or SPA per page. For rarely-changing marketing content, SSG with periodic rebuilds is optimal."
  },
  "function": "Record Vue 3 rendering mode selection for infrequently-updated marketing site learning",
  "acceptedOutput": "Use Nuxt generate (SSG) for the marketing website. Pre-render all 200 pages as static HTML at build time. Deploy to CDN. Configure CI/CD to rebuild on content changes (webhook from CMS). Time-to-first-byte from CDN edge: ~50ms vs 200-500ms for SSR. Use Nuxt's ISR for pages that change more frequently without full rebuilds.",
  "rejectedOutput": "Use SSR with Nuxt. Marketing sites need to serve up-to-date content on every request; SSG requires a rebuild for every content change.",
  "arbiters": ["learning-arbiter", "rendering-mode-arbiter"],
  "whyAccepted": "SSG provides CDN-speed delivery of pre-rendered HTML — optimal for infrequently-changing content. A weekly CMS update triggers a rebuild of all 200 pages (typically under 5 minutes). The performance and cost advantages of static hosting vastly outweigh the rebuild cycle for weekly-update content.",
  "whyRejected": "SSR for weekly-changing content requires a Node.js server running 24/7, paying for compute that primarily serves cached content. The 'up-to-date on every request' argument does not apply to content changing weekly — a CDN with a 1-hour cache max-age serves content effectively as fresh for users.",
  "sourceAnchors": ["https://vuejs.org/guide/scaling-up/ssr"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-626 | EFCORE-SOURCE | Family 7 | Domain C

**Unit title:** EF Core raw SQL translation — FromSqlRaw for stored procedure result mapping to API responses
**Source URL:** https://learn.microsoft.com/en-us/ef/core/querying/raw-sql
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT626-F7-C",
  "sourceIds": ["EXT-626"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "stored-procedure-result-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A legacy database has stored procedures that return complex result sets (multiple columns from JOINs not matching any single entity). The team needs to call these procedures from EF Core and map results to API response DTOs.",
    "b": "EF Core FromSqlRaw/FromSqlInterpolated works on DbSet<T> — the SQL result columns must match the entity's mapped properties. For ad-hoc result shapes (not matching any DbSet entity), EF Core Keyless Entity Types (HasNoKey()) allow mapping arbitrary query results to a class with matching properties.",
    "c": "Alternative: use Dapper for raw SQL result mapping when the query result does not fit any EF Core entity. EF Core and Dapper can share the same SqlConnection/DbContext connection."
  },
  "function": "Translate stored procedure results to API response DTOs via EF Core keyless entity types",
  "acceptedOutput": "For stored procedure results mapping to an API DTO: define a keyless entity class matching the procedure's result columns (public class OrderSummaryResult { public int OrderId; public string CustomerName; public decimal Total; }). Configure: modelBuilder.Entity<OrderSummaryResult>().HasNoKey(). Query: context.Set<OrderSummaryResult>().FromSqlRaw(\"EXEC GetOrderSummaries @p0\", fromDate).ToList().",
  "rejectedOutput": "Use ExecuteSqlRaw and manually map results via SqlDataReader.",
  "arbiters": ["api-translation-arbiter", "stored-procedure-result-arbiter"],
  "whyAccepted": "Keyless entity types provide type-safe column mapping for arbitrary query results without defining a primary key entity. EF Core handles column-to-property mapping, DbNull handling, and type conversion. The result class serves directly as the API response DTO.",
  "whyRejected": "Manual SqlDataReader mapping is verbose, error-prone (column ordinals break on schema changes), and does not benefit from EF Core's null handling and type conversion. Keyless entities achieve the same result with declarative configuration.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/ef/core/querying/raw-sql"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-627 | DOTNET-MICROSERVICES | Family 5 | Domain E

**Unit title:** .NET distributed tracing — Activity API vs manual span creation for security arbitration
**Source URL:** https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs
**License:** MIT | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT627-F5-E",
  "sourceIds": ["EXT-627"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "distributed-tracing-security-arbitration",
  "rowClass": "anti-pattern-detection",
  "input": {
    "a": "A .NET service propagates trace context by reading the traceparent HTTP header and including it in outbound requests. The security team flags that the service accepts and propagates any traceparent header from untrusted external clients, potentially allowing trace context injection.",
    "b": "W3C TraceContext (traceparent) propagation from external clients allows an attacker to set a specific trace_id, causing their requests to appear under a controlled trace ID in internal observability systems. This can be used for trace ID injection to hide activities or cause trace collision.",
    "c": "Best practice: accept trace context from trusted internal services (same mesh, authenticated headers). For external (internet-facing) entry points: generate a new trace context, ignoring any incoming traceparent. Propagate the generated context downstream."
  },
  "function": "Arbitrate trace context propagation security for external-facing .NET service",
  "acceptedOutput": "At the internet-facing API gateway, configure the OTel SDK to ignore incoming traceparent headers: set the sampler to always generate a new root span. Propagate the generated trace context to internal downstream services. Internal service-to-service calls (within the trusted mesh) should continue propagating trace context. This prevents external trace injection while maintaining internal trace continuity.",
  "rejectedOutput": "Accept incoming traceparent headers from all clients. Trace injection is an observability concern, not a security concern — it does not affect data confidentiality or integrity.",
  "arbiters": ["security-arbiter", "trace-injection-arbitration-arbiter"],
  "whyAccepted": "Trace context injection allows an attacker to control how their requests appear in internal monitoring, potentially grouping malicious requests with legitimate ones to obscure attack patterns. Generating new root spans at external boundaries is defense-in-depth for observability integrity.",
  "whyRejected": "Dismissing trace injection as a non-security concern ignores its use in reconnaissance (mapping internal trace topology) and obfuscation (hiding attack activities within legitimate trace trees). Observability integrity is part of the security posture.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/dotnet/core/diagnostics/distributed-tracing-instrumentation-walkthroughs"],
  "licenseConstraint": "MIT",
  "trainingStatus": "draft"
}
```

---

## EXT-628 | MICROSOFT-LEARN-ARCH | Family 7 | Domain C

**Unit title:** Azure Logic Apps vs Azure Functions for workflow orchestration — API translation layer selection
**Source URL:** https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-overview
**License:** CC-BY-4.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT628-F7-C",
  "sourceIds": ["EXT-628"],
  "sourceTrustTier": 1,
  "datasetFamily": 7,
  "domain": "C",
  "subdomain": "workflow-orchestration-translation",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "A business automation workflow: (1) receive order from partner via SFTP, (2) parse CSV, (3) call internal Order API, (4) send confirmation email, (5) write audit to SQL. The team evaluates Azure Logic Apps vs Azure Functions (Durable Functions).",
    "b": "Azure Logic Apps: low-code visual designer, 400+ built-in connectors (SFTP, email, SQL), no code deployment for connector interactions, managed state for long-running workflows. Azure Durable Functions: code-first (C#), full programming model, requires custom connector code for SFTP/email.",
    "c": "Logic Apps Standard tier supports code-behind for complex transformations. Durable Functions provide more control for CPU-intensive processing and complex branching logic. Logic Apps connectors handle authentication and retry for external services."
  },
  "function": "Translate workflow orchestration tool selection for integration scenario",
  "acceptedOutput": "Use Logic Apps Standard for this integration workflow. Built-in SFTP trigger, CSV parsing actions, HTTP connector for Order API calls, built-in email connector, and SQL connector cover all steps without custom code. Define custom inline code only for complex CSV transformation logic. Logic Apps manages state, retries, and monitoring.",
  "rejectedOutput": "Use Azure Durable Functions. Code-first gives full control over the workflow and does not depend on Logic Apps connector reliability.",
  "arbiters": ["api-translation-arbiter", "workflow-tool-selection-arbiter"],
  "whyAccepted": "Logic Apps built-in connectors are Microsoft-managed, SLA-backed, and handle authentication/retry for SFTP, email, and SQL without custom code. The integration scenario maps directly to Logic Apps capabilities. Development time is significantly shorter than implementing equivalent Durable Functions with custom connector code.",
  "whyRejected": "Durable Functions for a standard SFTP-to-SQL integration requires implementing SFTP client code, email client code, retry logic, and state management — all provided for free by Logic Apps connectors. 'Full control' is only valuable when the built-in connectors are insufficient.",
  "sourceAnchors": ["https://learn.microsoft.com/en-us/azure/logic-apps/logic-apps-overview"],
  "licenseConstraint": "CC-BY-4.0",
  "trainingStatus": "draft"
}
```

---

## EXT-629 | CWE-TOP25 | Family 5 | Domain E

**Unit title:** CWE-285 improper authorization — object-level authorization (IDOR) prevention in ASP.NET Core
**Source URL:** https://cwe.mitre.org/data/definitions/285.html
**License:** CWE-TLP | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT629-F5-E",
  "sourceIds": ["EXT-629"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "object-level-authorization-arbitration",
  "rowClass": "negative-example",
  "input": {
    "a": "An ASP.NET Core API endpoint GET /api/orders/{id} returns an order if the user is authenticated. User A (userId=1) can access GET /api/orders/456 which belongs to User B (userId=2). The [Authorize] attribute only checks authentication, not ownership.",
    "b": "IDOR (Insecure Direct Object Reference, a form of CWE-285) occurs when an authenticated user can access resources belonging to other users by manipulating resource IDs. Authentication ≠ authorization. Resource-level ownership check is required.",
    "c": "ASP.NET Core resource-based authorization: IAuthorizationService.AuthorizeAsync(user, resource, requirement) evaluates the resource against a policy that can check ownership. The policy handler receives the actual Order entity and the ClaimsPrincipal."
  },
  "function": "Arbitrate object-level authorization to prevent IDOR in ASP.NET Core API",
  "acceptedOutput": "Implement resource-based authorization: fetch the order, then authorize: var order = await _repo.GetOrder(id); var result = await _authService.AuthorizeAsync(User, order, OrderOperations.Read); if (!result.Succeeded) return Forbid(); return Ok(order.ToDto()). Define an OrderAuthorizationHandler that checks order.UserId == user.GetUserId().",
  "rejectedOutput": "Add WHERE UserId = @currentUserId to the database query: var order = await _repo.GetOrderForUser(id, currentUserId). Return 404 if not found (hides the existence of other users' orders).",
  "arbiters": ["security-arbiter", "object-authorization-arbiter"],
  "whyAccepted": "Resource-based authorization via IAuthorizationService is the ASP.NET Core idiomatic pattern. It separates authorization logic from data access, is testable, and supports complex ownership rules (shared orders, admin override) via policy handlers.",
  "whyRejected": "WHERE UserId filter is a valid defensive pattern for simple ownership scenarios and avoids exposing resource existence. Both approaches are acceptable; resource-based authorization is preferred when ownership rules are complex or when different operations have different authorization policies.",
  "sourceAnchors": ["https://cwe.mitre.org/data/definitions/285.html"],
  "licenseConstraint": "CWE-TLP",
  "trainingStatus": "draft"
}
```

---

## EXT-630 | OWASP-ASVS | Family 5 | Domain E

**Unit title:** OWASP ASVS V2.2 multi-factor authentication — TOTP vs SMS OTP security arbitration
**Source URL:** https://owasp.org/www-project-application-security-verification-standard/
**License:** CC-BY-SA-3.0 | **Trust tier:** 1

**Training row:**
```json
{
  "rowId": "ROW-EXT630-F5-E",
  "sourceIds": ["EXT-630"],
  "sourceTrustTier": 1,
  "datasetFamily": 5,
  "domain": "E",
  "subdomain": "mfa-mechanism-arbitration",
  "rowClass": "tradeoff-decision",
  "input": {
    "a": "An application implements MFA (Multi-Factor Authentication) using SMS OTP (One-Time Password). A security review references OWASP ASVS V2.2.1 and NIST 800-63B to flag SMS OTP as a restricted authenticator.",
    "b": "NIST 800-63B restricts SMS OTP due to SIM swapping attacks, SS7 (Signaling System 7) interception, and phone number portability attacks. TOTP (Time-based One-Time Password, RFC-6238) via authenticator apps is not susceptible to these telecom-layer attacks. FIDO2/WebAuthn is the highest assurance level.",
    "c": "OWASP ASVS V2.2.1 requires that anti-replay controls are used for all authentication factors. TOTP has a 30-second window but uses HMAC-based computation — replay within the window requires intercepting the OTP before first use. SMS OTP has no replay protection beyond its validity window."
  },
  "function": "Arbitrate MFA mechanism security for ASVS V2.2 compliance",
  "acceptedOutput": "Replace SMS OTP with TOTP via authenticator app (Google Authenticator, Microsoft Authenticator): generate a base32 secret per user, display as QR code for TOTP setup. Validate using RFC-6238 TOTP with ±1 window tolerance and server-side used-OTP tracking to prevent replay within the window. Offer FIDO2/WebAuthn as the higher-assurance alternative.",
  "rejectedOutput": "Keep SMS OTP. NIST restricted status does not mean prohibited — it means acceptable only in specific scenarios. SMS OTP provides sufficient security for most consumer applications.",
  "arbiters": ["security-arbiter", "mfa-mechanism-arbitration-arbiter"],
  "whyAccepted": "TOTP eliminates the telecom attack surface entirely — the secret is device-local, not transmitted via SS7. Implementation complexity is comparable (QR code enrollment vs SMS integration). TOTP is the minimum ASVS V2.2 compliant step-up from SMS OTP.",
  "whyRejected": "NIST 'restricted' classification means SMS OTP requires additional controls (risk assessments, alternative factor availability). For new implementations, choosing TOTP is straightforwardly better — no additional controls needed, no telecom dependency, lower implementation cost (no SMS API fees).",
  "sourceAnchors": ["https://owasp.org/www-project-application-security-verification-standard/"],
  "licenseConstraint": "CC-BY-SA-3.0",
  "trainingStatus": "draft"
}
```
