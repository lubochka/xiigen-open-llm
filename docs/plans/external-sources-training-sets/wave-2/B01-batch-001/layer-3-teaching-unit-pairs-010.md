# Layer 3 — Teaching Unit Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: TransitionType determination in B01

---

## B01-L3-010-P01 — PairParserUnit: TransitionType determination

```
source_id: xiigen-b01-l3-010-p01
source_title: XIIGen B01 teaching-010 — PairParser for TransitionType determination
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
  TeachingFocusId: "transition-type-determination-b01"
  TargetFiles:
    - "layer-1-transition-orchestrator-pairs.md"
    - "layer-1-transition-orchestrator-pairs-003.md"
    - "layer-1-transition-orchestrator-pairs-007.md"
    - "layer-1-transition-orchestrator-pairs-009.md"
    - "layer-0c-knowledge-edge-pairs-010.md"

B:
  TeachingFocusId: "transition-type-determination-b01"
  ParseStatus: "ready"

  TransitionTypeRules:
    Rule: "IMPLEMENT present → feature_addition; otherwise → documentation"
    IMPLEMENTGate: true
    NullPredicate: "knowledge_edge_result (no sub-phase route)"

    DecisionTable:
      - PredicateCombo: "[UI]"
        TransitionType: "feature_addition"
        Note: "UI alone is feature_addition — screen work is a feature"

      - PredicateCombo: "[DOCUMENT]"
        TransitionType: "documentation"
        Note: "DOCUMENT alone produces a documentation artifact"

      - PredicateCombo: "[ARCHITECTURE]"
        TransitionType: "documentation"
        Note: "Architecture specification without implementation = documentation"

      - PredicateCombo: "[UI, DOCUMENT]"
        TransitionType: "documentation"
        Note: "No IMPLEMENT present; UX doc + spec = documentation"

      - PredicateCombo: "[UI, ARCHITECTURE]"
        TransitionType: "documentation"
        Note: "No IMPLEMENT; architecture + screen design = documentation"

      - PredicateCombo: "[UI, IMPLEMENT]"
        TransitionType: "feature_addition"
        Note: "IMPLEMENT present — feature_addition"

      - PredicateCombo: "[DOCUMENT, IMPLEMENT]"
        TransitionType: "feature_addition"
        Note: "IMPLEMENT present — feature_addition even for documentation with write verb"

      - PredicateCombo: "[UI, DOCUMENT, IMPLEMENT]"
        TransitionType: "feature_addition"
        Note: "IMPLEMENT present — feature_addition"

      - PredicateCombo: "[UI, ARCHITECTURE, IMPLEMENT]"
        TransitionType: "feature_addition"
        Note: "IMPLEMENT present — feature_addition"

      - PredicateCombo: "[UI, ARCHITECTURE, IMPLEMENT, DOCUMENT]"
        TransitionType: "feature_addition"
        Note: "IMPLEMENT present — feature_addition; this is the four-predicate case"

      - PredicateCombo: "[]"
        TransitionType: "N/A"
        Note: "No predicate → knowledge_edge_result; no TransitionType assigned"

    PipelineConsequences:
      DocumentationType: "Produces documentation artifacts; ImplementationFlow41 NOT selected"
      FeatureAdditionType: "Produces implementation artifacts; ImplementationFlow41 IS selected"
      Misclassification: "If feature_addition set when should be documentation, ImplementationFlow41 fires incorrectly"

    SpecialCases:
      UIAlone: "UI alone → feature_addition (screen inventory IS a feature deliverable in B01)"
      ArchitectureAlone: "ARCHITECTURE alone → documentation (spec only)"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-010"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-010-P02 — ModelTrainerUnit: TransitionType determination

```
source_id: xiigen-b01-l3-010-p02
source_title: XIIGen B01 teaching-010 — ModelTrainer for TransitionType determination
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — TransitionType focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "transition-type-determination-b01"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "5"
    pairs_accepted: "5"
    pairs_rejected: "0"
    decision_table_rows: "10"
    special_cases_documented: "2"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "IMPLEMENT present → feature_addition rule documented for all 5 IMPLEMENT combos"
      Result: "pass"
    - Assertion: "DOCUMENT/ARCHITECTURE without IMPLEMENT → documentation for all 4 combos"
      Result: "pass"
    - Assertion: "UI alone → feature_addition special case documented"
      Result: "pass"
    - Assertion: "pipeline consequences of misclassification documented"
      Result: "pass"

  LearningTargets:
    - "Model applies IMPLEMENT gate rule for TransitionType"
    - "Model sets feature_addition when IMPLEMENT is in PredicateIds"
    - "Model sets documentation for UI+DOCUMENT or ARCHITECTURE without IMPLEMENT"
    - "Model does not fire ImplementationFlow41 for documentation TransitionType"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-010"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
