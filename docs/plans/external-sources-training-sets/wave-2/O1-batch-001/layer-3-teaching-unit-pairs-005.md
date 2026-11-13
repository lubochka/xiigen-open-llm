# Layer 3 — Teaching Unit Pairs (005)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-005-001 — PairParser ingests layer-1-transition-orchestrator-pairs files 002-006

A: BatchFileDescriptor(
     files: ["layer-1-transition-orchestrator-pairs-002.md",
             "layer-1-transition-orchestrator-pairs-003.md",
             "layer-1-transition-orchestrator-pairs-004.md",
             "layer-1-transition-orchestrator-pairs-005.md",
             "layer-1-transition-orchestrator-pairs-006.md"],
     layer: "1",
     pairKind: "transition_orchestrator"
   )
B: ParsedPairCollection(
     totalFiles: 5,
     totalPairs: 24,
     executionPatterns: {
       "single-branch": 8,
       "parallel-fan-out": 7,
       "sequential": 5,
       "mixed-parallel-then-sequential": 4
     },
     knowledgeEdgePairs: 1,
     parseErrors: 0
   )

---

## TU-005-002 — PairParser validates execution mode consistency

A: ParsedPairCollection (from TU-005-001)
B: ExecutionModeValidation(
     parallelRuleConsistent: true,
     sequentialRuleConsistent: true,
     testAlwaysAfterImplement: true,
     qaPositionValid: "QA not present in files 002-006 — covered in files 010",
     gateB: "passed — execution modes consistent across 24 pairs"
   )

---

## TU-005-003 — PairParser validates TransitionType assignments

A: ParsedPairCollection (from TU-005-001)
B: TransitionTypeValidation(
     implementFiresFeatureAddition: true,
     testDocumentWithoutImplementFiresDocumentation: true,
     noPredicate_FiresNone: true,
     gateB: "passed — all TransitionType assignments consistent with priority rules"
   )

---

## TU-005-004 — ModelTrainer receives parsed collection for layer-1 files 002-006

A: ParsedPairCollection (from TU-005-001, validated in TU-005-002 and TU-005-003)
B: TrainingMetrics(
     pairsSubmitted: 24,
     pairsAccepted: 24,
     pairsRejected: 0,
     dominantPattern: "parallel fan-out for independent branches, sequential for test-after-implement",
     coverageGap: "QA-last rule covered in files 010; v6-02 orchestration covered in files 011",
     trainingConfidence: 0.91,
     gateB: "passed — 0 rejected pairs; confidence 0.91 >= 0.85"
   )
