# Layer 0-B — Router Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-003-001 — API_CONTRACT alone → 3.3

A: Predicates: ["API_CONTRACT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]

---

## O1b-0B-003-002 — SECURITY+API_CONTRACT → 3.4+3.3 fan-out

A: Predicates: ["SECURITY", "API_CONTRACT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]
   Reasoning: SECURITY → 3.4; API_CONTRACT → 3.3. Two distinct sub-phases → two routing decisions (fan-out). No dedup applies because 3.4 ≠ 3.3.

---

## O1b-0B-003-003 — AUTHORIZATION+API_CONTRACT → 3.4+3.3 fan-out

A: Predicates: ["AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]

---

## O1b-0B-003-004 — SECURITY+AUTHORIZATION+API_CONTRACT → 3.4+3.3 (dedup within 3.4)

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]
   Reasoning: SECURITY+AUTHORIZATION dedup to one 3.4 decision. API_CONTRACT adds one 3.3 decision. Total: two routing decisions.

---

## O1b-0B-003-005 — API_CONTRACT alone (documentation context)

A: Predicates: ["API_CONTRACT", "DOCUMENT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }, { sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }]
