# Layer 3 — Teaching Unit Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-005-001 — PairParser: ingests orchestrator files 002-006

A: IngestRequest: { files: ["layer-1-transition-orchestrator-pairs-002.md", "layer-1-transition-orchestrator-pairs-003.md", "layer-1-transition-orchestrator-pairs-004.md", "layer-1-transition-orchestrator-pairs-005.md", "layer-1-transition-orchestrator-pairs-006.md"], batch: "O1-batch-002" }

B: ParseResult: { pairs_extracted: 25, pair_kinds: { orchestration: 25 }, validation: "all pairs include OrchestrationPlan with flows[] and execution model", errors: [] }

---

## O1b-L3-005-002 — PairParser: execution model distribution

A: MetricQuery: "Execution model distribution in orchestrator files 002-006"

B: Distribution: { single: 6, parallel: 8, sequential: 9, none: 2, total: 25, note: "Sequential is most common because 3.4 (security design) typically runs before 4.1 (implementation) and 5.1 (testing)" }

---

## O1b-L3-005-003 — PairParser: dedup rule in orchestration

A: MetricQuery: "How many orchestrator pairs demonstrate the one-flow dedup rule for SECURITY+AUTHORIZATION?"

B: Metric: { dedup_orchestration_pairs: 8, note: "SECURITY+AUTHORIZATION → one SecurityVerification34Flow (not two) is taught in files 002 and 003 extensively" }

---

## O1b-L3-005-004 — PairParser: knowledge_edge orchestration pairs

A: MetricQuery: "Knowledge edge orchestration pairs in files 002-006"

B: Metric: { knowledge_edge_pairs: 2, files: ["file 002 pair 005 (indirect)", "file 005 implied"], flows_produced: 0, note: "knowledge_edge orchestration always produces zero flows" }

---

## O1b-L3-005-005 — PairParser: TransitionType effect pairs

A: MetricQuery: "How many pairs demonstrate TransitionType hierarchy?"

B: Metric: { TransitionType_hierarchy_pairs: 5, file: "layer-1-transition-orchestrator-pairs-009.md", coverage: ["refactoring override", "IMPLEMENT priority", "TEST/DOCUMENT → documentation", "QA → no_contribution", "knowledge_edge"] }
