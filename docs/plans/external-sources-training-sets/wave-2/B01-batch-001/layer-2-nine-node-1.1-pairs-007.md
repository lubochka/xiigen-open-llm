# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 1.1 (007)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: OnboardingWizardFlow

---

## B01-L2-007-N01 — Node 01 DomainSplitterUnit: OnboardingWizardFlow

```
source_id: xiigen-b01-l2-007-n01
source_title: XIIGen B01 nine-node-007 — N01 DomainSplitter for OnboardingWizardFlow
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
  TransitionId: "transition-B01-l2-007-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Map the five-step onboarding wizard screens with progress indicator, back/next navigation, and completion state."
  PredicateIds: ["UI"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-006-002-prompt"]

B:
  DomainPackets:
    - PacketId: "B01-l2-007-packet-ui"
      DomainTag: "UI"
      Focus: "Five-step onboarding wizard: step screens, progress indicator, navigation (back/next/skip), completion state, exit conditions"
      SourceTransitionId: "transition-B01-l2-007-1.1-feature_addition"
      UserSignal: "five-step, onboarding wizard, screens, progress indicator, back/next, completion"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    SubPhaseId: "1.1"
    PacketCount: 1
    Confidence: 0.92
    Warnings: []

N01 result: ["UI"], 1.1, feature_addition — Onboarding wizard screen mapping started.

---

## B01-L2-007-N03 — Node 03 SchemePlannerUnit: Wizard steps branches

```
source_id: xiigen-b01-l2-007-n03
source_title: XIIGen B01 nine-node-007 — N03 SchemePlanner for OnboardingWizardFlow
node_role: node-03-scheme-planner
coverage_status: covered
pair_kind: nine_node_flow
```

A: [TranslatedDomainStatement — UI packet]
  TechnicalStatement: "Five-step onboarding wizard with linear progression. Each step has a step screen + optional sub-screens. Steps: (1) Profile Setup, (2) Team Config, (3) Feature Discovery, (4) Integrations, (5) Completion. Progress bar shows current/total. Back available from step 2+. Skip available on steps 3-4."

B:
  WorkBranches:
    - BranchId: "B01-l2-007-branch-step1", BranchTag: "WIZARD-STEP-1", Priority: 1, Scope: "Profile setup: name, avatar, role selection"
    - BranchId: "B01-l2-007-branch-step2", BranchTag: "WIZARD-STEP-2", Priority: 2, Scope: "Team config: team name, invite teammates (optional)"
    - BranchId: "B01-l2-007-branch-step3", BranchTag: "WIZARD-STEP-3", Priority: 3, Scope: "Feature discovery: highlight 3 key features with interactive cards"
    - BranchId: "B01-l2-007-branch-step4", BranchTag: "WIZARD-STEP-4", Priority: 4, Scope: "Integrations: connect optional third-party tools (can skip)"
    - BranchId: "B01-l2-007-branch-step5", BranchTag: "WIZARD-STEP-5", Priority: 5, Scope: "Completion: celebration screen, link to dashboard"

  [unit result envelope]
    UnitId: "node-03-scheme-planner"
    BranchCount: 5
    Confidence: 0.92
    Warnings: []

---

## B01-L2-007-N09 — Node 09 FormatterUnit: OnboardingWizard final result

```
source_id: xiigen-b01-l2-007-n09
source_title: XIIGen B01 nine-node-007 — N09 FormatterUnit for OnboardingWizardFlow
node_role: node-09-formatter
coverage_status: covered
pair_kind: nine_node_flow
```

A: [ArbiterVerdicts — approved]

B:
  FinalHumanResult:
    Title: "Onboarding Wizard Screen Map — B01-batch-007"
    SubPhaseId: "1.1"
    ScreenCount: 5
    WizardSteps:
      - Step 1: WZD-001 ProfileSetup (form) — name, avatar, role; no back; next → step 2
      - Step 2: WZD-002 TeamConfig (form) — team name, invite; back to step 1; next → step 3
      - Step 3: WZD-003 FeatureDiscovery (interactive) — 3 feature cards; back; next or skip → step 4
      - Step 4: WZD-004 Integrations (interactive) — connect tools; back; next or skip → step 5
      - Step 5: WZD-005 WizardComplete (confirmation) — celebration; CTA to dashboard
    ProgressIndicator: "5-dot stepper; current step highlighted; completed steps checkmarked"
    SkippableSteps: [3, 4]
    GateBResult: "PASS: 5 steps documented; all transitions specified; skip logic defined"

N09 result: Onboarding wizard screens complete; 5 steps documented.
