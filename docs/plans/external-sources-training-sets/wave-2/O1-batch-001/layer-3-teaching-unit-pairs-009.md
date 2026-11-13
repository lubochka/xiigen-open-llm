# Layer 3 — Teaching Unit Pairs (009)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-009-001 — ModelTrainer: batch training metrics for layer-2 nine-node set (files 002-012)

A: ParsedPairCollection(
     source: "layer-2-nine-node-3.4-pairs files 002-012",
     totalFiles: 11,
     totalScenarios: 10,
     totalNodePairs: 90
   )
B: TrainingMetrics(
     scenariosSubmitted: 10,
     scenariosAccepted: 10,
     scenariosRejected: 0,
     chainIntegrity: "90/90 node pairs chain-valid (B→A)",
     factoryStatesObserved: {
       "trusted_learned": 10,
       "probationary_rejected (5.1 noted)": 1,
       "deterministic_generic (2.1 noted)": 1
     },
     scenarioTypesCovered: ["token validation", "RBAC model", "API key", "tenant isolation", "refresh token", "MFA", "service account", "probationary rejected", "documentation", "full suite"],
     v6CorruptionCoverage: "probationary_rejected scenario (file 009)",
     gateB: "passed — 10/10 scenarios; chain integrity 100%; confidence >= 0.90"
   )

---

## TU-009-002 — ModelTrainer: chain integrity verification across layer-2 scenarios

A: ChainIntegrityReport(
     scenarios: 10,
     nodesPerScenario: 9,
     totalChecks: 90
   )
B: ChainIntegrityResult(
     passed: 90,
     failed: 0,
     weakestLink: "Node 03→04 (schema planner step selection) — requires correct step ID reference",
     gateB: "passed — 90/90 chain checks: PASS"
   )

---

## TU-009-003 — ModelTrainer: training run summary for all layer-2 pairs

A: Combined ParsedPairCollection (file 001 original + files 002-012 expansion)
   totalScenarios: 9 (original, 9 nodes each) + 10 (expansion) = 10 distinct new scenarios; original 9 nodes retained
B: TrainingMetrics(
     totalNodePairs: 9*9 + 10*9 = 81 + 90 = 171 (note: file 001 has 9 scenarios × 9 nodes = 81)
     Note: original layer-2 file has 9 nodes in 1 scenario per node; expansion files have 1 full scenario per file
     Correction: original file 001 has 9 node-pair entries representing one full nine-node scenario
     totalNodePairs: 9 (original 1 scenario) + 90 (10 expansion scenarios) = 99
     pairsAccepted: 99,
     chainIntegrityScore: "99/99",
     averageConfidence: 0.92,
     gateB: "passed — 99/99 node pairs; chain integrity 100%"
   )
