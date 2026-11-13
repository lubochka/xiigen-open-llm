# Layer 0-D — Flow Factory Selection Pairs (029)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-029-001 — BreadcrumbNav: knowledge-edge path has no factory invocation

```
source_id: xiigen-b01-0d-029-001
source_license_state: first_party
pair_kind: flow_factory
```

A: CompiledPredicateIds: [], scenario: "Add breadcrumb navigation to every page"
   KnowledgeEdge: true

B: No factory invocation occurs.
   Result: OverallVerdict: "KNOWLEDGE_EDGE" | FlowId: "knowledge-edge-missing-flow"
   Factory is bypassed entirely — no TrustState evaluation performed.

Rule: factory only runs when at least one predicate fires. CompiledPredicateIds: [] bypasses factory.

---

## B01-0D-029-002 — TabNavigation: trusted_learned for sub-phase 1.1

```
source_id: xiigen-b01-0d-029-002
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "trusted_learned", scenario: "TabNavigation component"
B: SelectedFlowId: "screen-inventory-flow-1.1-v3" | SelectionOutcome: "TRUSTED_LEARNED_SELECTED" | FallbackUsed: false | Warnings: []

---

## B01-0D-029-003 — ReviewWidget: probationary rejected for 1.1, accepted for 4.1

```
source_id: xiigen-b01-0d-029-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build the review widget and define the database schema."
CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE"]

Factory[1.1]: trust_state: "probationary_rejected", gate_b_passed: false
  → SelectedFlowId: "universal-nine-node-development-flow-1.1" | FallbackUsed: true | REJECTED_PROBATIONARY_FALLBACK_USED

Factory[4.1]: trust_state: null
  → SelectedFlowId: "universal-nine-node-development-flow-4.1" | FallbackUsed: false | DETERMINISTIC_GENERIC_SELECTED

Factory[3.1]: trust_state: null
  → SelectedFlowId: "universal-nine-node-development-flow-3.1" | FallbackUsed: false | DETERMINISTIC_GENERIC_SELECTED

Note: FallbackUsed applies independently per factory invocation. One rejection does not affect other sub-phases.
