# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; moderation service implementation prompt skill 44 is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, moderation service implementation prompt skill 44. Keep impact explanation and reprocess instruction separate. Allow approval boundary and moderation service implementation prompt skill 44 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, moderation service implementation prompt skill 44.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; moderation service implementation prompt skill 44 is present and needs its own handling; phase 1 core moderation is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, moderation service implementation prompt skill 44, phase 1 core moderation. Keep reprocess instruction and approval boundary separate. Allow moderation service implementation prompt skill 44 and phase 1 core moderation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, moderation service implementation prompt skill 44, phase 1 core moderation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: approval boundary is present and needs its own handling; moderation service implementation prompt skill 44 is present and needs its own handling; phase 1 core moderation is present and needs its own handling; phase 2 category scoring is present and needs its own handling.

B: Create work pieces for approval boundary, moderation service implementation prompt skill 44, phase 1 core moderation, phase 2 category scoring. Keep approval boundary and moderation service implementation prompt skill 44 separate. Allow phase 1 core moderation and phase 2 category scoring to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, moderation service implementation prompt skill 44, phase 1 core moderation, phase 2 category scoring.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: moderation service implementation prompt skill 44 is present and needs its own handling; phase 1 core moderation is present and needs its own handling; phase 2 category scoring is present and needs its own handling; phase 3 human review queue is present and needs its own handling.

B: Create work pieces for moderation service implementation prompt skill 44, phase 1 core moderation, phase 2 category scoring, phase 3 human review queue. Keep moderation service implementation prompt skill 44 and phase 1 core moderation separate. Allow phase 2 category scoring and phase 3 human review queue to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: moderation service implementation prompt skill 44, phase 1 core moderation, phase 2 category scoring, phase 3 human review queue.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Moderation Service — Implementation Prompt Skill 44: phase 1 core moderation is present and needs its own handling; phase 2 category scoring is present and needs its own handling; phase 3 human review queue is present and needs its own handling; phase 4 integration testing is present and needs its own handling.

B: Create work pieces for phase 1 core moderation, phase 2 category scoring, phase 3 human review queue, phase 4 integration testing. Keep phase 1 core moderation and phase 2 category scoring separate. Allow phase 3 human review queue and phase 4 integration testing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: phase 1 core moderation, phase 2 category scoring, phase 3 human review queue, phase 4 integration testing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

