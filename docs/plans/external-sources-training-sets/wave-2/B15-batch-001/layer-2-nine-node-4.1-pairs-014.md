# Layer 2 — Nine-Node Flow Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1
# Production date: 2026-05-06 | pair_kind: nine_node_flow (ImplementationFlow41)

## B15-nine-014 — RefundService (IMPLEMENT alone)

Request: "Implement the refund and chargeback service to handle customer refund requests and payment reversals."

### Flow: ImplementationFlow41

**Node 1 — DomainSplitterUnit**: Target: RefundAndChargebackService (feature_addition) | A = SelectedLogicalTransition: TransitionType: feature_addition, SubPhaseId: "4.1", PredicateIds: ["IMPLEMENT"] | Scope: refund request handler, partial/full refund calculator, chargeback dispute manager, payment reversal coordinator.

**Node 2 — DomainTranslatorUnit**: Gate B: service must implement three domains: (1) Refund Calculation — calculate refund amount (partial vs full), apply restocking rules; (2) Payment Reversal — coordinate reversal with payment service via IPaymentService interface; (3) Chargeback Dispute — record dispute, track chargeback status machine. All methods return OperationResult<T>.

**Node 3 — SchemePlannerUnit**: probationary_accepted — scope valid; IPaymentService interface approved; four work branches: RefundCalculator, PaymentReversal, ChargebackDispute, RefundRepository.

**Node 4 — DomainProcessorUnit**: WorkBranches processed:
- RefundCalculator: IRefundService.CalculateRefund(RefundRequest) → OperationResult<RefundAmount>; BR-RFND-001: restocking fee applies to non-defective returns over 30 days.
- PaymentReversal: IRefundService.ExecuteReversal(RefundAmount) → OperationResult<ReversalConfirmation>; BR-RFND-002: reversal must match original payment method.
- ChargebackDispute: IChargebackService.OpenDispute(ChargebackRequest) → OperationResult<DisputeId>; BR-RFND-003: disputes must be filed within 120 days of transaction.

**Node 5 — ApplierUnit**: Files generated: IRefundService.cs, RefundAndChargebackService.cs, IPaymentService.cs (interface for payment reversal), RefundCalculator.cs, ChargebackStateMachine.cs, RefundRepository.cs.

**Node 6 — ResultTranslatorUnit**: 6 files, 3 interfaces, 3 business rules, 1 domain event (RefundIssued). Test surface: 14 recommended cases (partial refund, full refund, chargeback, reversal failure, late dispute rejection).

**Node 7 — ArbiterSelectorUnit**: Arbiter: implementation-completeness-arbiter. ReviewFocus: three domains covered, IPaymentService injected (not concrete), OperationResult<T> on all public methods.

**Node 8 — ArbiterUnit**: Verdict: PASS. Three domains covered. IPaymentService injected via constructor (fabric-first confirmed). OperationResult<T> confirmed.

**Node 9 — FormatterUnit**: Verdict: IMPLEMENT_APPROVED | TransitionType: feature_addition | Files: 6 | BusinessRules: 3 | TestSurface: 14 cases
