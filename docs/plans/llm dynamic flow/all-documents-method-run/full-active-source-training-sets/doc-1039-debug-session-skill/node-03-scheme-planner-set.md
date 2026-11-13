# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: session intent is present and needs its own handling; creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling.

B: Create work pieces for session intent, creation evidence, decision trail, resume state. Keep session intent and creation evidence separate. Allow decision trail and resume state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: session intent, creation evidence, decision trail, resume state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling.

B: Create work pieces for creation evidence, decision trail, resume state, open work. Keep creation evidence and decision trail separate. Allow resume state and open work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: creation evidence, decision trail, resume state, open work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling.

B: Create work pieces for decision trail, resume state, open work, validation notes. Keep decision trail and resume state separate. Allow open work and validation notes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: decision trail, resume state, open work, validation notes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling; the problem is present and needs its own handling.

B: Create work pieces for resume state, open work, validation notes, the problem. Keep resume state and open work separate. Allow validation notes and the problem to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: resume state, open work, validation notes, the problem.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: open work is present and needs its own handling; validation notes is present and needs its own handling; the problem is present and needs its own handling; debug session file is present and needs its own handling.

B: Create work pieces for open work, validation notes, the problem, debug session file. Keep open work and validation notes separate. Allow the problem and debug session file to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: open work, validation notes, the problem, debug session file.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: validation notes is present and needs its own handling; the problem is present and needs its own handling; debug session file is present and needs its own handling; file format is present and needs its own handling.

B: Create work pieces for validation notes, the problem, debug session file, file format. Keep validation notes and the problem separate. Allow debug session file and file format to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: validation notes, the problem, debug session file, file format.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: the problem is present and needs its own handling; debug session file is present and needs its own handling; file format is present and needs its own handling; debug session issue title is present and needs its own handling.

B: Create work pieces for the problem, debug session file, file format, debug session issue title. Keep the problem and debug session file separate. Allow file format and debug session issue title to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: the problem, debug session file, file format, debug session issue title.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Debug-session-SKILL.md v1.0 — Persistent debug file that survives session reset: debug session file is present and needs its own handling; file format is present and needs its own handling; debug session issue title is present and needs its own handling; hypotheses is present and needs its own handling.

B: Create work pieces for debug session file, file format, debug session issue title, hypotheses. Keep debug session file and file format separate. Allow debug session issue title and hypotheses to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: debug session file, file format, debug session issue title, hypotheses.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

