# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the problem is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, the problem. Keep configuration choice and dependency rule separate. Allow completion gate and the problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, the problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the problem is present and needs its own handling; threshold protocol is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, the problem, threshold protocol. Keep dependency rule and completion gate separate. Allow the problem and threshold protocol to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, the problem, threshold protocol.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: completion gate is present and needs its own handling; the problem is present and needs its own handling; threshold protocol is present and needs its own handling; emergency stop protocol 25 is present and needs its own handling.

B: Create work pieces for completion gate, the problem, threshold protocol, emergency stop protocol 25. Keep completion gate and the problem separate. Allow threshold protocol and emergency stop protocol 25 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, the problem, threshold protocol, emergency stop protocol 25.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: the problem is present and needs its own handling; threshold protocol is present and needs its own handling; emergency stop protocol 25 is present and needs its own handling; wrap up mode rules 35 is present and needs its own handling.

B: Create work pieces for the problem, threshold protocol, emergency stop protocol 25, wrap up mode rules 35. Keep the problem and threshold protocol separate. Allow emergency stop protocol 25 and wrap up mode rules 35 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the problem, threshold protocol, emergency stop protocol 25, wrap up mode rules 35.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Context-overflow-SKILL.md v1.0 — Clean session end before context fills: threshold protocol is present and needs its own handling; emergency stop protocol 25 is present and needs its own handling; wrap up mode rules 35 is present and needs its own handling; session state before emergency stop is present and needs its own handling.

B: Create work pieces for threshold protocol, emergency stop protocol 25, wrap up mode rules 35, session state before emergency stop. Keep threshold protocol and emergency stop protocol 25 separate. Allow wrap up mode rules 35 and session state before emergency stop to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: threshold protocol, emergency stop protocol 25, wrap up mode rules 35, session state before emergency stop.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

