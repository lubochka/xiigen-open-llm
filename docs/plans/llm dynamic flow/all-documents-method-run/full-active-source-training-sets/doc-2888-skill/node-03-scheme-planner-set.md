# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; audit protocol v1 0 is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, audit protocol v1 0. Keep impact explanation and reprocess instruction separate. Allow approval boundary and audit protocol v1 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, audit protocol v1 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; audit protocol v1 0 is present and needs its own handling; every audit runs the same 10 checks no ad hoc reviews is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, audit protocol v1 0, every audit runs the same 10 checks no ad hoc reviews. Keep reprocess instruction and approval boundary separate. Allow audit protocol v1 0 and every audit runs the same 10 checks no ad hoc reviews to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, audit protocol v1 0, every audit runs the same 10 checks no ad hoc reviews.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: approval boundary is present and needs its own handling; audit protocol v1 0 is present and needs its own handling; every audit runs the same 10 checks no ad hoc reviews is present and needs its own handling; the 10 point audit checklist is present and needs its own handling.

B: Create work pieces for approval boundary, audit protocol v1 0, every audit runs the same 10 checks no ad hoc reviews, the 10 point audit checklist. Keep approval boundary and audit protocol v1 0 separate. Allow every audit runs the same 10 checks no ad hoc reviews and the 10 point audit checklist to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, audit protocol v1 0, every audit runs the same 10 checks no ad hoc reviews, the 10 point audit checklist.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: audit protocol v1 0 is present and needs its own handling; every audit runs the same 10 checks no ad hoc reviews is present and needs its own handling; the 10 point audit checklist is present and needs its own handling; arithmetic chain verification is present and needs its own handling.

B: Create work pieces for audit protocol v1 0, every audit runs the same 10 checks no ad hoc reviews, the 10 point audit checklist, arithmetic chain verification. Keep audit protocol v1 0 and every audit runs the same 10 checks no ad hoc reviews separate. Allow the 10 point audit checklist and arithmetic chain verification to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: audit protocol v1 0, every audit runs the same 10 checks no ad hoc reviews, the 10 point audit checklist, arithmetic chain verification.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Audit Protocol v1.0: every audit runs the same 10 checks no ad hoc reviews is present and needs its own handling; the 10 point audit checklist is present and needs its own handling; arithmetic chain verification is present and needs its own handling; blast radius tagging is present and needs its own handling.

B: Create work pieces for every audit runs the same 10 checks no ad hoc reviews, the 10 point audit checklist, arithmetic chain verification, blast radius tagging. Keep every audit runs the same 10 checks no ad hoc reviews and the 10 point audit checklist separate. Allow arithmetic chain verification and blast radius tagging to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: every audit runs the same 10 checks no ad hoc reviews, the 10 point audit checklist, arithmetic chain verification, blast radius tagging.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

