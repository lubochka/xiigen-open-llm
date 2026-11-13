# Node-03 - Scheme Planner Set

Runtime A is all translated domain statements. Runtime B is the separated work plan.

## Pair 3.1 - Plan Work Pieces

A: Translated statements for Security Policy: identity context is present; tenant boundary is present; permission decision is present; data isolation is present.

B: Create one work piece for each of identity context, tenant boundary, permission decision, data isolation. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.1 - Rephrased

A: The translated responsibilities are identity context, tenant boundary, permission decision, data isolation.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.2 - Plan Work Pieces

A: Translated statements for Security Policy: tenant boundary is present; permission decision is present; data isolation is present; audit evidence is present.

B: Create one work piece for each of tenant boundary, permission decision, data isolation, audit evidence. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.2 - Rephrased

A: The translated responsibilities are tenant boundary, permission decision, data isolation, audit evidence.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.3 - Plan Work Pieces

A: Translated statements for Security Policy: permission decision is present; data isolation is present; audit evidence is present; safe recovery is present.

B: Create one work piece for each of permission decision, data isolation, audit evidence, safe recovery. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.3 - Rephrased

A: The translated responsibilities are permission decision, data isolation, audit evidence, safe recovery.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.4 - Plan Work Pieces

A: Translated statements for Security Policy: data isolation is present; audit evidence is present; safe recovery is present; configuration boundary is present.

B: Create one work piece for each of data isolation, audit evidence, safe recovery, configuration boundary. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.4 - Rephrased

A: The translated responsibilities are data isolation, audit evidence, safe recovery, configuration boundary.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.5 - Plan Work Pieces

A: Translated statements for Security Policy: audit evidence is present; safe recovery is present; configuration boundary is present; release control is present.

B: Create one work piece for each of audit evidence, safe recovery, configuration boundary, release control. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.5 - Rephrased

A: The translated responsibilities are audit evidence, safe recovery, configuration boundary, release control.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.6 - Plan Work Pieces

A: Translated statements for Security Policy: safe recovery is present; configuration boundary is present; release control is present; security policy is present.

B: Create one work piece for each of safe recovery, configuration boundary, release control, security policy. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.6 - Rephrased

A: The translated responsibilities are safe recovery, configuration boundary, release control, security policy.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.7 - Plan Work Pieces

A: Translated statements for Security Policy: configuration boundary is present; release control is present; security policy is present; reporting a vulnerability is present.

B: Create one work piece for each of configuration boundary, release control, security policy, reporting a vulnerability. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.7 - Rephrased

A: The translated responsibilities are configuration boundary, release control, security policy, reporting a vulnerability.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

## Pair 3.8 - Plan Work Pieces

A: Translated statements for Security Policy: release control is present; security policy is present; reporting a vulnerability is present; response timeline is present.

B: Create one work piece for each of release control, security policy, reporting a vulnerability, response timeline. Preserve order only where one piece needs another result. Allow parallel movement where responsibilities are independent. Add a wait state before merge, a review point before formatting, and a recovery route that returns only the failed fragment.

Pair 3.8 - Rephrased

A: The translated responsibilities are release control, security policy, reporting a vulnerability, response timeline.

B: Keep the branches separate. Merge only after required evidence and review are ready. Do not collapse waiting, recovery, or review into a single happy path.

