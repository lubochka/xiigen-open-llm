# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the core rule is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, the core rule. Keep configuration choice and dependency rule separate. Allow completion gate and the core rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, the core rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the core rule is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, the core rule, when to invoke. Keep dependency rule and completion gate separate. Allow the core rule and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, the core rule, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: completion gate is present and needs its own handling; the core rule is present and needs its own handling; when to invoke is present and needs its own handling; fix propagation protocol is present and needs its own handling.

B: Create work pieces for completion gate, the core rule, when to invoke, fix propagation protocol. Keep completion gate and the core rule separate. Allow when to invoke and fix propagation protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, the core rule, when to invoke, fix propagation protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: the core rule is present and needs its own handling; when to invoke is present and needs its own handling; fix propagation protocol is present and needs its own handling; re validate all callers after fix is present and needs its own handling.

B: Create work pieces for the core rule, when to invoke, fix propagation protocol, re validate all callers after fix. Keep the core rule and when to invoke separate. Allow fix propagation protocol and re validate all callers after fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the core rule, when to invoke, fix propagation protocol, re validate all callers after fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Retroactive-development-SKILL.md v1.0 — Bug in output → fix the algorithm, not the output: when to invoke is present and needs its own handling; fix propagation protocol is present and needs its own handling; re validate all callers after fix is present and needs its own handling; run tests for each affected subproject is present and needs its own handling.

B: Create work pieces for when to invoke, fix propagation protocol, re validate all callers after fix, run tests for each affected subproject. Keep when to invoke and fix propagation protocol separate. Allow re validate all callers after fix and run tests for each affected subproject to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to invoke, fix propagation protocol, re validate all callers after fix, run tests for each affected subproject.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

