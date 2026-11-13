# Layer 2 — Nine-Node Flow Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-008 — Price calculation engine

Request: "Implement the price calculation engine with tiered discounts, tax rules, and currency conversion."

### Flow: ImplementationFlow41

**Node 1**: Target: PriceCalculationEngine | Scope: base price, tiered discount application, tax rule evaluation, currency conversion
**Node 2**: Gate B: discount tiers configurable; tax rules injected via interface; currency rates via external interface
**Node 3**: probationary_accepted — scope valid; ITaxRuleProvider, ICurrencyConverter interfaces defined
**Node 4**: Implementation: BasePriceCalculator, TieredDiscountApplicator, TaxRuleEvaluator, CurrencyConverter
**Node 5**: Arbiter: ImplementationArbiter41
**Node 6**: Coverage: base price ✓, tiered discounts ✓, tax rules ✓, currency conversion ✓ — all via OperationResult<T>
**Node 7**: ImplementationFlow41 GREEN
**Node 8**: Output: PriceCalculationEngine | 4 components | ITaxRuleProvider + ICurrencyConverter required (fabric-first)
**Node 9**: Verdict: IMPLEMENT_APPROVED — TransitionType: feature_addition
