# Layer 0-C — Knowledge-Edge Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-014-001 — boundary: "wireframe" IS UI — normal path

```
source_id: xiigen-b01-0c-014-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Produce a wireframe for the notification center's empty state."
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

## B01-0C-014-002 — gap: "onboarding flow" — no UI lexicon terms

```
source_id: xiigen-b01-0c-014-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Streamline the onboarding flow to get users to their first value moment faster."
  CompiledPredicateIds: []
  Note: "onboarding flow" — "flow" not in lexicon. "streamline" not in IMPLEMENT.

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-onboarding-flow-001"
    capability_gap_description: "Vague onboarding improvement intent. No UI, DOCUMENT, IMPLEMENT, or ARCHITECTURE terms."
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
    next_flow_id: "null"

---

## B01-0C-014-003 — boundary: "prototype" IS UI in B01 — normal path

```
source_id: xiigen-b01-0c-014-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Create a clickable prototype for the checkout screen flow."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Note: "prototype" fires UI (B01 rule — prototype IS in the UI lexicon for B01).
  "screen" also fires UI. "create" fires IMPLEMENT.
  RoutingDecision: [{1.1, UI}, {4.1, IMPLEMENT}]

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"
    next_flow_id: "universal-nine-node-development-flow-1.1"

B01 rule for prototype:
  "prototype" IS in the UI lexicon for B01. This contrasts with B32 where prototype is gap vocabulary.
  Model must apply the batch-specific lexicon, not a generic one.
