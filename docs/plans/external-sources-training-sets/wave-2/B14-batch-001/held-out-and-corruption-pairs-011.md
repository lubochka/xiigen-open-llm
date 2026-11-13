# Held-Out and Corruption Pairs (011)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H011-001 — Held-out: ARCHITECTURE — backpressure design

A: "Design the backpressure propagation mechanism for the OrderService event pipeline."
  Expected: ["ARCHITECTURE"]
  Held-out: "backpressure propagation mechanism" fires ARCHITECTURE.

---

## H011-002 — Held-out: ARCHITECTURE+SECURITY — network segmentation + zero-trust enforcement

A: "Architect the network segmentation model and enforce zero-trust boundary policies."
  Expected: ["ARCHITECTURE", "SECURITY"]
  Held-out: ARCHITECTURE fires on "architect the network segmentation model"; SECURITY fires on "enforce zero-trust".

---

## H011-003 — Held-out: gap — cloud configuration

A: "Add the AWS S3 bucket lifecycle policy."
  Expected: []
  Held-out: "add" (cloud configuration action) + "S3 bucket lifecycle policy" (operational detail) → gap.

---

## C011-001 — Corruption v6-01: PERFORMANCE invented

A: "Design the auto-scaling architecture for peak load scenarios."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "PERFORMANCE"] ← v6-01 (PERFORMANCE is not a valid predicate)
  Teaching: Performance-related architecture design → ARCHITECTURE only. PERFORMANCE is not a predicate.

---

## C011-002 — Corruption v6-02: gap → IMPLEMENT (wrong predicate for config verb)

A: "Deploy the Kubernetes manifests to the staging cluster."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-02 (model maps deployment commands to IMPLEMENT)
  Teaching: "deploy" is operational gap. IMPLEMENT fires on code writing verbs (implement, build, create, write). "deploy" ≠ implement.

