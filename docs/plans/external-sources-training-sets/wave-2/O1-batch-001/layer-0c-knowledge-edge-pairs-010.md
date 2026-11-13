# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-010-001 — knowledge_edge boundary: "authentication" fires vs "login" does not

A: Test 1: "Fix the login flow." | Test 2: "Fix the authentication flow."
B: Test 1 → KnowledgeEdge: true (no predicate) | Test 2 → KnowledgeEdge: false (SECURITY fires)

Boundary table:
  "login" → no predicate → knowledge_edge
  "log in" → no predicate → knowledge_edge
  "sign in" → no predicate → knowledge_edge
  "sign on" → no predicate → knowledge_edge
  "authenticate" → SECURITY (root match) → route to 3.4
  "authentication" → SECURITY → route to 3.4
  "auth" → SECURITY → route to 3.4
  Rule: UI-vocabulary login terms ≠ technical security vocabulary auth terms

---

## O1-kedge-010-002 — knowledge_edge boundary: "access" vs "access control"

A: Test 1: "Users can't access the dashboard." | Test 2: "Review the access control policies."
B: Test 1 → KnowledgeEdge: true ("access" alone not in AUTHORIZATION lexicon)
   Test 2 → KnowledgeEdge: false (AUTHORIZATION fires on "access control")

Boundary:
  "access" alone → no predicate
  "access control" (compound) → AUTHORIZATION fires
  "access denied" → no predicate (colloquial error message)
  "access policies" → borderline; "policies" not in AUTHORIZATION lexicon; "access" alone not enough
    Correct: knowledge_edge (neither alone fires)

---

## O1-kedge-010-003 — knowledge_edge boundary: "password" vs "credential"

A: Test 1: "Fix the password reset flow." | Test 2: "Fix the credential storage service."
B: Test 1 → KnowledgeEdge: true ("password" not in SECURITY lexicon)
   Test 2 → KnowledgeEdge: false (SECURITY fires on "credential")

Boundary:
  "password" → NOT in SECURITY lexicon → no predicate
  "credential" → IS in SECURITY lexicon → SECURITY fires
  "passwords" → NOT in SECURITY lexicon (plural form; "credential" is the token, not "password")

---

## O1-kedge-010-004 — knowledge_edge boundary: "role" fires vs "user group" does not

A: Test 1: "Organize users into groups with different levels of access."
   Test 2: "Assign roles to users based on their tenant membership."
B: Test 1 → KnowledgeEdge: true ("group", "levels", "access" alone — none fire AUTHORIZATION)
   Test 2 → KnowledgeEdge: false (AUTHORIZATION fires on "roles", "tenant")

Boundary:
  "group" → not in AUTHORIZATION lexicon
  "user group" → not in AUTHORIZATION lexicon
  "role" → IS in AUTHORIZATION lexicon → fires AUTHORIZATION
  "tenant" → IS in AUTHORIZATION lexicon → fires AUTHORIZATION
  "levels of access" → "access" alone not in lexicon; does not fire
