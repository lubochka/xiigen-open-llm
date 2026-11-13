# Layer 2 — Nine-Node Flow Pairs (012)
# Batch: B28-batch-001 | Scenario: B28 — QA automation and DevOps, QA predicate
# Production date: 2026-05-05 | pair_kind: nine_node_flow (QAGatingFlow73)

---

## B28-nine-012 — ReportingService v1 QA gate — FAIL

Request: "QA the ReportingService v1 — validate aggregation accuracy, time-zone normalization, and export formats (CSV, PDF, Excel)."

### Flow: QAGatingFlow73

**Node 1**: Target: ReportingService v1 | Features: data aggregation, time-zone normalization, multi-format export
**Node 2**: Gates: aggregation accuracy, time-zone handling, CSV export, PDF export, Excel export, performance
**Node 3**: Gate 1 — Aggregation: daily revenue total off by 0.3% for orders spanning midnight UTC ✗ FAIL
  Gate 2 — Time-zone: orders in UTC+8 normalized correctly ✓; UTC-5 offset applied twice for DST boundary ✗ FAIL
  Gate 3 — CSV export: all columns present, encoding correct ✓
  Gate 4 — PDF export: layout correct, no truncation ✓
  Gate 5 — Excel export: date columns serialized as text strings, not Excel date type ✗ WARNING
  Gate 6 — Performance: 10k-row report generated in 1.8s → threshold 2s ✓
**Node 4**: Critical defects found: aggregation error (midnight-spanning orders), DST double-offset bug. Non-critical: Excel date type serialization.
**Node 5**: Arbiter: QAGatingFlow73
**Node 6**: Coverage: aggregation: FAIL, time-zone: FAIL (DST), CSV: PASS, PDF: PASS, Excel: WARNING, performance: PASS
**Node 7**: FAIL — two critical defects block release: midnight aggregation error and DST double-offset. Excel date issue filed as non-blocking.
**Node 8**: QA report: ReportingService-v1-QA.md | BUG-891 (midnight aggregation), BUG-892 (DST double-offset), WARN-340 (Excel date type)
**Node 9**: Verdict: FAIL — ReportingService v1 not cleared. Fix BUG-891 and BUG-892 before re-QA cycle.
