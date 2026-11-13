# Held-Out and Corruption Pairs (017)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: held_out_corruption

## HC-017-A — Held-out: ReviewAndRatingService four predicates

A: "Build review service, secure submission endpoint, write integration tests, and document the moderation workflow."
B: Predicates: ["IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"] | feature_addition | DecisionCount: 4

## HC-017-B — Held-out: "moderate" does not fire SECURITY

A: "Implement the review service with content moderation."
B: Predicates: ["IMPLEMENT"] | "moderate" is not a SECURITY trigger. Only ["IMPLEMENT"] fires.

## HC-017-C — Corruption v6-01: "rating" suppresses IMPLEMENT

v6 corrupt: "Implement the review and rating service" → CompiledPredicateIds: [] ("rating aggregation is analytics terminology")
Correct: Predicates: ["IMPLEMENT"] | "implement" fires IMPLEMENT unconditionally.
Error: v6-01. "rating" is a domain noun — it does not suppress the predicate fired by "implement".

## HC-017-D — Corruption v6-02: "moderate" incorrectly caught as SECURITY

v6 corrupt (inverse): "Implement the review service and add content moderation" → Predicates: ["IMPLEMENT", "SECURITY"]
Correct: Predicates: ["IMPLEMENT"] | "moderate" does not fire SECURITY. SECURITY requires: secure/harden/protect/encrypt/authenticate.
Error: v6-02 inverse (spurious predicate added). This is the mirror of v6-02 — adding a predicate that should not fire.

## HC-017-E — Held-out: purchase history verification via IPurchaseHistoryService

A: ReviewAndRatingService must verify purchase before accepting a review. Which predicate does this design requirement fire?
B: None additional. This is a business rule implemented within the IMPLEMENT flow. Injecting IPurchaseHistoryService is a fabric-first design decision — it does not fire an additional predicate. Only ["IMPLEMENT"] fires from the original request.
