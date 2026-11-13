# Layer 0-D — Flow Factory Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-006-001 — SECURITY+AUTH+API_CONTRACT → two factory selections

A: RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", adapter: "ApiContract33FlowAdapter", state: "deterministic_generic" }]

---

## O1b-0D-006-002 — Factory selection is deterministic by sub_phase

A: Same sub_phase always produces same factory result

B: Rule confirmed: { sub_phase: "3.4" } → always { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" } in O1b. Factory selection is a pure function of sub_phase in this batch.

---

## O1b-0D-006-003 — Two factories run independently

A: FlowSelections: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "3.3", state: "deterministic_generic" }]

B: Execution: { parallel: true, note: "SecurityVerification34Flow and ApiContractFlow33 run concurrently. Each produces its own nine-node pipeline result. Results are merged by ResultTranslatorUnit in the parent orchestration." }

---

## O1b-0D-006-004 — Factory ordering follows predicate canonical order

A: For predicates ["API_CONTRACT", "SECURITY", "AUTHORIZATION"], what order do factories appear?

B: Factory selection order follows sub-phase ordering, not input predicate order. Result: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "3.3", state: "deterministic_generic" }]. 3.4 comes before 3.3 because SECURITY/AUTHORIZATION (predicate order 4-5) takes priority over API_CONTRACT (predicate order 2) in sub-phase routing output.

---

## O1b-0D-006-005 — Two factory selections from three predicates

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] — how many factories selected?

B: TWO factories selected. SECURITY+AUTHORIZATION → one 3.4 factory. API_CONTRACT → one 3.3 factory. Three predicates → two distinct sub-phases → two factory selections. The dedup rule applies to routing decisions AND to factory selections.
