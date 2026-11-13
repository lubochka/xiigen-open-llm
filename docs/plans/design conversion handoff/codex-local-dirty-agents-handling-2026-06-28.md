# LOCAL_DIRTY_AGENTS_HANDLING - 2026-06-28

## Honesty Guard

claim_scope:
- Bounded local dirty `AGENTS.md` handling report for the original `DesignToCode-clean` worktree after the governance repair was pushed to `codex/chat-gpt-5.5`.
- Classified whether the dirty original AGENTS.md is duplicate, divergent, or contains additional local changes.
- Created this decision-prep/handling report only.

evidence_scope:
- Ran read-only `git status --short --branch -- AGENTS.md` in `C:\Projects\llm_mvp_core`.
- Ran read-only `git rev-parse --abbrev-ref HEAD`.
- Ran read-only `git diff -- AGENTS.md`.
- Ran read-only `git diff --cached -- AGENTS.md`.
- Ran read-only `git rev-parse codex/chat-gpt-5.5`.
- Ran read-only `git diff --name-status HEAD..codex/chat-gpt-5.5 -- AGENTS.md`.
- Ran read-only blob checks for worktree `AGENTS.md`, `HEAD:AGENTS.md`, and `codex/chat-gpt-5.5:AGENTS.md`.
- Ran read-only `git diff HEAD..codex/chat-gpt-5.5 -- AGENTS.md`.
- Ran read-only `git diff codex/chat-gpt-5.5 -- AGENTS.md`.
- Ran read-only local branch listing for `codex/chat-gpt-5.5` and `origin/codex/chat-gpt-5.5`.
- Read `codex-fresh-adversarial-full-chain-challenge-2026-06-28.md`.
- Read `codex-governance-branch-decision-prep-addendum-2026-06-28.md`.
- Checked that this target report path did not exist before creation.

count_claimed:
- 1 dirty `AGENTS.md` classification.
- 1 report file created.
- 0 cleanup/revert/stash/restore actions.
- 0 git mutations.
- 0 PR mutations.
- 0 tests/builds/browser/runtime/external AI calls.

independent_evidence_count:
- 15 pre-write evidence outputs.

subagent_id_or_NONE:
- LOCAL_DIRTY_AGENTS_HANDLING_WORKER

downgrade_needed:
- Yes for any claim beyond this bounded report.
- The evidence supports only: local `AGENTS.md` in `DesignToCode-clean` is dirty and divergent from `codex/chat-gpt-5.5`; mutation handling remains blocked unless explicitly authorized by a later bounded work order.

## Required Literals

- LOCAL_DIRTY_AGENTS_HANDLING
- dirty original AGENTS.md
- no cleanup/revert/stash/restore
- DesignToCode-clean
- codex/chat-gpt-5.5
- PACKET_BLOCKED_BY_PREFLIGHT_FAILURE

## Classification

classification:
- Divergent with duplicate-overlap, not a byte-identical duplicate.

details:
- Current branch is `DesignToCode-clean`.
- `git status --short --branch -- AGENTS.md` reports `M AGENTS.md`; the dirty original AGENTS.md is unstaged.
- `git diff --cached -- AGENTS.md` returned no staged diff.
- Worktree `AGENTS.md` blob hash: `19900c60362f66362d47a314d2be0cd22da3a233`.
- `HEAD:AGENTS.md` blob hash: `5bb0ae012271a785cfc863bc0a0cbdd2ac6a5716`.
- `codex/chat-gpt-5.5:AGENTS.md` blob hash: `5c73285c4f6391ed919ff1cffdbf8069f7c3d32c`.
- Local branch `codex/chat-gpt-5.5` and `origin/codex/chat-gpt-5.5` both point to commit `d33a294fca01457b9f5321290dd44544a1b285d0` in local branch listing.
- `git diff -- AGENTS.md` shows the local dirty change adds the `Class B continuation gate` block to the original worktree file.
- `git diff HEAD..codex/chat-gpt-5.5 -- AGENTS.md` shows the pushed repair branch changes `AGENTS.md` more broadly than the local dirty patch: it includes the Class B gate in a different location and also rewrites/removes/adds other governance sections.
- `git diff codex/chat-gpt-5.5 -- AGENTS.md` shows the local worktree does not contain the pushed branch's full `AGENTS.md` shape. If treated as replacement, it would drop branch-side governance text and restore older sections while keeping a local Class B insertion later in the file.

decision:
- The dirty original AGENTS.md is not safe to classify as a clean duplicate of the pushed repair.
- The dirty original AGENTS.md contains an overlapping Class B repair fragment, but the overall file is divergent from `codex/chat-gpt-5.5`.
- Any cleanup, revert, stash, restore, commit, port, cherry-pick, or overwrite would mutate the original worktree and is outside this worker's authority.

## Safest Default

exact safest default:
- Preserve the dirty original AGENTS.md in `DesignToCode-clean` untouched.
- Treat `codex/chat-gpt-5.5` as the pushed governance repair branch for the repo-chain evidence.
- Do no cleanup/revert/stash/restore.
- Do not stage, commit, push, port, cherry-pick, overwrite, or otherwise mutate `AGENTS.md` without a later explicit bounded work order.
- Record this classification report as the safe non-mutating action.

current blocker classification:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

why:
- The local dirty file and pushed branch file are different blobs with materially different diff shapes.
- A destructive or mutating "resolution" could discard either the local dirty original AGENTS.md fragment or the broader pushed governance repair content.
- The current work order only authorizes documentation in this report.

## Decision-Prep Coverage

Class B mutation-required handling:
- Leave dirty original AGENTS.md untouched.
- Commit the local dirty Class B fragment on `DesignToCode-clean`.
- Revert/restore the local dirty AGENTS.md back to `HEAD`.
- Port/cherry-pick the full pushed `codex/chat-gpt-5.5` `AGENTS.md` repair into `DesignToCode-clean`.
- Preserve both versions by creating an explicitly authorized backup branch or comparison artifact.
- Defer AGENTS handling until branch/ref policy is resolved.

smallest owner decision needed:
- Select one exact handling policy for dirty original AGENTS.md on `DesignToCode-clean`, including whether mutation is allowed and which source ref, target ref, expected patch scope, and verification commands apply.

recommended default if no mutation is authorized:
- Leave the file dirty and documented.
- Continue using this report as the local-dirt handling packet.
- Do not claim clean repo proof or duplicate-resolution completion.

future bounded work order after decision:
- Worker: `bounded-dirty-agents-resolution-worker`.
- Preconditions: exact selector from Luba for leave/commit/revert/port/preserve/defer; exact source and target refs if porting; commit message policy if committing; explicit cleanup/revert/stash/restore permission if any of those actions are selected.
- Allowed only after that selector: perform the selected `AGENTS.md` action in the named worktree/ref scope.
- Required verification: before/after `git status --short --branch -- AGENTS.md`, before/after `git diff -- AGENTS.md`, before/after blob hashes for worktree/HEAD/source ref, and downgrade if branch/file state changed since this packet.
- Forbidden unless separately authorized: edits to `claude-*` files, tests/builds/browser/runtime/external AI, PR mutation, branch/ref mutation beyond the selected action, broad cleanup, unrelated file changes, commit/push without exact git authority.

## Class C

Class C yes/no:
- Yes.

Class C handled in this report:
- The safe non-mutating action was to document the dirty original AGENTS.md classification and safest default in this report.

Class C outside this write scope:
- Include or reference this report from the active handoff index/completion handoff if that inclusion is required by the active documentation-discoverability rules.
- Run a future read-only freshness recheck immediately before any later mutating `AGENTS.md` worker starts, because the dirty worktree or `codex/chat-gpt-5.5` branch may change after this packet.

Class B outside this write scope:
- Any actual cleanup/revert/stash/restore, commit, port, cherry-pick, branch/ref action, PR action, or worktree mutation.

## Verification

verification performed:
- Confirmed current worktree branch: `DesignToCode-clean`.
- Confirmed local dirty status: `M AGENTS.md`.
- Confirmed no staged `AGENTS.md` diff.
- Confirmed worktree, HEAD, and `codex/chat-gpt-5.5` blob hashes differ.
- Confirmed local dirty patch is narrower than the pushed branch repair and overlaps only the Class B gate content.
- Confirmed optional prior packets already classify dirty original `AGENTS.md` handling as decision-prep / `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

verification not performed:
- I did not run tests.
- I did not run builds.
- I did not run browser or runtime checks.
- I did not call external AI.
- I did not mutate git, PRs, branches, refs, or `AGENTS.md`.
- I did not clean/revert/stash/restore.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-local-dirty-agents-handling-2026-06-28.md`

final boundary:
- This report is decision-prep evidence only.
- It is not clean-repo proof, not duplicate-resolution completion, not product completion, not PR readiness, not merge readiness, not runtime proof, not visual fidelity proof, and not external AI judge proof.
