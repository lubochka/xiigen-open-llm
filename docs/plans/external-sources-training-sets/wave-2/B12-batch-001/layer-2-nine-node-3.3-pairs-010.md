# Layer 2 — Nine-Node 3.3 Pairs (010)
# Batch: B12-batch-001 | Scenario: B12 — Development — API contract documentation (REST and GraphQL)
# Production date: 2026-05-05 | pair_kind: nine_node

---

## B12-L2-010 — EventBusApiContract: probationary_rejected scenario with Gate B failure → fallback

```
source_id: xiigen-b12-l2-010
flow_name: EventBusApiContract
transition_type: documentation
phase: 3
sub_phase: 3.3
pair_kind: nine_node
gate_b: FALLBACK
flow_type: probationary_rejected → deterministic_generic
```

### Input (N01)

N01 classification:
  user_request: "Define the event bus API contract and specify the message schemas for service events."
  CompiledPredicateIds: ["API_CONTRACT", "DOCUMENT"]
  TransitionType: "documentation"
  DomainId: "software-development"
  FlowId: "EventBusApiContract"
  SubPhase: "3.3"
  FlowSelection:
    Attempted: "EventBusApiContract-learned-v0-FAILED" (probationary_rejected)
    GateB: FAIL (v6-01 corruption: message schema without API co-occurrence → API_CONTRACT suppressed)
    Fallback: "APIContractFlow33" (deterministic_generic)
  TriggerTerms:
    API_CONTRACT: "api", "contract"
    DOCUMENT: "define", "specify"

### Processing (N02-N08) — via deterministic_generic fallback

N02 — Domain analysis:
  Domain: event-driven messaging bus
  API style: message contract (event-driven)
  Events: order events, inventory events, notification events, payment events

N03 — Event taxonomy design:
  order.created, order.updated, order.cancelled
  inventory.adjusted, inventory.low-stock-alert
  notification.triggered, notification.delivered
  payment.completed, payment.failed

N04 — Message contract design per event:
  BaseEvent: { eventId, eventType, timestamp, version, sourceService }
  OrderCreatedEvent extends Base: { orderId, customerId, items, totalAmount }
  InventoryAdjustedEvent extends Base: { productId, adjustment, newQuantity }
  PaymentCompletedEvent extends Base: { paymentId, orderId, amount, currency }

N05 — Event bus routing contract:
  Topic naming: {domain}.{entity}.{action} (e.g., order.order.created)
  Consumer groups: each service subscribes to relevant topics
  Retention: 7 days
  Partition strategy: by entity ID for ordering guarantees

N06 — Message schema documentation:
  All event schemas with field descriptions
  Producer/consumer mapping
  Ordering guarantees per topic

N07 — Validation (deterministic_generic):
  All event types have schemas: YES
  Topic routing defined: YES
  Contract complete for deterministic_generic flow: YES

N08 — Documentation assembly:
  Event bus contract documentation
  Message schema reference

### Output (N09)

N09 — Result:
  FlowStatus: "PASS (via fallback)"
  FallbackUsed: true
  FailedFlow: "EventBusApiContract-learned-v0-FAILED"
  FailureReason: "v6-01 corruption: 'message schema' suppressed API_CONTRACT (wrong)"
  FallbackFlow: "APIContractFlow33 (deterministic_generic)"
  Artifacts:
    1. event-bus-api-contract.md (message contract specification)
    2. event-schemas-reference.md (all event type schemas)
  FilesGenerated: 2
  GateB: FALLBACK — deterministic_generic used after probationary_rejected
  TransitionType: "documentation"

Teaching note: The probationary_rejected flow failed because it classified "message schema" as
non-API context (v6-01 corruption). Fallback to deterministic_generic produced correct output.
This pair demonstrates that probationary_rejected → deterministic_generic fallback works correctly.
