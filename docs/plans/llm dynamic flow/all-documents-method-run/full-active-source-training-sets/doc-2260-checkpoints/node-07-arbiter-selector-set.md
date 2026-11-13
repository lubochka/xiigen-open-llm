# Node-07 - Arbiter Selector Set

Runtime A is the formal result collection.
Runtime B is the required review units.

## Pair 7.1 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include session intent, creation evidence, decision trail.

B: Select review units: [ordering reviewer, consistency reviewer]. Reason: the formal result collection includes session intent, creation evidence, decision trail and must be checked before approved translations reach the formatter.

Pair 7.1 - Rephrased

A: R1...Rn contains branches about session intent, creation evidence, decision trail.

B: Required review: ordering reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.2 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include creation evidence, decision trail, resume state.

B: Select review units: [privacy reviewer, consistency reviewer]. Reason: the formal result collection includes creation evidence, decision trail, resume state and must be checked before approved translations reach the formatter.

Pair 7.2 - Rephrased

A: R1...Rn contains branches about creation evidence, decision trail, resume state.

B: Required review: privacy reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.3 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include decision trail, resume state, open work.

B: Select review units: [business-rule reviewer, consistency reviewer]. Reason: the formal result collection includes decision trail, resume state, open work and must be checked before approved translations reach the formatter.

Pair 7.3 - Rephrased

A: R1...Rn contains branches about decision trail, resume state, open work.

B: Required review: business-rule reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.4 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include resume state, open work, validation notes.

B: Select review units: [evidence reviewer, consistency reviewer]. Reason: the formal result collection includes resume state, open work, validation notes and must be checked before approved translations reach the formatter.

Pair 7.4 - Rephrased

A: R1...Rn contains branches about resume state, open work, validation notes.

B: Required review: evidence reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.5 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include open work, validation notes, checkpoint human verify most common 90.

B: Select review units: [consistency reviewer, consistency reviewer]. Reason: the formal result collection includes open work, validation notes, checkpoint human verify most common 90 and must be checked before approved translations reach the formatter.

Pair 7.5 - Rephrased

A: R1...Rn contains branches about open work, validation notes, checkpoint human verify most common 90.

B: Required review: consistency reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.6 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include validation notes, checkpoint human verify most common 90, checkpoint decision 9.

B: Select review units: [recovery reviewer, consistency reviewer]. Reason: the formal result collection includes validation notes, checkpoint human verify most common 90, checkpoint decision 9 and must be checked before approved translations reach the formatter.

Pair 7.6 - Rephrased

A: R1...Rn contains branches about validation notes, checkpoint human verify most common 90, checkpoint decision 9.

B: Required review: recovery reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.7 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare.

B: Select review units: [audience reviewer, consistency reviewer]. Reason: the formal result collection includes checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare and must be checked before approved translations reach the formatter.

Pair 7.7 - Rephrased

A: R1...Rn contains branches about checkpoint human verify most common 90, checkpoint decision 9, checkpoint human action 1 rare.

B: Required review: audience reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

## Pair 7.8 - Select Review

A: Formal results for If Claude can run it, Claude runs it - Never ask user to execute CLI commands, start servers, or run builds include checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task.

B: Select review units: [completion reviewer, consistency reviewer]. Reason: the formal result collection includes checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task and must be checked before approved translations reach the formatter.

Pair 7.8 - Rephrased

A: R1...Rn contains branches about checkpoint decision 9, checkpoint human action 1 rare, stop immediately do not proceed to next task.

B: Required review: completion reviewer plus consistency reviewer. These reviews happen beside result translation and before final formatting.

