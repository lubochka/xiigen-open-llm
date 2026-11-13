# Layer 0-D — Flow Factory Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-008-001 — IMPLEMENT+SECURITY → 4.1 (generic) + 3.4 (trusted); feature_addition TransitionType

Predicates: ["IMPLEMENT", "SECURITY"]
Sub-phases: 4.1, 3.4
TransitionType: feature_addition
Factory 1: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned

Notes:
  IMPLEMENT fires → TransitionType is feature_addition (IMPLEMENT takes priority).
  IMPLEMENT → 4.1 uses the generic implementation flow (not a specialized O1 flow).
  SECURITY → 3.4 uses the fully trusted SecurityVerification34FlowAdapter.
  Orchestrator sequence: 4.1 first (implementation scaffolding), then 3.4 (security verification).

---

## FF-008-002 — IMPLEMENT+AUTHORIZATION → 4.1 + 3.4; feature_addition

Predicates: ["IMPLEMENT", "AUTHORIZATION"]
Sub-phases: 4.1, 3.4
TransitionType: feature_addition
Factory 1: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned

Notes:
  Same pattern as 008-001 but AUTHORIZATION triggers 3.4 instead of SECURITY.
  Factory selection for 3.4 is the same trusted adapter regardless of trigger predicate.

---

## FF-008-003 — IMPLEMENT+SECURITY+AUTHORIZATION → 4.1 + 3.4; feature_addition; two factory selections

Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION"]
Sub-phases: 4.1, 3.4
TransitionType: feature_addition
Factory 1: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned (EvidencePredicates: ["SECURITY", "AUTHORIZATION"])
Routing decisions: 2 (IMPLEMENT → 4.1; SECURITY+AUTHORIZATION deduplicated → 3.4)

---

## FF-008-004 — IMPLEMENT+API_CONTRACT+SECURITY → 4.1 + 3.3 + 3.4; feature_addition

Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
Sub-phases: 4.1, 3.3, 3.4
TransitionType: feature_addition
Factory 1: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
Factory 3: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Routing decisions: 3

---

## FF-008-005 — IMPLEMENT+SECURITY+TEST → 4.1 + 3.4 + 5.1; feature_addition (IMPLEMENT wins TransitionType)

Predicates: ["IMPLEMENT", "SECURITY", "TEST"]
Sub-phases: 4.1, 3.4, 5.1
TransitionType: feature_addition (IMPLEMENT overrides TEST's documentation TransitionType)
Factory 1: ImplementationFlow41 (4.1) | TrustState: deterministic_generic
Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 3: generic nine-node fallback (5.1) | TrustState: probationary_rejected

Notes:
  TransitionType priority: when IMPLEMENT fires alongside TEST, feature_addition wins.
  TEST alone would give documentation; IMPLEMENT+TEST gives feature_addition.
