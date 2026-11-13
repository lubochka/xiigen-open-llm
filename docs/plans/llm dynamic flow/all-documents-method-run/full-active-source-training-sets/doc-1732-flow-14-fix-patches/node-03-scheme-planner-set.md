# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; date 2026 03 23 is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, date 2026 03 23. Keep configuration choice and dependency rule separate. Allow completion gate and date 2026 03 23 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, date 2026 03 23.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; date 2026 03 23 is present and needs its own handling; after applying re run flow completeness checker expect 33 33 is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, date 2026 03 23, after applying re run flow completeness checker expect 33 33. Keep dependency rule and completion gate separate. Allow date 2026 03 23 and after applying re run flow completeness checker expect 33 33 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, date 2026 03 23, after applying re run flow completeness checker expect 33 33.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: completion gate is present and needs its own handling; date 2026 03 23 is present and needs its own handling; after applying re run flow completeness checker expect 33 33 is present and needs its own handling; patch 1 replace three required fields block after line 431 is present and needs its own handling.

B: Create work pieces for completion gate, date 2026 03 23, after applying re run flow completeness checker expect 33 33, patch 1 replace three required fields block after line 431. Keep completion gate and date 2026 03 23 separate. Allow after applying re run flow completeness checker expect 33 33 and patch 1 replace three required fields block after line 431 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, date 2026 03 23, after applying re run flow completeness checker expect 33 33, patch 1 replace three required fields block after line 431.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: date 2026 03 23 is present and needs its own handling; after applying re run flow completeness checker expect 33 33 is present and needs its own handling; patch 1 replace three required fields block after line 431 is present and needs its own handling; delete is present and needs its own handling.

B: Create work pieces for date 2026 03 23, after applying re run flow completeness checker expect 33 33, patch 1 replace three required fields block after line 431, delete. Keep date 2026 03 23 and after applying re run flow completeness checker expect 33 33 separate. Allow patch 1 replace three required fields block after line 431 and delete to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: date 2026 03 23, after applying re run flow completeness checker expect 33 33, patch 1 replace three required fields block after line 431, delete.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for The notifications flow — FIX PATCHES Apply to the notifications flow-REFERENCE-PLAN-v2.md: after applying re run flow completeness checker expect 33 33 is present and needs its own handling; patch 1 replace three required fields block after line 431 is present and needs its own handling; delete is present and needs its own handling; replace with is present and needs its own handling.

B: Create work pieces for after applying re run flow completeness checker expect 33 33, patch 1 replace three required fields block after line 431, delete, replace with. Keep after applying re run flow completeness checker expect 33 33 and patch 1 replace three required fields block after line 431 separate. Allow delete and replace with to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: after applying re run flow completeness checker expect 33 33, patch 1 replace three required fields block after line 431, delete, replace with.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

