# Layer 0-A — Compiler Pairs (017)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: compiler

---

## B15-compiler-017-001 — ReviewAndRatingService: "create" → IMPLEMENT

A: "Create the review and rating service that lets customers submit star ratings and written reviews for purchased products."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition | Sub-phase: 4.1
Evidence: create → IMPLEMENT; review and rating service → backend service noun.

---

## B15-compiler-017-002 — ReviewAndRatingService: "implement" + "moderate" → IMPLEMENT (SECURITY not triggered by "moderate")

A: "Implement the review service and add content moderation to reject abusive language."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; "content moderation" → not in SECURITY lexicon (secure/harden/protect trigger SECURITY; "moderate" alone does not). Single predicate fires.
Teaching point: "moderate" is not a SECURITY trigger. SECURITY fires on: secure, harden, protect, encrypt, authenticate. "Moderation" is a business rule, not a security action.

---

## B15-compiler-017-003 — ReviewAndRatingService: "write tests" → IMPLEMENT+TEST

A: "Build the review and rating service and write integration tests for the review submission and approval workflows."
B: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "write integration tests" → TEST fires on "write" + "integration tests".

---

## B15-compiler-017-004 — ReviewAndRatingService: "secure" → IMPLEMENT+SECURITY

A: "Implement the review service and secure the review submission endpoint against spam injection attacks."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; secure → SECURITY (secure + endpoint context confirms SECURITY).

---

## B15-compiler-017-005 — ReviewAndRatingService: "document" → IMPLEMENT+DOCUMENT

A: "Build the rating service and spec out the rating aggregation algorithm in the API documentation."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; "spec out" → DOCUMENT (spec out/document fires DOCUMENT).
