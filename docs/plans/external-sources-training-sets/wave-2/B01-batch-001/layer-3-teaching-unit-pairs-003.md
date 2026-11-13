# Layer 3 — Teaching Unit Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: "navigation" not in UI lexicon — impact on routing

---

## B01-L3-003-P01 — PairParserUnit: "navigation" inert rule

```
source_id: xiigen-b01-l3-003-p01
source_title: XIIGen B01 teaching-003 — PairParser for navigation gap rule
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
  TeachingFocusId: "navigation-not-in-ui-lexicon"
  TargetFiles:
    - "layer-0a-compiler-pairs-002.md"
    - "layer-0c-knowledge-edge-pairs-003.md"

B:
  TeachingFocusId: "navigation-not-in-ui-lexicon"
  ParseStatus: "ready"

  NavigationGapRule:
    Term: "navigation"
    LexiconStatus: "NOT_IN_UI_LEXICON"
    BehaviorAlone: "fires_no_predicate"
    BehaviorWithUITerm: "UI fires via co-present term; 'navigation' remains inert"

    IllustrativeContrasts:
      - Request: "Map the navigation structure for the admin panel."
        Analysis: "'admin panel' and 'navigation' alone — no UI term present"
        Result: "PredicateIds: [] — knowledge-edge path"
        Note: "No sub-phase route; KnowledgeEdgeResult returned"

      - Request: "Map the navigation and interface structure for the admin panel."
        Analysis: "'interface' is in UI lexicon; 'navigation' remains inert but UI fires"
        Result: "PredicateIds: [UI] — routes to 1.1"
        Note: "UI fires via 'interface'; 'navigation' contributes nothing"

      - Request: "Design the screen navigation flow for onboarding."
        Analysis: "'screen' is in UI lexicon; fires UI; 'navigation' inert"
        Result: "PredicateIds: [UI] — routes to 1.1 via 'screen'"
        Note: "The word 'navigation' makes no contribution to predicate detection"

    CommonMistakePattern:
      MistakeType: "false_positive_navigation"
      BadBehavior: "Model fires UI predicate for 'navigation' alone"
      CorrectBehavior: "Model fires no predicate for 'navigation' alone"
      DetectionMarker: "If UI fires and only evidence is 'navigation', this is v6-01 corruption"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-003"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-003-P02 — ModelTrainerUnit: "navigation" gap rule

```
source_id: xiigen-b01-l3-003-p02
source_title: XIIGen B01 teaching-003 — ModelTrainer for navigation gap
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — navigation gap focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "navigation-not-in-ui-lexicon"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "2"
    pairs_accepted: "2"
    pairs_rejected: "0"
    knowledge_edge_pairs: "1"
    gap_terms_documented: "1"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "'navigation' alone produces knowledge-edge path (no predicate)"
      Result: "pass"
    - Assertion: "'navigation' + UI term produces UI predicate via co-present term"
      Result: "pass"
    - Assertion: "co-presence rule correctly attributed"
      Result: "pass"

  LearningTargets:
    - "Model recognizes 'navigation' as outside UI lexicon"
    - "Model correctly routes 'navigation alone' to knowledge-edge path"
    - "Model does not attribute UI firing to 'navigation' when co-present UI term exists"
    - "Model identifies v6-01 corruption when 'navigation' is sole UI evidence"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-003"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
