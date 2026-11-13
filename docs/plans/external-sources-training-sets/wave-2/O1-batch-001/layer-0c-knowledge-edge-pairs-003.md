# Layer 0-C — Knowledge-Edge Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-003-001 — "users can't log in" → knowledge_edge ("log in" colloquial)

A: "Users can't log in to the system since the last update."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "log in" (two words) — not in SECURITY lexicon; same as "login" — colloquial verb phrase
  "system", "update", "users" — not in any predicate lexicon
  Result: zero predicates → knowledge_edge
  Note: "log in" ≠ "authenticate"; the latter fires SECURITY, the former does not

---

## O1-kedge-003-002 — "can't sign in" → knowledge_edge ("sign in" colloquial)

A: "New users can't sign in with their temporary credentials."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "sign in" — not in SECURITY lexicon
  "temporary" — not in any predicate lexicon
  "credentials" — IS in SECURITY lexicon; this changes the result
  Correction: SECURITY fires on "credentials"
  Revised: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4
  Teaching point: always scan the full prompt before declaring knowledge_edge

---

## O1-kedge-003-003 — "users locked out" → knowledge_edge

A: "Several users have been locked out and can't access their accounts."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "locked out" — not in SECURITY lexicon
  "accounts" — not in any predicate lexicon
  "access" (alone) — not in AUTHORIZATION lexicon (requires "access control" compound)
  Result: zero predicates → knowledge_edge

---

## O1-kedge-003-004 — Contrast: "users can't authenticate" → SECURITY fires (NOT knowledge_edge)

A: "Users can't authenticate since the token expiry was changed."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "authenticate" — fires SECURITY (root "auth-" matches "auth" in SECURITY lexicon)
  "token" — also fires SECURITY
  Result: SECURITY fires → not knowledge_edge; routes to 3.4
  Critical boundary: "log in"/"sign in"/"can't get in" = no predicate;
    "authenticate"/"auth" = SECURITY
