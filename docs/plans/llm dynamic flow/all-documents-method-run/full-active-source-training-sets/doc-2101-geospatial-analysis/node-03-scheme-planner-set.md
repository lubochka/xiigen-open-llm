# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; geospatial analysis reference is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, geospatial analysis reference. Keep configuration choice and dependency rule separate. Allow completion gate and geospatial analysis reference to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, geospatial analysis reference.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; geospatial analysis reference is present and needs its own handling; table of contents is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, geospatial analysis reference, table of contents. Keep dependency rule and completion gate separate. Allow geospatial analysis reference and table of contents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, geospatial analysis reference, table of contents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: completion gate is present and needs its own handling; geospatial analysis reference is present and needs its own handling; table of contents is present and needs its own handling; geospatial fundamentals is present and needs its own handling.

B: Create work pieces for completion gate, geospatial analysis reference, table of contents, geospatial fundamentals. Keep completion gate and geospatial analysis reference separate. Allow table of contents and geospatial fundamentals to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, geospatial analysis reference, table of contents, geospatial fundamentals.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: geospatial analysis reference is present and needs its own handling; table of contents is present and needs its own handling; geospatial fundamentals is present and needs its own handling; coordinate systems is present and needs its own handling.

B: Create work pieces for geospatial analysis reference, table of contents, geospatial fundamentals, coordinate systems. Keep geospatial analysis reference and table of contents separate. Allow geospatial fundamentals and coordinate systems to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: geospatial analysis reference, table of contents, geospatial fundamentals, coordinate systems.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Geospatial Analysis Reference: table of contents is present and needs its own handling; geospatial fundamentals is present and needs its own handling; coordinate systems is present and needs its own handling; python geospatial libraries is present and needs its own handling.

B: Create work pieces for table of contents, geospatial fundamentals, coordinate systems, python geospatial libraries. Keep table of contents and geospatial fundamentals separate. Allow coordinate systems and python geospatial libraries to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: table of contents, geospatial fundamentals, coordinate systems, python geospatial libraries.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

