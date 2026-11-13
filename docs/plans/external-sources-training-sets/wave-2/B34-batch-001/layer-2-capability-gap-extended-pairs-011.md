# Layer 2 — Capability Gap Extended Pairs (011)
# Batch: B34-batch-001 | Scenario: B34 — Skills and guidance gap
# Production date: 2026-05-05 | pair_kind: capability_gap_extended

---

## B34-gap-011 — UnitTestingFlow51 nine-node: Write unit tests for scoring algorithm

Request: "Write unit tests for the skill scoring algorithm in the assessment platform."

### Flow: UnitTestingFlow51 (TEST predicate; unit + algorithm → 5.1)

**Node 1**: Parse. "write unit tests" → TEST (test-writing verb). "scoring algorithm" → pure function → 5.1 routing.
**Node 2**: Predicate set = ["TEST"]. Sub-phase 5.1.
**Node 3**: Test scope: skill score calculation — weighted scoring, category aggregation, normalization, edge cases.
**Node 4**: Test gates: basic score calculation, weight application, category aggregation, zero-score edge case, max-score edge case, normalization range.
**Node 5**: Arbiter: UnitTestingArbiter51
**Node 6**: Test 1 — Basic score: 3 skills, weights applied → expected total ✓
  Test 2 — Weight application: 0.3+0.5+0.2 = 1.0 ✓
  Test 3 — Category aggregation: sub-scores roll up correctly ✓
  Test 4 — Zero score: all zeroes → 0.0 ✓
  Test 5 — Max score: all 100s → 100.0 ✓
  Test 6 — Normalization: output always in [0.0, 100.0] range ✓
**Node 7**: All 6 tests pass. Full algorithm coverage.
**Node 8**: Output: skill-scorer-unit-tests.md | 6 tests | all green
**Node 9**: Verdict: TEST_PASS — scoring algorithm cleared. TransitionType: documentation.
