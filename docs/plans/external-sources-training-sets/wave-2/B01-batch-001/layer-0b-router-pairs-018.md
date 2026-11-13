# Layer 0-B — Router Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-018-001 — WishlistPage: UI-only routing (screens, page, interaction, modal)

```
source_id: xiigen-b01-0b-018-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-018-001-prompt"
  CompiledText: "Map all screens in the wishlist page — populated wishlist, empty wishlist, and the move-to-cart interaction modal."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-018-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-018-002 — SavedItems: UI-only routing (UI, component, layout, button)

```
source_id: xiigen-b01-0b-018-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-018-002-prompt"
  CompiledText: "Inventory all SavedItems UI components — item card layout, share button, remove button, and add-to-cart button."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-018-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-018-003 — WishlistPage visual tokens: UI-only routing

```
source_id: xiigen-b01-0b-018-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-018-003-prompt"
  CompiledText: "Catalogue the visual design tokens — color, spacing, and typography — used in the wishlist page and saved items components."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-018-003-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-018-004 — WishlistPage accessibility: UI+DOCUMENT routing

```
source_id: xiigen-b01-0b-018-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-018-004-prompt"
  CompiledText: "Define the accessibility requirements for the wishlist page — ARIA labels for each button and color contrast compliance."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-018-004-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-018-004-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2
