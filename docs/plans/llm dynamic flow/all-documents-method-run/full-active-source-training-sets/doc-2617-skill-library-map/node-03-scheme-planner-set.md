# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; xiigen skill library map is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, xiigen skill library map. Keep configuration choice and dependency rule separate. Allow completion gate and xiigen skill library map to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, xiigen skill library map.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; xiigen skill library map is present and needs its own handling; skill ranges is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, xiigen skill library map, skill ranges. Keep dependency rule and completion gate separate. Allow xiigen skill library map and skill ranges to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, xiigen skill library map, skill ranges.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: completion gate is present and needs its own handling; xiigen skill library map is present and needs its own handling; skill ranges is present and needs its own handling; skills 01 10 core infrastructure is present and needs its own handling.

B: Create work pieces for completion gate, xiigen skill library map, skill ranges, skills 01 10 core infrastructure. Keep completion gate and xiigen skill library map separate. Allow skill ranges and skills 01 10 core infrastructure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, xiigen skill library map, skill ranges, skills 01 10 core infrastructure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: xiigen skill library map is present and needs its own handling; skill ranges is present and needs its own handling; skills 01 10 core infrastructure is present and needs its own handling; skills 11 20 processing ai is present and needs its own handling.

B: Create work pieces for xiigen skill library map, skill ranges, skills 01 10 core infrastructure, skills 11 20 processing ai. Keep xiigen skill library map and skill ranges separate. Allow skills 01 10 core infrastructure and skills 11 20 processing ai to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen skill library map, skill ranges, skills 01 10 core infrastructure, skills 11 20 processing ai.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen Skill Library Map: skill ranges is present and needs its own handling; skills 01 10 core infrastructure is present and needs its own handling; skills 11 20 processing ai is present and needs its own handling; skills 21 30 code generation is present and needs its own handling.

B: Create work pieces for skill ranges, skills 01 10 core infrastructure, skills 11 20 processing ai, skills 21 30 code generation. Keep skill ranges and skills 01 10 core infrastructure separate. Allow skills 11 20 processing ai and skills 21 30 code generation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: skill ranges, skills 01 10 core infrastructure, skills 11 20 processing ai, skills 21 30 code generation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

