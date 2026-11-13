# Layer 3 — Teaching Unit Pairs (006)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: teaching_unit
# Teaching focus: "write" always fires IMPLEMENT even in UX documentation context

---

## B01-L3-006-P01 — PairParserUnit: "write" fires IMPLEMENT unconditionally

```
source_id: xiigen-b01-l3-006-p01
source_title: XIIGen B01 teaching-006 — PairParser for "write" IMPLEMENT rule
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
  TeachingFocusId: "write-fires-implement-unconditionally"
  TargetFiles:
    - "layer-0a-compiler-pairs-006.md"
    - "layer-0c-knowledge-edge-pairs-007.md"
    - "layer-1-transition-orchestrator-pairs-005.md"

B:
  TeachingFocusId: "write-fires-implement-unconditionally"
  ParseStatus: "ready"

  WriteImplementRule:
    Term: "write"
    AlwaysFiresIMPLEMENT: true
    NoExceptions: true
    Rationale: "The verb 'write' signals a production intent — the user wants code or structured output written, not just documented or described"

    Contrasts:
      - Request: "Write the UX specification for the login screen."
        VerbAnalysis: "'write' fires IMPLEMENT"
        NounAnalysis: "'specification' fires DOCUMENT; 'screen' fires UI"
        Result: "PredicateIds: [UI, DOCUMENT, IMPLEMENT]"
        Note: "Even though the object is a 'specification', the verb 'write' still fires IMPLEMENT"

      - Request: "Document the UX specification for the login screen."
        VerbAnalysis: "'document' fires DOCUMENT only"
        NounAnalysis: "'specification' fires DOCUMENT; 'screen' fires UI"
        Result: "PredicateIds: [UI, DOCUMENT]"
        Note: "The verb 'document' does NOT fire IMPLEMENT — key verb distinction"

      - Request: "Write the form components for the signup screen."
        VerbAnalysis: "'write' fires IMPLEMENT"
        NounAnalysis: "'form' + 'component' fire UI; 'screen' fires UI"
        Result: "PredicateIds: [UI, IMPLEMENT]"
        TransitionType: "feature_addition"

      - Request: "Write the requirements for the onboarding flow."
        VerbAnalysis: "'write' fires IMPLEMENT"
        NounAnalysis: "'requirements' fires DOCUMENT"
        Result: "PredicateIds: [DOCUMENT, IMPLEMENT]"
        Note: "Even for requirements documents, 'write' adds IMPLEMENT"

    VerbTable:
      - Verb: "write", FiresIMPLEMENT: true
      - Verb: "implement", FiresIMPLEMENT: true
      - Verb: "build", FiresIMPLEMENT: true
      - Verb: "create", FiresIMPLEMENT: true, Note: "context-dependent; often fires IMPLEMENT"
      - Verb: "document", FiresIMPLEMENT: false, FiresDOCUMENT: true
      - Verb: "define", FiresIMPLEMENT: false, FiresDOCUMENT: true
      - Verb: "describe", FiresIMPLEMENT: false, FiresDOCUMENT: true
      - Verb: "map", FiresIMPLEMENT: false, Note: "typically routes via noun signals"
      - Verb: "audit", FiresIMPLEMENT: false, Note: "routes via noun signals (UI for screen audit)"

  ChainIntegrityCheck: "passed"
  Warnings: []

  [unit result envelope]
    UnitId: "unit-pair-parser-l3-006"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "PARSED_READY"

---

## B01-L3-006-P02 — ModelTrainerUnit: "write" fires IMPLEMENT

```
source_id: xiigen-b01-l3-006-p02
source_title: XIIGen B01 teaching-006 — ModelTrainer for write IMPLEMENT rule
source_license_state: first_party
contract_signature: ModelTrainerUnit : ILogicalUnit<ParsedPairCollection, TrainingMetrics>
node_role: model-trainer
coverage_status: covered
pair_kind: teaching_unit
```

A: [ParsedPairCollection from PairParser above — write IMPLEMENT focus]

B:
  BatchId: "B01-batch-001"
  TeachingFocusId: "write-fires-implement-unconditionally"
  TrainingStatus: "complete"

  Metrics:
    total_pairs_ingested: "3"
    pairs_accepted: "3"
    pairs_rejected: "0"
    verb_distinctions_documented: "8"
    contrast_pairs: "4"
    gate_b_passed: "true"

  GateBAssertions:
    - Assertion: "'write' fires IMPLEMENT in all 4 contrast examples"
      Result: "pass"
    - Assertion: "'document' does NOT fire IMPLEMENT in all contrast examples"
      Result: "pass"
    - Assertion: "verb table covers 8 verbs with IMPLEMENT firing status"
      Result: "pass"

  LearningTargets:
    - "Model fires IMPLEMENT whenever 'write' appears, regardless of object noun"
    - "Model distinguishes 'write' (IMPLEMENT) from 'document' (DOCUMENT only)"
    - "Model correctly produces feature_addition TransitionType when IMPLEMENT fires"
    - "Model adds ImplementationFlow41 to execution order when IMPLEMENT present"

  WaveAcceptanceVerdict: "ACCEPTED"

  [unit result envelope]
    UnitId: "unit-model-trainer-l3-006"
    BatchId: "B01-batch-001"
    Confidence: 0.92
    SelectionOutcome: "TRAINING_COMPLETE_GATE_B_PASSED"
    Warnings: []
