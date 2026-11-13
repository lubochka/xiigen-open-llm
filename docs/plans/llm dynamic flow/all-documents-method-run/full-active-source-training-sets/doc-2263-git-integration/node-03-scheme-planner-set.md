# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; project initialization brief roadmap together is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, project initialization brief roadmap together. Keep configuration choice and dependency rule separate. Allow completion gate and project initialization brief roadmap together to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, project initialization brief roadmap together.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; project initialization brief roadmap together is present and needs its own handling; task completion during plan execution is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, project initialization brief roadmap together, task completion during plan execution. Keep dependency rule and completion gate separate. Allow project initialization brief roadmap together and task completion during plan execution to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, project initialization brief roadmap together, task completion during plan execution.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: completion gate is present and needs its own handling; project initialization brief roadmap together is present and needs its own handling; task completion during plan execution is present and needs its own handling; standard task is present and needs its own handling.

B: Create work pieces for completion gate, project initialization brief roadmap together, task completion during plan execution, standard task. Keep completion gate and project initialization brief roadmap together separate. Allow task completion during plan execution and standard task to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, project initialization brief roadmap together, task completion during plan execution, standard task.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: project initialization brief roadmap together is present and needs its own handling; task completion during plan execution is present and needs its own handling; standard task is present and needs its own handling; tdd task red phase is present and needs its own handling.

B: Create work pieces for project initialization brief roadmap together, task completion during plan execution, standard task, tdd task red phase. Keep project initialization brief roadmap together and task completion during plan execution separate. Allow standard task and tdd task red phase to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: project initialization brief roadmap together, task completion during plan execution, standard task, tdd task red phase.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Project Initialization brief + roadmap together: task completion during plan execution is present and needs its own handling; standard task is present and needs its own handling; tdd task red phase is present and needs its own handling; tdd task green phase is present and needs its own handling.

B: Create work pieces for task completion during plan execution, standard task, tdd task red phase, tdd task green phase. Keep task completion during plan execution and standard task separate. Allow tdd task red phase and tdd task green phase to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: task completion during plan execution, standard task, tdd task red phase, tdd task green phase.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

