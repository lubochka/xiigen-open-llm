# Cross-Layer Synthesis (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: cross_layer_synthesis
# Trace: UI+DOCUMENT+IMPLEMENT — three routing decisions; feature_addition TransitionType

---

## Synthesis trace: UI+DOCUMENT+IMPLEMENT request — full pipeline

### Request
UserRequest: "Write the complete screen inventory for the onboarding flow, document all requirements and acceptance criteria, and implement the signup form components."

---

### Layer 0-A — PromptRoutingInputCompilerUnit

Input: raw user request string

Output:
  CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
  Evidence:
    - Term: "Write", Lexicon: "IMPLEMENT", Rule: "write-always-fires-IMPLEMENT"
    - Term: "screen", Lexicon: "UI", Term_index: 4
    - Term: "form", Lexicon: "UI", Term_index: 7
    - Term: "components", Lexicon: "UI", Term_index: 9
    - Term: "document", Lexicon: "DOCUMENT", Term_index: 1
    - Term: "requirements", Lexicon: "DOCUMENT", Term_index: 4
    - Term: "criteria", Lexicon: "DOCUMENT", Term_index: 6
    - Term: "implement", Lexicon: "IMPLEMENT", Rule: "implement-fires-IMPLEMENT"
  Confidence: 0.92

Key rules applied:
  "Write" unconditionally fires IMPLEMENT.
  "screen", "form", "components" fire UI.
  "document", "requirements", "criteria" fire DOCUMENT.
  "implement" reinforces IMPLEMENT.
  Result: [UI, DOCUMENT, IMPLEMENT].

---

### Layer 0-B — MultiSubPhaseRouterUnit

Input: CompiledPredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]

Output:
  RoutingDecisions:
    - SubPhaseId: "1.1", PredicateId: "UI", FlowId: "ScreenInventoryFlow11", TrustState: "deterministic_generic"
    - SubPhaseId: "2.1", PredicateId: "DOCUMENT", FlowId: "BusinessRequirementsFlow21", TrustState: "deterministic_generic"
    - SubPhaseId: "4.1", PredicateId: "IMPLEMENT", FlowId: "ImplementationFlow41", TrustState: "deterministic_generic"
  DecisionCount: 3

Key rule applied:
  Three predicates → three routing decisions. Each routes independently.

---

### Layer 0-C — Knowledge-Edge Check

Result: no knowledge-edge path. Three predicates matched.

---

### Layer 0-D — FlowFactory

Three flows selected:
  Flow 1: ScreenInventoryFlow11, SubPhaseId: "1.1"
  Flow 2: BusinessRequirementsFlow21, SubPhaseId: "2.1"
  Flow 3: ImplementationFlow41, SubPhaseId: "4.1"
  TotalFlowsSelected: 3
  FallbackUsed: false

---

### Layer 1 — TransitionOrchestratorUnit

Output:
  SelectedLogicalTransition:
    TransitionType: "feature_addition"
    SubPhaseId: "1.1"
    PredicateIds: ["UI", "DOCUMENT", "IMPLEMENT"]
    FlowsSelected: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21", "ImplementationFlow41"]
    flow_execution_order: ["ScreenInventoryFlow11", "BusinessRequirementsFlow21", "ImplementationFlow41"]

Key rules applied:
  IMPLEMENT present → TransitionType: feature_addition.
  SubPhaseId: 1.1 (UI is primary per hierarchy).
  Execution order: 1.1 → 2.1 → 4.1 (ascending sub-phase order).

---

### Layer 2 — Three flows execute sequentially

ScreenInventoryFlow11 (sub-phase 1.1):
  Node 01 Output: DomainPacket — UI
  Node 09 Output:
    ScreenCount: 12
    AllScreensConnected: true
    GateBResult: "PASS"

BusinessRequirementsFlow21 (sub-phase 2.1):
  Node 01 Output: DomainPacket — DOCUMENT
  Node 09 Output:
    RequirementsCount: 12
    AcceptanceCriteriaCount: 24
    GateBResult: "PASS"

ImplementationFlow41 (sub-phase 4.1):
  Node 01 Output: DomainPacket — IMPLEMENT
  Node 09 Output:
    ComponentsImplemented: 5
    Components: ["EmailInput", "PasswordInput", "NameInput", "TermsCheckbox", "SubmitButton"]
    GateBResult: "PASS: 5 components implemented; API contracts complete"

---

### Layer 3 — Teaching units confirm

  PairParser: three flow chains complete; 3 routing decisions; all three flows documented
  ModelTrainer: all Gate B assertions pass
  WaveAcceptanceVerdict: ACCEPTED

---

### Synthesis summary

Request → [UI, DOCUMENT, IMPLEMENT] → 1.1 + 2.1 + 4.1
→ 12 screens + 12 requirements + 24 criteria + 5 form components, feature_addition TransitionType.
Three predicates. Three routing decisions. Three flows. Sequential execution 1.1 → 2.1 → 4.1.
IMPLEMENT gate fires feature_addition. All three predicates route independently.
