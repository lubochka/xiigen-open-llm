# Code Documentation Depth Gap Diagnosis

Generated: 2026-05-14T07:17:27.9568208+03:00

## Headline

No depth improvement means the current generator is no longer selecting the exact uncovered symbols/edges.

## Missing Positive-Ready Method Rows

- Total: 75
- With inventory body: 75
- Without inventory body: 0

Cause: These methods have source bodies, but every generated row for them has been source/version blocked. This is a selection-policy gap, not a missing-code-resource gap.

Missing resource/change: targeted positive D2 leaf rows for these exact symbols

## D4 / Workflow-Level Method Rows

- Row-symbol D4 gaps: 123
- Have outgoing calls: 0
- No outgoing calls: 123
- Already propagated as known callee in D4 rows: 70
- No known callee propagation yet: 53

Cause: The current audit counts D4 only when the method is the row symbol. For leaf/callee methods this overstates the gap because bottom-up coverage should also count known_callees inside caller D4 rows.

Missing resource/change: audit metric needs callee-propagation coverage; remaining true gaps need incoming caller/workflow rows or richer static call extraction

## Missing-Callee Negative Edge Rows

- Total: 40
- Single-callee callers: 32
- Multi-callee callers: 8

Cause: The generator creates transit negatives by rotating modes, not by reading uncovered edges. Single-callee callers especially require explicit empty-transit negative rows.

Missing resource/change: targeted empty-transit negatives for single-callee callers and targeted partial-transit negatives for multi-callee callers

## Required Protocol Change

- Before each 10-batch interval, read METHOD-DEPTH-COVERAGE.jsonl and EDGE-DEPTH-COVERAGE.jsonl.
- Reserve part of each 50-row batch for exact uncovered symbols/edges instead of relying on modulo rotation.
- Add targeted positive rows for methods with positive_ready_rows == 0.
- Count leaf/callee propagation through known_callees in D4 caller rows; do not require leaf methods to become row-symbol D4.
- Add targeted empty-transit negatives for single-callee edge gaps.
- Add targeted partial-transit negatives for multi-callee edge gaps.
- If incoming caller/workflow context is missing, improve static call extraction or add workflow/lifecycle rows from allowed MIT/Apache sources.

