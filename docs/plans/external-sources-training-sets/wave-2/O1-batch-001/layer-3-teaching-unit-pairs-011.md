# Layer 3 — Teaching Unit Pairs (011)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-011-001 — v6-02 teaching pair: "login page" prompt misclassified — PairParser detects error

A: CorruptPair(
     source: "v6-02-example-001",
     prompt: "The login page is broken.",
     corruptOutput: "Predicates: ['SECURITY'] | TransitionType: feature_addition",
     expectedOutput: "Predicates: [] | TransitionType: none | KnowledgeEdge: true"
   )
B: PairParserDetectionResult(
     errorType: "v6-02 gap suppression",
     detectionMethod: "lexicon scan: 'login' not in SECURITY lexicon; no predicate should fire",
     corruptPredicateFound: "SECURITY",
     correctPredicates: [],
     verdict: "REJECTED — corrupt pair detected",
     rejectionReason: "SECURITY predicate invented without lexicon evidence; 'login' is not a SECURITY token"
   )

---

## TU-011-002 — v6-02 teaching pair: "tenant" not caught — PairParser detects error

A: CorruptPair(
     source: "v6-02-example-002",
     prompt: "Each tenant gets their own workspace.",
     corruptOutput: "Predicates: [] | TransitionType: none | KnowledgeEdge: true",
     expectedOutput: "Predicates: ['AUTHORIZATION'] | TransitionType: feature_addition"
   )
B: PairParserDetectionResult(
     errorType: "v6-02 gap suppression (inverse)",
     detectionMethod: "lexicon scan: 'tenant' IS in AUTHORIZATION lexicon; predicate should fire",
     missingPredicate: "AUTHORIZATION",
     correctPredicates: ["AUTHORIZATION"],
     verdict: "REJECTED — corrupt pair detected",
     rejectionReason: "AUTHORIZATION predicate suppressed despite 'tenant' lexicon match"
   )

---

## TU-011-003 — PairParser error reporting format

A: Two rejected corrupt pairs (from TU-011-001 and TU-011-002)
B: ErrorReport(
     totalPairsEvaluated: 2,
     totalRejected: 2,
     errorSummary: [
       {pairId: "v6-02-example-001", error: "v6-02 invented SECURITY on 'login'"},
       {pairId: "v6-02-example-002", error: "v6-02 suppressed AUTHORIZATION on 'tenant'"}
     ],
     correctedPairs: [
       {pairId: "v6-02-example-001", corrected: "Predicates: [] | knowledge_edge: true"},
       {pairId: "v6-02-example-002", corrected: "Predicates: ['AUTHORIZATION'] | feature_addition"}
     ],
     gateB: "detection accuracy: 2/2 = 100% — PASS"
   )
