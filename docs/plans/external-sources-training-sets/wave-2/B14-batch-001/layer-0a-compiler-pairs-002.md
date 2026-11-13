# Layer 0-A — Compiler Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: compiler

---

## B14-compiler-002-001 — ARCHITECTURE: load balancer design

A: "Design the load balancer topology for the API gateway ingress layer."
  "design" → ARCHITECTURE | "load balancer topology" → ARCHITECTURE ("topology") | "ingress layer" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "load balancer", "API gateway", "ingress" → cloud vocabulary, gap

---

## B14-compiler-002-002 — ARCHITECTURE: database replication

A: "Architect the primary-replica database topology for the OrderService data layer."
  "architect" → ARCHITECTURE (verb form) | "primary-replica topology" → ARCHITECTURE | "data layer" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "database", "OrderService" → domain vocabulary, gap

---

## B14-compiler-002-003 — ARCHITECTURE+DOCUMENT: CDN layer

A: "Design the CDN (Content Delivery Network) edge caching layer and document the cache invalidation strategy."
  "design" → ARCHITECTURE | "CDN edge caching layer" → ARCHITECTURE ("layer") | "document" → DOCUMENT | "cache invalidation strategy" → ARCHITECTURE ("strategy" in design context)
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT"]
  Gap: "CDN", "cache invalidation" → platform/operational vocabulary, gap

---

## B14-compiler-002-004 — ARCHITECTURE+IMPLEMENT: message queue

A: "Architect the message broker topology and implement the dead-letter queue configuration for the EventBus."
  "architect" → ARCHITECTURE | "message broker topology" → ARCHITECTURE | "implement" → IMPLEMENT | "dead-letter queue configuration" → IMPLEMENT (concrete configuration)
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "EventBus", "message broker" → platform vocabulary, gap

---

## B14-compiler-002-005 — gap: "plan the infrastructure"

A: "Plan the infrastructure upgrade for next quarter."
  "plan" → gap (ARCHITECTURE requires design/architect/blueprint/diagram/structure verbs) | "infrastructure upgrade" → gap without architecture framing | "next quarter" → gap
  CompiledPredicateIds: [] → gap
  Teaching: "plan" without architecture vocabulary is gap. ARCHITECTURE requires design/architect/structure/blueprint.

