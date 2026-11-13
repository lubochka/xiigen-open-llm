# Layer 2 — Nine-Node Flow Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41 — refactoring)

## B15-nine-022 — PricingEngineRefactor (IMPLEMENT, TransitionType: refactoring)

Request: "Refactor the pricing engine service to separate discount calculation from margin calculation into distinct strategy classes."

### Flow: ImplementationFlow41 (refactoring mode)

**Node 1 — DomainSplitterUnit**: Target: PricingEngineService (refactoring) | A = SelectedLogicalTransition: TransitionType: refactoring, SubPhaseId: "4.1", PredicateIds: ["IMPLEMENT"] | Existing code being refactored — not new feature. Scope: extract IDiscountStrategy and IMarginStrategy from monolithic PricingEngine; preserve all existing callers.

**Node 2 — DomainTranslatorUnit**: Gate B (refactoring): (1) existing PricingEngine.CalculatePrice() must not break (backward-compatible); (2) IDiscountStrategy interface extracted with at least TieredDiscountStrategy and PromoCodeDiscountStrategy; (3) IMarginStrategy interface extracted with at least FixedMarginStrategy and PercentageMarginStrategy; (4) no business logic deleted — only restructured.

**Node 3 — SchemePlannerUnit**: probationary_accepted — refactoring scope valid; strategy pattern approved; backward-compatibility constraint confirmed.

**Node 4 — DomainProcessorUnit**: WorkBranches:
- StrategyExtraction: IDiscountStrategy.Apply(Price, Context) → DiscountedPrice; IMarginStrategy.Apply(Cost, Context) → MarginPrice.
- PricingEngineRefactor: PricingEngine refactored to accept IDiscountStrategy[] and IMarginStrategy via constructor injection.
- BackwardCompatibility: existing public API signature preserved; callers unmodified.

**Node 5 — ApplierUnit**: Files modified: PricingEngine.cs (refactored). Files added: IDiscountStrategy.cs, TieredDiscountStrategy.cs, PromoCodeDiscountStrategy.cs, IMarginStrategy.cs, FixedMarginStrategy.cs, PercentageMarginStrategy.cs.

**Node 6 — ResultTranslatorUnit**: 1 file modified + 6 files added. 2 new interfaces. 0 business rules deleted. Backward-compatible. Test surface: 8 recommended regression cases.

**Node 7 — ArbiterSelectorUnit**: Arbiter: refactoring-safety-arbiter. ReviewFocus: no business logic deleted, existing callers unmodified, strategy pattern correctly injected.

**Node 8 — ArbiterUnit**: Verdict: PASS. Backward-compatibility confirmed. Strategy injection via constructor (fabric-first). No business logic deleted.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED — TransitionType: refactoring | Files changed: 7 (1 modified + 6 added) | NewInterfaces: 2 | Backward-compatible: YES
