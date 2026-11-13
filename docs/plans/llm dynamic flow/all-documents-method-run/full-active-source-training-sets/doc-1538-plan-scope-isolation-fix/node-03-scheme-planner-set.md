# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; session type generation is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, session type generation. Keep impact explanation and reprocess instruction separate. Allow approval boundary and session type generation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, session type generation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; session type generation is present and needs its own handling; branch claude vigorous margulis is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, session type generation, branch claude vigorous margulis. Keep reprocess instruction and approval boundary separate. Allow session type generation and branch claude vigorous margulis to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, session type generation, branch claude vigorous margulis.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: approval boundary is present and needs its own handling; session type generation is present and needs its own handling; branch claude vigorous margulis is present and needs its own handling; date 2026 04 07 is present and needs its own handling.

B: Create work pieces for approval boundary, session type generation, branch claude vigorous margulis, date 2026 04 07. Keep approval boundary and session type generation separate. Allow branch claude vigorous margulis and date 2026 04 07 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, session type generation, branch claude vigorous margulis, date 2026 04 07.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: session type generation is present and needs its own handling; branch claude vigorous margulis is present and needs its own handling; date 2026 04 07 is present and needs its own handling; supersedes plan scope isolation fix md v1 is present and needs its own handling.

B: Create work pieces for session type generation, branch claude vigorous margulis, date 2026 04 07, supersedes plan scope isolation fix md v1. Keep session type generation and branch claude vigorous margulis separate. Allow date 2026 04 07 and supersedes plan scope isolation fix md v1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: session type generation, branch claude vigorous margulis, date 2026 04 07, supersedes plan scope isolation fix md v1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PLAN: Scope Isolation Fix — Pre-the user registration flow Prerequisite: branch claude vigorous margulis is present and needs its own handling; date 2026 04 07 is present and needs its own handling; supersedes plan scope isolation fix md v1 is present and needs its own handling; tests baseline 8 118 passing 0 failures is present and needs its own handling.

B: Create work pieces for branch claude vigorous margulis, date 2026 04 07, supersedes plan scope isolation fix md v1, tests baseline 8 118 passing 0 failures. Keep branch claude vigorous margulis and date 2026 04 07 separate. Allow supersedes plan scope isolation fix md v1 and tests baseline 8 118 passing 0 failures to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: branch claude vigorous margulis, date 2026 04 07, supersedes plan scope isolation fix md v1, tests baseline 8 118 passing 0 failures.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

