# Layer 0-B — Router Pairs (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: router

---

## Method note

File 009 covers [ARCHITECTURE, IMPLEMENT, DOCUMENT, UI] → four routing decisions.

---

## B06-0B-009-001 — Four predicates → four routing decisions

```
source_id: xiigen-b06-0b-009-001
source_title: XIIGen B06 router — four predicates four routing decisions
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 2
sub_phase: 2.1
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B06-0a-009-001]
  DocumentId: "B06-0a-009-001-prompt"
  CompiledText: "Design the architecture, write the requirements spec, implement the checkout service, and document the UI flow."
  CompiledPredicateIds: ["ARCHITECTURE", "DOCUMENT", "IMPLEMENT", "UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B06-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B06-0a-009-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.33
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 4

Routing rationale:
  ARCHITECTURE → 3.1; DOCUMENT → 2.1; IMPLEMENT → 4.1; UI → 1.1.
  Four predicates, four independent routing decisions.
  DecisionCount: 4.

---

## B06-0B-009-002 — Four-decision routing consequence: four flow instantiations

```
source_id: xiigen-b06-0b-009-002
source_title: XIIGen B06 router — four decisions four flow instantiations
source_license_state: first_party
pair_kind: router
```

Teaching note — four routing decisions consequence:

  Four routing decisions result in four flow factory instantiations:
    3.1 → ArchitectureFlow31
    2.1 → RequirementsAuthoringFlow21
    4.1 → ImplementationFlow41
    1.1 → ScreenInventoryFlow11

  Each flow runs its nine-node chain independently.
  The TransitionOrchestratorUnit assigns TransitionType: feature_addition
  because IMPLEMENT is present.

  Queue ordering (default):
    1. RequirementsAuthoringFlow21 (2.1) — requirements first
    2. ArchitectureFlow31 (3.1) — architecture second
    3. ScreenInventoryFlow11 (1.1) — UI third
    4. ImplementationFlow41 (4.1) — implementation last

  Note: Queue ordering is a flow factory concern, not a routing concern.
  The router produces decisions without ordering. The factory orders them.
