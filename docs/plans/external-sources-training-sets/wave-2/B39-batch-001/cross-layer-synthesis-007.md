# Cross-Layer Synthesis (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-B39-007 — Four-predicate: IMPLEMENT+TEST+UI+QA (build + test + UI + QA gate)

### Request
"Build the accessibility test runner harness, write E2E tests for the modal and form inputs, and QA sign-off before the sprint ships."

### Predicate Set
["IMPLEMENT", "TEST", "UI", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "build" → IMPLEMENT; "write E2E tests" → TEST; "modal", "form inputs" → UI; "QA sign-off" → QA. Four predicates compiled.

**Router (0-B)**: IMPLEMENT → 4.1 (BackendServiceFlow41); TEST → 5.3 (E2ETestingFlow53); UI → 1.5 (UIInteractionFlow15); QA → 7.3 (QAGatingFlow73). Four sub-phases.

**Knowledge Edge (0-C)**: "build" fires IMPLEMENT (confirmed IMPLEMENT lexicon verb). "write E2E tests" fires TEST. "modal" + "form inputs" → UI (two triggers → one UI predicate). "QA sign-off" fires QA. Four independent predicate edges.

**Flow Factory (0-D)**: [BackendServiceFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]. TransitionType: feature_addition (IMPLEMENT present — overrides documentation default). QA flow last.

**Transition Orchestrator (Layer 1)**:
  Phase 1: BackendServiceFlow41 — build accessibility test runner harness.
  Phase 2: E2ETestingFlow53 + UIInteractionFlow15 (parallel) — write E2E tests; UI modal/form coverage.
  Phase 3: QAGatingFlow73 — QA validates: harness built, E2E tests pass, UI coverage confirmed.
  TransitionType: feature_addition.

**Nine-Node Flow (Layer 2)**: Harness built (BackendServiceFlow41). 8 E2E tests written (E2ETestingFlow53). Modal + form inputs covered (UIInteractionFlow15). QAGatingFlow73: PASS — harness functional, tests green.

**Teaching Unit (Layer 3)**: Four predicates from four clauses. IMPLEMENT drives feature_addition. QA validates complete work package. IMPLEMENT → TEST+UI (parallel) → QA (last) is the canonical ordering.

### Key Teaching Points
1. Four predicates → four independent flows → four sub-phases.
2. IMPLEMENT present → TransitionType: feature_addition (overrides documentation default from TEST+UI).
3. QA gate validates: implementation + tests + UI coverage. All three must pass before QA signs off.
4. IMPLEMENT+TEST+UI+QA is the canonical "build + test + deliver" pattern in B39.
5. "build the harness" = feature_addition because a new artifact is constructed. Compare CLS-005 (write tests only = documentation).
