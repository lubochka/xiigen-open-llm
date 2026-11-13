# Cross-Layer Synthesis (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: UI+DOCUMENT+IMPLEMENT three-flow parallel — full pipeline

---

## Synthesis trace: three-predicate parallel — full pipeline

### Request
UserRequest: "Build and document the checkout screen with full functional specifications, including all form inputs and component interactions."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  Evidence:
    - Term: "screen", Lexicon: "UI"
    - Term: "form", Lexicon: "UI"
    - Term: "component", Lexicon: "UI"
    - Term: "document", Lexicon: "DOCUMENT"
    - Term: "functional", Lexicon: "DOCUMENT"
    - Term: "specifications", Lexicon: "DOCUMENT"
    - Term: "Build", Lexicon: "IMPLEMENT"
  Confidence: 0.95

Key rules applied:
  "screen", "form", "component" — UI lexicon. UI fires.
  "document", "functional", "specifications" — DOCUMENT lexicon. DOCUMENT fires.
  "Build" — IMPLEMENT lexicon. IMPLEMENT fires.
  Three independent predicates detected.

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1"
      PredicateId: "UI"
      FlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
    - SubPhaseId: "2.1"
      PredicateId: "DOCUMENT"
      FlowId: "DocumentationFlow21"
      TrustState: "deterministic_generic"
    - SubPhaseId: "3.1"
      PredicateId: "IMPLEMENT"
      FlowId: "ImplementationFlow41"
      TrustState: "deterministic_generic"
  DecisionCount: 3

Key rule applied:
  Three predicates → three independent RoutingDecisions. One per predicate.
  EvidencePredicates per decision contains exactly one predicate. Never merged (v6-03 guard).

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path invoked.
  All three predicates matched. Normal routing proceeds for all three branches.

---

### Layer 0-D — FlowFactory

Input[1.1]: deterministic_generic → ScreenInventoryFlow11, FallbackUsed: false
Input[2.1]: deterministic_generic → DocumentationFlow21, FallbackUsed: false
Input[3.1]: deterministic_generic → ImplementationFlow41, FallbackUsed: false
All three: SelectionOutcome: "DETERMINISTIC_SELECTED"

---

### Layer 1 — TransitionOrchestratorUnit

Input: CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
       RoutingDecisions: [{1.1, UI}, {2.1, DOCUMENT}, {3.1, IMPLEMENT}]

Output:
  SelectedLogicalTransition:
    TransitionId: "transition-B01-synth-007-feature_addition"
    TransitionType: "feature_addition"
    SubPhaseId: "1.1"
    PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
    FlowsSelected: ["ScreenInventoryFlow11", "DocumentationFlow21", "ImplementationFlow41"]
    flow_execution_order: ["ScreenInventoryFlow11", "DocumentationFlow21", "ImplementationFlow41"]
    ContextCarryForward: true

Key rule applied:
  IMPLEMENT present → TransitionType: feature_addition (overrides documentation default).
  SubPhaseId: 1.1 (UI primary for screen inventory scenario).
  All three flows listed in FlowsSelected.

---

### Layer 2 — Three-flow parallel execution

Node 01 — DomainSplitterUnit:
  Input: SelectedLogicalTransition
  Output: 3 DomainPackets
    Packet[UI]: "Screen inventory for checkout: all checkout screen states and form inputs"
    Packet[DOCUMENT]: "Specification documentation for checkout: functional spec per screen"
    Packet[IMPLEMENT]: "Implementation for checkout: form input handlers, component logic, validation"

Node 03 — SchemePlannerUnit:
  Output: WorkBranches
    SCREENS[UI]: CheckoutStart, AddressEntry, PaymentEntry, OrderReview, OrderConfirmation
    DOCS[DOCUMENT]: FunctionalSpec per screen, ComponentSpec, InputSpec
    IMPL[IMPLEMENT]: AddressFormHandler, PaymentFormHandler, OrderSummaryLogic, ValidationService

Node 09 — FormatterUnit:
  Output:
    FinalHumanResult:
      ScreenCount: 5
      DocumentationArtifacts: 3
      ImplementationModules: 4
      TransitionType: "feature_addition"
      GateBResult: "PASS: 5 screens; 3 doc artifacts; 4 implementation modules; feature_addition confirmed"

---

### Layer 3 — Teaching units confirm

  PairParser: three-predicate parallel trace complete
  ModelTrainer: all Gate B assertions pass for this trace
  WaveAcceptanceVerdict: ACCEPTED for this trace

---

### Synthesis summary

Request → [UI, DOCUMENT, IMPLEMENT] → 1.1 + 2.1 + 3.1 → three flows parallel → feature_addition.
Three predicates. Three routing decisions. Three flows. DomainSplitter produces three packets.
TransitionType: feature_addition because IMPLEMENT fires (regardless of DOCUMENT+UI).
Key guard: DecisionCount must equal predicate count (3). Any merge is Corruption-v6-03.
