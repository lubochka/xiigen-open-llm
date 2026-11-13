# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: screen structure is present and needs its own handling; component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling.

B: Create work pieces for screen structure, component meaning, user action, data binding. Keep screen structure and component meaning separate. Allow user action and data binding to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: screen structure, component meaning, user action, data binding.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: component meaning is present and needs its own handling; user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling.

B: Create work pieces for component meaning, user action, data binding, system type. Keep component meaning and user action separate. Allow data binding and system type to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: component meaning, user action, data binding, system type.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: user action is present and needs its own handling; data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling.

B: Create work pieces for user action, data binding, system type, gap completion. Keep user action and data binding separate. Allow system type and gap completion to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: user action, data binding, system type, gap completion.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: data binding is present and needs its own handling; system type is present and needs its own handling; gap completion is present and needs its own handling; contact scene analysis fix 22c needs refinement is present and needs its own handling.

B: Create work pieces for data binding, system type, gap completion, contact scene analysis fix 22c needs refinement. Keep data binding and system type separate. Allow gap completion and contact scene analysis fix 22c needs refinement to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: data binding, system type, gap completion, contact scene analysis fix 22c needs refinement.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: system type is present and needs its own handling; gap completion is present and needs its own handling; contact scene analysis fix 22c needs refinement is present and needs its own handling; the discovery is present and needs its own handling.

B: Create work pieces for system type, gap completion, contact scene analysis fix 22c needs refinement, the discovery. Keep system type and gap completion separate. Allow contact scene analysis fix 22c needs refinement and the discovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: system type, gap completion, contact scene analysis fix 22c needs refinement, the discovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: gap completion is present and needs its own handling; contact scene analysis fix 22c needs refinement is present and needs its own handling; the discovery is present and needs its own handling; why group coordinates are different is present and needs its own handling.

B: Create work pieces for gap completion, contact scene analysis fix 22c needs refinement, the discovery, why group coordinates are different. Keep gap completion and contact scene analysis fix 22c needs refinement separate. Allow the discovery and why group coordinates are different to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: gap completion, contact scene analysis fix 22c needs refinement, the discovery, why group coordinates are different.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: contact scene analysis fix 22c needs refinement is present and needs its own handling; the discovery is present and needs its own handling; why group coordinates are different is present and needs its own handling; the conflict with cart is present and needs its own handling.

B: Create work pieces for contact scene analysis fix 22c needs refinement, the discovery, why group coordinates are different, the conflict with cart. Keep contact scene analysis fix 22c needs refinement and the discovery separate. Allow why group coordinates are different and the conflict with cart to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: contact scene analysis fix 22c needs refinement, the discovery, why group coordinates are different, the conflict with cart.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Contact Scene Analysis — fix 22c Needs Refinement: the discovery is present and needs its own handling; why group coordinates are different is present and needs its own handling; the conflict with cart is present and needs its own handling; the heuristic check if coords exceed parent dimensions is present and needs its own handling.

B: Create work pieces for the discovery, why group coordinates are different, the conflict with cart, the heuristic check if coords exceed parent dimensions. Keep the discovery and why group coordinates are different separate. Allow the conflict with cart and the heuristic check if coords exceed parent dimensions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the discovery, why group coordinates are different, the conflict with cart, the heuristic check if coords exceed parent dimensions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

