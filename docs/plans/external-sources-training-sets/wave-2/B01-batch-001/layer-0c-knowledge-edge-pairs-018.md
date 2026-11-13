# Layer 0-C — Knowledge-Edge Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-018-001 — boundary: "visual" IS UI for wishlist

```
source_id: xiigen-b01-0c-018-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Map the visual hierarchy of the wishlist page thumbnail grid."
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

## B01-0C-018-002 — gap: "wishlist sharing" — feature logic, no UI terms

```
source_id: xiigen-b01-0c-018-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Allow users to share their wishlist via a unique shareable link."
  CompiledPredicateIds: []
  Note: "share", "wishlist", "unique link" — none in any predicate lexicon.
  "allow" NOT in IMPLEMENT. "shareable link" not a predicate term.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-wishlist-sharing-001"
    capability_gap_description: "Wishlist sharing feature intent. No registered predicate terms."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

---

## B01-0C-018-003 — boundary: "accessibility" IS UI — normal path

```
source_id: xiigen-b01-0c-018-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Audit the accessibility of the SavedItems component for keyboard users."
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
