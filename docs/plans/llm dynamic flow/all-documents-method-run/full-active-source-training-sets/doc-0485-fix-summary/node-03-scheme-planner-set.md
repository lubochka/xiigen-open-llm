# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: screen structure is present and needs its own handling; component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling.

B: Create work pieces for screen structure, component meaning, user action, data binding. Keep screen structure and component meaning separate. Allow user action and data binding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: screen structure, component meaning, user action, data binding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling.

B: Create work pieces for component meaning, user action, data binding, system type. Keep component meaning and user action separate. Allow data binding and system type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: component meaning, user action, data binding, system type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling.

B: Create work pieces for user action, data binding, system type, gap completion. Keep user action and data binding separate. Allow system type and gap completion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: user action, data binding, system type, gap completion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling; z index layering bug fix summary is present and needs its own handling.

B: Create work pieces for data binding, system type, gap completion, z index layering bug fix summary. Keep data binding and system type separate. Allow gap completion and z index layering bug fix summary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: data binding, system type, gap completion, z index layering bug fix summary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: system type is present and needs its own handling; gap completion is present and needs its own handling; z index layering bug fix summary is present and needs its own handling; issue overview is present and needs its own handling.

B: Create work pieces for system type, gap completion, z index layering bug fix summary, issue overview. Keep system type and gap completion separate. Allow z index layering bug fix summary and issue overview to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: system type, gap completion, z index layering bug fix summary, issue overview.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: gap completion is present and needs its own handling; z index layering bug fix summary is present and needs its own handling; issue overview is present and needs its own handling; root cause is present and needs its own handling.

B: Create work pieces for gap completion, z index layering bug fix summary, issue overview, root cause. Keep gap completion and z index layering bug fix summary separate. Allow issue overview and root cause to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: gap completion, z index layering bug fix summary, issue overview, root cause.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: z index layering bug fix summary is present and needs its own handling; issue overview is present and needs its own handling; root cause is present and needs its own handling; the fix is present and needs its own handling.

B: Create work pieces for z index layering bug fix summary, issue overview, root cause, the fix. Keep z index layering bug fix summary and issue overview separate. Allow root cause and the fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: z index layering bug fix summary, issue overview, root cause, the fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Z-Index Layering Bug - Fix Summary: issue overview is present and needs its own handling; root cause is present and needs its own handling; the fix is present and needs its own handling; expected behavior after fix is present and needs its own handling.

B: Create work pieces for issue overview, root cause, the fix, expected behavior after fix. Keep issue overview and root cause separate. Allow the fix and expected behavior after fix to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: issue overview, root cause, the fix, expected behavior after fix.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

