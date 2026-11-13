# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; use item types for heterogeneous lists is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, use item types for heterogeneous lists. Keep configuration choice and dependency rule separate. Allow completion gate and use item types for heterogeneous lists to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, use item types for heterogeneous lists.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; use item types for heterogeneous lists is present and needs its own handling; no layout thrashing a header never recycles into an image cell is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, use item types for heterogeneous lists, no layout thrashing a header never recycles into an image cell. Keep dependency rule and completion gate separate. Allow use item types for heterogeneous lists and no layout thrashing a header never recycles into an image cell to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, use item types for heterogeneous lists, no layout thrashing a header never recycles into an image cell.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: completion gate is present and needs its own handling; use item types for heterogeneous lists is present and needs its own handling; no layout thrashing a header never recycles into an image cell is present and needs its own handling; type safety typescript can narrow the item type in each branch is present and needs its own handling.

B: Create work pieces for completion gate, use item types for heterogeneous lists, no layout thrashing a header never recycles into an image cell, type safety typescript can narrow the item type in each branch. Keep completion gate and use item types for heterogeneous lists separate. Allow no layout thrashing a header never recycles into an image cell and type safety typescript can narrow the item type in each branch to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, use item types for heterogeneous lists, no layout thrashing a header never recycles into an image cell, type safety typescript can narrow the item type in each branch.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: use item types for heterogeneous lists is present and needs its own handling; no layout thrashing a header never recycles into an image cell is present and needs its own handling; type safety typescript can narrow the item type in each branch is present and needs its own handling; better size estimation use with for is present and needs its own handling.

B: Create work pieces for use item types for heterogeneous lists, no layout thrashing a header never recycles into an image cell, type safety typescript can narrow the item type in each branch, better size estimation use with for. Keep use item types for heterogeneous lists and no layout thrashing a header never recycles into an image cell separate. Allow type safety typescript can narrow the item type in each branch and better size estimation use with for to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: use item types for heterogeneous lists, no layout thrashing a header never recycles into an image cell, type safety typescript can narrow the item type in each branch, better size estimation use with for.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Use Item Types for Heterogeneous Lists: no layout thrashing a header never recycles into an image cell is present and needs its own handling; type safety typescript can narrow the item type in each branch is present and needs its own handling; better size estimation use with for is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for no layout thrashing a header never recycles into an image cell, type safety typescript can narrow the item type in each branch, better size estimation use with for, training example. Keep no layout thrashing a header never recycles into an image cell and type safety typescript can narrow the item type in each branch separate. Allow better size estimation use with for and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: no layout thrashing a header never recycles into an image cell, type safety typescript can narrow the item type in each branch, better size estimation use with for, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

