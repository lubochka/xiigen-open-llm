# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen auth plan v2 client implementation reference is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen auth plan v2 client implementation reference. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen auth plan v2 client implementation reference to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen auth plan v2 client implementation reference.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen auth plan v2 client implementation reference is present and needs its own handling; do not use v2 for server side auth execution use v3 0 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen auth plan v2 client implementation reference, do not use v2 for server side auth execution use v3 0. Keep reprocess instruction and approval boundary separate. Allow xiigen auth plan v2 client implementation reference and do not use v2 for server side auth execution use v3 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen auth plan v2 client implementation reference, do not use v2 for server side auth execution use v3 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: approval boundary is present and needs its own handling; xiigen auth plan v2 client implementation reference is present and needs its own handling; do not use v2 for server side auth execution use v3 0 is present and needs its own handling; superseded by v3 0 is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen auth plan v2 client implementation reference, do not use v2 for server side auth execution use v3 0, superseded by v3 0. Keep approval boundary and xiigen auth plan v2 client implementation reference separate. Allow do not use v2 for server side auth execution use v3 0 and superseded by v3 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen auth plan v2 client implementation reference, do not use v2 for server side auth execution use v3 0, superseded by v3 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: xiigen auth plan v2 client implementation reference is present and needs its own handling; do not use v2 for server side auth execution use v3 0 is present and needs its own handling; superseded by v3 0 is present and needs its own handling; xiigen auth admin role execution plan v2 0 is present and needs its own handling.

B: Create work pieces for xiigen auth plan v2 client implementation reference, do not use v2 for server side auth execution use v3 0, superseded by v3 0, xiigen auth admin role execution plan v2 0. Keep xiigen auth plan v2 client implementation reference and do not use v2 for server side auth execution use v3 0 separate. Allow superseded by v3 0 and xiigen auth admin role execution plan v2 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen auth plan v2 client implementation reference, do not use v2 for server side auth execution use v3 0, superseded by v3 0, xiigen auth admin role execution plan v2 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen Auth Plan v2 — Client Implementation Reference: do not use v2 for server side auth execution use v3 0 is present and needs its own handling; superseded by v3 0 is present and needs its own handling; xiigen auth admin role execution plan v2 0 is present and needs its own handling; what changed from v1 0 is present and needs its own handling.

B: Create work pieces for do not use v2 for server side auth execution use v3 0, superseded by v3 0, xiigen auth admin role execution plan v2 0, what changed from v1 0. Keep do not use v2 for server side auth execution use v3 0 and superseded by v3 0 separate. Allow xiigen auth admin role execution plan v2 0 and what changed from v1 0 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: do not use v2 for server side auth execution use v3 0, superseded by v3 0, xiigen auth admin role execution plan v2 0, what changed from v1 0.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

