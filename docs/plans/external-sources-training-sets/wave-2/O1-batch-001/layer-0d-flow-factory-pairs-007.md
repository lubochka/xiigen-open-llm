# Layer 0-D — Flow Factory Pairs (007)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-007-001 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST → three sub-phases, three factories

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"]
Sub-phases: 3.4, 3.3, 5.1
Routing decisions: 3 (SECURITY+AUTHORIZATION deduplicated into one 3.4 decision)
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
Factory 3: generic nine-node fallback (5.1) | TrustState: probationary_rejected

Notes:
  The canonical O1 full-scenario factory selection.
  Source: "We need to add multi-tenant JWT authentication to the API and make sure tenant isolation is tested."
  Four predicates → three routing decisions → three factories.

---

## FF-007-002 — SECURITY+TEST → two factories (one trusted, one rejected/generic)

Predicates: ["SECURITY", "TEST"]
Sub-phases: 3.4, 5.1
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: generic nine-node fallback (5.1) | TrustState: probationary_rejected

Notes:
  Simpler two-predicate case. One specialized trusted factory; one generic fallback.

---

## FF-007-003 — AUTHORIZATION+TEST → two factories

Predicates: ["AUTHORIZATION", "TEST"]
Sub-phases: 3.4, 5.1
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: generic nine-node fallback (5.1) | TrustState: probationary_rejected

Notes:
  AUTHORIZATION alone selects SecurityVerification34FlowAdapter for 3.4.
  TEST selects the generic fallback for 5.1.

---

## FF-007-004 — API_CONTRACT+TEST → two factories (both non-trusted)

Predicates: ["API_CONTRACT", "TEST"]
Sub-phases: 3.3, 5.1
Factory 1: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
Factory 2: generic nine-node fallback (5.1) | TrustState: probationary_rejected

Notes:
  This combination has no trusted_learned factory. Both factories are generic/fallback.
  Both still produce valid results — neither is in an error state.

---

## FF-007-005 — SECURITY+AUTHORIZATION+API_CONTRACT+TEST+DOCUMENT → four factories

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST", "DOCUMENT"]
Sub-phases: 3.4, 3.3, 5.1, 2.1
Routing decisions: 3 standard + 1 for DOCUMENT = 4 total (SECURITY+AUTHORIZATION still consolidated to one 3.4)
Correction: routing decisions count = 4 (not 3 as initially stated)
  Decision 1: SECURITY+AUTHORIZATION → 3.4
  Decision 2: API_CONTRACT → 3.3
  Decision 3: TEST → 5.1
  Decision 4: DOCUMENT → 2.1
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
Factory 3: generic nine-node fallback (5.1) | TrustState: probationary_rejected
Factory 4: RequirementsAuthoringFlow21 (2.1) | TrustState: deterministic_generic
