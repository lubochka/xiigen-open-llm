# Layer 3 — Teaching Unit Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: Phase 1 sub-phase ownership and routing signals

---

## B01-L3-005-P01 — PairParserUnit: Phase 1 sub-phase routing

```
source_id: xiigen-b01-l3-005-p01
source_title: XIIGen B01 teaching-005 — PairParser for phase 1 sub-phase routing
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
  TeachingFocusId: "phase1-subphase-ownership"
  TargetFiles:
    - "layer-0b-router-pairs.md"
    - "layer-0b-router-pairs-002.md"
    - "layer-0b-router-pairs-003.md"
    - "layer-0c-knowledge-edge-pairs-005.md"
    - "layer-1-transition-orchestrator-pairs.md"

B:
  TeachingFocusId: "phase1-subphase-ownership"
  ParseStatus: "ready"

  Phase1SubPhaseMap:
    Phase: "1"
    SubPhases:
      - SubPhaseId: "1.1"
        Owner: "UI"
        FlowId: "ScreenInventoryFlow11"
        RoutingSignal: "Any UI lexicon term present"
        Description: "Screen inventory and UX definition"

      - SubPhaseId: "1.2"
        Owner: "UI (interaction modeling)"
        RoutingSignal: "UI + interaction complexity indicator"
        Description: "Interaction design and state modeling"

      - SubPhaseId: "1.3"
        Owner: "UI (responsive/breakpoints)"
        RoutingSignal: "UI + responsive / breakpoint signal"
        Description: "Responsive design specification"

      - SubPhaseId: "1.4"
        Owner: "UI (animation/transition)"
        RoutingSignal: "UI + animation / motion indicator"
        Description: "Animation and transition design"

      - SubPhaseId: "1.5"
        Owner: "UI (design tokens)"
        RoutingSignal: "UI + design-system / token / variable signal"
        Description: "Design system token specification"

    OtherPhasesForReference:
      - SubPhaseId: "2.1"
        Owner: "DOCUMENT"
        FlowId: "BusinessRequirementsFlow21"
        RoutingSignal: "Any DOCUMENT lexicon term present"

      - SubPhaseId: "3.1"
        Owner: "ARCHITECTURE"
        FlowId: "SystemArchitectureFlow31"
        RoutingSignal: "ARCHITECTURE predicate fired"

      - SubPhaseId: "4.1"
        Owner: "IMPLEMENT"
        FlowId: "ImplementationFlow41"
        RoutingSignal: "IMPLEMENT predicate fired"

    SubPhaseSelectionRule:
      PrimaryHierarchy: "UI > ARCHITECTURE > IMPLEMENT > DOCUMENT"
      Rule: "SubPhaseId in SelectedLogicalTransition is the highest-priority predicate's sub-phase"
      Example: "UI+DOCUMENT present → SubPhaseId = 1.1 (UI wins)"
      Example2: "DOCUMENT alone → SubPhaseId = 2.1"

    B01DefaultSubPhase:
      Note: "In B01, most requests include UI — default sub-phase is 1.1"
      Flow: "ScreenInventoryFlow11 is the primary flow for all B01 sub-phase 1.1 work"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-005"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-005-P02 — ModelTrainerUnit: Phase 1 sub-phase routing

```
source_id: xiigen-b01-l3-005-p02
source_title: XIIGen B01 teaching-005 — ModelTrainer for phase 1 sub-phase routing
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — phase 1 sub-phase focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "phase1-subphase-ownership"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "5"
    pairs_accepted: "5"
    pairs_rejected: "0"
    sub_phases_documented: "5"
    routing_signal_pairs: "5"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "all Phase 1 sub-phases (1.1 through 1.5) documented with owner and routing signal"
      Result: "pass"
    - Assertion: "SubPhaseId selection hierarchy (UI > ARCHITECTURE > IMPLEMENT > DOCUMENT) documented"
      Result: "pass"
    - Assertion: "ScreenInventoryFlow11 linked to sub-phase 1.1"
      Result: "pass"

  LearningTargets:
    - "Model routes UI → 1.1 for all B01 screen inventory requests"
    - "Model applies sub-phase hierarchy when multiple predicates present"
    - "Model identifies ScreenInventoryFlow11 as the correct flow for 1.1"
    - "Model does not route DOCUMENT to 1.1"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-005"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
