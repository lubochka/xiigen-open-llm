# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; phasecompletionpackager a planning skill is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, phasecompletionpackager a planning skill. Keep impact explanation and reprocess instruction separate. Allow approval boundary and phasecompletionpackager a planning skill to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, phasecompletionpackager a planning skill.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; phasecompletionpackager a planning skill is present and needs its own handling; purpose is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, phasecompletionpackager a planning skill, purpose. Keep reprocess instruction and approval boundary separate. Allow phasecompletionpackager a planning skill and purpose to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, phasecompletionpackager a planning skill, purpose.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: approval boundary is present and needs its own handling; phasecompletionpackager a planning skill is present and needs its own handling; purpose is present and needs its own handling; gate models read state json first is present and needs its own handling.

B: Create work pieces for approval boundary, phasecompletionpackager a planning skill, purpose, gate models read state json first. Keep approval boundary and phasecompletionpackager a planning skill separate. Allow purpose and gate models read state json first to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, phasecompletionpackager a planning skill, purpose, gate models read state json first.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: phasecompletionpackager a planning skill is present and needs its own handling; purpose is present and needs its own handling; gate models read state json first is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for phasecompletionpackager a planning skill, purpose, gate models read state json first, when to invoke. Keep phasecompletionpackager a planning skill and purpose separate. Allow gate models read state json first and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: phasecompletionpackager a planning skill, purpose, gate models read state json first, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PhaseCompletionPackager a planning skill: purpose is present and needs its own handling; gate models read state json first is present and needs its own handling; when to invoke is present and needs its own handling; gate check two models is present and needs its own handling.

B: Create work pieces for purpose, gate models read state json first, when to invoke, gate check two models. Keep purpose and gate models read state json first separate. Allow when to invoke and gate check two models to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: purpose, gate models read state json first, when to invoke, gate check two models.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

