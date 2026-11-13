# Layer 0-D — Flow Factory Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-006-001 — SECURITY+AUTHORIZATION+API_CONTRACT → three factory selections

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
Sub-phases: 3.4, 3.3
Routing decisions: 2 (SECURITY+AUTHORIZATION deduplicated into one 3.4 decision)
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic

Notes:
  Two routing decisions → two factory selections.
  The deduplication of SECURITY+AUTHORIZATION happens at routing, not at factory selection.
  Factory 1 receives both SECURITY and AUTHORIZATION in its EvidencePredicates.

---

## FF-006-002 — SECURITY+AUTHORIZATION+API_CONTRACT with full source

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
Source: "Build the tenant API with JWT authentication and OpenAPI contract."
Sub-phases: 3.4, 3.3
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned | EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic | EvidencePredicates: ["API_CONTRACT"]
TransitionType: feature_addition (IMPLEMENT fires on "build")

---

## FF-006-003 — SECURITY+AUTHORIZATION+API_CONTRACT+DOCUMENT → three routing decisions, three factories

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "DOCUMENT"]
Sub-phases: 3.4, 3.3, 2.1
Routing decisions: 3
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned
Factory 2: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
Factory 3: RequirementsAuthoringFlow21 (2.1) | TrustState: deterministic_generic

Notes:
  Four predicates → three routing decisions (SECURITY+AUTHORIZATION deduplicated) → three factories.

---

## FF-006-004 — Factory selection ordering matches routing decision ordering

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"]
Factory selection order:
  1. SecurityVerification34FlowAdapter (3.4) — selected first (SECURITY before API_CONTRACT in canonical order)
  2. ApiContract33FlowAdapter (3.3) — selected second

Rule: factory selections are produced in the same canonical ordering as routing decisions:
  ARCHITECTURE → API_CONTRACT → IMPLEMENT → SECURITY → AUTHORIZATION → TEST ‖ DOCUMENT → QA
  In practice, after deduplication, SECURITY+AUTHORIZATION share one factory selection at
  the SECURITY position. API_CONTRACT appears before SECURITY in canonical order, so
  3.3 factory would appear before 3.4 factory if strict canonical order is applied.
  Note: implementation may vary — the ordering rule governs routing output; factory instantiation
  order depends on orchestrator implementation.

---

## FF-006-005 — API_CONTRACT+SECURITY+AUTHORIZATION canonical ordering of factory output

Predicates: ["API_CONTRACT", "SECURITY", "AUTHORIZATION"]
Routing decisions (canonical order): [3.3 (API_CONTRACT), 3.4 (SECURITY+AUTHORIZATION)]
Factory 1: ApiContract33FlowAdapter (3.3) | TrustState: deterministic_generic
Factory 2: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned

Notes:
  Per canonical ordering, API_CONTRACT comes before SECURITY. So 3.3 routing decision appears
  before the 3.4 routing decision. Factory selections follow the same sequence.
  This is the only case where the 3.3 factory appears before the 3.4 factory in the output.
