# Layer 3 — Teaching Unit Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## O1b-L3-003-001 — PairParser: ingests router files 002-006

A: IngestRequest: { files: ["layer-0b-router-pairs-002.md", "layer-0b-router-pairs-003.md", "layer-0b-router-pairs-004.md", "layer-0b-router-pairs-005.md", "layer-0b-router-pairs-006.md"], batch: "O1-batch-002" }

B: ParseResult: { pairs_extracted: 25, pair_kinds: { router: 25 }, validation: "all pairs have RoutingDecisions[] output", errors: [] }

---

## O1b-L3-003-002 — PairParser: dedup rule coverage in router files 002-006

A: MetricQuery: "How many pairs demonstrate the SECURITY+AUTHORIZATION dedup rule?"

B: Metric: { dedup_examples: 8, files: ["layer-0b-router-pairs-002.md (pairs 002-005)", "layer-0b-router-pairs-003.md (pair 004)", "layer-0b-router-pairs-006.md (pairs 001, 004)"], note: "Dedup rule is the most frequently taught concept in router files 002-006" }

---

## O1b-L3-003-003 — PairParser: sub-phase coverage in router files 002-006

A: MetricQuery: "Sub-phase coverage in router files 002-006"

B: Coverage: { "3.4": 20, "3.3": 10, "4.1": 6, "5.1": 5, "2.1": 4, knowledge_edge: 5, total_routing_decision_instances: 45 }

---

## O1b-L3-003-004 — PairParser: probationary_rejected routing coverage

A: MetricQuery: "How many router pairs demonstrate probationary_rejected for 5.1?"

B: Metric: { probationary_rejected_pairs: 5, file: "layer-0b-router-pairs-004.md", note: "All 5.1 routing decisions in this batch include probationary_rejected state annotation" }

---

## O1b-L3-003-005 — PairParser: fan-out routing coverage

A: MetricQuery: "How many pairs demonstrate multi-flow fan-out routing?"

B: Metric: { fan_out_pairs: 12, two_flows: 8, three_flows: 3, four_plus_flows: 1, note: "Fan-out coverage progressively builds from 2-flow to maximum fan-out" }
