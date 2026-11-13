# Layer 1 — Transition Orchestrator Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: transition_orchestrator

---

## B01-L1-009-001 — TransitionType summary for B01

```
source_id: xiigen-b01-l1-009-001
source_title: XIIGen B01 layer1-009 — TransitionType summary table
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

TransitionType summary for B01:

| Predicates | TransitionType | Primary SubPhaseId | Rationale |
|---|---|---|---|
| [UI] | feature_addition | 1.1 | Creating new screen inventory = new product artifact |
| [DOCUMENT] | documentation | 2.1 | Pure requirements/spec document |
| [IMPLEMENT] | feature_addition | 4.1 | Building code |
| [ARCHITECTURE] | documentation | 3.1 | Design artifact only |
| [UI, DOCUMENT] | feature_addition | 1.1 | UI present → feature_addition |
| [UI, IMPLEMENT] | feature_addition | 1.1 | IMPLEMENT present |
| [UI, ARCHITECTURE] | documentation | 1.1 | No IMPLEMENT; design artifact |
| [DOCUMENT, IMPLEMENT] | feature_addition | 2.1 | IMPLEMENT present |
| [ARCHITECTURE, IMPLEMENT] | feature_addition | 3.1 | IMPLEMENT present |
| [ARCHITECTURE, DOCUMENT] | documentation | 3.1 | No IMPLEMENT |
| [UI, DOCUMENT, IMPLEMENT] | feature_addition | 1.1 | IMPLEMENT present + UI |
| [UI, ARCHITECTURE, IMPLEMENT] | feature_addition | 1.1 | IMPLEMENT present + UI |
| [UI, ARCHITECTURE, DOCUMENT] | documentation | 1.1 | No IMPLEMENT; UI+ARCH+DOC |
| [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT] | feature_addition | 1.1 | IMPLEMENT present |
| [] | unknown | null | knowledge-edge |

---

## B01-L1-009-002 — TransitionType edge case: ARCHITECTURE+UI without IMPLEMENT

```
source_id: xiigen-b01-l1-009-002
source_title: XIIGen B01 layer1-009 — ARCHITECTURE+UI documentation edge case
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

Edge case: [UI, ARCHITECTURE] without IMPLEMENT → documentation

  This is counterintuitive because UI alone → feature_addition, but
  UI + ARCHITECTURE (without IMPLEMENT) → documentation.

  Why: When "design" fires ARCHITECTURE, the intent is to produce a design
  artifact (architecture + wireframe + screen map). No code is being written.
  The "design" keyword signals a documentation/planning intent, which overrides
  the default "feature_addition" of UI alone.

  Contrast:
    "Map all screens" (UI only) → feature_addition (creating a new UX artifact)
    "Design the screen architecture" (UI + ARCHITECTURE) → documentation (creating
      a design document; not implementing anything)

  The test: Is there an "implement" or "build" or "create" or "code" signal?
    YES → feature_addition
    NO + ARCHITECTURE present → documentation

  This rule prevents incorrect TransitionType assignment for architecture/design
  sessions that produce diagrams and plans rather than code.

---

## B01-L1-009-003 — Complete SelectedLogicalTransition field schema

```
source_id: xiigen-b01-l1-009-003
source_title: XIIGen B01 layer1-009 — SelectedLogicalTransition canonical field schema
source_license_state: first_party
pair_kind: transition_orchestrator
coverage_status: covered
```

SelectedLogicalTransition canonical fields:

Required fields:
  TransitionId          — unique ID: "transition-[batch]-[run_id]-[SubPhaseId]-[TransitionType]"
  TransitionType        — "feature_addition" or "documentation" or "unknown"
  ScenarioId            — always "adaptive-logical-orchestration" for B01
  PhaseId               — phase number as string
  SubPhaseId            — primary sub-phase as string
  DomainId              — always "software-development" for B01
  UserRequest           — verbatim user request string
  PredicateIds          — array of fired predicates
  ContextCarryForward   — boolean; always true in B01 (context passed to nine-node flow)
  EvidenceIds           — array of document IDs from compiler output

Optional fields:
  Metadata.transition_classification_basis — explanation of why TransitionType was chosen
  Metadata.primary_intent — "create", "document", "build", "design"
  Metadata.domain_signal — domain context label
  Metadata.flow_execution_order — ordered array of FlowIds

Note: Node 01 of every nine-node flow receives the SelectedLogicalTransition as input.
The Node 01 input MUST be this object — never a raw user request string.
This is enforced by the EM-9 guard.
