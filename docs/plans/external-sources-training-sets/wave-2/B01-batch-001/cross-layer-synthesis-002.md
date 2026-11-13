# Cross-Layer Synthesis (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: UI+DOCUMENT — full trace; two routing decisions (1.1+2.1)

---

## Synthesis trace: UI+DOCUMENT request — full pipeline

### Request
UserRequest: "Map all onboarding screens and document the functional requirements and acceptance criteria for each screen."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Evidence:
    - Term: "screens", Lexicon: "UI", Term_index: 4
    - Term: "document", Lexicon: "DOCUMENT", Term_index: 1
    - Term: "functional", Lexicon: "DOCUMENT", Term_index: 9
    - Term: "requirements", Lexicon: "DOCUMENT", Term_index: 4
    - Term: "criteria", Lexicon: "DOCUMENT", Term_index: 6
  Confidence: 0.92

Key rules applied:
  "screens" fires UI. "document", "functional", "requirements", "criteria" all fire DOCUMENT.
  Independence rule: DOCUMENT is detected and recorded regardless of UI being present.
  Result: [UI, DOCUMENT].

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI", "DOCUMENT"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1", PredicateId: "UI", FlowId: "ScreenInventoryFlow11", TrustState: "deterministic_generic"
    - SubPhaseId: "2.1", PredicateId: "DOCUMENT", FlowId: "BusinessRequirementsFlow21", TrustState: "deterministic_generic"
  DecisionCount: 2

Key rule applied:
  Each predicate produces exactly one routing decision.
  UI → 1.1, DOCUMENT → 2.1. Two decisions, two flows.
  EM-9: no prohibited fields in either RoutingDecision.

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path. Two predicates matched.

---

### Layer 0-D — FlowFactory

Two flows selected:
  Flow 1: SelectedFlowId: "ScreenInventoryFlow11", SubPhaseId: "1.1", TrustState: "deterministic_generic"
  Flow 2: SelectedFlowId: "BusinessRequirementsFlow21", SubPhaseId: "2.1", TrustState: "deterministic_generic"
  TotalFlowsSelected: 2
  FallbackUsed: false

---

### Layer 1 — TransitionOrchestratorUnit

Output:
  SelectedLogicalTransition:
    TransitionType: "documentation"
    SubPhaseId: "1.1"
    PredicateIds: ["UI", "DOCUMENT"]
    FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]
    flow_execution_order: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21"]

Key rules applied:
  UI+DOCUMENT without IMPLEMENT → TransitionType: documentation.
  SubPhaseId: 1.1 (UI is primary per hierarchy UI > ARCHITECTURE > IMPLEMENT > DOCUMENT).
  Execution order: ScreenInventoryFlow11 first (1.1 before 2.1).

---

### Layer 2 — ScreenInventoryFlow11 then BusinessRequirementsFlow21

ScreenInventoryFlow11 (sub-phase 1.1):
  Node 01 — DomainSplitterUnit:
    DomainPacket: UI only
    Focus: "Screen inventory for all onboarding screens"

  Node 09 — FormatterUnit:
    FinalHumanResult:
      ScreenCount: 12
      AllScreensConnected: true
      GateBResult: "PASS"

BusinessRequirementsFlow21 (sub-phase 2.1):
  Node 01 — DomainSplitterUnit:
    DomainPacket: DOCUMENT only
    Focus: "Functional requirements and acceptance criteria per screen"

  Node 09 — FormatterUnit:
    FinalHumanResult:
      RequirementsCount: 12
      AcceptanceCriteriaCount: 24
      GateBResult: "PASS: 12 requirements; 24 acceptance criteria; all screens covered"

---

### Layer 3 — Teaching units confirm

  PairParser: both flow chains complete; 2 routing decisions; DOCUMENT independence verified
  ModelTrainer: all Gate B assertions pass
  WaveAcceptanceVerdict: ACCEPTED

---

### Synthesis summary

Request → [UI, DOCUMENT] → 1.1 + 2.1 → ScreenInventoryFlow11 + BusinessRequirementsFlow21
→ 12 screens + 12 requirements + 24 acceptance criteria, documentation TransitionType.
Two predicates. Two routing decisions. Two flows. Sequential execution (1.1 first, 2.1 second).
DOCUMENT routes independently; not suppressed by UI presence.
