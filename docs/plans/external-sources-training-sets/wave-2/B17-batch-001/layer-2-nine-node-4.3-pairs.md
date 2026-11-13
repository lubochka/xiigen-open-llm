# Layer 2 — Nine-Node Flow Pairs (sub-phase 4.3)
# Batch: B17-batch-001
# Scenario: B17 — Development — integration — third-party payment connector
# Flow: IntegrationConnectorFlow43
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node

---

## N01 — DomainSplitterUnit

```
source_id: xiigen-b17-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node: 01
```

A:
  TransitionId: "transition-B17-t1-001-4.3-feature_addition"
  UserRequest: "Build the Stripe payment integration connector — implement the charge, refund, and webhook handlers."
  PredicateIds: ["IMPLEMENT", "API_CONTRACT"]

B:
  DomainPackets:
    - PacketId: "pkt-B17-001-impl"
      Domain: "IMPLEMENT"
      Focus: "external payment connector construction"
      InputSummary: "Stripe connector: charge, refund, webhook — C# implementation"
      PredicateOwner: "IMPLEMENT"
  PacketCount: 1

---

## N02 — DomainTranslatorUnit

```
source_id: xiigen-b17-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node: 02
```

A: [IMPLEMENT DomainPacket from N01]

B:
  StatementId: "stmt-B17-001-impl"
  DomainStatement: "Implement StripeConnector integrating with Stripe API v2024-04-10.
    Three handlers: (1) ChargeHandler — create PaymentIntent, confirm, return ChargeResult;
    (2) RefundHandler — create Refund against existing PaymentIntentId, return RefundResult;
    (3) WebhookHandler — validate Stripe-Signature header (HMAC-SHA256), deserialize event,
    dispatch to IStripeEventHandler. Non-functional: idempotency key per charge, exponential
    backoff (3 retries) on 5xx, PCI-DSS constraint: no card data stored locally."
  SourcePacketId: "pkt-B17-001-impl"
  TranslationConfidence: 0.92

---

## N03 — SchemePlannerUnit

```
source_id: xiigen-b17-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node: 03
```

A: [TranslatedDomainStatement from N02]

B:
  WorkBranches:
    - BranchId: "branch-B17-charge"
      BranchName: "Charge Handler"
      Description: "ChargeHandler: PaymentIntent create/confirm, idempotency, retry logic"
      Priority: 1

    - BranchId: "branch-B17-refund"
      BranchName: "Refund Handler"
      Description: "RefundHandler: Refund creation against PaymentIntentId, partial refund support"
      Priority: 2

    - BranchId: "branch-B17-webhook"
      BranchName: "Webhook Handler"
      Description: "WebhookHandler: signature validation, event deserialization, event dispatch"
      Priority: 3

    - BranchId: "branch-B17-resilience"
      BranchName: "Resilience and Retry"
      Description: "Polly retry policy: exponential backoff 3 retries on 5xx; circuit breaker"
      Priority: 4

  BranchCount: 4

---

## N04 — DomainProcessorUnit

```
source_id: xiigen-b17-n04
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
node: 04
```

A: [CHARGE-HANDLER WorkBranch]
  BranchId: "branch-B17-charge"

B:
  BranchId: "branch-B17-charge"
  ProcessedItems:
    - ItemId: "impl-B17-CH-001"
      Name: "IStripeChargeHandler interface"
      Specification: "OperationResult<ChargeResult> ChargeAsync(ChargeRequest request, CancellationToken ct)"

    - ItemId: "impl-B17-CH-002"
      Name: "ChargeRequest"
      Specification: "{ AmountMinorUnits: long, CurrencyCode: string(ISO-4217), CustomerId: string, IdempotencyKey: string(required) }"
      BusinessRule: "BR-STR-001: IdempotencyKey must be set by caller; StripeConnector does not generate it"

    - ItemId: "impl-B17-CH-003"
      Name: "ChargeResult"
      Specification: "{ PaymentIntentId: string, Status: ChargeStatus, AmountCaptured: long, ErrorCode: string? }"

    - ItemId: "impl-B17-CH-004"
      Name: "PCI-DSS guard"
      Specification: "ChargeRequest must never contain CardNumber, CVV, or raw PAN — only Stripe PaymentMethodId"
      BusinessRule: "BR-STR-002: Validation rejects ChargeRequest with raw card data fields; OperationResult.Fail with PAYMENT_PCI_VIOLATION"

  OverallBranchStatus: "PROCESSED"

---

## N05 — ApplierUnit

```
source_id: xiigen-b17-n05
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
node: 05
```

A: [ProcessedBranchResult from N04]
  BranchId: "branch-B17-charge"

B:
  BranchId: "branch-B17-charge"
  AppliedItems:
    - ActionId: "apply-B17-A01"
      Action: "Generate IStripeChargeHandler and StripeChargeHandler with PaymentIntent flow"
      Result: "Generated: Connectors/Stripe/IStripeChargeHandler.cs, StripeChargeHandler.cs"

    - ActionId: "apply-B17-A02"
      Action: "Generate ChargeRequest and ChargeResult records with PCI guard validation"
      Result: "Generated: Connectors/Stripe/Models/ChargeRequest.cs, ChargeResult.cs"

    - ActionId: "apply-B17-A03"
      Action: "Register BR-STR-001 and BR-STR-002 in domain rule registry"
      Result: "Rules registered"

  AppliedCount: 3
  BranchStatus: "APPLIED"

---

## N06 — ResultTranslatorUnit

```
source_id: xiigen-b17-n06
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
node: 06
```

A: [All 4 AppliedBranchResults]

B:
  ResultFragments:
    - FragmentId: "frag-B17-connector-summary"
      FragmentType: "integration_connector_summary"
      Content:
        connector_name: "StripeConnector"
        files_generated: 9
        handlers_implemented: 3
        business_rules: 4
        resilience_policies: ["exponential_backoff_3_retries", "circuit_breaker"]
        pci_constraints: ["no_raw_card_data", "idempotency_key_required"]

    - FragmentId: "frag-B17-test-surface"
      FragmentType: "test_surface"
      Content:
        testable_units:
          - "ChargeAsync — happy path, idempotency replay, PCI violation rejection, 5xx retry"
          - "RefundAsync — full refund, partial refund, missing PaymentIntentId rejection"
          - "WebhookAsync — valid signature, invalid signature rejection, unknown event type"
        total_test_cases_recommended: 17

  FragmentCount: 2

---

## N07 — ArbiterSelectorUnit

```
source_id: xiigen-b17-n07
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
node: 07
```

A: [2 ResultFragments]

B:
  ArbiterRequests:
    - RequestId: "arb-B17-001"
      ArbiterType: "integration-completeness-arbiter"
      FragmentId: "frag-B17-connector-summary"
      ReviewFocus: "Verify all 3 handlers implemented, PCI constraints enforced, resilience policies present"

    - RequestId: "arb-B17-002"
      ArbiterType: "testability-arbiter"
      FragmentId: "frag-B17-test-surface"
      ReviewFocus: "Verify webhook signature validation and PCI violation path both have identified test cases"

  RequestCount: 2

---

## N08 — ArbiterUnit

```
source_id: xiigen-b17-n08
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
node: 08
```

A: [integration-completeness-arbiter]
  RequestId: "arb-B17-001"

B:
  RequestId: "arb-B17-001"
  Verdict: "PASS"
  Confidence: 0.95
  Findings:
    - "ChargeHandler: implemented with idempotency and PCI guard — PASS"
    - "RefundHandler: implemented with partial refund support — PASS"
    - "WebhookHandler: implemented with HMAC-SHA256 validation — PASS"
    - "Resilience: exponential backoff + circuit breaker registered — PASS"
    - "PCI-DSS: BR-STR-002 prevents raw card data storage — PASS"
  VerdictReason: "All 3 handlers implemented. PCI constraints and resilience policies in place."

---

## N09 — FormatterUnit

```
source_id: xiigen-b17-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node: 09
```

A:
  ReviewedResultSet:
    OverallVerdict: "PASS"

B:
  HumanResult:
    Title: "StripeConnector — payment integration complete"
    OverallVerdict: "PASS"
    Summary: "Stripe payment connector built with charge, refund, and webhook handlers. PCI-DSS constraints enforced. Resilience: exponential backoff (3 retries) and circuit breaker."
    FilesGenerated:
      - "Connectors/Stripe/IStripeConnector.cs"
      - "Connectors/Stripe/StripeChargeHandler.cs, StripeRefundHandler.cs, StripeWebhookHandler.cs"
      - "Connectors/Stripe/Models/ChargeRequest.cs, ChargeResult.cs, RefundRequest.cs, RefundResult.cs"
      - "Connectors/Stripe/Resilience/StripeRetryPolicy.cs"
    TestSurface: "17 recommended test cases"
  Metrics:
    node_01_unit_id: "DomainSplitterUnit"
    node_02_unit_id: "DomainTranslatorUnit"
    node_03_unit_id: "SchemePlannerUnit"
    node_04_unit_id: "DomainProcessorUnit"
    node_05_unit_id: "ApplierUnit"
    node_06_unit_id: "ResultTranslatorUnit"
    node_07_unit_id: "ArbiterSelectorUnit"
    node_08_unit_id: "ArbiterUnit"
    node_09_unit_id: "FormatterUnit"
    handlers_implemented: 3
    files_generated: 9
    sub_phase: "4.3"
    flow_id: "IntegrationConnectorFlow43"
