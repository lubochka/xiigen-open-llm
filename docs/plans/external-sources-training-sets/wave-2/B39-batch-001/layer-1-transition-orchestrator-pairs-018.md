# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T018-001 — TEST+UI+QA → documentation: cross-browser suite + QA

A: compiled_text: "Write cross-browser tests for the product listing page layout on Chrome, Firefox, Safari, and QA the results."
   compiled_predicate_ids: ["TEST", "UI", "QA"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write cross-browser tests' → test artifact documentation"
    - "QA validates results; does not set TransitionType"
    - "No IMPLEMENT"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T018-002 — TEST+UI → documentation: modal on IE11/Edge

A: compiled_text: "Add test coverage for the confirmation modal on IE11 and Edge to catch flexbox rendering bugs."
   compiled_predicate_ids: ["TEST", "UI"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Add test coverage' → adding test documentation"
    - "Modal referenced as test target, not being built"
    - "No IMPLEMENT"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T018-003 — TEST+UI+QA → documentation: regression + QA acceptance

A: compiled_text: "Write cross-browser regression test cases for each form input component and get QA acceptance."
   compiled_predicate_ids: ["TEST", "UI", "QA"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write regression test cases' → test artifact documentation"
    - "'QA acceptance' = gate validation, not a TransitionType driver"
    - "No IMPLEMENT"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90
