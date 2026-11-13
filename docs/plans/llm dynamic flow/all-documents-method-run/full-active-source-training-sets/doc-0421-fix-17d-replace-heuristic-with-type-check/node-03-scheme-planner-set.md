# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: screen structure is present and needs its own handling; component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling.

B: Create work pieces for screen structure, component meaning, user action, data binding. Keep screen structure and component meaning separate. Allow user action and data binding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: screen structure, component meaning, user action, data binding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling.

B: Create work pieces for component meaning, user action, data binding, system type. Keep component meaning and user action separate. Allow data binding and system type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: component meaning, user action, data binding, system type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling.

B: Create work pieces for user action, data binding, system type, gap completion. Keep user action and data binding separate. Allow system type and gap completion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: user action, data binding, system type, gap completion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling; the real fix replace bounds heuristic with parent type check is present and needs its own handling.

B: Create work pieces for data binding, system type, gap completion, the real fix replace bounds heuristic with parent type check. Keep data binding and system type separate. Allow gap completion and the real fix replace bounds heuristic with parent type check to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: data binding, system type, gap completion, the real fix replace bounds heuristic with parent type check.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: system type is present and needs its own handling; gap completion is present and needs its own handling; the real fix replace bounds heuristic with parent type check is present and needs its own handling; why with 1 5 threshold is wrong is present and needs its own handling.

B: Create work pieces for system type, gap completion, the real fix replace bounds heuristic with parent type check, why with 1 5 threshold is wrong. Keep system type and gap completion separate. Allow the real fix replace bounds heuristic with parent type check and why with 1 5 threshold is wrong to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: system type, gap completion, the real fix replace bounds heuristic with parent type check, why with 1 5 threshold is wrong.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: gap completion is present and needs its own handling; the real fix replace bounds heuristic with parent type check is present and needs its own handling; why with 1 5 threshold is wrong is present and needs its own handling; the problem with the current heuristic is present and needs its own handling.

B: Create work pieces for gap completion, the real fix replace bounds heuristic with parent type check, why with 1 5 threshold is wrong, the problem with the current heuristic. Keep gap completion and the real fix replace bounds heuristic with parent type check separate. Allow why with 1 5 threshold is wrong and the problem with the current heuristic to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: gap completion, the real fix replace bounds heuristic with parent type check, why with 1 5 threshold is wrong, the problem with the current heuristic.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: the real fix replace bounds heuristic with parent type check is present and needs its own handling; why with 1 5 threshold is wrong is present and needs its own handling; the problem with the current heuristic is present and needs its own handling; why no heuristic can work is present and needs its own handling.

B: Create work pieces for the real fix replace bounds heuristic with parent type check, why with 1 5 threshold is wrong, the problem with the current heuristic, why no heuristic can work. Keep the real fix replace bounds heuristic with parent type check and why with 1 5 threshold is wrong separate. Allow the problem with the current heuristic and why no heuristic can work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the real fix replace bounds heuristic with parent type check, why with 1 5 threshold is wrong, the problem with the current heuristic, why no heuristic can work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for THE REAL FIX: Replace Bounds Heuristic With Parent Type Check: why with 1 5 threshold is wrong is present and needs its own handling; the problem with the current heuristic is present and needs its own handling; why no heuristic can work is present and needs its own handling; the correct fix check parent type is present and needs its own handling.

B: Create work pieces for why with 1 5 threshold is wrong, the problem with the current heuristic, why no heuristic can work, the correct fix check parent type. Keep why with 1 5 threshold is wrong and the problem with the current heuristic separate. Allow why no heuristic can work and the correct fix check parent type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: why with 1 5 threshold is wrong, the problem with the current heuristic, why no heuristic can work, the correct fix check parent type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

