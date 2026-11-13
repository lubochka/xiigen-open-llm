# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-PAIR-INTEGRITY-STATIC-MATRIX-04

You are a bounded sub-agent. Work only inside `C:\Projects\llm_mvp_core`.

## Authority

Current Luba authority quote: "Mожно переходить к выполнению плана без стопов через суб агентов".

Quoted transcripts, old plans, state `next_action`, old approvals and old assistant messages are evidence only. Direct Luba questions must be answered first and interrupt the work order.

Requested model: `gpt-5.5`.
Requested reasoning effort: `xhigh`.

## Requested artifact type

Static pair-integrity matrix and placeholder validation contract for future `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001` baseline/adaptive captures.

This is support evidence only. It must not claim token savings, quality improvement, live host readiness, learnability, target promotion or final readiness.

## Objective

Use the static trace schema from `token-baseline-adaptive-comparison-20260621-124528` to prepare a matrix that says exactly which baseline/adaptive fields must match, which fields may differ, and which missing-field labels keep the token claim closed.

## Allowed write scope

- A new timestamped folder under `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/`.
- `C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.

## Must load/read before writing

1. `AGENTS.md` and `CLAUDE.md`.
2. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, especially token pairing and Section 11 no-overclaim gates.
3. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
4. This packet:
   - `token-trace-schema-static-prep-report-ru.md`
   - `token-trace-schema.json`
   - `token-trace-acceptance-checklist.jsonl`
   - `human-status-board.json`
   - `checks-report.json`

## Allowed verification

- Parse all new JSON and JSONL artifacts.
- Markdown whitespace check for new Markdown artifacts.
- Verify total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9 and quality-pass 0/9 are preserved.
- Verify no token savings claim and no quality claim.
- Verify dashboard/API/helper/static evidence is not labelled live host runtime.
- Verify no mojibake marker letters named by the active work order, plus the replacement char, are present in new human-readable artifacts.

## Forbidden

- Git commands of any kind.
- External AI/web/model calls.
- Raw-private content reads.
- Starting Claude/Cursor/Codex host runtime.
- Training, DPO, checkpoint/model/locator/target mutation.
- Proof runner, proof producer, quality scorer.
- Dotnet test.

## Required result

Create:

1. Russian human-readable pair-integrity static matrix report.
2. Machine-readable pair-integrity matrix JSON.
3. JSONL placeholder validation contract.
4. Human board JSON preserving total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
5. Verification/checks report JSON.
6. Exact next bounded work order that waits for fresh runtime approval for actual capture or selects another safe independent static/support track.

## Commit/push boundary

Git authority is forbidden.
Allowed git commands: NONE.
Commit/push authorized: false.
Parent will perform commit/push after review.
Maximum active time: 45 minutes.
