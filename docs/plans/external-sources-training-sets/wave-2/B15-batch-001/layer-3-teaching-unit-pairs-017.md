# Layer 3 — Teaching Unit Pairs (017)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: teaching_unit

## TU-017-A — "moderate" is not a SECURITY trigger

A: "Implement the review service and add content moderation." Does SECURITY fire?
B: No. "moderate" / "moderation" are not in the SECURITY lexicon. SECURITY fires only on: secure, harden, protect, encrypt, authenticate. Content moderation is a business rule enforced in IMPLEMENT. Only ["IMPLEMENT"] fires.

## TU-017-B — "secure the endpoint" fires SECURITY

A: "Implement the review service and secure the review submission endpoint against spam injection." How many predicates?
B: Two: ["IMPLEMENT", "SECURITY"]. "secure" → SECURITY fires. The endpoint context and "injection attacks" reinforce it, but "secure" alone is sufficient.

## TU-017-C — purchase verification: which interface?

A: ReviewAndRatingService must verify a customer has purchased a product before allowing a review. How is this implemented?
B: IPurchaseHistoryService is injected via constructor (fabric-first). ReviewService calls IPurchaseHistoryService.HasPurchased(CustomerId, ProductId) → OperationResult<bool>. The concrete implementation is injected by the DI (Dependency Injection) container — ReviewService never knows the concrete type.

## TU-017-D — Rating aggregate: approved reviews only

A: The rating aggregate returns AverageRating for a product. Does it include pending and rejected reviews?
B: No. BR-REV-003: aggregate is computed from approved reviews only. Pending and rejected reviews are excluded. This is enforced in the repository query (IReviewRepository.GetApprovedReviews(ProductId)) — not in the aggregation computation itself.

## TU-017-E — "spec out" fires DOCUMENT

A: "Build the rating service and spec out the aggregation algorithm in the documentation." How many predicates?
B: Two: ["IMPLEMENT", "DOCUMENT"]. "spec out" → DOCUMENT fires. "build" → IMPLEMENT fires. Two routing decisions: ImplementationFlow41 + DocumentationFlow21.
