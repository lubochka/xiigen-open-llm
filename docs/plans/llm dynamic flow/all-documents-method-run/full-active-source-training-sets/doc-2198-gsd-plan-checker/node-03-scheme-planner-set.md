# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plans contradict user decisions from context md is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, plans contradict user decisions from context md. Keep impact explanation and reprocess instruction separate. Allow approval boundary and plans contradict user decisions from context md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, plans contradict user decisions from context md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; plans contradict user decisions from context md is present and needs its own handling; dimension 1 requirement coverage is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, plans contradict user decisions from context md, dimension 1 requirement coverage. Keep reprocess instruction and approval boundary separate. Allow plans contradict user decisions from context md and dimension 1 requirement coverage to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, plans contradict user decisions from context md, dimension 1 requirement coverage.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: approval boundary is present and needs its own handling; plans contradict user decisions from context md is present and needs its own handling; dimension 1 requirement coverage is present and needs its own handling; dimension 2 task completeness is present and needs its own handling.

B: Create work pieces for approval boundary, plans contradict user decisions from context md, dimension 1 requirement coverage, dimension 2 task completeness. Keep approval boundary and plans contradict user decisions from context md separate. Allow dimension 1 requirement coverage and dimension 2 task completeness to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, plans contradict user decisions from context md, dimension 1 requirement coverage, dimension 2 task completeness.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: plans contradict user decisions from context md is present and needs its own handling; dimension 1 requirement coverage is present and needs its own handling; dimension 2 task completeness is present and needs its own handling; dimension 3 dependency correctness is present and needs its own handling.

B: Create work pieces for plans contradict user decisions from context md, dimension 1 requirement coverage, dimension 2 task completeness, dimension 3 dependency correctness. Keep plans contradict user decisions from context md and dimension 1 requirement coverage separate. Allow dimension 2 task completeness and dimension 3 dependency correctness to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: plans contradict user decisions from context md, dimension 1 requirement coverage, dimension 2 task completeness, dimension 3 dependency correctness.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Plans contradict user decisions from CONTEXT.md: dimension 1 requirement coverage is present and needs its own handling; dimension 2 task completeness is present and needs its own handling; dimension 3 dependency correctness is present and needs its own handling; dimension 4 key links planned is present and needs its own handling.

B: Create work pieces for dimension 1 requirement coverage, dimension 2 task completeness, dimension 3 dependency correctness, dimension 4 key links planned. Keep dimension 1 requirement coverage and dimension 2 task completeness separate. Allow dimension 3 dependency correctness and dimension 4 key links planned to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: dimension 1 requirement coverage, dimension 2 task completeness, dimension 3 dependency correctness, dimension 4 key links planned.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

