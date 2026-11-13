# Node-03 - Scheme Planner Set

Runtime A is the collection of translated domain statements.
Runtime B is the plan of separated work pieces.

## Pair 3.1 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: architecture rule is present and needs its own handling; skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling.

B: Create work pieces for architecture rule, skill responsibility, factory boundary, configuration choice. Keep architecture rule and skill responsibility separate. Allow factory boundary and configuration choice to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.1 - Rephrased

A: The current request has translated responsibilities: architecture rule, skill responsibility, factory boundary, configuration choice.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.2 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: skill responsibility is present and needs its own handling; factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling.

B: Create work pieces for skill responsibility, factory boundary, configuration choice, dependency rule. Keep skill responsibility and factory boundary separate. Allow configuration choice and dependency rule to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.2 - Rephrased

A: The current request has translated responsibilities: skill responsibility, factory boundary, configuration choice, dependency rule.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.3 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: factory boundary is present and needs its own handling; configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling.

B: Create work pieces for factory boundary, configuration choice, dependency rule, completion gate. Keep factory boundary and configuration choice separate. Allow dependency rule and completion gate to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.3 - Rephrased

A: The current request has translated responsibilities: factory boundary, configuration choice, dependency rule, completion gate.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.4 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: configuration choice is present and needs its own handling; dependency rule is present and needs its own handling; completion gate is present and needs its own handling; pdf extraction ocr guide is present and needs its own handling.

B: Create work pieces for configuration choice, dependency rule, completion gate, pdf extraction ocr guide. Keep configuration choice and dependency rule separate. Allow completion gate and pdf extraction ocr guide to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.4 - Rephrased

A: The current request has translated responsibilities: configuration choice, dependency rule, completion gate, pdf extraction ocr guide.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.5 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: dependency rule is present and needs its own handling; completion gate is present and needs its own handling; pdf extraction ocr guide is present and needs its own handling; table of contents is present and needs its own handling.

B: Create work pieces for dependency rule, completion gate, pdf extraction ocr guide, table of contents. Keep dependency rule and completion gate separate. Allow pdf extraction ocr guide and table of contents to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.5 - Rephrased

A: The current request has translated responsibilities: dependency rule, completion gate, pdf extraction ocr guide, table of contents.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.6 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: completion gate is present and needs its own handling; pdf extraction ocr guide is present and needs its own handling; table of contents is present and needs its own handling; data ingestion overview is present and needs its own handling.

B: Create work pieces for completion gate, pdf extraction ocr guide, table of contents, data ingestion overview. Keep completion gate and pdf extraction ocr guide separate. Allow table of contents and data ingestion overview to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.6 - Rephrased

A: The current request has translated responsibilities: completion gate, pdf extraction ocr guide, table of contents, data ingestion overview.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.7 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: pdf extraction ocr guide is present and needs its own handling; table of contents is present and needs its own handling; data ingestion overview is present and needs its own handling; llm assisted data pipeline is present and needs its own handling.

B: Create work pieces for pdf extraction ocr guide, table of contents, data ingestion overview, llm assisted data pipeline. Keep pdf extraction ocr guide and table of contents separate. Allow data ingestion overview and llm assisted data pipeline to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.7 - Rephrased

A: The current request has translated responsibilities: pdf extraction ocr guide, table of contents, data ingestion overview, llm assisted data pipeline.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

## Pair 3.8 - Plan Work Pieces

A: Translated domain statements for PDF Extraction & OCR Guide: table of contents is present and needs its own handling; data ingestion overview is present and needs its own handling; llm assisted data pipeline is present and needs its own handling; text document extraction is present and needs its own handling.

B: Create work pieces for table of contents, data ingestion overview, llm assisted data pipeline, text document extraction. Keep table of contents and data ingestion overview separate. Allow llm assisted data pipeline and text document extraction to move in parallel when they do not depend on each other. Add a wait state before merge, a review point before formatting, and a recovery path that sends only the failed fragment back through the flow.

Pair 3.8 - Rephrased

A: The current request has translated responsibilities: table of contents, data ingestion overview, llm assisted data pipeline, text document extraction.

B: Plan one branch per responsibility. Preserve order only where one branch needs another branch's result. Keep the merge explicit. Do not hide review, waiting, or repair work inside a general summary.

