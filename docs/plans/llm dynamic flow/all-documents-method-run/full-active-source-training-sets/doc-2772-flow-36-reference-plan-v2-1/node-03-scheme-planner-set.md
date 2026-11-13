# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; the feature registry flow feature registry is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, the feature registry flow feature registry. Keep impact explanation and reprocess instruction separate. Allow approval boundary and the feature registry flow feature registry to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, the feature registry flow feature registry.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; the feature registry flow feature registry is present and needs its own handling; reference plan v2 0 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, the feature registry flow feature registry, reference plan v2 0. Keep reprocess instruction and approval boundary separate. Allow the feature registry flow feature registry and reference plan v2 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, the feature registry flow feature registry, reference plan v2 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: approval boundary is present and needs its own handling; the feature registry flow feature registry is present and needs its own handling; reference plan v2 0 is present and needs its own handling; date 2026 03 20 status draft awaiting gate c approval is present and needs its own handling.

B: Create work pieces for approval boundary, the feature registry flow feature registry, reference plan v2 0, date 2026 03 20 status draft awaiting gate c approval. Keep approval boundary and the feature registry flow feature registry separate. Allow reference plan v2 0 and date 2026 03 20 status draft awaiting gate c approval to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, the feature registry flow feature registry, reference plan v2 0, date 2026 03 20 status draft awaiting gate c approval.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: the feature registry flow feature registry is present and needs its own handling; reference plan v2 0 is present and needs its own handling; date 2026 03 20 status draft awaiting gate c approval is present and needs its own handling; what changed from v1 and why is present and needs its own handling.

B: Create work pieces for the feature registry flow feature registry, reference plan v2 0, date 2026 03 20 status draft awaiting gate c approval, what changed from v1 and why. Keep the feature registry flow feature registry and reference plan v2 0 separate. Allow date 2026 03 20 status draft awaiting gate c approval and what changed from v1 and why to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the feature registry flow feature registry, reference plan v2 0, date 2026 03 20 status draft awaiting gate c approval, what changed from v1 and why.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The feature registry flow — Feature Registry: reference plan v2 0 is present and needs its own handling; date 2026 03 20 status draft awaiting gate c approval is present and needs its own handling; what changed from v1 and why is present and needs its own handling; executive summary is present and needs its own handling.

B: Create work pieces for reference plan v2 0, date 2026 03 20 status draft awaiting gate c approval, what changed from v1 and why, executive summary. Keep reference plan v2 0 and date 2026 03 20 status draft awaiting gate c approval separate. Allow what changed from v1 and why and executive summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: reference plan v2 0, date 2026 03 20 status draft awaiting gate c approval, what changed from v1 and why, executive summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

