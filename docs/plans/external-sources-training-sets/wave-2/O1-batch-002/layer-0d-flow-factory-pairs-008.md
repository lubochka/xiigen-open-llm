# Layer 0-D — Flow Factory Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## O1b-0D-008-001 — IMPLEMENT+SECURITY → 4.1 generic + 3.4 trusted

A: RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "4.1", adapter: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }]

---

## O1b-0D-008-002 — 3.4 trusted takes design priority in IMPLEMENT+SECURITY

A: When IMPLEMENT and SECURITY both present, which flow runs first?

B: 3.4 (SecurityVerification34Flow) runs first to establish security design requirements. 4.1 (UniversalNineNodeDevelopmentFlow) runs after, consuming the security design output as input constraints. 3.4 output feeds into 4.1 DomainProcessorUnit as security constraints for implementation planning.

---

## O1b-0D-008-003 — IMPLEMENT+AUTHORIZATION → 4.1+3.4

A: RoutingDecisions: [{ sub_phase: "3.4" }, { sub_phase: "4.1" }]

B: FlowSelections: [{ sub_phase: "3.4", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }, { sub_phase: "4.1", adapter: "UniversalNineNodeDevelopmentFlow", state: "deterministic_generic" }]
   Note: Same result whether triggered by IMPLEMENT+SECURITY or IMPLEMENT+AUTHORIZATION.

---

## O1b-0D-008-004 — IMPLEMENT+SECURITY+AUTHORIZATION → 4.1+3.4 (not 4.1+3.4+3.4)

A: Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"]

B: FlowSelections: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "4.1", state: "deterministic_generic" }]
   Rule: SECURITY+AUTHORIZATION dedup to one 3.4 factory. IMPLEMENT adds one 4.1 factory. Total: TWO factory selections, not three.

---

## O1b-0D-008-005 — 4.1 generic adapter name

A: What is the adapter name for sub-phase 4.1?

B: Sub-phase 4.1 adapter: UniversalNineNodeDevelopmentFlow. State: deterministic_generic. This is the standard development flow used for IMPLEMENT-predicate routing. It applies the standard nine-node pipeline without specialization.
