# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 3 write the architecture plan is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, step 3 write the architecture plan. Keep impact explanation and reprocess instruction separate. Allow approval boundary and step 3 write the architecture plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, step 3 write the architecture plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 3 write the architecture plan is present and needs its own handling; why the plan must come before any skill is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, step 3 write the architecture plan, why the plan must come before any skill. Keep reprocess instruction and approval boundary separate. Allow step 3 write the architecture plan and why the plan must come before any skill to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, step 3 write the architecture plan, why the plan must come before any skill.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: approval boundary is present and needs its own handling; step 3 write the architecture plan is present and needs its own handling; why the plan must come before any skill is present and needs its own handling; the four sections every plan must have is present and needs its own handling.

B: Create work pieces for approval boundary, step 3 write the architecture plan, why the plan must come before any skill, the four sections every plan must have. Keep approval boundary and step 3 write the architecture plan separate. Allow why the plan must come before any skill and the four sections every plan must have to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, step 3 write the architecture plan, why the plan must come before any skill, the four sections every plan must have.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: step 3 write the architecture plan is present and needs its own handling; why the plan must come before any skill is present and needs its own handling; the four sections every plan must have is present and needs its own handling; section 1 service list is present and needs its own handling.

B: Create work pieces for step 3 write the architecture plan, why the plan must come before any skill, the four sections every plan must have, section 1 service list. Keep step 3 write the architecture plan and why the plan must come before any skill separate. Allow the four sections every plan must have and section 1 service list to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 3 write the architecture plan, why the plan must come before any skill, the four sections every plan must have, section 1 service list.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 3 · Write the Architecture Plan: why the plan must come before any skill is present and needs its own handling; the four sections every plan must have is present and needs its own handling; section 1 service list is present and needs its own handling; section 2 dependency tree is present and needs its own handling.

B: Create work pieces for why the plan must come before any skill, the four sections every plan must have, section 1 service list, section 2 dependency tree. Keep why the plan must come before any skill and the four sections every plan must have separate. Allow section 1 service list and section 2 dependency tree to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: why the plan must come before any skill, the four sections every plan must have, section 1 service list, section 2 dependency tree.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

