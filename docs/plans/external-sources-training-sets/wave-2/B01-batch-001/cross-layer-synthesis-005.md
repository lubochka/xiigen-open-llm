# Cross-Layer Synthesis (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: UI+IMPLEMENT → ScreenInventoryFlow11 + ImplementationFlow41 → feature_addition

---

## Synthesis trace: UI+IMPLEMENT dual-predicate — full pipeline

### Request
UserRequest: "Build the product listing page with a responsive grid layout and filterable component."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Evidence:
    - Term: "page", Lexicon: "UI", Term_index: 3
    - Term: "responsive", Lexicon: "UI", Term_index: 9
    - Term: "layout", Lexicon: "UI", Term_index: 8
    - Term: "component", Lexicon: "UI", Term_index: 11
    - Term: "Build", Lexicon: "IMPLEMENT", Term_index: 1
  Confidence: 0.94

Key rules applied:
  "page", "responsive", "layout", "component" — all in UI lexicon. UI fires.
  "Build" — in IMPLEMENT lexicon. IMPLEMENT fires.
  "product", "listing", "grid", "filterable" — context words; no predicate.
  Result: two predicates [UI, IMPLEMENT].

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI", "IMPLEMENT"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1"
      PredicateId: "UI"
      FlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
    - SubPhaseId: "3.1"
      PredicateId: "IMPLEMENT"
      FlowId: "ImplementationFlow41"
      TrustState: "deterministic_generic"
  DecisionCount: 2

Key rule applied:
  Two predicates → two independent RoutingDecisions. Never merged.
  UI → 1.1; IMPLEMENT → 3.1. EM-9: no dependency_order field.

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path invoked.
  Both predicates matched. Normal routing proceeds for both branches.

---

### Layer 0-D — FlowFactory

Input: SubPhaseId: "1.1", TrustState: "deterministic_generic"
Output[1.1]:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_SELECTED"

Input: SubPhaseId: "3.1", TrustState: "deterministic_generic"
Output[3.1]:
  SelectedFlowId: "ImplementationFlow41"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_SELECTED"

Key rule applied:
  deterministic_generic flows always selected without fallback.

---

### Layer 1 — TransitionOrchestratorUnit

Input: CompiledPredicateIds: ["UI", "IMPLEMENT"], RoutingDecisions: [{1.1, UI}, {3.1, IMPLEMENT}]

Output:
  SelectedLogicalTransition:
    TransitionId: "transition-B01-synth-005-feature_addition"
    TransitionType: "feature_addition"
    SubPhaseId: "1.1"
    PredicateIds: ["UI", "IMPLEMENT"]
    FlowsSelected: ["ScreenInventoryFlow11", "ImplementationFlow41"]
    flow_execution_order: ["ScreenInventoryFlow11", "ImplementationFlow41"]
    ContextCarryForward: true

Key rule applied:
  IMPLEMENT co-present → TransitionType: feature_addition.
  SubPhaseId: 1.1 (UI is primary for screen inventory context).
  EM-9 guard: Node 01 A must be SelectedLogicalTransition, not a raw string.

---

### Layer 2 — ScreenInventoryFlow11 (UI branch)

Input: SelectedLogicalTransition (SubPhaseId: 1.1, PredicateIds: [UI, IMPLEMENT])

Node 01 — DomainSplitterUnit:
  Input: SelectedLogicalTransition
  Output: DomainPacket (UI) + DomainPacket (IMPLEMENT)
  PacketFocus[UI]: "Screen inventory for product listing page: all screen and component states"
  PacketFocus[IMPLEMENT]: "Implementation for product listing page: responsive grid, filter component code"

Node 03 — SchemePlannerUnit:
  Output: WorkBranches
    - SCREEN-STATES: ProductListingEmpty, ProductListingPopulated, FilterPanelOpen, FilterPanelClosed, LoadingState
    - COMPONENTS: GridComponent, FilterComponent, PaginationComponent
    - IMPLEMENTATION: ResponsiveGridLogic, FilterLogic, SortLogic

Node 09 — FormatterUnit:
  Output:
    FinalHumanResult:
      ScreenCount: 5
      ComponentCount: 3
      ImplementationModules: 3
      TransitionType: "feature_addition"
      GateBResult: "PASS: 5 screens, 3 components, 3 implementation modules; feature_addition confirmed"

---

### Layer 3 — Teaching units confirm

  PairParser: dual-predicate UI+IMPLEMENT chain complete
  ModelTrainer: all Gate B assertions pass for this trace
  WaveAcceptanceVerdict: ACCEPTED for this trace

---

### Synthesis summary

Request → [UI, IMPLEMENT] → 1.1 + 3.1 → ScreenInventoryFlow11 + ImplementationFlow41 → 5-screen inventory + 3 implementation modules, feature_addition.
Two predicates. Two routing decisions. Two flows. DomainSplitter produces two packets. TransitionType: feature_addition because IMPLEMENT fires.
