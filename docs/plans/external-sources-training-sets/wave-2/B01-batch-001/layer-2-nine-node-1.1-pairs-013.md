# Layer 2 — Nine-Node 1.1 Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: nine_node_flow
# UX Scenario: UserProfilePage — UI alone — trusted_learned

---

### Node 01 — DomainSplitterUnit

```
source_id: xiigen-b01-l2-013-n01
node_role: node-01-domain-splitter
pair_kind: nine_node_flow
```

A:
  TransitionId: "transition-B01-l2-013-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  UserRequest: "List all the UI components on the user profile page — avatar upload, display name input, and bio form."
  PredicateIds: ["UI"]

B:
  DomainPackets:
    - PacketId: "B01-l2-013-packet-ui"
      DomainTag: "UI"
      Focus: "User profile page component inventory — avatar, input fields, form sections, interactive states"
      UserSignal: "UI components, user profile page, avatar upload, display name input, bio form"
  PacketCount: 1 | Confidence: 0.92

### Node 02 — DomainTranslatorUnit

A: DomainPacket — UI — UserProfilePage
B:
  TranslatedPacketId: "B01-l2-013-translated-ui"
  Domain: "UI"
  WorkUnits:
    - "Identify all component types on the profile page"
    - "Map interactive states per component (default, hover, focus, error)"
    - "Record layout region for each component"

### Node 03 — SchemePlannerUnit

A: [TranslatedPackets from Node 02]
B:
  WorkBranches:
    - AVATAR-SECTION: AvatarUpload, AvatarPreview, AvatarRemoveButton
    - NAME-SECTION: DisplayNameInput, DisplayNameLabel, DisplayNameEditButton
    - BIO-SECTION: BioTextArea, BioCharCount, BioSaveButton, BioCancelButton
    - METADATA: ProfileCompleteness, AccountLinkBadge

### Node 04 — DomainProcessorUnit

A: WorkBranch — AVATAR-SECTION
B:
  ProcessedItems:
    - ComponentId: "AvatarUpload", Type: "input[file]", States: ["idle", "uploading", "error", "success"]
    - ComponentId: "AvatarPreview", Type: "img", States: ["default", "loading"]
    - ComponentId: "AvatarRemoveButton", Type: "button", States: ["visible", "hidden"]

### Node 05 — ApplierUnit

A: ProcessedItems — AVATAR-SECTION
B:
  AppliedItems:
    - ComponentId: "AvatarUpload" | AccessibilityLabel: "Upload profile photo" | Color: "brand-primary" | Spacing: "space-4"
    - ComponentId: "AvatarPreview" | Size: "80px × 80px" | BorderRadius: "50%"
    - ComponentId: "AvatarRemoveButton" | Color: "error-500" | Typography: "label-sm"

### Node 06 — ResultTranslatorUnit

A: [AllAppliedItems from Nodes 04–05 for all branches]
B:
  ComponentCount: 11
  AllComponentsDocumented: true
  UnresolvedComponents: 0

### Node 07 — ArbiterSelectorUnit

A: [ResultTranslation — 11 components, 4 sections]
B:
  ArbiterType: "StandardScreenInventoryArbiter"
  Threshold: "all components have type, states, accessibility label, and spacing"

### Node 08 — ArbiterUnit

A: ArbiterType: "StandardScreenInventoryArbiter"
B:
  Verdict: "APPROVED"
  GateBCheck: "11 components documented; all 4 sections covered; no unresolved components"

### Node 09 — FormatterUnit

A: ArbiterVerdict — APPROVED
B:
  FinalHumanResult:
    Title: "UserProfilePage Component Inventory"
    SubPhaseId: "1.1"
    ComponentCount: 11
    Sections:
      - SectionId: "AVATAR-SECTION" | Components: 3
      - SectionId: "NAME-SECTION" | Components: 3
      - SectionId: "BIO-SECTION" | Components: 4
      - SectionId: "METADATA" | Components: 2 (informal; not interactive)
    GateBResult: "PASS: 11 components; all 4 sections; all interactive states documented"
  Confidence: 0.92 | SelectionOutcome: "FORMATTED_COMPLETE"
