# Layer 1 — Transition Orchestrator Pairs (003)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: orchestration

---

## O1b-L1-003-001 — SECURITY+AUTHORIZATION → SecurityVerification34Flow (single flow)

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION"], transition_type: "feature_addition", routing: [{ sub_phase: "3.4" }] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow" }], execution: "single", note: "SECURITY+AUTHORIZATION dedup produces one flow, not two" }

---

## O1b-L1-003-002 — SECURITY+AUTHORIZATION dedup: orchestrator perspective

A: Why does SECURITY+AUTHORIZATION produce only one flow in orchestration?

B: Because routing dedup collapses SECURITY and AUTHORIZATION to one 3.4 routing decision. The factory selects one SecurityVerification34FlowAdapter. The orchestrator receives one FlowSelection → spawns one flow. If two flows were spawned for 3.4, they would produce duplicate and conflicting results. The dedup is essential for correctness.

---

## O1b-L1-003-003 — SECURITY+AUTHORIZATION: nine-node pipeline context

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION"], routing: [{ sub_phase: "3.4" }] } — which nine-node pipeline runs?

B: SecurityVerification34Flow executes the nine-node pipeline with both SECURITY and AUTHORIZATION domains as input. DomainSplitterUnit splits into both SECURITY and AUTHORIZATION domain packets. Both domains flow through the same nine-node pipeline. The pipeline is shared, not duplicated.

---

## O1b-L1-003-004 — SECURITY+AUTHORIZATION+SECURITY: still one flow

A: SelectedLogicalTransition: { predicates: ["SECURITY", "AUTHORIZATION", "SECURITY"] }

B: OrchestrationPlan: { flows: [{ flow: "SecurityVerification34Flow", count: 1 }], execution: "single" }
   Rule: No matter how many SECURITY/AUTHORIZATION predicates appear, exactly one SecurityVerification34Flow runs.

---

## O1b-L1-003-005 — Orchestrator dedup axiom

A: O1b orchestration dedup axiom

B: AXIOM: The number of flows spawned = the number of distinct sub-phases in routing decisions. SECURITY+AUTHORIZATION share sub-phase 3.4 → one flow. SECURITY+API_CONTRACT have distinct sub-phases (3.4, 3.3) → two flows. The orchestrator never spawns two flows for the same sub-phase.
