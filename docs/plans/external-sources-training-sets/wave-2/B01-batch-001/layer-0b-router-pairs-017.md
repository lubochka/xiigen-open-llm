# Layer 0-B — Router Pairs (017)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-017-001 — CartView: UI-only routing

```
source_id: xiigen-b01-0b-017-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-017-001-prompt"
  CompiledText: "Map all screens in the cart view — empty cart, cart with items, and cart with applied promo code — including the page layout for each."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-017-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-017-002 — MiniCart: UI-only routing

```
source_id: xiigen-b01-0b-017-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-017-002-prompt"
  CompiledText: "Inventory the MiniCart component — its collapsed icon button, expanded panel, and interactive item-removal action."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-017-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-017-003 — CartView responsive: UI-only routing

```
source_id: xiigen-b01-0b-017-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-017-003-prompt"
  CompiledText: "Catalogue the responsive layout and spacing decisions for the CartView screen on mobile and desktop."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-017-003-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-017-004 — CartView color+typography: UI+DOCUMENT routing

```
source_id: xiigen-b01-0b-017-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-017-004-prompt"
  CompiledText: "Describe the color palette and visual theme applied to the cart screen, including CTA button colors and typography."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-017-004-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-017-004-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2
