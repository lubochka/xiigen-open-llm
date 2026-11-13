# Layer 0-B — Router Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-002-001 — SECURITY alone → 3.4

A: Predicates: ["SECURITY"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]

---

## O1b-0B-002-002 — AUTHORIZATION alone → 3.4

A: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]
   Reasoning: AUTHORIZATION alone still routes to 3.4. Both SECURITY and AUTHORIZATION map to the same sub-phase.

---

## O1b-0B-002-003 — SECURITY+AUTHORIZATION → 3.4 (dedup: one decision, not two)

A: Predicates: ["SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]
   Reasoning: CRITICAL DEDUP RULE — SECURITY and AUTHORIZATION both route to 3.4. They are merged into ONE routing decision. Two predicates, one sub-phase. NOT two separate 3.4 entries.

---

## O1b-0B-002-004 — SECURITY+AUTHORIZATION → one 3.4 (dedup confirmed again)

A: Predicates: ["SECURITY", "AUTHORIZATION", "SECURITY"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]
   Reasoning: Even if SECURITY appears twice (from multiple keywords), the routing deduplicates to exactly one 3.4 decision.

---

## O1b-0B-002-005 — SECURITY+AUTHORIZATION separate then together

A: Predicates: ["SECURITY"] → RoutingDecisions: [{ sub_phase: "3.4" }] | Predicates: ["AUTHORIZATION"] → RoutingDecisions: [{ sub_phase: "3.4" }] | Predicates: ["SECURITY", "AUTHORIZATION"] → RoutingDecisions: [{ sub_phase: "3.4" }]

B: Pattern confirmed: SECURITY→3.4, AUTHORIZATION→3.4, SECURITY+AUTHORIZATION→3.4 (dedup). All three cases produce exactly one 3.4 routing decision.
