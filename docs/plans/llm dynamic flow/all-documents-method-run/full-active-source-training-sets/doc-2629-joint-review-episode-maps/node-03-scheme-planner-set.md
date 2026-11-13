# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Joint Review: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Joint Review: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Joint Review: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Joint Review: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; joint review is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, joint review. Keep impact explanation and reprocess instruction separate. Allow approval boundary and joint review to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, joint review.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Joint Review: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; joint review is present and needs its own handling; episode map extended md sub episode map md is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, joint review, episode map extended md sub episode map md. Keep reprocess instruction and approval boundary separate. Allow joint review and episode map extended md sub episode map md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, joint review, episode map extended md sub episode map md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Joint Review: approval boundary is present and needs its own handling; joint review is present and needs its own handling; episode map extended md sub episode map md is present and needs its own handling; read together as the two layers of the same corpus is present and needs its own handling.

B: Create work pieces for approval boundary, joint review, episode map extended md sub episode map md, read together as the two layers of the same corpus. Keep approval boundary and joint review separate. Allow episode map extended md sub episode map md and read together as the two layers of the same corpus to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, joint review, episode map extended md sub episode map md, read together as the two layers of the same corpus.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Joint Review: joint review is present and needs its own handling; episode map extended md sub episode map md is present and needs its own handling; read together as the two layers of the same corpus is present and needs its own handling; how these two documents relate is present and needs its own handling.

B: Create work pieces for joint review, episode map extended md sub episode map md, read together as the two layers of the same corpus, how these two documents relate. Keep joint review and episode map extended md sub episode map md separate. Allow read together as the two layers of the same corpus and how these two documents relate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: joint review, episode map extended md sub episode map md, read together as the two layers of the same corpus, how these two documents relate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Joint Review: episode map extended md sub episode map md is present and needs its own handling; read together as the two layers of the same corpus is present and needs its own handling; how these two documents relate is present and needs its own handling; episode map extended carry forward from previous review is present and needs its own handling.

B: Create work pieces for episode map extended md sub episode map md, read together as the two layers of the same corpus, how these two documents relate, episode map extended carry forward from previous review. Keep episode map extended md sub episode map md and read together as the two layers of the same corpus separate. Allow how these two documents relate and episode map extended carry forward from previous review to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: episode map extended md sub episode map md, read together as the two layers of the same corpus, how these two documents relate, episode map extended carry forward from previous review.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

