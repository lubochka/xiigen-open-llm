# Held-Out and Corruption Pairs (019)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H019-001 — Held-out: "multi-region deployment topology" fires ARCHITECTURE

A: "Architect the multi-region active-active deployment topology."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## H019-002 — Held-out: "deploy to region" is gap

A: "Deploy the order-service to ap-southeast-1."
  Expected: []
  Held-out: "deploy" standalone is gap; "ap-southeast-1" is region vocabulary, gap.

---

## H019-003 — Held-out: "data residency boundary" fires ARCHITECTURE

A: "Define the data residency boundary for the European market."
  Expected: ["ARCHITECTURE"]
  Held-out for evaluation.

---

## C019-001 — Corruption v6-01: MULTI_REGION invented as predicate

A: "Design the multi-region deployment topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "MULTI_REGION"] ← v6-01 (MULTI_REGION is not a valid predicate)
  Teaching: "multi-region" is a deployment context vocabulary word. It does not create a predicate. Only ARCHITECTURE fires via "design" + "topology".

---

## C019-002 — Corruption v6-03: "deploy to region" fires IMPLEMENT

A: "Deploy the application to the eu-west-1 region."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-03 (treats operational deployment as implementation)
  Teaching: "deploy" as a standalone operational command is gap. IMPLEMENT fires when in IaC/scripting context: "implement the deployment pipeline." Bare "deploy" is operational, not coding.
