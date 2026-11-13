# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen new feature plan v2 is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen new feature plan v2. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen new feature plan v2 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen new feature plan v2.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen new feature plan v2 is present and needs its own handling; date 2026 03 26 supersedes xiigen feature plan md v1 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen new feature plan v2, date 2026 03 26 supersedes xiigen feature plan md v1. Keep reprocess instruction and approval boundary separate. Allow xiigen new feature plan v2 and date 2026 03 26 supersedes xiigen feature plan md v1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen new feature plan v2, date 2026 03 26 supersedes xiigen feature plan md v1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: approval boundary is present and needs its own handling; xiigen new feature plan v2 is present and needs its own handling; date 2026 03 26 supersedes xiigen feature plan md v1 is present and needs its own handling; corrections applied from 4 independent reviews is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen new feature plan v2, date 2026 03 26 supersedes xiigen feature plan md v1, corrections applied from 4 independent reviews. Keep approval boundary and xiigen new feature plan v2 separate. Allow date 2026 03 26 supersedes xiigen feature plan md v1 and corrections applied from 4 independent reviews to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen new feature plan v2, date 2026 03 26 supersedes xiigen feature plan md v1, corrections applied from 4 independent reviews.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: xiigen new feature plan v2 is present and needs its own handling; date 2026 03 26 supersedes xiigen feature plan md v1 is present and needs its own handling; corrections applied from 4 independent reviews is present and needs its own handling; r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering is present and needs its own handling.

B: Create work pieces for xiigen new feature plan v2, date 2026 03 26 supersedes xiigen feature plan md v1, corrections applied from 4 independent reviews, r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering. Keep xiigen new feature plan v2 and date 2026 03 26 supersedes xiigen feature plan md v1 separate. Allow corrections applied from 4 independent reviews and r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen new feature plan v2, date 2026 03 26 supersedes xiigen feature plan md v1, corrections applied from 4 independent reviews, r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — New Feature Plan v2: date 2026 03 26 supersedes xiigen feature plan md v1 is present and needs its own handling; corrections applied from 4 independent reviews is present and needs its own handling; r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering is present and needs its own handling; r3 a 6 sizing 2 3 sessions b 2 fail open guarantee is present and needs its own handling.

B: Create work pieces for date 2026 03 26 supersedes xiigen feature plan md v1, corrections applied from 4 independent reviews, r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering, r3 a 6 sizing 2 3 sessions b 2 fail open guarantee. Keep date 2026 03 26 supersedes xiigen feature plan md v1 and corrections applied from 4 independent reviews separate. Allow r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering and r3 a 6 sizing 2 3 sessions b 2 fail open guarantee to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: date 2026 03 26 supersedes xiigen feature plan md v1, corrections applied from 4 independent reviews, r1 b 1 stalledafterflows 4 a 7 table split c 3 c 2 ordering, r3 a 6 sizing 2 3 sessions b 2 fail open guarantee.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

