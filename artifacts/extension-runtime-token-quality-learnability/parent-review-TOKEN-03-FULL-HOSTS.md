# Parent Review: TOKEN-03 Full Hosts Partial Capture

Verdict: PASS_WITH_REPAIR_ACCEPTED_REVIEW_ONLY. Evidence is accepted as partial full-host capture only; full-host completion, token savings, quality improvement, semantic quality, learnability, training/checkpoint mutation, and full readiness remain unproven.

Review scope:
- subagent_id: TOKEN-03-LIVE-TRACE-CAPTURE-FULL-HOSTS
- review_role: bounded parent-review subagent
- implementation_performed: false
- commit_push_performed: false

Accepted evidence:
- Codex: 36 traces, 18 pairs, adapter/core surface only.
- Cursor: 22 traces, 11 pairs, partial Extension Host capture after timeout.
- Claude: 0 traces, 0 pairs.
- JSON/JSONL parse succeeded for report, state, traces, pair results, and quality-scoring inputs.
- Sample host evidence references exist for Codex/Cursor traces and response refs; report host run roots exist.
- No trace or pair row claims token savings, semantic quality, learnability, training, checkpoint mutation, quality non-regression, or full readiness.

Rejected or corrected evidence:
- Corrected `pairing_fields_preserved` from false to true because all 29 pair-result rows preserve pairing fields and `pairing_failures` is empty.
- Rejected next-action order that scored partial evidence before repair. Because full capture remains actionable and the user goal wants full host traces and quality, capture repair must precede TOKEN-04 partial scoring.
- Repaired Claude gap wording from vague inspection guidance to the exact failure: generated `.js` runner used CommonJS `require` under `type=module`, causing `ReferenceError: require is not defined in ES module scope`; rerun as `.cjs` or ESM-safe.

Exact gaps:
- Cursor: Extension Host run timed out at 240 seconds after 22 physical rows / 11 complete pairs; runner summary JSON is missing. Next repair needs resumable capture and summary writing.
- Claude: SessionStart hook launched, but full-pack runner failed before rows were produced due CommonJS `require` in a `.js` file under package `type=module`. Next repair needs `.cjs` runner or ESM-safe imports.
- Codex: full scenario pack captured through adapter/core only; real Codex chat UI and real adaptive model effect remain unproven.

Next work order:
1. TOKEN-03-CURSOR-CLAUDE-GAP-REPAIR: rerun remaining Cursor rows with resumable summary writer; rerun Claude hook runner as `.cjs` or ESM-safe. Preserve no-overclaim flags and do not score token savings.
2. TOKEN-04-QUALITY-SCORING-CODEX-CURSOR-PARTIAL: only after the repair attempt, score captured real outputs with `token_savings_claim_allowed=false` unless full quality and adaptive-effect gates are met.

Verification:
- JSON/JSONL parse: PASS; traces=58, pairs=29, quality inputs=58.
- Counts: PASS; Codex 36 traces/18 pairs, Cursor 22 traces/11 pairs, Claude 0.
- Pairing fields: PASS after report/state correction; pair rows with `pairing_fields_preserved=false` = 0.
- No-overclaim search: PASS for TOKEN-03 report/state and rows.
- Host evidence paths: PASS for report roots and sampled Cursor response/trace refs.
- Cursor gap marker: PASS; `token03-cursor-full-pack-summary.json` missing and report records timeout.
- Claude gap marker: PASS; host stdout records `ReferenceError: require is not defined in ES module scope` and recommends `.cjs`.
- `git diff --check`: PASS in core, Codex host repo, Cursor host repo, and TOKEN-03 path-scoped Claude host repo. Full Claude repo diff-check reports pre-existing RUNTIME-02 whitespace findings outside this TOKEN-03 review scope.
- Authorship gate: PASS; this review only authored parent-review artifacts plus allowed report/state wording corrections.

Changed files:
- artifacts/extension-runtime-token-quality-learnability/token-03-full-hosts-report.json
- artifacts/extension-runtime-token-quality-learnability/token-03-full-hosts-report.md
- docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json
- artifacts/extension-runtime-token-quality-learnability/parent-review-TOKEN-03-FULL-HOSTS.md
- artifacts/extension-runtime-token-quality-learnability/parent-review-TOKEN-03-FULL-HOSTS.json
