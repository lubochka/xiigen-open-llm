# Cross-Layer Synthesis (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: ARCHITECTURE+API_CONTRACT+IMPLEMENT+UI+DOCUMENT — full five-predicate trace

---

## Synthesis trace: five-predicate full sweep — complete pipeline

### Request
UserRequest: "Design the system architecture and API contracts, implement the user onboarding screens, document the functional requirements, and build the backend service."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
  Evidence:
    - Term: "screens", Lexicon: "UI"
    - Term: "onboarding", Lexicon: "context only — not a predicate"
    - Term: "document", Lexicon: "DOCUMENT"
    - Term: "functional", Lexicon: "DOCUMENT"
    - Term: "requirements", Lexicon: "DOCUMENT"
    - Term: "implement", Lexicon: "IMPLEMENT"
    - Term: "build", Lexicon: "IMPLEMENT"
    - Term: "architecture", Lexicon: "ARCHITECTURE"
    - Term: "API", Lexicon: "ARCHITECTURE"
    - Term: "service", Lexicon: "ARCHITECTURE"
  Confidence: 0.96

Key rules applied:
  "screens" → UI.
  "document", "functional", "requirements" → DOCUMENT.
  "implement", "build" → IMPLEMENT.
  "architecture", "API", "service" → ARCHITECTURE.
  "API contracts" — "API" fires ARCHITECTURE; "contracts" alone has no predicate.
  Four predicates detected: [UI, DOCUMENT, IMPLEMENT, ARCHITECTURE].

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]

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
    - SubPhaseId: "4.1"
      PredicateId: "ARCHITECTURE"
      FlowId: "ArchitectureFlow51"
      TrustState: "deterministic_generic"
  DecisionCount: 4

Key rule applied:
  Four predicates → four independent RoutingDecisions. One per predicate.
  v6-03 guard: DecisionCount (4) == len(CompiledPredicateIds) (4). Verified.
  EM-9: no dependency_order, parallel_group, domain_label, or is_primary fields.

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path invoked.
  All four predicates matched. Normal routing proceeds for all four branches.

---

### Layer 0-D — FlowFactory

Input[1.1]: deterministic_generic → ScreenInventoryFlow11, FallbackUsed: false
Input[2.1]: deterministic_generic → DocumentationFlow21, FallbackUsed: false
Input[3.1]: deterministic_generic → ImplementationFlow41, FallbackUsed: false
Input[4.1]: deterministic_generic → ArchitectureFlow51, FallbackUsed: false
All four: SelectionOutcome: "DETERMINISTIC_SELECTED"

---

### Layer 1 — TransitionOrchestratorUnit

Input: CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
       RoutingDecisions: [{1.1, UI}, {2.1, DOCUMENT}, {3.1, IMPLEMENT}, {4.1, ARCHITECTURE}]

Output:
  SelectedLogicalTransition:
    TransitionId: "transition-B01-synth-008-feature_addition"
    TransitionType: "feature_addition"
    SubPhaseId: "1.1"
    PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT", "ARCHITECTURE"]
    FlowsSelected: ["ScreenInventoryFlow11", "DocumentationFlow21", "ImplementationFlow41", "ArchitectureFlow51"]
    flow_execution_order: ["ArchitectureFlow51", "ScreenInventoryFlow11", "DocumentationFlow21", "ImplementationFlow41"]
    ContextCarryForward: true

Key rule applied:
  IMPLEMENT present → TransitionType: feature_addition.
  SubPhaseId: 1.1 (UI primary for B01 screen inventory scenario).
  flow_execution_order: ARCHITECTURE first (dependency: system design before implementation).

---

### Layer 2 — Four-flow parallel execution

Node 01 — DomainSplitterUnit:
  Input: SelectedLogicalTransition
  Output: 4 DomainPackets
    Packet[UI]: "Screen inventory: all onboarding screens and states"
    Packet[DOCUMENT]: "Functional requirements documentation per onboarding screen"
    Packet[IMPLEMENT]: "Implementation: onboarding screen code and backend service"
    Packet[ARCHITECTURE]: "System architecture and API contracts for onboarding flow"

Node 03 — SchemePlannerUnit:
  Output: WorkBranches
    SCREENS[UI]: WelcomeScreen, RoleSelection, ProfileSetup, TeamInvite, FirstRun, ActivationComplete
    DOCS[DOCUMENT]: RequirementsDoc, UseCaseDoc, SpecDoc per screen
    IMPL[IMPLEMENT]: OnboardingController, ProfileService, TeamService, ValidationLayer
    ARCH[ARCHITECTURE]: SystemDiagram, APIContractSpec, ServiceBoundaries, DataFlowDiagram

Node 09 — FormatterUnit:
  Output:
    FinalHumanResult:
      ScreenCount: 6
      DocumentationArtifacts: 3
      ImplementationModules: 4
      ArchitectureArtifacts: 4
      TransitionType: "feature_addition"
      GateBResult: "PASS: 6 screens; 3 doc artifacts; 4 implementation modules; 4 architecture artifacts; feature_addition confirmed"

---

### Layer 3 — Teaching units confirm

  PairParser: four-predicate sweep trace complete
  ModelTrainer: all Gate B assertions pass for this trace
  WaveAcceptanceVerdict: ACCEPTED for this trace

---

### Synthesis summary

Request → [UI, DOCUMENT, IMPLEMENT, ARCHITECTURE] → 1.1 + 2.1 + 3.1 + 4.1 → four flows → feature_addition.
Four predicates. Four routing decisions. Four flows. DomainSplitter produces four packets.
TransitionType: feature_addition because IMPLEMENT fires.
flow_execution_order: ARCHITECTURE before IMPLEMENT (dependency ordering).
DecisionCount (4) == predicate count (4). v6-03 guard confirmed satisfied.

B01-batch-001 complete cross-layer synthesis series (001-008):
  001: UI alone → 1.1 → documentation
  002: knowledge-edge (gap vocabulary) → KNOWLEDGE_EDGE verdict
  003: DOCUMENT+UI dual → 1.1 + 2.1 → documentation
  004: probationary flow rejection → fallback → deterministic_generic
  005: UI+IMPLEMENT → 1.1 + 3.1 → feature_addition
  006: gap+UI boundary → "navigation" gap does not suppress "page" UI
  007: UI+DOCUMENT+IMPLEMENT → 1.1 + 2.1 + 3.1 → feature_addition
  008: UI+DOCUMENT+IMPLEMENT+ARCHITECTURE → 1.1 + 2.1 + 3.1 + 4.1 → feature_addition
