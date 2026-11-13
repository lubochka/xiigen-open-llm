# Layer 3 — Teaching Unit Pairs (002)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-002-001 — PairParser ingests layer-0a-compiler-pairs files 002-006

A: BatchFileDescriptor(
     files: ["layer-0a-compiler-pairs-002.md", "layer-0a-compiler-pairs-003.md",
             "layer-0a-compiler-pairs-004.md", "layer-0a-compiler-pairs-005.md",
             "layer-0a-compiler-pairs-006.md"],
     layer: "0-A",
     pairKind: "compiler"
   )
B: ParsedPairCollection(
     totalFiles: 5,
     totalPairs: 25,
     pairsPerFile: 5,
     predicatesObserved: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST"],
     transitionTypes: {feature_addition: 20, documentation: 5},
     knowledgeEdgePairs: 1,
     parseErrors: 0,
     boundaryPairsFound: ["login-vs-authentication", "access-denied-vs-access-control"]
   )

---

## TU-002-002 — PairParser validates predicate completeness

A: ParsedPairCollection (from TU-002-001)
B: ValidationResult(
     allPredicatesHaveLexiconEvidence: true,
     boundaryRulesCovered: ["login ≠ SECURITY", "access denied ≠ AUTHORIZATION", "verify isolation = TEST not SECURITY"],
     transitionTypeConsistency: true,
     gateB: "passed — 25 pairs parsed, 0 errors, all boundary rules present"
   )

---

## TU-002-003 — PairParser pair count assertion

A: ParsedPairCollection (from TU-002-001)
B: PairCountAssertion(
     expectedPairs: 25,
     actualPairs: 25,
     assertion: "actual == expected",
     result: "PASS",
     gateB: "pair count: 25 == 25"
   )

---

## TU-002-004 — ModelTrainer receives parsed collection for 0-A files 002-006

A: ParsedPairCollection (from TU-002-001, validated in TU-002-002)
B: TrainingMetrics(
     pairsSubmitted: 25,
     pairsAccepted: 25,
     pairsRejected: 0,
     dominantPredicate: "SECURITY",
     coverageGap: "DOCUMENT predicate not covered in files 002-006 — covered in files 010-012",
     trainingConfidence: 0.91,
     gateB: "passed — 0 rejected pairs; confidence 0.91 >= 0.85"
   )
