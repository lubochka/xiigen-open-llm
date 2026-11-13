# Layer 0-D — Flow Factory Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-003-001 — probationary_rejected for 5.1 (UnitTestDesign51FlowAdapter)

Predicates: ["TEST"]
Sub-phase: 5.1
Factory: UnitTestDesign51FlowAdapter | TrustState: probationary_rejected | GateBPassed: false

Notes:
  The specific learned adapter for 5.1 (UnitTestDesign51FlowAdapter) was evaluated and
  rejected — its Gate B assertion (Gate B: minimum 0.85 precision on test case coverage)
  was not met. The adapter remains registered but its TrustState is probationary_rejected.
  When TrustState is probationary_rejected, the factory falls back to the generic nine-node flow.

---

## FF-003-002 — probationary_rejected → generic fallback in use

Predicates: ["TEST"]
Sub-phase: 5.1
Factory: UnitTestDesign51FlowAdapter → generic nine-node fallback | TrustState: probationary_rejected

Fallback mechanism:
  ILogicalFlowFactory<TInput> checks TrustState before constructing the flow.
  When TrustState is probationary_rejected, the factory returns the generic nine-node flow
  rather than the specialized adapter. The specialized adapter is not invoked.
  The FactoryState in the Formatter (Node 09) output will reflect: probationary_rejected.

---

## FF-003-003 — SECURITY+TEST → trusted_learned (3.4) + generic fallback (5.1)

Predicates: ["SECURITY", "TEST"]
Sub-phase: 3.4 + 5.1
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: generic nine-node fallback (5.1) | TrustState: probationary_rejected (UnitTestDesign51FlowAdapter rejected)

Notes:
  Fan-out: two factories selected. One is fully trusted; one falls back to generic.
  The orchestrator handles each factory selection independently.

---

## FF-003-004 — SECURITY+AUTHORIZATION+TEST → trusted_learned + generic fallback

Predicates: ["SECURITY", "AUTHORIZATION", "TEST"]
Sub-phase: 3.4 + 5.1 (two routing decisions)
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: generic nine-node fallback (5.1) | TrustState: probationary_rejected

Notes:
  SECURITY+AUTHORIZATION co-predicate for 3.4 → one factory selection (trusted_learned).
  TEST → 5.1 factory selection (probationary_rejected, generic fallback).
  Two routing decisions → two factory selections.
