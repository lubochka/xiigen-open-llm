# Layer 3 — Teaching Unit Pairs (011)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: PairParserUnit and ModelTrainerUnit — what they produce and Gate B criteria

---

## B01-L3-011-P01 — PairParserUnit: what PairParser and ModelTrainer produce

```
source_id: xiigen-b01-l3-011-p01
source_title: XIIGen B01 teaching-011 — PairParser for layer 3 unit contracts
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
  TeachingFocusId: "layer3-unit-contracts-and-gate-b"
  TargetFiles:
    - "layer-3-teaching-unit-pairs.md"

B:
  TeachingFocusId: "layer3-unit-contracts-and-gate-b"
  ParseStatus: "ready"

  PairParserUnitContract:
    ContractSignature: "PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>"
    Input: "BatchFileDescriptor — contains BatchId, WaveId, BatchRootPath, file list, ScenarioId, LicenseState"
    Output: "ParsedPairCollection — contains all parsed A/B pairs organized by layer"
    Responsibilities:
      - "Read each batch file listed in the descriptor"
      - "Extract A/B pairs with source metadata"
      - "Organize pairs by layer (0-A, 0-B, 0-C, 0-D, 1, 2, 3)"
      - "Identify knowledge-edge pairs and mark them"
      - "Run chain integrity check (each pair has valid A and B sections)"
      - "Count pairs per layer and validate against expected minimums"

    OutputFields:
      - "BatchId: string"
      - "ParseStatus: 'ready' | 'failed'"
      - "ParsedPairs: array of ParsedLayer objects"
      - "TotalPairCount: int"
      - "ParsedPredicateCoverage: string[]"
      - "ParsedSubPhaseCoverage: string[]"
      - "KnowledgeEdgePairsFound: int"
      - "ChainIntegrityCheck: 'passed' | 'failed'"
      - "Warnings: string[]"

    GateBCriteria:
      - "ChainIntegrityCheck == 'passed'"
      - "TotalPairCount >= 20"
      - "KnowledgeEdgePairsFound >= 1"
      - "ParsedPredicateCoverage includes 'UI'"
      - "Layer 2 pair count == 9 (one per node)"

  ModelTrainerUnitContract:
    ContractSignature: "ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>"
    Input: "ParsedPairCollection — output from PairParserUnit"
    Output: "TrainingMetrics — contains acceptance verdict and per-layer metrics"
    Responsibilities:
      - "Ingest each A/B pair and validate format"
      - "Compute per-layer confidence statistics"
      - "Check wave acceptance gate criteria"
      - "Produce WaveAcceptanceVerdict and NextAction"
      - "Flag any pairs with Confidence < 0.70 as warnings"

    OutputFields:
      - "BatchId: string"
      - "TrainingStatus: 'complete' | 'failed'"
      - "Metrics: TrainingMetricsData"
      - "GateBAssertions: array of assertion results"
      - "WaveAcceptanceVerdict: 'ACCEPTED' | 'REJECTED'"
      - "NextAction: 'PROMOTE_TO_WAVE_REGISTER' | 'RESUBMIT_WITH_FIXES'"

    GateBCriteria:
      - "total_pairs_ingested >= 20"
      - "pairs_rejected == 0"
      - "knowledge_edge_pairs >= 1"
      - "chain_integrity == 'passed'"
      - "predicate_coverage includes 'UI'"
      - "layer_2_pairs == 9"
      - "min_confidence >= 0.70"

    WaveAcceptanceFlow:
      ACCEPTED: "NextAction = PROMOTE_TO_WAVE_REGISTER; batch enters production register"
      REJECTED: "NextAction = RESUBMIT_WITH_FIXES; GateBAssertions identifies which criterion failed"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-011"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-011-P02 — ModelTrainerUnit: layer 3 unit contracts teaching

```
source_id: xiigen-b01-l3-011-p02
source_title: XIIGen B01 teaching-011 — ModelTrainer for layer 3 unit contracts
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — layer 3 contracts focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "layer3-unit-contracts-and-gate-b"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "1"
    pairs_accepted: "1"
    pairs_rejected: "0"
    contract_fields_documented: "9"
    gate_b_criteria_documented: "7"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "PairParserUnit contract fully documented with input, output, and Gate B criteria"
      Result: "pass"
    - Assertion: "ModelTrainerUnit contract fully documented with input, output, and Gate B criteria"
      Result: "pass"
    - Assertion: "WaveAcceptanceVerdict flow documented (ACCEPTED and REJECTED paths)"
      Result: "pass"

  LearningTargets:
    - "Model understands PairParser reads batch files and extracts A/B pairs"
    - "Model understands ModelTrainer validates pairs and produces acceptance verdict"
    - "Model correctly applies all 7 Gate B criteria for wave acceptance"
    - "Model produces NextAction based on WaveAcceptanceVerdict"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-011"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
