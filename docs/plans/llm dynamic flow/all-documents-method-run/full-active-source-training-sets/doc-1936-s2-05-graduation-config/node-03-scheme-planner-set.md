# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: trigger and input is present and needs its own handling; node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling.

B: Create work pieces for trigger and input, node responsibility, parallel branch, wait and recovery. Keep trigger and input and node responsibility separate. Allow parallel branch and wait and recovery to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: trigger and input, node responsibility, parallel branch, wait and recovery.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: node responsibility is present and needs its own handling; parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling.

B: Create work pieces for node responsibility, parallel branch, wait and recovery, result merge. Keep node responsibility and parallel branch separate. Allow wait and recovery and result merge to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: node responsibility, parallel branch, wait and recovery, result merge.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: parallel branch is present and needs its own handling; wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling.

B: Create work pieces for parallel branch, wait and recovery, result merge, review before output. Keep parallel branch and wait and recovery separate. Allow result merge and review before output to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: parallel branch, wait and recovery, result merge, review before output.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: wait and recovery is present and needs its own handling; result merge is present and needs its own handling; review before output is present and needs its own handling; session s2 05 per tier graduation routing in freedom config b 3 is present and needs its own handling.

B: Create work pieces for wait and recovery, result merge, review before output, session s2 05 per tier graduation routing in freedom config b 3. Keep wait and recovery and result merge separate. Allow review before output and session s2 05 per tier graduation routing in freedom config b 3 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: wait and recovery, result merge, review before output, session s2 05 per tier graduation routing in freedom config b 3.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: result merge is present and needs its own handling; review before output is present and needs its own handling; session s2 05 per tier graduation routing in freedom config b 3 is present and needs its own handling; type generation is present and needs its own handling.

B: Create work pieces for result merge, review before output, session s2 05 per tier graduation routing in freedom config b 3, type generation. Keep result merge and review before output separate. Allow session s2 05 per tier graduation routing in freedom config b 3 and type generation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: result merge, review before output, session s2 05 per tier graduation routing in freedom config b 3, type generation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: review before output is present and needs its own handling; session s2 05 per tier graduation routing in freedom config b 3 is present and needs its own handling; type generation is present and needs its own handling; duration 1 hour is present and needs its own handling.

B: Create work pieces for review before output, session s2 05 per tier graduation routing in freedom config b 3, type generation, duration 1 hour. Keep review before output and session s2 05 per tier graduation routing in freedom config b 3 separate. Allow type generation and duration 1 hour to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: review before output, session s2 05 per tier graduation routing in freedom config b 3, type generation, duration 1 hour.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: session s2 05 per tier graduation routing in freedom config b 3 is present and needs its own handling; type generation is present and needs its own handling; duration 1 hour is present and needs its own handling; depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice is present and needs its own handling.

B: Create work pieces for session s2 05 per tier graduation routing in freedom config b 3, type generation, duration 1 hour, depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice. Keep session s2 05 per tier graduation routing in freedom config b 3 and type generation separate. Allow duration 1 hour and depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: session s2 05 per tier graduation routing in freedom config b 3, type generation, duration 1 hour, depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for SESSION S2-05 — Per-Tier Graduation Routing in FREEDOM Config B-3: type generation is present and needs its own handling; duration 1 hour is present and needs its own handling; depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice is present and needs its own handling; step 0 assumption registry is present and needs its own handling.

B: Create work pieces for type generation, duration 1 hour, depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice, step 0 assumption registry. Keep type generation and duration 1 hour separate. Allow depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice and step 0 assumption registry to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: type generation, duration 1 hour, depends on s1 04 curriculumtierresolver s1 07 ishadowrunservice, step 0 assumption registry.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

