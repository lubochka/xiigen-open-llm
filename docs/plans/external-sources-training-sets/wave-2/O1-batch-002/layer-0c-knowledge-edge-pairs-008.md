# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-008-001 — "permission denied" → knowledge_edge

A: "permission denied when trying to access the reporting module"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "permission denied" is a colloquial error phrase. It is NOT the same as the lexicon term "permission" in a technical requirement context. In this phrasing, it is a UI/OS error message. Zero predicates → knowledge_edge.

---

## O1b-0C-008-002 — "permission" in technical context → AUTHORIZATION fires

A: "define the permission model for the tenant API using RBAC (Role-Based Access Control)"

B: Result: VALID ROUTING | Predicates: ["AUTHORIZATION"]
   Reasoning: "permission" in a technical requirement context fires AUTHORIZATION; "rbac" also fires AUTHORIZATION; "tenant" fires AUTHORIZATION. Valid routing to 3.4. Contrast with "permission denied" error message above.

---

## O1b-0C-008-003 — "not authorized" → knowledge_edge

A: "the service is not authorized to call the payment API"

B: Result: knowledge_edge | Predicates: []
   Reasoning: "not authorized" is a status/error phrase. "authorized" in the sense of "to be authorized" (system status) is colloquial when embedded in this phrasing. However, EDGE NOTE: the word "authorized" is close to "authorize" in the AUTHORIZATION lexicon — in borderline cases, the full phrase context determines the outcome.

---

## O1b-0C-008-004 — "no permission to perform" → knowledge_edge

A: "users have no permission to perform this action"

B: Result: knowledge_edge | Predicates: []
   Reasoning: UI/system error message pattern. Not a technical requirement or implementation statement.

---

## O1b-0C-008-005 — "permission" summary

A: When does "permission" fire AUTHORIZATION vs. knowledge_edge?

B: FIRES AUTHORIZATION: "define permission model", "permission grants", "add permission to role", "permission policy", "user permission in RBAC". DOES NOT FIRE (knowledge_edge): "permission denied", "no permission to do X" (error messages). Rule: technical requirement/design statements fire predicates; colloquial error messages and status phrases do not.
