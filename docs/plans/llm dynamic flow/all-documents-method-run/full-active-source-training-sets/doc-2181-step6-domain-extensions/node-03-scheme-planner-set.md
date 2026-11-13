# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; step 6 extend with domain solutions is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, step 6 extend with domain solutions. Keep configuration choice and dependency rule separate. Allow completion gate and step 6 extend with domain solutions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, step 6 extend with domain solutions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; step 6 extend with domain solutions is present and needs its own handling; what a domain extension is is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, step 6 extend with domain solutions, what a domain extension is. Keep dependency rule and completion gate separate. Allow step 6 extend with domain solutions and what a domain extension is to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, step 6 extend with domain solutions, what a domain extension is.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: completion gate is present and needs its own handling; step 6 extend with domain solutions is present and needs its own handling; what a domain extension is is present and needs its own handling; the domain extension test is present and needs its own handling.

B: Create work pieces for completion gate, step 6 extend with domain solutions, what a domain extension is, the domain extension test. Keep completion gate and step 6 extend with domain solutions separate. Allow what a domain extension is and the domain extension test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, step 6 extend with domain solutions, what a domain extension is, the domain extension test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: step 6 extend with domain solutions is present and needs its own handling; what a domain extension is is present and needs its own handling; the domain extension test is present and needs its own handling; common reasons core changes are needed is present and needs its own handling.

B: Create work pieces for step 6 extend with domain solutions, what a domain extension is, the domain extension test, common reasons core changes are needed. Keep step 6 extend with domain solutions and what a domain extension is separate. Allow the domain extension test and common reasons core changes are needed to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: step 6 extend with domain solutions, what a domain extension is, the domain extension test, common reasons core changes are needed.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Step 6 · Extend with Domain Solutions: what a domain extension is is present and needs its own handling; the domain extension test is present and needs its own handling; common reasons core changes are needed is present and needs its own handling; how to define a domain extension is present and needs its own handling.

B: Create work pieces for what a domain extension is, the domain extension test, common reasons core changes are needed, how to define a domain extension. Keep what a domain extension is and the domain extension test separate. Allow common reasons core changes are needed and how to define a domain extension to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what a domain extension is, the domain extension test, common reasons core changes are needed, how to define a domain extension.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

