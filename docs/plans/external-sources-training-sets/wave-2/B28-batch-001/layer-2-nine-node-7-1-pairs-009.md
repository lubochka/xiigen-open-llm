# Layer 2 — Nine-Node Flow Pairs (009)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: nine_node_flow (QAGatingFlow73)

---

## B28-nine-009 — SearchService v2 QA gate — FAIL

Request: "QA sign-off on the SearchService v2 with the new Elasticsearch (search engine) indexing strategy."

### Flow: QAGatingFlow73

**Node 1**: Target: SearchService v2 | Change: new Elasticsearch indexing strategy with stemming and synonyms
**Node 2**: Gates: relevance quality, performance, regression on existing queries, spell correction
**Node 3**: Gate 1 — Relevance: top-5 results for "laptop" → mixed furniture results in position 3 ✗ FAIL
  Gate 2 — Performance: 95th percentile search latency = 340ms → threshold 200ms ✗ FAIL
  Gate 3 — Regression: 12 of 50 existing query test cases return different result ordering ✗
  Gate 4 — Spell correction: "lapotp" → "laptop" suggestions ✓
**Node 4**: CRITICAL: relevance degradation in top-5 results; performance regression 70% over threshold
**Node 5**: Arbiter: ReleaseGateArbiter
**Node 6**: Coverage: relevance: FAIL; performance: FAIL; regression: FAIL; spell correction: PASS
**Node 7**: FAIL — 3 of 4 gates failed; release blocked
**Node 8**: QA report: SearchService-v2-QA-FAIL.md | 3 blockers filed
**Node 9**: Verdict: FAIL — release blocked; indexing strategy requires tuning before re-QA

