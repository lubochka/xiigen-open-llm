# Layer 3 — Teaching Unit Pairs (011)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## Method note

File 011 covers PairParserUnit and ModelTrainerUnit training metrics for the expanded B06 batch.

---

## B06-L3-011-001 — PairParserUnit batch metrics

```
source_id: xiigen-b06-l3-011-001
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "PairParserUnit metrics for expanded B06 batch (101 total files)"
  SourceFiles: ["layer-0c-knowledge-edge-pairs-011.md"]

B:
  TeachingUnit: "PAIR-PARSER-METRICS-001"
  Content:
    title: "PairParserUnit Batch Parsing Metrics — B06-batch-001 (Expanded)"
    batch_id: "B06-batch-001"
    total_files: 101
    file_layers:
      "Layer 0-A (compiler)": 12
      "Layer 0-B (router)": 12
      "Layer 0-C (knowledge-edge)": 12
      "Layer 0-D (flow-factory)": 12
      "Layer 1 (transition-orchestrator)": 12
      "Layer 2 (nine-node 2.1)": 12
      "Layer 3 (teaching-unit)": 12
      "Held-out and corruption": 12
      "Cross-layer synthesis": 4
      "MANIFEST.md": 1
    pair_counts:
      compiler_pairs: 36
      router_pairs: 36
      knowledge_edge_pairs: 36
      factory_pairs: 36
      transition_pairs: 36
      nine_node_pairs: 44
      teaching_units: 33
      held_out_pairs: 12
      corruption_pairs: 12
      synthesis_traces: 4
      total_production_evidence_pairs: 285
    parser_health:
      parse_errors: 0
      malformed_pairs: 0
      missing_source_ids: 0
      chain_integrity_violations: 0

---

## B06-L3-011-002 — ModelTrainerUnit training metrics

```
source_id: xiigen-b06-l3-011-002
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

A:
  BatchId: "B06-batch-001"
  TeachingFocus: "ModelTrainerUnit metrics and promotion criteria"

B:
  TeachingUnit: "MODEL-TRAINER-METRICS-001"
  Content:
    title: "ModelTrainerUnit Training Metrics — B06-batch-001 (Expanded)"
    training_coverage:
      document_lexicon_coverage: "17/17 terms covered with firing examples"
      non_term_coverage: "8 non-firing terms documented"
      predicate_combinations_covered: 7
      sub_phase_combinations_covered: 5
      transition_types_covered: 2
      trust_states_covered: 4
      v6_corruption_types_covered: 3
      nine_node_scenarios_covered: 12
    promotion_criteria:
      min_production_evidence_pairs: 25
      actual_production_evidence_pairs: 285
      min_nine_node_coverage: "9/9 nodes per scenario"
      min_confidence: 0.70
      actual_min_confidence: 0.82
    promotion_verdict: "ELIGIBLE — all Gate B thresholds exceeded"
    gate_b_comparisons:
      "production_evidence_pairs: 285 >= 25": "PASS"
      "confidence: 0.82 >= 0.70": "PASS"
      "lexicon_coverage: 17/17 = 100%": "PASS"
      "v6_types_covered: 3/3 = 100%": "PASS"

---

## B06-L3-011-003 — Trust state progression for B06-batch-001

```
source_id: xiigen-b06-l3-011-003
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
pair_kind: teaching_unit
```

B:
  TeachingUnit: "TRUST-STATE-PROGRESSION-B06-001"
  Content:
    title: "Trust State Progression for B06-batch-001 Expanded Batch"
    initial_state: "deterministic_generic (original 25-pair batch)"
    after_expansion:
      state: "probationary_accepted"
      reason: "285 evidence pairs; all Gate B thresholds met; eligible for promotion"
    promotion_to_trusted_learned:
      condition: "Passes live evaluation on 3 held-out requests without v6 corruption"
      held_out_requests:
        - "transition-B06-held-out-001 through transition-B06-held-out-012"
      evaluation_criteria: "All three v6 types must be absent from outputs"
    post_promotion_state: "trusted_learned — RequirementsAuthoringFlow21 selected by default for DOCUMENT+2.1"
    regression_trigger: "Any v6 corruption in production demotes to probationary_accepted for re-evaluation"
