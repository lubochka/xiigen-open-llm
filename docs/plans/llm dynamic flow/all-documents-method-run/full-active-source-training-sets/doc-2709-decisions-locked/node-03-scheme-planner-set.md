# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: review responsibility is present and needs its own handling; rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling.

B: Create work pieces for review responsibility, rule coverage, conflict detection, impact explanation. Keep review responsibility and rule coverage separate. Allow conflict detection and impact explanation to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: review responsibility, rule coverage, conflict detection, impact explanation.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: rule coverage is present and needs its own handling; conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling.

B: Create work pieces for rule coverage, conflict detection, impact explanation, reprocess instruction. Keep rule coverage and conflict detection separate. Allow impact explanation and reprocess instruction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: rule coverage, conflict detection, impact explanation, reprocess instruction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: conflict detection is present and needs its own handling; impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling.

B: Create work pieces for conflict detection, impact explanation, reprocess instruction, approval boundary. Keep conflict detection and impact explanation separate. Allow reprocess instruction and approval boundary to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: conflict detection, impact explanation, reprocess instruction, approval boundary.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: impact explanation is present and needs its own handling; reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen architecture decisions all locked is present and needs its own handling.

B: Create work pieces for impact explanation, reprocess instruction, approval boundary, xiigen architecture decisions all locked. Keep impact explanation and reprocess instruction separate. Allow approval boundary and xiigen architecture decisions all locked to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: impact explanation, reprocess instruction, approval boundary, xiigen architecture decisions all locked.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: reprocess instruction is present and needs its own handling; approval boundary is present and needs its own handling; xiigen architecture decisions all locked is present and needs its own handling; single source of truth for all platform decisions is present and needs its own handling.

B: Create work pieces for reprocess instruction, approval boundary, xiigen architecture decisions all locked, single source of truth for all platform decisions. Keep reprocess instruction and approval boundary separate. Allow xiigen architecture decisions all locked and single source of truth for all platform decisions to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: reprocess instruction, approval boundary, xiigen architecture decisions all locked, single source of truth for all platform decisions.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: approval boundary is present and needs its own handling; xiigen architecture decisions all locked is present and needs its own handling; single source of truth for all platform decisions is present and needs its own handling; date 2026 03 20 is present and needs its own handling.

B: Create work pieces for approval boundary, xiigen architecture decisions all locked, single source of truth for all platform decisions, date 2026 03 20. Keep approval boundary and xiigen architecture decisions all locked separate. Allow single source of truth for all platform decisions and date 2026 03 20 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: approval boundary, xiigen architecture decisions all locked, single source of truth for all platform decisions, date 2026 03 20.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: xiigen architecture decisions all locked is present and needs its own handling; single source of truth for all platform decisions is present and needs its own handling; date 2026 03 20 is present and needs its own handling; context is present and needs its own handling.

B: Create work pieces for xiigen architecture decisions all locked, single source of truth for all platform decisions, date 2026 03 20, context. Keep xiigen architecture decisions all locked and single source of truth for all platform decisions separate. Allow date 2026 03 20 and context to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: xiigen architecture decisions all locked, single source of truth for all platform decisions, date 2026 03 20, context.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for XIIGEN ARCHITECTURE DECISIONS — ALL LOCKED: single source of truth for all platform decisions is present and needs its own handling; date 2026 03 20 is present and needs its own handling; context is present and needs its own handling; integration decisions d1 d10 is present and needs its own handling.

B: Create work pieces for single source of truth for all platform decisions, date 2026 03 20, context, integration decisions d1 d10. Keep single source of truth for all platform decisions and date 2026 03 20 separate. Allow context and integration decisions d1 d10 to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: single source of truth for all platform decisions, date 2026 03 20, context, integration decisions d1 d10.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

