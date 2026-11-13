# Layer 0-C — Knowledge-Edge Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-020-001 — boundary: "layout" IS UI — normal path for address book

```
source_id: xiigen-b01-0c-020-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Define the layout for address cards in the address book on mobile."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Note: "layout" fires UI. "define" fires DOCUMENT.
  RoutingDecision: [{1.1, UI}, {2.1, DOCUMENT}]

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"

---

## B01-0C-020-002 — gap: "address validation API" — no UI terms

```
source_id: xiigen-b01-0c-020-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Integrate an address validation API to verify postal codes at checkout."
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: "integrate" NOT in IMPLEMENT. "API" not in any lexicon.
  Wait — rechecking: no IMPLEMENT terms present. "integrate" is not "implement".
  CompiledPredicateIds: []

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-address-validation-api-001"
    capability_gap_description: "'integrate' is not in the IMPLEMENT lexicon. 'API' has no predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

---

## B01-0C-020-003 — boundary: "responsive" IS UI — normal path for delivery options

```
source_id: xiigen-b01-0c-020-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Catalogue responsive design behaviors for the delivery options selection panel."
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
    next_flow_id: "universal-nine-node-development-flow-1.1"
