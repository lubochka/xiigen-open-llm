# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen the business onboarding flow fce single source of truth is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen the business onboarding flow fce single source of truth. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen the business onboarding flow fce single source of truth to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen the business onboarding flow fce single source of truth.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen the business onboarding flow fce single source of truth is present and needs its own handling; merge plan date 2026 02 25 is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen the business onboarding flow fce single source of truth, merge plan date 2026 02 25. Keep reprocess instruction and approval boundary separate. Allow xiigen the business onboarding flow fce single source of truth and merge plan date 2026 02 25 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen the business onboarding flow fce single source of truth, merge plan date 2026 02 25.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: approval boundary is present and needs its own handling; xiigen the business onboarding flow fce single source of truth is present and needs its own handling; merge plan date 2026 02 25 is present and needs its own handling; status planning is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen the business onboarding flow fce single source of truth, merge plan date 2026 02 25, status planning. Keep approval boundary and xiigen the business onboarding flow fce single source of truth separate. Allow merge plan date 2026 02 25 and status planning to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen the business onboarding flow fce single source of truth, merge plan date 2026 02 25, status planning.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: xiigen the business onboarding flow fce single source of truth is present and needs its own handling; merge plan date 2026 02 25 is present and needs its own handling; status planning is present and needs its own handling; no code explanation is present and needs its own handling.

B: Create work pieces for xiigen the business onboarding flow fce single source of truth, merge plan date 2026 02 25, status planning, no code explanation. Keep xiigen the business onboarding flow fce single source of truth and merge plan date 2026 02 25 separate. Allow status planning and no code explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen the business onboarding flow fce single source of truth, merge plan date 2026 02 25, status planning, no code explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGen — the business onboarding flow + FCE → SINGLE SOURCE OF TRUTH: merge plan date 2026 02 25 is present and needs its own handling; status planning is present and needs its own handling; no code explanation is present and needs its own handling; what we re doing plain english is present and needs its own handling.

B: Create work pieces for merge plan date 2026 02 25, status planning, no code explanation, what we re doing plain english. Keep merge plan date 2026 02 25 and status planning separate. Allow no code explanation and what we re doing plain english to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: merge plan date 2026 02 25, status planning, no code explanation, what we re doing plain english.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

