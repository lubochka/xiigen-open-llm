# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the complete examination plan is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, the complete examination plan. Keep configuration choice and dependency rule separate. Allow completion gate and the complete examination plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, the complete examination plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; the complete examination plan is present and needs its own handling; purpose is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, the complete examination plan, purpose. Keep dependency rule and completion gate separate. Allow the complete examination plan and purpose to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, the complete examination plan, purpose.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: completion gate is present and needs its own handling; the complete examination plan is present and needs its own handling; purpose is present and needs its own handling; the goal being tested is present and needs its own handling.

B: Create work pieces for completion gate, the complete examination plan, purpose, the goal being tested. Keep completion gate and the complete examination plan separate. Allow purpose and the goal being tested to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, the complete examination plan, purpose, the goal being tested.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: the complete examination plan is present and needs its own handling; purpose is present and needs its own handling; the goal being tested is present and needs its own handling; the six hypotheses is present and needs its own handling.

B: Create work pieces for the complete examination plan, purpose, the goal being tested, the six hypotheses. Keep the complete examination plan and purpose separate. Allow the goal being tested and the six hypotheses to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the complete examination plan, purpose, the goal being tested, the six hypotheses.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The Complete Examination Plan: purpose is present and needs its own handling; the goal being tested is present and needs its own handling; the six hypotheses is present and needs its own handling; the examination protocol five steps applied to every flow is present and needs its own handling.

B: Create work pieces for purpose, the goal being tested, the six hypotheses, the examination protocol five steps applied to every flow. Keep purpose and the goal being tested separate. Allow the six hypotheses and the examination protocol five steps applied to every flow to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: purpose, the goal being tested, the six hypotheses, the examination protocol five steps applied to every flow.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

