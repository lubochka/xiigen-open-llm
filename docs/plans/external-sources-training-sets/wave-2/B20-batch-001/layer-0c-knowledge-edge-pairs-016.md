# Layer 0-C — Knowledge Edge Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E016-001 — GAP: "tax compliance check" does not fire TEST or SECURITY

A:
  user_request: "Do a tax compliance check on the TaxCalculationService output for EU orders."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "do a" — generic action phrase; no predicate
    "compliance check" — "check" is NOT TEST; "compliance" alone is NOT SECURITY; both are gap
    Result: zero predicates
  Teaching: "compliance check" is gap. "compliance" alone does not fire SECURITY (SECURITY fires on: security, auth, JWT, token, encryption, authentication, authorization, vulnerability). "check" does not fire TEST.

---

## E016-002 — EDGE: "tax" alone fires no predicate

A:
  user_request: "Tax calculation needs to support Canadian GST (Goods and Services Tax) from next quarter."

B:
  CompiledPredicateIds: []
  TransitionType: unknown
  SourceStatus: knowledge_edge
  Predicate evidence:
    "needs to support" — informal requirement phrasing; no predicate
    "tax calculation" — domain noun phrase; no predicate
    "GST" — domain tax acronym; no predicate
    "next quarter" — temporal qualifier; no predicate
    Result: zero predicates → knowledge_edge
  Teaching: Domain nouns and requirement phrases ("needs to", "should", "must support") produce no predicates. Predicates come from action verbs in the TEST/IMPLEMENT/SECURITY/etc. lexicons.
