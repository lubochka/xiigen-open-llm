# Layer 0-C — Knowledge-Edge Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## Method note (021)

Partial routing: UI fires but IMPLEMENT does not → TransitionType is documentation.
This file teaches the difference between:
  - UI present without IMPLEMENT → feature_addition (new UI artifact, no code)
  - DOCUMENT present without IMPLEMENT → documentation
  - Both UI and DOCUMENT, no IMPLEMENT → feature_addition (UI drives it)

---

## B01-0C-021-001 — partial routing: UI fires, IMPLEMENT does not — documentation TransitionType

```
source_id: xiigen-b01-0c-021-001
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Describe the interaction states for the profile modal — open, close, loading, and error."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Note: UI (modal, interaction). DOCUMENT (describe). No IMPLEMENT.
  RoutingDecision: [{1.1, UI}, {2.1, DOCUMENT}]
  TransitionType: feature_addition
    Rationale: UI present without IMPLEMENT → feature_addition (creating a new UI artifact description).

B (orchestrator output):
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  TransitionType resolved: feature_addition
  Metrics:
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"

Partial routing note:
  IMPLEMENT was absent → TransitionType is feature_addition because UI dominates.
  If ONLY DOCUMENT had fired (no UI), TransitionType would be documentation.

---

## B01-0C-021-002 — partial routing: DOCUMENT alone fires — documentation TransitionType

```
source_id: xiigen-b01-0c-021-002
source_license_state: first_party
pair_kind: knowledge_edge
```

A:
  CompiledText: "Document the behavioral specification for the delivery options selection step."
  CompiledPredicateIds: ["DOCUMENT"]
  Note: DOCUMENT (document, specification, behavioral → behavior root). No UI terms.
  RoutingDecision: [{2.1, DOCUMENT}]
  TransitionType: documentation (DOCUMENT without IMPLEMENT, no UI)

B (orchestrator output):
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING"
  TransitionType resolved: documentation
  Metrics:
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"

---

## B01-0C-021-003 — contrast: adding IMPLEMENT changes documentation to feature_addition

```
source_id: xiigen-b01-0c-021-003
source_license_state: first_party
pair_kind: knowledge_edge
```

A (with IMPLEMENT):
  CompiledText: "Write the behavioral specification for the delivery options selection step."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Note: DOCUMENT (specification, behavioral). IMPLEMENT (write).
  TransitionType: feature_addition (IMPLEMENT present)

B (orchestrator output):
  FlowId: "universal-nine-node-development-flow-4.1"
  OverallVerdict: "FLOW_EXECUTING"
  TransitionType resolved: feature_addition
  Metrics:
    matched_predicate_count: "2"
    recommended_action: "EXECUTE_FLOW"

Contrast teaching:
  Same topic. Adding "write" changes TransitionType from documentation to feature_addition.
  The verb "write" is the sole difference.
