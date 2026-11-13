# Layer 0-B — Router Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-016-001 — SearchResults: UI-only routing (screens, page, layout)

```
source_id: xiigen-b01-0b-016-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-016-001-prompt"
  CompiledText: "Map all screens in the search results experience — keyword results page, no-results state, and paginated results layout."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-016-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-016-002 — FilterPanel: UI-only routing (component, input, interaction)

```
source_id: xiigen-b01-0b-016-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-016-002-prompt"
  CompiledText: "Inventory the FilterPanel component — all input types (checkboxes, sliders, dropdowns) and their interaction states."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-016-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-016-003 — SearchResults+FilterPanel: UI+DOCUMENT dual routing

```
source_id: xiigen-b01-0b-016-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-016-003-prompt"
  CompiledText: "Describe how the search results layout and filter panel respond at mobile and tablet breakpoints."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-016-003-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-016-003-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

---

## B01-0B-016-004 — FilterPanel wireframe: UI+DOCUMENT routing

```
source_id: xiigen-b01-0b-016-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-016-005-prompt"
  CompiledText: "Produce a wireframe specification for the FilterPanel describing collapsed and expanded states."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-016-005-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-016-005-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.35
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (wireframe) → 1.1. DOCUMENT (specification, describing) → 2.1.
