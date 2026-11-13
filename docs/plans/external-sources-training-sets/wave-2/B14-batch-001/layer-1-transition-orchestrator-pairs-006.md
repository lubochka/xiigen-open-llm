# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-006-001 — ARCHITECTURE+IMPLEMENT: parallel flow execution

A: ["ARCHITECTURE", "IMPLEMENT"]
B: InfrastructureArchitectureFlow35 ∥ ImplementationFlow41
  Parallel execution. ARCHITECTURE design and IMPLEMENT coding are independent flows.
  Teaching: Multi-predicate flows execute in parallel unless explicit sequential dependency is stated.

---

## B14-transition-006-002 — ARCHITECTURE+IMPLEMENT+TEST: sequential preferred

A: ["ARCHITECTURE", "IMPLEMENT", "TEST"] | sequential wording: "architect first, then implement, then validate"
B: ARCHITECTURE → IMPLEMENT → TEST (sequential)
  Teaching: Sequential wording ("first", "then", "after") drives sequential ordering. Otherwise parallel.

---

## B14-transition-006-003 — ARCHITECTURE+SECURITY: audit before design is valid

A: ["ARCHITECTURE", "SECURITY"] | "audit existing boundaries, then redesign the architecture"
B: SECURITY (audit) → ARCHITECTURE (redesign)
  Teaching: SECURITY audit can precede ARCHITECTURE redesign. Ordering follows request wording.

---

## B14-transition-006-004 — ARCHITECTURE+QA: QA always last

A: ["ARCHITECTURE", "QA"] | design review workflow
B: ARCHITECTURE → QA gate
  Teaching: QA gating always comes last in the sequential flow. It validates the output of all preceding flows.

---

## B14-transition-006-005 — ARCHITECTURE+IMPLEMENT+TEST+DOCUMENT+QA: canonical sequence

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
B: ARCHITECTURE → IMPLEMENT → TEST → DOCUMENT → QA gate
  TransitionType: feature_addition
  Teaching: Canonical full delivery sequence for ARCHITECTURE-led features. QA gate always last.

