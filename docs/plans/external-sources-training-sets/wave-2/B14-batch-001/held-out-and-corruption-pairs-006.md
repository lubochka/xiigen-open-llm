# Held-Out and Corruption Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H006-001 — Held-out: ARCHITECTURE — SAGA pattern

A: "Design the SAGA (Saga Orchestration Pattern) for the OrderService distributed transaction."
  Expected: ["ARCHITECTURE"]
  Held-out: SAGA fires ARCHITECTURE as a distributed system pattern.

---

## H006-002 — Held-out: ARCHITECTURE+IMPLEMENT — sidecar implementation

A: "Architect the sidecar proxy pattern and implement the Envoy (proxy) filter configuration."
  Expected: ["ARCHITECTURE", "IMPLEMENT"]
  Held-out: "architect" fires ARCHITECTURE; "implement" fires IMPLEMENT.

---

## H006-003 — Held-out: gap — style name only

A: "We want to adopt event-driven architecture."
  Expected: []
  Held-out: "adopt" is gap; naming an architectural style without a design verb → gap.

---

## C006-001 — Corruption v6-03: ARCHITECTURE+IMPLEMENT collapsed to ARCHITECTURE only

A: ["ARCHITECTURE", "IMPLEMENT"]
  Correct: [InfrastructureArchitectureFlow35, ImplementationFlow41]
  Corrupt: [InfrastructureArchitectureFlow35] only ← v6-03 (IMPLEMENT suppressed by dominant ARCHITECTURE)
  Teaching: v6-03 variant: ARCHITECTURE suppresses IMPLEMENT. Incorrect. Both flows must be created.

---

## C006-002 — Corruption v6-02: "architect" triggers IMPLEMENT (wrong predicate)

A: "Architect the service mesh topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["IMPLEMENT"] ← v6-02 (model maps "architect" to IMPLEMENT instead of ARCHITECTURE)
  Teaching: "architect" (verb) → ARCHITECTURE, not IMPLEMENT. "architect" ≠ "implement".

