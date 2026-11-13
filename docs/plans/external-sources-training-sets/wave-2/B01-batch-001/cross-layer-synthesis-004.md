# Cross-Layer Synthesis (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: Full four-predicate (UI+ARCHITECTURE+DOCUMENT+IMPLEMENT) — complete pipeline

---

## Synthesis trace: four-predicate request — full pipeline

### Request
UserRequest: "Design the architecture for the onboarding screen flow, implement the form components, document the user requirements, and map all screen interactions."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI", "ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
  Evidence:
    - Term: "screen", Lexicon: "UI", Term_index: 4
    - Term: "form", Lexicon: "UI", Term_index: 7
    - Term: "interactions", Lexicon: "UI", Term_index: 14 (adjacent to "interaction")
    - Term: "architecture", Lexicon: "ARCHITECTURE", Rule: "design + structural noun"
    - Term: "design", Lexicon: "ARCHITECTURE", Context: "design + 'architecture for the screen flow'"
    - Term: "document", Lexicon: "DOCUMENT", Term_index: 1
    - Term: "requirements", Lexicon: "DOCUMENT", Term_index: 4
    - Term: "implement", Lexicon: "IMPLEMENT", Rule: "implement-fires-IMPLEMENT"
  TransitionType_candidate: "feature_addition"
  Confidence: 0.92

Key rules applied:
  "screen", "form", "interactions" fire UI.
  "design" + "architecture" fires ARCHITECTURE.
  "document" + "requirements" fire DOCUMENT.
  "implement" fires IMPLEMENT.
  Four predicates detected: [UI, ARCHITECTURE, DOCUMENT, IMPLEMENT].

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI", "ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1", PredicateId: "UI", FlowId: "ScreenInventoryFlow11", TrustState: "deterministic_generic"
    - SubPhaseId: "2.1", PredicateId: "DOCUMENT", FlowId: "BusinessRequirementsFlow21", TrustState: "deterministic_generic"
    - SubPhaseId: "3.1", PredicateId: "ARCHITECTURE", FlowId: "SystemArchitectureFlow31", TrustState: "deterministic_generic"
    - SubPhaseId: "4.1", PredicateId: "IMPLEMENT", FlowId: "ImplementationFlow41", TrustState: "deterministic_generic"
  DecisionCount: 4

Key rule applied:
  Four predicates → four routing decisions. Each predicate owns its sub-phase independently.
  EM-9 guard: no dependency_order, parallel_group, domain_label, is_primary in any decision.

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path. Four predicates matched.

---

### Layer 0-D — FlowFactory

Four flows selected:
  Flow 1: ScreenInventoryFlow11, SubPhaseId: "1.1", TrustState: "deterministic_generic"
  Flow 2: BusinessRequirementsFlow21, SubPhaseId: "2.1", TrustState: "deterministic_generic"
  Flow 3: SystemArchitectureFlow31, SubPhaseId: "3.1", TrustState: "deterministic_generic"
  Flow 4: ImplementationFlow41, SubPhaseId: "4.1", TrustState: "deterministic_generic"
  TotalFlowsSelected: 4
  FallbackUsed: false

---

### Layer 1 — TransitionOrchestratorUnit

Output:
  SelectedLogicalTransition:
    TransitionId: "transition-B01-synth-004-1.1-feature_addition"
    TransitionType: "feature_addition"
    SubPhaseId: "1.1"
    PredicateIds: ["UI", "ARCHITECTURE", "DOCUMENT", "IMPLEMENT"]
    FlowsSelected: ["ScreenInventoryFlow11", "SystemArchitectureFlow31", "BusinessRequirementsFlow21", "ImplementationFlow41"]
    flow_execution_order: ["ScreenInventoryFlow11", "SystemArchitectureFlow31", "BusinessRequirementsFlow21", "ImplementationFlow41"]
    ContextCarryForward: true

Key rules applied:
  IMPLEMENT present → TransitionType: feature_addition.
  SubPhaseId: 1.1 (UI is primary per hierarchy UI > ARCHITECTURE > IMPLEMENT > DOCUMENT).
  Execution order: ScreenInventoryFlow11 (1.1) → SystemArchitectureFlow31 (3.1) → BusinessRequirementsFlow21 (2.1) → ImplementationFlow41 (4.1).
  Note: execution order follows UI→ARCHITECTURE→DOCUMENT→IMPLEMENT convention for this scenario.

---

### Layer 2 — Four flows execute

ScreenInventoryFlow11 (sub-phase 1.1):
  Artifact: Screen inventory — 12 screens, 15 flow edges, all screens connected

SystemArchitectureFlow31 (sub-phase 3.1):
  Artifact: Architecture document — ScreenRouter, StateManager, FlowGuard, TransitionAnimator

BusinessRequirementsFlow21 (sub-phase 2.1):
  Artifact: Requirements document — 12 requirements, 24 acceptance criteria

ImplementationFlow41 (sub-phase 4.1):
  Artifact: Implementation plan — 5 form components: EmailInput, PasswordInput, NameInput, TermsCheckbox, SubmitButton

Final FormatterUnit Output:
  ArtifactsProduced: 4
  GateBResult: "PASS: all 4 flows completed; 4 artifacts produced; no dead ends in screen graph; all requirements have acceptance criteria; all components have implementation specs"

---

### Layer 3 — Teaching units confirm

  PairParser: four flow chains complete; 4 routing decisions; all four predicate families documented
  ModelTrainer:
    total_pairs_ingested: 101 (full batch)
    gate_b_passed: true
    predicate_coverage: 4
    sub_phase_coverage: 4
  WaveAcceptanceVerdict: ACCEPTED
  NextAction: PROMOTE_TO_WAVE_2_REGISTER

---

### Synthesis summary

Request → [UI, ARCHITECTURE, DOCUMENT, IMPLEMENT] → 1.1 + 3.1 + 2.1 + 4.1
→ 4 artifacts (screen inventory + architecture + requirements + implementation), feature_addition.

Four predicates. Four routing decisions. Four flows. Four artifacts.
IMPLEMENT gate fires feature_addition. All four predicates route independently.
SubPhaseId = 1.1 (UI primary per hierarchy). Execution order follows scenario convention.

This is the canonical maximum-predicate B01 trace: all four registered flows execute,
all four sub-phases are covered, and the complete onboarding UX deliverable is produced.
