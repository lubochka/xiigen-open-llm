# Held-Out and Corruption Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H013-001 — Held-out: "node group topology" fires ARCHITECTURE

A: "Architect the node group topology for the EKS production cluster."
  Expected: ["ARCHITECTURE"]
  Held-out from training set for evaluation.

---

## H013-002 — Held-out: gap — "restart the pods"

A: "Restart the order-service pods after the config change."
  Expected: []
  Held-out: "restart" is operational gap. "pods" → gap. "config change" → gap.

---

## H013-003 — Held-out: ARCHITECTURE+IMPLEMENT for cluster IaC

A: "Define the EKS cluster architecture and implement the Terraform node group configuration."
  Expected: ["ARCHITECTURE", "IMPLEMENT"]
  Held-out for evaluation.

---

## C013-001 — Corruption v6-01: KUBERNETES invented

A: "Design the EKS cluster topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "KUBERNETES"] ← v6-01 (KUBERNETES is not a valid predicate)
  Teaching: v6-01 invents platform-specific predicates. Only 9 valid predicates: SECURITY, AUTHORIZATION, API_CONTRACT, ARCHITECTURE, IMPLEMENT, TEST, QA, UI, DOCUMENT.

---

## C013-002 — Corruption v6-03: ARCHITECTURE suppressed by EKS gap

A: "Design the EKS cluster topology."
  Correct: ["ARCHITECTURE"]
  Corrupt: [] ← v6-03 (incorrectly treats "EKS" gap as invalidating all predicates)
  Teaching: v6-03 incorrectly allows gap vocabulary to suppress fired predicates. "EKS" is gap but "design" + "topology" fire ARCHITECTURE regardless.
