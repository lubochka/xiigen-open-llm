# TOKEN-03 Full Hosts Live Trace Capture

Verdict: PARTIAL_FULL_HOSTS_CAPTURE_GAPS_RECORDED_NO_TOKEN_CLAIM

Captured traces: 58/108. Pair results: 29/54. Quality scoring inputs: 58.

Counts by host/mode:
- Codex baseline=18, adaptive=18, pairs=18
- Cursor baseline=11, adaptive=11, pairs=11
- Claude baseline=0, adaptive=0, pairs=0

No-overclaim: token_savings_claim_allowed=false; quality_non_regression_passed=false; adaptive-unavailable rows=29; real adaptive model effect rows=0; training/eval/checkpoint mutation not started.

Host gaps:
- Codex: Full TOKEN-01 scenario pack captured through Codex adapter/core surface; real Codex chat UI and real adaptive model effect remain unproven.
- Cursor: Cursor Extension Host timed out at 240s after writing 22 physical rows (11 complete pairs); remaining full-pack rows and JSON runner summary are missing.
- Claude: Claude SessionStart hook launched, but the generated `token03-claude-full-pack-hook-runner.js` used CommonJS `require` under the repo `type=module` setting, producing `ReferenceError: require is not defined in ES module scope`; rerun with a `.cjs` runner or ESM-safe imports.

Next work order: TOKEN-03-CURSOR-CLAUDE-GAP-REPAIR first, then TOKEN-04-QUALITY-SCORING-CODEX-CURSOR-PARTIAL: rerun remaining Cursor rows with a resumable summary writer and rerun Claude hook runner as `.cjs`/ESM-safe; only after repair attempt, score captured real outputs with token_savings_claim_allowed=false.

Verification:
- JSON/JSONL parse: PASS (traces=58, pairs=29, quality_inputs=58)
- rg no-overclaim/live markers: PASS
- git diff --check: PASS path-scoped in core, Codex, Cursor, and Claude TOKEN-03 paths
- Product tests: NOT_RUN_NO_PRODUCT_HELPER_OR_SOURCE_CHANGED
