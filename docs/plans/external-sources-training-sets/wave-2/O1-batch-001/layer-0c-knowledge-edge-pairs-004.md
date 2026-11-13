# Layer 0-C — Knowledge-Edge Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-004-001 — "our auth is slow" → SECURITY fires on "auth" (NOT knowledge_edge)

A: "Our auth middleware is slow and causing timeouts on every request."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "auth" — IS in the SECURITY lexicon (auth is an explicit lexicon entry)
  "slow", "timeouts", "middleware" — not in SECURITY lexicon but auth alone is sufficient
  Result: SECURITY fires → NOT knowledge_edge → routes to 3.4
  CRITICAL: "auth" appearing anywhere in the prompt fires SECURITY. Performance context
    does not suppress the predicate.

---

## O1-kedge-004-002 — "auth service crashing" → SECURITY fires (NOT knowledge_edge)

A: "The auth service is crashing intermittently under load."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "auth" — fires SECURITY
  "crashing", "load" — performance/reliability context; does not suppress SECURITY
  Result: SECURITY fires → not knowledge_edge
  Note: Operational context around a security component does not block predicate firing

---

## O1-kedge-004-003 — "oauth callback slow" → SECURITY fires (NOT knowledge_edge)

A: "The OAuth callback endpoint is taking 3 seconds to respond."
B: KnowledgeEdge: false | Predicates: ["SECURITY", "API_CONTRACT"] | Action: route to 3.4 and 3.3

Explanation:
  "oauth" — fires SECURITY
  "endpoint" — fires API_CONTRACT
  Performance complaint context does not suppress predicates
  Result: SECURITY + API_CONTRACT fire → routes to 3.4 and 3.3 (2 routing decisions)

---

## O1-kedge-004-004 — "it's all broken" → knowledge_edge (no auth/security signal)

A: "The whole thing is broken, nothing works since the deploy."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  No tokens from any predicate lexicon
  "broken", "works", "deploy" — operational terms with no predicate coverage
  Result: zero predicates → knowledge_edge
  Contrast with 004-001: "auth is slow" (auth present) ≠ "nothing works" (no auth)
