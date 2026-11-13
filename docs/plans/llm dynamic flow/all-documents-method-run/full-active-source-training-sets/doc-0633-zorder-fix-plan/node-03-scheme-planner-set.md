# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: screen structure is present and needs its own handling; component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling.

B: Create work pieces for screen structure, component meaning, user action, data binding. Keep screen structure and component meaning separate. Allow user action and data binding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: screen structure, component meaning, user action, data binding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling.

B: Create work pieces for component meaning, user action, data binding, system type. Keep component meaning and user action separate. Allow data binding and system type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: component meaning, user action, data binding, system type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling.

B: Create work pieces for user action, data binding, system type, gap completion. Keep user action and data binding separate. Allow system type and gap completion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: user action, data binding, system type, gap completion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling; z order fix plan is present and needs its own handling.

B: Create work pieces for data binding, system type, gap completion, z order fix plan. Keep data binding and system type separate. Allow gap completion and z order fix plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: data binding, system type, gap completion, z order fix plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: system type is present and needs its own handling; gap completion is present and needs its own handling; z order fix plan is present and needs its own handling; file changes surgical is present and needs its own handling.

B: Create work pieces for system type, gap completion, z order fix plan, file changes surgical. Keep system type and gap completion separate. Allow z order fix plan and file changes surgical to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: system type, gap completion, z order fix plan, file changes surgical.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: gap completion is present and needs its own handling; z order fix plan is present and needs its own handling; file changes surgical is present and needs its own handling; root cause is present and needs its own handling.

B: Create work pieces for gap completion, z order fix plan, file changes surgical, root cause. Keep gap completion and z order fix plan separate. Allow file changes surgical and root cause to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: gap completion, z order fix plan, file changes surgical, root cause.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: z order fix plan is present and needs its own handling; file changes surgical is present and needs its own handling; root cause is present and needs its own handling; change 1 add is present and needs its own handling.

B: Create work pieces for z order fix plan, file changes surgical, root cause, change 1 add. Keep z order fix plan and file changes surgical separate. Allow root cause and change 1 add to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: z order fix plan, file changes surgical, root cause, change 1 add.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Z-Order Fix Plan: file changes surgical is present and needs its own handling; root cause is present and needs its own handling; change 1 add is present and needs its own handling; change 2 implement is present and needs its own handling.

B: Create work pieces for file changes surgical, root cause, change 1 add, change 2 implement. Keep file changes surgical and root cause separate. Allow change 1 add and change 2 implement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: file changes surgical, root cause, change 1 add, change 2 implement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

