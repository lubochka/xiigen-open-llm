# Layer 3 — Teaching Unit Pairs (004)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## T004-001 — "infrastructure" fires ARCHITECTURE with design verb

Rule: "infrastructure" + design verb → ARCHITECTURE
  "design the infrastructure" → ARCHITECTURE
  "check the infrastructure" → gap ("check" is not an architecture verb)
  Teaching: "infrastructure" is a design-verb-dependent trigger. It requires design/architect/structure.

---

## T004-002 — TransitionType: IMPLEMENT drives feature_addition

Rule: IMPLEMENT verb present → TransitionType: feature_addition
  "design + implement" → feature_addition
  "architect + implement" → feature_addition
  Teaching: Any IMPLEMENT trigger (implement, build, create, write, generate, develop, extend, add in code context) → feature_addition.

---

## T004-003 — TransitionType: "refactor" → refactoring for ARCHITECTURE

Rule: "refactor" verb in architecture context → TransitionType: refactoring
  "refactor the service mesh architecture" → refactoring
  Teaching: "refactor" overrides documentation TransitionType for ARCHITECTURE flows.

---

## T004-004 — TransitionType: pure design → documentation

Rule: design/architect/blueprint/diagram verbs only (no IMPLEMENT, no refactor) → TransitionType: documentation
  "design the topology" → documentation
  "architect the DR topology" → documentation
  Teaching: Architecture design artifacts (ADRs, topology diagrams) are documentation outputs.

---

## T004-005 — ARCHITECTURE + SECURITY: independent predicates

Rule: ARCHITECTURE and SECURITY are always independent predicates
  "design zero-trust network + audit existing boundaries" → ARCHITECTURE + SECURITY (both fire)
  ARCHITECTURE is never suppressed by SECURITY and vice versa
  Teaching: No predicate suppresses another. Both route to their independent sub-phases.

