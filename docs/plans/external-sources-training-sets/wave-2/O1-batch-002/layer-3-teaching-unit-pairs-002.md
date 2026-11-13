# Layer 3 — Teaching Unit Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-002-001 — PairParser: ingests compiler files 002-006

A: IngestRequest: { files: ["layer-0a-compiler-pairs-002.md", "layer-0a-compiler-pairs-003.md", "layer-0a-compiler-pairs-004.md", "layer-0a-compiler-pairs-005.md", "layer-0a-compiler-pairs-006.md"], batch: "O1-batch-002" }

B: ParseResult: { pairs_extracted: 25, pair_kinds: { compiler: 25 }, validation: "all pairs have A+B, all B sides have Predicates[] and TransitionType", errors: [] }

---

## O1b-L3-002-002 — PairParser: validates TransitionType in compiler pairs

A: ValidationRule: "all compiler pair B sides must include Predicates[] and TransitionType"

B: ValidationResult: { rule_applied: "TransitionType_present", pairs_checked: 25, passed: 25, failed: 0, note: "All compiler pairs in files 002-006 include TransitionType field" }

---

## O1b-L3-002-003 — PairParser: SECURITY predicate frequency in compiler files 002-006

A: MetricQuery: "SECURITY predicate count in compiler files 002-006"

B: Metric: { SECURITY_count: 19, pairs_with_SECURITY: 19, total_pairs: 25, frequency: "76%" }

---

## O1b-L3-002-004 — PairParser: knowledge_edge count in compiler files 002-006

A: MetricQuery: "knowledge_edge cases in compiler files 002-006"

B: Metric: { knowledge_edge_count: 1, source: "layer-0a-compiler-pairs-011.md, pair 004 (client portal boundary)", total_pairs: 25 }

---

## O1b-L3-002-005 — PairParser: TransitionType distribution in compiler files 002-006

A: MetricQuery: "TransitionType distribution in compiler files 002-006"

B: Distribution: { feature_addition: 18, documentation: 7, refactoring: 0, knowledge_edge: 1, total: 25, note: "Most O1b compiler cases are feature_addition because SECURITY/AUTHORIZATION typically accompany IMPLEMENT" }
