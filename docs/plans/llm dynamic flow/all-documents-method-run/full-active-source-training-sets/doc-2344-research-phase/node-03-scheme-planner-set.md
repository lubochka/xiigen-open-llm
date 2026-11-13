# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 0 resolve model profile is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, step 0 resolve model profile. Keep impact explanation and reprocess instruction separate. Allow approval boundary and step 0 resolve model profile to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, step 0 resolve model profile.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; step 0 resolve model profile is present and needs its own handling; step 1 normalize and validate phase is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, step 0 resolve model profile, step 1 normalize and validate phase. Keep reprocess instruction and approval boundary separate. Allow step 0 resolve model profile and step 1 normalize and validate phase to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, step 0 resolve model profile, step 1 normalize and validate phase.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: approval boundary is present and needs its own handling; step 0 resolve model profile is present and needs its own handling; step 1 normalize and validate phase is present and needs its own handling; step 2 check existing research is present and needs its own handling.

B: Create work pieces for approval boundary, step 0 resolve model profile, step 1 normalize and validate phase, step 2 check existing research. Keep approval boundary and step 0 resolve model profile separate. Allow step 1 normalize and validate phase and step 2 check existing research to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, step 0 resolve model profile, step 1 normalize and validate phase, step 2 check existing research.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: step 0 resolve model profile is present and needs its own handling; step 1 normalize and validate phase is present and needs its own handling; step 2 check existing research is present and needs its own handling; step 3 gather phase context is present and needs its own handling.

B: Create work pieces for step 0 resolve model profile, step 1 normalize and validate phase, step 2 check existing research, step 3 gather phase context. Keep step 0 resolve model profile and step 1 normalize and validate phase separate. Allow step 2 check existing research and step 3 gather phase context to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 0 resolve model profile, step 1 normalize and validate phase, step 2 check existing research, step 3 gather phase context.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 0: Resolve Model Profile: step 1 normalize and validate phase is present and needs its own handling; step 2 check existing research is present and needs its own handling; step 3 gather phase context is present and needs its own handling; step 4 spawn researcher is present and needs its own handling.

B: Create work pieces for step 1 normalize and validate phase, step 2 check existing research, step 3 gather phase context, step 4 spawn researcher. Keep step 1 normalize and validate phase and step 2 check existing research separate. Allow step 3 gather phase context and step 4 spawn researcher to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: step 1 normalize and validate phase, step 2 check existing research, step 3 gather phase context, step 4 spawn researcher.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

