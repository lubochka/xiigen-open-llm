# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; find most recent phase directory with work is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, find most recent phase directory with work. Keep configuration choice and dependency rule separate. Allow completion gate and find most recent phase directory with work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, find most recent phase directory with work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; find most recent phase directory with work is present and needs its own handling; current position which phase which plan which task is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, find most recent phase directory with work, current position which phase which plan which task. Keep dependency rule and completion gate separate. Allow find most recent phase directory with work and current position which phase which plan which task to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, find most recent phase directory with work, current position which phase which plan which task.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: completion gate is present and needs its own handling; find most recent phase directory with work is present and needs its own handling; current position which phase which plan which task is present and needs its own handling; work completed what got done this session is present and needs its own handling.

B: Create work pieces for completion gate, find most recent phase directory with work, current position which phase which plan which task, work completed what got done this session. Keep completion gate and find most recent phase directory with work separate. Allow current position which phase which plan which task and work completed what got done this session to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, find most recent phase directory with work, current position which phase which plan which task, work completed what got done this session.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: find most recent phase directory with work is present and needs its own handling; current position which phase which plan which task is present and needs its own handling; work completed what got done this session is present and needs its own handling; work remaining what s left in current plan phase is present and needs its own handling.

B: Create work pieces for find most recent phase directory with work, current position which phase which plan which task, work completed what got done this session, work remaining what s left in current plan phase. Keep find most recent phase directory with work and current position which phase which plan which task separate. Allow work completed what got done this session and work remaining what s left in current plan phase to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: find most recent phase directory with work, current position which phase which plan which task, work completed what got done this session, work remaining what s left in current plan phase.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Find most recent phase directory with work: current position which phase which plan which task is present and needs its own handling; work completed what got done this session is present and needs its own handling; work remaining what s left in current plan phase is present and needs its own handling; decisions made key decisions and rationale is present and needs its own handling.

B: Create work pieces for current position which phase which plan which task, work completed what got done this session, work remaining what s left in current plan phase, decisions made key decisions and rationale. Keep current position which phase which plan which task and work completed what got done this session separate. Allow work remaining what s left in current plan phase and decisions made key decisions and rationale to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: current position which phase which plan which task, work completed what got done this session, work remaining what s left in current plan phase, decisions made key decisions and rationale.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

