# Layer 0-B — Router Pairs (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-004-001 — TEST alone → 5.1

A: Predicates: ["TEST"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "5.1", flow: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]
   Reasoning: TEST → 5.1. Sub-phase 5.1 is probationary_rejected, so fallback generic flow is used.

---

## O1b-0B-004-002 — SECURITY+TEST → 3.4+5.1 fan-out

A: Predicates: ["SECURITY", "TEST"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "5.1", flow: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]
   Reasoning: SECURITY → 3.4 (trusted_learned); TEST → 5.1 (probationary_rejected, fallback). Fan-out to both sub-phases.

---

## O1b-0B-004-003 — AUTHORIZATION+TEST → 3.4+5.1 fan-out

A: Predicates: ["AUTHORIZATION", "TEST"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "5.1", flow: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]

---

## O1b-0B-004-004 — SECURITY+AUTHORIZATION+TEST → 3.4+5.1

A: Predicates: ["SECURITY", "AUTHORIZATION", "TEST"] | TransitionType: documentation

B: RoutingDecisions: [{ sub_phase: "3.4", flow: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "5.1", flow: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]
   Reasoning: SECURITY+AUTHORIZATION dedup to one 3.4. TEST adds 5.1. Two routing decisions.

---

## O1b-0B-004-005 — TEST with no SECURITY/AUTHORIZATION

A: Predicates: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition

B: RoutingDecisions: [{ sub_phase: "4.1", flow: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }, { sub_phase: "5.1", flow: "UnitTestingFlow51", state: "probationary_rejected", fallback: true }]
