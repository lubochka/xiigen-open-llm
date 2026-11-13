# Layer 0-C — Knowledge-Edge Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-015-001 — gap: "sidebar" — not in any lexicon

```
source_id: xiigen-b01-0c-015-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Collapse the sidebar to improve focus on the main content area."
  CompiledPredicateIds: []
  Note: "sidebar" NOT in UI lexicon. "collapse" NOT in IMPLEMENT. "main content area" is context.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-sidebar-collapse-001"
    capability_gap_description: "'sidebar' is not in the UI lexicon. No IMPLEMENT verb present."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

---

## B01-0C-015-002 — boundary: "spacing" IS UI — normal path

```
source_id: xiigen-b01-0c-015-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Standardize the spacing between all sections of the settings dashboard."
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

## B01-0C-015-003 — gap: "user experience" without lexicon terms

```
source_id: xiigen-b01-0c-015-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Improve the overall user experience of the cart to reduce abandonment."
  CompiledPredicateIds: []
  Note: "user experience" — "experience" is NOT in the UI lexicon (only "ux" fires UI, not "experience").
  "improve" NOT in IMPLEMENT. "abandonment" not in any lexicon.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-ux-experience-001"
    capability_gap_description: "'experience' is NOT the same as 'ux'. Only 'ux' is in the UI lexicon."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

Critical distinction:
  "ux" → UI fires.
  "user experience" (longform) → UI does NOT fire. "experience" alone is not in the lexicon.
  Only exact lexicon term matches trigger predicates.
