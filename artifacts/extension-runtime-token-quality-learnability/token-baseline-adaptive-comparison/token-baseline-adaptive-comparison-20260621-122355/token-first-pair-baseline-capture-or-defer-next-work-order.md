# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-BASELINE-CAPTURE-OR-DEFER-02

## Requested artifact type

Baseline capture authority check or defer packet for `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`. This is still bounded work: it must not claim token savings, quality improvement, live host readiness, learnability, target promotion or final readiness.

## Current carry-forward authority

The current packet authority is the quoted Luba instruction: "Mожно переходить к выполнению плана без стопов через суб агентов".

That quote authorizes bounded sub-agent execution, but it does not by itself authorize external AI/model calls, raw-private content reads, training, DPO, checkpoint/model/locator/target mutation, proof runner, quality scorer or git.

## Objective

Decide whether an exact `baseline_common_no_adaptive` capture can be performed for `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001` under fresh current authority. If capture authority is absent, create a defer packet instead of running Claude or any external model/runtime path.

## Allowed write scope

- A new timestamped folder under `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/`
- `C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Must load/read

1. `AGENTS.md` and `CLAUDE.md`.
2. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, especially token pairing and Section 11 no-overclaim gates.
3. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
4. This packet:
   - `token-baseline-companion-report-ru.md`
   - `pair-scope-matrix.json`
   - `human-status-board.json`
   - `checks-report.json`

## Allowed verification

- Parse new JSON artifacts.
- Markdown whitespace check for new Markdown artifacts.
- Verify total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9 and quality-pass 0/9 are preserved unless a later exact approved work order changes the board.
- Verify no token savings claim and no quality claim.
- Verify dashboard/API/helper/static evidence is not labelled live host runtime.

## Forbidden without fresh exact Luba approval

- Git commands.
- External AI/web/model calls.
- Raw-private content reads.
- Starting Claude/Cursor/Codex host runtime.
- Training, DPO, checkpoint/model/locator/target mutation.
- Proof runner, proof producer, quality scorer.
- Dotnet test unless a later exact work order authorizes a targeted test.

## Required result

Return either:

- a capture packet proving the exact authority, method, no-adaptive baseline contract and evidence scope before any capture occurs; or
- a defer packet saying baseline capture is not authorized yet.

In both cases, no token savings or quality claim is allowed.
