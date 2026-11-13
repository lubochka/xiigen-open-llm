# Layer 3 — Teaching Unit Pairs (012)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: Complete B01 teaching summary

---

## B01-L3-012-P01 — PairParserUnit: Complete B01 teaching summary

```
source_id: xiigen-b01-l3-012-p01
source_title: XIIGen B01 teaching-012 — PairParser for complete B01 summary
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
  TeachingFocusId: "b01-complete-teaching-summary"
  TargetFiles: ["all 101 B01-batch-001 files"]

B:
  TeachingFocusId: "b01-complete-teaching-summary"
  ParseStatus: "ready"

  B01TeachingSummary:
    ScenarioId: "B01-saas-onboarding-ux"
    ScenarioDescription: "SaaS onboarding screen flow mapping and screen inventory"

    KeyLexiconRules:
      UI_terms: 22
      DOCUMENT_terms: 15
      Critical_non_UI: ["navigation", "menu", "user", "flow", "onboarding", "dashboard"]
      Critical_B01_specific: "'prototype' IS in UI lexicon (not a gap term in B01)"

    RoutingRules:
      UI_routes_to: "1.1 via ScreenInventoryFlow11"
      DOCUMENT_routes_to: "2.1 via BusinessRequirementsFlow21"
      ARCHITECTURE_routes_to: "3.1 via SystemArchitectureFlow31"
      IMPLEMENT_routes_to: "4.1 via ImplementationFlow41"
      Sub_phase_hierarchy: "UI > ARCHITECTURE > IMPLEMENT > DOCUMENT"

    TransitionTypeRules:
      feature_addition: "IMPLEMENT present, OR UI alone"
      documentation: "DOCUMENT or ARCHITECTURE without IMPLEMENT"
      knowledge_edge: "no predicate detected"

    VerbRules:
      write: "always fires IMPLEMENT"
      document: "fires DOCUMENT only"
      define: "fires DOCUMENT only"
      describe: "fires DOCUMENT only"
      implement: "fires IMPLEMENT"
      build: "fires IMPLEMENT"

    V6CorruptionTypes:
      v6_01: "UI predicate missed — detection: UI term in request but not in PredicateIds"
      v6_02: "DOCUMENT suppressed — detection: DOCUMENT term in request but not in PredicateIds"
      v6_03: "UI routed to wrong sub-phase — detection: UI in PredicateIds but SubPhaseId ≠ 1.1"

    FourRegisteredFlows:
      - FlowId: "ScreenInventoryFlow11", SubPhase: "1.1", Nodes: 9, Primary: "UI"
      - FlowId: "BusinessRequirementsFlow21", SubPhase: "2.1", Nodes: 9, Primary: "DOCUMENT"
      - FlowId: "SystemArchitectureFlow31", SubPhase: "3.1", Nodes: 9, Primary: "ARCHITECTURE"
      - FlowId: "ImplementationFlow41", SubPhase: "4.1", Nodes: 9, Primary: "IMPLEMENT"

    EM9Guard:
      Input_rule: "Node 01 input MUST be SelectedLogicalTransition, never a raw string"
      Prohibited_RoutingDecision_fields: ["dependency_order", "parallel_group", "domain_label", "is_primary"]

    FallbackSemantics:
      FallbackUsed_true: "A registered candidate flow was proposed but rejected (Gate B failed)"
      FallbackUsed_false_or_absent: "No candidate; using default deterministic flow"

    UXScenariosInBatch:
      Count: 12
      Scenarios: [
        "OnboardingScreenFlow (base)",
        "AdminPanelScreenInventory",
        "CheckoutInterfaceFlow",
        "SignupFormDesign",
        "MobileResponsiveScreens",
        "PasswordResetFlow",
        "OnboardingWizardFlow",
        "ProductListingPages",
        "ErrorAndEmptyStates",
        "AccessibilityAuditScreens",
        "DesignSystemComponents",
        "FullUxDocumentation"
      ]

    BatchMetrics:
      TotalFiles: 101
      LayerBreakdown:
        layer_0a: 12
        layer_0b: 12
        layer_0c: 12
        layer_0d: 12
        layer_1: 12
        layer_2: 12
        layer_3: 12
        held_out_corruption: 12
        cross_layer_synthesis: 4
        manifest: 1

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-012"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-012-P02 — ModelTrainerUnit: Complete B01 summary training

```
source_id: xiigen-b01-l3-012-p02
source_title: XIIGen B01 teaching-012 — ModelTrainer for complete B01 summary
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — complete B01 summary]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "b01-complete-teaching-summary"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "101"
    pairs_accepted: "101"
    pairs_rejected: "0"
    predicate_coverage: "4"
    sub_phase_coverage: "4"
    ux_scenarios_covered: "12"
    knowledge_edge_pairs: "12"
    v6_corruption_types: "3"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "total_pairs_ingested >= 100"
      Result: "pass (101 >= 100)"
    - Assertion: "pairs_rejected == 0"
      Result: "pass"
    - Assertion: "all 4 predicates covered (UI, DOCUMENT, ARCHITECTURE, IMPLEMENT)"
      Result: "pass"
    - Assertion: "all 4 flows documented (ScreenInventoryFlow11, BusinessRequirementsFlow21, SystemArchitectureFlow31, ImplementationFlow41)"
      Result: "pass"
    - Assertion: "12 UX scenarios covered in Layer 2"
      Result: "pass"
    - Assertion: "all 3 v6 corruption types documented"
      Result: "pass"
    - Assertion: "min_confidence >= 0.70"
      Result: "pass (0.88 minimum — AccessibilityAuditScreens fallback)"
    - Assertion: "chain_integrity == passed"
      Result: "pass"

  WaveAcceptanceVerdict: "ACCEPTED"
  NextAction: "PROMOTE_TO_WAVE_2_REGISTER"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-012"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
