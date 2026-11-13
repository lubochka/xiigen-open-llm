# Layer 2 — Nine-Node Flow Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-013 — OrderFulfillmentService (IMPLEMENT alone)

Request: "Build the order fulfillment service that picks, packs, and dispatches orders to the carrier."

### Flow: ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: OrderFulfillmentService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseId: "4.1", PredicateIds: ["IMPLEMENT"] | Scope: pick-and-pack orchestrator, dispatch router, carrier integration adapter, packing slip generator

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement three domains: (1) Pick orchestration — locate stock, generate pick list; (2) Pack domain — validate pack contents, generate packing slip; (3) Dispatch — select carrier, submit dispatch request. Non-functional: idempotent dispatch (duplicate order IDs rejected), OperationResult<T> on all public methods, inject ICarrierClient interface (fabric-first).

**Node 3 — SchemePlannerUnit**: probationary_accepted — scope valid; ICarrierClient interface approved; three work branches planned: PickOrchestrator, PackDomain, DispatchRouter.

**Node 4 — DomainProcessorUnit**: WorkBranches processed:
- PickOrchestrator: IFulfillmentService.StartPick(OrderId) → OperationResult<PickList>; BR-FULF-001: stock must be confirmed before pick starts.
- PackDomain: IPackService.Pack(PickList) → OperationResult<PackingSlip>; BR-FULF-002: packing slip must include all line items.
- DispatchRouter: IDispatchService.Dispatch(PackingSlip) → OperationResult<TrackingNumber>; BR-FULF-003: carrier selected by weight and destination zone.

**Node 5 — ApplierUnit**: Files generated: IFulfillmentService.cs, OrderFulfillmentService.cs, ICarrierClient.cs, PickList.cs, PackingSlip.cs, DispatchRouter.cs. Business rules BR-FULF-001 through BR-FULF-003 registered.

**Node 6 — ResultTranslatorUnit**: Implementation summary: 6 files, 3 interfaces, 3 business rules, 1 domain event (OrderDispatched). Test surface: 12 recommended cases.

**Node 7 — ArbiterSelectorUnit**: Arbiter: implementation-completeness-arbiter. ReviewFocus: all 3 domains covered, all public methods return OperationResult<T>, ICarrierClient injected.

**Node 8 — ArbiterUnit**: Verdict: PASS. All 3 domains covered. Fabric-first confirmed (ICarrierClient injected via constructor). OperationResult<T> confirmed on all public methods.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED | TransitionType: feature_addition | Files: 6 | BusinessRules: 3 | TestSurface: 12 cases
