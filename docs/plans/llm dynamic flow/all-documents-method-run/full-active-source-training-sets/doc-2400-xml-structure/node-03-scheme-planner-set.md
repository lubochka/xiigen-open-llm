# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; xml tag structure patterns is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, xml tag structure patterns. Keep configuration choice and dependency rule separate. Allow completion gate and xml tag structure patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, xml tag structure patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; xml tag structure patterns is present and needs its own handling; tag hierarchy best practices is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, xml tag structure patterns, tag hierarchy best practices. Keep dependency rule and completion gate separate. Allow xml tag structure patterns and tag hierarchy best practices to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, xml tag structure patterns, tag hierarchy best practices.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: completion gate is present and needs its own handling; xml tag structure patterns is present and needs its own handling; tag hierarchy best practices is present and needs its own handling; hierarchy levels is present and needs its own handling.

B: Create work pieces for completion gate, xml tag structure patterns, tag hierarchy best practices, hierarchy levels. Keep completion gate and xml tag structure patterns separate. Allow tag hierarchy best practices and hierarchy levels to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, xml tag structure patterns, tag hierarchy best practices, hierarchy levels.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: xml tag structure patterns is present and needs its own handling; tag hierarchy best practices is present and needs its own handling; hierarchy levels is present and needs its own handling; maximum nesting rule is present and needs its own handling.

B: Create work pieces for xml tag structure patterns, tag hierarchy best practices, hierarchy levels, maximum nesting rule. Keep xml tag structure patterns and tag hierarchy best practices separate. Allow hierarchy levels and maximum nesting rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xml tag structure patterns, tag hierarchy best practices, hierarchy levels, maximum nesting rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XML Tag Structure Patterns: tag hierarchy best practices is present and needs its own handling; hierarchy levels is present and needs its own handling; maximum nesting rule is present and needs its own handling; document wrapping patterns is present and needs its own handling.

B: Create work pieces for tag hierarchy best practices, hierarchy levels, maximum nesting rule, document wrapping patterns. Keep tag hierarchy best practices and hierarchy levels separate. Allow maximum nesting rule and document wrapping patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: tag hierarchy best practices, hierarchy levels, maximum nesting rule, document wrapping patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

