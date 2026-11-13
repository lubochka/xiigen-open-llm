# Layer 1 — Transition Orchestrator Pairs (025)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B01-L1-025-001 — HeroSection: UI+IMPLEMENT → feature_addition, primary 1.1

```
source_id: xiigen-b01-l1-025-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Build a hero section component with a headline, subheading, and CTA button."
   CompiledPredicateIds: ["UI", "IMPLEMENT"]

B:
  TransitionId: "transition-B01-l1-025-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
  PredicateIds: ["UI", "IMPLEMENT"]
  ContextCarryForward: true

---

## B01-L1-025-002 — BreadcrumbNav gap: knowledge-edge orchestration path

```
source_id: xiigen-b01-l1-025-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A: user_request: "Add breadcrumb navigation to every page in the product catalog."
   CompiledPredicateIds: []

B:
  TransitionId: null
  TransitionType: null
  SubPhaseId: null
  FlowsSelected: ["knowledge-edge-missing-flow"]
  PredicateIds: []
  KnowledgeEdgePath: true
  ContextCarryForward: false

No SelectedLogicalTransition produced for knowledge-edge inputs. Layer 2 does not execute.
