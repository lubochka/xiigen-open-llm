# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: test objective is present and needs its own handling; scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling.

B: Create work pieces for test objective, scenario setup, expected behavior, failure evidence. Keep test objective and scenario setup separate. Allow expected behavior and failure evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: test objective, scenario setup, expected behavior, failure evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: scenario setup is present and needs its own handling; expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling.

B: Create work pieces for scenario setup, expected behavior, failure evidence, coverage decision. Keep scenario setup and expected behavior separate. Allow failure evidence and coverage decision to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: scenario setup, expected behavior, failure evidence, coverage decision.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: expected behavior is present and needs its own handling; failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling.

B: Create work pieces for expected behavior, failure evidence, coverage decision, release gate. Keep expected behavior and failure evidence separate. Allow coverage decision and release gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: expected behavior, failure evidence, coverage decision, release gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: failure evidence is present and needs its own handling; coverage decision is present and needs its own handling; release gate is present and needs its own handling; scene driven testing is present and needs its own handling.

B: Create work pieces for failure evidence, coverage decision, release gate, scene driven testing. Keep failure evidence and coverage decision separate. Allow release gate and scene driven testing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: failure evidence, coverage decision, release gate, scene driven testing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: coverage decision is present and needs its own handling; release gate is present and needs its own handling; scene driven testing is present and needs its own handling; the problem is present and needs its own handling.

B: Create work pieces for coverage decision, release gate, scene driven testing, the problem. Keep coverage decision and release gate separate. Allow scene driven testing and the problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: coverage decision, release gate, scene driven testing, the problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: release gate is present and needs its own handling; scene driven testing is present and needs its own handling; the problem is present and needs its own handling; the solution record replay verify is present and needs its own handling.

B: Create work pieces for release gate, scene driven testing, the problem, the solution record replay verify. Keep release gate and scene driven testing separate. Allow the problem and the solution record replay verify to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: release gate, scene driven testing, the problem, the solution record replay verify.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: scene driven testing is present and needs its own handling; the problem is present and needs its own handling; the solution record replay verify is present and needs its own handling; step 1 record capture scene input data during plugin execution is present and needs its own handling.

B: Create work pieces for scene driven testing, the problem, the solution record replay verify, step 1 record capture scene input data during plugin execution. Keep scene driven testing and the problem separate. Allow the solution record replay verify and step 1 record capture scene input data during plugin execution to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: scene driven testing, the problem, the solution record replay verify, step 1 record capture scene input data during plugin execution.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Scene-Driven Testing: the problem is present and needs its own handling; the solution record replay verify is present and needs its own handling; step 1 record capture scene input data during plugin execution is present and needs its own handling; step 2 store save scene fixtures from debug output is present and needs its own handling.

B: Create work pieces for the problem, the solution record replay verify, step 1 record capture scene input data during plugin execution, step 2 store save scene fixtures from debug output. Keep the problem and the solution record replay verify separate. Allow step 1 record capture scene input data during plugin execution and step 2 store save scene fixtures from debug output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the problem, the solution record replay verify, step 1 record capture scene input data during plugin execution, step 2 store save scene fixtures from debug output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

