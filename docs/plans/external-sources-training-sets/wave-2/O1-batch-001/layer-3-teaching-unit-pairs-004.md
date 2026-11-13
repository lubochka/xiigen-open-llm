# Layer 3 — Teaching Unit Pairs (004)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-004-001 — PairParser ingests layer-0d-flow-factory-pairs files 002-006

A: BatchFileDescriptor(
     files: ["layer-0d-flow-factory-pairs-002.md", "layer-0d-flow-factory-pairs-003.md",
             "layer-0d-flow-factory-pairs-004.md", "layer-0d-flow-factory-pairs-005.md",
             "layer-0d-flow-factory-pairs-006.md"],
     layer: "0-D",
     pairKind: "flow_factory"
   )
B: ParsedPairCollection(
     totalFiles: 5,
     totalPairs: 22,
     trustStatesObserved: {
       "trusted_learned": 12,
       "probationary_accepted": 4,
       "probationary_rejected": 4,
       "deterministic_generic": 8
     },
     factoryNamesObserved: ["SecurityVerification34FlowAdapter", "ApiContract33FlowAdapter",
                            "UnitTestDesign51FlowAdapter", "RequirementsAuthoringFlow21",
                            "ImplementationFlow41"],
     parseErrors: 0
   )

---

## TU-004-002 — PairParser validates factory name registry

A: ParsedPairCollection (from TU-004-001)
B: FactoryNameValidation(
     allNamesInRegistry: true,
     inventedNamesFound: 0,
     registryNames: ["SecurityVerification34FlowAdapter", "ApiContract33FlowAdapter",
                     "UnitTestDesign51FlowAdapter", "RequirementsAuthoringFlow21",
                     "ImplementationFlow41", "QAGatingFlow73", "ArchitectureFlow"],
     gateB: "passed — 0 invented factory names in files 002-006"
   )

---

## TU-004-003 — PairParser validates trust state transitions

A: ParsedPairCollection (from TU-004-001)
B: TrustStateValidation(
     allTrustStateTransitionsValid: true,
     probationaryRejectedHasFallback: true,
     trustedLearnedHasNoFallback: true,
     gateB: "passed — all trust state assignments consistent with factory map"
   )

---

## TU-004-004 — ModelTrainer receives parsed collection for 0-D files 002-006

A: ParsedPairCollection (from TU-004-001, validated in TU-004-002 and TU-004-003)
B: TrainingMetrics(
     pairsSubmitted: 22,
     pairsAccepted: 22,
     pairsRejected: 0,
     coverageGap: "v6-01 corruption pairs not in files 002-006 — covered in files 010-012",
     trainingConfidence: 0.91,
     gateB: "passed — 0 rejected pairs; confidence 0.91 >= 0.85"
   )
