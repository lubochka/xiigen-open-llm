# Layer 0-C — Knowledge-Edge Pairs (011)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note

File 011 covers PairParserUnit + ModelTrainerUnit B06 training metrics as a
knowledge-edge meta-teaching item for the expanded batch.

---

## B06-0C-011-001 — PairParserUnit for expanded B06 batch

```
source_id: xiigen-b06-0c-011-001
source_title: XIIGen B06 knowledge-edge — PairParserUnit expanded batch metrics
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-011-001-kedge"
  CompiledText: "PairParser metrics for B06 expanded batch with 002-012 files"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    expanded_batch_parse_projection:
      BatchId: "B06-batch-001"
      new_files_added: 92
      layers_covered:
        "0-A": { files: 11, pairs_per_file: 4, total_pairs: 44 }
        "0-B": { files: 11, pairs_per_file: 3, total_pairs: 33 }
        "0-C": { files: 11, pairs_per_file: 3, total_pairs: 33 }
        "0-D": { files: 11, pairs_per_file: 3, total_pairs: 33 }
        "1":   { files: 11, pairs_per_file: 3, total_pairs: 33 }
        "2":   { files: 11, pairs_per_file: 9, total_pairs: 99 }
        "3":   { files: 11, pairs_per_file: 3, total_pairs: 33 }
        "held_out": { files: 11, pairs_per_file: 4, total_pairs: 44 }
        "cross_layer": { files: 4, pairs_per_file: 5, total_pairs: 20 }
      original_pairs: 31
      new_pairs_total: 372
      grand_total: 403
      predicate_coverage: ["DOCUMENT", "IMPLEMENT", "ARCHITECTURE", "UI"]
      sub_phase_coverage: ["2.1", "2.4", "4.1", "3.1", "1.1"]
      knowledge_edge_pairs: 33
      chain_integrity: "all files chain correctly via source_id cross-references"

---

## B06-0C-011-002 — ModelTrainerUnit: gate B assertions for expanded batch

```
source_id: xiigen-b06-0c-011-002
source_title: XIIGen B06 knowledge-edge — ModelTrainerUnit gate B expanded
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  DocumentId: "B06-0c-011-002-kedge"
  CompiledText: "ModelTrainer gate B assertions for expanded B06 batch"
  CompiledPredicateIds: ["META"]
  SourceStatus: "knowledge_edge: meta-teaching pair"

B:
  OverallVerdict: "KNOWLEDGE_EDGE_TEACHING"
  Metrics:
    gate_b_assertions_expanded:
      - assertion: "total_pairs_ingested >= 20"
        result: "pass (403 >= 20)"
      - assertion: "pairs_rejected == 0"
        result: "pass (0 rejected)"
      - assertion: "knowledge_edge_pairs >= 1"
        result: "pass (33 knowledge-edge pairs)"
      - assertion: "predicate_coverage includes DOCUMENT"
        result: "pass"
      - assertion: "predicate_coverage includes IMPLEMENT"
        result: "pass"
      - assertion: "predicate_coverage includes ARCHITECTURE"
        result: "pass (added in 002-012 files)"
      - assertion: "predicate_coverage includes UI"
        result: "pass (added in 002-012 files)"
      - assertion: "sub_phase_coverage includes 2.1"
        result: "pass"
      - assertion: "sub_phase_coverage includes 2.4"
        result: "pass (stakeholder routing cases)"
      - assertion: "v6_corruption_types_covered == 3"
        result: "pass (v6-01, v6-02, v6-03 all covered)"
      - assertion: "layer_2_scenarios >= 12"
        result: "pass (12 requirements scenarios in layer-2-nine-node-2.1 files)"
      WaveAcceptanceVerdict: "ACCEPTED"
