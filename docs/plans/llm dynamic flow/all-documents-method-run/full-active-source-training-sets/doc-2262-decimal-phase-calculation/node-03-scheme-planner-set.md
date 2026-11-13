# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; decimal phase calculation is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, decimal phase calculation. Keep configuration choice and dependency rule separate. Allow completion gate and decimal phase calculation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, decimal phase calculation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; decimal phase calculation is present and needs its own handling; using gsd tools is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, decimal phase calculation, using gsd tools. Keep dependency rule and completion gate separate. Allow decimal phase calculation and using gsd tools to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, decimal phase calculation, using gsd tools.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: completion gate is present and needs its own handling; decimal phase calculation is present and needs its own handling; using gsd tools is present and needs its own handling; get next decimal phase after phase 6 is present and needs its own handling.

B: Create work pieces for completion gate, decimal phase calculation, using gsd tools, get next decimal phase after phase 6. Keep completion gate and decimal phase calculation separate. Allow using gsd tools and get next decimal phase after phase 6 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, decimal phase calculation, using gsd tools, get next decimal phase after phase 6.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: decimal phase calculation is present and needs its own handling; using gsd tools is present and needs its own handling; get next decimal phase after phase 6 is present and needs its own handling; extract values is present and needs its own handling.

B: Create work pieces for decimal phase calculation, using gsd tools, get next decimal phase after phase 6, extract values. Keep decimal phase calculation and using gsd tools separate. Allow get next decimal phase after phase 6 and extract values to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: decimal phase calculation, using gsd tools, get next decimal phase after phase 6, extract values.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Decimal Phase Calculation: using gsd tools is present and needs its own handling; get next decimal phase after phase 6 is present and needs its own handling; extract values is present and needs its own handling; returns just 06 1 is present and needs its own handling.

B: Create work pieces for using gsd tools, get next decimal phase after phase 6, extract values, returns just 06 1. Keep using gsd tools and get next decimal phase after phase 6 separate. Allow extract values and returns just 06 1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: using gsd tools, get next decimal phase after phase 6, extract values, returns just 06 1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

