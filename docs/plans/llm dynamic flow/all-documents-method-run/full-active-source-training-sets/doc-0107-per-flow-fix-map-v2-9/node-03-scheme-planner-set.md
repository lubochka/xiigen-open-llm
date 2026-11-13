# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; xiigen module separation per flow fix map is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, xiigen module separation per flow fix map. Keep configuration choice and dependency rule separate. Allow completion gate and xiigen module separation per flow fix map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, xiigen module separation per flow fix map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; xiigen module separation per flow fix map is present and needs its own handling; how to read this map is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, xiigen module separation per flow fix map, how to read this map. Keep dependency rule and completion gate separate. Allow xiigen module separation per flow fix map and how to read this map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, xiigen module separation per flow fix map, how to read this map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: completion gate is present and needs its own handling; xiigen module separation per flow fix map is present and needs its own handling; how to read this map is present and needs its own handling; req blocked which of the 5 requirements is currently blocked is present and needs its own handling.

B: Create work pieces for completion gate, xiigen module separation per flow fix map, how to read this map, req blocked which of the 5 requirements is currently blocked. Keep completion gate and xiigen module separation per flow fix map separate. Allow how to read this map and req blocked which of the 5 requirements is currently blocked to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, xiigen module separation per flow fix map, how to read this map, req blocked which of the 5 requirements is currently blocked.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: xiigen module separation per flow fix map is present and needs its own handling; how to read this map is present and needs its own handling; req blocked which of the 5 requirements is currently blocked is present and needs its own handling; gaps to apply ordered list execute top to bottom is present and needs its own handling.

B: Create work pieces for xiigen module separation per flow fix map, how to read this map, req blocked which of the 5 requirements is currently blocked, gaps to apply ordered list execute top to bottom. Keep xiigen module separation per flow fix map and how to read this map separate. Allow req blocked which of the 5 requirements is currently blocked and gaps to apply ordered list execute top to bottom to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen module separation per flow fix map, how to read this map, req blocked which of the 5 requirements is currently blocked, gaps to apply ordered list execute top to bottom.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen Module Separation — Per-Flow Fix Map: how to read this map is present and needs its own handling; req blocked which of the 5 requirements is currently blocked is present and needs its own handling; gaps to apply ordered list execute top to bottom is present and needs its own handling; flow 01 13 corrections from v1 2 is present and needs its own handling.

B: Create work pieces for how to read this map, req blocked which of the 5 requirements is currently blocked, gaps to apply ordered list execute top to bottom, flow 01 13 corrections from v1 2. Keep how to read this map and req blocked which of the 5 requirements is currently blocked separate. Allow gaps to apply ordered list execute top to bottom and flow 01 13 corrections from v1 2 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: how to read this map, req blocked which of the 5 requirements is currently blocked, gaps to apply ordered list execute top to bottom, flow 01 13 corrections from v1 2.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

