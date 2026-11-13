# Layer 0-D — Flow Factory Pairs (007)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-007-001 — SECURITY+AUTH+API_CONTRACT+TEST → three sub-phases

A: RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "5.1" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", adapter: "ApiContract33FlowAdapter", state: "deterministic_generic" }, { sub_phase: "5.1", adapter: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]

---

## O1b-0D-007-002 — Three factories: execution model

A: FlowSelections: [3.4, 3.3, 5.1]

B: Execution: { "3.4": "run with trusted_learned adapter", "3.3": "run with generic adapter", "5.1": "fallback to generic (probationary_rejected)", execution_model: "parallel for 3.4 and 3.3; 5.1 in fallback mode runs sequentially after", merge: "ResultTranslatorUnit merges all three result fragments" }

---

## O1b-0D-007-003 — Factory count with all major predicates

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST", "DOCUMENT"]

B: FlowSelections: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "3.3", state: "deterministic_generic" }, { sub_phase: "4.1", state: "deterministic_generic" }, { sub_phase: "5.1", state: "probationary_rejected" }, { sub_phase: "2.1", state: "deterministic_generic" }]
   Count: 5 factory selections from 6 predicates (SECURITY+AUTHORIZATION dedup reduces one).

---

## O1b-0D-007-004 — Factory count rule

A: Factory count rule for O1b

B: Factory selections count = routing decisions count = distinct sub-phase targets count. Each predicate maps to one sub-phase. Predicates sharing a sub-phase produce one factory selection for that sub-phase. SECURITY and AUTHORIZATION share 3.4 → one factory. All others are distinct.

---

## O1b-0D-007-005 — QA does not produce a factory selection

A: Predicates: ["SECURITY", "QA"] — how many factory selections?

B: ONE factory selection: { sub_phase: "3.4", state: "trusted_learned" }. QA does not map to a sub-phase and does not produce a factory selection. QA is the absolute last predicate by canonical ordering but does not instantiate a flow.
