# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; runtime candidates runtime config dir is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, runtime candidates runtime config dir. Keep configuration choice and dependency rule separate. Allow completion gate and runtime candidates runtime config dir to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, runtime candidates runtime config dir.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": dependency rule is present and needs its own handling; completion gate is present and needs its own handling; runtime candidates runtime config dir is present and needs its own handling; if not set infer from runtime env vars fallback to claude is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, runtime candidates runtime config dir, if not set infer from runtime env vars fallback to claude. Keep dependency rule and completion gate separate. Allow runtime candidates runtime config dir and if not set infer from runtime env vars fallback to claude to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, runtime candidates runtime config dir, if not set infer from runtime env vars fallback to claude.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": completion gate is present and needs its own handling; runtime candidates runtime config dir is present and needs its own handling; if not set infer from runtime env vars fallback to claude is present and needs its own handling; reorder entries so preferred runtime is checked first is present and needs its own handling.

B: Create work pieces for completion gate, runtime candidates runtime config dir, if not set infer from runtime env vars fallback to claude, reorder entries so preferred runtime is checked first. Keep completion gate and runtime candidates runtime config dir separate. Allow if not set infer from runtime env vars fallback to claude and reorder entries so preferred runtime is checked first to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, runtime candidates runtime config dir, if not set infer from runtime env vars fallback to claude, reorder entries so preferred runtime is checked first.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": runtime candidates runtime config dir is present and needs its own handling; if not set infer from runtime env vars fallback to claude is present and needs its own handling; reorder entries so preferred runtime is checked first is present and needs its own handling; gsd update is present and needs its own handling.

B: Create work pieces for runtime candidates runtime config dir, if not set infer from runtime env vars fallback to claude, reorder entries so preferred runtime is checked first, gsd update. Keep runtime candidates runtime config dir and if not set infer from runtime env vars fallback to claude separate. Allow reorder entries so preferred runtime is checked first and gsd update to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: runtime candidates runtime config dir, if not set infer from runtime env vars fallback to claude, reorder entries so preferred runtime is checked first, gsd update.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Runtime candidates: "<runtime :<config-dir ": if not set infer from runtime env vars fallback to claude is present and needs its own handling; reorder entries so preferred runtime is checked first is present and needs its own handling; gsd update is present and needs its own handling; gsd update available is present and needs its own handling.

B: Create work pieces for if not set infer from runtime env vars fallback to claude, reorder entries so preferred runtime is checked first, gsd update, gsd update available. Keep if not set infer from runtime env vars fallback to claude and reorder entries so preferred runtime is checked first separate. Allow gsd update and gsd update available to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: if not set infer from runtime env vars fallback to claude, reorder entries so preferred runtime is checked first, gsd update, gsd update available.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

