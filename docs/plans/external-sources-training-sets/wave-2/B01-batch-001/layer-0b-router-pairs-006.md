# Layer 0-B — Router Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-006-001 — [UI, DOCUMENT, IMPLEMENT] → 1.1 + 2.1 + 4.1: three routing decisions

```
source_id: xiigen-b01-0b-006-001
source_title: XIIGen B01 router-006 — [UI, DOCUMENT, IMPLEMENT] three-branch routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-006-001]
  DocumentId: "B01-0a-006-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-006-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "User dashboard screen specifications and responsive layout component implementation"
  CompiledText: "Write the screen specifications for the user dashboard and implement the responsive layout components."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-006-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-006-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-006-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.30
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Routing rationale:
  UI (screen, responsive, layout, component) → 1.1. DOCUMENT (specification) → 2.1.
  IMPLEMENT (write, implement) → 4.1. Three independent decisions. The flows
  that will be invoked: ScreenInventoryFlow11 for 1.1, BusinessRequirementsFlow21
  for 2.1, ImplementationFlow41 for 4.1. All three run; outputs are combined.

---

## B01-0B-006-002 — Flow ordering for multi-branch results

```
source_id: xiigen-b01-0b-006-002
source_title: XIIGen B01 router-006 — flow invocation ordering for multi-branch decisions
source_license_state: first_party
pair_kind: router
coverage_status: covered
```

Rule annotation — flow ordering for multi-branch routing decisions:

  When the router produces multiple RoutingDecision entries, the TransitionOrchestrator
  (Layer 1) orders the flows for invocation as follows:
    Priority 1: UI → ScreenInventoryFlow11 (sub-phase 1.1)
    Priority 2: ARCHITECTURE → SystemArchitectureFlow31 (sub-phase 3.1)
    Priority 3: DOCUMENT → BusinessRequirementsFlow21 (sub-phase 2.1)
    Priority 4: IMPLEMENT → ImplementationFlow41 (sub-phase 4.1)

  This ordering is deterministic and applies to all B01 multi-predicate requests.
  The flows are invoked sequentially; each flow's output may be referenced by
  subsequent flows in the same batch.

---

## B01-0B-006-003 — [UI, DOCUMENT, IMPLEMENT] with flows named

```
source_id: xiigen-b01-0b-006-003
source_title: XIIGen B01 router-006 — three-predicate with named flows
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-006-002]
  DocumentId: "B01-0a-006-002-prompt"
  CompiledText: "Write the user stories for the dashboard screen interface and build the component library."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-006-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
      FlowHint: "ScreenInventoryFlow11"

    - DocumentId: "B01-0a-006-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.34
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null
      FlowHint: "BusinessRequirementsFlow21"

    - DocumentId: "B01-0a-006-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.28
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null
      FlowHint: "ImplementationFlow41"

  DecisionCount: 3

Routing rationale:
  "Write" fires IMPLEMENT. "stories" fires DOCUMENT. "screen" and "interface"
  and "component" fire UI. "build" and "library" fire IMPLEMENT. Three predicates,
  three decisions, three flows to invoke. FlowHint is advisory — the factory
  selects the actual flow.
