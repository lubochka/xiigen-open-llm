# Layer 3 — Teaching Unit Pairs (010)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-010-001 — ModelTrainer: full O1-batch-001 wave-2 expansion training run

A: BatchTrainingDescriptor(
     batchId: "O1-batch-001",
     wave: "wave-2",
     expansionFiles: 92,
     originalFiles: 9,
     totalFiles: 101,
     layers: ["0-A", "0-B", "0-C", "0-D", "1", "2", "3", "held-out-corruption", "cross-layer-synthesis"],
     filesPerLayer: 12,
     crossLayerSynthesis: 4,
     manifest: 1
   )
B: TrainingMetrics(
     totalPairsInExpansion: {
       "0-A (55 pairs × 11 files)": 55,
       "0-B (55 pairs × 11 files)": 55,
       "0-C (44 pairs × 11 files)": 44,
       "0-D (50 pairs × 11 files)": 50,
       "1 (55 pairs × 11 files)": 55,
       "2 (10 scenarios × 9 nodes × 11 files)": 90,
       "3 (44 pairs × 11 files)": 44,
       "held-out-corruption (44 pairs × 11 files)": 44
     },
     totalTrainingPairsExpansion: 437,
     pairsAccepted: 437,
     pairsRejected: 0,
     averageConfidence: 0.91,
     gateB: "passed — 437/437 accepted; confidence 0.91 >= 0.85"
   )

---

## TU-010-002 — Full batch coverage summary

A: BatchTrainingDescriptor (from TU-010-001)
B: CoverageSummary(
     predicatesCovered: ["SECURITY", "AUTHORIZATION", "API_CONTRACT", "IMPLEMENT", "TEST", "DOCUMENT", "QA", "ARCHITECTURE"],
     subPhasesCovered: ["3.4", "3.3", "5.1", "2.1", "4.1", "7.x"],
     boundaryRulesCovered: 12,
     v6CorruptionTypesCovered: ["v6-01", "v6-02", "v6-03"],
     heldOutPairs: 11,
     knowledgeEdgeScenariosCovered: 40,
     nineNodeScenariosCovered: 10,
     gateB: "passed — all 3 v6 corruption types covered; all 6 sub-phases covered; held-out >= 11"
   )

---

## TU-010-003 — Final wave-2 expansion acceptance verdict

A: CoverageSummary (from TU-010-002) + TrainingMetrics (from TU-010-001)
B: WaveAcceptanceVerdict(
     verdict: "ACCEPTED",
     gateAssertions: [
       "Training pairs total: 437 >= 200 — PASS",
       "Pairs rejected: 0 == 0 — PASS",
       "v6 corruption types covered: 3/3 — PASS",
       "Sub-phases covered: 6/6 — PASS",
       "Chain integrity (nine-node): 100% — PASS",
       "Held-out pairs: 11 >= 2 — PASS",
       "Average confidence: 0.91 >= 0.85 — PASS"
     ],
     promotionAction: "PROMOTE_O1_BATCH_001_WAVE2_EXPANSION to EXTERNAL-SOURCE-REGISTRY-v6.md"
   )
