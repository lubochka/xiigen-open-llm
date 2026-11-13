# Layer 0-C — Knowledge Edge Pairs (045)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-045-001 — CacheCluster: gap vocabulary — cache runtime operations

Confirmed gap vocabulary for cache runtime operations (zero predicates):
  Action verbs: flush, invalidate, evict, expire, warm, prime, hit, miss, set (key), get (key), delete (key), clear, refresh (cache), purge, populate
  Cache runtime nouns: cache key, TTL (Time To Live) value (runtime), hit ratio, miss ratio, eviction policy (runtime setting)

---

## B14-edge-045-002 — CacheCluster: SECURITY boundary — cache authentication compound phrases

SECURITY fires for:
  "TLS for Redis cluster connections", "rotate Redis authentication tokens", "encrypt cache data at rest", "Redis AUTH token rotation"
  "flush Redis cache" → gap | "warm up cache" → gap
  Note: "Redis AUTH" contains "authentication" but the operational act of setting it ("set Redis AUTH") is gap; only rotation/enforcement fires SECURITY.

---

## B14-edge-045-003 — CacheCluster: IMPLEMENT boundary — cache IaC artifacts

IMPLEMENT fires for:
  Redis cluster Helm chart, cache cluster Terraform module, Memcached StatefulSet configuration, cache cluster values file
  "write the Redis Helm chart" → IMPLEMENT | "scaffold Memcached StatefulSet" → IMPLEMENT
  "flush the cache" → gap | "set a cache key" → gap

---

## B14-edge-045-004 — CacheCluster: disambiguation — "eviction policy" design vs runtime

"design the cache eviction policy" → ARCHITECTURE ("design" + "policy" in design context)
"set the eviction policy to allkeys-lru" → gap ("set" is operational; specific policy config is operational)
"implement the cache eviction policy configuration" → IMPLEMENT ("implement" + "configuration")
Rule: design verb = ARCHITECTURE; set/configure operationally = gap; implement + artifact = IMPLEMENT.
