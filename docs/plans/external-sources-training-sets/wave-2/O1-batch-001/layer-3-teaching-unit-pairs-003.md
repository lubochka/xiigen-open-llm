# Layer 3 — Teaching Unit Pairs (003)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-003-001 — PairParser ingests layer-0b-router-pairs files 002-006

A: BatchFileDescriptor(
     files: ["layer-0b-router-pairs-002.md", "layer-0b-router-pairs-003.md",
             "layer-0b-router-pairs-004.md", "layer-0b-router-pairs-005.md",
             "layer-0b-router-pairs-006.md"],
     layer: "0-B",
     pairKind: "router"
   )
B: ParsedPairCollection(
     totalFiles: 5,
     totalPairs: 25,
     routingDecisionPatterns: {
       "1 predicate → 1 decision": 8,
       "2 predicates same sub-phase → 1 decision": 5,
       "2 predicates different sub-phases → 2 decisions": 7,
       "3+ predicates → fan-out": 5
     },
     knowledgeEdgePairs: 1,
     deduplicationExamplesFound: 5,
     parseErrors: 0
   )

---

## TU-003-002 — PairParser validates deduplication rule coverage

A: ParsedPairCollection (from TU-003-001)
B: ValidationResult(
     deduplicationRuleCovered: true,
     deduplicationExample: "SECURITY+AUTHORIZATION → 1 routing decision to 3.4 (not 2)",
     fanOutRuleCovered: true,
     fanOutExample: "SECURITY+API_CONTRACT → 2 routing decisions (3.4, 3.3)",
     gateB: "passed — deduplication and fan-out both demonstrated"
   )

---

## TU-003-003 — PairParser validates routing count accuracy

A: ParsedPairCollection (from TU-003-001)
B: RoutingCountValidation(
     allRoutingCountsMatchDeduplicationRule: true,
     suspiciousEntries: 0,
     gateB: "passed — all routing decision counts are mathematically consistent with deduplication rule"
   )

---

## TU-003-004 — ModelTrainer receives parsed collection for 0-B files 002-006

A: ParsedPairCollection (from TU-003-001, validated in TU-003-002)
B: TrainingMetrics(
     pairsSubmitted: 25,
     pairsAccepted: 25,
     pairsRejected: 0,
     dominantPattern: "SECURITY+AUTHORIZATION deduplication to single 3.4 decision",
     coverageGap: "v6-03 corruption pairs not in files 002-006 — covered in files 011-012",
     trainingConfidence: 0.92,
     gateB: "passed — 0 rejected pairs; confidence 0.92 >= 0.85"
   )
