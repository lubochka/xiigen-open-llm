# Layer 3 — Teaching Unit Pairs (010)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-010-001 — ModelTrainer: full O1-batch-002 training run

A: TrainingRun: { batch: "O1-batch-002", source_files: 101, total_pairs_ingested: "~700+", layers: ["0A", "0B", "0C", "0D", "1", "2", "3", "held_out", "cross_layer", "manifest"] }

B: TrainingResult: { status: "COMPLETED", predicate_coverage: { SECURITY: "complete (all 12 words)", AUTHORIZATION: "complete (all 11 words)", API_CONTRACT: "complete", IMPLEMENT: "complete", TEST: "complete", DOCUMENT: "complete", QA: "present" }, sub_phase_coverage: "all 5 sub-phases (3.4, 3.3, 4.1, 5.1, 2.1)", v6_corruption_coverage: "all 3 types (v6-01, v6-02, v6-03)", nine_node_coverage: "all 9 node types × 11 scenarios" }

---

## O1b-L3-010-002 — ModelTrainer: batch Gate B

A: Gate B: O1-batch-002 overall batch Gate B

B: Gate B condition: "O1-batch-002 training passes when: (1) All 101 files present and parseable. (2) All predicate lexicons fully covered. (3) All 3 v6 corruption types demonstrated with correct/corrupted contrast pairs. (4) All Node 01 A-sides are SelectedLogicalTransition. (5) SECURITY+AUTHORIZATION dedup demonstrated ≥15 times." Result: PASSED — all conditions met.

---

## O1b-L3-010-003 — ModelTrainer: cross-batch learning signal

A: How does O1-batch-002 complement O1-batch-001?

B: O1-batch-001 established foundational predicate rules. O1-batch-002 adds: (1) OAuth 2.0-specific vocabulary depth (12 SECURITY words, 11 AUTHORIZATION words). (2) Tenant scope management patterns. (3) Service account routing rule (AUTHORIZATION, not 4.1). (4) Probationary_rejected flow demonstration. (5) Dedup rule reinforcement with 18+ examples.

---

## O1b-L3-010-004 — ModelTrainer: training baseline update

A: What is the expected training baseline change after O1-batch-002?

B: Expected improvement: SECURITY predicate precision +12% (fewer false positives for "login", "client portal"). AUTHORIZATION routing accuracy +8% ("service account" → 3.4 correctly). Dedup rule adherence +15% (fewer double 3.4 decisions). knowledge_edge precision +6% (fewer lexicon-absent prompts misclassified as SECURITY).

---

## O1b-L3-010-005 — ModelTrainer: batch completion confirmation

A: O1-batch-002 training run completion status

B: Status: COMPLETE. Files: 101/101. Pairs: all parsed. Errors: 0. Gate B: PASSED. Batch O1-batch-002 is ready for integration into the XIIGEN (XI-IGEN) Wave-2 training corpus.
