# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; post mortem why all 4 fixes applied together broke cart is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, post mortem why all 4 fixes applied together broke cart. Keep wait and recovery and result merge separate. Allow review before output and post mortem why all 4 fixes applied together broke cart to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, post mortem why all 4 fixes applied together broke cart.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: result merge is present and needs its own handling; review before output is present and needs its own handling; post mortem why all 4 fixes applied together broke cart is present and needs its own handling; the cascade failure is present and needs its own handling.

B: Create work pieces for result merge, review before output, post mortem why all 4 fixes applied together broke cart, the cascade failure. Keep result merge and review before output separate. Allow post mortem why all 4 fixes applied together broke cart and the cascade failure to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, post mortem why all 4 fixes applied together broke cart, the cascade failure.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: review before output is present and needs its own handling; post mortem why all 4 fixes applied together broke cart is present and needs its own handling; the cascade failure is present and needs its own handling; what happened step by step is present and needs its own handling.

B: Create work pieces for review before output, post mortem why all 4 fixes applied together broke cart, the cascade failure, what happened step by step. Keep review before output and post mortem why all 4 fixes applied together broke cart separate. Allow the cascade failure and what happened step by step to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, post mortem why all 4 fixes applied together broke cart, the cascade failure, what happened step by step.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: post mortem why all 4 fixes applied together broke cart is present and needs its own handling; the cascade failure is present and needs its own handling; what happened step by step is present and needs its own handling; before fixes shopping cart item 1 stretch layoutmode none is present and needs its own handling.

B: Create work pieces for post mortem why all 4 fixes applied together broke cart, the cascade failure, what happened step by step, before fixes shopping cart item 1 stretch layoutmode none. Keep post mortem why all 4 fixes applied together broke cart and the cascade failure separate. Allow what happened step by step and before fixes shopping cart item 1 stretch layoutmode none to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: post mortem why all 4 fixes applied together broke cart, the cascade failure, what happened step by step, before fixes shopping cart item 1 stretch layoutmode none.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for POST-MORTEM: Why All 4 Fixes Applied Together Broke Cart: the cascade failure is present and needs its own handling; what happened step by step is present and needs its own handling; before fixes shopping cart item 1 stretch layoutmode none is present and needs its own handling; after all fixes shopping cart item 1 is present and needs its own handling.

B: Create work pieces for the cascade failure, what happened step by step, before fixes shopping cart item 1 stretch layoutmode none, after all fixes shopping cart item 1. Keep the cascade failure and what happened step by step separate. Allow before fixes shopping cart item 1 stretch layoutmode none and after all fixes shopping cart item 1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the cascade failure, what happened step by step, before fixes shopping cart item 1 stretch layoutmode none, after all fixes shopping cart item 1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

