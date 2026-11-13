# Layer 0-B — Router Pairs (009)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-009-001 — Routing ordering rule: SECURITY before AUTHORIZATION in output

A: Predicates: ["AUTHORIZATION", "SECURITY"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }]
   Ordering rule: Even if AUTHORIZATION is listed first in input, the canonical predicate order is SECURITY → AUTHORIZATION. Both dedup to one 3.4 decision regardless of input order.

---

## O1b-0B-009-002 — Deduplication rule: same sub-phase from different predicates

A: Predicates: ["SECURITY", "AUTHORIZATION", "SECURITY"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }]
   Rule: Duplicate sub-phase entries are always collapsed to one. Three predicates pointing to 3.4 → one 3.4 routing decision.

---

## O1b-0B-009-003 — Routing precedence: 3.4 takes design priority over 4.1 when combined

A: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]
   Precedence rule: When SECURITY or AUTHORIZATION combined with IMPLEMENT, 3.4 (SecurityVerification34Flow) takes design priority. The nine-node flow for 3.4 runs with trusted_learned specialization.

---

## O1b-0B-009-004 — Routing: QA does not add a routing decision

A: Predicates: ["SECURITY", "QA"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }]
   Rule: QA alone does not add a routing decision. QA is the absolute last predicate by ordering but does not map to an independent sub-phase in this batch.

---

## O1b-0B-009-005 — Routing: ARCHITECTURE adds a routing decision

A: Predicates: ["ARCHITECTURE", "SECURITY"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "architecture", flow: "ArchitectureFlow", state: "deterministic_generic" }]
   Note: ARCHITECTURE predicate routes to its own sub-phase node. In O1b context, SECURITY+ARCHITECTURE produces two routing decisions.
