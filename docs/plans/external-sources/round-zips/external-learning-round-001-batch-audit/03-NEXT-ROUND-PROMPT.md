# Next Round Prompt — External Learning Round 002

You are continuing the external-source learning-set preparation work.

Read first:

- C:\Users\lubao\Documents\xiigen\llm mvp\plan the learning set\EXAMINATION-CONTINUATION-PLAN-v1.0.md
- C:\Users\lubao\Documents\xiigen\llm mvp\external-sources\round-zips\external-learning-round-001-batch-audit\00-ROUND-MANIFEST.md
- C:\Users\lubao\Documents\xiigen\llm mvp\external-sources\round-zips\external-learning-round-001-batch-audit\02-VALIDATION-RESULTS.md
- C:\Users\lubao\Documents\xiigen\llm mvp\external-sources\round-zips\external-learning-round-001-batch-audit\evidence\batch-audit-summary.json

Goal for Round 002:

Normalize the external candidate rows into one canonical pre-promotion schema without importing them into local learning sets yet.

Hard rules:

- Do not promote any row with unknown, vague, or blocked license.
- Do not treat candidate rows as final learning pairs.
- Do not shorten accepted/rejected output text just to make parsing easier.
- Repair invalid JSON by preserving full content in valid escaped strings or by converting the row to canonical JSONL with structured text fields.
- Preserve source provenance and row identifiers.
- Produce a zip at the end of the round.

Required outputs:

- canonical-candidate-rows.jsonl
- held-license-review.jsonl
- blocked-license-rows.jsonl
- malformed-held-for-repair.jsonl
- normalization-report.md
- 00-ROUND-MANIFEST.md
- 01-FILES-INCLUDED.md
- 02-VALIDATION-RESULTS.md
- 03-NEXT-ROUND-PROMPT.md
- round zip

Round 002 closes only when every candidate row is either normalized, held for license review, blocked by license, or held as malformed with a specific repair reason.
