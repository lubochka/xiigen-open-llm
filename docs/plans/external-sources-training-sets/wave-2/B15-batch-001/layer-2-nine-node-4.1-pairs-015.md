# Layer 2 — Nine-Node Flow Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-015 — InventoryAllocationService (IMPLEMENT alone)

Request: "Create the inventory allocation service that reserves stock when an order is placed and releases it on cancellation."

### Flow: ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: InventoryAllocationService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseId: "4.1", PredicateIds: ["IMPLEMENT"] | Scope: stock reservation handler, allocation release handler, reservation expiry daemon, inventory quantity manager.

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement: (1) Stock Reservation — reserve quantity for order, write allocation record; (2) Allocation Release — release reservation on order cancellation or expiry; (3) Quantity Guard — ensure reservation does not exceed available stock. Concurrency: optimistic locking via ETag (Entity Tag) on allocation records. All methods return OperationResult<T>.

**Node 3 — SchemePlannerUnit**: probationary_accepted — scope valid; IInventoryRepository interface approved for fabric-first injection.

**Node 4 — DomainProcessorUnit**: WorkBranches processed:
- StockReservation: IAllocationService.Reserve(AllocationRequest) → OperationResult<AllocationId>; BR-ALLOC-001: reservation must not exceed AvailableQuantity; BR-ALLOC-002: reservation ID is deterministic UUID(v5) from OrderId + ProductId (idempotent).
- AllocationRelease: IAllocationService.Release(AllocationId) → OperationResult<ReleaseConfirmation>; BR-ALLOC-003: release updates AvailableQuantity immediately.
- ReservationExpiry: IAllocationService.ExpireStale(TimeSpan maxAge) → OperationResult<int> (count released); BR-ALLOC-004: reservations older than 15 minutes without order confirmation auto-release.

**Node 5 — ApplierUnit**: Files generated: IAllocationService.cs, InventoryAllocationService.cs, IInventoryRepository.cs, AllocationRecord.cs, AllocationExpiryJob.cs, InventoryQuantityGuard.cs.

**Node 6 — ResultTranslatorUnit**: 6 files, 3 interfaces, 4 business rules, 1 domain event (StockReserved). Test surface: 11 recommended cases.

**Node 7 — ArbiterSelectorUnit**: Arbiter: implementation-completeness-arbiter. ReviewFocus: concurrency guard present, idempotent reservation confirmed, OperationResult<T> on all public methods.

**Node 8 — ArbiterUnit**: Verdict: PASS. Optimistic locking via ETag confirmed. Idempotent reservation via UUID(v5) confirmed. OperationResult<T> confirmed.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED | TransitionType: feature_addition | Files: 6 | BusinessRules: 4 | TestSurface: 11 cases
