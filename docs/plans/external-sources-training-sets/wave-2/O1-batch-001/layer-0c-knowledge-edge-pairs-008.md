# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-008-001 — "access denied error" → knowledge_edge (colloquial, no lexicon match)

A: "Users are seeing access denied errors in the admin panel."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "access denied" — colloquial runtime error; not the same as "access control" (compound lexicon entry)
  "access" alone — NOT in AUTHORIZATION lexicon
  "admin panel" — not in any predicate lexicon
  Result: zero predicates → knowledge_edge

---

## O1-kedge-008-002 — "permission denied" → knowledge_edge (similar to "access denied")

A: "Getting permission denied when trying to update a record."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "permission denied" — colloquial runtime error message
  "permission" IS in the AUTHORIZATION lexicon as a standalone token
  Correction: AUTHORIZATION fires on "permission"
  Revised: KnowledgeEdge: false | Predicates: ["AUTHORIZATION"] | Action: route to 3.4
  Teaching point: "permission" as a standalone word fires AUTHORIZATION even in an error message context.
    Contrast with "access denied" where "access" alone does NOT fire AUTHORIZATION.

---

## O1-kedge-008-003 — "forbidden response" → knowledge_edge (no lexicon match)

A: "The API keeps returning a 403 forbidden response for valid requests."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "403", "forbidden" — HTTP status terms; not in any predicate lexicon
  "api" — fires API_CONTRACT
  "response" — fires API_CONTRACT (request/response schema context)
  Correction: API_CONTRACT fires on "api", "response"
  Revised: KnowledgeEdge: false | Predicates: ["API_CONTRACT"] | Action: route to 3.3
  Teaching point: even in an operational complaint, "api" and "response" fire API_CONTRACT

---

## O1-kedge-008-004 — "unauthorized error" → knowledge_edge (boundary case)

A: "We're getting unauthorized errors from the gateway."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "unauthorized" — close to "authorize" but NOT the exact AUTHORIZATION lexicon token
  AUTHORIZATION lexicon: authorize, tenant, role, permission, access control, multi-tenant,
    rbac, privilege, entitlement, scope, claims
  "authorize" IS in AUTHORIZATION lexicon; "unauthorized" is the adjective form — match depends
    on root matching: if root "authoriz" matches "authorize", then AUTHORIZATION fires
  Teaching point: root-form matching — "unauthorized" root "authoriz-" matches "authorize"
  Revised: KnowledgeEdge: false | Predicates: ["AUTHORIZATION"] | Action: route to 3.4
  Note: This is a boundary case — implementations must decide if root-form matching applies
