# Layer 0-C — Knowledge-Edge Pairs (019)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-019-001 — boundary: "screen" IS UI for order history

```
source_id: xiigen-b01-0c-019-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "List all order tracking screen states for the mobile view."
  CompiledPredicateIds: ["UI"]
  RoutingDecision:
    PredictedSubPhaseId: "1.1"
    KnowledgeEdge: false

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"

---

## B01-0C-019-002 — gap: "order fulfillment logic" — IMPLEMENT + no UI

```
source_id: xiigen-b01-0c-019-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Automate the order fulfillment logic to update order status via webhook."
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: "logic" fires IMPLEMENT. "automate" not in IMPLEMENT but "logic" is sufficient.
  No UI terms present.
  RoutingDecision: [{4.1, IMPLEMENT}]

B:
  FlowId: "universal-nine-node-development-flow-4.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"
    next_flow_id: "universal-nine-node-development-flow-4.1"

Note: routes to 4.1, not knowledge-edge. IMPLEMENT fires on "logic".

---

## B01-0C-019-003 — gap: "delivery ETA" — no predicate terms

```
source_id: xiigen-b01-0c-019-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Show an accurate estimated delivery date on the tracking page... wait — 'page' fires UI."

Corrected A:
  CompiledText: "Show an accurate estimated delivery time to customers after an order ships."
  CompiledPredicateIds: []
  Note: "estimated delivery time", "customers", "ships" — no predicate terms.
  "show" NOT in IMPLEMENT lexicon.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-delivery-eta-001"
    capability_gap_description: "Delivery ETA display intent. No UI term in text (no 'page', 'screen', 'interface', etc.). No other predicate fires."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
