# Layer 0-C — Knowledge Edge Pairs (004)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-004-001 — "access denied on dashboard" → knowledge_edge

A: "access denied on the admin dashboard"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "access denied" is a colloquial UI error message. It is NOT in the AUTHORIZATION lexicon. The AUTHORIZATION lexicon contains: authorize, authorization, tenant, permission, role, access-control, scope, privilege, claim, policy, acl, entitlement, rbac, principal, grant, revoke. "access denied" is not on this list.

---

## O1b-0C-004-002 — "you don't have permission" → knowledge_edge

A: "you don't have permission to view this page"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "you don't have permission" is a UI error string displayed to users. "permission" alone without technical context does not fire AUTHORIZATION here — this is a colloquial display message, not a technical requirement statement.
   Note: EDGE CASE — in a technical context ("define the permission model"), "permission" would fire AUTHORIZATION. In this colloquial UI error context, it does not. The key signal is whether the statement is a technical requirement or a user-facing display string.

---

## O1b-0C-004-003 — "forbidden error on API call" → API_CONTRACT fires (NOT knowledge_edge)

A: "getting a 403 forbidden error on the API call to the billing service"

B: Result: VALID ROUTING | Predicates: ["API_CONTRACT"]
   Reasoning: "API call" fires API_CONTRACT ("api" is in the lexicon). "403" is not a predicate itself, but the technical context "API call" is sufficient. Routes to 3.3.

---

## O1b-0C-004-004 — "access denied" alone → knowledge_edge

A: "access denied"

B: Result: knowledge_edge | Predicates: []
   Reasoning: Two-word colloquial phrase. Neither "access" nor "denied" are in any predicate lexicon. Zero predicates.

---

## O1b-0C-004-005 — "authorization error on dashboard" → AUTHORIZATION fires

A: "authorization error on the admin dashboard after tenant migration"

B: Result: VALID ROUTING | Predicates: ["AUTHORIZATION"]
   Reasoning: "authorization" IS in the AUTHORIZATION lexicon (the word itself, not the colloquial phrase "access denied"). "tenant" also fires AUTHORIZATION. This IS routable to 3.4. Contrast with "access denied" above.
