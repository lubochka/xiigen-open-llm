# Layer 0-C — Knowledge-Edge Pairs (033)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: knowledge-edge | focus: Zendesk support connector edge cases

---

## B17-0C-033-001 — Zendesk: "authorization" English word trap

The most important edge for Zendesk:
- "Build Zendesk connector with proper authorization for support agents." → AUTHORIZATION does NOT fire
- "authorization" (English noun, any form) ≠ AUTHORIZATION predicate trigger
- ONLY "OAuth" (the word, any case) fires AUTHORIZATION

Correct: Predicates: [IMPLEMENT, API_CONTRACT]
Incorrect (v6-01): Predicates: [IMPLEMENT, API_CONTRACT, AUTHORIZATION]

This is the identical rule that applies to "authorization" for Okta, HubSpot, Zuora, etc.

---

## B17-0C-033-002 — Zendesk: "ticket", "agent", "support" are domain nouns

Domain nouns: ticket, agent, support, helpdesk, SLA (Service Level Agreement), macro, trigger (Zendesk product concept)
Note: Zendesk uses the word "trigger" as a product concept. This does NOT fire any predicate in this corpus.
Only structural vocabulary fires predicates.

---

## B17-0C-033-003 — Zendesk: SECURITY from token AND from authentication separately

Input: "Build Zendesk connector with API token authentication for agent access."
- "token" → SECURITY (Rule 6; unconditional)
- "authentication" → SECURITY (Rule 8; reinforces; fires once)

SECURITY fires once — not twice — because predicates are sets. Multiple SECURITY triggers produce one SECURITY predicate.

---

## B17-0C-033-004 — Zendesk: OAuth 2.0 with token: both SECURITY and AUTHORIZATION

Input: "Build Zendesk connector with OAuth 2.0 token for agent authentication."
- "OAuth" → AUTHORIZATION (independently)
- "token" → SECURITY (independently; Rule 6 is unconditional)
- "authentication" → SECURITY (reinforces; fires once)

SECURITY and AUTHORIZATION are independent predicates. The presence of "OAuth" does NOT suppress SECURITY from "token".
Both fire. Both route to 3.4.
