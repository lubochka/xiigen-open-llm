# Layer 0-A — Compiler Pairs (045)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-045-001 — CacheCluster: ARCHITECTURE fires on cache design vocabulary

"architect the distributed cache cluster topology" → ARCHITECTURE ("architect", "topology")
"design the cache layer for the API gateway" → ARCHITECTURE ("design", "layer" in architecture context)
"structure cache partitioning and eviction policy" → ARCHITECTURE ("structure")
"flush the cache" → gap ("flush" is operational gap)

---

## B14-compiler-045-002 — CacheCluster: IMPLEMENT fires on cache IaC artifacts

"implement the Redis cluster Terraform module" → IMPLEMENT ("implement" + "module")
"write the cache cluster Helm chart" → IMPLEMENT ("write" + "chart" = Helm chart is IaC artifact)
"scaffold the Memcached StatefulSet configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-045-003 — CacheCluster: SECURITY fires on cache encryption vocabulary

"enforce TLS for Redis cluster connections" → SECURITY ("TLS")
"encrypt cache data at rest with AES-256" → SECURITY ("encrypt", "at rest")
"rotate Redis authentication tokens on 30-day schedule" → SECURITY ("rotate" + "token" = key rotation equivalent)

---

## B14-compiler-045-004 — CacheCluster: AUTHORIZATION fires on cache access control

"restrict Redis cluster access to application service accounts via RBAC" → AUTHORIZATION ("RBAC", "service accounts", "restrict" in access context)
"grant cache-reader role to read-through service" → AUTHORIZATION ("grant", "role")

---

## B14-compiler-045-005 — CacheCluster: gap vocabulary — cache runtime operations

Confirmed gap verbs for cache runtime operations (fire zero predicates):
  flush, invalidate, evict, expire, warm, prime, hit, miss, set (cache key), get (cache key), delete (cache key), clear
  "flush the Redis cache" → gap | "warm up the cache" → gap | "invalidate cache entries" → gap
