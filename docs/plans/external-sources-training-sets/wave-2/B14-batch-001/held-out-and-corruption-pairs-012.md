# Held-Out and Corruption Pairs (012)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: held_out + corruption

---

## H012-001 — Held-out: ARCHITECTURE — bulkhead isolation

A: "Define the bulkhead isolation boundaries for the PaymentService thread pools."
  Expected: ["ARCHITECTURE"]
  Held-out: "bulkhead isolation boundaries" fires ARCHITECTURE.

---

## H012-002 — Held-out: ARCHITECTURE+DOCUMENT+QA — architecture sign-off

A: "Finalize the platform architecture blueprint, document all ADRs, and get QA sign-off before production."
  Expected: ["ARCHITECTURE", "DOCUMENT", "QA"]
  Held-out: Three predicates, three flows, documentation TransitionType (no IMPLEMENT).

---

## H012-003 — Held-out: gap — vague improvement request

A: "Our infrastructure could be more scalable."
  Expected: []
  Held-out: Vague improvement statement without design imperative → gap.

---

## C012-001 — Corruption v6-03: five-predicate collapsed to two

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
  Correct: [InfrastructureArchitectureFlow35, ImplementationFlow41, UnitTestingFlow51, DocumentationFlow62, QAGatingFlow73]
  Corrupt: [InfrastructureArchitectureFlow35, ImplementationFlow41] only ← v6-03 (three flows suppressed)
  Teaching: No cap on flow count. All five flows must be instantiated. v6-03 illegally collapses.

---

## C012-002 — Corruption: "architect" → TransitionType: feature_addition (wrong)

A: ["ARCHITECTURE"] only | "architect the disaster recovery topology"
  Correct: TransitionType: documentation
  Corrupt: TransitionType: feature_addition ← wrong (no IMPLEMENT predicate)
  Teaching: "architect" (verb) without IMPLEMENT → documentation TransitionType. feature_addition requires IMPLEMENT.

