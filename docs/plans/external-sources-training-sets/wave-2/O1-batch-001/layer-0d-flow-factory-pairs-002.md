# Layer 0-D — Flow Factory Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-002-001 — trusted_learned for 3.4 (SecurityVerification34FlowAdapter)

Predicates: ["SECURITY", "AUTHORIZATION"]
Sub-phase: 3.4
Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned | GateBPassed: true

Notes:
  SecurityVerification34FlowAdapter is the learned adapter for sub-phase 3.4 in the O1 scenario.
  TrustState trusted_learned means the adapter has passed Gate B (Gate B: Production Evidence Threshold)
    and is registered as the primary flow adapter for 3.4.
  Any prompt routing to 3.4 will select SecurityVerification34FlowAdapter when TrustState is trusted_learned.

---

## FF-002-002 — SECURITY alone → trusted_learned (3.4)

Predicates: ["SECURITY"]
Sub-phase: 3.4
Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned | GateBPassed: true

Notes:
  A single SECURITY predicate is sufficient to activate SecurityVerification34FlowAdapter.
  The factory selection is identical whether SECURITY alone or SECURITY+AUTHORIZATION fires —
    both resolve to the same adapter with the same TrustState.

---

## FF-002-003 — AUTHORIZATION alone → trusted_learned (3.4)

Predicates: ["AUTHORIZATION"]
Sub-phase: 3.4
Factory: SecurityVerification34FlowAdapter | TrustState: trusted_learned | GateBPassed: true

Notes:
  AUTHORIZATION alone also selects SecurityVerification34FlowAdapter for 3.4.
  This confirms that 3.4 is the "Security and Authorization Model" sub-phase —
    both predicates map to the same learned adapter.

---

## FF-002-004 — API_CONTRACT → deterministic_generic (3.3)

Predicates: ["API_CONTRACT"]
Sub-phase: 3.3
Factory: ApiContract33FlowAdapter | TrustState: deterministic_generic | GateBPassed: false (pending promotion)

Notes:
  3.3 uses the generic nine-node flow adapter for O1. The ApiContract33FlowAdapter is
  registered but in deterministic_generic state — it has not yet achieved trusted_learned
  status for the O1 scenario specifically.
  Gate B for 3.3 promotion is pending production evidence (see MANIFEST.md next actions).

---

## FF-002-005 — SECURITY+API_CONTRACT → two factory selections (one per sub-phase)

Predicates: ["SECURITY", "API_CONTRACT"]
Sub-phase: 3.4 + 3.3
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic

Notes:
  Each routing decision selects its own factory. Fan-out routing → fan-out factory selection.
  The orchestrator creates two execution branches, each with its own factory instance.
