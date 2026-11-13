# Layer 0-C — Knowledge-Edge Pairs (017)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B15-kedge-017-001 — "moderate" does not fire SECURITY

A: "Implement the review service and add content moderation to reject abusive language."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "moderate" / "moderation" — not in SECURITY lexicon. Content moderation is a business rule (implement-scope). SECURITY fires only on: secure, harden, protect, encrypt, authenticate.

---

## B15-kedge-017-002 — "secure the submission endpoint" fires SECURITY

A: "Implement the review service and secure the review submission endpoint against spam injection attacks."
B:
  CompiledPredicateIds: ["IMPLEMENT", "SECURITY"]
  Edge analysis: "secure" → SECURITY fires. "submission endpoint" + "injection attacks" provide security context but "secure" alone is sufficient to fire SECURITY.

---

## B15-kedge-017-003 — "write integration tests" fires TEST

A: "Build the review and rating service and write integration tests for the review submission workflow."
B:
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Edge analysis: "write integration tests" → TEST fires on "write" + "tests". "integration" qualifies the test type (maps to TestingFlow51 sub-phase 5.1, not E2E sub-phase 5.3 — integration tests are 5.1).

---

## B15-kedge-017-004 — "spec out" fires DOCUMENT

A: "Build the rating service and spec out the rating aggregation algorithm in the API documentation."
B:
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  Edge analysis: "spec out" → DOCUMENT fires. "spec out" is in DOCUMENT lexicon alongside document/specify/define spec.

---

## B15-kedge-017-005 — "approval workflow" does not fire any additional predicate

A: "Build the review service with an approval workflow for pending reviews."
B:
  CompiledPredicateIds: ["IMPLEMENT"]
  Edge analysis: "approval workflow" — business process noun; not a predicate trigger. Only "build" fires IMPLEMENT. The workflow concept is within the IMPLEMENT scope (build the workflow = feature_addition).
