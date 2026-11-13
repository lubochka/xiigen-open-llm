# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 1.1 (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: PasswordResetFlow

---

## B01-L2-006-N01 — Node 01 DomainSplitterUnit: PasswordResetFlow

```
source_id: xiigen-b01-l2-006-n01
source_title: XIIGen B01 nine-node-006 — N01 DomainSplitter for PasswordResetFlow
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
  TransitionId: "transition-B01-l2-006-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Map the password reset screen flow and define the requirements for each screen's interaction."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-005-003-prompt"]

B:
  DomainPackets:
    - PacketId: "B01-l2-006-packet-ui"
      DomainTag: "UI"
      Focus: "Password reset screen sequence, form interactions, success and error states at each step"
      SourceTransitionId: "transition-B01-l2-006-1.1-feature_addition"
      UserSignal: "password reset, screen flow, screen, interaction"

    - PacketId: "B01-l2-006-packet-doc"
      DomainTag: "DOCUMENT"
      Focus: "Password reset requirements: acceptance criteria for each screen's behavior, edge cases (expired token, invalid email)"
      SourceTransitionId: "transition-B01-l2-006-1.1-feature_addition"
      UserSignal: "define, requirements, interaction"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    SubPhaseId: "1.1"
    PacketCount: 2
    Confidence: 0.92
    Warnings: []

N01 result: ["UI", "DOCUMENT"], 1.1, feature_addition — PasswordReset flow started.

---

## B01-L2-006-N04 — Node 04 DomainProcessorUnit: RequestReset branch

```
source_id: xiigen-b01-l2-006-n04
source_title: XIIGen B01 nine-node-006 — N04 DomainProcessor for PasswordResetFlow
node_role: node-04-domain-processor
coverage_status: covered
pair_kind: nine_node_flow
```

A: [WorkBranch — REQUEST-RESET branch]
  BranchId: "B01-l2-006-branch-request", BranchTag: "REQUEST-RESET", Priority: 1
  Scope: "Email entry form, submit action, confirmation display"

B:
  ProcessedResultId: "B01-l2-006-proc-request"
  Screens:
    - ScreenId: "PWR-001", CanonicalName: "ForgotPasswordEntry", ScreenType: "form",
      EntryConditions: ["click Forgot Password on login screen"],
      ExitTransitions: [
        {Action: "submit valid email", DestinationScreenId: "PWR-002"},
        {Action: "submit unknown email", DestinationScreenId: "PWR-002 (same — security: no disclosure)"},
        {Action: "back to login", DestinationScreenId: "login (outside flow)"}
      ],
      FormFields: ["email address (required)"],
      Buttons: ["Send Reset Link", "Back to Login"]

    - ScreenId: "PWR-002", CanonicalName: "ResetEmailSent", ScreenType: "confirmation",
      EntryConditions: ["from PWR-001 (any email submit — security)"],
      ExitTransitions: [
        {Action: "click reset link in email", DestinationScreenId: "PWR-003"},
        {Action: "resend link", DestinationScreenId: "PWR-002 (self)"},
        {Action: "back to login", DestinationScreenId: "login"}
      ]

  [unit result envelope]
    UnitId: "node-04-domain-processor"
    ScreenCount: 2
    Confidence: 0.92
    Warnings: []

---

## B01-L2-006-N09 — Node 09 FormatterUnit: PasswordReset final result

```
source_id: xiigen-b01-l2-006-n09
source_title: XIIGen B01 nine-node-006 — N09 FormatterUnit for PasswordResetFlow
node_role: node-09-formatter
coverage_status: covered
pair_kind: nine_node_flow
```

A: [ArbiterVerdicts — approved]

B:
  FinalHumanResult:
    Title: "Password Reset Screen Flow + Requirements — B01-batch-006"
    SubPhaseId: "1.1"
    ScreenCount: 4
    Screens:
      - PWR-001 ForgotPasswordEntry (form)
      - PWR-002 ResetEmailSent (confirmation)
      - PWR-003 NewPasswordEntry (form) — token validated, new password input
      - PWR-004 PasswordResetSuccess (confirmation) — reset complete, redirect to login
    RequirementsDocumented: true
    RequirementItems:
      - "Security: no email disclosure on unknown email submission"
      - "Token expiry: 60 minutes; show expired-token error screen if token used after expiry"
      - "Edge case: expired token → PWR-005 TokenExpired screen → resend flow"
    GateBResult: "PASS: 4 screens + requirements doc; edge cases covered; no dead ends"

N09 result: PasswordReset flow + requirements doc complete; 4 screens.
