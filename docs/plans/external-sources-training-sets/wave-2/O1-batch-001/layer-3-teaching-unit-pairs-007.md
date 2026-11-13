# Layer 3 — Teaching Unit Pairs (007)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-007-001 — ModelTrainer: batch training metrics for compiler expansion set (0-A files 002-012)

A: ParsedPairCollection(
     source: "layer-0a-compiler-pairs files 002-012",
     totalFiles: 11,
     totalPairs: 55
   )
B: TrainingMetrics(
     pairsSubmitted: 55,
     pairsAccepted: 55,
     pairsRejected: 0,
     predicatesCovered: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST", "DOCUMENT"],
     boundaryRulesCovered: [
       "login ≠ SECURITY",
       "access denied ≠ AUTHORIZATION (alone)",
       "verify isolation = TEST not SECURITY",
       "write tests = TEST (not IMPLEMENT)",
       "write [service] = IMPLEMENT"
     ],
     v6CorruptionTypesCovered: ["v6-02 gap suppression (files 011)"],
     referenceFilesCovered: ["predicate lexicon (012)", "TransitionType rules (012)", "ordering (012)"],
     trainingConfidence: 0.91,
     gateB: "passed — 55/55 accepted; all boundary rules present; confidence 0.91 >= 0.85"
   )

---

## TU-007-002 — ModelTrainer: precision on boundary discrimination after compiler training

A: TrainingMetrics (from TU-007-001)
B: BoundaryDiscriminationTest(
     loginVsAuthentication: "PASS — login: no predicate; authentication: SECURITY",
     accessDeniedVsAccessControl: "PASS — access denied: no predicate; access control: AUTHORIZATION",
     verifyIsolationVsSecurityIsolation: "PASS — verify isolation: TEST; security isolation: SECURITY",
     writeTestsVsWriteCode: "PASS — write tests: documentation; write [service]: feature_addition",
     gateB: "passed — all 4 boundary discriminations: PASS"
   )

---

## TU-007-003 — ModelTrainer: training run summary for all 0-A pairs

A: Combined ParsedPairCollection (files 001-012, including original file)
   totalPairs: 3 (original) + 55 (expansion) = 58
B: TrainingMetrics(
     totalPairs: 58,
     pairsAccepted: 58,
     pairsRejected: 0,
     confidenceRange: "0.88 — 0.95",
     averageConfidence: 0.91,
     gateB: "passed — 58/58 accepted; confidence range all >= 0.85"
   )
