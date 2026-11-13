# Layer 0-B — Router Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-019-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Build the notification service."
Router:
  - PredictedSubPhaseId: "4.1" | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-019-002 — IMPLEMENT+API_CONTRACT → two flows (SendGrid + Twilio)

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Build the notification service and integrate with the SendGrid email API and Twilio SMS API."
Router:
  - APIContractFlow33 (API_CONTRACT) — sub-phase 3.3
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | Note: Two external APIs mentioned but deduplication → single API_CONTRACT predicate → single APIContractFlow33 routing.

---

## B15-router-019-003 — IMPLEMENT+API_CONTRACT+SECURITY → three flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY"] | Request: "Build the notification service, wire up the email API, and secure the SMS dispatch with token-based authentication."
Router:
  - APIContractFlow33 | SecurityAuditFlow | ImplementationFlow41
  DecisionCount: 3 | Ordering: 3.3 → SecurityAuditFlow → 4.1.

---

## B15-router-019-004 — Deduplication routing: three API triggers → one APIContractFlow33

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Implement the notification service, integrate with email API, integrate with SMS API, and connect to push gateway."
Router:
  - APIContractFlow33 | ImplementationFlow41
  DecisionCount: 2 | Teaching point: Three API-firing verbs (integrate×2, connect×1) deduplicate to single API_CONTRACT → single APIContractFlow33.

---

## B15-router-019-005 — IMPLEMENT+API_CONTRACT+SECURITY+DOCUMENT → four flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"] | Request: "Build the notification service, integrate with messaging APIs, secure the dispatch tokens, and document the notification API."
Router:
  - APIContractFlow33 | SecurityAuditFlow | ImplementationFlow41 | DocumentationFlow21
  DecisionCount: 4
