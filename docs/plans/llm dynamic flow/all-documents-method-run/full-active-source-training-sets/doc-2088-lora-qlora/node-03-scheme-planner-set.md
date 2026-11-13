# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; lora qlora parameter efficient fine tuning is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, lora qlora parameter efficient fine tuning. Keep impact explanation and reprocess instruction separate. Allow approval boundary and lora qlora parameter efficient fine tuning to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, lora qlora parameter efficient fine tuning.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; lora qlora parameter efficient fine tuning is present and needs its own handling; table of contents is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, lora qlora parameter efficient fine tuning, table of contents. Keep reprocess instruction and approval boundary separate. Allow lora qlora parameter efficient fine tuning and table of contents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, lora qlora parameter efficient fine tuning, table of contents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: approval boundary is present and needs its own handling; lora qlora parameter efficient fine tuning is present and needs its own handling; table of contents is present and needs its own handling; lora architecture is present and needs its own handling.

B: Create work pieces for approval boundary, lora qlora parameter efficient fine tuning, table of contents, lora architecture. Keep approval boundary and lora qlora parameter efficient fine tuning separate. Allow table of contents and lora architecture to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, lora qlora parameter efficient fine tuning, table of contents, lora architecture.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: lora qlora parameter efficient fine tuning is present and needs its own handling; table of contents is present and needs its own handling; lora architecture is present and needs its own handling; the core idea is present and needs its own handling.

B: Create work pieces for lora qlora parameter efficient fine tuning, table of contents, lora architecture, the core idea. Keep lora qlora parameter efficient fine tuning and table of contents separate. Allow lora architecture and the core idea to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: lora qlora parameter efficient fine tuning, table of contents, lora architecture, the core idea.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for LoRA & QLoRA: Parameter-Efficient Fine-Tuning: table of contents is present and needs its own handling; lora architecture is present and needs its own handling; the core idea is present and needs its own handling; how lora works is present and needs its own handling.

B: Create work pieces for table of contents, lora architecture, the core idea, how lora works. Keep table of contents and lora architecture separate. Allow the core idea and how lora works to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: table of contents, lora architecture, the core idea, how lora works.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

