# FIGMA_FIXTURE_RESIDUE_RESOLVER - 2026-06-28

claim_scope:
- Bounded resolver packet for the `FigmaToCode-design-gen` fixture residue follow-up from `PREFLIGHT_RESOLUTION_SWEEP`.
- This packet classifies safe doc-only actions versus blocked fixture cleanup/delete/move/stash/restore/regenerate actions.
- This packet does not execute fixture cleanup, delete, move, stash, restore, regeneration, refresh-index, stage, commit, push, PR, test, build, browser, runtime, or secret actions.

evidence_scope:
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-preflight-resolution-sweep-2026-06-28.md`.
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-figma-nested-worktree-resolver-2026-06-28.md`.
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-handoff-index-2026-06-28.md`.
- Read `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-completion-handoff-2026-06-28.md`.
- Ran read-only file and git listing/status/diff checks for the three scoped fixture paths in `C:\Projects\FigmaToCode-design-gen`.
- Did not inspect fixture JSON contents, did not inspect secrets, did not mutate files outside this packet, and did not run tests/builds/browser/runtime.

count_claimed:
- resolver packets created by this worker: 1
- safe doc-only actions classified: 5
- blocked fixture mutation/action classes classified: 10
- fixture mutations executed: 0

independent_evidence_count:
- 13 current-turn read-only evidence records used: handoff folder listing, preflight read, nested resolver read, handoff index read, completion handoff read, target packet existence check, scoped fixture file listing, scoped fixture status with branch, scoped fixture `diff --numstat`, scoped fixture `diff --name-status`, scoped fixture `diff --raw`, scoped fixture `ls-files --stage`, and generated-fixture status listing.

subagent_id_or_NONE:
- FIGMA_FIXTURE_RESIDUE_RESOLVER

downgrade_needed:
- Yes for any broader claim. The evidence supports only a doc-only fixture residue resolver packet and current read-only metadata for the three scoped fixture reports. It does not prove fixture correctness, source-design fidelity, clean repo state, safe cleanup, merge readiness, product readiness, runtime behavior, or visual fidelity.

Required literals:
- `FIGMA_FIXTURE_RESIDUE_RESOLVER`
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- `no fixture cleanup`
- `no delete/move/stash/restore`
- `FigmaToCode-design-gen`

## Current Read-Only Snapshot

Observed repository:
- Path: `C:\Projects\FigmaToCode-design-gen`
- Branch from scoped read-only status: `DesignToCode...origin/DesignToCode`

Scoped fixture targets:
- `tests/fixtures/generated/cart-layout-report.json`
- `tests/fixtures/generated/login-layout-report.json`
- `tests/fixtures/generated/mail-layout-report.json`

Filesystem listing for scoped targets:

| Path | Observed size |
| --- | ---: |
| `tests/fixtures/generated/cart-layout-report.json` | 38275 bytes |
| `tests/fixtures/generated/login-layout-report.json` | 11492 bytes |
| `tests/fixtures/generated/mail-layout-report.json` | 34765 bytes |

Git status for scoped targets:

| Path | Observed status |
| --- | --- |
| `tests/fixtures/generated/cart-layout-report.json` | `M` |
| `tests/fixtures/generated/login-layout-report.json` | `M` |
| `tests/fixtures/generated/mail-layout-report.json` | `M` |

Diff checks for scoped targets:
- `git diff --numstat` returned no rows.
- `git diff --name-status` returned no rows.
- `git diff --raw` returned no rows.

Tracked index entries from read-only `ls-files --stage`:

| Path | Mode | Blob SHA |
| --- | --- | --- |
| `tests/fixtures/generated/cart-layout-report.json` | `100644` | `96ddb56ef46951e18593fab451698bef40df4f29` |
| `tests/fixtures/generated/login-layout-report.json` | `100644` | `5539aadc74f2710e13f6215169d3bcc0ba7dfdce` |
| `tests/fixtures/generated/mail-layout-report.json` | `100644` | `19a476089f7068f5e6c6b1732631479987c0f770` |

Boundary:
- The three fixture path names, status markers, file sizes, and index blob SHAs are known.
- Fixture JSON contents were not read by this worker.
- No fixture cleanup and no delete/move/stash/restore were performed.
- No `git update-index --refresh`, restore, fixture regeneration, stage, commit, push, test, build, browser, or runtime action was performed.

## Preflight Linkage

Source packet family:
- Packet Family 10 - Figma Fixture Status-Only Residue from `codex-preflight-resolution-sweep-2026-06-28.md`.

Original blocker:
- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`: missing fixture policy selector and mutation approval for refresh/restore.

Preflight conservative/default route:
- selected_action: `preserve-status-only-waiver-boundary`
- selected targets:
  - `tests/fixtures/generated/cart-layout-report.json`
  - `tests/fixtures/generated/login-layout-report.json`
  - `tests/fixtures/generated/mail-layout-report.json`
- mutation policy: no `update-index --refresh`, restore, fixture generation, owner-model inspection, stage, commit, push, cleanup, or file edit.

Related context:
- The handoff index records `codex-figma-fixture-status-waiver-2026-06-28.md` as the prior waiver report for the same three status-only fixture paths.
- The nested worktree resolver is related Figma residue context, but nested worktree mutation and fixture residue mutation are separate decision paths.

This resolver preserves the conservative route and narrows future work into selectors. It does not clear the blocker for mutating actions.

## Safe Doc-Only Actions

The following actions are safe within this documentation-only resolver scope:

1. Record current read-only fixture status and no-content-diff metadata in a resolver packet.
2. Carry forward `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` for every non-default or mutating action.
3. Classify `FigmaToCode-design-gen` fixture residue as status-only/no-content-diff evidence, not fixture correctness proof.
4. List exact decisions/selectors required before any future fixture mutation.
5. Create future bounded work orders that require re-reading current state before acting.

These actions are documentation/accounting only. They do not mutate fixtures, indexes, repos, branches, PRs, or runtime state.

## Blocked Actions

Blocked until exact selectors and owner/fixture policy are supplied:

| Action class | Status | Reason |
| --- | --- | --- |
| fixture cleanup | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Cleanup scope and owner intent are not selected; current evidence supports only status-only residue accounting. |
| delete fixture reports | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Destructive deletion needs exact path scope, approval, backup/recovery policy, and expected post-state. |
| move fixture reports | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Moving fixture outputs changes repository artifact layout and needs an approved destination and recovery plan. |
| stash fixture reports | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Stashing changes repository state and needs exact path selector, stash message, and owner acceptance. |
| restore fixture reports | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Restore may discard index/working-tree residue or future changes; needs explicit approval and stop conditions. |
| refresh fixture index | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | `git update-index --refresh` is a git index mutation and needs explicit index-mutation approval. |
| regenerate fixture reports | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Regeneration needs exact command, expected outputs, artifact-write permission, and validation policy. |
| edit fixture report contents | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Content mutation needs fixture ownership, source-of-truth, expected JSON schema/values, and verification. |
| stage/commit fixture reports | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Publishing repo changes needs explicit commit scope, message, verification, and push/PR policy. |
| waive fixture status as final | `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE` | Waiver needs owner-approved scope, wording, duration, and acknowledgement that it is not fixture correctness proof. |

Hard boundary:
- no fixture cleanup
- no delete/move/stash/restore
- no fixture regeneration
- no index refresh
- no fixture edits
- no stage/commit/push/PR mutation
- no tests/builds/browser/runtime
- no secrets

## Decisions And Selectors Needed

Before any future fixture mutation or final waiver, Luba or an authorized owner decision packet must supply:

1. selected_fixture_policy:
   - one of `preserve-status-only-waiver-boundary`, `refresh-index`, `waive`, `restore`, `owner-inspect`, `regenerate-fixtures`, `cleanup-delete`, `move-fixtures`, or `stash-fixtures`.
2. selected_targets:
   - exact fixture paths; current scoped set is the three `tests/fixtures/generated/*-layout-report.json` files listed above.
3. owner_intent:
   - whether the current `M` status is expected index/metadata residue, obsolete residue, owner/model output to preserve, or unknown.
4. content_inspection_policy:
   - whether fixture JSON contents may be read, compared, redacted, or must remain uninspected.
5. index_mutation_policy:
   - whether `git update-index --refresh` or any equivalent index mutation is allowed.
6. working_tree_mutation_policy:
   - whether restore, delete, move, edit, regenerate, or cleanup is allowed, and which alternatives remain forbidden.
7. regeneration_policy:
   - exact command or script entrypoint, working directory, expected output files, allowed artifact writes, and validation rule if regeneration is selected.
8. preservation_policy:
   - backup branch/ref, patch export, archive path, stash message, copied evidence packet, or explicit no-backup waiver.
9. verification_evidence:
   - exact commands and expected outputs for post-action status, diff, index entries, file existence, and any content/schema validation.
10. stop_conditions:
   - stop if branch, status, path list, file content diff, file size, index blob, or expected generated output differs from this packet's expected snapshot unless the future work order explicitly accepts the changed snapshot.

## Future Bounded Work Orders

### Work Order A - Preserve/Status-Only Confirmation

worker_id:
- `bounded-figma-fixture-preserve-status-only-worker`

allowed actions:
- Read-only re-check of branch/status, scoped fixture file listing, `git diff --numstat`, `git diff --name-status`, `git diff --raw`, and `git ls-files --stage` for the selected fixture paths.
- Update a codex-prefixed documentation packet only if explicitly scoped.

forbidden actions:
- no fixture cleanup
- no delete/move/stash/restore
- no fixture regeneration
- no index refresh
- no fixture edits
- no stage/commit/push/PR mutation
- no tests/builds/browser/runtime
- no secrets

exit evidence:
- current status/diff/index snapshot and a documented preserve/status-only boundary.

### Work Order B - Fixture Owner Decision Prep

worker_id:
- `bounded-figma-fixture-owner-decision-prep-worker`

allowed actions:
- Produce options, consequences, and required selectors for preserve/status-only, refresh-index, waive, restore, owner-inspect, regenerate, cleanup-delete, move, or stash.
- Read only metadata unless explicit content-inspection permission is added.

forbidden actions:
- no fixture cleanup
- no delete/move/stash/restore
- no fixture regeneration
- no index refresh
- no fixture edits
- no stage/commit/push/PR mutation
- no tests/builds/browser/runtime
- no secrets

exit evidence:
- owner decision matrix and smallest remaining explicit Luba decision.

### Work Order C - Fixture Mutation Worker

worker_id:
- `bounded-figma-fixture-mutation-worker`

precondition:
- May run only after Luba supplies all decisions/selectors above and explicitly authorizes the selected mutation class.

required first step:
- Re-read branch/status, scoped fixture file listing, `git diff --numstat`, `git diff --name-status`, `git diff --raw`, and `git ls-files --stage` before mutation.

allowed actions:
- Only the exact selected action from the approved work order.

forbidden without explicit selector:
- no fixture cleanup
- no delete/move/stash/restore
- no fixture regeneration
- no index refresh
- no fixture edits
- no stage/commit/push/PR mutation

exit evidence:
- before/after status, diff, index entries, file existence, and backup/stash/restore/regeneration evidence if applicable.

## Final Boundary

This resolver packet keeps Packet Family 10 blocked for any non-default action:

- `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`
- no fixture cleanup
- no delete/move/stash/restore
- `FigmaToCode-design-gen`

The evidence supports only:
- current read-only metadata for the three observed fixture reports;
- safe doc-only routing;
- future selectors needed before any fixture mutation or final waiver.

It does not support:
- fixture correctness;
- source-design fidelity;
- safe cleanup;
- deletion/move/stash/restore readiness;
- index refresh readiness;
- regeneration readiness;
- commit/push readiness;
- PR readiness;
- clean repo state;
- tests/builds/browser/runtime success;
- product completion.

files_changed:
- `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-figma-fixture-residue-resolver-2026-06-28.md`

verification:
- Post-write readback by this worker confirmed file existence and required literals: `FIGMA_FIXTURE_RESIDUE_RESOLVER`, `PACKET_BLOCKED_BY_PREFLIGHT_FAILURE`, `no fixture cleanup`, `no delete/move/stash/restore`, and `FigmaToCode-design-gen`.
