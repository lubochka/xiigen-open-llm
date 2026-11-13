# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: session intent is present and needs its own handling; creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling.

B: Create work pieces for session intent, creation evidence, decision trail, resume state. Keep session intent and creation evidence separate. Allow decision trail and resume state to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: session intent, creation evidence, decision trail, resume state.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: creation evidence is present and needs its own handling; decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling.

B: Create work pieces for creation evidence, decision trail, resume state, open work. Keep creation evidence and decision trail separate. Allow resume state and open work to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: creation evidence, decision trail, resume state, open work.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: decision trail is present and needs its own handling; resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling.

B: Create work pieces for decision trail, resume state, open work, validation notes. Keep decision trail and resume state separate. Allow open work and validation notes to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: decision trail, resume state, open work, validation notes.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: resume state is present and needs its own handling; open work is present and needs its own handling; validation notes is present and needs its own handling; figma to code plugin fix plan is present and needs its own handling.

B: Create work pieces for resume state, open work, validation notes, figma to code plugin fix plan. Keep resume state and open work separate. Allow validation notes and figma to code plugin fix plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: resume state, open work, validation notes, figma to code plugin fix plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: open work is present and needs its own handling; validation notes is present and needs its own handling; figma to code plugin fix plan is present and needs its own handling; session summary next session plan is present and needs its own handling.

B: Create work pieces for open work, validation notes, figma to code plugin fix plan, session summary next session plan. Keep open work and validation notes separate. Allow figma to code plugin fix plan and session summary next session plan to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: open work, validation notes, figma to code plugin fix plan, session summary next session plan.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: validation notes is present and needs its own handling; figma to code plugin fix plan is present and needs its own handling; session summary next session plan is present and needs its own handling; part 1 what was done 7 fixes shipped 2 reverted is present and needs its own handling.

B: Create work pieces for validation notes, figma to code plugin fix plan, session summary next session plan, part 1 what was done 7 fixes shipped 2 reverted. Keep validation notes and figma to code plugin fix plan separate. Allow session summary next session plan and part 1 what was done 7 fixes shipped 2 reverted to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: validation notes, figma to code plugin fix plan, session summary next session plan, part 1 what was done 7 fixes shipped 2 reverted.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: figma to code plugin fix plan is present and needs its own handling; session summary next session plan is present and needs its own handling; part 1 what was done 7 fixes shipped 2 reverted is present and needs its own handling; shipped confirmed working is present and needs its own handling.

B: Create work pieces for figma to code plugin fix plan, session summary next session plan, part 1 what was done 7 fixes shipped 2 reverted, shipped confirmed working. Keep figma to code plugin fix plan and session summary next session plan separate. Allow part 1 what was done 7 fixes shipped 2 reverted and shipped confirmed working to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: figma to code plugin fix plan, session summary next session plan, part 1 what was done 7 fixes shipped 2 reverted, shipped confirmed working.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Figma-to-Code Plugin Fix Plan: session summary next session plan is present and needs its own handling; part 1 what was done 7 fixes shipped 2 reverted is present and needs its own handling; shipped confirmed working is present and needs its own handling; reverted why is present and needs its own handling.

B: Create work pieces for session summary next session plan, part 1 what was done 7 fixes shipped 2 reverted, shipped confirmed working, reverted why. Keep session summary next session plan and part 1 what was done 7 fixes shipped 2 reverted separate. Allow shipped confirmed working and reverted why to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: session summary next session plan, part 1 what was done 7 fixes shipped 2 reverted, shipped confirmed working, reverted why.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

