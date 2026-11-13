# Layer 1 — Transition Orchestrator Pairs (005)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-005-001 — SECURITY+TEST → SecurityVerification34Flow → UnitTestingFlow51 (rejected)

A: SelectedLogicalTransition: { predicates: ["SECURITY", "TEST"], transition_type: "documentation", routing: [{ sub_phase: "3.4", state: "trusted_learned" }, { sub_phase: "5.1", state: "probationary_rejected" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", sub_phase: "3.4", state: "trusted_learned" }, { flow: "GenericFallbackFlow", sub_phase: "5.1", state: "probationary_rejected", fallback: true }], execution: "sequential", note: "3.4 runs first (security design); 5.1 fallback runs after with generic test scaffold" }

---

## O1b-L1-005-002 — SECURITY before TEST in execution order

A: When SECURITY+TEST are present, why does 3.4 run before 5.1?

B: Security design (3.4) establishes what needs to be tested. The test plan (5.1) is derived from the security design output. Sequential ordering: SecurityVerification34Flow produces security requirements → GenericFallbackFlow (5.1) receives security requirements as context → produces test scaffold targeting those requirements.

---

## O1b-L1-005-003 — TEST fallback does not block 3.4

A: Does probationary_rejected for 5.1 prevent 3.4 from running?

B: NO. SecurityVerification34Flow runs with trusted_learned state regardless of 5.1's state. Only the 5.1 flow is in fallback mode. 3.4 output is unaffected.

---

## O1b-L1-005-004 — AUTHORIZATION+TEST → same pattern

A: SelectedLogicalTransition: { predicates: ["AUTHORIZATION", "TEST"], routing: [{ sub_phase: "3.4" }, { sub_phase: "5.1" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", sub_phase: "3.4" }, { flow: "GenericFallbackFlow", sub_phase: "5.1", fallback: true }], execution: "sequential" }

---

## O1b-L1-005-005 — SECURITY+AUTHORIZATION+TEST → one 3.4 + one 5.1

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "TEST"], routing: [{ sub_phase: "3.4" }, { sub_phase: "5.1" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", sub_phase: "3.4", count: 1 }, { flow: "GenericFallbackFlow", sub_phase: "5.1", count: 1, fallback: true }], execution: "sequential", note: "Two flows total (not three) — SECURITY+AUTHORIZATION dedup maintains one 3.4 flow" }
