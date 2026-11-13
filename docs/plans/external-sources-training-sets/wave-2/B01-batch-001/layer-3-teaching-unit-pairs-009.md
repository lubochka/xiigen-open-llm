# Layer 3 — Teaching Unit Pairs (009)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: v6 corruption patterns for B01

---

## B01-L3-009-P01 — PairParserUnit: v6 corruption patterns

```
source_id: xiigen-b01-l3-009-p01
source_title: XIIGen B01 teaching-009 — PairParser for v6 corruption patterns
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
  TeachingFocusId: "v6-corruption-patterns-b01"
  TargetFiles:
    - "layer-0a-compiler-pairs-011.md"
    - "layer-0b-router-pairs-011.md"
    - "layer-0c-knowledge-edge-pairs-009.md"
    - "held-out-and-corruption-pairs.md"

B:
  TeachingFocusId: "v6-corruption-patterns-b01"
  ParseStatus: "ready"

  V6CorruptionCatalog:
    Version: "v6"
    CorruptionTypes:

      - CorruptionId: "v6-01"
        Name: "UI predicate missed"
        Description: "UI lexicon term present in request but predicate not detected"
        Trigger: "Model fails to recognize a valid UI lexicon term"
        CommonCauses:
          - "Term at sentence boundary misidentified"
          - "Term treated as non-predicate context word"
          - "Compound noun splitting error ('design-system' → ['design', 'system'] without recognizing hyphenated form)"
        B01Example:
          CorruptInput: "Map the navigation and page structure — PredicateIds: []"
          CorrectOutput: "PredicateIds: [UI] — 'page' fires UI"
          Detection: "UI term present in request but absent from PredicateIds"

      - CorruptionId: "v6-02"
        Name: "DOCUMENT predicate suppressed"
        Description: "DOCUMENT lexicon term present but predicate suppressed — usually because UI was found first"
        Trigger: "Model suppresses DOCUMENT when UI already firing"
        CommonCauses:
          - "Model assumes UI→1.1 is the 'complete' answer and stops scanning"
          - "Model treats DOCUMENT as subordinate to UI rather than independent"
        B01Example:
          CorruptInput: "'Map screens and document requirements' → PredicateIds: [UI]"
          CorrectOutput: "PredicateIds: [UI, DOCUMENT] — independence rule"
          Detection: "DOCUMENT term present in request but absent from PredicateIds"

      - CorruptionId: "v6-03"
        Name: "UI routes to wrong sub-phase"
        Description: "UI predicate correctly detected but routed to 2.1 instead of 1.1"
        Trigger: "Model confuses predicate → sub-phase mapping"
        CommonCauses:
          - "SubPhaseId lookup table corrupted"
          - "Model copies DOCUMENT sub-phase (2.1) for UI by mistake"
        B01Example:
          CorruptInput: "UI→SubPhaseId:2.1"
          CorrectOutput: "UI→SubPhaseId:1.1"
          Detection: "UI in PredicateIds but SubPhaseId ≠ 1.1 in RoutingDecision"

    DetectionChecklist:
      - "Step 1: For each UI term in request, verify UI is in PredicateIds (catches v6-01)"
      - "Step 2: For each DOCUMENT term in request, verify DOCUMENT is in PredicateIds (catches v6-02)"
      - "Step 3: Verify UI maps to SubPhaseId 1.1, not 2.1 (catches v6-03)"
      - "Step 4: Verify RoutingDecision count equals PredicateIds count"

    RemediationPriority:
      Order: "v6-01 → v6-02 → v6-03"
      Reason: "v6-01 cascades: missed UI causes missed 1.1 routing and missing ScreenInventoryFlow11"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-009"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-009-P02 — ModelTrainerUnit: v6 corruption patterns

```
source_id: xiigen-b01-l3-009-p02
source_title: XIIGen B01 teaching-009 — ModelTrainer for v6 corruption
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — v6 corruption focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "v6-corruption-patterns-b01"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "4"
    pairs_accepted: "4"
    pairs_rejected: "0"
    corruption_types_documented: "3"
    detection_steps: "4"
    remediation_priorities: "3"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "all 3 v6 corruption types documented with B01 examples"
      Result: "pass"
    - Assertion: "detection checklist covers all 3 types"
      Result: "pass"
    - Assertion: "remediation priority order established (v6-01 first)"
      Result: "pass"

  LearningTargets:
    - "Model recognizes v6-01 (UI missed) and provides correct PredicateIds"
    - "Model recognizes v6-02 (DOCUMENT suppressed) and restores independence"
    - "Model recognizes v6-03 (wrong sub-phase) and routes UI to 1.1"
    - "Model applies 4-step detection checklist in order"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-009"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
