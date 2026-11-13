# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-002-001 — "login page is broken" → knowledge_edge

A: "The login page is broken and users cannot get in."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "login" — not in SECURITY lexicon (not jwt, auth, authentication, token, etc.)
  "page" — not in any predicate lexicon
  "broken" — not in any predicate lexicon
  "users" — not in any predicate lexicon
  Result: zero predicates fire → knowledge_edge
  The orchestrator is notified; no sub-phase routing decision is produced.

---

## O1-kedge-002-002 — "login page redirecting" → knowledge_edge

A: "The login page keeps redirecting users after they submit the form."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  Same "login" boundary as 002-001.
  "redirecting", "submit", "form" — none in any predicate lexicon.
  Result: zero predicates → knowledge_edge.

---

## O1-kedge-002-003 — "something on the login screen" → knowledge_edge

A: "Something on the login screen is causing issues after the latest release."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "login screen" — "login" not in SECURITY; "screen" not in any lexicon
  "release", "issues", "latest" — none in any predicate lexicon
  Result: zero predicates → knowledge_edge

---

## O1-kedge-002-004 — Contrast: "authentication screen" → SECURITY fires (NOT knowledge_edge)

A: "The authentication screen is causing issues after the latest release."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "authentication" — IS in the SECURITY lexicon
  This is the critical contrast to 002-003: "login screen" = no predicate;
  "authentication screen" = SECURITY fires.
  Result: SECURITY fires → 3.4 routing decision (not knowledge_edge)
