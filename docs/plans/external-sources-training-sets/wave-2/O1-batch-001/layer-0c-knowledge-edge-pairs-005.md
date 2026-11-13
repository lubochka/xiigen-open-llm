# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-005-001 — "fix the authentication bug" → SECURITY fires (not knowledge_edge)

A: "Fix the authentication bug that's causing intermittent failures."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "authentication" — IS in the SECURITY lexicon (explicit lexicon entry)
  "bug", "fix", "failures" — not in any predicate lexicon, but SECURITY already fires
  Result: SECURITY fires → routes to 3.4 → NOT knowledge_edge
  TransitionType: feature_addition (IMPLEMENT fires on "fix" as structural modification)

---

## O1-kedge-005-002 — "fix the auth token expiry" → SECURITY fires (not knowledge_edge)

A: "Fix the auth token expiry calculation — tokens are expiring too early."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "auth" — fires SECURITY
  "token" — also fires SECURITY (redundant, but confirms SECURITY)
  "expiry", "calculation" — not in any lexicon
  Result: SECURITY fires (twice on same prompt, counts once) → routes to 3.4

---

## O1-kedge-005-003 — "fix the tenant data leak" → AUTHORIZATION fires (not knowledge_edge)

A: "Fix the tenant data leak where users can read other tenants' records."
B: KnowledgeEdge: false | Predicates: ["AUTHORIZATION"] | Action: route to 3.4

Explanation:
  "tenant" — fires AUTHORIZATION (twice in prompt, counts once)
  "data leak" — not in SECURITY lexicon (not "encrypt", "secure", "hash", etc.)
  Result: AUTHORIZATION fires → routes to 3.4 → NOT knowledge_edge
  Note: "data leak" as a phrase is a security concern in human terms but is NOT in the
    predicate lexicon; AUTHORIZATION fires only on "tenant"

---

## O1-kedge-005-004 — "fix the login bug" → knowledge_edge (login not in lexicon)

A: "Fix the login bug that's blocking all users from getting in."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "login" — NOT in SECURITY lexicon (boundary rule confirmed again)
  "bug", "blocking", "users" — not in any predicate lexicon
  Result: zero predicates → knowledge_edge
  Contrast with 005-001: "fix the authentication bug" (authentication fires SECURITY)
    vs "fix the login bug" (login fires nothing)
