# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; what this library does is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, what this library does. Keep impact explanation and reprocess instruction separate. Allow approval boundary and what this library does to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, what this library does.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; what this library does is present and needs its own handling; the copy paste prompt is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, what this library does, the copy paste prompt. Keep reprocess instruction and approval boundary separate. Allow what this library does and the copy paste prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, what this library does, the copy paste prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: approval boundary is present and needs its own handling; what this library does is present and needs its own handling; the copy paste prompt is present and needs its own handling; task generate list b documentation for flow xx slug is present and needs its own handling.

B: Create work pieces for approval boundary, what this library does, the copy paste prompt, task generate list b documentation for flow xx slug. Keep approval boundary and what this library does separate. Allow the copy paste prompt and task generate list b documentation for flow xx slug to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, what this library does, the copy paste prompt, task generate list b documentation for flow xx slug.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: what this library does is present and needs its own handling; the copy paste prompt is present and needs its own handling; task generate list b documentation for flow xx slug is present and needs its own handling; sources is present and needs its own handling.

B: Create work pieces for what this library does, the copy paste prompt, task generate list b documentation for flow xx slug, sources. Keep what this library does and the copy paste prompt separate. Allow task generate list b documentation for flow xx slug and sources to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: what this library does, the copy paste prompt, task generate list b documentation for flow xx slug, sources.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Prompt to Claude — How to Apply the Flow-Prep Library to a New Flow Spec: the copy paste prompt is present and needs its own handling; task generate list b documentation for flow xx slug is present and needs its own handling; sources is present and needs its own handling; rules is present and needs its own handling.

B: Create work pieces for the copy paste prompt, task generate list b documentation for flow xx slug, sources, rules. Keep the copy paste prompt and task generate list b documentation for flow xx slug separate. Allow sources and rules to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the copy paste prompt, task generate list b documentation for flow xx slug, sources, rules.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

