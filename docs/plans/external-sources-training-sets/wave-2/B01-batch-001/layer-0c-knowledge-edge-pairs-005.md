# Layer 0-C — Knowledge-Edge Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-005-001 — Phase 1 sub-phase map: predicate signals for 1.1 through 1.5

```
source_id: xiigen-b01-0c-005-001
source_title: XIIGen B01 knowledge-edge-005 — Phase 1 sub-phase map
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-005-001-prompt"
  CompiledText: "What are the five sub-phases in Phase 1 and what routes to each?"
  CompiledPredicateIds: ["UI"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-1.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    knowledge_edge_reason: "null"
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

Phase 1 sub-phase map:

| Sub-phase | Name | Primary predicate signal | Secondary distinguishers |
|---|---|---|---|
| 1.1 | Screen Flow and UX Definition | UI (general) | screen, page, ux, interface — any UI term |
| 1.2 | Wireframes and Prototypes | UI (wireframe, prototype) | wireframe, prototype, mockup |
| 1.3 | Design System | UI (design-system, theme) | design-system, theme, typography, color, spacing |
| 1.4 | Accessibility | UI (accessibility) | accessibility, WCAG |
| 1.5 | UI Testing | UI (interaction testing) | test, spec combination with UI terms |

In B01 batch routing, all UI signals route to 1.1 as the primary target.
Sub-phases 1.2-1.5 are reached only through secondary routing rules that apply
when the primary sub-phase routing also includes a distinguishing secondary signal.

For this batch: 1.1 is the only Phase 1 sub-phase with registered routing.
1.2 through 1.5 routing is covered in subsequent B02-B05 batches.

---

## B01-0C-005-002 — Sub-phase 1.1 signal strength hierarchy

```
source_id: xiigen-b01-0c-005-002
source_title: XIIGen B01 knowledge-edge-005 — sub-phase 1.1 signal strength ordering
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Sub-phase 1.1 (Screen Flow and UX Definition) — signal strength:

Strong signals (very likely 1.1):
  - "screen inventory" / "screen map"
  - "user flow" + UI terms
  - "page inventory" / "page map"
  - "screen flow"
  - "ux definition"

Medium signals (probably 1.1 but could be 1.2 or 1.4):
  - "interface" alone
  - "page" alone
  - "component" alone
  - "accessibility" alone (might suggest 1.4)
  - "wireframe" alone (might suggest 1.2)

Routing rule in this batch: regardless of signal strength, all UI-predicate
signals route to 1.1 in B01. The NearestWrongSubPhaseScore reflects the
competition from 1.2-1.5 but the PredictedSubPhaseId is always 1.1.

---

## B01-0C-005-003 — DOCUMENT sub-phase routing: 2.1 vs 2.4

```
source_id: xiigen-b01-0c-005-003
source_title: XIIGen B01 knowledge-edge-005 — DOCUMENT routing: 2.1 vs 2.4
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

DOCUMENT predicate routes to two possible sub-phases:
  2.1 — Business Requirements and Use Cases (primary DOCUMENT target)
  2.4 — Stakeholder Requirements (secondary DOCUMENT target)

When 2.1 applies:
  - Business requirements, functional specifications, use cases
  - Developer-facing specification documents
  - "functional", "behavior", "use-case", "specification", "criteria"

When 2.4 applies:
  - Stakeholder-facing requirement documents
  - Executive summaries, business case documents
  - "stakeholder", "executive", "business case"
  - Less common in the B01 onboarding UX scenario

In B01, all DOCUMENT routing defaults to 2.1 unless explicit stakeholder signals
are present. The B01 batch does not contain 2.4 routing examples; all DOCUMENT
routing in this batch goes to 2.1.
