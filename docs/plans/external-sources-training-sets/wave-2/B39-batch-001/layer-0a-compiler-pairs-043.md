# Layer 0-A — Compiler Pairs (043)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-043-001 — OAuthFlowTest: predicate compilation

A: "Write E2E test scenarios for the OAuth (Open Authorization) login page — assert the provider button renders, verify the authorization screen loads."
B: TEST (write, assert, verify). UI ("OAuth login page" → page; "provider button" → button; "authorization screen" → screen — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-043-002 — OAuthFlowTest: "Google" is gap

A: "Configure Google OAuth for the login page." Predicates?
B: "configure" is gap. "Google" is a third-party provider name — gap. "login page" → page → UI fires. Result: [UI]. Single flow: UIInteractionFlow15 (1.5).

---

## C0A-043-003 — OAuthFlowTest: consent modal fires UI

A: "Assert the OAuth consent modal requests appropriate permissions." Predicates?
B: TEST (assert). UI (OAuth consent modal → modal). Result: [TEST, UI].

---

## C0A-043-004 — OAuthFlowTest: "execute OAuth flow" is gap

A: "Execute the OAuth flow for all providers." Predicates?
B: "execute" is gap. "OAuth flow" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-043-005 — OAuthFlowTest: IMPLEMENT fires on "implement OAuth integration"

A: "Implement Google OAuth integration and write E2E tests for the login page." Predicates?
B: IMPLEMENT ("implement ... integration"). TEST ("write E2E tests"). UI ("login page" → page). [IMPLEMENT, TEST, UI]. TransitionType: feature_addition.
