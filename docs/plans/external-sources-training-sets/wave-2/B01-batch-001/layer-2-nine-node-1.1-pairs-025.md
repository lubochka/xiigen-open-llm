# Layer 2 — Nine-Node 1.1 Pairs (025)
# Batch: B01-batch-001 | Scenario: B01 — HeroSection component flow
# Production date: 2026-05-06 | pair_kind: nine_node_flow

---

## B01-L2-025-001 — HeroSection: Node 01 DomainSplitterUnit

```
source_id: xiigen-b01-l2-025-001
pair_kind: nine_node_flow
```

A (SelectedLogicalTransition):
  TransitionId: "transition-B01-l2-025-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "IMPLEMENT"]

B (DomainPackets):
  Packet[UI]: "Screen inventory: all hero section states and layout variants"
  Packet[IMPLEMENT]: "Implementation: hero section component code"

---

## B01-L2-025-002 — HeroSection: Node 09 FinalHumanResult

```
source_id: xiigen-b01-l2-025-002
pair_kind: nine_node_flow
```

A (ArbiterVerdict from Node 08):
  ArbiterType: "SCREEN_INVENTORY_ARBITER"
  Verdict: "APPROVED"

B (FinalHumanResult):
  FinalHumanResult:
    Title: "HeroSection Component Screen Inventory"
    SubPhaseId: "1.1"
    ScreenCount: 4
    Screens:
      - ScreenId: "hero-default"
        Name: "Default Hero State"
        EntryConditions: ["page load complete", "user not authenticated"]
        ExitTransitions: ["CTA click → ConversionFlow", "scroll → BelowFoldContent"]
      - ScreenId: "hero-authenticated"
        Name: "Authenticated Hero State"
        EntryConditions: ["user authenticated", "session valid"]
        ExitTransitions: ["CTA click → DashboardFlow", "scroll → FeatureSection"]
      - ScreenId: "hero-loading"
        Name: "Hero Loading State"
        EntryConditions: ["page loading", "content not yet fetched"]
        ExitTransitions: ["load complete → DefaultHero or AuthenticatedHero"]
      - ScreenId: "hero-mobile"
        Name: "Hero Mobile Breakpoint"
        EntryConditions: ["viewport width < 768px"]
        ExitTransitions: ["same as default, adapted for touch"]
    GateBResult: "PASS: 4 screens; entry/exit conditions met for all; no unresolved screens"
  Confidence: 0.91
  SelectionOutcome: "FORMATTED_COMPLETE"

---

## B01-L2-025-003 — HeroSection: EM-9 Node 01 guard rule

```
source_id: xiigen-b01-l2-025-003
pair_kind: nine_node_flow
```

Correct Node 01 input:
  TransitionId: "transition-B01-l2-025-1.1-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseId: "1.1"
  PredicateIds: ["UI", "IMPLEMENT"]

Corrupt Node 01 input (EM-9 violation):
  "Build a hero section with headline and CTA button."  [raw string — WRONG]

Rule: Node 01 A side is ALWAYS SelectedLogicalTransition with all four required fields.
