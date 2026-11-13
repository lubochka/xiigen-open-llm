# Review: QUALITY-02 Eval-only Runner Implementation Packet

Generated at local time: 2026-06-18T11:43:40+03:00

Reviewer: bounded REVIEW_INTERNAL sub-agent

Requested model: `gpt-5.5`

Requested reasoning effort: `xhigh`

## Verdict

FAIL.

The packet is mostly acceptable as an artifact/evidence-only implementation packet, but it cannot pass the current review because the dirty worktree contains the forbidden Cursor user data directory:

`artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/`

The packet checks artifact claims this path is absent from tracked diff and from the packet file set. That check is too narrow for the review question, because `git status --porcelain=v1` shows the forbidden directory as untracked and `Test-Path` returns `True`.

Parent may not commit this packet as accepted until the forbidden path is removed or quarantined outside the phase scope and the packet checks are corrected.

## Review Questions

| Question | Verdict | Evidence |
|---|---|---|
| Human-readable Russian board with total, done, current, remaining, result type, accepted evidence, closed claims with reasons, open gaps, next action, and execution-paused status | PASS | `implementation-packet.md` and `implementation-packet.json` include the board fields. |
| Clearly states 10 major plan blocks and current slice is only packet/artifact evidence, not product readiness | PASS | The board says there are 10 major plan blocks and the scope/does-not-prove sections keep product readiness closed. |
| Exact next work order names files/classes, allowed/forbidden scope, tests/checks, review scope, and boundaries | PASS | The next work order names expected files, records/classes, targeted tests, allowed verification, forbidden broad verification, git boundary, and stop-after-report boundary. |
| Keeps core logic in `llm_mvp_core` and host adapters thin | PASS | The work order requires `43.SmartSkillsDomain` ownership and thin console/host surfaces. |
| Prohibits hidden deterministic shortcuts and labels deterministic helpers as eval-only/helper/internal/baseline | PASS | The packet requires a hidden deterministic shortcut sweep and eval-only/helper/internal/baseline labels. |
| Preserves Direct Preference Optimization/source/training/eval boundaries and local model lifecycle requirements | PASS | The packet keeps actual eval, training, checkpoints, locators, rollback, common training, and final readiness closed; future lifecycle gates are named. |
| Keeps forbidden product claims closed | PASS | Token savings, quality improvement, semantic quality, learnability, common readiness, target promotion, locator update, and final readiness are all kept false/closed. |
| Worker avoided source code changes and forbidden Cursor user data | FAIL | Source-code changes were not seen in tracked diff, but forbidden `cursor-user-data/` exists as an untracked directory in the worktree. |
| JSON files parse and `git diff --check` passes | PASS | JSON parse passed for packet/checks/state. `git diff --check` exited with no output. |

## Checks Run

- JSON parse: PASS for `implementation-packet.json`.
- JSON parse: PASS for `implementation-packet-checks.json`.
- JSON parse: PASS for `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- `git diff --check`: PASS, no output.
- `git diff --name-only`: tracked diff contains only `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`.
- `git status --porcelain=v1`: FAIL for this review because it shows the forbidden untracked `cursor-user-data/` directory.
- `Test-Path` for the forbidden directory: `True`.

## Required Fixes

1. Remove the untracked forbidden Cursor user data directory from the dirty worktree, or quarantine it outside this phase with an explicit parent-approved evidence note.
2. Update `implementation-packet-checks.md` and `implementation-packet-checks.json` so the forbidden path check covers the full dirty worktree scope, not only tracked diff and the packet file set.
3. Rerun the allowed checks: JSON parse for packet/checks/state, `git diff --check`, `git diff --name-only`, and `git status --porcelain=v1`.
4. Only after the forbidden path is absent from the dirty scope may parent treat this packet as commit-ready artifact evidence.

## Parent Commit Guidance

Parent may not commit the reviewed packet as accepted in its current state. This review artifact itself may be kept as failure evidence. No product readiness is claimed.
