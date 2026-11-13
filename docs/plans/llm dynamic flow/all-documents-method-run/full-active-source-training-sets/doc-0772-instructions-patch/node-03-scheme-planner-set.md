# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; patch add to claude instructions md is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, patch add to claude instructions md. Keep configuration choice and dependency rule separate. Allow completion gate and patch add to claude instructions md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, patch add to claude instructions md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; patch add to claude instructions md is present and needs its own handling; insert in the session start protocol section is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, patch add to claude instructions md, insert in the session start protocol section. Keep dependency rule and completion gate separate. Allow patch add to claude instructions md and insert in the session start protocol section to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, patch add to claude instructions md, insert in the session start protocol section.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: completion gate is present and needs its own handling; patch add to claude instructions md is present and needs its own handling; insert in the session start protocol section is present and needs its own handling; level 1 unit test is present and needs its own handling.

B: Create work pieces for completion gate, patch add to claude instructions md, insert in the session start protocol section, level 1 unit test. Keep completion gate and patch add to claude instructions md separate. Allow insert in the session start protocol section and level 1 unit test to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, patch add to claude instructions md, insert in the session start protocol section, level 1 unit test.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: patch add to claude instructions md is present and needs its own handling; insert in the session start protocol section is present and needs its own handling; level 1 unit test is present and needs its own handling; level 2 scene replay run after every change is present and needs its own handling.

B: Create work pieces for patch add to claude instructions md, insert in the session start protocol section, level 1 unit test, level 2 scene replay run after every change. Keep patch add to claude instructions md and insert in the session start protocol section separate. Allow level 1 unit test and level 2 scene replay run after every change to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: patch add to claude instructions md, insert in the session start protocol section, level 1 unit test, level 2 scene replay run after every change.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PATCH: Add to .claude/instructions.md: insert in the session start protocol section is present and needs its own handling; level 1 unit test is present and needs its own handling; level 2 scene replay run after every change is present and needs its own handling; level 3 replay bug test add for every fix is present and needs its own handling.

B: Create work pieces for insert in the session start protocol section, level 1 unit test, level 2 scene replay run after every change, level 3 replay bug test add for every fix. Keep insert in the session start protocol section and level 1 unit test separate. Allow level 2 scene replay run after every change and level 3 replay bug test add for every fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: insert in the session start protocol section, level 1 unit test, level 2 scene replay run after every change, level 3 replay bug test add for every fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

