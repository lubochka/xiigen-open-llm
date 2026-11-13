# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; verification patterns is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, verification patterns. Keep configuration choice and dependency rule separate. Allow completion gate and verification patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, verification patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; verification patterns is present and needs its own handling; exists file is present at expected path is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, verification patterns, exists file is present at expected path. Keep dependency rule and completion gate separate. Allow verification patterns and exists file is present at expected path to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, verification patterns, exists file is present at expected path.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: completion gate is present and needs its own handling; verification patterns is present and needs its own handling; exists file is present at expected path is present and needs its own handling; substantive content is real implementation not placeholder is present and needs its own handling.

B: Create work pieces for completion gate, verification patterns, exists file is present at expected path, substantive content is real implementation not placeholder. Keep completion gate and verification patterns separate. Allow exists file is present at expected path and substantive content is real implementation not placeholder to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, verification patterns, exists file is present at expected path, substantive content is real implementation not placeholder.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: verification patterns is present and needs its own handling; exists file is present at expected path is present and needs its own handling; substantive content is real implementation not placeholder is present and needs its own handling; wired connected to the rest of the system is present and needs its own handling.

B: Create work pieces for verification patterns, exists file is present at expected path, substantive content is real implementation not placeholder, wired connected to the rest of the system. Keep verification patterns and exists file is present at expected path separate. Allow substantive content is real implementation not placeholder and wired connected to the rest of the system to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: verification patterns, exists file is present at expected path, substantive content is real implementation not placeholder, wired connected to the rest of the system.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Verification Patterns: exists file is present at expected path is present and needs its own handling; substantive content is real implementation not placeholder is present and needs its own handling; wired connected to the rest of the system is present and needs its own handling; functional actually works when invoked is present and needs its own handling.

B: Create work pieces for exists file is present at expected path, substantive content is real implementation not placeholder, wired connected to the rest of the system, functional actually works when invoked. Keep exists file is present at expected path and substantive content is real implementation not placeholder separate. Allow wired connected to the rest of the system and functional actually works when invoked to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: exists file is present at expected path, substantive content is real implementation not placeholder, wired connected to the rest of the system, functional actually works when invoked.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

