# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## E011-001 — "SAGA" pattern fires ARCHITECTURE

A: "Design the SAGA pattern for the OrderService distributed transaction — define the compensation steps and failure boundaries."
  "design" → ARCHITECTURE | "SAGA pattern" → ARCHITECTURE (distributed system architectural pattern) | "compensation steps" → ARCHITECTURE | "failure boundaries" → ARCHITECTURE ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "SAGA" fires ARCHITECTURE as a distributed transaction architectural pattern.

---

## E011-002 — "outbox pattern" fires ARCHITECTURE

A: "Architect the transactional outbox pattern for the PaymentService event publishing."
  "architect" → ARCHITECTURE | "transactional outbox pattern" → ARCHITECTURE ("pattern")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "outbox pattern" fires ARCHITECTURE. The transactional outbox is a data consistency architectural pattern.

---

## E011-003 — "bulkhead" fires ARCHITECTURE

A: "Define the bulkhead isolation boundaries for the PaymentService thread pools."
  "define" → ARCHITECTURE | "bulkhead isolation boundaries" → ARCHITECTURE ("bulkhead" = resilience pattern, "boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "bulkhead" fires ARCHITECTURE as a resilience architectural pattern (borrowed from ship compartment design).

---

## E011-004 — "strangler fig" fires ARCHITECTURE

A: "Design the strangler fig migration pattern for replacing the legacy monolith payment module."
  "design" → ARCHITECTURE | "strangler fig migration pattern" → ARCHITECTURE (migration architectural pattern)
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "strangler fig" fires ARCHITECTURE as a legacy system migration architectural pattern.

---

## E011-005 — "data mesh" fires ARCHITECTURE

A: "Architect the data mesh topology — define domain-oriented data ownership boundaries and federated governance model."
  "architect" → ARCHITECTURE | "data mesh topology" → ARCHITECTURE ("mesh", "topology") | "domain-oriented data ownership boundaries" → ARCHITECTURE ("boundaries")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "data mesh" fires ARCHITECTURE as a data architecture paradigm. "mesh" in architectural context fires ARCHITECTURE.

