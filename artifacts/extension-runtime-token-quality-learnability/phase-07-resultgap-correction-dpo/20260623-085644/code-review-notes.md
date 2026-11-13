# Code review notes

1. Missing-pattern writer schema: fixed chat-only risk by adding structured `ResultGapTrainableDpoCandidate`.
2. Privacy/source routing: fixed raw-ref acceptance by adding safe-ref validation and source quarantine.
3. Near-miss quality: fixed generic-negative risk with taxonomy and schema validation.
4. Evidence completeness: scoped lifecycle and dashboard claims to staged/core-only evidence.
5. P1 writer-level taxonomy gate: fixed safe-route `generic_bad_output` persistence by quarantining non-train-ready taxonomy before append.
6. P2 writer-level taxonomy evidence: added missing-pattern and correction writer-path coverage plus runner report rows.
