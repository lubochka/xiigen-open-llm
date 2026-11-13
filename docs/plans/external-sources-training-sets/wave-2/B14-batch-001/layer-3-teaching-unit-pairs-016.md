# Layer 3 — Teaching Unit Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## T016-001 — "edge topology" fires ARCHITECTURE

A: "Design the edge topology for the global CDN." ARCHITECTURE fires?
B: Yes. "design" → ARCHITECTURE. "edge topology" → ARCHITECTURE via "topology". "CDN" → gap. Route to 3.5.

---

## T016-002 — "purge the CDN cache" is operational gap

A: "Purge the CDN cache." Any predicates?
B: Zero predicates. "purge" → operational gap. "CDN cache" → gap. Result: [] → knowledge_edge. To fire ARCHITECTURE: "design the cache invalidation topology for the CDN edge layer."

---

## T016-003 — "TLS termination at edge" fires SECURITY

A: "Configure TLS termination at the edge layer." Predicates?
B: ["IMPLEMENT", "SECURITY"]. "configure" → IMPLEMENT (in configuration context). "TLS termination" → SECURITY ("tls"). Both fire.

---

## T016-004 — "cache hierarchy" fires ARCHITECTURE

A: "Architect the cache hierarchy — L1 edge, L2 regional, L3 origin shield." Predicates?
B: ["ARCHITECTURE"]. "architect" → ARCHITECTURE. "cache hierarchy" → ARCHITECTURE via "layer" (implied layered structure). Route to 3.5.

---

## T016-005 — ARCHITECTURE+DOCUMENT for CDN strategy ADR

A: "Design the CDN caching strategy and document the TTL policy decisions as an ADR." Result?
B: ["ARCHITECTURE", "DOCUMENT"]. "design" + "caching strategy" → ARCHITECTURE. "document" + "ADR" → DOCUMENT. TransitionType: documentation (DOCUMENT present, no IMPLEMENT). Two flows: 3.5 + 6.2.
