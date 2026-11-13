# Layer 3 — Teaching Unit Pairs
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Contracts: PairParserUnit, ModelTrainerUnit
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: teaching_unit

---

## Pair B06B1-WDA-L3-001 — PairParserUnit

```
source_id: xiigen-b06-l3-parser-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  WaveId: "wave-2"
  BatchRootPath: "external-sources-training-sets/wave-2/B06-batch-001/"
  Files:
    - "layer-0a-compiler-pairs.md"
    - "layer-0b-router-pairs.md"
    - "layer-0c-knowledge-edge-pairs.md"
    - "layer-0d-flow-factory-pairs.md"
    - "layer-1-transition-orchestrator-pairs.md"
    - "layer-2-nine-node-2.1-pairs.md"
    - "layer-3-teaching-unit-pairs.md"
  ScenarioId: "B06-checkout-business-requirements"
  LicenseState: "first_party"

B:
  BatchId: "B06-batch-001"
  ParseStatus: "ready"
  ParsedPairs:
    - Layer: "0-A"
      PairCount: 3
      ContractSignature: "PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>"
      PairIds: ["B06B1-WDA-compiler-001", "B06B1-WDA-compiler-002", "B06B1-WDA-compiler-003"]

    - Layer: "0-B"
      PairCount: 3
      ContractSignature: "MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>"
      PairIds: ["B06B1-WDA-router-001", "B06B1-WDA-router-002", "B06B1-WDA-router-003"]

    - Layer: "0-C"
      PairCount: 2
      ContractSignature: "AdaptiveLogicalOrchestrator — knowledge-edge result path"
      PairIds: ["B06B1-WDA-kedge-001", "B06B1-WDA-kedge-002"]

    - Layer: "0-D"
      PairCount: 4
      ContractSignature: "ILogicalFlowFactory<TInput> — flow factory selection"
      PairIds: ["B06B1-WDA-factory-001", "B06B1-WDA-factory-002", "B06B1-WDA-factory-003", "B06B1-WDA-factory-004"]

    - Layer: "1"
      PairCount: 2
      ContractSignature: "TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>"
      PairIds: ["B06B1-WDA-T1-001", "B06B1-WDA-T1-002"]

    - Layer: "2"
      PairCount: 9
      ContractSignature: "RequirementsAuthoringFlow21 — Nodes 01-09"
      PairIds: ["B06B1-2.1-N01-001", "B06B1-2.1-N02-001", "B06B1-2.1-N03-001", "B06B1-2.1-N04-001", "B06B1-2.1-N05-001", "B06B1-2.1-N06-001", "B06B1-2.1-N07-001", "B06B1-2.1-N08-001", "B06B1-2.1-N09-001"]

    - Layer: "3"
      PairCount: 2
      ContractSignature: "PairParserUnit + ModelTrainerUnit"
      PairIds: ["B06B1-WDA-L3-001", "B06B1-WDA-L3-002"]

  TotalPairCount: 25
  ParsedPredicateCoverage: ["DOCUMENT", "IMPLEMENT"]
  ParsedSubPhaseCoverage: ["2.1", "4.1"]
  KnowledgeEdgePairsFound: 2
  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-v1"
    UnitRole: "pair-parser"
    BatchId: "B06-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## Pair B06B1-WDA-L3-002 — ModelTrainerUnit

```
source_id: xiigen-b06-l3-trainer-001
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser B — 25 pairs across 7 layers]

B:
  BatchId: "B06-batch-001"
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
    predicate_coverage: "2"
    sub_phase_coverage: "2"
    knowledge_edge_pairs: "2"
    chain_integrity: "passed"
    min_confidence: "0.92"
    mean_confidence: "0.92"
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
    - Assertion: "predicate_coverage includes DOCUMENT"
      Result: "pass (DOCUMENT present)"
    - Assertion: "layer_2_pairs == 9"
      Result: "pass (9 nine-node pairs for sub-phase 2.1)"
    - Assertion: "min_confidence >= 0.70"
      Result: "pass (0.92)"

  WaveAcceptanceVerdict: "ACCEPTED"
  NextAction: "PROMOTE_TO_WAVE_2_REGISTER"

  [unit result envelope]
    UnitId: "unit-model-trainer-v1"
    UnitRole: "model-trainer"
    BatchId: "B06-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
