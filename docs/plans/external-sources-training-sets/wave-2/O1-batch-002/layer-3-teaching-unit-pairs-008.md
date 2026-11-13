# Layer 3 — Teaching Unit Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-008-001 — ModelTrainer: router expansion metrics

A: TrainingInput: { source: "layer-0b-router-pairs (all files 001-012)", pairs_total: 63, batch: "O1-batch-002" }

B: TrainingMetrics: { pairs_ingested: 63, sub_phase_routing_coverage: { "3.4": 42, "3.3": 22, "4.1": 14, "5.1": 12, "2.1": 8, knowledge_edge: 12 }, dedup_rule_examples: 18, fan_out_examples: 22 }

---

## O1b-L3-008-002 — ModelTrainer: router dedup rule learning signal

A: QualityAssessment: dedup rule teaching in router files

B: Quality: { dedup_examples: 18, files_with_dedup: ["002", "003", "009", "010"], dedup_is_most_taught_rule: true, note: "SECURITY+AUTHORIZATION → one 3.4 decision is the most critical routing concept for O1b; 18 examples provide strong reinforcement" }

---

## O1b-L3-008-003 — ModelTrainer: v6-03 routing corruption coverage

A: MetricQuery: "How many pairs demonstrate v6-03 wrong routing?"

B: Metric: { v6_03_pairs: 5, file: "layer-0b-router-pairs-011.md", cases: ["service account to 4.1", "scope to wrong sub-phase", "SECURITY+AUTH two 3.4 decisions", "tenant to 2.1", "confirmed correct routing"], note: "All three types of v6-03 routing errors are covered with correct/incorrect contrast pairs" }

---

## O1b-L3-008-004 — ModelTrainer: router knowledge_edge coverage

A: MetricQuery: "knowledge_edge coverage in router files"

B: Metric: { knowledge_edge_pairs: 12, files: ["002 (pair 002)", "007 (all 5 pairs)", "011 (pair 001 contrast)"], negative_routing_examples: 12, note: "12 pairs teach that zero predicates → zero routing decisions" }

---

## O1b-L3-008-005 — ModelTrainer: router Gate B

A: Gate B condition for router training module

B: Gate B: "Router training passes when: (1) Dedup rule demonstrated ≥15 times. (2) All 5 sub-phases (3.4, 3.3, 4.1, 5.1, 2.1) appear in routing decisions. (3) All 3 v6-03 corruption types have contrast pairs. (4) knowledge_edge routing (zero decisions) ≥8 examples." Result: PASSED for O1-batch-002.
