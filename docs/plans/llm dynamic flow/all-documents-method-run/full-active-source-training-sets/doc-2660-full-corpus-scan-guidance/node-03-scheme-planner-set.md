# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; how to scan the full knowledge base is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, how to scan the full knowledge base. Keep impact explanation and reprocess instruction separate. Allow approval boundary and how to scan the full knowledge base to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, how to scan the full knowledge base.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; how to scan the full knowledge base is present and needs its own handling; four ways this scan goes wrong know them before starting is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, how to scan the full knowledge base, four ways this scan goes wrong know them before starting. Keep reprocess instruction and approval boundary separate. Allow how to scan the full knowledge base and four ways this scan goes wrong know them before starting to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, how to scan the full knowledge base, four ways this scan goes wrong know them before starting.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: approval boundary is present and needs its own handling; how to scan the full knowledge base is present and needs its own handling; four ways this scan goes wrong know them before starting is present and needs its own handling; the scan loop apply this to every directory every time is present and needs its own handling.

B: Create work pieces for approval boundary, how to scan the full knowledge base, four ways this scan goes wrong know them before starting, the scan loop apply this to every directory every time. Keep approval boundary and how to scan the full knowledge base separate. Allow four ways this scan goes wrong know them before starting and the scan loop apply this to every directory every time to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, how to scan the full knowledge base, four ways this scan goes wrong know them before starting, the scan loop apply this to every directory every time.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: how to scan the full knowledge base is present and needs its own handling; four ways this scan goes wrong know them before starting is present and needs its own handling; the scan loop apply this to every directory every time is present and needs its own handling; step 1 count everything before reading anything is present and needs its own handling.

B: Create work pieces for how to scan the full knowledge base, four ways this scan goes wrong know them before starting, the scan loop apply this to every directory every time, step 1 count everything before reading anything. Keep how to scan the full knowledge base and four ways this scan goes wrong know them before starting separate. Allow the scan loop apply this to every directory every time and step 1 count everything before reading anything to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: how to scan the full knowledge base, four ways this scan goes wrong know them before starting, the scan loop apply this to every directory every time, step 1 count everything before reading anything.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for How to Scan the Full Knowledge Base: four ways this scan goes wrong know them before starting is present and needs its own handling; the scan loop apply this to every directory every time is present and needs its own handling; step 1 count everything before reading anything is present and needs its own handling; step 2 sort files into five groups from names only before reading is present and needs its own handling.

B: Create work pieces for four ways this scan goes wrong know them before starting, the scan loop apply this to every directory every time, step 1 count everything before reading anything, step 2 sort files into five groups from names only before reading. Keep four ways this scan goes wrong know them before starting and the scan loop apply this to every directory every time separate. Allow step 1 count everything before reading anything and step 2 sort files into five groups from names only before reading to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: four ways this scan goes wrong know them before starting, the scan loop apply this to every directory every time, step 1 count everything before reading anything, step 2 sort files into five groups from names only before reading.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

