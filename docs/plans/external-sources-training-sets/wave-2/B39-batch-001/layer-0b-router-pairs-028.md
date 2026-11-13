# Layer 0-B — Router Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-028-001 — LoginFlowTest: routing decision

A: "Write E2E test cases for the login page — assert valid credentials, verify the screen loads." Predicates: [TEST, UI]. Route.
B: TEST → 5.3 (E2ETestingFlow53): login flow is browser-level E2E. UI → 1.5 (UIInteractionFlow15): "login page", "screen" → "page", "screen" always route to 1.5. TransitionType: documentation.

---

## C0B-028-002 — LoginFlowTest: locked-out scenario routing

A: "Create test scenarios for the locked-out login screen — assert the error banner and retry button." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("login screen", "error banner", "retry button" → "screen", "banner", "button" fire UI → 1.5). Same routing as standard login test. No IMPLEMENT. TransitionType: documentation.

---

## C0B-028-003 — LoginFlowTest: gap request

A: "Run the login E2E suite against staging." Predicates: []. Route.
B: Zero predicates. knowledge_edge_synthetic routing. CapabilityGapSignal emitted. No FlowInstances. TrustState: knowledge_edge_synthetic. Gap vocabulary logged: "run" (gap verb), "login E2E suite" (test noun with gap verb only).

---

## C0B-028-004 — LoginFlowTest: IMPLEMENT+TEST+UI routing

A: "Build the login form with remember-me toggle, add test coverage for toggle persistence, assert the checkbox retains state." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5 ("login form", "remember-me toggle", "checkbox" → multiple UI triggers → one UI predicate → 1.5). TransitionType: feature_addition. Execution order: IMPLEMENT → TEST+UI (parallel).

---

## C0B-028-005 — LoginFlowTest: QA routing

A: "Write login flow specs, assert the redirect, and QA gate before sprint review." Predicates: [TEST, UI, QA]. Route QA.
B: QA → 7.3 (QAGatingFlow73): "sprint review" is sprint/feature-acceptance context, default 7.3 applies. Not "staging deployment" or "release cut" — so 7.1 does not apply. AllSubPhaseIds: [5.3, 1.5, 7.3].
