# Layer 0-B — Router Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-008-001 — IMPLEMENT alone → 4.1

A: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "4.1", flow: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }]

---

## O1b-0B-008-002 — IMPLEMENT+SECURITY → 4.1+3.4

A: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "4.1", flow: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }]
   Reasoning: SECURITY → 3.4 (design priority per sub-phase 3.4 rules); IMPLEMENT → 4.1. Two routing decisions.

---

## O1b-0B-008-003 — IMPLEMENT+AUTHORIZATION → 4.1+3.4

A: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "4.1", flow: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }]

---

## O1b-0B-008-004 — IMPLEMENT+SECURITY+AUTHORIZATION → 4.1+3.4 (dedup within 3.4)

A: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "4.1", flow: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }]
   Reasoning: SECURITY+AUTHORIZATION dedup to one 3.4. IMPLEMENT adds 4.1. Two decisions total (not three).

---

## O1b-0B-008-005 — IMPLEMENT+API_CONTRACT → 4.1+3.3

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "4.1", flow: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]
