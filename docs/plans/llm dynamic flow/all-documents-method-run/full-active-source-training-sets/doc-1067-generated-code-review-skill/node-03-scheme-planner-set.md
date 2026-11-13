# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, when to invoke. Keep impact explanation and reprocess instruction separate. Allow approval boundary and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; when to invoke is present and needs its own handling; the principle translated from xiigen generated code review is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, when to invoke, the principle translated from xiigen generated code review. Keep reprocess instruction and approval boundary separate. Allow when to invoke and the principle translated from xiigen generated code review to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, when to invoke, the principle translated from xiigen generated code review.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: approval boundary is present and needs its own handling; when to invoke is present and needs its own handling; the principle translated from xiigen generated code review is present and needs its own handling; layers of review is present and needs its own handling.

B: Create work pieces for approval boundary, when to invoke, the principle translated from xiigen generated code review, layers of review. Keep approval boundary and when to invoke separate. Allow the principle translated from xiigen generated code review and layers of review to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, when to invoke, the principle translated from xiigen generated code review, layers of review.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: when to invoke is present and needs its own handling; the principle translated from xiigen generated code review is present and needs its own handling; layers of review is present and needs its own handling; layer 1 semantic review does output communicate what it does is present and needs its own handling.

B: Create work pieces for when to invoke, the principle translated from xiigen generated code review, layers of review, layer 1 semantic review does output communicate what it does. Keep when to invoke and the principle translated from xiigen generated code review separate. Allow layers of review and layer 1 semantic review does output communicate what it does to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: when to invoke, the principle translated from xiigen generated code review, layers of review, layer 1 semantic review does output communicate what it does.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Generated-code-review-SKILL.md v1.0 — Review SemanticCompiler output before recording results: the principle translated from xiigen generated code review is present and needs its own handling; layers of review is present and needs its own handling; layer 1 semantic review does output communicate what it does is present and needs its own handling; layer 3 rule compliance review does output respect d 01 d 12 is present and needs its own handling.

B: Create work pieces for the principle translated from xiigen generated code review, layers of review, layer 1 semantic review does output communicate what it does, layer 3 rule compliance review does output respect d 01 d 12. Keep the principle translated from xiigen generated code review and layers of review separate. Allow layer 1 semantic review does output communicate what it does and layer 3 rule compliance review does output respect d 01 d 12 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: the principle translated from xiigen generated code review, layers of review, layer 1 semantic review does output communicate what it does, layer 3 rule compliance review does output respect d 01 d 12.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

