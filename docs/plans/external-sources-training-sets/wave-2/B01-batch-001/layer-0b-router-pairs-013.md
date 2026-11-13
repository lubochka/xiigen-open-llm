# Layer 0-B — Router Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-013-001 — UserProfilePage: UI-only routing to 1.1

```
source_id: xiigen-b01-0b-013-001
source_title: XIIGen B01 router-013 — UserProfilePage UI-only routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from Layer 0-A 013-001]
  DocumentId: "B01-0a-013-001-prompt"
  CompiledText: "List all the UI components on the user profile page — avatar upload, display name input, and bio form."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-013-001-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  UI predicate alone routes to 1.1. ProfilePage, component inventory — clear 1.1 signal.

---

## B01-0B-013-002 — UserProfilePage: UI+DOCUMENT dual routing to 1.1 and 2.1

```
source_id: xiigen-b01-0b-013-002
source_title: XIIGen B01 router-013 — UserProfilePage UI+DOCUMENT dual routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from Layer 0-A 013-002]
  DocumentId: "B01-0a-013-002-prompt"
  CompiledText: "Describe the interaction pattern for the edit profile modal, including button placement and typography choices."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-013-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-013-002-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI → 1.1; DOCUMENT → 2.1. Two independent decisions. Neither suppresses the other.

---

## B01-0B-013-003 — UserProfilePage: UI+DOCUMENT routing from 013-003

```
source_id: xiigen-b01-0b-013-003
source_title: XIIGen B01 router-013 — UserProfilePage responsive layout UI+DOCUMENT routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from Layer 0-A 013-003]
  DocumentId: "B01-0a-013-003-prompt"
  CompiledText: "Define the responsive layout breakpoints for the user profile page, covering mobile, tablet, and desktop spacing."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-013-003-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-013-003-prompt"
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
  UI (responsive, layout, page, spacing) → 1.1.
  DOCUMENT (define) → 2.1. Two independent RoutingDecision entries required.

---

## B01-0B-013-004 — UserProfilePage: UI-only color/theme routing

```
source_id: xiigen-b01-0b-013-004
source_title: XIIGen B01 router-013 — UserProfilePage color/theme UI-only routing
source_license_state: first_party
contract_signature: MultiSubPhaseRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
pair_kind: router
```

A: [SubPhaseRouterSourceDocument from Layer 0-A 013-004]
  DocumentId: "B01-0a-013-004-prompt"
  CompiledText: "Inventory the color tokens and theme settings applied to the user profile screen."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-013-004-prompt"
      SourceStatus: "ready"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 1

Routing rationale:
  UI (color, theme, screen) → single decision to 1.1.
