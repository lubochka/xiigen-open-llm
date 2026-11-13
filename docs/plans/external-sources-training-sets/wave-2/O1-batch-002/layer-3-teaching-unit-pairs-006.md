# Layer 3 — Teaching Unit Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-006-001 — PairParser: ingests nine-node files 002-006

A: IngestRequest: { files: ["layer-2-nine-node-3.4-pairs-002.md", "layer-2-nine-node-3.4-pairs-003.md", "layer-2-nine-node-3.4-pairs-004.md", "layer-2-nine-node-3.4-pairs-005.md", "layer-2-nine-node-3.4-pairs-006.md"], batch: "O1-batch-002" }

B: ParseResult: { pairs_extracted: 45, pair_kinds: { nine_node: 45 }, scenarios: 5, validation: "all Node 01 A-sides are SelectedLogicalTransition (not raw strings)", errors: [] }

---

## O1b-L3-006-002 — PairParser: Node 01 A-side validation

A: ValidationRule: "Node 01 A-side must be SelectedLogicalTransition"

B: ValidationResult: { pairs_checked: 5, node01_correct: 5, raw_string_violations: 0, result: "PASSED — all Node 01 A-sides use SelectedLogicalTransition structure" }

---

## O1b-L3-006-003 — PairParser: scenario domain coverage in nine-node files 002-006

A: MetricQuery: "Domain coverage across nine-node scenarios 002-006"

B: Coverage: { scenarios_covered: ["ClientCredentialsFlow", "TenantScopeEnforcement", "ServiceAccountSetup", "TokenIntrospectionEndpoint", "ScopeManagementTests"], domains_represented: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"], missing_from_002_006: "IMPLEMENT-only, DOCUMENT, refactoring" }

---

## O1b-L3-006-004 — PairParser: ArbiterVerdict gate_b coverage

A: MetricQuery: "How many ArbiterUnit verdicts include gate_b field?"

B: Metric: { verdicts_with_gate_b: 5, file_coverage: "all Node 08 pairs in files 002-006", gate_b_values: ["PASSED", "CONDITIONAL (one in 006)"], note: "gate_b field is required in all ArbiterVerdict outputs" }

---

## O1b-L3-006-005 — PairParser: FormatterUnit output format consistency

A: MetricQuery: "Are all Node 09 HumanResult outputs in consistent format?"

B: Metric: { pairs_checked: 5, format_consistent: 5, format: "Scenario name + components + security summary", errors: [], note: "All HumanResult outputs follow: [ScenarioName]: [component list]. [security/auth summary]." }
