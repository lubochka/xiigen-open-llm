# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; skill 37 security implementation prompt is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, skill 37 security implementation prompt. Keep configuration choice and dependency rule separate. Allow completion gate and skill 37 security implementation prompt to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, skill 37 security implementation prompt.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; skill 37 security implementation prompt is present and needs its own handling; phase 1 input sanitization is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, skill 37 security implementation prompt, phase 1 input sanitization. Keep dependency rule and completion gate separate. Allow skill 37 security implementation prompt and phase 1 input sanitization to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, skill 37 security implementation prompt, phase 1 input sanitization.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: completion gate is present and needs its own handling; skill 37 security implementation prompt is present and needs its own handling; phase 1 input sanitization is present and needs its own handling; phase 2 scope verification is present and needs its own handling.

B: Create work pieces for completion gate, skill 37 security implementation prompt, phase 1 input sanitization, phase 2 scope verification. Keep completion gate and skill 37 security implementation prompt separate. Allow phase 1 input sanitization and phase 2 scope verification to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, skill 37 security implementation prompt, phase 1 input sanitization, phase 2 scope verification.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: skill 37 security implementation prompt is present and needs its own handling; phase 1 input sanitization is present and needs its own handling; phase 2 scope verification is present and needs its own handling; phase 3 owasp compliance is present and needs its own handling.

B: Create work pieces for skill 37 security implementation prompt, phase 1 input sanitization, phase 2 scope verification, phase 3 owasp compliance. Keep skill 37 security implementation prompt and phase 1 input sanitization separate. Allow phase 2 scope verification and phase 3 owasp compliance to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: skill 37 security implementation prompt, phase 1 input sanitization, phase 2 scope verification, phase 3 owasp compliance.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for Skill 37: Security — Implementation Prompt: phase 1 input sanitization is present and needs its own handling; phase 2 scope verification is present and needs its own handling; phase 3 owasp compliance is present and needs its own handling; phase 4 security monitoring is present and needs its own handling.

B: Create work pieces for phase 1 input sanitization, phase 2 scope verification, phase 3 owasp compliance, phase 4 security monitoring. Keep phase 1 input sanitization and phase 2 scope verification separate. Allow phase 3 owasp compliance and phase 4 security monitoring to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: phase 1 input sanitization, phase 2 scope verification, phase 3 owasp compliance, phase 4 security monitoring.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

