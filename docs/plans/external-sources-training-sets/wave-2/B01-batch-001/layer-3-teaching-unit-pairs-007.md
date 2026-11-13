# Layer 3 — Teaching Unit Pairs (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: DOCUMENT + UI routing to both 1.1 and 2.1

---

## B01-L3-007-P01 — PairParserUnit: DOCUMENT + UI dual routing

```
source_id: xiigen-b01-l3-007-p01
source_title: XIIGen B01 teaching-007 — PairParser for DOCUMENT+UI dual routing
source_license_state: first_party
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: learning_pipeline_ingestion
node_role: pair-parser
coverage_status: covered
pair_kind: teaching_unit
```

A:
  BatchId: "B01-batch-001"
  TeachingFocusId: "document-ui-dual-routing"
  TargetFiles:
    - "layer-0b-router-pairs-004.md"
    - "layer-0b-router-pairs-005.md"
    - "layer-0b-router-pairs-009.md"
    - "layer-0c-knowledge-edge-pairs-008.md"
    - "layer-1-transition-orchestrator-pairs-003.md"

B:
  TeachingFocusId: "document-ui-dual-routing"
  ParseStatus: "ready"

  DualRoutingRule:
    PredicateCombination: "[UI, DOCUMENT]"
    SubPhasesActivated: ["1.1", "2.1"]
    FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]
    SubPhaseIdInTransition: "1.1"
    Note: "UI wins the SubPhaseId slot (primary hierarchy); DOCUMENT gets its own routing decision"

    RoutingDecisionCount: 2
    Decision1:
      SubPhaseId: "1.1"
      Predicate: "UI"
      FlowId: "ScreenInventoryFlow11"
      IsPrimary: "not in RoutingDecision — determined by SubPhaseId in transition"

    Decision2:
      SubPhaseId: "2.1"
      Predicate: "DOCUMENT"
      FlowId: "BusinessRequirementsFlow21"
      IsPrimary: "not in RoutingDecision"

    IndependenceRule:
      Principle: "Each predicate routes independently; DOCUMENT does not 'depend' on UI"
      BadPattern: "Suppressing DOCUMENT routing because UI is already routing to 1.1"
      CorrectPattern: "Both predicates produce their own routing decisions; both flows execute"

    TransitionTypeRule:
      UI_plus_DOCUMENT_without_IMPLEMENT: "documentation"
      UI_plus_DOCUMENT_plus_IMPLEMENT: "feature_addition"
      Explanation: "IMPLEMENT presence is the feature_addition gate; UI+DOCUMENT alone is documentation"

    FlowExecutionOrder:
      Standard: "ScreenInventoryFlow11 first, then BusinessRequirementsFlow21"
      Note: "UI sub-phase flows execute before DOCUMENT sub-phase flows"

    ContrastPairs:
      - Request: "Map the login screens and document the requirements."
        Result: "[UI, DOCUMENT] → 1.1 + 2.1, documentation TransitionType"

      - Request: "Map the login screens only."
        Result: "[UI] → 1.1 only, feature_addition TransitionType"

      - Request: "Document the requirements only."
        Result: "[DOCUMENT] → 2.1 only, documentation TransitionType"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-007"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-007-P02 — ModelTrainerUnit: DOCUMENT + UI dual routing

```
source_id: xiigen-b01-l3-007-p02
source_title: XIIGen B01 teaching-007 — ModelTrainer for DOCUMENT+UI dual routing
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — dual routing focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "document-ui-dual-routing"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "5"
    pairs_accepted: "5"
    pairs_rejected: "0"
    dual_routing_scenarios: "3"
    independence_rule_documented: "1"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "[UI, DOCUMENT] produces 2 routing decisions (1.1 and 2.1)"
      Result: "pass"
    - Assertion: "SubPhaseId in transition = 1.1 when UI present"
      Result: "pass"
    - Assertion: "DOCUMENT independence rule documented (not suppressed by UI)"
      Result: "pass"
    - Assertion: "TransitionType = documentation for UI+DOCUMENT without IMPLEMENT"
      Result: "pass"

  LearningTargets:
    - "Model produces two routing decisions for [UI, DOCUMENT]"
    - "Model never suppresses DOCUMENT routing when UI is also present"
    - "Model sets SubPhaseId to 1.1 when UI is the primary predicate"
    - "Model sets TransitionType to documentation for UI+DOCUMENT without IMPLEMENT"
    - "Model executes ScreenInventoryFlow11 before BusinessRequirementsFlow21"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-007"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
