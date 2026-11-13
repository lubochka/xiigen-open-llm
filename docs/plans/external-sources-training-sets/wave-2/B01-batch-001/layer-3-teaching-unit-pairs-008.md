# Layer 3 — Teaching Unit Pairs (008)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: "design" fires ARCHITECTURE in B01; when ARCHITECTURE routes separately from UI

---

## B01-L3-008-P01 — PairParserUnit: "design" ARCHITECTURE firing rule

```
source_id: xiigen-b01-l3-008-p01
source_title: XIIGen B01 teaching-008 — PairParser for design+ARCHITECTURE rule
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
  TeachingFocusId: "design-fires-architecture"
  TargetFiles:
    - "layer-0a-compiler-pairs-007.md"
    - "layer-0b-router-pairs-007.md"
    - "layer-0c-knowledge-edge-pairs-006.md"
    - "layer-1-transition-orchestrator-pairs-007.md"
    - "layer-0d-flow-factory-pairs-011.md"

B:
  TeachingFocusId: "design-fires-architecture"
  ParseStatus: "ready"

  ArchitectureFireRule:
    PrimaryVerb: "design"
    FiresARCHITECTURE_when: "design + structural noun (architecture, system, routing, state management, graph, structure)"
    FiresUI_only_when: "design + UI noun (screen, layout, form, component, visual)"
    NeitherWhen: "design + context noun only (design the onboarding)"

    Examples:
      - Request: "Design the architecture for the onboarding screen flow."
        Analysis: "'design' + 'architecture' fires ARCHITECTURE; 'screen' fires UI"
        Result: "PredicateIds: [UI, ARCHITECTURE]"
        Note: "Both fire independently"

      - Request: "Design the screen layout for the onboarding wizard."
        Analysis: "'design' + 'screen' + 'layout' — all UI nouns; no structural noun present"
        Result: "PredicateIds: [UI]"
        Note: "No ARCHITECTURE — 'design' alone without structural noun does not fire ARCHITECTURE"

      - Request: "Design the routing system and state management for the screen flow."
        Analysis: "'design' + 'routing system' + 'state management' — both structural nouns"
        Result: "PredicateIds: [ARCHITECTURE]"
        Note: "No UI lexicon term present — ARCHITECTURE alone"

      - Request: "Design the screen routing system."
        Analysis: "'screen' fires UI; 'routing system' fires ARCHITECTURE"
        Result: "PredicateIds: [UI, ARCHITECTURE]"
        Note: "Dual firing from a single request"

    ARCHITECTURESubPhase:
      SubPhaseId: "3.1"
      FlowId: "SystemArchitectureFlow31"
      RoutingDecision: "separate routing decision, SubPhaseId 3.1"

    TransitionTypeRule:
      ARCHITECTURE_plus_UI_without_IMPLEMENT: "documentation"
      ARCHITECTURE_plus_UI_plus_IMPLEMENT: "feature_addition"
      Explanation: "IMPLEMENT is the feature_addition gate; design/architecture without implement = documentation"

    EM9GuardNote:
      Prohibited_in_RoutingDecision: ["dependency_order", "parallel_group", "domain_label", "is_primary"]
      Reason: "EM-9 guard prohibits these fields; RoutingDecision contains only SubPhaseId, PredicateId, FlowId, TrustState"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-008"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-008-P02 — ModelTrainerUnit: "design" ARCHITECTURE rule

```
source_id: xiigen-b01-l3-008-p02
source_title: XIIGen B01 teaching-008 — ModelTrainer for design+ARCHITECTURE rule
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — design ARCHITECTURE focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "design-fires-architecture"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "5"
    pairs_accepted: "5"
    pairs_rejected: "0"
    architecture_firing_scenarios: "4"
    em9_guard_documented: "1"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "'design' + structural noun fires ARCHITECTURE in all 4 examples"
      Result: "pass"
    - Assertion: "'design' + UI noun alone does NOT fire ARCHITECTURE"
      Result: "pass"
    - Assertion: "SystemArchitectureFlow31 linked to sub-phase 3.1"
      Result: "pass"
    - Assertion: "EM-9 prohibited fields documented"
      Result: "pass"

  LearningTargets:
    - "Model fires ARCHITECTURE for 'design' + structural noun"
    - "Model does not fire ARCHITECTURE for 'design' + UI noun alone"
    - "Model routes ARCHITECTURE to sub-phase 3.1 via SystemArchitectureFlow31"
    - "Model sets TransitionType to documentation for ARCHITECTURE+UI without IMPLEMENT"
    - "Model excludes prohibited EM-9 fields from RoutingDecision"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-008"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
