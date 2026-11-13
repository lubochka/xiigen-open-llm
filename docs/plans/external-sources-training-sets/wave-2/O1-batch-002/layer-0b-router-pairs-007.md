# Layer 0-B — Router Pairs (007)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: router

---

## O1b-0B-007-001 — knowledge_edge: "client portal is down"

A: Predicates: [] | TransitionType: knowledge_edge | source: "the client portal is down"

B: RoutingDecisions: []
   Reasoning: No predicates → no routing. knowledge_edge boundary case.

---

## O1b-0B-007-002 — knowledge_edge: "access denied on dashboard"

A: Predicates: [] | TransitionType: knowledge_edge | source: "access denied on the admin dashboard"

B: RoutingDecisions: []
   Reasoning: "access denied" is colloquial — not in AUTHORIZATION lexicon. Zero predicates.

---

## O1b-0B-007-003 — knowledge_edge: colloquial complaint with no keyword

A: Predicates: [] | TransitionType: knowledge_edge | source: "the login button is broken"

B: RoutingDecisions: []
   Reasoning: "login" is explicitly NOT in SECURITY lexicon (boundary rule). No other keyword fires.

---

## O1b-0B-007-004 — knowledge_edge: vague infra complaint

A: Predicates: [] | TransitionType: knowledge_edge | source: "users are getting kicked out randomly"

B: RoutingDecisions: []
   Reasoning: No predicate keywords present. "kicked out" is colloquial. Zero predicates.

---

## O1b-0B-007-005 — knowledge_edge vs. valid routing contrast

A: Case 1 — Predicates: [] | source: "the portal is broken" → RoutingDecisions: []
   Case 2 — Predicates: ["SECURITY"] | source: "the oauth token is broken" → RoutingDecisions: [{ sub_phase: "3.4" }]

B: Contrast: "portal is broken" → knowledge_edge (no lexicon word). "oauth token is broken" → SECURITY fires on "oauth" + "token" → valid 3.4 routing. The presence of even one lexicon keyword prevents knowledge_edge classification.
