# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; core modules what it contains is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, core modules what it contains. Keep impact explanation and reprocess instruction separate. Allow approval boundary and core modules what it contains to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, core modules what it contains.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; core modules what it contains is present and needs its own handling; a workspace projects permissions is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, core modules what it contains, a workspace projects permissions. Keep reprocess instruction and approval boundary separate. Allow core modules what it contains and a workspace projects permissions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, core modules what it contains, a workspace projects permissions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: approval boundary is present and needs its own handling; core modules what it contains is present and needs its own handling; a workspace projects permissions is present and needs its own handling; b visual editor page builder is present and needs its own handling.

B: Create work pieces for approval boundary, core modules what it contains, a workspace projects permissions, b visual editor page builder. Keep approval boundary and core modules what it contains separate. Allow a workspace projects permissions and b visual editor page builder to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, core modules what it contains, a workspace projects permissions, b visual editor page builder.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: core modules what it contains is present and needs its own handling; a workspace projects permissions is present and needs its own handling; b visual editor page builder is present and needs its own handling; c theme design system is present and needs its own handling.

B: Create work pieces for core modules what it contains, a workspace projects permissions, b visual editor page builder, c theme design system. Keep core modules what it contains and a workspace projects permissions separate. Allow b visual editor page builder and c theme design system to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: core modules what it contains, a workspace projects permissions, b visual editor page builder, c theme design system.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Core modules what it “contains”: a workspace projects permissions is present and needs its own handling; b visual editor page builder is present and needs its own handling; c theme design system is present and needs its own handling; d the content management system data modeling collections is present and needs its own handling.

B: Create work pieces for a workspace projects permissions, b visual editor page builder, c theme design system, d the content management system data modeling collections. Keep a workspace projects permissions and b visual editor page builder separate. Allow c theme design system and d the content management system data modeling collections to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: a workspace projects permissions, b visual editor page builder, c theme design system, d the content management system data modeling collections.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

