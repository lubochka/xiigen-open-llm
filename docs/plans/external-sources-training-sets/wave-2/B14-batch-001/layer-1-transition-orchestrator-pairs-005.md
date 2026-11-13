# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: B14-batch-001 | Scenario: B14 — Cloud infrastructure, ARCHITECTURE, sub-phase 3.5
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B14-transition-005-001 — TransitionType: feature_addition triggers

A: IMPLEMENT verb present
B: TransitionType: feature_addition
  Triggers: "implement", "build", "create", "write", "generate", "develop", "extend", "add" (in code context)
  Teaching: Any IMPLEMENT predicate trigger → feature_addition TransitionType.

---

## B14-transition-005-002 — TransitionType: refactoring triggers

A: refactor verb present, no IMPLEMENT predicate context
B: TransitionType: refactoring
  Triggers: "refactor", "extract", "restructure", "reorganize" (in architecture context)
  Teaching: "refactor" drives refactoring TransitionType for ARCHITECTURE flows.

---

## B14-transition-005-003 — TransitionType: documentation triggers

A: design/diagram/document verbs only, no IMPLEMENT, no refactor
B: TransitionType: documentation
  Triggers: "design", "architect", "blueprint", "diagram", "document", "define" (in structural context)
  Teaching: Pure architecture design without implementation → documentation.

---

## B14-transition-005-004 — Five-predicate: feature_addition wins

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT", "QA"]
B: TransitionType: feature_addition (IMPLEMENT present, overrides all other TransitionType considerations)
  Teaching: IMPLEMENT always drives feature_addition regardless of how many other predicates coexist.

---

## B14-transition-005-005 — ARCHITECTURE+DOCUMENT+QA: documentation (no IMPLEMENT)

A: ["ARCHITECTURE", "DOCUMENT", "QA"]
B: TransitionType: documentation
  Sequential: ARCHITECTURE design → DOCUMENT recording → QA sign-off
  Teaching: QA does not drive TransitionType. Only IMPLEMENT does (feature_addition) or refactor verbs (refactoring).

