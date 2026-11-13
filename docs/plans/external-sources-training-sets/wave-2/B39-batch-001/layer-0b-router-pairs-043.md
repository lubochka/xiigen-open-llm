# Layer 0-B — Router Pairs (043)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-043-001 — OAuthFlowTest: routing [TEST, UI]

A: "Write E2E tests for the OAuth login page — assert provider button and authorization screen." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, button, screen). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-043-002 — OAuthFlowTest: "Google" gap — UI still fires

A: "Configure Google OAuth for the login page." Predicates [UI]. AllSubPhaseIds?
B: AllSubPhaseIds: [1.5]. "login page" → page → UI → 1.5. "Google" and "configure" are gap. Single flow.

---

## C0B-043-003 — OAuthFlowTest: consent modal → 1.5

A: Does an "OAuth consent modal" route to 1.5?
B: Yes. Modal → 1.5 always.

---

## C0B-043-004 — OAuthFlowTest: IMPLEMENT → 4.1

A: "Implement Google OAuth integration and write E2E tests for the login page." AllSubPhaseIds?
B: [4.1, 5.3, 1.5]. TransitionType: feature_addition.

---

## C0B-043-005 — OAuthFlowTest: "execute OAuth flow" gap

A: "Execute the OAuth flow." Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.
