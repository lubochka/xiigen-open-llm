# Layer 0-C — Knowledge-Edge Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-002-001 — UI lexicon: complete list of all 21+ UI terms

```
source_id: xiigen-b01-0c-002-001
source_title: XIIGen B01 knowledge-edge-002 — complete UI lexicon reference
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-002-001-prompt"
  CompiledText: "What terms in the UI lexicon fire the UI predicate in B01?"
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    capability_gap_id: "null"
    recommended_action: "EXECUTE_FLOW"
    next_flow_id: "universal-nine-node-development-flow-1.1"
  Warnings: []

UI lexicon — complete reference (22 entries):
  1.  ui
  2.  interface
  3.  ux
  4.  screen
  5.  page
  6.  modal
  7.  form
  8.  layout
  9.  component
  10. responsive
  11. accessibility
  12. visual
  13. wireframe
  14. prototype
  15. interaction
  16. button
  17. input
  18. typography
  19. color
  20. spacing
  21. design-system  (matches "design system" as two words or hyphenated)
  22. theme

Each of these 22 terms independently fires the UI predicate when present
in a user request. No combination or context required — each single term
is sufficient. Multiple UI terms in one request still produce only one UI
predicate (["UI"] not ["UI", "UI"]).

---

## B01-0C-002-002 — Terms adjacent to UI lexicon that do NOT fire UI

```
source_id: xiigen-b01-0c-002-002
source_title: XIIGen B01 knowledge-edge-002 — UI lexicon boundary: non-firing terms
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-002-002-prompt"
  CompiledText: "What UI-adjacent terms do NOT fire the UI predicate?"
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no predicate matched"
  KnownFailureMode: "no_predicate_match"

B:
  FlowId: "knowledge-edge-missing-flow"
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics:
    knowledge_edge_reason: "no_predicate_match"
    matched_predicate_count: "0"
    capability_gap_id: "gap-b01-0c-002-002"
    recommended_action: "SCHEDULE_CAPABILITY_LEARNING"
  Warnings: ["knowledge_edge: no predicate matched — capability learning scheduled"]

Terms that do NOT fire UI despite appearing in UX contexts:
  - navigation / nav / navbar / nav-menu
  - menu / dropdown / flyout
  - flow (as in "user flow" — not a lexicon term)
  - journey / user journey
  - experience / user experience (only "ux" the abbreviation fires, not "experience")
  - onboarding / offboarding
  - signup / sign-up / sign up
  - login / logout
  - dashboard (the word alone does not fire UI; "screen" or "page" would need to co-occur)
  - animation / transition (motion terms — not in lexicon)
  - grid (alone — "layout grid" requires "layout" to fire)
  - icon / iconography
  - branding / brand
  - style / styles / styling

---

## B01-0C-002-003 — Practical lexicon application: sample sentence analysis

```
source_id: xiigen-b01-0c-002-003
source_title: XIIGen B01 knowledge-edge-002 — lexicon application examples
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Sample sentence analysis — which predicates fire:

  Sentence: "Redesign the navigation menu for the onboarding experience."
  → UI fires? NO. "redesign" not in IMPLEMENT (it's not listed); "navigation" not
    in UI; "menu" not in UI; "onboarding" not in any lexicon; "experience" not in
    UI (only "ux" fires).
  → Predicate result: []  → knowledge-edge

  Sentence: "Update the interface navigation so screens are easier to reach."
  → UI fires on: "interface", "screen" (via "screens")
  → Predicate result: ["UI"]
  → Routes to: 1.1

  Sentence: "Write accessibility guidelines for the modal and form components."
  → UI fires on: "accessibility", "modal", "form", "component" (via "components")
  → IMPLEMENT fires on: "write"
  → Predicate result: ["UI", "IMPLEMENT"]
  → Routes to: 1.1 + 4.1
