# Layer 1 — Transition Orchestrator Pairs (006)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-006-001 — SECURITY+AUTH+API_CONTRACT+TEST → full fan-out

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"], transition_type: "documentation", routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "5.1" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", sub_phase: "3.4", state: "trusted_learned", execution: "first" }, { flow: "ApiContractFlow33", sub_phase: "3.3", state: "deterministic_generic", execution: "parallel_with_3.4" }, { flow: "GenericFallbackFlow", sub_phase: "5.1", state: "probationary_rejected", fallback: true, execution: "after_3.4" }] }

---

## O1b-L1-006-002 — Fan-out execution model: 3.4 ‖ 3.3, then 5.1

A: In SECURITY+API_CONTRACT+TEST orchestration, what is the execution model?

B: Execution model: Phase 1 (parallel): SecurityVerification34Flow (3.4) ‖ ApiContractFlow33 (3.3). Phase 2 (sequential after Phase 1): GenericFallbackFlow (5.1) runs after 3.4 completes to consume security design as test context. Phase 3: ResultTranslatorUnit merges all three result fragments.

---

## O1b-L1-006-003 — Fan-out with four routing decisions

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST"], routing: [{ sub_phase: "3.4" }, { sub_phase: "3.3" }, { sub_phase: "4.1" }, { sub_phase: "5.1" }] }

B: OrchestrationPlan: { flows: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "3.3", state: "deterministic_generic" }, { sub_phase: "4.1", state: "deterministic_generic" }, { sub_phase: "5.1", state: "probationary_rejected", fallback: true }], execution: "3.4 first; 3.3 and 4.1 parallel; 5.1 after 3.4" }

---

## O1b-L1-006-004 — Fan-out result merging

A: How are results merged in a 4-flow fan-out?

B: All flow result fragments are collected by ResultTranslatorUnit. Each fragment is labeled by sub-phase. ArbiterSelectorUnit distributes review requests based on domain. ArbiterUnit reviews each fragment. FormatterUnit produces unified HumanResult covering all domains (security design + API contract + implementation plan + test scaffold).

---

## O1b-L1-006-005 — Maximum fan-out for O1b

A: What is the maximum flow fan-out for O1b?

B: Maximum fan-out: 5 flows (3.4+3.3+4.1+5.1+2.1) from predicates SECURITY+AUTHORIZATION+API_CONTRACT+IMPLEMENT+TEST+DOCUMENT. SECURITY+AUTHORIZATION dedup reduces from 6 predicates to 5 routing decisions → 5 flows. QA does not add a flow. ARCHITECTURE would add one more if present.
