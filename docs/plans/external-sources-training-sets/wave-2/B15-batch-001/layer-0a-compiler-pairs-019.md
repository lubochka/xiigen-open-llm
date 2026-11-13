# Layer 0-A — Compiler Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-019-001 — NotificationService: "build" → IMPLEMENT

A: "Build the notification service to dispatch email, SMS, and push notifications for order lifecycle events."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: build → IMPLEMENT; notification service → backend service noun.

---

## B15-compiler-019-002 — NotificationService: "integrate" → IMPLEMENT+API_CONTRACT

A: "Build the notification service and integrate with the SendGrid email API and Twilio SMS API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; integrate → API_CONTRACT (integrate fires API_CONTRACT; two external APIs mentioned but deduplication applies — still one API_CONTRACT predicate).

---

## B15-compiler-019-003 — NotificationService: "connect" → IMPLEMENT+API_CONTRACT

A: "Implement the notification service and connect it to the push notification gateway."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; connect → API_CONTRACT (connect is in API_CONTRACT lexicon same as integrate/wire up).

---

## B15-compiler-019-004 — NotificationService: "secure" + "wire up" → IMPLEMENT+API_CONTRACT+SECURITY

A: "Build the notification service, wire up the email API, and secure the SMS dispatch with token-based authentication."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; wire up → API_CONTRACT; secure → SECURITY. Three independent predicates.

---

## B15-compiler-019-005 — NotificationService: deduplication confirmed

A: "Implement the notification service, integrate with email API, integrate with SMS API, and connect to push gateway."
B: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: feature_addition
Teaching point: "integrate" fires twice and "connect" fires once — all three trigger API_CONTRACT. Deduplication collapses to a single API_CONTRACT predicate. DecisionCount: 2 (not 4).
