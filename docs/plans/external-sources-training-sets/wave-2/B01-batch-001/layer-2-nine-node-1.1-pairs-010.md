# Layer 2 — Nine-Node Flow Pairs — Sub-Phase 1.1 (010)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: AccessibilityAuditScreens (probationary_rejected — fallback flow used)

---

## B01-L2-010-N01 — Node 01 DomainSplitterUnit: AccessibilityAudit

```
source_id: xiigen-b01-l2-010-n01
source_title: XIIGen B01 nine-node-010 — N01 DomainSplitter for AccessibilityAuditScreens
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
  TransitionId: "transition-B01-l2-010-1.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "1"
  SubPhaseId: "1.1"
  DomainId: "software-development"
  UserRequest: "Audit all onboarding screens for accessibility compliance and document the WCAG criteria for each screen."
  PredicateIds: ["UI", "DOCUMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B01-0a-008-001-prompt"]

  FlowContext:
    SelectedFlowId: "ScreenInventoryFlow11"
    TrustState: "deterministic_generic"
    FallbackUsed: true
    RejectedCandidateFlowId: "AccessibilityScreenFlow11-candidate"
    FlowSelectionNote: "AccessibilityScreenFlow11-candidate was rejected (Gate B failed: 4 of 9 nodes covered); fallback to ScreenInventoryFlow11"

B:
  DomainPackets:
    - PacketId: "B01-l2-010-packet-ui"
      DomainTag: "UI"
      Focus: "Accessibility compliance check for each onboarding screen: color contrast, keyboard navigation, ARIA labels, focus management"
      SourceTransitionId: "transition-B01-l2-010-1.1-feature_addition"
      UserSignal: "onboarding screens, accessibility, screen"

    - PacketId: "B01-l2-010-packet-doc"
      DomainTag: "DOCUMENT"
      Focus: "WCAG criteria documentation for each screen: Level AA conformance requirements, failure modes identified"
      SourceTransitionId: "transition-B01-l2-010-1.1-feature_addition"
      UserSignal: "document, WCAG criteria, screen"

  [unit result envelope]
    UnitId: "node-01-domain-splitter"
    SubPhaseId: "1.1"
    PacketCount: 2
    Confidence: 0.92
    Warnings:
      - "fallback_flow_in_use: AccessibilityScreenFlow11-candidate rejected"
      - "monitoring: output quality may differ from specialized accessibility flow"

N01 result: ["UI", "DOCUMENT"], 1.1, feature_addition — AccessibilityAudit started; fallback flow used.

---

## B01-L2-010-N06 — Node 06 ResultTranslatorUnit: Accessibility results

```
source_id: xiigen-b01-l2-010-n06
source_title: XIIGen B01 nine-node-010 — N06 ResultTranslator for AccessibilityAuditScreens
node_role: node-06-result-translator
coverage_status: covered
pair_kind: nine_node_flow
```

A: [All AppliedBranchResults — 3 branches: visual, interaction, document]

B:
  TranslatedResultFragments:
    - FragmentId: "frag-B01-l2-010-accessibility-screen-map"
      FragmentType: "screen_inventory"
      Content:
        ScreensAudited: 12
        AccessibilityFindings:
          - SCR-002 SignupForm: "Missing ARIA label on password strength indicator; color contrast 3.2:1 (fails AA min 4.5:1)"
          - SCR-011 ActivationLanding: "Focus not returned to top after page load; no skip-nav link"
          - SCR-022 RoleSelection: "Interactive cards not keyboard accessible; role=button missing"

    - FragmentId: "frag-B01-l2-010-wcag-criteria"
      FragmentType: "requirements_document"
      Content:
        WcagLevel: "AA"
        CriteriaDocumented: 8
        FailingCriteria: ["1.4.3 Contrast (Minimum)", "2.1.1 Keyboard", "4.1.2 Name Role Value"]
        PassingCriteria: ["1.1.1 Non-text Content", "1.3.1 Info and Relationships", "2.4.3 Focus Order", "2.4.4 Link Purpose", "3.1.1 Language of Page"]

  [unit result envelope]
    UnitId: "node-06-result-translator"
    FragmentCount: 2
    Confidence: 0.88
    Warnings: ["fallback_flow_used: specialized accessibility analysis may be less precise"]

---

## B01-L2-010-N09 — Node 09 FormatterUnit: AccessibilityAudit final result

```
source_id: xiigen-b01-l2-010-n09
source_title: XIIGen B01 nine-node-010 — N09 FormatterUnit for AccessibilityAuditScreens
node_role: node-09-formatter
coverage_status: covered
pair_kind: nine_node_flow
```

A: [ArbiterVerdicts — approved with caveats]

B:
  FinalHumanResult:
    Title: "Accessibility Audit Screen Inventory + WCAG Criteria — B01-batch-010"
    SubPhaseId: "1.1"
    ScreensAudited: 12
    WcagLevel: "AA"
    FailingIssues: 3
    PassingCriteria: 5
    DocumentedScreens: 12
    FlowNote: "Produced using fallback ScreenInventoryFlow11 (AccessibilityScreenFlow11-candidate was rejected — Gate B: 4/9 nodes); specialized accessibility flow needed for full analysis"
    GateBResult: "PASS (with caveat): 12 screens audited; WCAG criteria documented; fallback flow produced valid but non-specialized output"

N09 result: AccessibilityAudit complete; fallback flow used; 12 screens, 3 issues documented.
