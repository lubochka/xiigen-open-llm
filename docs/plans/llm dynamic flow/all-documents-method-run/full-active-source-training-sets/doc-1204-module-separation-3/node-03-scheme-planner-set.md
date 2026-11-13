# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 1 confirm viewerrole is exported from the sdk is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, step 1 confirm viewerrole is exported from the sdk. Keep impact explanation and reprocess instruction separate. Allow approval boundary and step 1 confirm viewerrole is exported from the sdk to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, step 1 confirm viewerrole is exported from the sdk.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 1 confirm viewerrole is exported from the sdk is present and needs its own handling; step 0 find where viewerrole actually lives right now is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, step 1 confirm viewerrole is exported from the sdk, step 0 find where viewerrole actually lives right now. Keep reprocess instruction and approval boundary separate. Allow step 1 confirm viewerrole is exported from the sdk and step 0 find where viewerrole actually lives right now to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, step 1 confirm viewerrole is exported from the sdk, step 0 find where viewerrole actually lives right now.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: approval boundary is present and needs its own handling; step 1 confirm viewerrole is exported from the sdk is present and needs its own handling; step 0 find where viewerrole actually lives right now is present and needs its own handling; document verdict location required action is present and needs its own handling.

B: Create work pieces for approval boundary, step 1 confirm viewerrole is exported from the sdk, step 0 find where viewerrole actually lives right now, document verdict location required action. Keep approval boundary and step 1 confirm viewerrole is exported from the sdk separate. Allow step 0 find where viewerrole actually lives right now and document verdict location required action to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, step 1 confirm viewerrole is exported from the sdk, step 0 find where viewerrole actually lives right now, document verdict location required action.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: step 1 confirm viewerrole is exported from the sdk is present and needs its own handling; step 0 find where viewerrole actually lives right now is present and needs its own handling; document verdict location required action is present and needs its own handling; docker compose platform yml runs alongside your dev environment is present and needs its own handling.

B: Create work pieces for step 1 confirm viewerrole is exported from the sdk, step 0 find where viewerrole actually lives right now, document verdict location required action, docker compose platform yml runs alongside your dev environment. Keep step 1 confirm viewerrole is exported from the sdk and step 0 find where viewerrole actually lives right now separate. Allow document verdict location required action and docker compose platform yml runs alongside your dev environment to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 1 confirm viewerrole is exported from the sdk, step 0 find where viewerrole actually lives right now, document verdict location required action, docker compose platform yml runs alongside your dev environment.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 1: Confirm ViewerRole is exported from the sdk: step 0 find where viewerrole actually lives right now is present and needs its own handling; document verdict location required action is present and needs its own handling; docker compose platform yml runs alongside your dev environment is present and needs its own handling; verdict finding is present and needs its own handling.

B: Create work pieces for step 0 find where viewerrole actually lives right now, document verdict location required action, docker compose platform yml runs alongside your dev environment, verdict finding. Keep step 0 find where viewerrole actually lives right now and document verdict location required action separate. Allow docker compose platform yml runs alongside your dev environment and verdict finding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: step 0 find where viewerrole actually lives right now, document verdict location required action, docker compose platform yml runs alongside your dev environment, verdict finding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

