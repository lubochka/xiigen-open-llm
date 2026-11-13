# Cross-Layer Synthesis (001)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B39-001 — "Build modal, write tests, fix focus trap, QA sign-off"

### Request
"Build the confirmation modal, write interaction tests, fix the focus trap behavior, and QA sign-off before merge."

### Predicate Set
["IMPLEMENT", "TEST", "UI", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "build" → IMPLEMENT; "write interaction tests" → TEST; "confirmation modal", "focus trap behavior" → UI; "QA sign-off" → QA. Four predicates compiled.
**Router (0-B)**: IMPLEMENT → 4.1; TEST → 5.3 (interaction/E2E context); UI → 1.5; QA → 7.3 (feature acceptance). Four sub-phases.
**Knowledge Edge (0-C)**: build→IMPLEMENT; write-interaction-tests→TEST; confirmation-modal+focus-trap→UI; QA-sign-off→QA.
**Flow Factory (0-D)**: [ImplementationFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]. TransitionType: feature_addition (IMPLEMENT present). QA last.
**Transition Orchestrator (Layer 1)**: IMPLEMENT → TEST+UI (parallel) → QA (last). QA validates modal implementation, interaction tests, and focus trap fix.
**Nine-Node Flow (Layer 2)**: Modal built with confirm/cancel/ESC. Interaction tests written (6 gates, PASS). Focus trap fixed. QAGatingFlow73: PASS.
**Teaching Unit (Layer 3)**: Four predicates from four independent clauses. QA validates all three preceding outputs. IMPLEMENT drives TransitionType.

### Key Teaching Points
1. "build" → IMPLEMENT (4.1); "write interaction tests" → TEST (5.3); UI vocabulary → UI (1.5); "QA sign-off" → QA (7.3).
2. All four predicates fire independently. No collapse (v6-03 would suppress TEST or UI).
3. TransitionType: feature_addition (IMPLEMENT present). QA does not drive TransitionType.
4. QA gate validates complete work package: implementation + tests + UI fix.
