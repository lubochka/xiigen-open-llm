# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-019-001 — "dispatch" and "lifecycle events" do not fire additional predicates

A: "Build the notification service to dispatch email, SMS, and push notifications for order lifecycle events."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "dispatch" — action verb within IMPLEMENT scope; not an API_CONTRACT trigger (dispatch is not in API_CONTRACT lexicon). "lifecycle events" — domain noun. Single IMPLEMENT fires.

---

## B15-kedge-019-002 — "integrate with SendGrid" fires API_CONTRACT — deduplication

A: "Build the notification service and integrate with the SendGrid email API and Twilio SMS API."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "integrate" → API_CONTRACT fires once. Two external APIs (SendGrid, Twilio) mentioned — deduplication rule applies. Single API_CONTRACT predicate regardless of how many external systems are named.

---

## B15-kedge-019-003 — "connect" fires API_CONTRACT same as "integrate"

A: "Implement the notification service and connect it to the push notification gateway."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: "connect" → API_CONTRACT fires. "connect to [external system]" is equivalent to "integrate with [external system]" in predicate firing.

---

## B15-kedge-019-004 — "token-based authentication" fires SECURITY via "authenticate" lexeme

A: "Secure the SMS dispatch with token-based authentication."
B:
  CompiledPredicateIds: ["SECURITY"]
  Edge analysis: "secure" → SECURITY fires. "token-based authentication" confirms security context. "authenticate" is also in SECURITY lexicon — either "secure" or "authenticate" alone would fire SECURITY.

---

## B15-kedge-019-005 — deduplication: three API triggers still one API_CONTRACT

A: "Implement the notification service, integrate with email API, integrate with SMS API, and connect to push gateway."
B:
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Edge analysis: integrate (×2) + connect (×1) = three API_CONTRACT triggers → deduplication → single API_CONTRACT predicate. This is the deduplication rule applied across multiple verbs for the same predicate.
