# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; css engine merged fix plan canonical is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, css engine merged fix plan canonical. Keep wait and recovery and result merge separate. Allow review before output and css engine merged fix plan canonical to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, css engine merged fix plan canonical.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: result merge is present and needs its own handling; review before output is present and needs its own handling; css engine merged fix plan canonical is present and needs its own handling; march 13 2026 v4 1 is present and needs its own handling.

B: Create work pieces for result merge, review before output, css engine merged fix plan canonical, march 13 2026 v4 1. Keep result merge and review before output separate. Allow css engine merged fix plan canonical and march 13 2026 v4 1 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, css engine merged fix plan canonical, march 13 2026 v4 1.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: review before output is present and needs its own handling; css engine merged fix plan canonical is present and needs its own handling; march 13 2026 v4 1 is present and needs its own handling; part 1 baseline is present and needs its own handling.

B: Create work pieces for review before output, css engine merged fix plan canonical, march 13 2026 v4 1, part 1 baseline. Keep review before output and css engine merged fix plan canonical separate. Allow march 13 2026 v4 1 and part 1 baseline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, css engine merged fix plan canonical, march 13 2026 v4 1, part 1 baseline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: css engine merged fix plan canonical is present and needs its own handling; march 13 2026 v4 1 is present and needs its own handling; part 1 baseline is present and needs its own handling; part 2 two independent bugs is present and needs its own handling.

B: Create work pieces for css engine merged fix plan canonical, march 13 2026 v4 1, part 1 baseline, part 2 two independent bugs. Keep css engine merged fix plan canonical and march 13 2026 v4 1 separate. Allow part 1 baseline and part 2 two independent bugs to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: css engine merged fix plan canonical, march 13 2026 v4 1, part 1 baseline, part 2 two independent bugs.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for CSS ENGINE — MERGED FIX PLAN Canonical: march 13 2026 v4 1 is present and needs its own handling; part 1 baseline is present and needs its own handling; part 2 two independent bugs is present and needs its own handling; bug a vertical branch missing flex true 39 nodes is present and needs its own handling.

B: Create work pieces for march 13 2026 v4 1, part 1 baseline, part 2 two independent bugs, bug a vertical branch missing flex true 39 nodes. Keep march 13 2026 v4 1 and part 1 baseline separate. Allow part 2 two independent bugs and bug a vertical branch missing flex true 39 nodes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: march 13 2026 v4 1, part 1 baseline, part 2 two independent bugs, bug a vertical branch missing flex true 39 nodes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

