# Node-03 - Scheme Planner Set

Runtime A is all translated domain statements. Runtime B is the separated work plan.

## Pair 3.1 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: user intent is present; creation evidence is present; decision trail is present; version change is present.

B: Create one work piece for each of user intent, creation evidence, decision trail, version change. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.1 - Rephrased

A: The translated responsibilities are user intent, creation evidence, decision trail, version change.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.2 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: creation evidence is present; decision trail is present; version change is present; resume state is present.

B: Create one work piece for each of creation evidence, decision trail, version change, resume state. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.2 - Rephrased

A: The translated responsibilities are creation evidence, decision trail, version change, resume state.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.3 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: decision trail is present; version change is present; resume state is present; open work is present.

B: Create one work piece for each of decision trail, version change, resume state, open work. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.3 - Rephrased

A: The translated responsibilities are decision trail, version change, resume state, open work.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.4 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: version change is present; resume state is present; open work is present; validation note is present.

B: Create one work piece for each of version change, resume state, open work, validation note. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.4 - Rephrased

A: The translated responsibilities are version change, resume state, open work, validation note.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.5 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: resume state is present; open work is present; validation note is present; later use is present.

B: Create one work piece for each of resume state, open work, validation note, later use. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.5 - Rephrased

A: The translated responsibilities are resume state, open work, validation note, later use.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.6 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: open work is present; validation note is present; later use is present; session-2: snapshotservice + filesystem persistence is present.

B: Create one work piece for each of open work, validation note, later use, session-2: snapshotservice + filesystem persistence. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.6 - Rephrased

A: The translated responsibilities are open work, validation note, later use, session-2: snapshotservice + filesystem persistence.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.7 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: validation note is present; later use is present; session-2: snapshotservice + filesystem persistence is present; consumer: claude code is present.

B: Create one work piece for each of validation note, later use, session-2: snapshotservice + filesystem persistence, consumer: claude code. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.7 - Rephrased

A: The translated responsibilities are validation note, later use, session-2: snapshotservice + filesystem persistence, consumer: claude code.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.8 - Plan Work Pieces

A: Translated statements for SESSION-2: SnapshotService + Filesystem Persistence: later use is present; session-2: snapshotservice + filesystem persistence is present; consumer: claude code is present; prerequisites: session 1 complete 5 stores have exportstate/importstate is present.

B: Create one work piece for each of later use, session-2: snapshotservice + filesystem persistence, consumer: claude code, prerequisites: session 1 complete 5 stores have exportstate/importstate. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.8 - Rephrased

A: The translated responsibilities are later use, session-2: snapshotservice + filesystem persistence, consumer: claude code, prerequisites: session 1 complete 5 stores have exportstate/importstate.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

