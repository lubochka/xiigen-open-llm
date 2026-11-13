# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; step 1 detect backed up patches is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, step 1 detect backed up patches. Keep configuration choice and dependency rule separate. Allow completion gate and step 1 detect backed up patches to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, step 1 detect backed up patches.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; step 1 detect backed up patches is present and needs its own handling; global install detect runtime config directory is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, step 1 detect backed up patches, global install detect runtime config directory. Keep dependency rule and completion gate separate. Allow step 1 detect backed up patches and global install detect runtime config directory to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, step 1 detect backed up patches, global install detect runtime config directory.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: completion gate is present and needs its own handling; step 1 detect backed up patches is present and needs its own handling; global install detect runtime config directory is present and needs its own handling; local install fallback check all runtime directories is present and needs its own handling.

B: Create work pieces for completion gate, step 1 detect backed up patches, global install detect runtime config directory, local install fallback check all runtime directories. Keep completion gate and step 1 detect backed up patches separate. Allow global install detect runtime config directory and local install fallback check all runtime directories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, step 1 detect backed up patches, global install detect runtime config directory, local install fallback check all runtime directories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: step 1 detect backed up patches is present and needs its own handling; global install detect runtime config directory is present and needs its own handling; local install fallback check all runtime directories is present and needs its own handling; step 2 show patch summary is present and needs its own handling.

B: Create work pieces for step 1 detect backed up patches, global install detect runtime config directory, local install fallback check all runtime directories, step 2 show patch summary. Keep step 1 detect backed up patches and global install detect runtime config directory separate. Allow local install fallback check all runtime directories and step 2 show patch summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 1 detect backed up patches, global install detect runtime config directory, local install fallback check all runtime directories, step 2 show patch summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 1: Detect backed-up patches: global install detect runtime config directory is present and needs its own handling; local install fallback check all runtime directories is present and needs its own handling; step 2 show patch summary is present and needs its own handling; local patches to reapply is present and needs its own handling.

B: Create work pieces for global install detect runtime config directory, local install fallback check all runtime directories, step 2 show patch summary, local patches to reapply. Keep global install detect runtime config directory and local install fallback check all runtime directories separate. Allow step 2 show patch summary and local patches to reapply to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: global install detect runtime config directory, local install fallback check all runtime directories, step 2 show patch summary, local patches to reapply.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

