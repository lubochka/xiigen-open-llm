# FIGMA_NESTED_WORKTREE_RESOLVER - 2026-06-28

claim_scope:
- Bounded resolver packet for the `FigmaToCode-design-gen` nested worktree follow-up from `PREFLIGHT_RESOLUTION_SWEEP`.
- This packet classifies safe doc-only actions versus blocked nested worktree cleanup/delete/move/stash/restore/branch actions.
- This packet does not execute nested worktree cleanup, delete, move, stash, restore, branch, commit, push, PR, test, build, browser, runtime, or secret actions.

evidence_scope:
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-preflight-resolution-sweep-2026-06-28.md`.
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-handoff-index-2026-06-28.md`.
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-completion-handoff-2026-06-28.md`.
- Ran read-only file/listing and git metadata checks for `C:\Projects\FigmaToCode-design-gen` and `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`.
- Did not inspect `src/ui/components/App.tsx` content, did not inspect secrets, did not mutate files outside this packet, and did not run tests/builds/browser/runtime.

count_claimed:
- resolver packets created by this worker: 1
- safe doc-only actions classified: 5
- blocked nested worktree mutation classes classified: 9
- nested worktree mutations executed: 0

independent_evidence_count:
- 12 current-turn read-only evidence records used: preflight file metadata, handoff-folder file list, preflight read, handoff index read, completion handoff read, Figma repo existence metadata, Figma main status, Figma worktree list, Figma main diff name-status, nested worktree status, nested branch/head reads, and nested diff name-status.

subagent_id_or_NONE:
- FIGMA_NESTED_WORKTREE_RESOLVER

downgrade_needed:
- Yes for any broader claim. The evidence supports only a doc-only nested-worktree resolver packet and current read-only metadata. It does not prove owner intent, safe cleanup, repo cleanliness, merge readiness, product readiness, source correctness, or visual/runtime fidelity.

Required literals:
- `FIGMA_NESTED_WORKTREE_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no nested worktree cleanup`
- `no delete/move/stash/restore`
- `FigmaToCode-design-gen`

## Current Read-Only Snapshot

Observed parent repository:
- Path: `C:\Projects\FigmaToCode-design-gen`
- Branch from read-only status: `DesignToCode...origin/DesignToCode`
- Parent repo status showed:
  - `m .claude/worktrees/romantic-dewdney`
  - `M tests/fixtures/generated/cart-layout-report.json`
  - `M tests/fixtures/generated/login-layout-report.json`
  - `M tests/fixtures/generated/mail-layout-report.json`
- Parent repo `git diff --name-status` showed:
  - `M .claude/worktrees/romantic-dewdney`

Observed nested worktree:
- Path: `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`
- Worktree list entry: branch `refs/heads/claude/romantic-dewdney`
- Current branch: `claude/romantic-dewdney`
- Current HEAD: `e6a33a45d5377b248e598d8e61b10d3111da6143`
- Status: `claude/romantic-dewdney...origin/claude/romantic-dewdney [ahead 1]`
- Dirty path from status/diff name-status:
  - `M src/ui/components/App.tsx`

Boundary:
- The modified path name is known, but file content was not read by this worker.
- The ahead commit was not inspected by this worker.
- No nested worktree cleanup and no delete/move/stash/restore were performed.

## Preflight Linkage

Source packet family:
- Packet Family 9 - Figma Nested Worktree from `codex-preflight-resolution-sweep-2026-06-28.md`.

Original blocker:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing explicit worktree policy and git mutation/preservation inputs.

Preflight conservative/default route:
- selected_action: `preserve-exclude`
- selected target: `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`
- mutation policy: no stash, cleanup/reset, commit, push, restore, delete, or parent repo cleanup action.

This resolver preserves that conservative route and narrows future work into selectors. It does not clear the blocker for mutating actions.

## Safe Doc-Only Actions

The following actions are safe within a documentation-only resolver scope:

1. Record current read-only nested worktree metadata in a resolver packet.
2. Carry forward `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` for every non-default or mutating action.
3. Classify `FigmaToCode-design-gen` nested worktree cleanup as blocked pending owner/worktree policy.
4. List exact decisions/selectors required before any future nested worktree mutation.
5. Create future bounded work orders that require re-reading current state before acting.

These actions are documentation/accounting only. They do not mutate nested repos, branches, files, PRs, or runtime state.

## Blocked Actions

Blocked until exact selectors and owner/worktree policy are supplied:

| Action class | Status | Reason |
| --- | --- | --- |
| nested worktree cleanup/reset | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Dirty/ahead nested worktree may contain owner or other-model work. |
| delete nested worktree directory | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Destructive deletion needs explicit owner approval, backup/ref policy, and recovery plan. |
| move nested worktree directory | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Moving changes worktree location and can break repo metadata without an approved migration plan. |
| stash nested worktree changes | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Needs explicit stash selector, message, included paths, and owner acceptance of stash-only preservation. |
| restore nested worktree files | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Would discard or change dirty file state; needs explicit owner approval and backup policy. |
| branch switch/create/move | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Branch mutation needs target branch, preservation refs, and rollback/recovery instructions. |
| commit nested worktree changes | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Needs owner intent, exact paths, commit message, verification, and push/PR policy. |
| push nested branch | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Publishing needs target remote/branch, expected SHA, and owner approval. |
| parent repo cleanup of `.claude/worktrees/romantic-dewdney` | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Parent sees nested worktree as modified; cleanup is blocked until nested state is resolved. |

Hard boundary:
- no nested worktree cleanup
- no delete/move/stash/restore
- no branch/ref mutation
- no commit/push/PR mutation
- no tests/builds/browser/runtime
- no secrets

## Decisions And Selectors Needed

Before any future nested worktree mutation, Luba or an authorized owner decision packet must supply:

1. selected_worktree_policy:
   - one of `preserve-exclude`, `commit-push`, `stash`, `restore`, `cleanup-reset`, `delete-worktree`, or `move-worktree`.
2. selected_target:
   - exact path, currently observed as `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`.
3. owner_intent:
   - whether `claude/romantic-dewdney` and `src/ui/components/App.tsx` changes are owner work, model work to preserve, obsolete residue, or unknown.
4. exact_path_scope:
   - whether future action applies only to `src/ui/components/App.tsx`, the whole nested worktree, branch metadata, or parent repo status residue.
5. preservation_policy:
   - backup branch/ref/tag, patch export, stash message, archive path, or explicit no-backup waiver.
6. mutation_policy:
   - allowed git/file action and forbidden alternatives.
7. remote_policy:
   - whether pushes are allowed; target remote and branch; expected current local HEAD and expected remote HEAD.
8. rollback_or_recovery_policy:
   - exact recovery step if the future action fails or changes the wrong state.
9. verification_evidence:
   - commands and expected outputs for post-action status, refs, stash entry, branch state, parent repo status, and path existence.
10. stop_conditions:
   - stop if branch, HEAD, dirty path list, ahead/behind state, or parent repo status differs from this packet's expected snapshot unless the future work order explicitly accepts the changed snapshot.

## Future Bounded Work Orders

### Work Order A - Preserve/Exclude Confirmation

worker_id:
- `bounded-figma-nested-worktree-preserve-exclude-worker`

allowed actions:
- Read-only re-check of parent repo status, worktree list, nested status, nested branch/head, and nested diff name-status.
- Update a codex-prefixed documentation packet only if explicitly scoped.

forbidden actions:
- no nested worktree cleanup
- no delete/move/stash/restore
- no branch/ref mutation
- no commit/push/PR mutation
- no tests/builds/browser/runtime
- no secrets

exit evidence:
- current status snapshot and a documented preserve/exclude decision boundary.

### Work Order B - Owner Intent Decision Prep

worker_id:
- `bounded-figma-nested-worktree-owner-intent-decision-prep-worker`

allowed actions:
- Produce options, consequences, and required selectors for preserve, commit-push, stash, restore, cleanup/reset, delete, or move.
- Read only metadata unless explicit content-inspection permission is added.

forbidden actions:
- no nested worktree cleanup
- no delete/move/stash/restore
- no branch/ref mutation
- no commit/push/PR mutation
- no tests/builds/browser/runtime
- no secrets

exit evidence:
- owner decision matrix and smallest remaining explicit Luba decision.

### Work Order C - Mutating Nested Worktree Worker

worker_id:
- `bounded-figma-nested-worktree-mutation-worker`

precondition:
- May run only after Luba supplies all decisions/selectors above and explicitly authorizes the selected mutation class.

required first step:
- Re-read parent status, worktree list, nested status, nested branch/head, nested diff name-status, and expected remote/branch state before mutation.

allowed actions:
- Only the exact selected action from the approved work order.

forbidden without explicit selector:
- no nested worktree cleanup
- no delete/move/stash/restore
- no branch/ref mutation
- no commit/push/PR mutation

exit evidence:
- before/after status, refs, stash/commit/backup evidence if applicable, and downgrade notes for any unverified state.

## Final Boundary

This resolver packet keeps Packet Family 9 blocked for any non-default action:

- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- no nested worktree cleanup
- no delete/move/stash/restore
- `FigmaToCode-design-gen`

The evidence supports only:
- current read-only metadata for the observed nested worktree;
- safe doc-only routing;
- future selectors needed before any nested worktree mutation.

It does not support:
- owner intent;
- safe cleanup;
- deletion/move/stash/restore readiness;
- branch/ref mutation readiness;
- commit/push readiness;
- PR readiness;
- clean repo state;
- tests/builds/browser/runtime success;
- product completion.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-figma-nested-worktree-resolver-2026-06-28.md`

verification:
- Post-write readback by this worker confirmed file existence and required literals: `FIGMA_NESTED_WORKTREE_RESOLVER`, `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`, `no nested worktree cleanup`, `no delete/move/stash/restore`, and `FigmaToCode-design-gen`.
