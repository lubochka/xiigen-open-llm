# Held-Out and Corruption Pairs (010)
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-06 | pair_kind: held_out | corruption

---

## Method note

File 010 covers HO-020 (use-case structure completeness), COR-012 (v6-01: incomplete use case — missing alternative flows).

---

## Held-Out Pair B06B1-WDA-HO-020 — use case output completeness

```
source_id: xiigen-b06-held-out-020
pair_kind: held_out
```

A:
  run_id: "B06-ho-020"
  user_request: "Define the checkout use cases for guest checkout, covering the happy path, payment failure, and session timeout scenarios."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

Expected B (N09 FormatterUnit output):
  FlowId: "requirements-authoring-flow-2.1"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Guest checkout use cases:

    UC-GC-001 Happy Path: Guest selects checkout, provides shipping address, payment
      method, and order summary review; places order successfully.
      Preconditions: cart not empty; valid shipping zone.
      Main Flow: 6 steps (cart review → address → payment → review → confirm → confirmation email).
      Postconditions: order created; payment authorized; confirmation email sent.

    UC-GC-002 Payment Failure: Payment authorization fails; guest presented with retry
      option and alternative payment method. BR-GC-01 (max 3 retries).

    UC-GC-003 Session Timeout: Guest session expires mid-checkout; cart preserved for
      30 minutes on return. BR-GC-02 (cart recovery timeout window).

    Acceptance criteria: 9 Given/When/Then assertions covering all 3 scenarios.

  Metrics:
    use_case_count: "3"
    acceptance_criteria_count: "9"
    business_rule_count: "2"
  Warnings: []

Arbiter reviews:
  N08-review-1: PASS — all 3 use cases have Main Flow and Alternative Flows
  N08-review-2: PASS — all 9 criteria are independently testable

---

## Held-Out Pair B06B1-WDA-HO-021 — nine-node flow with N04/N05 content

```
source_id: xiigen-b06-held-out-021
pair_kind: held_out
```

A:
  run_id: "B06-ho-021"
  scenario: "GuestCheckoutSpec"
  sub_phase_id: "2.1"
  node: "N04-N05"

Expected B (N04 DomainProcessor + N05 Applier):
  N04_DomainProcessor:
    BranchId: "B06-ho-021-branch-happy-path"
    ProcessedResult: "UC-GC-001 main flow: 6-step sequence from cart review to confirmation email"
    EvidenceApplied: ["happy path", "guest checkout", "payment authorization"]
    ProcessingNotes: "Each step includes actor action and system response"

  N05_Applier:
    AppliedResultId: "B06-ho-021-applied-001"
    Schema: "UseCase"
    Fields:
      UseCaseId: "UC-GC-001"
      Actor: "Guest User"
      Preconditions: ["Cart contains at least one item", "Shipping address within valid zone"]
      MainFlow: ["Step 1: Guest reviews cart", "Step 2: Guest enters shipping address", "Step 3: Guest selects payment method", "Step 4: Guest reviews order summary", "Step 5: Guest confirms order", "Step 6: System sends confirmation email"]
      AlternativeFlows: ["AF-1: Payment failure (BR-GC-01)", "AF-2: Session timeout (BR-GC-02)"]
      Postconditions: ["Order record created", "Payment authorized", "Confirmation email sent within 2 minutes"]

---

## Corruption Pair B06B1-WDA-COR-012 — v6-01: use case missing AlternativeFlows

```
source_id: xiigen-b06-corruption-012
pair_kind: corruption
corruption_type: Corruption-v6-01-incomplete-use-case
```

A:
  DocumentId: "B06-ho-020-prompt"
  CompiledText: "Define guest checkout use cases covering happy path, payment failure, and session timeout."
  SourceStatus: "N05 Applier output"

B-correct:
  UseCaseId: "UC-GC-001"
  Actor: "Guest User"
  Preconditions: ["Cart not empty", "Valid shipping zone"]
  MainFlow: ["6 steps"]
  AlternativeFlows: ["AF-1: Payment failure", "AF-2: Session timeout"]
  Postconditions: ["Order created", "Payment authorized", "Email sent"]

B-corrupt:
  UseCaseId: "UC-GC-001"
  Actor: "Guest User"
  Preconditions: ["Cart not empty"]
  MainFlow: ["6 steps"]
  AlternativeFlows: []
  Postconditions: ["Order created"]

Corruption type: v6-01 — incomplete use case; AlternativeFlows section empty despite
explicit request for "payment failure" and "session timeout" scenarios.
Why harmful: N08 Arbiter review-1 (use case completeness) will FAIL because AlternativeFlows
are required when alternative scenarios are specified in the request. The N09 FormatterUnit
will output an incomplete use case document. Downstream: payment failure handling and session
timeout recovery are not documented, leading to implementation gaps.
The model must populate AlternativeFlows for every scenario explicitly mentioned in the request.
