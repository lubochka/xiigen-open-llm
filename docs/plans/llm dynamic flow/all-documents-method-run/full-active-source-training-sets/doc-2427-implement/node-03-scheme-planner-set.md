# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; implement skill 01 core interfaces microservicebase is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, implement skill 01 core interfaces microservicebase. Keep configuration choice and dependency rule separate. Allow completion gate and implement skill 01 core interfaces microservicebase to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, implement skill 01 core interfaces microservicebase.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; implement skill 01 core interfaces microservicebase is present and needs its own handling; prerequisites is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, implement skill 01 core interfaces microservicebase, prerequisites. Keep dependency rule and completion gate separate. Allow implement skill 01 core interfaces microservicebase and prerequisites to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, implement skill 01 core interfaces microservicebase, prerequisites.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: completion gate is present and needs its own handling; implement skill 01 core interfaces microservicebase is present and needs its own handling; prerequisites is present and needs its own handling; step 1 create project is present and needs its own handling.

B: Create work pieces for completion gate, implement skill 01 core interfaces microservicebase, prerequisites, step 1 create project. Keep completion gate and implement skill 01 core interfaces microservicebase separate. Allow prerequisites and step 1 create project to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, implement skill 01 core interfaces microservicebase, prerequisites, step 1 create project.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: implement skill 01 core interfaces microservicebase is present and needs its own handling; prerequisites is present and needs its own handling; step 1 create project is present and needs its own handling; step 2 create folder structure is present and needs its own handling.

B: Create work pieces for implement skill 01 core interfaces microservicebase, prerequisites, step 1 create project, step 2 create folder structure. Keep implement skill 01 core interfaces microservicebase and prerequisites separate. Allow step 1 create project and step 2 create folder structure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: implement skill 01 core interfaces microservicebase, prerequisites, step 1 create project, step 2 create folder structure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Implement Skill 01: Core Interfaces & MicroserviceBase: prerequisites is present and needs its own handling; step 1 create project is present and needs its own handling; step 2 create folder structure is present and needs its own handling; step 3 copy code from skill md is present and needs its own handling.

B: Create work pieces for prerequisites, step 1 create project, step 2 create folder structure, step 3 copy code from skill md. Keep prerequisites and step 1 create project separate. Allow step 2 create folder structure and step 3 copy code from skill md to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: prerequisites, step 1 create project, step 2 create folder structure, step 3 copy code from skill md.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

