# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; owasp llm top 10 security patterns is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, owasp llm top 10 security patterns. Keep configuration choice and dependency rule separate. Allow completion gate and owasp llm top 10 security patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, owasp llm top 10 security patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; owasp llm top 10 security patterns is present and needs its own handling; sec 1 prompt injection detection is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, owasp llm top 10 security patterns, sec 1 prompt injection detection. Keep dependency rule and completion gate separate. Allow owasp llm top 10 security patterns and sec 1 prompt injection detection to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, owasp llm top 10 security patterns, sec 1 prompt injection detection.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: completion gate is present and needs its own handling; owasp llm top 10 security patterns is present and needs its own handling; sec 1 prompt injection detection is present and needs its own handling; overview is present and needs its own handling.

B: Create work pieces for completion gate, owasp llm top 10 security patterns, sec 1 prompt injection detection, overview. Keep completion gate and owasp llm top 10 security patterns separate. Allow sec 1 prompt injection detection and overview to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, owasp llm top 10 security patterns, sec 1 prompt injection detection, overview.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: owasp llm top 10 security patterns is present and needs its own handling; sec 1 prompt injection detection is present and needs its own handling; overview is present and needs its own handling; attack categories is present and needs its own handling.

B: Create work pieces for owasp llm top 10 security patterns, sec 1 prompt injection detection, overview, attack categories. Keep owasp llm top 10 security patterns and sec 1 prompt injection detection separate. Allow overview and attack categories to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: owasp llm top 10 security patterns, sec 1 prompt injection detection, overview, attack categories.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for OWASP LLM Top 10 Security Patterns: sec 1 prompt injection detection is present and needs its own handling; overview is present and needs its own handling; attack categories is present and needs its own handling; detection patterns is present and needs its own handling.

B: Create work pieces for sec 1 prompt injection detection, overview, attack categories, detection patterns. Keep sec 1 prompt injection detection and overview separate. Allow attack categories and detection patterns to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: sec 1 prompt injection detection, overview, attack categories, detection patterns.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

