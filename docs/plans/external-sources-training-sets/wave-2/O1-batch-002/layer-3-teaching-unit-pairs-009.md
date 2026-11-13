# Layer 3 — Teaching Unit Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-009-001 — ModelTrainer: nine-node expansion metrics

A: TrainingInput: { source: "layer-2-nine-node-3.4-pairs (all files 001-012)", pairs_total: 99, batch: "O1-batch-002" }

B: TrainingMetrics: { pairs_ingested: 99, scenarios_covered: 11, node_types_covered: 9, all_nodes_present: true, predicate_combinations: 8, flow_states_represented: ["trusted_learned", "deterministic_generic", "probationary_rejected"] }

---

## O1b-L3-009-002 — ModelTrainer: Node 01 A-side correctness across all nine-node files

A: ValidationMetric: "Node 01 A-side type across all nine-node scenarios"

B: Metric: { scenarios_checked: 11, node_01_correct: 11, raw_string_violations: 0, gate_b: "PASSED — 100% of Node 01 A-sides are SelectedLogicalTransition" }

---

## O1b-L3-009-003 — ModelTrainer: nine-node scenario diversity

A: MetricQuery: "Predicate combination diversity across nine-node scenarios"

B: Combinations: { "SECURITY+AUTHORIZATION+IMPLEMENT": 2, "SECURITY+AUTHORIZATION": 1, "AUTHORIZATION+API_CONTRACT": 2, "SECURITY+API_CONTRACT": 1, "SECURITY+AUTHORIZATION+TEST": 1, "SECURITY+TEST (fallback)": 1, "SECURITY+DOCUMENT": 1, "all-predicates": 1, "base full": 1, total_unique: 9 }

---

## O1b-L3-009-004 — ModelTrainer: HumanResult quality metric

A: MetricQuery: "HumanResult completeness across all nine-node scenarios"

B: Metric: { scenarios_with_complete_HumanResult: 11, required_fields: ["scenario name", "component specs", "security/auth summary"], all_complete: true, note: "FormatterUnit output quality is consistent — no truncated or vague HumanResult instances" }

---

## O1b-L3-009-005 — ModelTrainer: nine-node Gate B

A: Gate B condition for nine-node training module

B: Gate B: "Nine-node training passes when: (1) All 9 node types present in at least 5 scenarios. (2) All Node 01 A-sides are SelectedLogicalTransition. (3) ArbiterVerdict gate_b field present in all Node 08 outputs. (4) At least 1 probationary_rejected scenario covered. (5) At least 1 multi-flow scenario (3+ predicates) covered." Result: PASSED for O1-batch-002.
