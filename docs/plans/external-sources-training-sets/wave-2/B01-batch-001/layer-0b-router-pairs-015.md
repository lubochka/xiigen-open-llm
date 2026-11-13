# Layer 0-B — Router Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: router

---

## B01-0B-015-001 — SettingsDashboard: UI-only routing (screens)

```
source_id: xiigen-b01-0b-015-001
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-015-001-prompt"
  CompiledText: "Map all screens in the settings dashboard — account, security, billing, integrations, and notification preference pages."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-015-001-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-015-002 — SettingsDashboard: UI-only routing (layout, spacing)

```
source_id: xiigen-b01-0b-015-002
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-015-002-prompt"
  CompiledText: "Inventory the layout structure and spacing tokens used across all settings dashboard sections."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-015-002-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.39
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-015-003 — SettingsDashboard: UI-only routing (form, input, button)

```
source_id: xiigen-b01-0b-015-003
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-015-003-prompt"
  CompiledText: "Catalogue the form input patterns used in the settings screens: text fields, toggles, dropdowns, and save/cancel button interactions."
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-015-003-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.38
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null
  DecisionCount: 1

---

## B01-0B-015-004 — SettingsDashboard: UI+DOCUMENT dual routing (accessibility + WCAG)

```
source_id: xiigen-b01-0b-015-004
source_license_state: first_party
pair_kind: router
```

A:
  DocumentId: "B01-0a-015-004-prompt"
  CompiledText: "Document the accessibility compliance requirements for the settings dashboard, specifying WCAG 2.1 AA criteria for each screen."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"

B:
  RoutingDecisions:
    - DocumentId: "B01-0a-015-004-prompt"
      PredictedPhaseId: "1"
      PredictedSubPhaseId: "1.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.40
      EvidencePredicates: ["UI"]
      KnowledgeEdge: false
      FailureClassification: null

    - DocumentId: "B01-0a-015-004-prompt"
      PredictedPhaseId: "2"
      PredictedSubPhaseId: "2.1"
      Confidence: 0.92
      NearestWrongSubPhaseScore: 0.36
      EvidencePredicates: ["DOCUMENT"]
      KnowledgeEdge: false
      FailureClassification: null

  DecisionCount: 2

Routing rationale:
  UI (accessibility, screen) → 1.1. DOCUMENT (document, requirements, criteria) → 2.1.
