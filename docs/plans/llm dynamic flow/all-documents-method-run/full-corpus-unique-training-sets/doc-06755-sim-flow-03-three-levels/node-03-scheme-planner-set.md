# Node-03 - Scheme Planner Set

Runtime A is all translated domain statements. Runtime B is the separated work plan.

## Pair 3.1 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: test objective is present; scenario setup is present; expected behavior is present; failure evidence is present.

B: Create one work piece for each of test objective, scenario setup, expected behavior, failure evidence. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.1 - Rephrased

A: The translated responsibilities are test objective, scenario setup, expected behavior, failure evidence.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.2 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: scenario setup is present; expected behavior is present; failure evidence is present; coverage decision is present.

B: Create one work piece for each of scenario setup, expected behavior, failure evidence, coverage decision. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.2 - Rephrased

A: The translated responsibilities are scenario setup, expected behavior, failure evidence, coverage decision.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.3 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: expected behavior is present; failure evidence is present; coverage decision is present; release gate is present.

B: Create one work piece for each of expected behavior, failure evidence, coverage decision, release gate. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.3 - Rephrased

A: The translated responsibilities are expected behavior, failure evidence, coverage decision, release gate.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.4 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: failure evidence is present; coverage decision is present; release gate is present; simulation result is present.

B: Create one work piece for each of failure evidence, coverage decision, release gate, simulation result. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.4 - Rephrased

A: The translated responsibilities are failure evidence, coverage decision, release gate, simulation result.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.5 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: coverage decision is present; release gate is present; simulation result is present; repair signal is present.

B: Create one work piece for each of coverage decision, release gate, simulation result, repair signal. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.5 - Rephrased

A: The translated responsibilities are coverage decision, release gate, simulation result, repair signal.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.6 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: release gate is present; simulation result is present; repair signal is present; simulation: the event creation flow — event creation is present.

B: Create one work piece for each of release gate, simulation result, repair signal, simulation: the event creation flow — event creation. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.6 - Rephrased

A: The translated responsibilities are release gate, simulation result, repair signal, simulation: the event creation flow — event creation.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.7 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: simulation result is present; repair signal is present; simulation: the event creation flow — event creation is present; same user, same three stacks, different domain is present.

B: Create one work piece for each of simulation result, repair signal, simulation: the event creation flow — event creation, same user, same three stacks, different domain. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.7 - Rephrased

A: The translated responsibilities are simulation result, repair signal, simulation: the event creation flow — event creation, same user, same three stacks, different domain.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.8 - Plan Work Pieces

A: Translated statements for SIMULATION: the event creation flow — EVENT CREATION: repair signal is present; simulation: the event creation flow — event creation is present; same user, same three stacks, different domain is present; protocol: xiigen-gap-simulation-protocol.md step table format is present.

B: Create one work piece for each of repair signal, simulation: the event creation flow — event creation, same user, same three stacks, different domain, protocol: xiigen-gap-simulation-protocol.md step table format. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.8 - Rephrased

A: The translated responsibilities are repair signal, simulation: the event creation flow — event creation, same user, same three stacks, different domain, protocol: xiigen-gap-simulation-protocol.md step table format.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

