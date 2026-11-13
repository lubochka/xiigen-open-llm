# Layer 0-B — Router Pairs (Base)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-router-001 — SECURITY+AUTHORIZATION+API_CONTRACT → 3.4+3.3 fan-out

A: Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]
   Reasoning: SECURITY+AUTHORIZATION both map to sub-phase 3.4 — they are deduped into ONE routing decision (not two). API_CONTRACT maps to 3.3. Result: two routing decisions total (3.4, 3.3).

---

## O1b-router-002 — No predicates → knowledge_edge

A: Predicates: [] | TransitionType: knowledge_edge

B: RoutingDecisions: []
   Reasoning: No predicates detected → no sub-phase routing. knowledge_edge transitions do not produce routing decisions. Flow selection: none.

---

## O1b-router-003 — DOCUMENT+API_CONTRACT → 2.1+3.3

A: Predicates: ["DOCUMENT", "API_CONTRACT"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "2.1", flow: "DocumentationFlow21", state: "deterministic_generic" }, { sub_phase: "3.3", flow: "ApiContract33FlowAdapter", state: "deterministic_generic" }]
   Reasoning: DOCUMENT → 2.1; API_CONTRACT → 3.3. Two distinct sub-phases, no deduplication needed. TransitionType documentation because DOCUMENT present without IMPLEMENT.
