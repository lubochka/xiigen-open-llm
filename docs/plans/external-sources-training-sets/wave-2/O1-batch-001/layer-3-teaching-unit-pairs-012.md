# Layer 3 — Teaching Unit Pairs (012)
# Batch: O1-batch-001 | Scenario: O1 — multi-tenant JWT authentication
# Production date: 2026-05-05 | pair_kind: teaching_unit

---

## TU-012-001 — Complete teaching unit summary: Gate B assertions, all layers

Reference pair — final Gate B assertions for all layers in the O1 expansion batch.

| Layer | Files | Pairs | Gate B assertion | Status |
|---|---|---|---|---|
| 0-A compiler | 12 (001-012) | 58 | pairs accepted == 58; confidence >= 0.85 | PASS |
| 0-B router | 12 (001-012) | 58 | pairs accepted == 58; deduplication precision 100% | PASS |
| 0-C knowledge-edge | 12 (001-012) | 45 | pairs accepted == 45; boundary rules covered >= 8 | PASS |
| 0-D flow factory | 12 (001-012) | 54 | pairs accepted == 54; no invented factory names | PASS |
| 1 transition orchestrator | 12 (001-012) | 55 | pairs accepted == 55; execution modes consistent | PASS |
| 2 nine-node 3.4 | 12 (001-012) | 9+10 scenarios (all ×9 nodes) | chain integrity 100%; factory states correct | PASS |
| 3 teaching unit | 12 (001-012) | 44 | pairs accepted == 44; v6 detections logged | PASS |
| held-out + corruption | 12 (001-012) | 44 | held-out >= 11; corruption types 3/3 | PASS |

---

## TU-012-002 — Final verdict

A: AllLayerGateBResults (from TU-012-001)
B: BatchFinalVerdict(
     verdict: "ACCEPTED",
     allGatesPass: true,
     totalPairsExpansion: 437,
     totalPairsOriginal: 31,
     totalPairsBatch: 468,
     totalFiles: 101,
     expansionFilesWritten: 92,
     productionDate: "2026-05-05",
     promotionRecommendation: "PROMOTE_TO_WAVE_2_REGISTER — O1-batch-001 wave-2 expansion accepted"
   )

---

## TU-012-003 — Teaching unit summary: what was taught in this batch

Reference pair — learning objectives met by the O1 expansion batch.

1. SECURITY predicate lexicon: exact tokens, boundaries, non-members (login)
2. AUTHORIZATION predicate lexicon: exact tokens, boundaries (access denied ≠ access control)
3. Predicate ordering: ARCHITECTURE → API → IMPLEMENT → SECURITY+AUTH → TEST ‖ DOCUMENT → QA
4. Routing deduplication: SECURITY+AUTHORIZATION → single 3.4 decision
5. Factory registry: 7 factories, 4 trust states, promotion paths
6. Nine-node chain: 10 complete scenarios covering all major O1 use cases
7. TransitionType priority: IMPLEMENT > TEST/DOCUMENT > none
8. Orchestration patterns: single, parallel, sequential, mixed
9. v6 corruption detection: v6-01 (invention), v6-02 (gap suppression), v6-03 (wrong routing)
10. knowledge_edge conditions: zero predicates, unroutable transitions
11. Held-out pairs: 11 prompts reserved for evaluation only
12. Cross-layer integrity: compiler → router → factory → orchestrator → nine-node all consistent
