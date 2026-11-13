# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; source meaning is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, source meaning. Keep wait and recovery and result merge separate. Allow review before output and source meaning to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, source meaning.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": result merge is present and needs its own handling; review before output is present and needs its own handling; source meaning is present and needs its own handling; audience need is present and needs its own handling.

B: Create work pieces for result merge, review before output, source meaning, audience need. Keep result merge and review before output separate. Allow source meaning and audience need to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, source meaning, audience need.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": review before output is present and needs its own handling; source meaning is present and needs its own handling; audience need is present and needs its own handling; version evidence is present and needs its own handling.

B: Create work pieces for review before output, source meaning, audience need, version evidence. Keep review before output and source meaning separate. Allow audience need and version evidence to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, source meaning, audience need, version evidence.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": source meaning is present and needs its own handling; audience need is present and needs its own handling; version evidence is present and needs its own handling; edge condition is present and needs its own handling.

B: Create work pieces for source meaning, audience need, version evidence, edge condition. Keep source meaning and audience need separate. Allow version evidence and edge condition to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: source meaning, audience need, version evidence, edge condition.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for <svg width="100%" viewBox="0 0 680 820" role="img" xmlns=": audience need is present and needs its own handling; version evidence is present and needs its own handling; edge condition is present and needs its own handling; training example is present and needs its own handling.

B: Create work pieces for audience need, version evidence, edge condition, training example. Keep audience need and version evidence separate. Allow edge condition and training example to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: audience need, version evidence, edge condition, training example.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

