# Layer 3 — Teaching Unit Pairs (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-004-001 — PairParser: ingests factory files 002-006

A: IngestRequest: { files: ["layer-0d-flow-factory-pairs-002.md", "layer-0d-flow-factory-pairs-003.md", "layer-0d-flow-factory-pairs-004.md", "layer-0d-flow-factory-pairs-005.md", "layer-0d-flow-factory-pairs-006.md"], batch: "O1-batch-002" }

B: ParseResult: { pairs_extracted: 25, pair_kinds: { flow_factory: 25 }, validation: "all pairs include adapter name and state", errors: [] }

---

## O1b-L3-004-002 — PairParser: adapter name validation

A: ValidationRule: "all factory pair B sides must use canonical adapter names only"

B: ValidationResult: { canonical_adapters_used: ["SecurityVerification34FlowAdapter", "ApiContract33FlowAdapter", "UniversalNineNodeDevelopmentFlow", "DocumentationFlow21", "UnitTestingFlow51", "GenericFallbackFlow"], invented_names_found: 0, result: "PASSED" }

---

## O1b-L3-004-003 — PairParser: trusted_learned state frequency

A: MetricQuery: "How many factory pairs include trusted_learned state?"

B: Metric: { trusted_learned_count: 14, total_pairs: 25, frequency: "56%", note: "trusted_learned for 3.4 is the most common state in factory files 002-006 — reflects O1b's primary sub-phase" }

---

## O1b-L3-004-004 — PairParser: probationary_rejected coverage

A: MetricQuery: "probationary_rejected factory pair count in files 002-006"

B: Metric: { probationary_rejected_count: 6, files: ["layer-0d-flow-factory-pairs-003.md (pairs 001-005)", "layer-0d-flow-factory-pairs-007.md"], fallback_flag_present: "all 6 pairs include fallback: true annotation" }

---

## O1b-L3-004-005 — PairParser: factory idempotency coverage

A: MetricQuery: "How many factory pairs demonstrate idempotency rule?"

B: Metric: { idempotency_pairs: 2, sources: ["layer-0d-flow-factory-pairs-002.md pair 005", "layer-0d-flow-factory-pairs-009.md pair 005"], note: "Idempotency — same routing input always produces same factory output — is a key correctness property" }
