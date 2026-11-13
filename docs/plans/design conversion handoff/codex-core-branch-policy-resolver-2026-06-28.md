# CORE_BRANCH_POLICY_RESOLVER - 2026-06-28

claim_scope:
- Bounded core branch policy resolver packet for `DesignToCode-clean`, `DesignToCode`, and latest updated branch authority around the core repository.
- Created one report-only resolver packet.
- Classified safe now vs blocked branch actions.
- No branch mutation.

evidence_scope:
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-preflight-resolution-sweep-2026-06-28.md`.
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-handoff-index-2026-06-28.md`.
- Read the handoff folder listing to locate the index.
- Ran read-only git metadata checks in `C:\Projects\llm_mvp_core`: current branch, HEAD SHA, local main/origin-main SHAs, merge-base, ahead/behind count, local heads, remote branch query, and tracked-file dirty summary.
- Did not run tests, builds, browser/runtime checks, branch creation, branch switch, branch rename, merge, rebase, push, force push, PR mutation, staging, commit, cleanup, restore, stash, external AI, or secret access.

count_claimed:
- resolver packets created: 1
- report files created: 1
- branch mutations executed: 0
- PR mutations executed: 0

independent_evidence_count:
- 8 usable current-turn read-only evidence records counted.
- Timed-out read-only git commands are not counted as evidence.

subagent_id_or_NONE:
- CORE_BRANCH_POLICY_RESOLVER

downgrade_needed:
- Yes for any broader claim. The evidence supports only report-only branch policy routing and current observed ref metadata. It does not prove merge readiness, PR readiness, clean repo state, product completion, full runtime proof, full visual proof, branch-renaming safety, or cross-repo freshness beyond the read documents and current core git metadata.

## Required Policy Statement

The branch policy requirement recorded by this resolver is:

- all participating repos use the `latest updated branch as authority` when choosing the current work authority;
- the shared working name for a new or normalized branch is `DesignToCode`;
- the current core exception and actual core branch evidence is `DesignToCode-clean`;
- any mutating branch action without an exact policy selector defaults to `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`;
- this packet performs `no branch mutation`.

This means `DesignToCode` is the intended cross-repo working name, while the current core evidence still records `DesignToCode-clean` as the actual core PR branch and current local branch.

## Source Evidence Read

From `codex-preflight-resolution-sweep-2026-06-28.md`:

- Packet family 2 was originally blocked by `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` because the branch policy selector and preservation/ref-mutation policy were missing.
- Conservative/default branch routing selected `keep DesignToCode-clean`.
- The preflight sweep recorded local `DesignToCode` and `DesignToCode-with-luba-evidence` at `3e47d7a22ae968a3e2e5abb21ed9bc057854ec27`.
- The preflight sweep recorded local and remote `DesignToCode-clean` at `642b67c2bb2330b35a2b5a4e3972f7c6657abff3`.
- It explicitly left non-default branch/ref actions blocked until exact source ref, destination ref, local/remote scope, mutation approval, preservation/rollback ref, push permission, and PR update permission are supplied.

From `codex-designconversion-handoff-index-2026-06-28.md`:

- Core PR branch is listed as `DesignToCode-clean`.
- Platform, flows, figma, and canva PR branches are listed as `DesignToCode`.
- Latest pushed refs list core `origin/DesignToCode-clean` at `642b67c2bb2330b35a2b5a4e3972f7c6657abff3`.
- The index records `bounded-core-branch-policy-worker` as future work only after preservation and mutation rules are supplied.

## Current Core Git Metadata

Read-only metadata observed in `C:\Projects\llm_mvp_core`:

| Field | Observed value |
| --- | --- |
| Current branch | `DesignToCode-clean` |
| Current HEAD | `642b67c2bb2330b35a2b5a4e3972f7c6657abff3` |
| Local `main` | `cfa6a1e95e1635aa3540dda369b875c684d8b10e` |
| Local `origin/main` | `020e6eba3c090f441cdda76317c128fcaa1bcbdb` |
| Merge-base of HEAD and `origin/main` | `385898c8f8050944d23b1266b890e85b1816d25c` |
| `origin/main...HEAD` count | behind `4`, ahead `308` |
| Local `DesignToCode` | `3e47d7a22ae968a3e2e5abb21ed9bc057854ec27` |
| Local `DesignToCode-clean` | `642b67c2bb2330b35a2b5a4e3972f7c6657abff3` |
| Local `DesignToCode-with-luba-evidence` | `3e47d7a22ae968a3e2e5abb21ed9bc057854ec27` |
| Remote query returned | `origin/DesignToCode-clean` at `642b67c2bb2330b35a2b5a4e3972f7c6657abff3` |
| Remote query did not return | `origin/DesignToCode`, `origin/DesignToCode-with-luba-evidence` |

Tracked-file dirty summary with untracked files hidden:

```text
## DesignToCode-clean...origin/DesignToCode-clean
 M docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json
 M docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl
```

Boundary: this is a tracked-file summary only. A broader untracked dirty count command timed out and is not counted as evidence.

## Resolver Decision

Packet id:
- `CORE_BRANCH_POLICY_RESOLVER`

Conservative current routing:
- `CORE_BRANCH_POLICY_KEEP_DESIGNTOCODE_CLEAN_NOOP`

Authority rule:
- Use `latest updated branch as authority` for branch policy decisions across participating repos.
- For a new or normalized branch, the requested working name is `DesignToCode`.
- For current core evidence, `DesignToCode-clean` remains the actual branch and current PR/ref evidence.

Default without selector:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no branch mutation`

Reason:
- The work order requires preserving the latest updated branch authority rule and the `DesignToCode` working-name target, while recording current core `DesignToCode-clean` as evidence.
- The read documents and git metadata do not supply an exact selector for renaming, creating, switching, force pushing, merging/rebasing, or PR retargeting.
- Therefore this resolver can classify actions and prepare future work orders, but cannot authorize mutation.

## Safe Now

Safe now under this packet:

1. Record branch policy evidence in this report.
2. Preserve current core branch topology.
3. Treat `DesignToCode-clean` as current actual core evidence until a selector says otherwise.
4. Treat `DesignToCode` as the intended working name for any future new/normalized branch.
5. Use current local/remote read-only metadata for future decision prep.
6. Return `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` for any mutating branch action without exact selector.
7. Continue with `no branch mutation`.

## Blocked Actions

| Action | Current classification | Required exact selector before action |
| --- | --- | --- |
| Branch rename | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Rename source, destination, local/remote scope, preservation ref, rollback instruction, push permission, PR update permission. |
| Branch creation | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Source SHA/ref, destination branch name, local/remote scope, collision policy, preservation/rollback instruction, push permission. |
| Branch switch | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Target branch, dirty-worktree handling policy, confirmation that switching is allowed, rollback instruction. |
| Force push | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Source SHA, remote ref, lease policy, explicit force-push approval, rollback ref, PR impact acceptance. |
| Merge/rebase | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Base/ref pair, selected operation, conflict policy, dirty-worktree policy, test/build policy if required, rollback instruction. |
| PR retarget | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | PR number, new base/head target, expected current PR head SHA, retarget approval, post-mutation verification. |

No blocked action was executed.

## Future Bounded Work Orders

### Work Order 1 - Branch Policy Selector Prep

Worker:
- `bounded-core-branch-policy-selector-prep-worker`

Scope:
- Read the current handoff/index and core git metadata.
- Produce an owner-facing selector with options:
  - keep `DesignToCode-clean` as core exception;
  - create or move `DesignToCode` from latest authority SHA;
  - rename `DesignToCode-clean` to `DesignToCode`;
  - preserve both branches and retarget later;
  - defer all mutation.

Required output:
- exact consequences;
- exact source and destination refs;
- rollback/preservation refs;
- PR retarget need or no-need;
- branch mutation risk;
- default if no selection remains `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`.

Permissions:
- read-only only unless a later work order explicitly grants mutation.

### Work Order 2 - Branch Mutation Worker

Worker:
- `bounded-core-branch-policy-mutation-worker`

Precondition:
- Luba supplies exact selector and mutation approval after reviewing Work Order 1 output.

Allowed only if explicitly granted:
- create preservation ref;
- create branch;
- rename branch;
- switch branch;
- merge/rebase;
- push or force push with lease;
- PR retarget.

Required verification:
- before/after local refs;
- before/after remote refs;
- current branch;
- HEAD SHA;
- PR metadata if PR mutation is approved;
- rollback evidence.

Default if any selector is missing:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no branch mutation`

### Work Order 3 - Cross-Repo Branch Authority Reconciliation

Worker:
- `bounded-cross-repo-branch-authority-reconciliation-worker`

Scope:
- Read-only metadata across participating repos.
- Confirm which branch is latest updated and ahead of main in each repo.
- Compare actual PR branches against intended shared working name `DesignToCode`.
- Record exceptions such as core `DesignToCode-clean`.

Required output:
- per-repo branch authority table;
- latest updated branch as authority evidence;
- ahead/behind relative to main;
- PR branch evidence;
- safe/no-op default;
- mutation packet requirements if any branch should be normalized.

Permissions:
- no mutation unless separately approved.

## Final Boundary

This packet records:

- `CORE_BRANCH_POLICY_RESOLVER`
- `latest updated branch as authority`
- `DesignToCode`
- `DesignToCode-clean`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no branch mutation`

It does not claim:

- branch rename completed;
- branch creation completed;
- branch switch completed;
- force push completed;
- merge/rebase completed;
- PR retarget completed;
- clean repository;
- merge readiness;
- product completion;
- current cross-repo freshness beyond the read documents and current core git metadata.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-core-branch-policy-resolver-2026-06-28.md`

verification:
- Read target preflight sweep: yes.
- Read handoff index: yes.
- Read handoff folder listing: yes.
- Ran current branch/HEAD/status read-only metadata: yes.
- Ran local main/origin-main/merge-base/ahead-behind read-only metadata: yes.
- Ran local branch ref read-only metadata: yes.
- Ran remote selected-ref query: yes.
- Ran tracked-file dirty summary: yes.
- Ran tests/builds/browser/runtime: no.
- Mutated branches/PRs/git state: no.
