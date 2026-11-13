# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 1.1 User Flows and Screen Inventory
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Contract: ScreenInventoryFlow11 — nine sequential logical units
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: nine_node_flow

---

## Method note

Layer 2 pairs cover each of the nine canonical logical units inside the
ScreenInventoryFlow for sub-phase 1.1. The flow is invoked with the
SelectedLogicalTransition from Layer 1 pair B01B1-WDA-T1-001.

Node input chaining rule:
- Nodes 03, 06, 07, 09 take collection inputs (arrays) assembled from all
  preceding per-item runs.
- Nodes 02, 04, 05, 08 process one item at a time; each pair shows a
  representative item.
- A must always be the canonical output type of the preceding node.

Node 01 A must be SelectedLogicalTransition — never a raw user prompt string.

---

## Pair B01B1-1.1-N01-001 — Node 01 DomainSplitterUnit

```
source_id: xiigen-b01-1.1-n01-001
source_title: XIIGen B01 nine-node 1.1 — Node 01 DomainSplitterUnit
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
  TransitionId: "transition-B01-t1-001-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "We need to map all screen flows and create a complete screen inventory for the SaaS onboarding UX, covering signup, email activation, and the first-run experience."
  PredicateIds: ["UI"]
  ContextCarryForward: true
  EvidenceIds: ["B01-compiler-001-prompt"]
  Metadata:
    transition_classification_basis: "add_new_capability_signal: screen_inventory, user_flows"

B:
  DomainPackets:
    - PacketId: "B01-1.1-packet-ui"
      DomainTag: "UI"
      Focus: "Screen identification and categorization, user journey flow mapping, navigation topology, entry and exit conditions per screen, screen role in the onboarding objective"
      SourceTransitionId: "transition-B01-t1-001-1.1-feature_addition"
      UserSignal: "screen flows, screen inventory, SaaS onboarding UX, signup, email activation, first-run experience"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    UnitRole: "domain-splitter"
    SubPhaseId: "1.1"
    PacketCount: 1
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N02-001 — Node 02 DomainTranslatorUnit

```
source_id: xiigen-b01-1.1-n02-001
source_title: XIIGen B01 nine-node 1.1 — Node 02 DomainTranslatorUnit
source_license_state: first_party
contract_signature: DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-02-domain-translator
coverage_status: covered
pair_kind: nine_node_flow
```

Note: DomainTranslatorUnit processes one DomainPacket at a time. This pair
shows the run for the single UI packet from Node 01.

A: [DomainPacket from Node 01 B — UI packet]
  PacketId: "B01-1.1-packet-ui"
  DomainTag: "UI"
  Focus: "Screen identification, user journey flow mapping, navigation topology, entry/exit conditions per screen"
  SourceTransitionId: "transition-B01-t1-001-1.1-feature_addition"
  UserSignal: "screen flows, screen inventory, SaaS onboarding UX, signup, email activation, first-run experience"

B:
  StatementId: "B01-1.1-stmt-ui"
  DomainTag: "UI"
  TechnicalStatement: "The SaaS onboarding user experience requires a systematic screen inventory organized as a directed flow graph. The inventory must cover four journey stages: (1) Signup and account creation — landing page through signup form, email-sent confirmation; (2) Email activation — activation link landing, account confirmed, optional password set; (3) Onboarding profile setup — team name, role selection, workspace configuration; (4) First-run feature discovery — guided tour trigger, key feature highlights, completion state. Each screen must carry a canonical screen ID, display name, screen type (form, confirmation, informational, interactive), entry conditions, and exit transitions."
  SourcePacketId: "B01-1.1-packet-ui"
  KeyTerms: ["screen inventory", "directed flow graph", "entry conditions", "exit transitions", "signup", "activation", "first-run"]

  [unit result envelope]
    UnitId: "node-02-domain-translator"
    UnitRole: "domain-translator"
    SubPhaseId: "1.1"
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N03-001 — Node 03 SchemePlannerUnit

```
source_id: xiigen-b01-1.1-n03-001
source_title: XIIGen B01 nine-node 1.1 — Node 03 SchemePlannerUnit
source_license_state: first_party
contract_signature: SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-03-scheme-planner
coverage_status: covered
pair_kind: nine_node_flow
```

A: [All TranslatedDomainStatements — one per DomainPacket from Node 01]
  - StatementId: "B01-1.1-stmt-ui"
    DomainTag: "UI"
    TechnicalStatement: "Screen inventory must cover four journey stages: signup, activation, profile setup, and first-run discovery. Each screen carries canonical ID, name, type, entry conditions, and exit transitions."

B:
  WorkBranches:
    - BranchId: "B01-1.1-branch-signup"
      BranchTag: "SIGNUP-SCREENS"
      Scope: "Landing page, signup form, email-sent confirmation — entry conditions: direct URL or referral link; exit: successful form submit or back navigation"
      Priority: 1
      SourceStatementIds: ["B01-1.1-stmt-ui"]

    - BranchId: "B01-1.1-branch-activation"
      BranchTag: "ACTIVATION-SCREENS"
      Scope: "Email activation link landing, account confirmed state, optional password-set screen — entry: activation token in URL; exit: proceed to profile setup"
      Priority: 2
      SourceStatementIds: ["B01-1.1-stmt-ui"]

    - BranchId: "B01-1.1-branch-setup"
      BranchTag: "PROFILE-SETUP-SCREENS"
      Scope: "Team name input, role selection, workspace configuration — entry: successful activation; exit: all required fields submitted"
      Priority: 3
      SourceStatementIds: ["B01-1.1-stmt-ui"]

    - BranchId: "B01-1.1-branch-firstrun"
      BranchTag: "FIRST-RUN-SCREENS"
      Scope: "Guided tour trigger, key feature highlights (up to 3 steps), completion state — entry: first post-setup dashboard load; exit: tour dismissed or completed"
      Priority: 4
      SourceStatementIds: ["B01-1.1-stmt-ui"]

  [unit result envelope]
    UnitId: "node-03-scheme-planner"
    UnitRole: "scheme-planner"
    SubPhaseId: "1.1"
    BranchCount: 4
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N04-001 — Node 04 DomainProcessorUnit

```
source_id: xiigen-b01-1.1-n04-001
source_title: XIIGen B01 nine-node 1.1 — Node 04 DomainProcessorUnit
source_license_state: first_party
contract_signature: DomainProcessorUnit : ILogicalUnit<WorkBranch, ProcessedBranchResult>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-04-domain-processor
coverage_status: covered
pair_kind: nine_node_flow
```

Note: DomainProcessorUnit processes one WorkBranch at a time. This pair
shows the run for the SIGNUP-SCREENS branch.

A: [Representative WorkBranch from Node 03 B — SIGNUP-SCREENS branch]
  BranchId: "B01-1.1-branch-signup"
  BranchTag: "SIGNUP-SCREENS"
  Scope: "Landing page, signup form, email-sent confirmation"
  Priority: 1
  SourceStatementIds: ["B01-1.1-stmt-ui"]

B:
  ProcessedResultId: "B01-1.1-proc-signup"
  BranchId: "B01-1.1-branch-signup"
  BranchTag: "SIGNUP-SCREENS"
  Screens:
    - ScreenId: "SCR-001"
      CanonicalName: "LandingPage"
      ScreenType: "informational"
      EntryConditions: ["direct URL access", "referral link with campaign parameter", "return visit without active session"]
      ExitTransitions:
        - Action: "click primary CTA (Get started free)"
          DestinationScreenId: "SCR-002"
        - Action: "click Login link"
          DestinationScreenId: "SCR-010 (LoginPage — outside onboarding flow)"
      Notes: "Marketing content; no authentication required; anonymous session"

    - ScreenId: "SCR-002"
      CanonicalName: "SignupForm"
      ScreenType: "form"
      EntryConditions: ["CTA click from SCR-001", "direct URL /signup"]
      ExitTransitions:
        - Action: "submit valid form (email + password)"
          DestinationScreenId: "SCR-003"
        - Action: "back navigation"
          DestinationScreenId: "SCR-001"
        - Action: "OAuth provider button (Google/GitHub)"
          DestinationScreenId: "SCR-004 (OAuthCallback — conditional)"
      FormFields: ["email (required)", "password (required, min 8 chars)", "full name (required)"]
      Notes: "Inline validation; password strength indicator; OAuth social login shortcuts"

    - ScreenId: "SCR-003"
      CanonicalName: "EmailSentConfirmation"
      ScreenType: "confirmation"
      EntryConditions: ["successful signup form submission (SCR-002)"]
      ExitTransitions:
        - Action: "click activation link in email"
          DestinationScreenId: "SCR-011 (ActivationLanding — next branch)"
        - Action: "click Resend email"
          DestinationScreenId: "SCR-003 (self — resend triggered)"
        - Action: "click Change email address"
          DestinationScreenId: "SCR-002"
      Notes: "Polling optional; auto-advance if tab detects activation in background"

  [unit result envelope]
    UnitId: "node-04-domain-processor"
    UnitRole: "domain-processor"
    SubPhaseId: "1.1"
    ScreenCount: 3
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N05-001 — Node 05 ApplierUnit

```
source_id: xiigen-b01-1.1-n05-001
source_title: XIIGen B01 nine-node 1.1 — Node 05 ApplierUnit
source_license_state: first_party
contract_signature: ApplierUnit : ILogicalUnit<ProcessedBranchResult, AppliedBranchResult>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-05-applier
coverage_status: covered
pair_kind: nine_node_flow
```

Note: ApplierUnit processes one ProcessedBranchResult at a time. This pair
shows the run for the SIGNUP-SCREENS branch result.

A: [ProcessedBranchResult from Node 04 B — SIGNUP-SCREENS branch]
  ProcessedResultId: "B01-1.1-proc-signup"
  BranchId: "B01-1.1-branch-signup"
  BranchTag: "SIGNUP-SCREENS"
  ScreenCount: 3
  Screens: [SCR-001 LandingPage, SCR-002 SignupForm, SCR-003 EmailSentConfirmation]

B:
  AppliedResultId: "B01-1.1-applied-signup"
  BranchId: "B01-1.1-branch-signup"
  BranchTag: "SIGNUP-SCREENS"
  AppliedItems:
    - ItemId: "applied-B01-1.1-01"
      Target: "Screen inventory — SCR-001 through SCR-003"
      Change: "Register SCR-001 (LandingPage), SCR-002 (SignupForm), SCR-003 (EmailSentConfirmation) in the master screen inventory with full entry/exit metadata."
      Outcome: "3 screens registered; signup branch complete; 4 exit transitions documented; form field schema captured"

    - ItemId: "applied-B01-1.1-02"
      Target: "Flow graph — signup segment"
      Change: "Add directed edges: SCR-001→SCR-002 (CTA click), SCR-002→SCR-003 (submit), SCR-002→SCR-001 (back), SCR-003→SCR-003 (resend self-loop), SCR-003→SCR-002 (change email)."
      Outcome: "Signup segment of flow graph complete; no dead-end screens identified; self-loop on SCR-003 noted"

    - ItemId: "applied-B01-1.1-03"
      Target: "Screen type classification"
      Change: "SCR-001 classified informational; SCR-002 classified form; SCR-003 classified confirmation. Classifications align with design system rendering rules."
      Outcome: "Type classifications set; design system can auto-select component templates per type"

  ApplicationSummary: "Signup branch inventory complete: 3 screens, 5 transitions, 1 self-loop, form field schema captured, screen types classified."
  Warnings: []

  [unit result envelope]
    UnitId: "node-05-applier"
    UnitRole: "applier"
    SubPhaseId: "1.1"
    AppliedItemCount: 3
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N06-001 — Node 06 ResultTranslatorUnit

```
source_id: xiigen-b01-1.1-n06-001
source_title: XIIGen B01 nine-node 1.1 — Node 06 ResultTranslatorUnit
source_license_state: first_party
contract_signature: ResultTranslatorUnit : ILogicalUnit<AppliedBranchResult[], TranslatedResultFragment[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-06-result-translator
coverage_status: covered
pair_kind: nine_node_flow
```

A: [All AppliedBranchResults — one per WorkBranch from Node 03, processed through Nodes 04-05]
  - AppliedResultId: "B01-1.1-applied-signup"
    BranchTag: "SIGNUP-SCREENS"
    AppliedItemCount: 3
  - AppliedResultId: "B01-1.1-applied-activation"
    BranchTag: "ACTIVATION-SCREENS"
    AppliedItemCount: 3
  - AppliedResultId: "B01-1.1-applied-setup"
    BranchTag: "PROFILE-SETUP-SCREENS"
    AppliedItemCount: 3
  - AppliedResultId: "B01-1.1-applied-firstrun"
    BranchTag: "FIRST-RUN-SCREENS"
    AppliedItemCount: 3

B:
  TranslatedResultFragments:
    - FragmentId: "frag-B01-1.1-screen-inventory"
      FragmentType: "screen_inventory"
      Content:
        TotalScreenCount: 12
        Screens:
          - ScreenId: "SCR-001", Name: "LandingPage", Type: "informational", Branch: "signup"
          - ScreenId: "SCR-002", Name: "SignupForm", Type: "form", Branch: "signup"
          - ScreenId: "SCR-003", Name: "EmailSentConfirmation", Type: "confirmation", Branch: "signup"
          - ScreenId: "SCR-011", Name: "ActivationLanding", Type: "informational", Branch: "activation"
          - ScreenId: "SCR-012", Name: "AccountConfirmed", Type: "confirmation", Branch: "activation"
          - ScreenId: "SCR-013", Name: "PasswordSet", Type: "form", Branch: "activation"
          - ScreenId: "SCR-021", Name: "TeamNameInput", Type: "form", Branch: "setup"
          - ScreenId: "SCR-022", Name: "RoleSelection", Type: "interactive", Branch: "setup"
          - ScreenId: "SCR-023", Name: "WorkspaceConfig", Type: "form", Branch: "setup"
          - ScreenId: "SCR-031", Name: "GuidedTourTrigger", Type: "interactive", Branch: "first-run"
          - ScreenId: "SCR-032", Name: "FeatureHighlights", Type: "interactive", Branch: "first-run"
          - ScreenId: "SCR-033", Name: "OnboardingComplete", Type: "confirmation", Branch: "first-run"
      SourceBranchIds: ["B01-1.1-branch-signup", "B01-1.1-branch-activation", "B01-1.1-branch-setup", "B01-1.1-branch-firstrun"]

    - FragmentId: "frag-B01-1.1-flow-graph"
      FragmentType: "flow_graph"
      Content:
        Edges:
          - From: "SCR-001", To: "SCR-002", Trigger: "CTA click"
          - From: "SCR-002", To: "SCR-003", Trigger: "form submit (success)"
          - From: "SCR-002", To: "SCR-001", Trigger: "back navigation"
          - From: "SCR-003", To: "SCR-011", Trigger: "email activation link"
          - From: "SCR-003", To: "SCR-003", Trigger: "resend (self-loop)"
          - From: "SCR-011", To: "SCR-012", Trigger: "token validated"
          - From: "SCR-012", To: "SCR-013", Trigger: "set password prompt"
          - From: "SCR-012", To: "SCR-021", Trigger: "skip (OAuth accounts)"
          - From: "SCR-013", To: "SCR-021", Trigger: "password saved"
          - From: "SCR-021", To: "SCR-022", Trigger: "team name submitted"
          - From: "SCR-022", To: "SCR-023", Trigger: "role selected"
          - From: "SCR-023", To: "SCR-031", Trigger: "config saved"
          - From: "SCR-031", To: "SCR-032", Trigger: "start tour"
          - From: "SCR-031", To: "SCR-033", Trigger: "skip tour"
          - From: "SCR-032", To: "SCR-033", Trigger: "tour steps complete"
        TotalEdges: 15
        DeadEnds: []
        SelfLoops: ["SCR-003"]
      SourceBranchIds: ["B01-1.1-branch-signup", "B01-1.1-branch-activation", "B01-1.1-branch-setup", "B01-1.1-branch-firstrun"]

  [unit result envelope]
    UnitId: "node-06-result-translator"
    UnitRole: "result-translator"
    SubPhaseId: "1.1"
    FragmentCount: 2
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N07-001 — Node 07 ArbiterSelectorUnit

```
source_id: xiigen-b01-1.1-n07-001
source_title: XIIGen B01 nine-node 1.1 — Node 07 ArbiterSelectorUnit
source_license_state: first_party
contract_signature: ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-07-arbiter-selector
coverage_status: covered
pair_kind: nine_node_flow
```

A: [TranslatedResultFragments from Node 06 B — 2 fragments]
  - FragmentId: "frag-B01-1.1-screen-inventory", FragmentType: "screen_inventory"
  - FragmentId: "frag-B01-1.1-flow-graph", FragmentType: "flow_graph"

B:
  ArbiterReviewRequests:
    - RequestId: "arbiter-req-B01-1.1-01"
      ArbiterRole: "ux-completeness-arbiter"
      FragmentIds: ["frag-B01-1.1-screen-inventory"]
      ReviewFocus: "Verify the screen inventory covers all four onboarding journey stages — no stage missing, no orphan screens, all screen types correctly classified"
      AcceptanceCriteria: "ScreenCount >= 10; all four branches represented; no screen without an entry condition; no screen without at least one exit transition"

    - RequestId: "arbiter-req-B01-1.1-02"
      ArbiterRole: "flow-integrity-arbiter"
      FragmentIds: ["frag-B01-1.1-flow-graph"]
      ReviewFocus: "Verify the flow graph has no dead-end screens, all screens are reachable from SCR-001, and all transitions are explicitly typed"
      AcceptanceCriteria: "DeadEnds == []; all 12 screens appear as graph nodes; EdgeCount >= 12"

  [unit result envelope]
    UnitId: "node-07-arbiter-selector"
    UnitRole: "arbiter-selector"
    SubPhaseId: "1.1"
    RequestCount: 2
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N08-001 — Node 08 ArbiterUnit

```
source_id: xiigen-b01-1.1-n08-001
source_title: XIIGen B01 nine-node 1.1 — Node 08 ArbiterUnit
source_license_state: first_party
contract_signature: ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-08-arbiter
coverage_status: covered
pair_kind: nine_node_flow
```

Note: ArbiterUnit processes one ArbiterReviewRequest at a time. This pair
shows the run for the UX completeness review request.

A: [Representative ArbiterReviewRequest from Node 07 B — UX completeness request]
  RequestId: "arbiter-req-B01-1.1-01"
  ArbiterRole: "ux-completeness-arbiter"
  FragmentIds: ["frag-B01-1.1-screen-inventory"]
  ReviewFocus: "Verify screen inventory covers all four onboarding journey stages"
  AcceptanceCriteria: "ScreenCount >= 10; all four branches represented; no screen without entry condition or exit transition"

B:
  VerdictId: "verdict-B01-1.1-01-ux"
  RequestId: "arbiter-req-B01-1.1-01"
  ArbiterRole: "ux-completeness-arbiter"
  Verdict: "PASS"
  ReviewedFragmentId: "frag-B01-1.1-screen-inventory"
  Findings:
    - FindingId: "finding-B01-1.1-01"
      Type: "pass"
      Description: "ScreenCount 12 satisfies threshold >= 10. All four branches (signup, activation, setup, first-run) are represented with 3 screens each. Every screen has at least one entry condition and at least one exit transition."
    - FindingId: "finding-B01-1.1-02"
      Type: "observation"
      Description: "SCR-013 (PasswordSet) is on an optional path for OAuth users who skip it. This is correctly modeled — SCR-012 has two exit transitions: one to SCR-013 (password set) and one directly to SCR-021 (skip for OAuth)."
  CriteriaResult:
    ScreenCount: "12 (threshold >= 10: PASS)"
    AllBranchesRepresented: "true (4 of 4: PASS)"
    NoOrphanScreens: "true (all have entry and exit: PASS)"
  AcceptanceMet: true

  [unit result envelope]
    UnitId: "node-08-arbiter"
    UnitRole: "arbiter"
    SubPhaseId: "1.1"
    Confidence: 0.92
    Warnings: []

---

## Pair B01B1-1.1-N09-001 — Node 09 FormatterUnit

```
source_id: xiigen-b01-1.1-n09-001
source_title: XIIGen B01 nine-node 1.1 — Node 09 FormatterUnit
source_license_state: first_party
contract_signature: FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
transition_type: feature_addition
phase: 1
sub_phase: 1.1
task_type: ui-ux-definition
node_role: node-09-formatter
coverage_status: covered
pair_kind: nine_node_flow
```

A: [ReviewedResultSet — all ArbiterVerdicts from Node 08 runs]
  Verdicts:
    - VerdictId: "verdict-B01-1.1-01-ux"
      ArbiterRole: "ux-completeness-arbiter"
      Verdict: "PASS"
      AcceptanceMet: true
    - VerdictId: "verdict-B01-1.1-02-flow"
      ArbiterRole: "flow-integrity-arbiter"
      Verdict: "PASS"
      AcceptanceMet: true

B:
  FlowId: "screen-inventory-flow-1.1"
  ScenarioId: "adaptive-logical-orchestration"
  OverallVerdict: "PASS"
  FinalHumanResult: |
    Screen inventory and user flow map for SaaS onboarding (sub-phase 1.1):

    Total screens: 12 across four journey stages.

    Stage 1 — Signup (SCR-001 through SCR-003):
      SCR-001 LandingPage (informational) → SCR-002 on CTA click
      SCR-002 SignupForm (form) → SCR-003 on submit; back to SCR-001
      SCR-003 EmailSentConfirmation (confirmation) → SCR-011 on email link;
        self-loop on resend; back to SCR-002 to change email

    Stage 2 — Activation (SCR-011 through SCR-013):
      SCR-011 ActivationLanding → SCR-012 on token validated
      SCR-012 AccountConfirmed → SCR-013 (password set) or SCR-021 (OAuth skip)
      SCR-013 PasswordSet (form) → SCR-021 on save

    Stage 3 — Profile Setup (SCR-021 through SCR-023):
      SCR-021 TeamNameInput → SCR-022 on submit
      SCR-022 RoleSelection → SCR-023 on selection
      SCR-023 WorkspaceConfig → SCR-031 on save

    Stage 4 — First-Run Experience (SCR-031 through SCR-033):
      SCR-031 GuidedTourTrigger → SCR-032 (start tour) or SCR-033 (skip)
      SCR-032 FeatureHighlights → SCR-033 on completion
      SCR-033 OnboardingComplete (confirmation) — terminal screen

    Flow graph: 15 directed edges, 0 dead-ends, 1 self-loop (SCR-003 resend).
    All screens reachable from SCR-001. Arbiter reviews: PASS (completeness)
    and PASS (flow integrity).

  Metrics:
    node_count: "9"
    sub_phase_id: "1.1"
    node_01_unit_id: "node-01-domain-splitter"
    node_02_unit_id: "node-02-domain-translator"
    node_03_unit_id: "node-03-scheme-planner"
    node_04_unit_id: "node-04-domain-processor"
    node_05_unit_id: "node-05-applier"
    node_06_unit_id: "node-06-result-translator"
    node_07_unit_id: "node-07-arbiter-selector"
    node_08_unit_id: "node-08-arbiter"
    node_09_unit_id: "node-09-formatter"
    branch_count: "4"
    screen_count: "12"
    edge_count: "15"
    dead_end_count: "0"
    self_loop_count: "1"
    fragment_count: "2"
    arbiter_request_count: "2"
    verdict_count: "2"

  EvidenceIds:
    - "B01-1.1-stmt-ui"
    - "B01-1.1-branch-signup"
    - "B01-1.1-branch-activation"
    - "B01-1.1-branch-setup"
    - "B01-1.1-branch-firstrun"
    - "frag-B01-1.1-screen-inventory"
    - "frag-B01-1.1-flow-graph"
    - "verdict-B01-1.1-01-ux"
    - "verdict-B01-1.1-02-flow"

  Warnings: []

  [unit result envelope]
    UnitId: "node-09-formatter"
    UnitRole: "formatter"
    SubPhaseId: "1.1"
    Confidence: 0.92
    Warnings: []
