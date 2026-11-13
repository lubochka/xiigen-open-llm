# Layer 0-D — Flow Factory Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-002-001 — 3.4 trusted_learned selection

A: RoutingDecision: { sub_phase: "3.4", predicates: ["SECURITY"] }

B: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned", flow: "SecurityVerification34Flow" }

---

## O1b-0D-002-002 — 3.4 trusted_learned from AUTHORIZATION

A: RoutingDecision: { sub_phase: "3.4", predicates: ["AUTHORIZATION"] }

B: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned", flow: "SecurityVerification34Flow" }
   Note: Same adapter and state regardless of whether SECURITY or AUTHORIZATION triggered the 3.4 routing.

---

## O1b-0D-002-003 — 3.3 deterministic_generic selection

A: RoutingDecision: { sub_phase: "3.3", predicates: ["API_CONTRACT"] }

B: FlowSelection: { adapter: "ApiContract33FlowAdapter", state: "deterministic_generic", flow: "ApiContractFlow33" }

---

## O1b-0D-002-004 — 3.4+3.3 dual factory selection

A: RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", adapter: "ApiContract33FlowAdapter", state: "deterministic_generic" }]

---

## O1b-0D-002-005 — Factory always selects by sub_phase, not predicate count

A: RoutingDecision: { sub_phase: "3.4", predicates: ["SECURITY", "AUTHORIZATION", "SECURITY"] }

B: FlowSelection: { adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }
   Rule: Factory selection is based on sub_phase only. The number or repetition of predicates does not change the adapter or state selected.
