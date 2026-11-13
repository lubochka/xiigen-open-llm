# Layer 0-C — Knowledge-Edge Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-013-001 — boundary: "navigation" is gap; "button" is UI

```
source_id: xiigen-b01-0c-013-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A (gap case — navigation only):
  CompiledText: "Review the navigation flow between the profile page and the settings area."
  CompiledPredicateIds: ["UI"]
  Note: "page" fires UI even though "navigation" does not.

Wait — corrected analysis:
  "page" IS in the UI lexicon → CompiledPredicateIds: ["UI"] → normal path, not knowledge-edge.
  The knowledge-edge fires only when ALL terms are gap vocabulary with no UI lexicon hit.

A (true gap — navigation only, no UI lexicon terms):
  CompiledText: "Evaluate the navigation flow between the settings area and the account section."
  CompiledPredicateIds: []
  Note: "navigation", "flow", "settings area", "account section" — none in UI lexicon.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-navigation-flow-001"
    capability_gap_description: "'navigation' and 'flow' are not in the UI lexicon. No predicate fires."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

---

## B01-0C-013-002 — boundary: "button" IS UI — normal path

```
source_id: xiigen-b01-0c-013-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Map all button interactions in the checkout confirmation screen."
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
    capability_gap_id: "null"
    recommended_action: "EXECUTE_FLOW"
    next_flow_id: "universal-nine-node-development-flow-1.1"

Normal-path rationale:
  "button" fires UI; "screen" fires UI. Two UI terms present.
  Normal path executes. FLOW_EXECUTING, not KNOWLEDGE_EDGE.

---

## B01-0C-013-003 — boundary: "menu" is gap; "modal" is UI

```
source_id: xiigen-b01-0c-013-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A (gap — menu only):
  CompiledText: "Redesign the dropdown menu to be more compact and mobile-friendly."
  CompiledPredicateIds: []
  Note: "menu" NOT in UI lexicon. "mobile-friendly" NOT in lexicon. "redesign" NOT in IMPLEMENT.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-menu-redesign-001"
    capability_gap_description: "'menu' is not in the UI lexicon. 'dropdown menu' has no predicate."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"
