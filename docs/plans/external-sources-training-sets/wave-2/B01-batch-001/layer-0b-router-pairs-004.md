# Layer 0-B — Router Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-004-001 — [UI, DOCUMENT] → 1.1 + 2.1: two routing decisions

```
source_id: xiigen-b01-0b-004-001
source_title: XIIGen B01 router-004 — [UI, DOCUMENT] dual routing to 1.1 and 2.1
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-004-001]
  DocumentId: "B01-0a-004-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/0a-004-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Checkout user interface flow documentation and page specification"
  CompiledText: "Document the complete user interface flows for the checkout process and produce a page specification."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-004-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.44
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-004-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI evidence (interface, page) routes to 1.1 (Screen Flow and UX Definition).
  DOCUMENT evidence (document, specification) routes to 2.1 (Business Requirements
  and Use Cases). The "page specification" is a business requirements artifact —
  it specifies what the page must do, which is a sub-phase 2.1 concern.
  Two decisions, each independent. EvidencePredicates in each decision contain
  only the predicates that drove that specific routing branch.

---

## B01-0B-004-002 — [UI, DOCUMENT] → 1.1 + 2.1: dashboard screen variant

```
source_id: xiigen-b01-0b-004-002
source_title: XIIGen B01 router-004 — [UI, DOCUMENT] dashboard screen and requirements
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-004-003]
  DocumentId: "B01-0a-004-003-prompt"
  CompiledText: "Map the user dashboard interface screens and define the requirements for each screen's behavior."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-004-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-004-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI fires on "interface", "screen" → 1.1. DOCUMENT fires on "define", "requirement",
  "behavior" → 2.1. The "requirements for each screen's behavior" is a business
  requirements document concern (sub-phase 2.1 owns requirement specification);
  the "map the user dashboard interface screens" is a UX screen inventory concern
  (sub-phase 1.1 owns screen mapping).

---

## B01-0B-004-003 — [UI, DOCUMENT, IMPLEMENT] → 1.1 + 2.1 + 4.1

```
source_id: xiigen-b01-0b-004-003
source_title: XIIGen B01 router-004 — [UI, DOCUMENT, IMPLEMENT] three routing decisions
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: router
```

A: [SubPhaseRouterSourceDocument — from compiler pair B01-0a-004-002]
  DocumentId: "B01-0a-004-002-prompt"
  CompiledText: "Write a specification for the checkout screen form layout including all input fields and their visual states."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-004-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.43
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-004-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-004-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.31
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Routing rationale:
  UI (screen, form, layout, input, visual) → 1.1; DOCUMENT (specification) → 2.1;
  IMPLEMENT (write) → 4.1. Three separate routing branches. The "write" predicate
  fires IMPLEMENT regardless of the documentation intent of the sentence — this is
  the dual-predicate rule applied at routing stage. DecisionCount must be 3.
