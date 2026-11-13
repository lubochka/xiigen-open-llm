# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T017-001 — TEST+UI → documentation: snapshot tests for component library

A: compiled_text: "Write snapshot tests for the button, input, and dropdown components."
   compiled_predicate_ids: ["TEST", "UI"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write snapshot tests' → test artifact documentation"
    - "Components referenced for testing, not being built"
    - "No IMPLEMENT"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T017-002 — TEST+UI+IMPLEMENT → feature_addition: build screenshot harness + assertions

A: compiled_text: "Build the screenshot test harness and create snapshot assertions for every modal variant."
   compiled_predicate_ids: ["TEST", "UI", "IMPLEMENT"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'Build the screenshot test harness' → building new test infrastructure = feature_addition"
    - "IMPLEMENT ('build') overrides documentation default"
    - "Test harness is a new artifact, not a documentation update"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.92

---

## T017-003 — TEST+UI+QA → documentation: full snapshot review

A: compiled_text: "Create visual snapshot tests for each tooltip variant and QA sign-off that all baselines are approved."
   compiled_predicate_ids: ["TEST", "UI", "QA"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Create visual snapshot tests' → test documentation"
    - "QA sign-off validates; does not change TransitionType"
    - "No IMPLEMENT"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90
