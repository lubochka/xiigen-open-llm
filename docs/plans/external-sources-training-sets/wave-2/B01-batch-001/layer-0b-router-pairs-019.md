# Layer 0-B — Router Pairs (019)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-019-001 — OrderHistoryPage: UI-only routing

```
source_id: xiigen-b01-0b-019-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-019-001-prompt"
  CompiledText: "Map all screens in the order history section — the order list page, individual order detail screen, and receipt download modal."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-019-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-019-002 — TrackingView: UI-only routing

```
source_id: xiigen-b01-0b-019-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-019-002-prompt"
  CompiledText: "Inventory all states of the order tracking screen — processing, shipped, out for delivery, delivered, and delivery exception."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-019-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-019-003 — OrderHistoryPage layout+typography: UI+DOCUMENT routing

```
source_id: xiigen-b01-0b-019-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-019-003-prompt"
  CompiledText: "Specify the layout grid and typography scale for the order history page, from order summary rows to detail headers."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-019-003-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-019-003-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

---

## B01-0B-019-004 — OrderHistoryPage accessibility: UI+DOCUMENT routing

```
source_id: xiigen-b01-0b-019-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-019-005-prompt"
  CompiledText: "Document the accessibility requirements for the order history page — screen reader labels and color contrast for status badges."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-019-005-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-019-005-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2
