# Held-Out and Corruption Pairs (009)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H009-001 — Held-out: ARCHITECTURE — VPC peering

A: "Design the VPC peering topology for cross-account communication."
  Expected: ["ARCHITECTURE"]
  Held-out: "VPC peering topology" fires ARCHITECTURE.

---

## H009-002 — Held-out: ARCHITECTURE+AUTHORIZATION — RBAC

A: "Design the RBAC architecture for the Kubernetes cluster."
  Expected: ["ARCHITECTURE", "AUTHORIZATION"]
  Held-out: ARCHITECTURE fires on "design the RBAC architecture"; AUTHORIZATION fires on "RBAC".

---

## H009-003 — Held-out: ARCHITECTURE alone (TransitionType: documentation)

A: "Architect the disaster recovery topology for the payments platform."
  Expected: ["ARCHITECTURE"] | TransitionType: documentation
  Held-out: "architect" verb → documentation TransitionType (no IMPLEMENT present).

---

## C009-001 — Corruption: "design" + UI vocabulary → ARCHITECTURE (wrong predicate)

A: "Design the user interface for the admin dashboard."
  Correct: ["UI"]
  Corrupt: ["ARCHITECTURE"] ← v6-01 (model maps all "design" verbs to ARCHITECTURE)
  Teaching: "design" fires ARCHITECTURE only with architecture vocabulary (infrastructure, topology, etc.). "design" + UI vocabulary → UI predicate.

---

## C009-002 — Corruption v6-03: ARCHITECTURE+QA collapsed to ARCHITECTURE

A: ["ARCHITECTURE", "QA"]
  Correct: [InfrastructureArchitectureFlow35, QAGatingFlow73]
  Corrupt: [InfrastructureArchitectureFlow35] only ← v6-03 (QA suppressed)
  Teaching: QA never suppressed by ARCHITECTURE. Both flows always created.

