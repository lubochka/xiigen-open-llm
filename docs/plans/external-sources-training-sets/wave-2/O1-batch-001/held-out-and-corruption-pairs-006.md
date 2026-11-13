# Held-Out and Corruption Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: held_out_and_corruption

---

## Held-out pair 006-H001 — build the permission assignment service

A: "Build the permission assignment service that maps tenant roles to resource-level entitlements."
Expected B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

Lexicon evidence (for evaluator reference only):
  IMPLEMENT: "build"
  AUTHORIZATION: "permission", "tenant", "role", "entitlement"

---

## Held-out pair 006-H002 — build entitlement provisioning

A: "Build the entitlement provisioning workflow for onboarding new tenant administrators."
Expected B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
(not in training set — held out for evaluation)

---

## Corruption 006-C001 — v6-02 "access denied" → model claims SECURITY fires (wrong — this IS the correct no-predicate case; v6-02 error is claiming SECURITY fires)

v6 corrupt: "Users are seeing access denied errors in the admin panel."
  → Predicates: ["SECURITY"] | TransitionType: feature_addition
Correct: Predicates: [] | TransitionType: none | KnowledgeEdge: true
Error type: v6-02 (gap suppression — claiming SECURITY where no predicate exists)
Explanation: "access denied" is a colloquial runtime error message; "access" alone is not
  in any predicate lexicon; the correct result is knowledge_edge with no predicates
  v6-02 error: model fires SECURITY on "access denied" treating it as a security issue

---

## Corruption 006-C002 — v6-02 "permission denied" boundary — permission IS in AUTHORIZATION lexicon

v6 corrupt: "Getting permission denied when trying to update a record."
  → Predicates: [] | KnowledgeEdge: true
Correct: Predicates: ["AUTHORIZATION"] | TransitionType: feature_addition
Error type: v6-02 (gap suppression — failing to catch AUTHORIZATION when "permission" appears)
Explanation: "permission" IS in the AUTHORIZATION lexicon; "permission denied" as a phrase
  contains the lexicon token "permission" — AUTHORIZATION fires
  Contrast with "access denied" where "access" alone is NOT the lexicon token ("access control" is)
