# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; use plans summaries from init json or list files is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, use plans summaries from init json or list files. Keep impact explanation and reprocess instruction separate. Allow approval boundary and use plans summaries from init json or list files to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, use plans summaries from init json or list files.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; use plans summaries from init json or list files is present and needs its own handling; config settings can be fetched via gsd tools config get if needed is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, use plans summaries from init json or list files, config settings can be fetched via gsd tools config get if needed. Keep reprocess instruction and approval boundary separate. Allow use plans summaries from init json or list files and config settings can be fetched via gsd tools config get if needed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, use plans summaries from init json or list files, config settings can be fetched via gsd tools config get if needed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: approval boundary is present and needs its own handling; use plans summaries from init json or list files is present and needs its own handling; config settings can be fetched via gsd tools config get if needed is present and needs its own handling; extract the second to last summary from the json result is present and needs its own handling.

B: Create work pieces for approval boundary, use plans summaries from init json or list files, config settings can be fetched via gsd tools config get if needed, extract the second to last summary from the json result. Keep approval boundary and use plans summaries from init json or list files separate. Allow config settings can be fetched via gsd tools config get if needed and extract the second to last summary from the json result to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, use plans summaries from init json or list files, config settings can be fetched via gsd tools config get if needed, extract the second to last summary from the json result.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: use plans summaries from init json or list files is present and needs its own handling; config settings can be fetched via gsd tools config get if needed is present and needs its own handling; extract the second to last summary from the json result is present and needs its own handling; authentication gates is present and needs its own handling.

B: Create work pieces for use plans summaries from init json or list files, config settings can be fetched via gsd tools config get if needed, extract the second to last summary from the json result, authentication gates. Keep use plans summaries from init json or list files and config settings can be fetched via gsd tools config get if needed separate. Allow extract the second to last summary from the json result and authentication gates to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: use plans summaries from init json or list files, config settings can be fetched via gsd tools config get if needed, extract the second to last summary from the json result, authentication gates.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Use plans/summaries from INIT JSON, or list files: config settings can be fetched via gsd tools config get if needed is present and needs its own handling; extract the second to last summary from the json result is present and needs its own handling; authentication gates is present and needs its own handling; deviation rules is present and needs its own handling.

B: Create work pieces for config settings can be fetched via gsd tools config get if needed, extract the second to last summary from the json result, authentication gates, deviation rules. Keep config settings can be fetched via gsd tools config get if needed and extract the second to last summary from the json result separate. Allow authentication gates and deviation rules to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: config settings can be fetched via gsd tools config get if needed, extract the second to last summary from the json result, authentication gates, deviation rules.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

