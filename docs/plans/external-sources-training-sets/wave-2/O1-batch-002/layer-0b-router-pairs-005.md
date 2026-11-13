# Layer 0-B — Router Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-005-001 — DOCUMENT alone → 2.1

A: Predicates: ["DOCUMENT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }]

---

## O1b-0B-005-002 — DOCUMENT+API_CONTRACT → 2.1+3.3

A: Predicates: ["DOCUMENT", "API_CONTRACT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]

---

## O1b-0B-005-003 — DOCUMENT+SECURITY → 2.1+3.4

A: Predicates: ["DOCUMENT", "SECURITY"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }, { sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]

---

## O1b-0B-005-004 — DOCUMENT+AUTHORIZATION → 2.1+3.4

A: Predicates: ["DOCUMENT", "AUTHORIZATION"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }, { sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]

---

## O1b-0B-005-005 — DOCUMENT+SECURITY+API_CONTRACT → 2.1+3.4+3.3

A: Predicates: ["DOCUMENT", "SECURITY", "API_CONTRACT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }, { sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]
