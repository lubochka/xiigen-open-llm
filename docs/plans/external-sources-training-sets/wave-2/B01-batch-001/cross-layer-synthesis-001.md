# Cross-Layer Synthesis (001)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: UI alone (1.1) — full layer trace from request to formatter output

---

## Synthesis trace: UI-only request — full pipeline

### Request
UserRequest: "Map all the screens in the SaaS onboarding flow, grouped by journey stage."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI"]
  Evidence:
    - Term: "screens", Lexicon: "UI", Term_index: 4
  TransitionId_candidate: "transition-B01-synth-001-1.1"
  Confidence: 0.92

Key rule applied:
  "screens" is term 4 in the 22-term UI lexicon. Fires UI.
  "onboarding", "journey stage" — context words; no predicate.
  Result: single predicate [UI].

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1"
      PredicateId: "UI"
      FlowId: "ScreenInventoryFlow11"
      TrustState: "deterministic_generic"
  DecisionCount: 1

Key rule applied:
  UI → SubPhaseId 1.1. ScreenInventoryFlow11 is the registered flow for 1.1.
  EM-9: no dependency_order, parallel_group, domain_label, or is_primary fields in RoutingDecision.

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path invoked.
  At least one predicate matched. Normal routing proceeds.

---

### Layer 0-D — FlowFactory

Input: SubPhaseId: "1.1", TrustState: "deterministic_generic"

Output:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_SELECTED"

Key rule applied:
  deterministic_generic flows are always selected without candidate evaluation.
  FallbackUsed: false because no probationary candidate was proposed and rejected.

---

### Layer 1 — TransitionOrchestratorUnit

Input: CompiledPredicateIds: ["UI"], RoutingDecisions: [{1.1, UI}]

Output:
  SelectedLogicalTransition:
    TransitionId: "transition-B01-synth-001-1.1-feature_addition"
    TransitionType: "feature_addition"
    SubPhaseId: "1.1"
    PredicateIds: ["UI"]
    FlowsSelected: ["ScreenInventoryFlow11"]
    flow_execution_order: ["ScreenInventoryFlow11"]
    ContextCarryForward: true

Key rule applied:
  UI alone → TransitionType: feature_addition.
  SubPhaseId: 1.1 (UI is the only and primary predicate).
  Input to Layer 2 is SelectedLogicalTransition — never a raw string (EM-9 guard).

---

### Layer 2 — ScreenInventoryFlow11 (nine-node chain)

Input: SelectedLogicalTransition (TransitionId, SubPhaseId: 1.1, PredicateIds: [UI])

Node 01 — DomainSplitterUnit:
  Input: SelectedLogicalTransition
  Output: DomainPacket — UI only
  PacketFocus: "Screen inventory for onboarding flow: all screen types, entry/exit conditions, screen graph"

Node 03 — SchemePlannerUnit:
  Output: WorkBranches — grouped by journey stage
    - AWARENESS-STAGE: LandingPage, ProductTour
    - SIGNUP-STAGE: SignupForm, EmailVerification
    - SETUP-STAGE: ProfileSetup, RoleSelection, TeamInvite
    - ACTIVATION-STAGE: FirstRun, FeatureDiscovery, ActivationComplete, DashboardLanding, UpgradePrompt

Node 09 — FormatterUnit:
  Output:
    FinalHumanResult:
      ScreenCount: 12
      ScreensByStage:
        Awareness: 2, Signup: 2, Setup: 3, Activation: 5 (including 1 optional)
      AllScreensConnected: true
      GateBResult: "PASS: 12 screens; 4 journey stages; all screens connected in flow graph"

---

### Layer 3 — Teaching units confirm

  PairParser: chain complete for UI-only path
  ModelTrainer: all Gate B assertions pass for this trace
  WaveAcceptanceVerdict: ACCEPTED for this trace

---

### Synthesis summary

Request → [UI] → 1.1 → ScreenInventoryFlow11 → 12-screen inventory, feature_addition.
Single predicate. Single routing decision. Single flow. Straightforward path with no fallback.
