# Layer 0-B — Router Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-014-001 — NotificationCenter: UI-only routing to 1.1

```
source_id: xiigen-b01-0b-014-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-014-001-prompt"
  CompiledText: "Map all screens in the notification center, including the full notification list page, detail view, and empty state."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-014-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-014-002 — AlertBanner: UI-only routing to 1.1

```
source_id: xiigen-b01-0b-014-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-014-002-prompt"
  CompiledText: "Inventory all AlertBanner component variants — info, warning, error, success — including color, spacing, and icon placement."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-014-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-014-003 — NotificationCenter: UI-only wireframe routing to 1.1

```
source_id: xiigen-b01-0b-014-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-014-003-prompt"
  CompiledText: "Produce a wireframe for the notification center showing interaction states: unread, read, dismissed, and bulk-action mode."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-014-003-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.42
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-014-004 — AlertBanner: UI+DOCUMENT dual routing to 1.1 and 2.1

```
source_id: xiigen-b01-0b-014-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-014-004-prompt"
  CompiledText: "Define the accessibility specification for the AlertBanner: ARIA roles, focus behavior, and color contrast requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-014-004-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-014-004-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.37
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (accessibility, color) → 1.1. DOCUMENT (define, specification, requirements) → 2.1.
  EM-9 guard: two separate RoutingDecision entries; no forbidden fields.
