# Cross-Layer Synthesis (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: cross_layer_synthesis

---

## CLS-B39-006 — TEST+UI+QA: full three-predicate trace for visual regression

### Request
"Write visual regression tests for the component library button, card, and modal components, assert all baselines match, and QA sign-off on the complete regression suite."

### Predicate Set
["TEST", "UI", "QA"]

### Cross-Layer Trace

**Compiler (0-A)**: "write visual regression tests" → TEST (compound trigger); "assert all baselines match" → TEST (second trigger, deduplicated); "button", "card", "modal", "components" → UI (four UI triggers, deduplicated to one predicate); "QA sign-off" → QA. CompiledPredicateIds: ["TEST", "UI", "QA"].

**Router (0-B)**: TEST → 5.3 (E2ETestingFlow53); UI → 1.5 (UIInteractionFlow15); QA → 7.3 (QAGatingFlow73). Three independent routing decisions.

**Knowledge Edge (0-C)**: "visual regression tests" is a valid TEST compound. "assert" deduplicates with TEST already fired. "button/card/modal/components" are all UI vocabulary — four triggers → one UI predicate. "QA sign-off" fires QA on "QA" abbreviation. Deduplication confirmed.

**Flow Factory (0-D)**: [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]. TransitionType: documentation. QA flow last. No IMPLEMENT → no feature_addition.

**Transition Orchestrator (Layer 1)**: Phase 1: E2ETestingFlow53 + UIInteractionFlow15 (parallel). Phase 2: QAGatingFlow73 (last — validates visual regression suite and UI component baselines). TransitionType: documentation.

**Nine-Node Flow (Layer 2)**: 10 snapshot tests across button/card/modal. VisualRegressionArbiter: PASS_WITH_WARNINGS (secondary button baseline pending). QAGatingFlow73: sign-off pending one baseline.

**Teaching Unit (Layer 3)**: Three predicates from dense signal. UI deduplication: four component triggers → one UI. QA validates test output. TransitionType documentation confirms TEST+UI without IMPLEMENT.

### Key Teaching Points
1. "visual regression tests" + "assert" → two TEST triggers → one TEST predicate (deduplication).
2. "button + card + modal + components" → four UI triggers → one UI predicate (deduplication).
3. TEST+UI+QA without IMPLEMENT → TransitionType: documentation.
4. QAGatingFlow73 is always the final flow; it validates E2ETestingFlow53 and UIInteractionFlow15 outputs.
5. QA does not change TransitionType; QA is a validator, not a builder.
