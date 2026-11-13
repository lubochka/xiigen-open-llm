# Layer 0-C — Knowledge-Edge Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-012-001 — Complete knowledge-edge rule summary for O1

Reference pair — rules for knowledge_edge determination in O1 scenario.

Rule 1 — Zero predicates: If CompiledPredicateIds is empty after compiler processing,
  the router produces exactly one knowledge_edge decision record. No sub-phase routing
  decisions are produced.

Rule 2 — Unroutable transition: If a predicate fires but no sub-phase in the O1 topology
  registry handles it, the predicate produces a knowledge_edge record with
  FailureClassification: "unroutable_transition".

Rule 3 — Partial routing is valid: When some predicates route and others are unroutable,
  the known predicates proceed to normal routing decisions. Only the unroutable predicate
  produces a knowledge_edge record. The two types can coexist in the RoutingDecision[] array.

Rule 4 — Strict lexicon matching: Only exact lexicon tokens (or root matches where specified)
  fire predicates. Domain knowledge about related terms does not override lexicon membership.
  "bearer" is security-related but not in the lexicon → no SECURITY predicate.
  "cookies" are session-related but not in the lexicon → no SECURITY predicate.

---

## O1-kedge-012-002 — Knowledge-edge classification types

Reference pair — FailureClassification values used in O1.

| FailureClassification | Meaning | Example |
|---|---|---|
| no_predicate_match | Zero predicates fired from the compiled prompt | "login page is broken" |
| unroutable_transition | Predicate fired but no sub-phase registered for it | UI predicate in O1 scope |
| null | Not a knowledge edge — routing decision is valid | Any normal routing |

---

## O1-kedge-012-003 — Orchestrator behavior on knowledge_edge

Reference pair — what the orchestrator does when it receives a knowledge_edge result.

When all decisions are knowledge_edge (no_predicate_match):
  The AdaptiveLogicalOrchestrator routes to the knowledge-edge synthetic flow path.
  No production flow is selected. The request is handled as an ambiguous or out-of-scope prompt.

When some decisions are knowledge_edge (partial unroutable):
  The orchestrator proceeds with the valid routing decisions.
  For the knowledge_edge decisions, it either:
    A. Skips those sub-phases (if unroutable_transition and scope is bounded)
    B. Logs the unroutable predicate for later topology expansion
  The final response is assembled from only the successfully routed sub-phases.

---

## O1-kedge-012-004 — v6-02 and knowledge_edge interaction

Reference pair — how v6-02 corruption affects knowledge_edge detection.

v6-02 gap suppression manifests in two directions relative to knowledge_edge:

Direction 1 — False positive knowledge_edge:
  v6-02 corrupt: "tenant role assignment" → incorrectly returns knowledge_edge (gap suppression)
  Correct: AUTHORIZATION fires on "tenant", "role" → routes to 3.4 (not knowledge_edge)
  Error type: model suppresses a valid predicate and wrongly returns knowledge_edge

Direction 2 — False negative knowledge_edge:
  v6-02 corrupt: "login page is broken" → incorrectly fires SECURITY (inverts gap suppression)
  Correct: no predicate fires → knowledge_edge
  Error type: model fires a wrong predicate and suppresses the correct knowledge_edge result

Detection: the knowledge_edge boundary tests in files 002-011 provide training signal
  for both directions of v6-02 gap suppression.
