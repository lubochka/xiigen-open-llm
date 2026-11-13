# Layer 1 — Transition Orchestrator Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Contract: TransitionOrchestratorUnit : ILogicalUnit<FlowInstanceDescriptor, TransitionClassification>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B20B1-WDA-trans-001 — TEST feature_addition (adding tests to existing service)

```
source_id: xiigen-b20-trans-001
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: transition_orchestrator
```

A:
  compiled_text: "Write unit tests for the PaymentCalculationService covering all edge cases."
  compiled_predicate_ids: ["TEST"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "Adding a test suite to an existing service = net-new test coverage = feature_addition"
    - "No refactoring (refactor/restructure/extract) vocabulary"
    - "No documentation-only signals"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

  TransitionType note: Writing tests for an existing service is classified as
  feature_addition (adding new test artifacts) rather than refactoring or documentation.
  Tests are code artifacts that add coverage, not structural changes to existing code.

---

## Pair B20B1-WDA-trans-002 — TEST + SECURITY + IMPLEMENT → feature_addition

```
source_id: xiigen-b20-trans-002
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: transition_orchestrator
```

A:
  compiled_text: "Write the test suite for the authentication service and verify the JWT token validation logic."
  compiled_predicate_ids: ["TEST", "SECURITY", "IMPLEMENT"]
  flow_id: "UnitTestingFlow51"

B:
  TransitionType: feature_addition
  TransitionEvidence:
    - "'write the test suite' — constructing a new test artifact = feature_addition"
    - "'verify JWT token validation' — adding verification coverage = feature_addition"
    - "Authentication service test suite is net-new"
    - "No refactoring or migration signals"
  PhaseId: 5
  SubPhaseId: 5.1
  TransitionConfidence: 0.92

  Multi-predicate TransitionType: All three predicates (TEST, SECURITY, IMPLEMENT) are
  classified as feature_addition because the request constructs new artifacts (test suite,
  validation logic). A single TransitionType applies to the request as a whole; individual
  predicate routing decisions do not each carry independent TransitionTypes.
