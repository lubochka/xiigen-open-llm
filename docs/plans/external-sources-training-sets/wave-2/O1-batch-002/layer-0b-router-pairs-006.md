# Layer 0-B — Router Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-006-001 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST → 3.4+3.3+5.1

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }, { sub_phase: "5.1", flow: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]
   Reasoning: SECURITY+AUTHORIZATION dedup → one 3.4. API_CONTRACT → 3.3. TEST → 5.1. Three routing decisions total.

---

## O1b-0B-006-002 — SECURITY+AUTHORIZATION+API_CONTRACT+DOCUMENT → 3.4+3.3+2.1

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "DOCUMENT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }, { sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }]

---

## O1b-0B-006-003 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST+DOCUMENT → 3.4+3.3+5.1+2.1

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST", "DOCUMENT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "3.3", state: "deterministic_generic" }, { sub_phase: "5.1", state: "probationary_rejected" }, { sub_phase: "2.1", state: "deterministic_generic" }]

---

## O1b-0B-006-004 — SECURITY+AUTHORIZATION+API_CONTRACT+IMPLEMENT+TEST → 3.4+4.1+3.3+5.1

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }, { sub_phase: "3.3", state: "deterministic_generic" }, { sub_phase: "5.1", state: "probationary_rejected" }]

---

## O1b-0B-006-005 — Full O1b predicate set: all six

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST", "DOCUMENT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }, { sub_phase: "3.3", state: "deterministic_generic" }, { sub_phase: "5.1", state: "probationary_rejected" }, { sub_phase: "2.1", state: "deterministic_generic" }]
