# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; abstraction extraction from flows to patterns is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, abstraction extraction from flows to patterns. Keep configuration choice and dependency rule separate. Allow completion gate and abstraction extraction from flows to patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, abstraction extraction from flows to patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; abstraction extraction from flows to patterns is present and needs its own handling; when to extract is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, abstraction extraction from flows to patterns, when to extract. Keep dependency rule and completion gate separate. Allow abstraction extraction from flows to patterns and when to extract to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, abstraction extraction from flows to patterns, when to extract.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: completion gate is present and needs its own handling; abstraction extraction from flows to patterns is present and needs its own handling; when to extract is present and needs its own handling; the 6 steps is present and needs its own handling.

B: Create work pieces for completion gate, abstraction extraction from flows to patterns, when to extract, the 6 steps. Keep completion gate and abstraction extraction from flows to patterns separate. Allow when to extract and the 6 steps to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, abstraction extraction from flows to patterns, when to extract, the 6 steps.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: abstraction extraction from flows to patterns is present and needs its own handling; when to extract is present and needs its own handling; the 6 steps is present and needs its own handling; step 1 lay flows side by side plain english only no code is present and needs its own handling.

B: Create work pieces for abstraction extraction from flows to patterns, when to extract, the 6 steps, step 1 lay flows side by side plain english only no code. Keep abstraction extraction from flows to patterns and when to extract separate. Allow the 6 steps and step 1 lay flows side by side plain english only no code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: abstraction extraction from flows to patterns, when to extract, the 6 steps, step 1 lay flows side by side plain english only no code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Abstraction Extraction — From Flows to Patterns: when to extract is present and needs its own handling; the 6 steps is present and needs its own handling; step 1 lay flows side by side plain english only no code is present and needs its own handling; step 2 draw fixed vs varies columns is present and needs its own handling.

B: Create work pieces for when to extract, the 6 steps, step 1 lay flows side by side plain english only no code, step 2 draw fixed vs varies columns. Keep when to extract and the 6 steps separate. Allow step 1 lay flows side by side plain english only no code and step 2 draw fixed vs varies columns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to extract, the 6 steps, step 1 lay flows side by side plain english only no code, step 2 draw fixed vs varies columns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

