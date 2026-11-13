# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T013-001 — TEST+UI → documentation: Cypress e2e checkout button

A: compiled_text: "Write Cypress e2e tests for the shopping cart button flow."
   compiled_predicate_ids: ["TEST", "UI"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write e2e tests' — producing new test documentation artifacts"
    - "No IMPLEMENT verb → no feature_addition"
    - "UI vocabulary fires but no implementation action"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T013-002 — TEST+UI+QA → documentation: full e2e + QA sign-off

A: compiled_text: "Write e2e tests for the product detail page and QA sign-off before release."
   compiled_predicate_ids: ["TEST", "UI", "QA"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write e2e tests' → test documentation artifacts"
    - "QA sign-off validates test output but does not change TransitionType"
    - "No IMPLEMENT verb present"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T013-003 — TEST+UI+IMPLEMENT → feature_addition: build + test cart

A: compiled_text: "Implement the Cypress test harness and write e2e tests for the cart button."
   compiled_predicate_ids: ["TEST", "UI", "IMPLEMENT"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement' verb → building new artifacts = feature_addition"
    - "IMPLEMENT overrides documentation default"
    - "Test harness is a new production artifact"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.92
