# Cross-Layer Synthesis (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B39-003 — TEST+UI: "Write responsive tests and fix breakpoints"

### Request
"Write responsive tests for the product grid at 320px, 768px, and 1440px, and fix the column layout at 768px."

### Predicate Set
["TEST", "UI"]

### Cross-Layer Trace

**Compiler (0-A)**: "write responsive tests" → TEST; "column layout", "320px/768px/1440px" (responsive vocabulary) → UI. Two predicates.
**Router (0-B)**: TEST → 5.3 (E2ETestingFlow53, responsive/viewport context); UI → 1.5 (UIInteractionFlow15). Two flows.
**Knowledge Edge (0-C)**: write-responsive-tests→TEST; column-layout+viewport-sizes→UI. Two independent trigger edges.
**Flow Factory (0-D)**: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation (no IMPLEMENT). No QA → no gate.
**Transition Orchestrator (Layer 1)**: E2ETestingFlow53 and UIInteractionFlow15 may execute in parallel (no dependency, no QA ordering constraint).
**Nine-Node Flow (Layer 2)**: E2ETestingFlow53 → responsive tests at 3 breakpoints, all pass. UIInteractionFlow15 → 768px two-column layout fixed.

### Key Teaching Points
1. TEST + UI: two predicates, two flows. TEST does not subsume UI.
2. TEST routes to 5.3 (responsive/E2E context). UI always routes to 1.5.
3. TransitionType: documentation (no IMPLEMENT). Testing existing component + fixing bug.
4. No QA gate → no ordering constraint. TEST and UI can run in parallel.
