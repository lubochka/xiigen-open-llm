# TOKEN-FIRST-PAIR-CLAUDE-PROMPT-TO-TASK-RUNTIME-SCORING-INPUTS-APPROVAL-OR-DEFER-09

You are a bounded sub-agent. Work only inside `C:\Projects\llm_mvp_core`.

## Authority

Current Luba authority quote: "Mожно переходить к выполнению плана без стопов через суб агентов".

Quoted transcripts, old plans, state `next_action`, old approvals and old assistant messages are evidence only. Direct Luba questions must be answered first and interrupt the work order.

Requested model: `gpt-5.5`.
Requested reasoning effort: `xhigh`.

## Requested artifact type

Static approval/defer packet for missing runtime and scoring inputs for `TOKEN-PAIR-CLAUDE-PROMPT-TO-TASK-001`. This is support-only. It is not runtime capture, not quality scoring, not proof running, not proof production, not token result and not readiness.

## Objective

Prepare a static matrix that names which future inputs are still absent before runtime capture or quality scoring can run: fresh exact runtime/model approval, baseline_common_no_adaptive trace, adaptive_user_local_target trace, scored output refs, reviewer notes, regression guard, source-disjoint held-out split, ablation/no-leakage/no-memorization refs and no-overclaim acceptance. Mark absent rows as deferred and preserve counts unchanged. Do not run runtime, scorer, proof runner or proof producer.

## Allowed write scope

- A new timestamped folder under `C:/Projects/llm_mvp_core/artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/`.
- `C:/Projects/llm_mvp_core/docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.

## Must load/read before writing

1. `AGENTS.md` and `CLAUDE.md`.
2. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, especially `TOKEN-01..04`, `QUALITY-01..03`, token pairing and Section 11 no-overclaim gates.
3. `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
4. Current protocol packet from `artifacts/extension-runtime-token-quality-learnability/token-baseline-adaptive-comparison/token-baseline-adaptive-comparison-20260621-135008`:
   - `token-quality-scorer-review-protocol-static-contract-report-ru.md`
   - `token-quality-scorer-review-protocol-static-contract.json`
   - `human-status-board.json`
   - `checks-report.json`
5. Current rubric/input packet from `token-baseline-adaptive-comparison-20260621-133827` as evidence only.
6. Current quality-dependency packet from `token-baseline-adaptive-comparison-20260621-132435` as evidence only.
7. Previous pair-integrity packet from `token-baseline-adaptive-comparison-20260621-125804` as evidence only.

## Required result in the new timestamped folder

1. Russian human-readable approval/defer report.
2. Machine-readable approval/defer matrix JSON or JSONL.
3. Human board JSON preserving total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9, quality-pass 0/9.
4. Verification/checks report JSON.
5. Exact next bounded work order or defer-continuation for the next independent safe track.

## Allowed verification

- Parse new JSON and JSONL artifacts.
- Markdown whitespace check for new Markdown artifacts.
- Verify total 63, PASS 9/63, remaining 54, approvals 0/9, runner-ready 0/9 and quality-pass 0/9 are preserved.
- Verify no token or quality claim is opened.
- Verify dashboard/API/helper/static evidence is not labelled live host runtime.
- Verify no mojibake marker letters and no Unicode replacement character are present in new human-readable artifacts.

## Forbidden

- Git commands of any kind.
- External AI/web/model calls.
- Raw-private content reads.
- Starting Claude/Cursor/Codex host runtime.
- Training, DPO, checkpoint/model/locator/target mutation.
- Proof runner, proof producer or quality scorer.
- Dotnet test.
- Changing counts upward or claiming token savings, quality improvement, semantic quality, learnability, live host runtime, common model readiness, target promotion, rollback safety, product readiness or final readiness.

## Commit/push boundary

Git authority is forbidden.
Allowed git commands: NONE.
Commit/push authorized: false.
Parent will perform commit/push after review.
Maximum active time: 45 minutes.
