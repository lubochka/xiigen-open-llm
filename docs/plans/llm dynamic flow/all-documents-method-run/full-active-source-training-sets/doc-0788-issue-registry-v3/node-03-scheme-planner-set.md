# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; issue registry v3 corrections validation is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, issue registry v3 corrections validation. Keep impact explanation and reprocess instruction separate. Allow approval boundary and issue registry v3 corrections validation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, issue registry v3 corrections validation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; issue registry v3 corrections validation is present and needs its own handling; march 15 2026 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, issue registry v3 corrections validation, march 15 2026. Keep reprocess instruction and approval boundary separate. Allow issue registry v3 corrections validation and march 15 2026 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, issue registry v3 corrections validation, march 15 2026.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: approval boundary is present and needs its own handling; issue registry v3 corrections validation is present and needs its own handling; march 15 2026 is present and needs its own handling; apply to complete issue registry v3 md before implementation is present and needs its own handling.

B: Create work pieces for approval boundary, issue registry v3 corrections validation, march 15 2026, apply to complete issue registry v3 md before implementation. Keep approval boundary and issue registry v3 corrections validation separate. Allow march 15 2026 and apply to complete issue registry v3 md before implementation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, issue registry v3 corrections validation, march 15 2026, apply to complete issue registry v3 md before implementation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: issue registry v3 corrections validation is present and needs its own handling; march 15 2026 is present and needs its own handling; apply to complete issue registry v3 md before implementation is present and needs its own handling; validation summary is present and needs its own handling.

B: Create work pieces for issue registry v3 corrections validation, march 15 2026, apply to complete issue registry v3 md before implementation, validation summary. Keep issue registry v3 corrections validation and march 15 2026 separate. Allow apply to complete issue registry v3 md before implementation and validation summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: issue registry v3 corrections validation, march 15 2026, apply to complete issue registry v3 md before implementation, validation summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for ISSUE REGISTRY v3 — Corrections + Validation: march 15 2026 is present and needs its own handling; apply to complete issue registry v3 md before implementation is present and needs its own handling; validation summary is present and needs its own handling; correction 1 issue 03 is already fixed fix 28 applied is present and needs its own handling.

B: Create work pieces for march 15 2026, apply to complete issue registry v3 md before implementation, validation summary, correction 1 issue 03 is already fixed fix 28 applied. Keep march 15 2026 and apply to complete issue registry v3 md before implementation separate. Allow validation summary and correction 1 issue 03 is already fixed fix 28 applied to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: march 15 2026, apply to complete issue registry v3 md before implementation, validation summary, correction 1 issue 03 is already fixed fix 28 applied.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

