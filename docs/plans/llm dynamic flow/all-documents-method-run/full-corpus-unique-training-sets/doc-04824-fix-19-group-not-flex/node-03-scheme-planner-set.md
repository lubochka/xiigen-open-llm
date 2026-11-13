# Node-03 - Scheme Planner Set

Runtime A is all translated domain statements. Runtime B is the separated work plan.

## Pair 3.1 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: trigger and input is present; node responsibility is present; parallel branch is present; wait state is present.

B: Create one work piece for each of trigger and input, node responsibility, parallel branch, wait state. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.1 - Rephrased

A: The translated responsibilities are trigger and input, node responsibility, parallel branch, wait state.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.2 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: node responsibility is present; parallel branch is present; wait state is present; recovery path is present.

B: Create one work piece for each of node responsibility, parallel branch, wait state, recovery path. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.2 - Rephrased

A: The translated responsibilities are node responsibility, parallel branch, wait state, recovery path.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.3 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: parallel branch is present; wait state is present; recovery path is present; result merge is present.

B: Create one work piece for each of parallel branch, wait state, recovery path, result merge. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.3 - Rephrased

A: The translated responsibilities are parallel branch, wait state, recovery path, result merge.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.4 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: wait state is present; recovery path is present; result merge is present; review point is present.

B: Create one work piece for each of wait state, recovery path, result merge, review point. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.4 - Rephrased

A: The translated responsibilities are wait state, recovery path, result merge, review point.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.5 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: recovery path is present; result merge is present; review point is present; formatted output is present.

B: Create one work piece for each of recovery path, result merge, review point, formatted output. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.5 - Rephrased

A: The translated responsibilities are recovery path, result merge, review point, formatted output.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.6 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: result merge is present; review point is present; formatted output is present; fix 19: group nodes are not flex containers is present.

B: Create one work piece for each of result merge, review point, formatted output, fix 19: group nodes are not flex containers. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.6 - Rephrased

A: The translated responsibilities are result merge, review point, formatted output, fix 19: group nodes are not flex containers.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.7 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: review point is present; formatted output is present; fix 19: group nodes are not flex containers is present; status: ready to apply — supersedes all previous group-related fixes is present.

B: Create one work piece for each of review point, formatted output, fix 19: group nodes are not flex containers, status: ready to apply — supersedes all previous group-related fixes. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.7 - Rephrased

A: The translated responsibilities are review point, formatted output, fix 19: group nodes are not flex containers, status: ready to apply — supersedes all previous group-related fixes.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.8 - Plan Work Pieces

A: Translated statements for fix 19: GROUP Nodes Are NOT Flex Containers: formatted output is present; fix 19: group nodes are not flex containers is present; status: ready to apply — supersedes all previous group-related fixes is present; the failure chain proven by debug traces is present.

B: Create one work piece for each of formatted output, fix 19: group nodes are not flex containers, status: ready to apply — supersedes all previous group-related fixes, the failure chain proven by debug traces. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.8 - Rephrased

A: The translated responsibilities are formatted output, fix 19: group nodes are not flex containers, status: ready to apply — supersedes all previous group-related fixes, the failure chain proven by debug traces.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

