# Layer 2 — Nine-Node Flow Pairs (011)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-05 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-011 — Order refund service

Request: "Build the refund processing service supporting full and partial refunds with automatic payment gateway reversal."

### Flow: ImplementationFlow41

**Node 1**: Target: RefundProcessingService | Scope: refund eligibility, full refund, partial refund, gateway reversal dispatch
**Node 2**: Gate B: eligibility rules configurable; gateway reversal via interface (IPaymentGatewayClient); partial amounts validated
**Node 3**: probationary_accepted — scope valid; IRefundPolicy, IPaymentGatewayClient interfaces defined
**Node 4**: Implementation: RefundEligibilityChecker, FullRefundProcessor, PartialRefundProcessor, GatewayReversalDispatcher
**Node 5**: Arbiter: ImplementationArbiter41
**Node 6**: Coverage: eligibility ✓, full refund ✓, partial refund ✓, gateway reversal ✓ — all OperationResult<T>
**Node 7**: ImplementationFlow41 GREEN
**Node 8**: Output: RefundProcessingService | 4 components | IPaymentGatewayClient required (fabric-first)
**Node 9**: Verdict: IMPLEMENT_APPROVED — TransitionType: feature_addition
