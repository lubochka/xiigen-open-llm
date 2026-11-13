# Node-07 - Arbiter Selector Set

Runtime A is the formal result collection. Runtime B is the required review units.

## Pair 7.1 - Select Review

A: Formal results for Cleanup Summary include trigger and input, node responsibility, parallel branch.

B: Select review units: [ordering reviewer, consistency reviewer]. Reason: the formal result collection includes trigger and input, node responsibility, parallel branch and must be checked before approved translations reach the formatter.

Pair 7.1 - Rephrased

A: R1...Rn contains branches about trigger and input, node responsibility, parallel branch.

B: Required review: ordering reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.2 - Select Review

A: Formal results for Cleanup Summary include node responsibility, parallel branch, wait state.

B: Select review units: [privacy reviewer, consistency reviewer]. Reason: the formal result collection includes node responsibility, parallel branch, wait state and must be checked before approved translations reach the formatter.

Pair 7.2 - Rephrased

A: R1...Rn contains branches about node responsibility, parallel branch, wait state.

B: Required review: privacy reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.3 - Select Review

A: Formal results for Cleanup Summary include parallel branch, wait state, recovery path.

B: Select review units: [business-rule reviewer, consistency reviewer]. Reason: the formal result collection includes parallel branch, wait state, recovery path and must be checked before approved translations reach the formatter.

Pair 7.3 - Rephrased

A: R1...Rn contains branches about parallel branch, wait state, recovery path.

B: Required review: business-rule reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.4 - Select Review

A: Formal results for Cleanup Summary include wait state, recovery path, result merge.

B: Select review units: [evidence reviewer, consistency reviewer]. Reason: the formal result collection includes wait state, recovery path, result merge and must be checked before approved translations reach the formatter.

Pair 7.4 - Rephrased

A: R1...Rn contains branches about wait state, recovery path, result merge.

B: Required review: evidence reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.5 - Select Review

A: Formal results for Cleanup Summary include recovery path, result merge, review point.

B: Select review units: [consistency reviewer, consistency reviewer]. Reason: the formal result collection includes recovery path, result merge, review point and must be checked before approved translations reach the formatter.

Pair 7.5 - Rephrased

A: R1...Rn contains branches about recovery path, result merge, review point.

B: Required review: consistency reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.6 - Select Review

A: Formal results for Cleanup Summary include result merge, review point, formatted output.

B: Select review units: [recovery reviewer, consistency reviewer]. Reason: the formal result collection includes result merge, review point, formatted output and must be checked before approved translations reach the formatter.

Pair 7.6 - Rephrased

A: R1...Rn contains branches about result merge, review point, formatted output.

B: Required review: recovery reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.7 - Select Review

A: Formal results for Cleanup Summary include review point, formatted output, cleanup summary.

B: Select review units: [audience reviewer, consistency reviewer]. Reason: the formal result collection includes review point, formatted output, cleanup summary and must be checked before approved translations reach the formatter.

Pair 7.7 - Rephrased

A: R1...Rn contains branches about review point, formatted output, cleanup summary.

B: Required review: audience reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.8 - Select Review

A: Formal results for Cleanup Summary include formatted output, cleanup summary, v x.y — milestone name.

B: Select review units: [completion reviewer, consistency reviewer]. Reason: the formal result collection includes formatted output, cleanup summary, v x.y — milestone name and must be checked before approved translations reach the formatter.

Pair 7.8 - Rephrased

A: R1...Rn contains branches about formatted output, cleanup summary, v x.y — milestone name.

B: Required review: completion reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

