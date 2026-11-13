# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; previous code protocol copy adapt rewrite is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, previous code protocol copy adapt rewrite. Keep configuration choice and dependency rule separate. Allow completion gate and previous code protocol copy adapt rewrite to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, previous code protocol copy adapt rewrite.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; previous code protocol copy adapt rewrite is present and needs its own handling; the three categories is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, previous code protocol copy adapt rewrite, the three categories. Keep dependency rule and completion gate separate. Allow previous code protocol copy adapt rewrite and the three categories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, previous code protocol copy adapt rewrite, the three categories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: completion gate is present and needs its own handling; previous code protocol copy adapt rewrite is present and needs its own handling; the three categories is present and needs its own handling; file inventory do this first every time is present and needs its own handling.

B: Create work pieces for completion gate, previous code protocol copy adapt rewrite, the three categories, file inventory do this first every time. Keep completion gate and previous code protocol copy adapt rewrite separate. Allow the three categories and file inventory do this first every time to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, previous code protocol copy adapt rewrite, the three categories, file inventory do this first every time.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: previous code protocol copy adapt rewrite is present and needs its own handling; the three categories is present and needs its own handling; file inventory do this first every time is present and needs its own handling; reading existing codebases is present and needs its own handling.

B: Create work pieces for previous code protocol copy adapt rewrite, the three categories, file inventory do this first every time, reading existing codebases. Keep previous code protocol copy adapt rewrite and the three categories separate. Allow file inventory do this first every time and reading existing codebases to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: previous code protocol copy adapt rewrite, the three categories, file inventory do this first every time, reading existing codebases.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Previous Code Protocol — COPY / ADAPT / REWRITE: the three categories is present and needs its own handling; file inventory do this first every time is present and needs its own handling; reading existing codebases is present and needs its own handling; read to find patterns not to copy code is present and needs its own handling.

B: Create work pieces for the three categories, file inventory do this first every time, reading existing codebases, read to find patterns not to copy code. Keep the three categories and file inventory do this first every time separate. Allow reading existing codebases and read to find patterns not to copy code to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the three categories, file inventory do this first every time, reading existing codebases, read to find patterns not to copy code.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

