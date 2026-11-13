# Layer 0-D — Flow Factory Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## FF-009-001 — Factory selection ordering matches routing decision ordering

Reference pair — canonical factory selection sequence.

Canonical ordering of factory selections:
  1. ARCHITECTURE sub-phase factory (if ARCHITECTURE predicate fires)
  2. API_CONTRACT → ApiContract33FlowAdapter (3.3)
  3. IMPLEMENT → ImplementationFlow41 (4.1)
  4. SECURITY+AUTHORIZATION → SecurityVerification34FlowAdapter (3.4) [consolidated if both fire]
  5. TEST → generic fallback (5.1) [or UnitTestDesign51FlowAdapter if not rejected]
  6. DOCUMENT → RequirementsAuthoringFlow21 (2.1)
  7. QA → QAGatingFlow73 (7.x) [always last]

Rule: the factory selection sequence is the same as the routing decision sequence.
The orchestrator respects this ordering when instantiating flow branches.

---

## FF-009-002 — Factory selection with QA last

Predicates: ["SECURITY", "QA"]
Sub-phases: 3.4, 7.x
Factory 1: SecurityVerification34FlowAdapter (3.4) | TrustState: trusted_learned (position 1)
Factory 2: QAGatingFlow73 (7.x) | TrustState: deterministic_generic (position 2, always last)

Notes:
  QA factory selection is always appended last, regardless of predicate discovery order.
  If QA appeared first in the prompt, the factory selection still places QAGatingFlow73 last.

---

## FF-009-003 — Factory selection with IMPLEMENT first (canonical position 3)

Predicates: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST"]
Sub-phases: 4.1, 3.4, 5.1
Factory selection order:
  1. ImplementationFlow41 (4.1) — IMPLEMENT at canonical position 3
  2. SecurityVerification34FlowAdapter (3.4) — SECURITY+AUTHORIZATION at canonical position 4
  3. generic nine-node fallback (5.1) — TEST at canonical position 5
Routing decisions: 3

---

## FF-009-004 — Factory selection with API_CONTRACT before IMPLEMENT (canonical position 2 vs 3)

Predicates: ["API_CONTRACT", "IMPLEMENT", "SECURITY"]
Sub-phases: 3.3, 4.1, 3.4
Factory selection order:
  1. ApiContract33FlowAdapter (3.3) — API_CONTRACT at canonical position 2
  2. ImplementationFlow41 (4.1) — IMPLEMENT at canonical position 3
  3. SecurityVerification34FlowAdapter (3.4) — SECURITY at canonical position 4
Routing decisions: 3

Notes:
  API_CONTRACT (3.3) appears before IMPLEMENT (4.1) in canonical ordering.
  This ordering is preserved in both routing decisions and factory selections.

---

## FF-009-005 — Full eight-predicate factory selection sequence

Predicates: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
Factory selection sequence (canonical):
  1. architecture flow (3.x)
  2. ApiContract33FlowAdapter (3.3)
  3. ImplementationFlow41 (4.1)
  4. SecurityVerification34FlowAdapter (3.4) — EvidencePredicates: ["SECURITY", "AUTHORIZATION"]
  5. generic nine-node fallback (5.1) — UnitTestDesign51FlowAdapter rejected
  6. RequirementsAuthoringFlow21 (2.1)
  7. QAGatingFlow73 (7.x) — always last
Total: 7 factory instantiations (8 predicates; SECURITY+AUTHORIZATION consolidated)
