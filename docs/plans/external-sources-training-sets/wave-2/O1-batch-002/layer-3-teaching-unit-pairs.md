# Layer 3 — Teaching Unit Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-001 — PairParserUnit: parses base layer files

A: IngestRequest: { files: ["layer-0a-compiler-pairs.md", "layer-0b-router-pairs.md", "layer-0c-knowledge-edge-pairs.md", "layer-0d-flow-factory-pairs.md", "layer-1-transition-orchestrator-pairs.md", "layer-2-nine-node-3.4-pairs.md"], batch: "O1-batch-002" }

B: ParseResult: { pairs_extracted: 29, pair_kinds: { compiler: 3, router: 3, knowledge_edge: 2, flow_factory: 4, orchestration: 2, nine_node: 9, teaching_unit: 2, held_out_corruption: 6 }, validation: "all pairs have A+B structure, all Node 01 A-sides are SelectedLogicalTransition", errors: [] }
   Unit: PairParserUnit — reads each file, extracts A/B pairs, validates structure, counts by kind.

---

## O1b-L3-002 — ModelTrainerUnit: produces training metrics

A: ParseResult: { pairs_extracted: 29, batch: "O1-batch-002", pair_kinds: { compiler: 3, router: 3, knowledge_edge: 2, flow_factory: 4, orchestration: 2, nine_node: 9, teaching_unit: 2, held_out_corruption: 6 } }

B: TrainingMetrics: { batch: "O1-batch-002", pairs_ingested: 29, predicate_coverage: { SECURITY: 18, AUTHORIZATION: 14, API_CONTRACT: 10, IMPLEMENT: 8, TEST: 4, DOCUMENT: 5, QA: 1, knowledge_edge: 5 }, sub_phase_coverage: { "3.4": 22, "3.3": 8, "2.1": 3, "4.1": 4, "5.1": 3 }, v6_corruption_coverage: { "v6-01": 1, "v6-02": 1, "v6-03": 1 }, gate_b: "PASSED — all predicate types covered, all sub-phases represented, all v6 corruption types present" }
   Unit: ModelTrainerUnit — ingests parsed pairs, computes predicate and sub-phase coverage, outputs training metrics.
