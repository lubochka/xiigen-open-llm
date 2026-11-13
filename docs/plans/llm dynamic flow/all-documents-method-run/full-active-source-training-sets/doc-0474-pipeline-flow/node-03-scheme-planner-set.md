# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; figma to code v6 3 complete pipeline flow is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, figma to code v6 3 complete pipeline flow. Keep impact explanation and reprocess instruction separate. Allow approval boundary and figma to code v6 3 complete pipeline flow to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, figma to code v6 3 complete pipeline flow.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; figma to code v6 3 complete pipeline flow is present and needs its own handling; how to read this document is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, figma to code v6 3 complete pipeline flow, how to read this document. Keep reprocess instruction and approval boundary separate. Allow figma to code v6 3 complete pipeline flow and how to read this document to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, figma to code v6 3 complete pipeline flow, how to read this document.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: approval boundary is present and needs its own handling; figma to code v6 3 complete pipeline flow is present and needs its own handling; how to read this document is present and needs its own handling; stage 0 plugin launch is present and needs its own handling.

B: Create work pieces for approval boundary, figma to code v6 3 complete pipeline flow, how to read this document, stage 0 plugin launch. Keep approval boundary and figma to code v6 3 complete pipeline flow separate. Allow how to read this document and stage 0 plugin launch to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, figma to code v6 3 complete pipeline flow, how to read this document, stage 0 plugin launch.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: figma to code v6 3 complete pipeline flow is present and needs its own handling; how to read this document is present and needs its own handling; stage 0 plugin launch is present and needs its own handling; stage 1 recursive tree scan is present and needs its own handling.

B: Create work pieces for figma to code v6 3 complete pipeline flow, how to read this document, stage 0 plugin launch, stage 1 recursive tree scan. Keep figma to code v6 3 complete pipeline flow and how to read this document separate. Allow stage 0 plugin launch and stage 1 recursive tree scan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: figma to code v6 3 complete pipeline flow, how to read this document, stage 0 plugin launch, stage 1 recursive tree scan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code v6.3 — Complete Pipeline Flow: how to read this document is present and needs its own handling; stage 0 plugin launch is present and needs its own handling; stage 1 recursive tree scan is present and needs its own handling; step 1 1 visibility gate is present and needs its own handling.

B: Create work pieces for how to read this document, stage 0 plugin launch, stage 1 recursive tree scan, step 1 1 visibility gate. Keep how to read this document and stage 0 plugin launch separate. Allow stage 1 recursive tree scan and step 1 1 visibility gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: how to read this document, stage 0 plugin launch, stage 1 recursive tree scan, step 1 1 visibility gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

