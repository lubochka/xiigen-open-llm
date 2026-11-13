# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; skill 12 ai review service implementation prompt is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, skill 12 ai review service implementation prompt. Keep impact explanation and reprocess instruction separate. Allow approval boundary and skill 12 ai review service implementation prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, skill 12 ai review service implementation prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; skill 12 ai review service implementation prompt is present and needs its own handling; overview is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, skill 12 ai review service implementation prompt, overview. Keep reprocess instruction and approval boundary separate. Allow skill 12 ai review service implementation prompt and overview to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, skill 12 ai review service implementation prompt, overview.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: approval boundary is present and needs its own handling; skill 12 ai review service implementation prompt is present and needs its own handling; overview is present and needs its own handling; phase 1 models interfaces is present and needs its own handling.

B: Create work pieces for approval boundary, skill 12 ai review service implementation prompt, overview, phase 1 models interfaces. Keep approval boundary and skill 12 ai review service implementation prompt separate. Allow overview and phase 1 models interfaces to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, skill 12 ai review service implementation prompt, overview, phase 1 models interfaces.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: skill 12 ai review service implementation prompt is present and needs its own handling; overview is present and needs its own handling; phase 1 models interfaces is present and needs its own handling; phase 2 core review pipeline is present and needs its own handling.

B: Create work pieces for skill 12 ai review service implementation prompt, overview, phase 1 models interfaces, phase 2 core review pipeline. Keep skill 12 ai review service implementation prompt and overview separate. Allow phase 1 models interfaces and phase 2 core review pipeline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: skill 12 ai review service implementation prompt, overview, phase 1 models interfaces, phase 2 core review pipeline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Skill 12 — AI Review Service: Implementation Prompt: overview is present and needs its own handling; phase 1 models interfaces is present and needs its own handling; phase 2 core review pipeline is present and needs its own handling; phase 3 response parsing is present and needs its own handling.

B: Create work pieces for overview, phase 1 models interfaces, phase 2 core review pipeline, phase 3 response parsing. Keep overview and phase 1 models interfaces separate. Allow phase 2 core review pipeline and phase 3 response parsing to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: overview, phase 1 models interfaces, phase 2 core review pipeline, phase 3 response parsing.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

