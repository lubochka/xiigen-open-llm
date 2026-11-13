# Layer 0-B — Router Pairs (025)
# Batch: B17-batch-001 | Scenario: B17 — Third-party connector integrations (sub-phase 4.3)
# Production date: 2026-05-06 | pair_kind: router | focus: Twilio SMS connector routing

---

## B17-0B-025-001 — TwilioSmsConnector routing table

| Input pattern | Predicates | SubPhase(s) | TransitionType | DecisionCount |
|---|---|---|---|---|
| Build Twilio SMS connector with webhook and token | [IMPL, API, SEC] | 4.3, 3.3, 3.4 | feature_addition | 3 |
| Integrate Twilio Messaging API | [API] | 3.3 | documentation | 1 |
| Wire up Twilio SMS endpoints | [API] | 3.3 | documentation | 1 |
| Build Twilio connector with OAuth token | [IMPL, API, SEC, AUTH] | 4.3, 3.3, 3.4 | feature_addition | 4 |
| Refactor Twilio connector | [IMPL, API] | 4.3, 3.3 | refactoring | 2 |

---

## B17-0B-025-002 — "integrate" vs "build" routing for TwilioSmsConnector

| Input | Predicates | Route | Notes |
|---|---|---|---|
| "Integrate Twilio" | [API_CONTRACT] | 3.3 | "integrate" = API_CONTRACT only |
| "Build Twilio connector" | [IMPL, API] | 4.3 + 3.3 | "connector" integration vocab → 4.3 |
| "Wire up Twilio endpoints" | [API] | 3.3 | "wire up" = API_CONTRACT only |
| "Build Twilio SMS service" (no connector) | [IMPL] | 4.1 | no integration vocab → 4.1 |

---

## B17-0B-025-003 — SECURITY routing for TwilioSmsConnector

When input contains "token" or "authentication" for Twilio:
- SECURITY fires unconditionally → 3.4 (SecurityAuditFlow34)
- SECURITY and AUTHORIZATION both route to 3.4 if both present
- DecisionCount reflects all predicates; UniqueSubPhases counts distinct destinations

---

## B17-0B-025-004 — Nearest wrong answers for TwilioSmsConnector

| Input | Nearest wrong | Correct |
|---|---|---|
| "Integrate Twilio SMS API" | IMPLEMENT invented | [API_CONTRACT] only → 3.3 |
| "Build Twilio service" (no connector) | IMPLEMENT → 4.3 | IMPLEMENT → 4.1 (no connector vocab) |
| "Build Twilio connector with token" | SECURITY suppressed | SECURITY fires on "token" unconditionally |
