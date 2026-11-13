# Layer 0-B — Router Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: router

---

## O1-router-010-001 — SECURITY+AUTHORIZATION deduplication: one 3.4 decision (not two)

Predicates: ["SECURITY", "AUTHORIZATION"] | Routes to: 3.4
Routing decisions: 1
Deduplication rule: SECURITY and AUTHORIZATION are co-predicates for sub-phase 3.4.
  Even though both predicates fire, the router produces ONE routing decision record
  with EvidencePredicates: ["SECURITY", "AUTHORIZATION"].
  It does NOT produce two separate decision records both pointing to 3.4.
  This deduplication is a key correctness requirement for the O1 router.

---

## O1-router-010-002 — Deduplication with three sub-phases: SECURITY+AUTHORIZATION+API_CONTRACT

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT"] | Routes to: 3.4, 3.3
Routing decisions: 2
Decision 1: SECURITY+AUTHORIZATION → 3.4 (co-predicate, deduplicated)
Decision 2: API_CONTRACT → 3.3 (separate destination)
Result: 3 predicates → 2 routing decisions (not 3)
Key: only same-destination predicates get deduplicated; cross-destination predicates each get their own decision

---

## O1-router-010-003 — Deduplication with four predicates: SECURITY+AUTHORIZATION+API_CONTRACT+TEST

Predicates: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "TEST"] | Routes to: 3.4, 3.3, 5.1
Routing decisions: 3
Decision 1: SECURITY+AUTHORIZATION → 3.4
Decision 2: API_CONTRACT → 3.3
Decision 3: TEST → 5.1
Result: 4 predicates → 3 routing decisions (SECURITY+AUTHORIZATION collapsed)
This is the canonical O1 full-scenario result from layer-0b-router-pairs.md pair 001.

---

## O1-router-010-004 — No deduplication when predicates route to different sub-phases

Predicates: ["API_CONTRACT", "DOCUMENT"] | Routes to: 3.3, 2.1
Routing decisions: 2 (no deduplication — different destinations)
Rule: Deduplication only applies to predicates that share the same destination sub-phase.
  API_CONTRACT → 3.3 and DOCUMENT → 2.1 are different destinations → no deduplication

---

## O1-router-010-005 — Deduplication does not apply to IMPLEMENT and SECURITY (different destinations)

Predicates: ["IMPLEMENT", "SECURITY"] | Routes to: 4.1, 3.4
Routing decisions: 2 (no deduplication — different destinations)
Rule: IMPLEMENT → 4.1 and SECURITY → 3.4 are different destinations.
  Even though both predicates may appear in the same prompt, they are NOT deduplicated.
  Each gets its own routing decision pointing to its own sub-phase.
  Contrast: SECURITY+AUTHORIZATION are deduplicated because they share 3.4.
