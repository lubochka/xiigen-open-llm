# Layer 0-A — Compiler Pairs (016)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-016-001 — ARCHITECTURE: CDN topology

A: "Design the CDN (Content Delivery Network) edge topology — origin shield, PoP (Point of Presence) placement, and cache hierarchy."
  "design" → ARCHITECTURE | "edge topology" → ARCHITECTURE ("topology") | "cache hierarchy" → ARCHITECTURE ("layer") | "PoP placement" → ARCHITECTURE ("deployment")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "CDN", "origin shield", "PoP" → platform/network vocabulary, gap

---

## B14-compiler-016-002 — ARCHITECTURE+DOCUMENT: cache invalidation strategy

A: "Architect the cache invalidation strategy for the edge layer and document the TTL (Time to Live) policy decisions."
  "architect" → ARCHITECTURE | "cache invalidation strategy" → ARCHITECTURE ("strategy", "layer") | "document" → DOCUMENT | "TTL policy decisions" → ARCHITECTURE ("pattern")
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "TTL", "cache invalidation" → operational vocabulary, gap

---

## B14-compiler-016-003 — ARCHITECTURE: edge security boundary

A: "Structure the edge security boundary — WAF (Web Application Firewall) integration, DDoS (Distributed Denial of Service) mitigation layer, and TLS termination at edge."
  "structure" → ARCHITECTURE | "security boundary" → ARCHITECTURE ("boundary") + SECURITY | "WAF integration" → SECURITY | "DDoS mitigation layer" → ARCHITECTURE ("layer") | "TLS termination" → SECURITY ("tls")
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY"]
  Gap: "WAF", "DDoS", "Cloudflare" → platform/operational vocabulary, gap

---

## B14-compiler-016-004 — ARCHITECTURE+IMPLEMENT: CloudFront distribution config

A: "Design the CloudFront distribution architecture and implement the origin failover configuration."
  "design" → ARCHITECTURE | "distribution architecture" → ARCHITECTURE | "implement" → IMPLEMENT | "origin failover configuration" → IMPLEMENT (concrete config)
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "CloudFront" → platform name, gap

---

## B14-compiler-016-005 — gap: "purge the CDN cache"

A: "Purge the CDN cache for the product image assets."
  "purge" → gap (operational action) | "CDN cache" → gap | "product image assets" → gap
  CompiledPredicateIds: []
  Teaching: Cache purge is operational gap. To fire ARCHITECTURE: "design the cache invalidation topology for the CDN edge layer."
