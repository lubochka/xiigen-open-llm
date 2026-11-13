# Layer 3 — Teaching Unit Pairs
# Batch: O1-batch-001
# Scenario: O1 — multi-tenant JWT authentication
# Contracts: PairParserUnit, ModelTrainerUnit
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Method note

Layer 3 pairs teach the model how the learning pipeline ingests and trains
on the structured A/B pairs produced in Layers 0-2. These are meta-pairs —
the A side describes a batch file or parsed pair collection, and the B side
describes what the pipeline unit produces from it.

  Pair 001: PairParserUnit — reads a Layer 0/1/2 batch file and extracts the
    structured A/B pair collection with metadata.

  Pair 002: ModelTrainerUnit — takes a parsed pair collection and produces
    training metrics that satisfy the wave acceptance gate.

---

## Coverage map

| Pair ID | Unit | Contract |
|---|---|---|
| O1B1-WDA-L3-001 | PairParserUnit | ILogicalUnit<BatchFileDescriptor, ParsedPairCollection> |
| O1B1-WDA-L3-002 | ModelTrainerUnit | ILogicalUnit<ParsedPairCollection, TrainingMetrics> |

---

## Pair O1B1-WDA-L3-001 — PairParserUnit

```
source_id: xiigen-o1-l3-parser-001
source_title: XIIGen O1 teaching unit — PairParserUnit batch parse
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
  BatchId: "O1-batch-001"
  WaveId: "wave-2"
  BatchRootPath: "external-sources-training-sets/wave-2/O1-batch-001/"
  Files:
    - "layer-0a-compiler-pairs.md"
    - "layer-0b-router-pairs.md"
    - "layer-0c-knowledge-edge-pairs.md"
    - "layer-0d-flow-factory-pairs.md"
    - "layer-1-transition-orchestrator-pairs.md"
    - "layer-2-nine-node-3.4-pairs.md"
    - "layer-3-teaching-unit-pairs.md"
  ScenarioId: "O1-multi-tenant-jwt"
  LicenseState: "first_party"

B:
  BatchId: "O1-batch-001"
  ParseStatus: "ready"
  ParsedPairs:
    - Layer: "0-A"
      PairCount: 3
      ContractSignature: "PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>"
      PairIds: ["O1B1-WDA-compiler-001", "O1B1-WDA-compiler-002", "O1B1-WDA-compiler-003"]

    - Layer: "0-B"
      PairCount: 3
      ContractSignature: "MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>"
      PairIds: ["O1B1-WDA-router-001", "O1B1-WDA-router-002", "O1B1-WDA-router-003"]

    - Layer: "0-C"
      PairCount: 2
      ContractSignature: "AdaptiveLogicalOrchestrator — knowledge-edge result path"
      PairIds: ["O1B1-WDA-kedge-001", "O1B1-WDA-kedge-002"]

    - Layer: "0-D"
      PairCount: 4
      ContractSignature: "ILogicalFlowFactory<TInput> — flow factory selection"
      PairIds: ["O1B1-WDA-factory-001", "O1B1-WDA-factory-002", "O1B1-WDA-factory-003", "O1B1-WDA-factory-004"]

    - Layer: "1"
      PairCount: 2
      ContractSignature: "TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>"
      PairIds: ["O1B1-WDA-T1-001", "O1B1-WDA-T1-002"]

    - Layer: "2"
      PairCount: 9
      ContractSignature: "SecurityVerification34Flow — Nodes 01-09"
      PairIds: ["O1B1-3.4-N01-001", "O1B1-3.4-N02-001", "O1B1-3.4-N03-001", "O1B1-3.4-N04-001", "O1B1-3.4-N05-001", "O1B1-3.4-N06-001", "O1B1-3.4-N07-001", "O1B1-3.4-N08-001", "O1B1-3.4-N09-001"]

    - Layer: "3"
      PairCount: 2
      ContractSignature: "PairParserUnit + ModelTrainerUnit"
      PairIds: ["O1B1-WDA-L3-001", "O1B1-WDA-L3-002"]

  TotalPairCount: 25
  ParsedPredicateCoverage: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST", "DOCUMENT"]
  ParsedSubPhaseCoverage: ["3.4", "3.3", "5.1", "2.1"]
  KnowledgeEdgePairsFound: 2
  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-v1"
    UnitRole: "pair-parser"
    BatchId: "O1-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## Pair O1B1-WDA-L3-002 — ModelTrainerUnit

```
source_id: xiigen-o1-l3-trainer-001
source_title: XIIGen O1 teaching unit — ModelTrainerUnit training metrics
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: learning_pipeline_training
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser B — 25 pairs across 7 layers]

B:
  BatchId: "O1-batch-001"
  TrainingStatus: "complete"
  Metrics:
    total_pairs_ingested: "25"
    pairs_accepted: "25"
    pairs_rejected: "0"
    layer_0a_pairs: "3"
    layer_0b_pairs: "3"
    layer_0c_pairs: "2"
    layer_0d_pairs: "4"
    layer_1_pairs: "2"
    layer_2_pairs: "9"
    layer_3_pairs: "2"
    predicate_coverage: "5"
    sub_phase_coverage: "4"
    knowledge_edge_pairs: "2"
    chain_integrity: "passed"
    min_confidence: "0.70"
    mean_confidence: "0.91"
    max_confidence: "0.92"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "total_pairs_ingested >= 20"
      Result: "pass (25 >= 20)"

    - Assertion: "pairs_rejected == 0"
      Result: "pass (0 == 0)"

    - Assertion: "knowledge_edge_pairs >= 1"
      Result: "pass (2 >= 1)"

    - Assertion: "chain_integrity == passed"
      Result: "pass"

    - Assertion: "predicate_coverage includes SECURITY and AUTHORIZATION"
      Result: "pass (both present)"

    - Assertion: "layer_2_pairs == 9"
      Result: "pass (9 nine-node pairs for sub-phase 3.4)"

    - Assertion: "min_confidence >= 0.70"
      Result: "pass (0.70 for no-predicate cases; 0.92 for all others)"

  WaveAcceptanceVerdict: "ACCEPTED"
  NextAction: "PROMOTE_TO_WAVE_2_REGISTER"

  [unit result envelope]
    UnitId: "unit-model-trainer-v1"
    UnitRole: "model-trainer"
    BatchId: "O1-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
