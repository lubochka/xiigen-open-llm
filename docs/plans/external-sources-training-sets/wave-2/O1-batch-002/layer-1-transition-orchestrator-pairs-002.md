# Layer 1 — Transition Orchestrator Pairs (002)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-002-001 — SECURITY alone → SecurityVerification34Flow

A: SelectedLogicalTransition: { predicates: ["SECURITY"], transition_type: "feature_addition", routing: [{ sub_phase: "3.4", state: "trusted_learned" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }], execution: "single" }

---

## O1b-L1-002-002 — AUTHORIZATION alone → SecurityVerification34Flow

A: SelectedLogicalTransition: { predicates: ["AUTHORIZATION"], transition_type: "feature_addition", routing: [{ sub_phase: "3.4", state: "trusted_learned" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }], execution: "single" }
   Note: Same flow as SECURITY alone. AUTHORIZATION alone routes to 3.4 with the same trusted_learned adapter.

---

## O1b-L1-002-003 — SECURITY+AUTHORIZATION → SecurityVerification34Flow (one flow)

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION"], transition_type: "feature_addition", routing: [{ sub_phase: "3.4", state: "trusted_learned" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }], execution: "single" }
   Note: SECURITY+AUTHORIZATION dedup → ONE routing decision → ONE flow. Not two flows.

---

## O1b-L1-002-004 — SECURITY with refactor verb → SecurityVerification34Flow, TransitionType: refactoring

A: SelectedLogicalTransition: { predicates: ["SECURITY"], transition_type: "refactoring", routing: [{ sub_phase: "3.4", state: "trusted_learned" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", adapter: "SecurityVerification34FlowAdapter", state: "trusted_learned" }], execution: "single", transition_type_note: "refactoring — nine-node pipeline applies to refactor scope, not new feature" }

---

## O1b-L1-002-005 — AUTHORIZATION alone (documentation context)

A: SelectedLogicalTransition: { predicates: ["AUTHORIZATION", "DOCUMENT"], transition_type: "documentation", routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "2.1", state: "deterministic_generic" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", sub_phase: "3.4", state: "trusted_learned" }, { flow: "DocumentationFlow21", sub_phase: "2.1", state: "deterministic_generic" }], execution: "parallel" }
