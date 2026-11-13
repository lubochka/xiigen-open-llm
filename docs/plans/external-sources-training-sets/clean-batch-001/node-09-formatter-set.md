# Node 09 — Formatter — Clean Batch 001

Node 9 receives all approved translated results plus the target audience type.
It produces one combined user-readable output shaped for that audience.
The same approved results produce different output for different audiences — the facts do not change, the shape does.
The formatter never receives raw results, unchecked results, or results that failed arbitration.

A is the exact input: the approved translated results plus the audience specification.
B is the exact combined output shaped for that audience.
A B that says "a clear explanation suited to the audience" is wrong — the explanation itself must appear as B.

At least two audience types must be covered in any node-09 file.

---

Pair CB1-09-001 — HTTP cache versus Redis: explaining to a junior developer
Source: https://www.rfc-editor.org/rfc/rfc7234 — HTTP Caching; Redis documentation (redis.io)
License: IETF TLP (RFC text) — NOTE: IETF TLP requires further license review before promotion. Using only the concepts, not the text. Concept explanation is unencumbered.
A: Approved results:
[R1] HTTP Cache-Control headers instruct CDN edge nodes and browsers to serve cached responses without contacting the origin server, for the duration specified by max-age.
[R2] Redis is an in-process application cache that stores computed results or session data server-side, reducing repeated computation or database queries.
[R3] Both caches are active in the system simultaneously. Cache-Control handles traffic before it reaches the backend. Redis handles computation cost after the request reaches the backend.
[R4] Removing Cache-Control causes every request to hit the backend origin server. Removing Redis causes the backend to recompute or re-query on every cache miss.

Audience: Junior developer who asked "if we have Redis, why do we need Cache-Control headers? Isn't that double caching?"
B: They solve different problems at different layers, so they are not redundant — they are complementary.

Cache-Control tells the CDN and the browser: "This response is valid for the next N seconds. Do not ask me again until then." When a user in Frankfurt requests your product page, the CDN edge node in Frankfurt serves the cached copy without the request ever reaching your server in Virginia. Cache-Control stops traffic before it arrives.

Redis caches the result of work your backend already agreed to do. When the CDN misses — because the cache expired, or because the request bypassed the CDN — your NestJS server receives the request. Redis lets it answer "I already computed this report two minutes ago, here it is" instead of running the query again.

Remove Cache-Control and every user request hits your origin server, including all the Frankfurt users. Your server handles ten times the traffic.
Remove Redis and your server re-runs the expensive computation or database query on every CDN miss. Your database handles ten times the queries.

You need both. Cache-Control keeps traffic off the server. Redis keeps expensive work off the database.
Reason: The audience asked specifically about the relationship between the two caches. The explanation must answer that question directly, using concrete cause-and-effect examples for each cache being removed. Technical terms (CDN, cache miss, origin server) are appropriate for a junior developer context — these are standard terms they are learning.
Source: Concept explanation — RFC 7234 scope (CDN/browser caching) and Redis documentation (application caching)
License: Concepts explained, not text copied. No license constraint on the explanation itself.

Pair CB1-09-001 — Phrasing B
Source: Concept explanation — RFC 7234 and Redis documentation
License: No constraint on concept explanation
A: Approved results: [same four as above]
Audience: Junior developer who cannot understand why both HTTP caching and Redis caching are needed at the same time.
B: Think of it as two different tollbooths on a highway.

The first tollbooth is Cache-Control at the CDN edge. It sits between the user and your server. When a user requests the product page, the CDN checks: "Did I already serve this response recently?" If yes, it hands the response back without the request ever reaching your server. Most traffic never reaches you.

The second tollbooth is Redis inside your server. It sits between your application code and your database. When a request does reach your server — because the CDN cache expired, or the user bypassed the CDN — Redis checks: "Did I already compute this result recently?" If yes, your code returns it without touching the database.

They live at different points. Cache-Control cuts server load. Redis cuts database load. You can have both turned on at full power and they never interfere with each other.

---

Pair CB1-09-002 — WebSocket versus HTTP polling: explaining to a product manager
Source: https://react.dev/learn — React real-time patterns; WebSocket concept
License: CC-BY 4.0
A: Approved results:
[R1] The current implementation polls the server for order status updates every 5 seconds. At 50,000 active users, this generates 600,000 server requests per minute. The majority of these requests receive an empty response because the order status has not changed.
[R2] WebSocket (persistent bidirectional connection) allows the server to push updates the instant they occur. The client does not ask; the server notifies.
[R3] Switching to WebSocket eliminates the 5-second update delay. Users see order status changes immediately.
[R4] Implementation requires: server-side WebSocket handler, client-side connection management, reconnect logic on disconnect. The existing polling timer, retry logic, and deduplication code become unnecessary.

Audience: Product manager who asked "We already have HTTP polling working. Why add WebSockets? It sounds more complex."
B: Right now the app sends the server a question every 5 seconds: "Has anything changed?" At 50,000 users, that's 600,000 questions per minute — and most of them get the answer "No, nothing changed." We are generating massive traffic just to keep asking an empty question.

WebSockets flip the model. Instead of the app asking the server, the server tells the app the moment something changes. The user sees their order status update the instant it changes, not up to 5 seconds later.

You're right that it sounds more complex, but here is what actually gets removed once we switch: the 5-second polling timer goes away, the retry logic we have for failed polls goes away, and the deduplication code we use to prevent showing the same notification twice goes away. The complexity that's added (connection setup) is one-time. The complexity that's removed (polling management) was ongoing.

Net result: fewer moving parts in the client, zero empty server requests, and users who see real-time updates.
Reason: The product manager's concern is about added complexity. The explanation must acknowledge the concern, quantify the current problem (600,000 empty requests/minute), and show what gets removed — not just what gets added. Product managers respond to net-change framing, not technical architecture framing.
Source: https://react.dev/learn — Real-time patterns; WebSocket concepts
License: CC-BY 4.0

Pair CB1-09-002 — Phrasing B
Source: https://react.dev/learn — Real-time patterns; WebSocket concepts
License: CC-BY 4.0
A: Approved results: [same four as above]
Audience: Product manager concerned about engineering complexity of adding WebSockets when polling already works.
B: Polling works the same way as repeatedly refreshing your inbox every 5 seconds to see if a new email arrived. It works — but it means 50,000 users are collectively sending 600,000 refresh requests per minute to a server that usually replies "no new email."

WebSockets work like a phone line that stays open. When a new email arrives, the server tells you immediately. No refreshing needed.

The perceived complexity is the connection itself — opening and maintaining the line. But switching to WebSockets removes more than it adds: the 5-second timer disappears, the retry logic disappears, and the code that prevents the same status update from showing twice disappears. The end result is a simpler client, no wasted traffic, and instant updates for users. The one-time cost of building the WebSocket connection buys a permanent reduction in ongoing moving parts.

---

Pair CB1-09-003 — ETag conditional requests: explaining to a frontend developer
Source: https://react.dev/learn — API integration patterns; ETags concept from HTTP
License: CC-BY 4.0
A: Approved results:
[R1] An ETag is a version fingerprint for an HTTP response. The server generates it from the response content. If the content has not changed, the ETag is the same.
[R2] The client stores the ETag from the first response. On subsequent requests, the client sends If-None-Match: <stored-ETag> in the request header.
[R3] If the content has not changed, the server replies with HTTP 304 Not Modified and an empty body. The client uses its cached version. If the content has changed, the server replies with HTTP 200 and the new content plus a new ETag.
[R4] For a 50KB product catalog polled every 5 minutes, with a 95% cache hit rate, the bandwidth reduction is approximately 95% (empty 304 responses instead of 50KB responses).

Audience: Frontend developer who received a response with an ETag header and asked "What do I do with this?"
B: An ETag is the server's way of giving your response a version label. Think of it as a checksum or fingerprint of the data you just received.

Here is what to do with it in code:

1. When you first fetch the catalog, save the ETag from the response header:
   const etag = response.headers.get('ETag')

2. On the next poll (5 minutes later), send that ETag back as the If-None-Match header:
   fetch('/api/catalog', { headers: { 'If-None-Match': etag } })

3. If nothing has changed, the server replies with 304 Not Modified and an empty body — you keep using the data you already have.

4. If the catalog has changed, the server replies with 200 and the new data, plus a new ETag — update your stored ETag and your cached data.

The result: your app sends a tiny request every 5 minutes instead of downloading the full 50KB catalog every time. On the 95% of polls where nothing changed, you are transferring a few bytes instead of 50KB.
Reason: The developer's question is operational — what do I do with this header? The explanation must answer with code, not with architecture. The working code example is the core of a correct B here.
Source: https://react.dev/learn — API integration patterns
License: CC-BY 4.0

Pair CB1-09-003 — Phrasing B
Source: https://react.dev/learn — API integration patterns
License: CC-BY 4.0
A: Approved results: [same four as above]
Audience: Frontend developer who needs to implement conditional GET requests using the ETag returned by the API.
B: The ETag is a fingerprint for the response. Every time the catalog data changes, the fingerprint changes. Every time the data stays the same, the fingerprint stays the same.

Your job on the client side:

Save the fingerprint when you first fetch: const etag = response.headers.get('ETag')

On the next fetch, send it back: the server checks whether its current fingerprint matches yours. If yes: 304 Not Modified, empty body, use what you have. If no: 200, new data, new fingerprint.

```javascript
const response = await fetch('/api/catalog', {
  headers: etag ? { 'If-None-Match': etag } : {}
});
if (response.status === 304) return cachedCatalog;
const data = await response.json();
setEtag(response.headers.get('ETag'));
return data;
```

This cuts bandwidth by 95% on polls where nothing changed.

---

Pair CB1-09-004 — BFF architecture decision: explaining to a devops engineer
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT
A: Approved results:
[R1] The platform serves three frontend clients: web (8 data fields per request), mobile (4 fields, bandwidth-constrained), IoT dashboard (2 fields, 10-second polling). A single API gateway would apply the same response structure to all three clients.
[R2] The selected architecture is Backend For Frontend (BFF): three separate thin ASP.NET Core APIs, one per client type. Each BFF aggregates only the fields its client requires and can be deployed and scaled independently.
[R3] The mobile BFF implements aggressive field projection, reducing mobile response payload by approximately 50%. The IoT BFF implements a 10-second server-side cache, reducing IoT polling load on the downstream microservices by 99%.
[R4] Each BFF is a thin orchestration layer with no business logic. Shared service client code is in a common NuGet package. Three new deployable services are added to the platform.

Audience: DevOps engineer who must maintain and deploy the resulting system.
B: Three new services are being added: BFF-Web, BFF-Mobile, and BFF-IoT. Here is what you need to know for operations.

Deployment: Each BFF deploys independently. A mobile app update can require a BFF-Mobile deployment without touching BFF-Web or BFF-IoT. There is no shared release cycle between them.

Scaling: BFF-Web handles the highest request volume (web users). BFF-IoT handles the highest request frequency (10-second polls from IoT devices) but serves tiny payloads. Scale them independently based on their load profiles. The IoT BFF has a 10-second server-side cache — its downstream microservice calls are minimal even under high device count.

Shared infrastructure: All three BFFs use the same internal NuGet package for microservice client code. When that package is updated, all three BFFs need a rebuild and redeploy. Watch for this dependency.

Health checks: Each BFF needs its own liveness and readiness probes. A BFF-Mobile outage does not affect BFF-Web. Do not combine their health checks.

New failure mode: If BFF-Mobile is down, mobile users cannot reach the backend — but web and IoT clients are unaffected. This is the intended isolation. Monitor each BFF independently.
Reason: A devops engineer needs deployment, scaling, dependency, health check, and failure mode information — not the API design rationale. The explanation organizes by operational concern, not by architectural concern.
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT

Pair CB1-09-004 — Phrasing B
Source: https://github.com/dotnet-architecture/eShopOnContainers — architecture guide
License: MIT
A: Approved results: [same four as above]
Audience: DevOps engineer receiving three new services (BFF-Web, BFF-Mobile, BFF-IoT) to operate.
B: You are getting three new services. Key operational notes:

Independent deploys: BFF-Web, BFF-Mobile, BFF-IoT deploy separately. No need to coordinate releases between them.

Scale separately: BFF-Web gets the most users. BFF-IoT gets the most frequent small requests (10-second poll intervals) but the IoT BFF has a built-in 10-second cache so downstream load is low. Profile and scale each independently.

Shared NuGet package: All three BFFs depend on a shared internal package for microservice clients. Package update = rebuild and deploy all three. Track this as a dependency.

No cross-BFF health dependency: An outage in BFF-Mobile does not affect web or IoT users. Run separate health checks and separate alerts per BFF. Do not combine them into a single "API gateway is up" check.

Failure isolation: This is the intended benefit of BFF. If mobile traffic spikes and overwhelms BFF-Mobile, web and IoT are protected. Treat each BFF as an independent service for all alerting and on-call purposes.
