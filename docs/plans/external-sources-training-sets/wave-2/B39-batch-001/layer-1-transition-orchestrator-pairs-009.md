# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## TO-009-001 — Complete five-flow orchestration

A: ["IMPLEMENT", "TEST", "UI", "DOCUMENT", "QA"]
Orchestration:
  Phase 1: ImplementationFlow41 (4.1)
  Phase 2: E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5) [parallel]
  Phase 3: DocumentationFlow62 (documentation)
  Phase 4: QAGatingFlow73 (7.3) [always last]
TransitionType: feature_addition | Five flows, no collapsing.

---

## TO-009-002 — QA FAIL blocks: orchestration halts at gate

A: ["IMPLEMENT", "TEST", "UI", "QA"] — QA returns FAIL
Behavior: Flows 1-3 complete. QAGatingFlow73 returns FAIL. The complete transition is blocked.
Result: Feature is not merged/released. FAIL verdict triggers re-work cycle.
Rule: QA FAIL is a hard block. It does not roll back prior work; it prevents completion.

---

## TO-009-003 — IMPLEMENT+TEST: strict dependency

A: ["IMPLEMENT", "TEST"] | "Build the pagination component and write tests"
Strict ordering: ImplementationFlow41 → E2ETestingFlow53
Rule: Cannot write tests for a component before it is implemented. IMPLEMENT is a prerequisite for TEST in feature_addition context.

---

## TO-009-004 — TEST+UI+QA: weak ordering

A: ["TEST", "UI", "QA"] | "Add tests for existing dropdown and fix dropdown positioning, then QA"
Ordering: E2ETestingFlow53 and UIInteractionFlow15 may be parallel. QAGatingFlow73 must be last.
Rule: When no IMPLEMENT dependency, TEST and UI can execute in either order or in parallel. QA is the only hard constraint.

---

## TO-009-005 — TransitionType resolution priority

Priority order (highest to lowest):
1. "refactor" verb → refactoring (overrides all)
2. IMPLEMENT present → feature_addition
3. TEST + DOCUMENT (no IMPLEMENT) → documentation
4. UI alone / TEST alone / QA alone → documentation
5. QA alone → no TransitionType contribution (gate only)
