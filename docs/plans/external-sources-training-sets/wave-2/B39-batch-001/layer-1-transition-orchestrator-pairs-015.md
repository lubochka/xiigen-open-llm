# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T015-001 — TEST+UI+QA → documentation: visual regression + QA sign-off

A: compiled_text: "Write visual regression tests for buttons, cards, modals and QA sign-off on all baselines."
   compiled_predicate_ids: ["TEST", "UI", "QA"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write visual regression tests' → test documentation artifacts"
    - "QA sign-off validates test output; does not set TransitionType"
    - "No IMPLEMENT verb"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T015-002 — TEST+UI → documentation: snapshot baseline for product card

A: compiled_text: "Create snapshot tests for the product card component across light and dark themes."
   compiled_predicate_ids: ["TEST", "UI"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Create snapshot tests' → generating test artifacts"
    - "'create' as test verb (create tests) → documentation; 'create' as IMPLEMENT (create a component) → feature_addition"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T015-003 — TEST+QA → documentation: assert baselines pass QA review

A: compiled_text: "Assert that all visual regression test baselines pass QA review before the design token update ships."
   compiled_predicate_ids: ["TEST", "QA"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'assert' → test assertion artifact (documentation)"
    - "QA review validates; does not contribute to TransitionType"
    - "No IMPLEMENT, no UI component builds"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90
