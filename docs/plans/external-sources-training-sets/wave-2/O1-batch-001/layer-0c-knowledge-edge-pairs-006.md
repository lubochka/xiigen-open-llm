# Layer 0-C — Knowledge-Edge Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1-kedge-006-001 — "session expiry issues" → SECURITY fires on "session" (not knowledge_edge)

A: "Session expiry issues are logging users out too frequently."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "session" — IS in the SECURITY lexicon (explicit entry)
  "expiry", "logging out", "frequently" — not in lexicon but SECURITY fires on "session"
  Result: SECURITY fires → routes to 3.4 → NOT knowledge_edge
  Note: "session" fires SECURITY regardless of surrounding context (complaints, performance, etc.)

---

## O1-kedge-006-002 — "session management" → SECURITY fires (not knowledge_edge)

A: "We need to overhaul the session management service to improve security."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "session" — fires SECURITY
  "security" — also fires SECURITY
  Result: SECURITY fires (confirmed twice) → routes to 3.4 → NOT knowledge_edge
  TransitionType: feature_addition (IMPLEMENT fires on "overhaul")

---

## O1-kedge-006-003 — "user sessions timing out" → SECURITY fires on "sessions" (not knowledge_edge)

A: "User sessions are timing out after 5 minutes even with active interactions."
B: KnowledgeEdge: false | Predicates: ["SECURITY"] | Action: route to 3.4

Explanation:
  "sessions" — fires SECURITY (plural form; "session" root matches)
  "timing out", "active", "interactions" — not in any predicate lexicon
  Result: SECURITY fires → routes to 3.4

---

## O1-kedge-006-004 — Contrast: "cookies expiring" → knowledge_edge (cookie not in SECURITY lexicon)

A: "User cookies keep expiring too early after our CDN change."
B: KnowledgeEdge: ambiguous_no_predicate | Action: route to knowledge_edge synthetic flow

Explanation:
  "cookies" — NOT in the SECURITY lexicon ("session" IS; "cookie" is NOT an explicit entry)
  "expiring", "CDN" (Content Delivery Network), "change" — not in any predicate lexicon
  Result: zero predicates → knowledge_edge
  Contrast with 006-001: "session expiry" (session fires SECURITY) ≠ "cookies expiring" (no match)
