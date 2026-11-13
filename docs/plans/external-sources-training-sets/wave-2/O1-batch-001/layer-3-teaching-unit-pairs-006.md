# Layer 3 — Teaching Unit Pairs (006)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-006-001 — PairParser ingests layer-2-nine-node-3.4-pairs files 002-006

A: BatchFileDescriptor(
     files: ["layer-2-nine-node-3.4-pairs-002.md",
             "layer-2-nine-node-3.4-pairs-003.md",
             "layer-2-nine-node-3.4-pairs-004.md",
             "layer-2-nine-node-3.4-pairs-005.md",
             "layer-2-nine-node-3.4-pairs-006.md"],
     layer: "2",
     pairKind: "nine_node_flow"
   )
B: ParsedPairCollection(
     totalFiles: 5,
     totalScenarios: 5,
     nodesPerScenario: 9,
     totalNodePairs: 45,
     scenariosFound: ["OAuthTokenValidation", "RbacPermissionModel", "ApiKeySecuritySchema", "TenantIsolationTest", "JwtRefreshTokenFlow"],
     factoryStateObserved: "trusted_learned (all 5 scenarios)",
     chainIntegrityCheck: "B-to-A chaining validated: each node's B output matches next node's A input",
     parseErrors: 0
   )

---

## TU-006-002 — PairParser validates chain integrity (B→A chaining)

A: ParsedPairCollection (from TU-006-001)
B: ChainIntegrityValidation(
     node01_B_to_node02_A: "DomainPacket[] → DomainPacket: consistent across all 5 scenarios",
     node02_B_to_node03_A: "WorkBranch → WorkBranch[]: consistent",
     node03_B_to_node04_A: "ExecutionSchema step → ProcessedDomain: consistent",
     node05_B_to_node06_A: "AppliedResult → AppliedResult[]: consistent",
     node06_B_to_node07_A: "TranslatedResult → TranslatedResult[]: consistent",
     node07_B_to_node08_A: "ArbiterReviewRequest → ArbiterReviewRequest: consistent",
     node08_B_to_node09_A: "ArbiterDecision → ArbiterDecision[]: consistent",
     gateB: "passed — chain integrity verified for all 5 scenarios × 9 nodes = 45 node pairs"
   )

---

## TU-006-003 — PairParser validates FactoryState in Formatter output

A: ParsedPairCollection (from TU-006-001)
B: FactoryStateValidation(
     allNode09OutputsHaveFactoryState: true,
     factoryStatesObserved: {"trusted_learned": 5},
     unexpectedFactoryStates: 0,
     gateB: "passed — all 5 scenarios output FactoryState: trusted_learned"
   )

---

## TU-006-004 — ModelTrainer receives parsed collection for layer-2 files 002-006

A: ParsedPairCollection (from TU-006-001, validated in TU-006-002)
B: TrainingMetrics(
     scenariosSubmitted: 5,
     scenariosAccepted: 5,
     scenariosRejected: 0,
     chainIntegrityScore: "45/45 node pairs valid",
     coverageGap: "probationary_rejected scenario covered in file 009; documentation scenario in file 010",
     trainingConfidence: 0.93,
     gateB: "passed — 0 rejected scenarios; chain integrity 100%; confidence 0.93 >= 0.85"
   )
