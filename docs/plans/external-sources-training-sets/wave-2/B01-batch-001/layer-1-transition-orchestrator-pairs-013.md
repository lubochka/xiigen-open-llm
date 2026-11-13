# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-013-001 — feature_addition: UserProfilePage UI-only

```
source_id: xiigen-b01-l1-013-001
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-013-001"
  user_request: "List all the UI components on the user profile page — avatar upload, display name input, and bio form."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-013-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  UserRequest: "List all the UI components on the user profile page — avatar upload, display name input, and bio form."
  DomainId: "software-development"
  ContextCarryForward: true
  Metadata:
    primary_intent: "inventory"
    domain_signal: "ui_component_mapping"

---

## B01-L1-013-002 — feature_addition: NotificationCenter screens

```
source_id: xiigen-b01-l1-013-002
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-013-002"
  user_request: "Map all screens in the notification center, including the full notification list page, detail view, and empty state."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-013-002-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_screens"
    domain_signal: "screen_inventory"

---

## B01-L1-013-003 — feature_addition: SettingsDashboard screens (UI)

```
source_id: xiigen-b01-l1-013-003
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-013-003"
  user_request: "Map all screens in the settings dashboard — account, security, billing, integrations, and notification preference pages."
  CompiledPredicateIds: ["UI"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-013-003-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "map_screens"
    domain_signal: "settings_screen_inventory"

---

## B01-L1-013-004 — documentation: SettingsDashboard WCAG compliance (UI+DOCUMENT)

```
source_id: xiigen-b01-l1-013-004
source_license_state: first_party
pair_kind: transition_orchestrator
```

A:
  run_id: "B01-l1-013-004"
  user_request: "Document the accessibility compliance requirements for the settings dashboard."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  PredictedSubPhaseId: "1.1"

B:
  TransitionId: "transition-B01-l1-013-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    primary_intent: "document_and_map"
    domain_signal: "accessibility_compliance"

Note: UI + DOCUMENT without IMPLEMENT → feature_addition (UI drives it).
