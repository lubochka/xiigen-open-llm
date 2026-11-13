# Layer 3 — Teaching Unit Pairs (002)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: UI lexicon — complete list and when each term fires

---

## B01-L3-002-P01 — PairParserUnit: UI lexicon teaching

```
source_id: xiigen-b01-l3-002-p01
source_title: XIIGen B01 teaching-002 — PairParser for UI lexicon pairs
source_license_state: first_party
contract_signature: PairParserUnit : ILogicalUnit<BatchFileDescriptor, ParsedPairCollection>
transition_type: feature_addition
phase: whole_development_area
sub_phase: whole_development_area
task_type: learning_pipeline_ingestion
node_role: pair-parser
coverage_status: covered
pair_kind: teaching_unit
```

A:
  BatchId: "B01-batch-001"
  TeachingFocusId: "ui-lexicon-complete"
  TargetLayer: "0-A"
  TargetFiles:
    - "layer-0a-compiler-pairs.md"
    - "layer-0a-compiler-pairs-002.md"
    - "layer-0a-compiler-pairs-003.md"
    - "layer-0a-compiler-pairs-004.md"
    - "layer-0c-knowledge-edge-pairs-002.md"
    - "layer-0c-knowledge-edge-pairs-011.md"

B:
  TeachingFocusId: "ui-lexicon-complete"
  ParseStatus: "ready"

  UILexiconFull:
    TermCount: 22
    Terms:
      - Term: "ui", FiresAlways: true, Notes: "direct abbreviation; always fires"
      - Term: "interface", FiresAlways: true, Notes: "admin interface, user interface"
      - Term: "ux", FiresAlways: true, Notes: "direct abbreviation; always fires"
      - Term: "screen", FiresAlways: true, Notes: "primary B01 trigger; screen flow, screen inventory"
      - Term: "page", FiresAlways: true, Notes: "page layout, page design; very common"
      - Term: "modal", FiresAlways: true, Notes: "modal dialog, modal component"
      - Term: "form", FiresAlways: true, Notes: "signup form, form component, form design"
      - Term: "layout", FiresAlways: true, Notes: "page layout, responsive layout"
      - Term: "component", FiresAlways: true, Notes: "UI component, design component"
      - Term: "responsive", FiresAlways: true, Notes: "responsive design, responsive behavior"
      - Term: "accessibility", FiresAlways: true, Notes: "accessibility audit, WCAG compliance"
      - Term: "visual", FiresAlways: true, Notes: "visual design, visual states"
      - Term: "wireframe", FiresAlways: true, Notes: "wireframe, wireframing — always fires in B01"
      - Term: "prototype", FiresAlways: true, Notes: "interactive prototype — fires in B01 (not a gap term here)"
      - Term: "interaction", FiresAlways: true, Notes: "interaction pattern, user interaction"
      - Term: "button", FiresAlways: true, Notes: "button component, button states"
      - Term: "input", FiresAlways: true, Notes: "input field, text input"
      - Term: "typography", FiresAlways: true, Notes: "typography scale, font system"
      - Term: "color", FiresAlways: true, Notes: "color palette, color system"
      - Term: "spacing", FiresAlways: true, Notes: "spacing system, layout spacing"
      - Term: "design-system", FiresAlways: true, Notes: "design system, component library system"
      - Term: "theme", FiresAlways: true, Notes: "UI theme, visual theme"

  TermsAdjacentButNotInLexicon:
    - Term: "navigation", Status: "NOT_IN_LEXICON", Note: "navigation alone does not fire UI; requires co-present UI term"
    - Term: "menu", Status: "NOT_IN_LEXICON", Note: "menu structure alone does not fire UI"
    - Term: "user", Status: "NOT_IN_LEXICON", Note: "user story, user flow — does not fire UI"
    - Term: "flow", Status: "NOT_IN_LEXICON", Note: "screen flow fires via 'screen'; 'flow' alone does not fire"
    - Term: "onboarding", Status: "NOT_IN_LEXICON", Note: "context word; fires no predicate on its own"
    - Term: "dashboard", Status: "NOT_IN_LEXICON", Note: "specific screen name; does not fire UI alone"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-002"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-002-P02 — ModelTrainerUnit: UI lexicon teaching

```
source_id: xiigen-b01-l3-002-p02
source_title: XIIGen B01 teaching-002 — ModelTrainer for UI lexicon
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — UI lexicon focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "ui-lexicon-complete"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "6"
    pairs_accepted: "6"
    pairs_rejected: "0"
    predicate_coverage: "UI"
    lexicon_terms_taught: "22"
    adjacent_non_firing_terms: "6"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "all 22 UI lexicon terms present in training pairs"
      Result: "pass"
    - Assertion: "adjacent non-firing terms documented >= 4"
      Result: "pass (6 documented)"
    - Assertion: "each UI term appears in at least one A/B pair"
      Result: "pass"
    - Assertion: "chain_integrity == passed"
      Result: "pass"

  LearningTargets:
    - "Model correctly maps all 22 terms to UI predicate"
    - "Model does not fire UI for 'navigation' alone"
    - "Model does not fire UI for 'onboarding' alone"
    - "Model fires UI for 'prototype' in B01 context"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-002"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
