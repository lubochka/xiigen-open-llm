# Layer 0-B — Router Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## Method note (020)

Multi-routing fan-out cases: inputs that produce 3 or 4 routing decisions.
Each predicate routes independently. DecisionCount equals the number of distinct predicates.

---

## B01-0B-020-001 — UI+DOCUMENT+IMPLEMENT: three-branch fan-out

```
source_id: xiigen-b01-0b-020-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-023-001-prompt"
  CompiledText: "Write user stories for the checkout screen, specifying the form interactions and button accessibility requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-023-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-023-001-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-023-001-prompt"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 3

Fan-out rationale:
  Three predicates → three independent RoutingDecision entries.
  UI → 1.1 (ScreenInventoryFlow11).
  DOCUMENT → 2.1 (BusinessRequirementsFlow21).
  IMPLEMENT → 4.1 (ImplementationFlow41).
  No predicate suppresses another. EM-9 guard: no forbidden fields present.

---

## B01-0B-020-002 — UI+DOCUMENT+IMPLEMENT+ARCHITECTURE: four-branch fan-out

```
source_id: xiigen-b01-0b-020-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-024-001-prompt"
  CompiledText: "Build the design-system component library, document the specification for each screen's visual behavior, and define the service module architecture that exposes the UI theme tokens."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-024-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-024-001-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-024-001-prompt"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["ARCHITECTURE"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-024-001-prompt"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 4

Fan-out rationale:
  Four predicates → four independent decisions.
  ARCHITECTURE → 3.1. All four sub-phases activated.
  The maximum fan-out for B01 is 4 (all four predicates).

---

## B01-0B-020-003 — UI+IMPLEMENT: two-branch fan-out (no DOCUMENT)

```
source_id: xiigen-b01-0b-020-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-022-001-prompt"
  CompiledText: "Build the checkout screen with all form inputs, a responsive layout, and the confirm payment button."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-022-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-022-001-prompt"
      PredictedPhaseId: "4"
      PredictedSubPhaseId: "4.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["IMPLEMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Fan-out note: DOCUMENT absent → no 2.1 decision. Exactly 2 decisions for 2 predicates.

---

## B01-0B-020-004 — AddressBook UI-only routing for comparison

```
source_id: xiigen-b01-0b-020-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-020-001-prompt"
  CompiledText: "Map all screens in the address book — address list page, add new address form, edit address form, and delete confirmation modal."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-020-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1
