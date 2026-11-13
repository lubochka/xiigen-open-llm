# Layer 2 — Nine-Node 1.1 Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: ProbationaryRejected — UI+TEST — OnboardingTestFlow rejected, generic fallback

---

## Method note (023)

This file demonstrates the probationary_rejected path in Layer 2.
A candidate flow was proposed for a UI+TEST predicate combination but rejected at Gate B
because the test (TEST) predicate has no registered sub-phase in B01.
The factory falls back to the generic deterministic flow.

---

### Node 01 — DomainSplitterUnit

A:
  TransitionId: "transition-B01-l2-023-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  UserRequest: "Map all screens for the onboarding UX accessibility audit and test all interactive components."
  PredicateIds: ["UI"]
  Note: "test" is NOT in any predicate lexicon — only UI fires (on "screen", "components", "interactive").
  TEST predicate does not exist in B01. Only UI fires here.

B:
  DomainPackets:
    - PacketId: "B01-l2-023-packet-ui"
      DomainTag: "UI"
      Focus: "Onboarding accessibility audit screen inventory"
      UserSignal: "screens, onboarding, interactive components"
  PacketCount: 1 | Confidence: 0.92

  FlowSelection:
    RequestedFlow: "onboarding-test-flow-1.1-candidate"
    TrustState: "probationary"
    GateBPassed: false
    GateBReason: "candidate_flow_scope_mismatch: TEST predicate not registered; flow coverage incomplete"
    FallbackFlow: "universal-nine-node-development-flow-1.1"
    FallbackUsed: true

### Node 02 — DomainTranslatorUnit

A: DomainPacket — UI (fallback flow executing)
B:
  WorkUnits: ["Map onboarding screens for accessibility", "Inventory interactive components"]
  FallbackFlowNote: "Using universal-nine-node-development-flow-1.1 — candidate rejected"

### Node 03 — SchemePlannerUnit

A: [TranslatedPacket]
B:
  WorkBranches:
    - ONBOARDING-SCREENS: SignupPage, EmailVerificationPage, ProfileSetupPage, FirstRunPage
    - ACCESSIBILITY-COMPONENTS: KeyboardFocusIndicator, ARIALabelSet, ColorContrastTokens

### Node 04 — DomainProcessorUnit

A: WorkBranch — ONBOARDING-SCREENS
B:
  ProcessedScreens:
    - ScreenId: "ONB-001" | Name: "SignupPage" | AccessibilityChecked: true
    - ScreenId: "ONB-002" | Name: "EmailVerificationPage" | AccessibilityChecked: true
    - ScreenId: "ONB-003" | Name: "ProfileSetupPage" | AccessibilityChecked: true
    - ScreenId: "ONB-004" | Name: "FirstRunPage" | AccessibilityChecked: true

### Node 05 — ApplierUnit

A: ProcessedScreens
B:
  AppliedScreens:
    - All 4 screens with ARIA labels, keyboard navigation support, and color contrast tokens applied

### Node 06 — ResultTranslatorUnit

A: [4 onboarding screens processed via fallback flow]
B: ScreenCount: 4 | FallbackUsed: true | Unresolved: 0

### Node 07 — ArbiterSelectorUnit

A: [ResultTranslation — fallback path]
B: ArbiterType: "StandardScreenInventoryArbiter"

### Node 08 — ArbiterUnit

A: ArbiterType: "StandardScreenInventoryArbiter"
B:
  Verdict: "APPROVED_WITH_WARNING"
  GateBCheck: "4 screens documented; fallback flow used; candidate rejected"
  Warning: "FallbackUsed: true — probationary candidate 'onboarding-test-flow-1.1-candidate' rejected"

### Node 09 — FormatterUnit

A: ArbiterVerdict — APPROVED_WITH_WARNING
B:
  FinalHumanResult:
    Title: "Onboarding Accessibility Screen Inventory (Fallback Flow)"
    SubPhaseId: "1.1"
    ScreenCount: 4
    FallbackUsed: true
    RejectedCandidateFlowId: "onboarding-test-flow-1.1-candidate"
    GateBResult: "PASS with warning: 4 screens; fallback to universal flow"
  Confidence: 0.88
  Warnings: ["FallbackUsed: true — candidate rejected at Gate B"]
