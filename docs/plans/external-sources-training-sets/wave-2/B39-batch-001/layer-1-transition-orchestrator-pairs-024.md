# Layer 1 — Transition Orchestrator Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T024-001 — Complete TransitionType reference: TEST alone

A: TEST alone fires. What is the TransitionType?
B: documentation. Test-writing without IMPLEMENT → producing test artifact documentation. Rule is absolute: TEST without IMPLEMENT → documentation.

---

## T024-002 — Complete TransitionType reference: TEST+UI without IMPLEMENT

A: TEST+UI fires (no IMPLEMENT). What is the TransitionType?
B: documentation. Multiple predicates do not change the rule when IMPLEMENT is absent. TEST+UI → documentation. The UI predicate does not contribute to feature_addition.

---

## T024-003 — Complete TransitionType reference: IMPLEMENT present overrides

A: TEST+UI+IMPLEMENT fires. What is the TransitionType?
B: feature_addition. IMPLEMENT present → overrides documentation default. Applies regardless of how many other predicates are present. IMPLEMENT+TEST+UI+QA → feature_addition.

---

## T024-004 — Complete TransitionType reference: QA alone

A: QA alone fires. What is the TransitionType?
B: QA alone does not set TransitionType. Result: TransitionType = null / no contribution. QA is a validator, not a builder or documenter. QA gate is a final pass; it validates prior work but does not produce a TransitionType independently.

---

## T024-005 — Complete TransitionType reference: the add-as-test-verb ambiguity

A: "Add tests for the checkout button." Does "add" fire IMPLEMENT?
B: No. "add" in the context of "add tests" (adding test coverage/specs) fires TEST as the test-writing verb. It does not fire IMPLEMENT. IMPLEMENT fires when "add" is combined with a feature noun: "add a button", "add an endpoint", "add a service". Context (test noun vs. feature noun) determines whether "add" fires TEST or IMPLEMENT.
Rule: If the direct object of "add" is a test artifact (tests, spec, coverage, assertions, scenarios) → TEST fires via "add [test artifact]". If the direct object is a product/feature noun → IMPLEMENT fires.
