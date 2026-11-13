# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; patch reference contract template md add arbiterconfig field is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, patch reference contract template md add arbiterconfig field. Keep impact explanation and reprocess instruction separate. Allow approval boundary and patch reference contract template md add arbiterconfig field to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, patch reference contract template md add arbiterconfig field.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; patch reference contract template md add arbiterconfig field is present and needs its own handling; applies to reference contract template md is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, patch reference contract template md add arbiterconfig field, applies to reference contract template md. Keep reprocess instruction and approval boundary separate. Allow patch reference contract template md add arbiterconfig field and applies to reference contract template md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, patch reference contract template md add arbiterconfig field, applies to reference contract template md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: approval boundary is present and needs its own handling; patch reference contract template md add arbiterconfig field is present and needs its own handling; applies to reference contract template md is present and needs its own handling; version v2 0 0 date 2026 03 25 is present and needs its own handling.

B: Create work pieces for approval boundary, patch reference contract template md add arbiterconfig field, applies to reference contract template md, version v2 0 0 date 2026 03 25. Keep approval boundary and patch reference contract template md add arbiterconfig field separate. Allow applies to reference contract template md and version v2 0 0 date 2026 03 25 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, patch reference contract template md add arbiterconfig field, applies to reference contract template md, version v2 0 0 date 2026 03 25.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: patch reference contract template md add arbiterconfig field is present and needs its own handling; applies to reference contract template md is present and needs its own handling; version v2 0 0 date 2026 03 25 is present and needs its own handling; how to apply is present and needs its own handling.

B: Create work pieces for patch reference contract template md add arbiterconfig field, applies to reference contract template md, version v2 0 0 date 2026 03 25, how to apply. Keep patch reference contract template md add arbiterconfig field and applies to reference contract template md separate. Allow version v2 0 0 date 2026 03 25 and how to apply to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: patch reference contract template md add arbiterconfig field, applies to reference contract template md, version v2 0 0 date 2026 03 25, how to apply.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PATCH: reference--contract-template.md — Add arbiterConfig field: applies to reference contract template md is present and needs its own handling; version v2 0 0 date 2026 03 25 is present and needs its own handling; how to apply is present and needs its own handling; pre flight is present and needs its own handling.

B: Create work pieces for applies to reference contract template md, version v2 0 0 date 2026 03 25, how to apply, pre flight. Keep applies to reference contract template md and version v2 0 0 date 2026 03 25 separate. Allow how to apply and pre flight to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: applies to reference contract template md, version v2 0 0 date 2026 03 25, how to apply, pre flight.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

