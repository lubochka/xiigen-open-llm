# Layer 2 — Nine-Node 3.3 Pairs (013)
# Batch: B12-batch-001 | Scenario: B12 — REST and GraphQL API contract definition
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-013 — OrderWebhookContract: API_CONTRACT alone (documentation TransitionType)

```
source_id: xiigen-b12-l2-013
flow_name: OrderWebhookContract
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: PASS
```

### Input (N01)

N01 classification:
  user_request: "Specify the order webhook contract for shipment and cancellation events."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "OrderWebhookContract"
  SubPhase: "3.3"
  SelectedLogicalTransition: A = SelectedLogicalTransition (N01 primary input)
  TriggerTerms:
    API_CONTRACT: "webhook", "contract"
    DOCUMENT: "specify"

### Processing (N02-N08)

N02 — DomainTranslatorUnit:
  Domain: order management, event-driven webhooks
  Contract type: webhook event contract
  Events scope: shipment confirmation, order cancellation

N03 — SchemePlannerUnit:
  WorkBranches:
    - branch-001: order.shipped event schema
    - branch-002: order.cancelled event schema
    - branch-003: webhook delivery specification (retry, signature)

N04 — DomainProcessorUnit:
  order.shipped: { orderId, customerId, shipmentId, carrier, trackingNumber, shippedAt }
  order.cancelled: { orderId, customerId, cancellationReason, cancelledAt, refundEligible }
  delivery: at-least-once, HMAC-SHA256 signature, X-Webhook-Signature header, 3 retries

N05 — ApplierUnit:
  Apply: webhook-order-contract.json (event schema definitions)
  Apply: X-Webhook-Signature header specification
  Apply: retry policy documentation

N06 — ResultTranslatorUnit:
  events_defined: 2 (order.shipped, order.cancelled)
  signature_method: HMAC-SHA256
  delivery_guarantee: at-least-once

N07 — ArbiterSelectorUnit:
  Request: api-contract-completeness-arbiter
  Focus: all events have schemas, delivery spec complete

N08 — ArbiterUnit:
  Verdict: PASS
  Findings: "2 event types defined with complete schemas; delivery spec documented"

### Output (N09)

N09 — FormatterUnit:
  FlowStatus: "PASS"
  Artifacts:
    1. webhook-order-contract.json (event contract with schemas)
    2. webhook-integration-guide.md (from DocumentationFlow21)
  GateB: PASS — "webhook-order-contract.json defines ≥2 order event types with complete schemas"
  TransitionType: "documentation"
