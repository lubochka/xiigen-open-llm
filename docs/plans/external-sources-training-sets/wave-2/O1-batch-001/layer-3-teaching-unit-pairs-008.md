# Layer 3 — Teaching Unit Pairs (008)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-008-001 — ModelTrainer: batch training metrics for router expansion set (0-B files 002-012)

A: ParsedPairCollection(
     source: "layer-0b-router-pairs files 002-012",
     totalFiles: 11,
     totalPairs: 55
   )
B: TrainingMetrics(
     pairsSubmitted: 55,
     pairsAccepted: 55,
     pairsRejected: 0,
     routingPatternsCovered: [
       "single predicate → 1 decision",
       "SECURITY+AUTHORIZATION → 1 decision (deduplication)",
       "fan-out → N decisions",
       "knowledge_edge → 0 decisions",
       "QA always last"
     ],
     v6CorruptionTypesCovered: ["v6-03 wrong routing (files 011)"],
     referenceFilesCovered: ["routing table (012)", "deduplication rules (010)", "routing ordering (009)"],
     trainingConfidence: 0.92,
     gateB: "passed — 55/55 accepted; deduplication rule demonstrated in multiple files"
   )

---

## TU-008-002 — ModelTrainer: routing decision count accuracy

A: TrainingMetrics (from TU-008-001)
B: RoutingCountAccuracyTest(
     securityAlone: "PASS — 1 decision to 3.4",
     authorizationAlone: "PASS — 1 decision to 3.4",
     securityPlusAuthorization: "PASS — 1 decision to 3.4 (not 2)",
     securityPlusApiContract: "PASS — 2 decisions (3.4, 3.3)",
     securityPlusAuthPlusApiPlusTest: "PASS — 3 decisions (3.4, 3.3, 5.1)",
     gateB: "passed — all 5 routing count cases: PASS"
   )

---

## TU-008-003 — ModelTrainer: training run summary for all 0-B pairs

A: Combined ParsedPairCollection (files 001-012, including original file)
   totalPairs: 3 (original) + 55 (expansion) = 58
B: TrainingMetrics(
     totalPairs: 58,
     pairsAccepted: 58,
     pairsRejected: 0,
     averageConfidence: 0.92,
     deduplicationPrecision: "100% — no false double-routing in any pair",
     gateB: "passed — 58/58 accepted; deduplication precision 100%"
   )
