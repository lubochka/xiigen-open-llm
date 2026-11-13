# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; episode query mapping is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, episode query mapping. Keep impact explanation and reprocess instruction separate. Allow approval boundary and episode query mapping to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, episode query mapping.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; episode query mapping is present and needs its own handling; alternative ways a user might ask about each episode is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, episode query mapping, alternative ways a user might ask about each episode. Keep reprocess instruction and approval boundary separate. Allow episode query mapping and alternative ways a user might ask about each episode to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, episode query mapping, alternative ways a user might ask about each episode.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: approval boundary is present and needs its own handling; episode query mapping is present and needs its own handling; alternative ways a user might ask about each episode is present and needs its own handling; how this mapping works is present and needs its own handling.

B: Create work pieces for approval boundary, episode query mapping, alternative ways a user might ask about each episode, how this mapping works. Keep approval boundary and episode query mapping separate. Allow alternative ways a user might ask about each episode and how this mapping works to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, episode query mapping, alternative ways a user might ask about each episode, how this mapping works.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: episode query mapping is present and needs its own handling; alternative ways a user might ask about each episode is present and needs its own handling; how this mapping works is present and needs its own handling; episode 1 architecture planning from a business idea is present and needs its own handling.

B: Create work pieces for episode query mapping, alternative ways a user might ask about each episode, how this mapping works, episode 1 architecture planning from a business idea. Keep episode query mapping and alternative ways a user might ask about each episode separate. Allow how this mapping works and episode 1 architecture planning from a business idea to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: episode query mapping, alternative ways a user might ask about each episode, how this mapping works, episode 1 architecture planning from a business idea.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Episode Query Mapping: alternative ways a user might ask about each episode is present and needs its own handling; how this mapping works is present and needs its own handling; episode 1 architecture planning from a business idea is present and needs its own handling; episode 3a platform modules too tightly coupled is present and needs its own handling.

B: Create work pieces for alternative ways a user might ask about each episode, how this mapping works, episode 1 architecture planning from a business idea, episode 3a platform modules too tightly coupled. Keep alternative ways a user might ask about each episode and how this mapping works separate. Allow episode 1 architecture planning from a business idea and episode 3a platform modules too tightly coupled to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: alternative ways a user might ask about each episode, how this mapping works, episode 1 architecture planning from a business idea, episode 3a platform modules too tightly coupled.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

