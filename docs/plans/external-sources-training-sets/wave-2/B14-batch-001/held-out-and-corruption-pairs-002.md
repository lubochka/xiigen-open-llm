# Held-Out and Corruption Pairs (002)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H002-001 — Held-out: "topology" fires ARCHITECTURE

A: "Define the message routing topology for the NotificationService."
  Expected: ["ARCHITECTURE"]
  Held-out from training set for evaluation.

---

## H002-002 — Held-out: gap — "configure Redis"

A: "Configure Redis for session caching."
  Expected: []
  Held-out: "configure" is gap. "Redis" is platform vocabulary gap.

---

## H002-003 — Held-out: ARCHITECTURE+IMPLEMENT — IaC

A: "Architect the networking layer and implement the VPC peering configuration using Terraform."
  Expected: ["ARCHITECTURE", "IMPLEMENT"]
  Held-out for evaluation.

---

## C002-001 — Corruption v6-01: DEVOPS invented

A: "Design the cloud infrastructure deployment pipeline."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "DEVOPS"] ← v6-01 (DEVOPS is not a valid predicate)
  Teaching: v6-01 invents predicates. Only 9 valid predicates exist: SECURITY, AUTHORIZATION, API_CONTRACT, ARCHITECTURE, IMPLEMENT, TEST, QA, UI, DOCUMENT.

---

## C002-002 — Corruption v6-03: single flow instead of two

A: ["ARCHITECTURE", "IMPLEMENT"]
  Correct: [InfrastructureArchitectureFlow35, ImplementationFlow41]
  Corrupt: [ImplementationFlow41] only ← v6-03 (ARCHITECTURE suppressed by IMPLEMENT)
  Teaching: v6-03 incorrectly collapses multi-predicate routing. Both flows must be created.

