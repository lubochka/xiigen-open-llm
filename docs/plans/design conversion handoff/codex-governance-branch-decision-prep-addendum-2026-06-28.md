# GOVERNANCE_BRANCH_DECISION_PREP_ADDENDUM - 2026-06-28

## Honesty Guard

claim_scope:
- Bounded decision-prep addendum for the remaining post-push Class B items after the `codex/chat-gpt-5.5` governance repair push audit.
- Created one documentation packet only.
- Covered three requested blockers: PR creation/mutation for `codex/chat-gpt-5.5`, dirty original `AGENTS.md` on `DesignToCode-clean`, and branch/ref reconciliation between `DesignToCode`, `DesignToCode-clean`, and latest authority `codex/chat-gpt-5.5`.

evidence_scope:
- Read `codex-post-push-downstream-audit-2026-06-28.md`.
- Read `codex-class-b-decision-prep-coverage-audit-2026-06-28.md`.
- Read `codex-core-branch-policy-resolver-2026-06-28.md`.
- Read `codex-pr-state-action-resolver-2026-06-28.md`.
- Checked that this target file did not exist before creation.
- Did not independently re-run git remote checks, GitHub PR checks, tests, builds, browser/runtime checks, visual checks, external AI judge checks, or clean-repo checks.

count_claimed:
- 3 remaining Class B blockers converted into smallest-decision packets.
- 1 report file created.
- 0 git mutations.
- 0 PR mutations.
- 0 tests/builds/browser/runtime/external AI calls.

independent_evidence_count:
- 5 current tool evidence records: four source report reads and one target absence check.

subagent_id_or_NONE:
- GOVERNANCE_BRANCH_DECISION_PREP_ADDENDUM_WORKER

downgrade_needed:
- Yes for any claim beyond this narrow decision-prep packet.
- The evidence supports only: same-day source packets identify these three items as Class B decision blockers and this addendum supplies smallest owner decisions, safe defaults, no-decision consequences, future bounded work orders, and no-overclaim boundaries.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-governance-branch-decision-prep-addendum-2026-06-28.md`

## Required Literals

- GOVERNANCE_BRANCH_DECISION_PREP_ADDENDUM
- PACKET_BLOCKED_BY_PREFLIGHT_FAILURE
- Class B requires decision-prep or mitigation sub-agents before final answer
- No final answer after Class B unless smallest-decision packet exists for each blocker
- exact PR action/title/body/draft policy
- dirty original AGENTS.md
- branch/ref reconciliation

## Preserved Boundaries

This packet is:
- not product completion
- not PR readiness
- not clean-repo proof
- not runtime proof
- not visual fidelity proof
- not external AI judge proof

This packet does not authorize mutation. Any PR creation, PR update, branch/ref action, cleanup, revert, commit, stage, push, or runtime verification remains blocked until a later bounded worker receives the exact owner selector and its own preflight passes.

## Source-Evidence Summary

The post-push downstream audit records:
- remote ref `codex/chat-gpt-5.5` exists at commit `5d10f8b1e4074158a3215771c19fe0975919c21c`;
- no open PR exists for head `codex/chat-gpt-5.5`, while historical merged PRs #2-#5 exist for that head name;
- current original worktree branch is `DesignToCode-clean`;
- `AGENTS.md` is dirty in the original worktree and the dirty patch shape matches the 17-line `Class B continuation gate` repair, but the whole file is not byte-identical to the pushed governance branch version because branch bases differ;
- `DesignToCode-clean` is current actual core evidence, `DesignToCode` is intended cross-repo working name, and `codex/chat-gpt-5.5` is the newly pushed governance repair branch.

The branch policy resolver records:
- current core actual branch evidence remains `DesignToCode-clean`;
- `DesignToCode` is the intended normalized working name;
- branch/ref mutations require exact selector and policy;
- default without selector is `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` and `no branch mutation`.

The PR state action resolver records:
- mutating PR actions require an exact PR action selector, selected PRs, exact title/body or patch policy where applicable, expected head SHA policy, and before/after verification;
- default without selector is `no PR mutation`;
- mark-ready, merge, close/reopen, review request, and title/body changes remain `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

The Class B coverage audit records:
- `Class B requires decision-prep or mitigation sub-agents before final answer`;
- Class B blockers require smallest owner decision coverage;
- decision-prep coverage is not product completion, merge readiness, clean-repo proof, runtime proof, visual fidelity proof, or external AI judge proof.

## Decision Packet 1 - PR Creation Or Mutation For `codex/chat-gpt-5.5`

blocker:
- PR creation/mutation for `codex/chat-gpt-5.5` is blocked by missing exact PR action/title/body/draft policy.

current classification:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

exact owner decision still needed:
- Choose the exact PR action for branch `codex/chat-gpt-5.5`:
  - create a new draft PR to `main`;
  - create a ready-for-review PR to `main`;
  - leave the branch without an open PR;
  - update an existing PR only if a specific existing PR number is selected and GitHub allows the action;
  - close/defer any PR path for this governance repair.
- If creating or updating a PR, provide exact PR action/title/body/draft policy:
  - base branch;
  - head branch;
  - exact title;
  - exact body or exact body patch text;
  - draft versus ready state;
  - expected head SHA policy for `5d10f8b1e4074158a3215771c19fe0975919c21c`;
  - whether historical merged PRs #2-#5 affect naming/body wording.

safe default:
- `no PR mutation`
- Leave `codex/chat-gpt-5.5` as a pushed governance repair branch with no open PR until Luba selects exact PR action/title/body/draft policy.

consequence of no decision:
- The governance branch remains pushed but not represented by a new open PR according to the post-push audit evidence.
- Final handoff must not claim PR readiness, review readiness, merge readiness, or completed PR creation for `codex/chat-gpt-5.5`.
- Any worker asked to create or mutate a PR without the exact policy must return `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

future bounded work order after decision:
- Worker: `bounded-governance-pr-action-worker`.
- Preconditions: Luba supplies selected action, base/head, exact title/body or patch, draft policy, expected head SHA policy, and post-mutation verification fields.
- Allowed after approval only: create or update the selected PR metadata exactly as specified.
- Required verification: before/after PR metadata, head SHA, base/head refs, title/body/draft state, URL if created, and explicit no-merge/no-readiness boundary unless separately approved.
- Forbidden unless separately authorized: merge, mark-ready without ready policy, request reviewers, close/reopen unrelated PRs, branch mutation, git mutation, tests/builds/runtime/browser/external AI.

no-overclaim boundary:
- This packet does not create or update a PR.
- This packet is not PR readiness, not merge readiness, not product completion, not runtime proof, not visual fidelity proof, and not external AI judge proof.

## Decision Packet 2 - Dirty Original `AGENTS.md` On `DesignToCode-clean`

blocker:
- Cleanup, commit, revert, port, or preservation of the dirty original AGENTS.md on `DesignToCode-clean` is blocked by missing exact selector.

current classification:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

exact owner decision still needed:
- Choose the exact handling selector for the dirty original `AGENTS.md` on `DesignToCode-clean`:
  - leave it untouched as local dirty state;
  - commit the 17-line governance repair on `DesignToCode-clean`;
  - port/cherry-pick from `codex/chat-gpt-5.5` only after exact source/target policy;
  - revert/clean the dirty `AGENTS.md` local change;
  - preserve both branch versions and document divergence;
  - defer until a later governance branch policy decision.
- If committing, porting, or reverting, provide exact source ref, target ref, expected patch scope, commit message policy if any, cleanup/revert approval, and verification requirement.

safe default:
- Leave the dirty original `AGENTS.md` untouched.
- Do not clean, revert, commit, stage, stash, restore, port, or cherry-pick without the exact owner selector.

consequence of no decision:
- The original worktree remains dirty for `AGENTS.md` under the post-push audit evidence.
- Final handoff must not claim clean repo proof, cleanup completion, revert completion, or duplicate-resolution completion.
- Later repo-gate, clean-worktree, or commit-boundary workers must treat the dirty original `AGENTS.md` as unresolved Class B until Luba selects handling.

future bounded work order after decision:
- Worker: `bounded-dirty-agents-handling-worker`.
- Preconditions: Luba supplies exact handling selector, source/target refs if applicable, dirty-worktree policy, commit/revert/port authorization if applicable, and verification commands.
- Allowed after approval only: the selected handling action for `AGENTS.md` in the specified worktree/ref scope.
- Required verification: before/after `git status --short -- AGENTS.md`, before/after diff summary, exact resulting branch/ref/HEAD if git mutation is approved, and a downgrade if the worktree has changed.
- Forbidden unless separately authorized: edits to claude-* files, broad cleanup, unrelated file changes, tests/builds/runtime/browser/external AI, PR mutation, branch/ref mutation beyond the selected action.

no-overclaim boundary:
- This packet does not clean, revert, commit, stage, stash, restore, port, cherry-pick, or edit `AGENTS.md`.
- This packet is not clean-repo proof, not product completion, not PR readiness, not runtime proof, not visual fidelity proof, and not external AI judge proof.

## Decision Packet 3 - Branch/Ref Reconciliation

blocker:
- Branch/ref reconciliation between `DesignToCode`, `DesignToCode-clean`, and latest authority `codex/chat-gpt-5.5` is blocked by missing exact selector/policy.

current classification:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`

exact owner decision still needed:
- Choose the exact reconciliation policy:
  - keep `DesignToCode-clean` as the current core evidence branch and keep `codex/chat-gpt-5.5` as a separate governance repair branch;
  - create a PR for `codex/chat-gpt-5.5` to `main` while leaving `DesignToCode-clean` unchanged;
  - port/cherry-pick the governance repair into `DesignToCode-clean`;
  - rename or recreate a normalized `DesignToCode` branch from a selected source ref;
  - preserve both `DesignToCode` and `DesignToCode-clean` and document the exception;
  - defer all branch/ref mutation.
- If any mutation is chosen, provide exact source ref, destination ref, local/remote scope, collision policy, preservation/rollback ref, push permission, expected SHA policy, and PR retarget/update policy.

safe default:
- `no branch mutation`
- Preserve current branch topology as documented by the source packets:
  - `DesignToCode-clean` remains the actual current core evidence branch;
  - `DesignToCode` remains the intended normalized working name;
  - `codex/chat-gpt-5.5` remains a separate governance repair branch.

consequence of no decision:
- Branch/ref reconciliation remains unresolved Class B.
- Final handoff must not claim branch-name reconciliation completion, cross-repo branch normalization, PR retarget completion, clean repo proof, or merge readiness.
- Future mutating workers must return `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` until exact selector/policy exists.

future bounded work order after decision:
- Worker: `bounded-governance-branch-reconciliation-worker`.
- Preconditions: Luba supplies exact reconciliation selector, source/destination refs, local/remote scope, preservation/rollback refs, push/force-push policy if any, dirty-worktree policy, and PR retarget/update policy if any.
- Allowed after approval only: the selected branch/ref reconciliation action.
- Required verification: before/after local refs, before/after remote refs, current branch, HEAD SHA, preservation/rollback ref evidence, and PR metadata if PR retarget/update is approved.
- Forbidden unless separately authorized: force push without explicit force-with-lease policy, cleanup/revert of dirty files, PR merge, tests/builds/runtime/browser/external AI, or mutation of unrelated repositories.

no-overclaim boundary:
- This packet performs no branch/ref reconciliation.
- This packet is not clean-repo proof, not PR readiness, not merge readiness, not product completion, not runtime proof, not visual fidelity proof, and not external AI judge proof.

## Class C Check

GOVERNANCE_BRANCH_DECISION_PREP_ADDENDUM_FOUND_NO_NEW_IMPLEMENTABLE_SAFE_FOLLOW_UPS

No new implementable safe follow-up was found inside this addendum's bounded scope. The implementable doc-only follow-up from the post-push audit was this addendum itself, and it is now represented by this packet. All three remaining items above still require Luba's smallest owner decision before any mutation worker can proceed.

## Final Boundary

No final answer after Class B unless smallest-decision packet exists for each blocker.

This addendum supplies smallest-decision packet coverage for the three requested governance branch blockers only. It does not execute the decisions, does not mutate git or PR state, does not clean the worktree, and does not prove product readiness.

