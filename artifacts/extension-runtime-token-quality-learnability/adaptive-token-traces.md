# TOKEN-04 adaptive token traces

Verdict: PASS_WITH_LIMITATION

## What this proves

- Adaptive-mode trace rows were generated for the same scenario ids, pair ids, host names, methods, debug settings, and tokenizer/accounting identity as TOKEN-03.
- Pair compatibility against baseline traces passed.
- Token delta alone still cannot pass quality.

## Boundary limitation

No user-local adaptive checkpoint was loaded in this phase. These rows prove adaptive-requested trace compatibility, not local model learnability or improvement.

## What this does not prove

- This does not prove token savings, semantic quality, DPO quality, checkpoint readiness, or local learnability.
- This does not claim host chat UI runtime for this phase; it is adaptive-requested trace collection over the core extension API boundary.

## Counts

- Trace rows: 96
- Claude: 32
- Cursor: 32
- Codex: 32
- Total token delta versus baseline: 72

## Next independent track

TOKEN-05 should produce the token delta report. It must keep this limitation visible and must not claim improvement while no user-local checkpoint was loaded.
