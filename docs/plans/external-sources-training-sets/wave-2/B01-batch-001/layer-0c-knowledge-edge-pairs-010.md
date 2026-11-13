# Layer 0-C — Knowledge-Edge Pairs (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## B01-0C-010-001 — TransitionType for B01 scenarios

```
source_id: xiigen-b01-0c-010-001
source_title: XIIGen B01 knowledge-edge-010 — TransitionType determination in B01
source_license_state: first_party
contract_signature: AdaptiveLogicalOrchestrator — knowledge-edge result path
pair_kind: knowledge_edge
coverage_status: covered
```

A:
  DocumentId: "B01-0c-010-001-prompt"
  CompiledText: "What TransitionType applies to each predicate combination in B01?"
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  KnownFailureMode: null

B:
  FlowId: "universal-nine-node-development-flow-2.1"
  OverallVerdict: "FLOW_EXECUTING"
  Metrics:
    matched_predicate_count: "1"
    recommended_action: "EXECUTE_FLOW"
  Warnings: []

TransitionType determination rules:

  Rule 1 — IMPLEMENT present (any combination):
    TransitionType: feature_addition
    Rationale: IMPLEMENT signals that code or an artifact is being built or written;
      any build activity = feature_addition regardless of other predicates.

  Rule 2 — No IMPLEMENT; UI present without IMPLEMENT:
    TransitionType: feature_addition
    Rationale: Creating a new screen inventory, wireframe, or UX flow is adding
      a new feature capability even without writing code.

  Rule 3 — No IMPLEMENT; DOCUMENT alone:
    TransitionType: documentation
    Rationale: Pure requirements, specifications, or use-case documents are
      documentation artifacts only.

  Rule 4 — No IMPLEMENT; ARCHITECTURE alone or ARCHITECTURE + UI:
    TransitionType: documentation
    Rationale: Architecture diagrams and design documents are documentation
      artifacts; no implementation is specified.

  Rule 5 — No IMPLEMENT; ARCHITECTURE + DOCUMENT:
    TransitionType: documentation

  Summary: feature_addition when IMPLEMENT is present OR when UI is present
    without IMPLEMENT (UI always implies a new artifact is created).
    documentation only when no IMPLEMENT and no UI (just DOCUMENT and/or ARCHITECTURE).

---

## B01-0C-010-002 — TransitionType examples across B01 scenarios

```
source_id: xiigen-b01-0c-010-002
source_title: XIIGen B01 knowledge-edge-010 — TransitionType worked examples
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Worked examples for TransitionType in B01:

  "Map all screen flows for SaaS onboarding."
  → Predicates: ["UI"]
  → TransitionType: feature_addition (UI present, no IMPLEMENT)

  "Document the checkout page interface flow."
  → Predicates: ["UI", "DOCUMENT"]
  → TransitionType: feature_addition (UI present)

  "Describe the functional behavior of the onboarding wizard."
  → Predicates: ["DOCUMENT"]
  → TransitionType: documentation (DOCUMENT alone, no UI or IMPLEMENT)

  "Write the screen specifications."
  → Predicates: ["UI", "DOCUMENT", "IMPLEMENT"] (write=IMPLEMENT; specifications=DOCUMENT; screen=UI)
  → TransitionType: feature_addition (IMPLEMENT present)

  "Design the architecture for the screen flow system."
  → Predicates: ["UI", "ARCHITECTURE"]
  → TransitionType: documentation (no IMPLEMENT; ARCHITECTURE + UI without IMPLEMENT)

  "Implement the responsive form components."
  → Predicates: ["UI", "IMPLEMENT"]
  → TransitionType: feature_addition (IMPLEMENT present)

---

## B01-0C-010-003 — TransitionType determines which pipeline gate runs

```
source_id: xiigen-b01-0c-010-003
source_title: XIIGen B01 knowledge-edge-010 — TransitionType pipeline gate consequences
source_license_state: first_party
pair_kind: knowledge_edge
coverage_status: covered
```

Pipeline consequences of TransitionType:

  feature_addition:
    - Invokes both UX flows AND implementation flows (if applicable)
    - Gate B criteria include: code quality, test coverage, API integration
    - Artifacts: screen inventory + implementation code + specifications
    - Learning pipeline: updates AutonomousTrainer with new feature patterns

  documentation:
    - Invokes only specification/documentation flows
    - Gate B criteria include: completeness, coverage, acceptance criteria clarity
    - Artifacts: requirements document, architecture diagram, use-case document
    - Learning pipeline: updates AutonomousTrainer with new documentation patterns

  Misclassifying feature_addition as documentation (or vice versa) causes:
    - Wrong Gate B assertions being evaluated
    - Implementation code not scheduled when needed
    - Incorrect artifact types produced
    - AutonomousTrainer learning from wrong classification
