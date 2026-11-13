# Held-Out and Corruption Pairs (017)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H017-001 — Held-out: "auto-scaling architecture" fires ARCHITECTURE

A: "Design the auto-scaling architecture for the API gateway tier."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H017-002 — Held-out: "scale the pods" is gap

A: "Scale the payment-service to 8 replicas."
  Expected: []
  Held-out: "scale" operational action is gap.

---

## H017-003 — Held-out: "capacity planning model" fires ARCHITECTURE

A: "Define the capacity planning model for the production cluster."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## C017-001 — Corruption v6-01: SCALING invented as predicate

A: "Design the auto-scaling architecture."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "SCALING"] ← v6-01 (SCALING is not a valid predicate)
  Teaching: "auto-scaling" vocabulary in context of architecture design fires ARCHITECTURE via "architecture". SCALING is not a valid predicate.

---

## C017-002 — Corruption v6-03: "scale the pods" fires ARCHITECTURE

A: "Scale the pods to 20 replicas."
  Correct: []
  Corrupt: ["ARCHITECTURE"] ← v6-03 (incorrectly infers scaling involves architectural decisions)
  Teaching: "scale" as operational action is gap regardless of the domain. ARCHITECTURE requires explicit design vocabulary (design, architect, structure). Operational actions are gap.
