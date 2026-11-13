# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; gsd orchestration skill is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, gsd orchestration skill. Keep impact explanation and reprocess instruction separate. Allow approval boundary and gsd orchestration skill to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, gsd orchestration skill.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; gsd orchestration skill is present and needs its own handling; what is gsd is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, gsd orchestration skill, what is gsd. Keep reprocess instruction and approval boundary separate. Allow gsd orchestration skill and what is gsd to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, gsd orchestration skill, what is gsd.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: approval boundary is present and needs its own handling; gsd orchestration skill is present and needs its own handling; what is gsd is present and needs its own handling; the problem is present and needs its own handling.

B: Create work pieces for approval boundary, gsd orchestration skill, what is gsd, the problem. Keep approval boundary and gsd orchestration skill separate. Allow what is gsd and the problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, gsd orchestration skill, what is gsd, the problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: gsd orchestration skill is present and needs its own handling; what is gsd is present and needs its own handling; the problem is present and needs its own handling; hour 1 perfect code thorough testing is present and needs its own handling.

B: Create work pieces for gsd orchestration skill, what is gsd, the problem, hour 1 perfect code thorough testing. Keep gsd orchestration skill and what is gsd separate. Allow the problem and hour 1 perfect code thorough testing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: gsd orchestration skill, what is gsd, the problem, hour 1 perfect code thorough testing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for GSD Orchestration Skill: what is gsd is present and needs its own handling; the problem is present and needs its own handling; hour 1 perfect code thorough testing is present and needs its own handling; hour 2 starting to cut corners is present and needs its own handling.

B: Create work pieces for what is gsd, the problem, hour 1 perfect code thorough testing, hour 2 starting to cut corners. Keep what is gsd and the problem separate. Allow hour 1 perfect code thorough testing and hour 2 starting to cut corners to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what is gsd, the problem, hour 1 perfect code thorough testing, hour 2 starting to cut corners.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

