# Cross-Layer Synthesis (002)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis

---

## CLS-B39-002 — Gap path: "Automate the UI tests with Cypress"

### Request
"Automate the UI tests with Cypress."

### Predicate Set
[] → GapHandler → SCHEDULE_CAPABILITY_LEARNING

### Cross-Layer Trace

**Compiler (0-A)**: "automate" → gap (not a test-writing verb). "Cypress" → gap (framework name). Zero predicates compiled.
**Router (0-B)**: Zero predicates → GapHandler. DomainId: "software-development".
**Knowledge Edge (0-C)**: automate→gap (not TEST trigger); Cypress→gap (framework name); "UI tests" without test-writing verb → gap.
**Flow Factory (0-D)**: No flow instantiated. GapHandler emits SCHEDULE_CAPABILITY_LEARNING to subproject 16.
**Transition Orchestrator (Layer 1)**: No TransitionType. Gap verdict returned.
**Nine-Node Flow (Layer 2)**: No nine-node flow.
**Teaching Unit (Layer 3)**: "automate" ≠ TEST. Framework names ≠ predicates.

### Key Teaching Points
1. "automate" is gap — not a test-writing verb. TEST fires on: write, create, add, assert, validate (in test context).
2. Framework names (Cypress, Playwright, Selenium) are gap on their own.
3. Zero predicates → SCHEDULE_CAPABILITY_LEARNING emitted to AutonomousLearningPipeline.

### Contrasts: What would fire predicates
- "Write E2E tests for the checkout form" → ["TEST", "UI"] (if form vocabulary present)
- "Write tests using Cypress" → ["TEST"] (test-writing verb present; "using Cypress" is contextual but not a trigger)
- "Set up Cypress" → gap (operational verb + framework name)
- "Automate the test suite" → gap (automate ≠ write tests)
