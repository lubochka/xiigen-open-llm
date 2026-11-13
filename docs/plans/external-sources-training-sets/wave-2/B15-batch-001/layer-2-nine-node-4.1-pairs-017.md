# Layer 2 — Nine-Node Flow Pairs (017)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-017 — ReviewAndRatingService (IMPLEMENT alone)

Request: "Create the review and rating service that lets customers submit star ratings and written reviews for purchased products."

### Flow: ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: ReviewAndRatingService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseId: "4.1", PredicateIds: ["IMPLEMENT"] | Scope: review submission handler, rating aggregator, review approval workflow, review retrieval service.

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement: (1) Review Submission — accept customer review (rating + text), validate against purchase history; (2) Rating Aggregation — compute AverageRating and RatingCount per product; (3) Moderation — status machine (Pending → Approved / Rejected). All methods return OperationResult<T>. Inject IReviewRepository and IPurchaseHistoryService.

**Node 3 — SchemePlannerUnit**: probationary_accepted — scope valid; IPurchaseHistoryService interface approved for fabric-first injection; three work branches planned.

**Node 4 — DomainProcessorUnit**: WorkBranches processed:
- ReviewSubmission: IReviewService.SubmitReview(ReviewRequest) → OperationResult<ReviewId>; BR-REV-001: customer must have purchased product (verified via IPurchaseHistoryService); BR-REV-002: star rating must be 1–5.
- RatingAggregation: IReviewService.GetAggregateRating(ProductId) → OperationResult<RatingAggregate>; BR-REV-003: aggregate computed from approved reviews only.
- ModerationWorkflow: IReviewModerationService.Approve(ReviewId) → OperationResult<bool>; IReviewModerationService.Reject(ReviewId, string reason) → OperationResult<bool>.

**Node 5 — ApplierUnit**: Files generated: IReviewService.cs, ReviewAndRatingService.cs, IReviewModerationService.cs, ReviewModerationService.cs, IReviewRepository.cs, ReviewStateMachine.cs, RatingAggregator.cs.

**Node 6 — ResultTranslatorUnit**: 7 files, 4 interfaces, 3 business rules, 2 domain events (ReviewSubmitted, ReviewApproved). Test surface: 13 recommended cases.

**Node 7 — ArbiterSelectorUnit**: Arbiter: implementation-completeness-arbiter. ReviewFocus: purchase verification present, rating validation present, aggregate from approved only.

**Node 8 — ArbiterUnit**: Verdict: PASS. Purchase verification via IPurchaseHistoryService (fabric-first). Rating range validation confirmed. Aggregate computed from approved-only reviews confirmed.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED | TransitionType: feature_addition | Files: 7 | BusinessRules: 3 | TestSurface: 13 cases
