# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; look for what isn t there is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, look for what isn t there. Keep configuration choice and dependency rule separate. Allow completion gate and look for what isn t there to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, look for what isn t there.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; look for what isn t there is present and needs its own handling; the gap audit procedure is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, look for what isn t there, the gap audit procedure. Keep dependency rule and completion gate separate. Allow look for what isn t there and the gap audit procedure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, look for what isn t there, the gap audit procedure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: completion gate is present and needs its own handling; look for what isn t there is present and needs its own handling; the gap audit procedure is present and needs its own handling; step 1 state what the component needs to be correct is present and needs its own handling.

B: Create work pieces for completion gate, look for what isn t there, the gap audit procedure, step 1 state what the component needs to be correct. Keep completion gate and look for what isn t there separate. Allow the gap audit procedure and step 1 state what the component needs to be correct to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, look for what isn t there, the gap audit procedure, step 1 state what the component needs to be correct.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: look for what isn t there is present and needs its own handling; the gap audit procedure is present and needs its own handling; step 1 state what the component needs to be correct is present and needs its own handling; step 2 find the config payload object is present and needs its own handling.

B: Create work pieces for look for what isn t there, the gap audit procedure, step 1 state what the component needs to be correct, step 2 find the config payload object. Keep look for what isn t there and the gap audit procedure separate. Allow step 1 state what the component needs to be correct and step 2 find the config payload object to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: look for what isn t there, the gap audit procedure, step 1 state what the component needs to be correct, step 2 find the config payload object.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Look for What Isn't There: the gap audit procedure is present and needs its own handling; step 1 state what the component needs to be correct is present and needs its own handling; step 2 find the config payload object is present and needs its own handling; step 3 cross reference list against config is present and needs its own handling.

B: Create work pieces for the gap audit procedure, step 1 state what the component needs to be correct, step 2 find the config payload object, step 3 cross reference list against config. Keep the gap audit procedure and step 1 state what the component needs to be correct separate. Allow step 2 find the config payload object and step 3 cross reference list against config to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the gap audit procedure, step 1 state what the component needs to be correct, step 2 find the config payload object, step 3 cross reference list against config.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

