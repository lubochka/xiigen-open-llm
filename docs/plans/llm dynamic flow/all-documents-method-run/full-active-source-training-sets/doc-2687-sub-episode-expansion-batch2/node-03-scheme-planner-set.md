# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; individual episode detail sections groups 01 19 22 34 is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, individual episode detail sections groups 01 19 22 34. Keep configuration choice and dependency rule separate. Allow completion gate and individual episode detail sections groups 01 19 22 34 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, individual episode detail sections groups 01 19 22 34.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; individual episode detail sections groups 01 19 22 34 is present and needs its own handling; one section per individual episode from episode map extended md is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, individual episode detail sections groups 01 19 22 34, one section per individual episode from episode map extended md. Keep dependency rule and completion gate separate. Allow individual episode detail sections groups 01 19 22 34 and one section per individual episode from episode map extended md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, individual episode detail sections groups 01 19 22 34, one section per individual episode from episode map extended md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: completion gate is present and needs its own handling; individual episode detail sections groups 01 19 22 34 is present and needs its own handling; one section per individual episode from episode map extended md is present and needs its own handling; version evidence is present and needs its own handling.

B: Create work pieces for completion gate, individual episode detail sections groups 01 19 22 34, one section per individual episode from episode map extended md, version evidence. Keep completion gate and individual episode detail sections groups 01 19 22 34 separate. Allow one section per individual episode from episode map extended md and version evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, individual episode detail sections groups 01 19 22 34, one section per individual episode from episode map extended md, version evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: individual episode detail sections groups 01 19 22 34 is present and needs its own handling; one section per individual episode from episode map extended md is present and needs its own handling; version evidence is present and needs its own handling; edge condition is present and needs its own handling.

B: Create work pieces for individual episode detail sections groups 01 19 22 34, one section per individual episode from episode map extended md, version evidence, edge condition. Keep individual episode detail sections groups 01 19 22 34 and one section per individual episode from episode map extended md separate. Allow version evidence and edge condition to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: individual episode detail sections groups 01 19 22 34, one section per individual episode from episode map extended md, version evidence, edge condition.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for INDIVIDUAL EPISODE DETAIL SECTIONS — GROUPS 01-19, 22-34: one section per individual episode from episode map extended md is present and needs its own handling; version evidence is present and needs its own handling; edge condition is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for one section per individual episode from episode map extended md, version evidence, edge condition, training example. Keep one section per individual episode from episode map extended md and version evidence separate. Allow edge condition and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: one section per individual episode from episode map extended md, version evidence, edge condition, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

