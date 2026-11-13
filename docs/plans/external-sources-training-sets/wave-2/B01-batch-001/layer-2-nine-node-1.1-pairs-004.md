# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 1.1 (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: SignupFormDesign

---

## B01-L2-004-N01 — Node 01 DomainSplitterUnit: SignupFormDesign

```
source_id: xiigen-b01-l2-004-n01
source_title: XIIGen B01 nine-node-004 — N01 DomainSplitter for SignupFormDesign
source_license_state: first_party
contract_signature: DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-01-domain-splitter
coverage_status: covered
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B01-l2-004-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Design the signup form component with all input fields, button states, and validation interaction patterns."
  PredicateIds: ["UI"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-007-003-prompt"]

B:
  DomainPackets:
    - PacketId: "B01-l2-004-packet-ui"
      DomainTag: "UI"
      Focus: "Signup form component design: field types, input validation states, button interaction states, error/success feedback patterns"
      SourceTransitionId: "transition-B01-l2-004-1.1-feature_addition"
      UserSignal: "signup form, component, input fields, button states, validation, interaction patterns"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    SubPhaseId: "1.1"
    PacketCount: 1
    Confidence: 0.92
    Warnings: []

N01 result: ["UI"], 1.1, feature_addition — SignupForm component design started.

---

## B01-L2-004-N03 — Node 03 SchemePlannerUnit: SignupForm branches

```
source_id: xiigen-b01-l2-004-n03
source_title: XIIGen B01 nine-node-004 — N03 SchemePlanner for SignupFormDesign
source_license_state: first_party
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
node_role: node-03-scheme-planner
coverage_status: covered
pair_kind: nine_node_flow
```

A: [TranslatedDomainStatements — UI packet]
  StatementId: "B01-l2-004-stmt-ui"
  DomainTag: "UI"
  TechnicalStatement: "Signup form requires 5 input component types: email, password, full name, company (optional), and terms checkbox. Each input has 4 states: default, focused, filled, error. The primary CTA button has 3 states: default, loading, success."

B:
  WorkBranches:
    - BranchId: "B01-l2-004-branch-fields", BranchTag: "INPUT-FIELDS", Priority: 1,
      Scope: "Email, password, full name, company, terms checkbox — field types and validation rules"
    - BranchId: "B01-l2-004-branch-states", BranchTag: "INPUT-STATES", Priority: 2,
      Scope: "Default, focused, filled, error states for each input type — visual and interaction spec"
    - BranchId: "B01-l2-004-branch-buttons", BranchTag: "BUTTON-STATES", Priority: 3,
      Scope: "Primary CTA: default, loading, success; secondary: back/cancel"
    - BranchId: "B01-l2-004-branch-validation", BranchTag: "VALIDATION-PATTERNS", Priority: 4,
      Scope: "Inline validation timing, error message positioning, field highlight on error"
    - BranchId: "B01-l2-004-branch-layout", BranchTag: "FORM-LAYOUT", Priority: 5,
      Scope: "Single-column layout, label positioning, field spacing, submit button placement"

  [unit result envelope]
    UnitId: "node-03-scheme-planner"
    BranchCount: 5
    Confidence: 0.92
    Warnings: []

---

## B01-L2-004-N09 — Node 09 FormatterUnit: SignupForm final result

```
source_id: xiigen-b01-l2-004-n09
source_title: XIIGen B01 nine-node-004 — N09 FormatterUnit for SignupFormDesign
node_role: node-09-formatter
coverage_status: covered
pair_kind: nine_node_flow
```

A: [ArbiterVerdicts — all approved]

B:
  FinalHumanResult:
    Title: "Signup Form Component Design Specification — B01-batch-004"
    SubPhaseId: "1.1"
    ComponentCount: 5
    Components:
      - EmailInput: 4 states (default, focused, filled, error); validation: format check on blur
      - PasswordInput: 4 states + visibility toggle; validation: min 8 chars, strength indicator
      - FullNameInput: 4 states; validation: non-empty on blur
      - CompanyInput (optional): 4 states; no validation required
      - TermsCheckbox: 2 states (unchecked, checked); required for form submit
    Buttons:
      - PrimarySubmit: default, loading (spinner), success (checkmark)
      - BackLink: text link, hover state
    Layout: Single-column; labels above inputs; 24px field spacing; submit button full-width at bottom
    GateBResult: "PASS: 5 components specified; all states documented; validation rules complete"

N09 result: SignupForm component spec complete; 5 components.
