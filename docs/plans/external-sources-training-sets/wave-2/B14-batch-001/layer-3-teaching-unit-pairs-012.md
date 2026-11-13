# Layer 3 — Teaching Unit Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T012-001 — "data mesh" fires ARCHITECTURE

Rule: "data mesh topology/architecture" → ARCHITECTURE
  "architect the data mesh topology" → ARCHITECTURE
  Teaching: "data mesh" fires ARCHITECTURE as a data architecture paradigm with topology and boundary concepts.

---

## T012-002 — "egress/ingress" fire ARCHITECTURE in network design

Rule: "egress control layer" / "ingress layer" + design verb → ARCHITECTURE
  "design the ingress and egress control layers" → ARCHITECTURE
  Teaching: "egress" and "ingress" in network design context fire ARCHITECTURE as boundary control concepts.

---

## T012-003 — "anti-affinity" fires ARCHITECTURE

Rule: "anti-affinity rules" + design verb → ARCHITECTURE
  "define the pod anti-affinity rules" → ARCHITECTURE
  Teaching: "anti-affinity" fires ARCHITECTURE as a scheduling architecture constraint.

---

## T012-004 — "event sourcing" fires ARCHITECTURE

Rule: "event sourcing topology/pattern" + design verb → ARCHITECTURE
  "architect the event sourcing topology" → ARCHITECTURE
  Teaching: "event sourcing" fires ARCHITECTURE as a recognized data consistency architectural pattern.

---

## T012-005 — ARCHITECTURE predicate firing summary

ARCHITECTURE fires on:
  Verbs: design, architect, structure, blueprint, diagram, define (structural context), finalize (design context), refactor (architecture context)
  Nouns: architecture, topology, layer, tier, boundary, component, pattern, blueprint, strategy (design), infrastructure, mesh, ADR, C4
  Patterns: CQRS, SAGA, CDC, EDA, IaC, pub/sub, circuit breaker, bulkhead, sidecar, strangler fig, canary, blue/green, outbox, sharding, event sourcing, data mesh, serverless
  Gap verbs: plan, configure, set up, deploy, install, restart, rollback, migrate (operational), scale (operational), add (without code context)

