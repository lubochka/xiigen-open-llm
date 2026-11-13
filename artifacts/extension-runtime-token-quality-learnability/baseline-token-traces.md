# TOKEN-03 baseline token traces

Verdict: PASS

## What this proves

- Baseline traces were generated for the fixed TOKEN-01 scenarios across Claude, Cursor, and Codex host names.
- Adaptive mode was disabled for these traces.
- The TOKEN-02 heuristic accounting identity is used consistently.
- Prompt and visible answer are represented by hashes in the trace rows.

## What this does not prove

- This does not prove adaptive improvement, token savings, semantic quality, DPO quality, checkpoint readiness, or local learnability.
- This does not claim host chat UI runtime for this phase; it is a baseline trace collection over the core extension API boundary.

## Counts

- Trace rows: 96
- Claude: 32
- Cursor: 32
- Codex: 32

## Next independent track

TOKEN-04 should collect adaptive traces with the same scenario ids, pair ids, hosts, methods, debug settings, and tokenizer/accounting identity.
