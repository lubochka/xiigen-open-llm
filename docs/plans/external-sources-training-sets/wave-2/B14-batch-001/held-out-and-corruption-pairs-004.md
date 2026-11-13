# Held-Out and Corruption Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H004-001 — Held-out: ARCHITECTURE+DOCUMENT — C4 diagram

A: "Produce a C4 (Context, Containers, Components, Code) model and document the platform design decisions."
  Expected: ["ARCHITECTURE", "DOCUMENT"]
  Held-out: C4 fires ARCHITECTURE; "document" fires DOCUMENT. Both present.

---

## H004-002 — Held-out: gap — "plan the migration"

A: "Plan the database migration."
  Expected: []
  Held-out: "plan" is gap. No architecture design verb.

---

## H004-003 — Held-out: ARCHITECTURE+IMPLEMENT+TEST — IaC

A: "Architect the infrastructure, implement the Terraform modules, and validate the provisioning."
  Expected: ["ARCHITECTURE", "IMPLEMENT", "TEST"]
  Held-out: Three predicates, three flows.

---

## C004-001 — Corruption v6-03: four predicates collapsed to two

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"]
  Correct: [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62]
  Corrupt: [InfrastructureArchitectureFlow35, ImplementationFlow41] only ← v6-03 (TEST and DOCUMENT suppressed)
  Teaching: v6-03 collapses multi-predicate routing. All four flows must be instantiated.

---

## C004-002 — Corruption v6-01: INFRASTRUCTURE invented

A: "Design the infrastructure topology for the microservices platform."
  Correct: ["ARCHITECTURE"]
  Corrupt: ["ARCHITECTURE", "INFRASTRUCTURE"] ← v6-01 (INFRASTRUCTURE is not a valid predicate)
  Teaching: INFRASTRUCTURE is not a predicate. Cloud/infra vocabulary fires ARCHITECTURE, not a separate INFRASTRUCTURE predicate.

