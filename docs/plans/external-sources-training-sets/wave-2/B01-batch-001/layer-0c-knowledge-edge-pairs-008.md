# Layer 0-C — Knowledge-Edge Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-008-001 — DOCUMENT + UI: routing to both 1.1 and 2.1 explained

```
source_id: xiigen-b01-0c-008-001
source_title: XIIGen B01 knowledge-edge-008 — DOCUMENT + UI: why two routing decisions
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-008-001-prompt"
  CompiledText: "Map the checkout interface screens and document the page requirements."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "2"
    routing_summary: "UI → 1.1 (ScreenInventoryFlow11); DOCUMENT → 2.1 (BusinessRequirementsFlow21)"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

Why two routing decisions:

  The UI predicate and DOCUMENT predicate own different sub-phases. When both fire,
  the orchestrator must invoke two separate flows:

  Flow 1: ScreenInventoryFlow11 (sub-phase 1.1)
    Purpose: Map, categorize, and describe all screens in the checkout interface
    Output: Screen inventory document with IDs, types, entry/exit conditions
    Duration: Nine-node flow execution

  Flow 2: BusinessRequirementsFlow21 (sub-phase 2.1)
    Purpose: Produce the page requirements document for each checkout page
    Output: Requirements specification with acceptance criteria
    Duration: Nine-node flow execution (different flow)

  The two flows produce independent, complementary artifacts. Neither replaces
  the other. The combined output is a screen inventory + a requirements document.

---

## B01-0C-008-002 — Why not route UI to 2.1 or DOCUMENT to 1.1

```
source_id: xiigen-b01-0c-008-002
source_title: XIIGen B01 knowledge-edge-008 — sub-phase ownership explanation
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Sub-phase 1.1 ownership: UI
  Sub-phase 1.1 is designed to receive UX and UI artifacts — screen maps,
  interaction diagrams, visual flows. It is NOT designed to produce business
  requirements documents. A requirements document placed in sub-phase 1.1
  would be misclassified and might be processed by the wrong pipeline unit.

Sub-phase 2.1 ownership: DOCUMENT
  Sub-phase 2.1 is designed to receive requirement specifications — use cases,
  functional behavior descriptions, acceptance criteria. It is NOT designed to
  produce screen inventory documents. A screen map placed in 2.1 would be
  treated as a requirements artifact and lose its visual/flow context.

The two sub-phases have different pipeline processing: 1.1 feeds into visual
design tools; 2.1 feeds into requirement management systems. Routing each
predicate to its correct sub-phase ensures the right downstream processing.

---

## B01-0C-008-003 — DOCUMENT routing independently of UI count

```
source_id: xiigen-b01-0c-008-003
source_title: XIIGen B01 knowledge-edge-008 — DOCUMENT independence from UI signal strength
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Rule: DOCUMENT routes to 2.1 regardless of how many UI terms are present.

  Low UI signal (1 term) + DOCUMENT (1 term) → 2 routing decisions: 1.1 + 2.1
  High UI signal (7 terms) + DOCUMENT (1 term) → still 2 routing decisions: 1.1 + 2.1

  The ratio of UI terms to DOCUMENT terms does not suppress either predicate.
  Even one DOCUMENT term is sufficient to produce a 2.1 routing decision.
  Even one UI term is sufficient to produce a 1.1 routing decision.

  Example: "Document the complete design system component library: typography scale,
    color system, spacing tokens, theme variants, and layout grid."
    → UI fires 7 times (design-system, component, typography, color, spacing, theme, layout)
    → DOCUMENT fires 1 time (document)
    → Result: ["UI", "DOCUMENT"] → two routing decisions: 1.1 + 2.1
    → The single DOCUMENT fire is not suppressed by the 7 UI fires.

This independence rule applies to all predicate combinations in B01.
