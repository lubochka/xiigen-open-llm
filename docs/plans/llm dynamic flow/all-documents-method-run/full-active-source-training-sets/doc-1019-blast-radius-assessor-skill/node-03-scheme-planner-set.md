# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; before touching any file classify its blast radius is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, before touching any file classify its blast radius. Keep impact explanation and reprocess instruction separate. Allow approval boundary and before touching any file classify its blast radius to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, before touching any file classify its blast radius.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; before touching any file classify its blast radius is present and needs its own handling; rules is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, before touching any file classify its blast radius, rules. Keep reprocess instruction and approval boundary separate. Allow before touching any file classify its blast radius and rules to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, before touching any file classify its blast radius, rules.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: approval boundary is present and needs its own handling; before touching any file classify its blast radius is present and needs its own handling; rules is present and needs its own handling; core change validation template is present and needs its own handling.

B: Create work pieces for approval boundary, before touching any file classify its blast radius, rules, core change validation template. Keep approval boundary and before touching any file classify its blast radius separate. Allow rules and core change validation template to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, before touching any file classify its blast radius, rules, core change validation template.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: before touching any file classify its blast radius is present and needs its own handling; rules is present and needs its own handling; core change validation template is present and needs its own handling; after any core cs or fabric interfaces cs change is present and needs its own handling.

B: Create work pieces for before touching any file classify its blast radius, rules, core change validation template, after any core cs or fabric interfaces cs change. Keep before touching any file classify its blast radius and rules separate. Allow core change validation template and after any core cs or fabric interfaces cs change to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: before touching any file classify its blast radius, rules, core change validation template, after any core cs or fabric interfaces cs change.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Blast-radius-assessor-SKILL.md v1.0 — How far does this change propagate?: rules is present and needs its own handling; core change validation template is present and needs its own handling; after any core cs or fabric interfaces cs change is present and needs its own handling; check every subproject that uses the changed interface is present and needs its own handling.

B: Create work pieces for rules, core change validation template, after any core cs or fabric interfaces cs change, check every subproject that uses the changed interface. Keep rules and core change validation template separate. Allow after any core cs or fabric interfaces cs change and check every subproject that uses the changed interface to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: rules, core change validation template, after any core cs or fabric interfaces cs change, check every subproject that uses the changed interface.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

