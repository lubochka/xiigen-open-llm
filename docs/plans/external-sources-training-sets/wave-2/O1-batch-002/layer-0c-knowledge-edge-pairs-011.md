# Layer 0-C — Knowledge Edge Pairs (011)
# Batch: O1-batch-002 | Scenario: O1b — OAuth 2.0 client credentials and tenant scope management
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## O1b-0C-011-001 — "oauth is broken" boundary → SECURITY fires

A: "oauth is broken in the staging environment"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth" is in the SECURITY lexicon. Even a vague "is broken" complaint fires SECURITY when the technical term is present. Routes to 3.4. NOT knowledge_edge.

---

## O1b-0C-011-002 — "oauth2 is down" → SECURITY fires

A: "oauth2 is down, no services can authenticate"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth2" fires SECURITY (oauth prefix); "authenticate" fires SECURITY. Two SECURITY signals, one 3.4 routing decision.

---

## O1b-0C-011-003 — "oauth broken" vs "portal broken" — the distinction

A: Contrast: "oauth is broken" vs "the portal is broken"

B: "oauth is broken" → SECURITY fires ("oauth" in lexicon) → VALID ROUTING to 3.4.
   "the portal is broken" → knowledge_edge ("portal" not in lexicon, "broken" not in lexicon) → NO routing.
   The distinction: presence vs. absence of a lexicon keyword in the prompt.

---

## O1b-0C-011-004 — "openid connect is failing" → SECURITY fires

A: "openid connect (OIDC) is failing for the enterprise tenant"

B: Result: VALID ROUTING | Predicates: ["SECURITY", "AUTHORIZATION"]
   Reasoning: "openid" fires SECURITY; "oidc" fires SECURITY; "tenant" fires AUTHORIZATION. Routes to 3.4.

---

## O1b-0C-011-005 — "oauth" anywhere in sentence fires SECURITY

A: "we're having issues with the oauth integration since the upgrade"

B: Result: VALID ROUTING | Predicates: ["SECURITY"]
   Reasoning: "oauth" fires SECURITY regardless of surrounding colloquial language. One lexicon word is sufficient.
