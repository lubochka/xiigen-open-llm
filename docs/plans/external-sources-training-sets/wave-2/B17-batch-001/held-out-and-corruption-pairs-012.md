# Held-Out and Corruption Pairs (012)
# Batch: B17-batch-001 | Scenario: B17 — Development — integration — third-party payment connector
# Production date: 2026-05-05 | pair_kind: held-out-and-corruption

---

## B17-HO-012-001 — HELD-OUT: Four-case mix #1 — "wire up" + "secure" + refactor

**Input:**
"Refactor the payment gateway connector, wire up the new Stripe v3 API endpoints, and secure the webhook payload handling."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY"]
SubPhase: 4.3, 3.3, 3.4
TransitionType: refactoring (refactor verb → refactoring hierarchy wins)

**Reasoning:** refactor → IMPLEMENT, refactoring; connector/wire up/API → API_CONTRACT; secure → SECURITY. No "oauth" or "token" → AUTHORIZATION and extra SECURITY don't fire.

---

## B17-HO-012-002 — HELD-OUT: Four-case mix #2 — "create tests" + "implement connector"

**Input:**
"Implement the SlackNotificationConnector for posting messages and create integration tests for the message dispatch flow."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST"]
SubPhase: 4.3, 3.3, 5.1
TransitionType: feature_addition

**Key tests:** "implement" + "connector" → 4.3 (not 4.1); "create integration tests" → TEST; TEST → 5.1.

---

## B17-HO-012-003 — HELD-OUT: Four-case mix #3 — "spec" + "build" + "OAuth"

**Input:**
"Build the Auth0 OAuth identity connector and spec out the authorization flow interface."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "AUTHORIZATION", "DOCUMENT"]
SubPhase: 4.3, 3.3, 3.4, 2.1
TransitionType: feature_addition

**Note:** SECURITY does NOT fire (no token or authentication vocabulary). DOCUMENT fires from "spec out".

---

## B17-HO-012-004 — HELD-OUT: Four-case mix #4 — Full seven-predicate input

**Input:**
"Build the enterprise payment connector, implement OAuth token refresh, secure client credentials, document the payment API interfaces, write integration tests, and QA the end-to-end payment flow."

**Expected output:**
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "AUTHORIZATION", "TEST", "DOCUMENT", "QA"]
SubPhase: 4.3, 3.3, 3.4, 3.4, 5.1, 2.1, 7.x
TransitionType: feature_addition
DecisionCount: 7
QAPosition: last-absolute

**This is the maximum B17 pipeline.** All seven valid predicates fire. DecisionCount: 7. Unique sub-phases: 5 (4.3, 3.3, 3.4, 5.1, 2.1, 7.x — 3.4 counted once despite two predicates).

**Gate B:** All seven predicates in canonical order, QA last, SECURITY and AUTHORIZATION both present (not merged), DecisionCount = 7.
