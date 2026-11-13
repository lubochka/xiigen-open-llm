# Layer 0-C — Knowledge-Edge Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-016-001 — boundary: "form" IS UI in search-and-filter context

```
source_id: xiigen-b01-0c-016-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Catalogue all form fields in the advanced search panel."
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

---

## B01-0C-016-002 — gap: "search algorithm" — ARCHITECTURE not UI

```
source_id: xiigen-b01-0c-016-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Improve the search algorithm ranking logic to prioritize recent items."
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: "logic" fires IMPLEMENT. "algorithm" fires neither IMPLEMENT nor ARCHITECTURE.
  "ranking" not in any lexicon. RoutingDecision: [{4.1, IMPLEMENT}]

B:
  FlowId: "universal-nine-node-development-flow-4.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"
    next_flow_id: "universal-nine-node-development-flow-4.1"

Note: "logic" fires IMPLEMENT → routes to 4.1 (implementation). Not a knowledge-edge case.

---

## B01-0C-016-003 — gap: "card sorting" UX research — no lexicon terms

```
source_id: xiigen-b01-0c-016-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Run a card sorting exercise to determine the optimal category hierarchy for the filter panel."
  CompiledPredicateIds: []
  Note: "card sorting", "exercise", "optimal", "category hierarchy" — none in any lexicon.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-card-sorting-001"
    capability_gap_description: "UX research methodology (card sorting) has no registered predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
