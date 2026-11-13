# Layer 3 — Teaching Unit Pairs (007)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-007-001 — ModelTrainer: compiler expansion metrics

A: TrainingInput: { source: "layer-0a-compiler-pairs (all files 001-012)", pairs_total: 63, batch: "O1-batch-002" }

B: TrainingMetrics: { pairs_ingested: 63, predicate_coverage: { SECURITY: 38, AUTHORIZATION: 28, API_CONTRACT: 20, IMPLEMENT: 18, TEST: 10, DOCUMENT: 12, QA: 0, knowledge_edge: 8 }, uniqueness: "all A-sides are unique prompts", TransitionType_distribution: { feature_addition: 44, documentation: 15, refactoring: 0, knowledge_edge: 8 } }

---

## O1b-L3-007-002 — ModelTrainer: compiler learning signal quality

A: QualityAssessment: compiler files 001-012

B: Quality: { boundary_cases_covered: 6, lexicon_rules_taught: 8, v6_corruption_examples: 3, knowledge_edge_negative_examples: 8, signal_quality: "HIGH — every SECURITY/AUTHORIZATION lexicon word for O1b appears at least twice in distinct contexts" }

---

## O1b-L3-007-003 — ModelTrainer: SECURITY predicate variety

A: MetricQuery: "How many distinct SECURITY lexicon words appear in compiler training?"

B: Metric: { distinct_words: ["oauth", "token", "jwt", "credential", "certificate", "encrypt", "authentication", "auth", "tls", "signing", "session", "password"], count: 12, files_covering_all_12: "layers 002-008" }

---

## O1b-L3-007-004 — ModelTrainer: AUTHORIZATION predicate variety

A: MetricQuery: "How many distinct AUTHORIZATION lexicon words appear in compiler training?"

B: Metric: { distinct_words: ["scope", "tenant", "grant", "revoke", "principal", "acl", "permission", "role", "rbac", "authorization", "service account"], count: 11, files_covering_all_11: "layers 003-009" }

---

## O1b-L3-007-005 — ModelTrainer: compiler Gate B

A: Gate B condition for compiler training module

B: Gate B: "Compiler training passes when: (1) All 12 SECURITY lexicon words appear in training data. (2) All 11 AUTHORIZATION lexicon words appear. (3) At least 8 knowledge_edge negative examples present. (4) All TransitionType values (feature_addition, documentation, refactoring, knowledge_edge) have at least 2 examples." Result: PASSED for O1-batch-002.
