# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; dimension 1 copywriting is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, dimension 1 copywriting. Keep impact explanation and reprocess instruction separate. Allow approval boundary and dimension 1 copywriting to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, dimension 1 copywriting.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; dimension 1 copywriting is present and needs its own handling; dimension 2 visuals is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, dimension 1 copywriting, dimension 2 visuals. Keep reprocess instruction and approval boundary separate. Allow dimension 1 copywriting and dimension 2 visuals to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, dimension 1 copywriting, dimension 2 visuals.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: approval boundary is present and needs its own handling; dimension 1 copywriting is present and needs its own handling; dimension 2 visuals is present and needs its own handling; dimension 3 color is present and needs its own handling.

B: Create work pieces for approval boundary, dimension 1 copywriting, dimension 2 visuals, dimension 3 color. Keep approval boundary and dimension 1 copywriting separate. Allow dimension 2 visuals and dimension 3 color to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, dimension 1 copywriting, dimension 2 visuals, dimension 3 color.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: dimension 1 copywriting is present and needs its own handling; dimension 2 visuals is present and needs its own handling; dimension 3 color is present and needs its own handling; dimension 4 typography is present and needs its own handling.

B: Create work pieces for dimension 1 copywriting, dimension 2 visuals, dimension 3 color, dimension 4 typography. Keep dimension 1 copywriting and dimension 2 visuals separate. Allow dimension 3 color and dimension 4 typography to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: dimension 1 copywriting, dimension 2 visuals, dimension 3 color, dimension 4 typography.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Dimension 1: Copywriting: dimension 2 visuals is present and needs its own handling; dimension 3 color is present and needs its own handling; dimension 4 typography is present and needs its own handling; dimension 5 spacing is present and needs its own handling.

B: Create work pieces for dimension 2 visuals, dimension 3 color, dimension 4 typography, dimension 5 spacing. Keep dimension 2 visuals and dimension 3 color separate. Allow dimension 4 typography and dimension 5 spacing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: dimension 2 visuals, dimension 3 color, dimension 4 typography, dimension 5 spacing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

