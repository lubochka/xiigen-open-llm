# Cross-Layer Synthesis (005)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-B39-005 — TEST alone: Cypress checkout flow (documentation TransitionType)

### Request
"Write Cypress e2e tests for the shopping cart checkout flow: add-to-cart, quantity update, and proceed-to-checkout button."

### Predicate Set
["TEST", "UI"]
Note: "Cypress" is gap (framework name). "e2e tests" → TEST. "cart", "button" → UI.

### Cross-Layer Trace

**Compiler (0-A)**: "write e2e tests" → TEST; "cart" — "cart" is not in UI lexicon; "button" → UI fires; "checkout flow" → gap. CompiledPredicateIds: ["TEST", "UI"].

**Router (0-B)**: TEST → 5.3 (E2ETestingFlow53); UI → 1.5 (UIInteractionFlow15). Two sub-phases.

**Knowledge Edge (0-C)**: "Cypress" → gap (framework name); "add-to-cart" → gap (feature vocabulary, not UI component name); "quantity update" → "update" is gap; "proceed-to-checkout button" → "button" fires UI; "e2e tests" → TEST.

**Flow Factory (0-D)**: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation (TEST+UI without IMPLEMENT).

**Transition Orchestrator (Layer 1)**: TransitionType: documentation. TEST+UI without IMPLEMENT → producing test documentation artifacts. No feature is being built.

**Nine-Node Flow (Layer 2)**: DomainSplitter: cart button interaction + quantity selector + navigation. 4 test cases written. Arbiter: PASS.

**Teaching Unit (Layer 3)**: "Cypress" is gap. "e2e tests" is the TEST trigger. "button" is the UI trigger. TransitionType: documentation for TEST+UI. Contrast: if "build the Cypress test harness" were added, IMPLEMENT fires → feature_addition.

### Key Teaching Points
1. Framework name (Cypress) is gap. Predicate fires on "e2e tests" and "button".
2. TEST alone (without QA) produces documentation TransitionType. No QA gate.
3. Two predicates → two flows: E2ETestingFlow53, UIInteractionFlow15.
4. "Cypress e2e tests" → sub-phase 5.3, not 5.1. E2E/browser vocabulary → 5.3.
