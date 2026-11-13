# Layer 0-A — Compiler Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: compiler

---

## Method note (024)

This file is the complete compiler rule reference for B01-batch-001.
Pair 001 demonstrates the four-predicate sweep (UI + DOCUMENT + IMPLEMENT + ARCHITECTURE).
Pairs 002–005 contain the complete B01 predicate rule tables for training reference.

---

## B01-0A-024-001 — Four-predicate sweep: UI + DOCUMENT + IMPLEMENT + ARCHITECTURE

```
source_id: xiigen-b01-0a-024-001
source_title: XIIGen B01 compiler-024 — four-predicate sweep (UI+DOCUMENT+IMPLEMENT+ARCHITECTURE)
source_license_state: first_party
contract_signature: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
coverage_status: covered
pair_kind: compiler
```

A:
  run_id: "B01-0a-024-001"
  scenario_id: "adaptive-logical-orchestration"
  user_request: "Build the design-system component library, document the specification for each screen's visual behavior, and define the service module architecture that exposes the UI theme tokens."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "B01-0a-024-001-prompt"
  SourcePath: "wave-2/B01-batch-001/compiler-input/024-001"
  SourceTier: "internal"
  LicenseOrAuthority: "first_party"
  DocumentType: "compiled_prompt"
  DocumentTitle: "Design-system build + screen spec + service architecture — four-predicate"
  CompiledText: "Build the design-system component library, document the specification for each screen's visual behavior, and define the service module architecture that exposes the UI theme tokens."
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  SourceStatus: "ready"
  KnownFailureMode: null

Predicate evidence:
  IMPLEMENT fires on: "build"
  DOCUMENT fires on: "document", "specification"
  UI fires on: "design-system", "component", "screen", "visual", "UI", "theme"
  ARCHITECTURE fires on: "service", "module", "architecture"
  Four predicates present. TransitionType: feature_addition (IMPLEMENT present).
  Routing: four independent decisions → sub-phases 1.1, 2.1, 3.1, 4.1.
  Confidence: 0.92

---

## B01-0A-024-002 — Complete B01 UI predicate lexicon reference

```
source_id: xiigen-b01-0a-024-ref-ui
source_title: XIIGen B01 compiler-024 — complete UI predicate lexicon
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

Complete UI lexicon (22 terms):
  ui | interface | ux | screen | page | modal | form | layout | component |
  responsive | accessibility | visual | wireframe | prototype | interaction |
  button | input | typography | color | spacing | design-system | theme

Non-firing terms commonly confused with UI:
  navigation (NOT UI — critical rule: navigation does NOT fire UI)
  menu (NOT UI — "nav bar", "menu" are gap vocabulary)
  flow (NOT UI — context noun only)
  journey (NOT UI — context noun only)
  redesign (NOT UI and NOT IMPLEMENT)
  experience (NOT UI)
  onboarding (NOT any predicate)

---

## B01-0A-024-003 — Complete B01 DOCUMENT predicate lexicon reference

```
source_id: xiigen-b01-0a-024-ref-doc
source_title: XIIGen B01 compiler-024 — complete DOCUMENT predicate lexicon
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

Complete DOCUMENT lexicon (15 terms):
  document | spec | specification | requirement | story | criteria | describe |
  define | functional | behavior | use-case | narrative | feature-brief | capability | epics

Special cases:
  "requirements" (plural) fires DOCUMENT via root "requirement"
  "describing" fires DOCUMENT via root "describe"
  "specifying" fires DOCUMENT via root "spec"
  "stories" fires DOCUMENT via root "story"
  "use case" (two words) = "use-case" — fires DOCUMENT
  "behaviors" fires DOCUMENT via root "behavior"
  "write user stories" → DOCUMENT (story) + IMPLEMENT (write) — both fire

---

## B01-0A-024-004 — Complete B01 IMPLEMENT predicate lexicon reference

```
source_id: xiigen-b01-0a-024-ref-impl
source_title: XIIGen B01 compiler-024 — complete IMPLEMENT predicate lexicon
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

Complete IMPLEMENT lexicon (17 terms):
  implement | code | build | develop | write | create | function | class | logic |
  library | handler | middleware | controller | backend | feature | method | refactor

Key rule: "write" always fires IMPLEMENT regardless of what is being written.
  "write user stories" → IMPLEMENT (write) + DOCUMENT (stories)
  "write a spec" → IMPLEMENT (write) + DOCUMENT (spec)
  "write the screen" → IMPLEMENT (write) + UI (screen)

TransitionType rule:
  IMPLEMENT present → feature_addition (unconditional)
  IMPLEMENT absent, UI present → feature_addition
  IMPLEMENT absent, DOCUMENT only → documentation
  IMPLEMENT absent, ARCHITECTURE only → documentation

---

## B01-0A-024-005 — Complete B01 TransitionType determination table

```
source_id: xiigen-b01-0a-024-ref-transition
source_title: XIIGen B01 compiler-024 — complete TransitionType table and routing summary
source_license_state: first_party
pair_kind: compiler
coverage_status: covered
```

TransitionType table:

| Predicates | TransitionType | Routing decisions |
|---|---|---|
| [UI] | feature_addition | 1 (1.1) |
| [DOCUMENT] | documentation | 1 (2.1) |
| [ARCHITECTURE] | documentation | 1 (3.1) |
| [IMPLEMENT] | feature_addition | 1 (4.1) |
| [UI, DOCUMENT] | feature_addition | 2 (1.1 + 2.1) |
| [UI, IMPLEMENT] | feature_addition | 2 (1.1 + 4.1) |
| [UI, ARCHITECTURE] | documentation | 2 (1.1 + 3.1) |
| [DOCUMENT, IMPLEMENT] | feature_addition | 2 (2.1 + 4.1) |
| [UI, DOCUMENT, IMPLEMENT] | feature_addition | 3 (1.1 + 2.1 + 4.1) |
| [UI, ARCHITECTURE, IMPLEMENT, DOCUMENT] | feature_addition | 4 (1.1 + 2.1 + 3.1 + 4.1) |

Sub-phase routing table:
  UI → 1.1 (ScreenInventoryFlow11)
  DOCUMENT → 2.1 (BusinessRequirementsFlow21)
  ARCHITECTURE → 3.1 (SystemArchitectureFlow31)
  IMPLEMENT → 4.1 (ImplementationFlow41)

Knowledge-edge path:
  CompiledPredicateIds: [] → KnowledgeEdge: true → no sub-phase → SCHEDULE_CAPABILITY_LEARNING
  Model must NOT infer predicates from context alone.
  Model must NOT guess a sub-phase when no predicate fires.
