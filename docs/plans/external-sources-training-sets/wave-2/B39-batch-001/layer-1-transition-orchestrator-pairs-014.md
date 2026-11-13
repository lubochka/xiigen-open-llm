# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T014-001 — TEST+UI → documentation: Playwright login form test cases

A: compiled_text: "Write Playwright test cases for the login form: valid credentials, invalid password, locked-out account."
   compiled_predicate_ids: ["TEST", "UI"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Write test cases' → test documentation artifacts for existing login form"
    - "No IMPLEMENT → no feature_addition"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.90

---

## T014-002 — TEST+UI+IMPLEMENT → feature_addition: build login page + write assertions

A: compiled_text: "Implement the login page and write assertions for each validation error state on input fields."
   compiled_predicate_ids: ["TEST", "UI", "IMPLEMENT"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'implement the login page' → constructing a new page = feature_addition"
    - "'write assertions' → test artifacts accompany the new feature"
    - "IMPLEMENT is the overriding predicate"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.92

---

## T014-003 — TEST+UI → documentation: spec coverage for nav keyboard accessibility

A: compiled_text: "Add spec coverage for the top navigation menu keyboard accessibility interactions."
   compiled_predicate_ids: ["TEST", "UI"] | flow_id: "E2ETestingFlow53"
B:
  TransitionType: documentation
  TransitionEvidence:
    - "'Add spec coverage' → new test specification artifacts"
    - "No IMPLEMENT verb; 'add' here is in the context of adding test specs, not building features"
  PhaseId: 5 | SubPhaseId: 5.3 | TransitionConfidence: 0.88
  Note: "'add' as a test-writing verb (add spec coverage, add tests) → documentation. 'add' as IMPLEMENT (add a button, add a service) → feature_addition. Context determines which fires."
