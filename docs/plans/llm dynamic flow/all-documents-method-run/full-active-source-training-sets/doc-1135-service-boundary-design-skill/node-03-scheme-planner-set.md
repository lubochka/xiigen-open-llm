# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the principle translated from xiigen service boundary design is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, the principle translated from xiigen service boundary design. Keep configuration choice and dependency rule separate. Allow completion gate and the principle translated from xiigen service boundary design to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, the principle translated from xiigen service boundary design.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the principle translated from xiigen service boundary design is present and needs its own handling; the 4 boundary tests is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, the principle translated from xiigen service boundary design, the 4 boundary tests. Keep dependency rule and completion gate separate. Allow the principle translated from xiigen service boundary design and the 4 boundary tests to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, the principle translated from xiigen service boundary design, the 4 boundary tests.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: completion gate is present and needs its own handling; the principle translated from xiigen service boundary design is present and needs its own handling; the 4 boundary tests is present and needs its own handling; test 1 transaction boundary is present and needs its own handling.

B: Create work pieces for completion gate, the principle translated from xiigen service boundary design, the 4 boundary tests, test 1 transaction boundary. Keep completion gate and the principle translated from xiigen service boundary design separate. Allow the 4 boundary tests and test 1 transaction boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, the principle translated from xiigen service boundary design, the 4 boundary tests, test 1 transaction boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: the principle translated from xiigen service boundary design is present and needs its own handling; the 4 boundary tests is present and needs its own handling; test 1 transaction boundary is present and needs its own handling; test 2 lifecycle test is present and needs its own handling.

B: Create work pieces for the principle translated from xiigen service boundary design, the 4 boundary tests, test 1 transaction boundary, test 2 lifecycle test. Keep the principle translated from xiigen service boundary design and the 4 boundary tests separate. Allow test 1 transaction boundary and test 2 lifecycle test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the principle translated from xiigen service boundary design, the 4 boundary tests, test 1 transaction boundary, test 2 lifecycle test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Service-boundary-design-SKILL.md v1.0 — Right boundaries for algorithms and subprojects: the 4 boundary tests is present and needs its own handling; test 1 transaction boundary is present and needs its own handling; test 2 lifecycle test is present and needs its own handling; test 3 consumer test is present and needs its own handling.

B: Create work pieces for the 4 boundary tests, test 1 transaction boundary, test 2 lifecycle test, test 3 consumer test. Keep the 4 boundary tests and test 1 transaction boundary separate. Allow test 2 lifecycle test and test 3 consumer test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the 4 boundary tests, test 1 transaction boundary, test 2 lifecycle test, test 3 consumer test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

