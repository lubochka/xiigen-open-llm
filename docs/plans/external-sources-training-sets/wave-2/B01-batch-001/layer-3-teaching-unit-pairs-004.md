# Layer 3 — Teaching Unit Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: "prototype" IS in the UI lexicon in B01 context

---

## B01-L3-004-P01 — PairParserUnit: "prototype" B01 context rule

```
source_id: xiigen-b01-l3-004-p01
source_title: XIIGen B01 teaching-004 — PairParser for prototype UI membership in B01
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
  TeachingFocusId: "prototype-ui-membership-b01"
  TargetFiles:
    - "layer-0a-compiler-pairs-007.md"
    - "layer-0c-knowledge-edge-pairs-004.md"

B:
  TeachingFocusId: "prototype-ui-membership-b01"
  ParseStatus: "ready"

  PrototypeRule:
    Term: "prototype"
    LexiconStatus_B01: "IN_UI_LEXICON"
    LexiconStatus_B32: "GAP_VOCABULARY"
    Note: "Lexicon membership is scenario-specific; B01 includes 'prototype' in UI lexicon; B32 treats it as a gap term"

    B01Behavior:
      - Request: "Create an interactive prototype for the onboarding wizard."
        Analysis: "'prototype' fires UI in B01"
        Result: "PredicateIds: [UI] — routes to 1.1"

      - Request: "Wireframe and prototype the signup screens."
        Analysis: "'wireframe' and 'prototype' both fire UI; redundant but consistent"
        Result: "PredicateIds: [UI] — routes to 1.1"

      - Request: "Design the prototype and document the UX specification."
        Analysis: "'prototype' fires UI; 'document' + 'specification' fire DOCUMENT"
        Result: "PredicateIds: [UI, DOCUMENT] — routes to 1.1 + 2.1"

    ContrastWithB32:
      ScenarioId_B32: "B32"
      B32Behavior: "In B32, 'prototype' is gap vocabulary — fires no predicate, returns knowledge-edge result"
      B01Behavior: "In B01, 'prototype' is a full UI lexicon member — fires UI predicate"
      Rule: "Always apply the lexicon of the current scenario; do not carry gap status from other scenarios"

    KeyDistinction:
      Principle: "Lexicon membership is defined per scenario, not universally"
      B01Conclusion: "'prototype' → UI → 1.1 in all B01 requests"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-004"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-004-P02 — ModelTrainerUnit: "prototype" B01 context

```
source_id: xiigen-b01-l3-004-p02
source_title: XIIGen B01 teaching-004 — ModelTrainer for prototype UI membership
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — prototype B01 focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "prototype-ui-membership-b01"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "2"
    pairs_accepted: "2"
    pairs_rejected: "0"
    scenario_specific_rules_taught: "1"
    contrast_pairs_documented: "1"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "'prototype' fires UI in B01 context"
      Result: "pass"
    - Assertion: "B01 vs B32 contrast documented"
      Result: "pass"
    - Assertion: "scenario-specific lexicon rule established"
      Result: "pass"

  LearningTargets:
    - "Model applies B01 lexicon when processing B01 requests"
    - "Model fires UI for 'prototype' in B01"
    - "Model understands lexicon membership is per-scenario"
    - "Model does not apply B32 gap status to B01 vocabulary"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-004"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
