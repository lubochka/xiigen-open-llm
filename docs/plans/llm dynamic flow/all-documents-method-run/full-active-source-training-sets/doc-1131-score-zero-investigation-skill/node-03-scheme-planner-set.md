# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; the rule is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, the rule. Keep impact explanation and reprocess instruction separate. Allow approval boundary and the rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, the rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; the rule is present and needs its own handling; when to invoke is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, the rule, when to invoke. Keep reprocess instruction and approval boundary separate. Allow the rule and when to invoke to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, the rule, when to invoke.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: approval boundary is present and needs its own handling; the rule is present and needs its own handling; when to invoke is present and needs its own handling; step 1 identify which rule fired is present and needs its own handling.

B: Create work pieces for approval boundary, the rule, when to invoke, step 1 identify which rule fired. Keep approval boundary and the rule separate. Allow when to invoke and step 1 identify which rule fired to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, the rule, when to invoke, step 1 identify which rule fired.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: the rule is present and needs its own handling; when to invoke is present and needs its own handling; step 1 identify which rule fired is present and needs its own handling; find which dna rule or test integrity rule blocked the result is present and needs its own handling.

B: Create work pieces for the rule, when to invoke, step 1 identify which rule fired, find which dna rule or test integrity rule blocked the result. Keep the rule and when to invoke separate. Allow step 1 identify which rule fired and find which dna rule or test integrity rule blocked the result to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the rule, when to invoke, step 1 identify which rule fired, find which dna rule or test integrity rule blocked the result.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Score-zero-investigation-SKILL.md v1.0 — When a named check blocks an experiment result: when to invoke is present and needs its own handling; step 1 identify which rule fired is present and needs its own handling; find which dna rule or test integrity rule blocked the result is present and needs its own handling; find the specific assertion that blocked the result is present and needs its own handling.

B: Create work pieces for when to invoke, step 1 identify which rule fired, find which dna rule or test integrity rule blocked the result, find the specific assertion that blocked the result. Keep when to invoke and step 1 identify which rule fired separate. Allow find which dna rule or test integrity rule blocked the result and find the specific assertion that blocked the result to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: when to invoke, step 1 identify which rule fired, find which dna rule or test integrity rule blocked the result, find the specific assertion that blocked the result.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

