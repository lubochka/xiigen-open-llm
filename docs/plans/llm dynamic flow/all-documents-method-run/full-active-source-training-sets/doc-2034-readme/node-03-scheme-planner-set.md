# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; gsd example todo the programming interface project is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, gsd example todo the programming interface project. Keep configuration choice and dependency rule separate. Allow completion gate and gsd example todo the programming interface project to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, gsd example todo the programming interface project.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; gsd example todo the programming interface project is present and needs its own handling; what this example shows is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, gsd example todo the programming interface project, what this example shows. Keep dependency rule and completion gate separate. Allow gsd example todo the programming interface project and what this example shows to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, gsd example todo the programming interface project, what this example shows.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: completion gate is present and needs its own handling; gsd example todo the programming interface project is present and needs its own handling; what this example shows is present and needs its own handling; complete gsd structure all required files and organization is present and needs its own handling.

B: Create work pieces for completion gate, gsd example todo the programming interface project, what this example shows, complete gsd structure all required files and organization. Keep completion gate and gsd example todo the programming interface project separate. Allow what this example shows and complete gsd structure all required files and organization to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, gsd example todo the programming interface project, what this example shows, complete gsd structure all required files and organization.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: gsd example todo the programming interface project is present and needs its own handling; what this example shows is present and needs its own handling; complete gsd structure all required files and organization is present and needs its own handling; phase based development how phases are structured is present and needs its own handling.

B: Create work pieces for gsd example todo the programming interface project, what this example shows, complete gsd structure all required files and organization, phase based development how phases are structured. Keep gsd example todo the programming interface project and what this example shows separate. Allow complete gsd structure all required files and organization and phase based development how phases are structured to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: gsd example todo the programming interface project, what this example shows, complete gsd structure all required files and organization, phase based development how phases are structured.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for GSD Example: Todo the programming interface Project: what this example shows is present and needs its own handling; complete gsd structure all required files and organization is present and needs its own handling; phase based development how phases are structured is present and needs its own handling; xml task templates how plan files are formatted is present and needs its own handling.

B: Create work pieces for what this example shows, complete gsd structure all required files and organization, phase based development how phases are structured, xml task templates how plan files are formatted. Keep what this example shows and complete gsd structure all required files and organization separate. Allow phase based development how phases are structured and xml task templates how plan files are formatted to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: what this example shows, complete gsd structure all required files and organization, phase based development how phases are structured, xml task templates how plan files are formatted.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

