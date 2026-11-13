# Layer 2 — Nine-Node Flow Pairs (sub-phase 4.1)
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Flow: BackendServiceFlow41
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node

---

## N01 — DomainSplitterUnit

```
source_id: xiigen-b15-n01
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
node: 01
```

A:
  TransitionId: "transition-B15-t1-001-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  UserRequest: "Build an order processing service from scratch including order creation, status tracking, and fulfillment logic."
  PredicateIds: ["IMPLEMENT"]

B:
  DomainPackets:
    - PacketId: "pkt-B15-001-impl"
      Domain: "IMPLEMENT"
      Focus: "backend service construction from scratch"
      InputSummary: "Order processing service: order creation, status management, fulfillment logic"
      PredicateOwner: "IMPLEMENT"
  PacketCount: 1

---

## N02 — DomainTranslatorUnit

```
source_id: xiigen-b15-n02
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
node: 02
```

A: [IMPLEMENT DomainPacket from N01]
  PacketId: "pkt-B15-001-impl"
  Domain: "IMPLEMENT"
  Focus: "backend service construction from scratch"
  InputSummary: "Order processing service: order creation, status management, fulfillment logic"

B:
  StatementId: "stmt-B15-001-impl"
  DomainStatement: "Implement OrderProcessingService from scratch in C#. Three functional
    domains: (1) Order Creation — accept order request, validate line items, persist to
    OrderRepository, emit OrderCreated event; (2) Status Management — update order state
    machine (Pending→Confirmed→Shipped→Delivered, with Cancelled branch); (3) Fulfillment
    Logic — route order to warehouse, decrement inventory, generate packing slip.
    Non-functional: idempotent CreateOrder (duplicate order IDs rejected), transactional
    status updates, structured logging per order lifecycle event."
  SourcePacketId: "pkt-B15-001-impl"
  TranslationConfidence: 0.92

---

## N03 — SchemePlannerUnit

```
source_id: xiigen-b15-n03
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node: 03
```

A: [TranslatedDomainStatement from N02]

B:
  WorkBranches:
    - BranchId: "branch-B15-order-creation"
      BranchName: "Order Creation Domain"
      Description: "IOrderService.CreateOrder: validate, persist, emit event, return OrderId"
      Priority: 1

    - BranchId: "branch-B15-status-management"
      BranchName: "Status State Machine"
      Description: "OrderStateMachine: state transitions, guard conditions, transition events"
      Priority: 2

    - BranchId: "branch-B15-fulfillment"
      BranchName: "Fulfillment Logic"
      Description: "FulfillmentRouter: warehouse routing, inventory decrement, packing slip generation"
      Priority: 3

    - BranchId: "branch-B15-repository"
      BranchName: "Data Access Layer"
      Description: "OrderRepository: CRUD operations, idempotency enforcement, transaction support"
      Priority: 4

  BranchCount: 4

---

## N04 — DomainProcessorUnit

```
source_id: xiigen-b15-n04
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
node: 04
```

A: [ORDER-CREATION WorkBranch]
  BranchId: "branch-B15-order-creation"
  BranchName: "Order Creation Domain"

B:
  BranchId: "branch-B15-order-creation"
  ProcessedItems:
    - ItemId: "impl-B15-OC-001"
      Name: "IOrderService interface"
      Specification: "OperationResult<OrderId> CreateOrder(CreateOrderRequest request)"
      BusinessRule: "BR-ORD-001: OrderId is a deterministic UUID(v5) from CustomerId + CartId — ensures idempotency"

    - ItemId: "impl-B15-OC-002"
      Name: "CreateOrderRequest validation"
      Specification: "LineItems.Count >= 1; each LineItem: ProductId not null, Quantity > 0, UnitPrice > 0"
      BusinessRule: "BR-ORD-002: ValidationFailure returns OperationResult.Fail with error code ORDER_INVALID"

    - ItemId: "impl-B15-OC-003"
      Name: "OrderCreated domain event"
      Specification: "Emitted after successful persist; contains OrderId, CustomerId, TotalAmount, LineItems[]"
      BusinessRule: "BR-ORD-003: Event emission is fire-and-forget; persist must complete before emit"

    - ItemId: "impl-B15-OC-004"
      Name: "Duplicate order guard"
      Specification: "If OrderId already exists in OrderRepository, return OperationResult.Ok with existing OrderId (idempotent)"
      BusinessRule: "BR-ORD-004: Duplicate detection is pre-persist; no second write occurs"

  OverallBranchStatus: "PROCESSED"

---

## N05 — ApplierUnit

```
source_id: xiigen-b15-n05
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
node: 05
```

A: [ProcessedBranchResult from N04 — order creation branch]
  BranchId: "branch-B15-order-creation"

B:
  BranchId: "branch-B15-order-creation"
  AppliedItems:
    - ActionId: "apply-B15-A01"
      Action: "Generate IOrderService interface with CreateOrder signature"
      Result: "Interface generated: Services/IOrderService.cs"

    - ActionId: "apply-B15-A02"
      Action: "Generate OrderProcessingService implementing IOrderService with validation, persist, event emit"
      Result: "Service generated: Services/OrderProcessingService.cs"

    - ActionId: "apply-B15-A03"
      Action: "Generate CreateOrderRequest record with validation annotations"
      Result: "Record generated: Models/CreateOrderRequest.cs"

    - ActionId: "apply-B15-A04"
      Action: "Register business rules BR-ORD-001 through BR-ORD-004 in domain documentation"
      Result: "Rules registered"

  AppliedCount: 4
  BranchStatus: "APPLIED"

---

## N06 — ResultTranslatorUnit

```
source_id: xiigen-b15-n06
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
node: 06
```

A: [All 4 AppliedBranchResults]

B:
  ResultFragments:
    - FragmentId: "frag-B15-implementation-summary"
      FragmentType: "implementation_summary"
      Content:
        service_name: "OrderProcessingService"
        files_generated: 8
        interfaces_defined: 2
        business_rules_registered: 6
        domain_events_defined: 2
        state_machine_states: ["Pending", "Confirmed", "Shipped", "Delivered", "Cancelled"]
        state_transitions: 6

    - FragmentId: "frag-B15-test-surface"
      FragmentType: "test_surface"
      Content:
        testable_units:
          - "CreateOrder — 4 cases: valid, duplicate, invalid line items, zero items"
          - "State machine — 6 transition cases + 3 invalid transition rejections"
          - "FulfillmentRouter — 2 warehouse routing cases + 1 out-of-stock rejection"
        total_test_cases_recommended: 23

  FragmentCount: 2

---

## N07 — ArbiterSelectorUnit

```
source_id: xiigen-b15-n07
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
node: 07
```

A: [2 ResultFragments]

B:
  ArbiterRequests:
    - RequestId: "arb-B15-001"
      ArbiterType: "implementation-completeness-arbiter"
      FragmentId: "frag-B15-implementation-summary"
      ReviewFocus: "Verify all 3 functional domains are covered (creation, status, fulfillment) and business rules are registered"

    - RequestId: "arb-B15-002"
      ArbiterType: "testability-arbiter"
      FragmentId: "frag-B15-test-surface"
      ReviewFocus: "Verify all public methods have at least one identified test case including the happy path and one error case"

  RequestCount: 2

---

## N08 — ArbiterUnit

```
source_id: xiigen-b15-n08
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
node: 08
```

A: [implementation-completeness-arbiter request]
  RequestId: "arb-B15-001"
  ReviewFocus: "Verify all 3 functional domains covered and business rules registered"

B:
  RequestId: "arb-B15-001"
  Verdict: "PASS"
  Confidence: 0.94
  Findings:
    - "Order Creation domain: covered (IOrderService, OrderProcessingService, CreateOrderRequest) — PASS"
    - "Status State Machine: covered (OrderStateMachine, 5 states, 6 transitions) — PASS"
    - "Fulfillment Logic: covered (FulfillmentRouter, warehouse routing, inventory) — PASS"
    - "Business rules: 6 registered (BR-ORD-001 through BR-ORD-006) — PASS"
    - "Domain events: 2 defined (OrderCreated, OrderStatusChanged) — PASS"
  VerdictReason: "All three functional domains implemented with interfaces, business rules, and domain events."

---

## N09 — FormatterUnit

```
source_id: xiigen-b15-n09
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
node: 09
```

A:
  ReviewedResultSet:
    Fragments: ["frag-B15-implementation-summary", "frag-B15-test-surface"]
    Verdicts: ["arb-B15-001:PASS", "arb-B15-002:PASS"]
    OverallVerdict: "PASS"

B:
  HumanResult:
    Title: "OrderProcessingService — from-scratch implementation complete"
    OverallVerdict: "PASS"
    Summary: "Order processing service built from scratch. Three functional domains implemented: order creation (idempotent, event-driven), state machine (5 states, 6 transitions), and fulfillment routing."
    FilesGenerated:
      - "Services/IOrderService.cs — interface"
      - "Services/OrderProcessingService.cs — implementation"
      - "Models/CreateOrderRequest.cs — request record"
      - "Models/Order.cs — domain entity"
      - "StateMachines/OrderStateMachine.cs — state transitions"
      - "Repositories/IOrderRepository.cs — repository interface"
      - "Repositories/OrderRepository.cs — EF Core implementation"
      - "Events/OrderCreated.cs, OrderStatusChanged.cs — domain events"
    BusinessRules: ["BR-ORD-001 (idempotency)", "BR-ORD-002 (validation)", "BR-ORD-003 (event order)", "BR-ORD-004 (duplicate guard)", "BR-ORD-005 (inventory check)", "BR-ORD-006 (fulfillment routing)"]
    TestSurface: "23 recommended test cases across 3 test suites"
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
    files_generated: 8
    business_rules: 6
    sub_phase: "4.1"
    flow_id: "BackendServiceFlow41"
