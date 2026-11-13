# Codex Approval-Ready Execution Packets - 2026-06-28

claim_scope:
- Created this approval-ready execution packet sweep report only.
- Covered the remaining approval-dependent action groups from the handoff, decision workflow, execution sweep, default sweep, and classification reports.
- Did not execute destructive, publishing, credential, PR, branch/ref, cleanup, archive, stash, restore, test, build, browser, runtime, external AI, staging, commit, push, merge, close, or mark-ready actions.

evidence_scope:
- Read existing Codex handoff/index/workflow/sweep/classification reports in `C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\`.
- Checked that this target report did not already exist before creation.
- Used the current work-order supplied latest core PR #6 head: `14af29483ed5497df18ecb35edabeeeab3f23057`.
- Did not re-query live PRs, refs, checks, mergeability, worktree status, artifact contents, secrets, runtime behavior, browser UI, tests, builds, or external services.

count_claimed:
- 1 report file created.
- 8 required packet groups covered.
- 10 total packet families covered: PR state, core branch policy, core evidence/residue, package artifacts, wiki corpora, trace JSON, external AI visual judge, runtime/design proof, Figma nested worktree, Figma fixture status-only residue.
- 0 git actions.
- 0 PR mutations.
- 0 destructive/publishing/credential actions.

independent_evidence_count:
- 1 directory listing of the handoff folder.
- 2 targeted `rg` inspections over handoff reports.
- 6 targeted readbacks of relevant report sections.
- 1 exact target-file existence check.

subagent_id_or_NONE: APPROVAL_READY_EXECUTION_PACKET_SWEEP

downgrade_needed:
- No for the narrow claim that this report was created and packets were prepared from existing evidence.
- Yes for live/current repo state, PR state, ref state, clean-repo state, product readiness, runtime proof, full visual fidelity, external AI judge success, artifact sensitivity, and merge readiness.

Required literals:
- `APPROVAL_READY_EXECUTION_PACKET_SWEEP_COMPLETE`
- `FINAL_ALLOWED_AFTER_APPROVAL_READY_EXECUTION_PACKET_SWEEP`
- `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`
- `explicit approval required is not final-stop authority under live continue`

## Boundary

This artifact replaces a forbidden final stop on approval-dependent actions with execution packets. It does not approve or execute the actions.

`explicit approval required is not final-stop authority under live continue`

After Luba approves a specific option with the required selector values named in that packet, the receiving bounded worker can execute the approved path without a new planning stop. If a packet names a missing secret, external account, archive destination, or merge method as required approval input, execution remains blocked until that exact missing item is supplied or waived.

## Current Evidence Index

Core PR #6 latest head for this sweep:
- `lubochka/llm_mvp_core` PR #6, branch `DesignToCode-clean`, latest work-order supplied head `14af29483ed5497df18ecb35edabeeeab3f23057`.
- Existing handoff/index files still contain older core SHA records, including `091117f05672b2968e9a54895690c0f20b212868`, `60dff3549dae44e60873d11182e86544f7884ddc`, and historical governance SHAs. This report did not edit those files.

Other recorded PR heads from existing handoff evidence:
- `lubochka/xiigen-platform-dot-net-angular` PR #1, branch `DesignToCode`, recorded head `bf62e291eca3c366641345781ebda85c7d255f90`.
- `lubochka/xiigen-flows-dot-net-angular` PR #1, branch `DesignToCode`, recorded head `5e6836e7328cd896c686ae083e8014b8b94db09b`.
- `lubochka/FigmaToCode-design-gen` PR #40, branch `DesignToCode`, recorded head `500fcab72e2df9ef7ce8999a63c786e26d24db47`.
- `lubochka/CanvaToCode` PR #1, branch `DesignToCode`, recorded head `2fa6629c9a1e3a70e5385fa4afa8109a8d264add`.

Existing reports used:
- `codex-designconversion-handoff-index-2026-06-28.md`
- `codex-designconversion-completion-handoff-2026-06-28.md`
- `codex-designconversion-decision-workflow-2026-06-28.md`
- `codex-decision-workflow-execution-sweep-2026-06-28.md`
- `codex-default-selection-execution-sweep-2026-06-28.md`
- `codex-core-evidence-dirt-manifest-2026-06-28.md`
- `codex-package-artifact-classification-2026-06-28.md`
- `codex-wiki-figma-residue-classification-2026-06-28.md`
- `codex-figma-fixture-status-waiver-2026-06-28.md`

## Packet Group 1 - PR State Actions For All Five PRs

Approval selectors Luba must name:
- PR set: one or more of `core#6`, `platform#1`, `flows#1`, `figma#40`, `canva#1`, or `all five`.
- Action: `leave-draft`, `mark-ready`, `merge`, or `close`.
- For merge only: merge method `merge`, `squash`, or `rebase`, plus whether to require currently observed head SHA match.

Targets:
- `core#6`: `lubochka/llm_mvp_core`, PR #6, base `main`, head `DesignToCode-clean`, expected supplied head `14af29483ed5497df18ecb35edabeeeab3f23057`.
- `platform#1`: `lubochka/xiigen-platform-dot-net-angular`, PR #1, base `main`, head `DesignToCode`, recorded head `bf62e291eca3c366641345781ebda85c7d255f90`.
- `flows#1`: `lubochka/xiigen-flows-dot-net-angular`, PR #1, base `main`, head `DesignToCode`, recorded head `5e6836e7328cd896c686ae083e8014b8b94db09b`.
- `figma#40`: `lubochka/FigmaToCode-design-gen`, PR #40, base `improve_group_css_user`, head `DesignToCode`, recorded head `500fcab72e2df9ef7ce8999a63c786e26d24db47`.
- `canva#1`: `lubochka/CanvaToCode`, PR #1, base `main`, head `DesignToCode`, recorded head `2fa6629c9a1e3a70e5385fa4afa8109a8d264add`.

Current evidence:
- Prior read-only PR monitor reported all five PRs open, draft, unmerged, and mergeable.
- This sweep did not perform a fresh PR metadata read.
- Core PR #6 head is updated by current work-order evidence to `14af29483ed5497df18ecb35edabeeeab3f23057`.

Dry-run/readiness check before execution:
- Read-only GitHub PR metadata for each selected PR:
  - current state open/closed
  - draft true/false
  - merged true/false
  - base/head branch
  - current head SHA
  - mergeable/check status if merging
- Compare current head SHA to the expected SHA above. If it differs, stop that PR only and report `HEAD_CHANGED_REQUIRES_OWNER_CONFIRMATION`.
- Confirm no selected PR depends on unresolved blockers unless Luba explicitly waived those blockers.

No-secret/no-destructive preflight:
- Do not read or print secrets.
- Do not mutate unselected PRs.
- For mark-ready, merge, and close, confirm Luba's approval names the selected PRs and action.
- For merge, confirm Luba named merge method.

Exact API actions after approval:

Leave draft/open:
```text
For each selected PR:
1. Call GitHub read-only PR info.
2. If PR is open and not merged, perform no mutation.
3. Record retained state and current head SHA in the handoff update.
```

Mark ready:
```text
For each selected PR:
1. Call GitHub read-only PR info and verify expected state/head.
2. If PR is draft and open, call mark_pull_request_ready_for_review for that repo/PR.
3. If PR is already ready, record no-op result.
4. Do not merge, close, retarget, label, or edit title/body.
```

Merge:
```text
For each selected PR:
1. Call GitHub read-only PR info and status/check metadata.
2. Verify PR is open, not draft unless Luba explicitly approved merging draft via ready-first path, not merged, and current head matches the approved expected SHA.
3. Call merge_pull_request with:
   repository_full_name=<selected repo>
   pr_number=<selected PR number>
   merge_method=<Luba-approved merge|squash|rebase>
   expected_head_sha=<current verified head SHA>
4. Record merge SHA and method.
```

Close:
```text
For each selected PR:
1. Call GitHub read-only PR info and verify expected state/head.
2. Save current URL, title, state, draft flag, merged flag, base, head branch, and head SHA in the execution report.
3. Call update_pull_request with state=closed.
4. Do not delete branches unless separately approved.
```

Rollback/recovery:
- Leave draft: no rollback needed.
- Mark ready: convert selected PR back to draft only if Luba explicitly approves rollback; use GitHub `convert_pull_request_to_draft`.
- Merge: GitHub merge is not safely reversible from this packet. Recovery requires a new revert PR or branch reset plan approved separately.
- Close: reopen selected PR only if Luba approves rollback; use GitHub update PR `state=open`.

Risk/consequence:
- Mark-ready can imply review readiness that this handoff does not prove.
- Merge publishes unresolved residue/branch/proof decisions into base branches and may require revert work.
- Close may abandon useful branches or require reopening/recreating PRs.
- Leave draft can allow heads to drift and leaves work incomplete.

Artifacts/docs to update after execution:
- `codex-designconversion-handoff-index-2026-06-28.md`
- `codex-designconversion-completion-handoff-2026-06-28.md`
- Add an execution result report named with timestamp and selected PR/action.

Post-action verification:
- Read selected PR metadata again.
- Record before/after state, draft flag, merged flag, head SHA, merge SHA if any, and URL.
- For merge, verify base branch contains merge result or GitHub reports merged true.

Packet blocked notes:
- None by secrets.
- Merge is blocked until explicit merge method and selected PR set are supplied.

## Packet Group 2 - Core Branch Policy

Approval selectors Luba must name:
- Branch policy: `keep DesignToCode-clean`, `rename/move to exact DesignToCode`, `preserve both and create exact-name PR`, `preserve both only`, or `defer`.
- Whether preservation tags/refs are allowed before any ref movement.
- For new exact-name PR: target base branch and PR title/body policy.

Targets:
- Repo: `lubochka/llm_mvp_core`
- Current PR branch: `DesignToCode-clean`
- Exact-name branch candidate: `DesignToCode`
- Preservation branch recorded by handoff: `DesignToCode-with-luba-evidence`
- Luba evidence commit recorded by handoff: `3e47d7a22`
- Current supplied clean PR head: `14af29483ed5497df18ecb35edabeeeab3f23057`

Current evidence:
- Existing handoff says core uses `DesignToCode-clean` because local `DesignToCode` preserves Luba-authored evidence/history.
- Existing decision workflow recommends keeping `DesignToCode-clean` until Luba explicitly chooses branch-history operation.
- This sweep did not fetch refs.

Dry-run/readiness check before execution:
```text
git fetch origin --prune
git show-ref --heads --verify refs/heads/DesignToCode
git show-ref --heads --verify refs/heads/DesignToCode-clean
git show-ref --heads --verify refs/heads/DesignToCode-with-luba-evidence
git ls-remote origin refs/heads/DesignToCode refs/heads/DesignToCode-clean refs/heads/DesignToCode-with-luba-evidence
git rev-parse DesignToCode-clean
git rev-parse DesignToCode
git rev-parse DesignToCode-with-luba-evidence
```

No-secret/no-destructive preflight:
- No secrets.
- Do not delete or force-update any ref unless Luba's approval explicitly names that ref and preservation plan.
- If any ref differs from packet evidence, report `REF_CHANGED_REQUIRES_OWNER_CONFIRMATION`.

Exact commands/API actions after approval:

Keep `DesignToCode-clean`:
```text
No branch/ref mutation.
Update handoff docs to state PR #6 intentionally remains on `DesignToCode-clean`.
```

Rename/move active branch to exact `DesignToCode`:
```text
1. Fetch/read refs and record before SHAs.
2. Create preservation refs/tags if explicitly approved, for example:
   git branch codex/preserve-DesignToCode-before-clean-move <current DesignToCode SHA>
   git branch codex/preserve-DesignToCode-clean-before-move <current DesignToCode-clean SHA>
3. Move or recreate `DesignToCode` only according to Luba's exact policy:
   git branch -f DesignToCode <approved clean SHA>
4. Push only if approved:
   git push origin DesignToCode
5. Update or recreate PR only if approved by Luba.
```

Preserve both and create exact-name PR:
```text
1. Fetch/read refs and record before SHAs.
2. Create local exact-name branch from `14af29483ed5497df18ecb35edabeeeab3f23057`:
   git branch DesignToCode 14af29483ed5497df18ecb35edabeeeab3f23057
3. If local `DesignToCode` already exists, create `DesignToCode-exact-clean` or other Luba-approved name instead; do not overwrite.
4. Push approved exact branch:
   git push origin <approved exact branch name>
5. Open or update PR targeting `main` only if approved.
```

Preserve both only:
```text
No branch/ref mutation unless Luba approves preservation refs/tags.
Record both branch purposes in handoff docs.
```

Defer:
```text
No branch/ref mutation.
Record branch policy as unresolved owner decision blocking merge/cleanup claims.
```

Rollback/recovery:
- Keep/defer/preserve docs only: revert doc-only report update if needed.
- Ref mutation: restore from recorded before SHAs using `git branch -f` and `git push --force-with-lease` only with explicit rollback approval.
- PR creation: close newly created PR only with explicit approval.

Risk/consequence:
- Ref moves can obscure Luba evidence history.
- New exact-name PR can duplicate review surface.
- Deferral preserves safety but keeps branch naming mismatch unresolved.

Artifacts/docs to update after execution:
- Handoff index and completion handoff branch table.
- A branch reconciliation execution report with before/after local and remote SHAs.

Post-action verification:
- `git ls-remote origin` for affected refs.
- PR metadata read for PR #6 or new PR.
- Confirm evidence preservation refs exist if created.

Packet blocked notes:
- No secret blocker.
- Branch mutation blocked until Luba names exact branch policy and preservation policy.

## Packet Group 3 - Core Evidence And Residue

Approval selectors Luba must name:
- Categories to act on: any subset of core manifest categories below.
- Action per category: `commit`, `archive`, `cleanup`, `waive`, or `defer`.
- For archive: destination path and redaction policy.
- For commit: whether sensitivity/provenance review is required before staging.
- For cleanup: whether archive backup is required first.

Targets and current evidence from `codex-core-evidence-dirt-manifest-2026-06-28.md`:
- `docs/plans/evidence/extension-full-readiness-dual-path`: 4,170 untracked entries, evidence archive with possible generated cache/user-data mixed into evidence folders.
- `artifacts/extension-runtime-token-quality-learnability`: 2,678 untracked entries, evidence archive and commit candidate pending owner policy.
- `evidence/run/*flow47*`: 26 untracked entries, evidence archive and likely narrow commit candidate.
- `evidence/run/flow-platform-bridge-20260628-161923`: 12 untracked entries, evidence archive and likely narrow commit candidate; includes saved HTML/CSS browser proof only.
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-*.json*`: 2 tracked modified plan/state edits.
- `xiigen-codex-*.json`: handled in Packet Group 6 because sensitivity is a separate hard blocker.
- Manifest total: 6,891 matching dirty entries, including 6,889 untracked entries and 2 tracked diffs.

Dry-run/readiness check before execution:
```text
git status --short --untracked-files=all -- <selected pathspecs>
git diff --name-status -- <selected tracked pathspecs>
Get-ChildItem -Recurse -LiteralPath <selected root> | Measure-Object
```

For commit or archive, add sensitivity/provenance read only after Luba approves that inspection:
```text
Search selected files for obvious secret tokens, credentials, private keys, and account identifiers.
Produce exact file list and excluded file list.
```

No-secret/no-destructive preflight:
- Do not inspect content for sensitivity unless approval includes sensitivity inspection.
- Do not stage, commit, archive/copy, delete, clean, or waive categories outside the selected list.
- For cleanup, require archive/backup approval or explicit no-backup waiver.

Exact commands/actions after approval:

Commit:
```text
1. Produce selected file manifest.
2. Run approved sensitivity/provenance inspection.
3. If clean, stage exactly selected files:
   git add -- <selected pathspecs>
4. Commit only with Luba-approved message:
   git commit -m "<approved message>"
5. Push only if Luba separately approved push:
   git push origin <current branch>
```

Archive:
```text
1. Create approved archive destination.
2. Copy selected roots/files to destination.
3. Generate SHA256 manifest and source path manifest.
4. Do not delete source files unless cleanup is separately approved.
```

Cleanup:
```text
1. Verify optional archive/backup exists if required.
2. Delete only selected files/roots.
3. Run targeted status check for selected pathspecs.
```

Waive:
```text
1. Record exact selected categories, counts, and forbidden claims.
2. Do not mutate repo files.
```

Defer:
```text
1. Record selected categories as unresolved owner decisions.
2. Do not mutate repo files.
```

Rollback/recovery:
- Commit: revert commit or reset branch only with explicit rollback approval.
- Archive: preserve archive manifest; if copied to wrong destination, delete archive copy only with cleanup approval.
- Cleanup: restore from archive/backup if available; if no backup was approved, recovery may be impossible.
- Waive/defer: update docs to reverse policy.

Risk/consequence:
- Commit can publish logs, screenshots, traces, generated corpora, or sensitive local paths.
- Cleanup can destroy evidence.
- Archive may copy sensitive data to an approved but still external or shared location.
- Waive/defer leaves clean-repo and full-completion claims blocked.

Artifacts/docs to update after execution:
- Core evidence dirt manifest or a successor manifest.
- Handoff index and completion handoff owner-decision ledger.
- Archive manifest, cleanup manifest, or commit-scope manifest.

Post-action verification:
- Targeted `git status --short --untracked-files=all -- <selected pathspecs>`.
- If commit: `git show --stat --oneline HEAD`.
- If archive: verify SHA256 manifest and file counts.
- If cleanup: verify selected paths absent or clean.

Packet blocked notes:
- Potential sensitivity blocker for evidence categories. If Luba requires content inspection but does not approve it, mark the selected category `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` only if the blocker is a missing external credential/account; otherwise mark `BLOCKED_REQUIRES_EXPLICIT_LUBA_CHOICE`.

## Packet Group 4 - Package Artifacts

Approval selectors Luba must name:
- Package action: `restore platform packages`, `archive selected packages`, `commit after provenance verification`, `cleanup untracked flows packages`, or `defer`.
- Selected package set.
- For archive: destination path.
- For commit: provenance rule and commit/push policy.

Targets and current evidence from `codex-package-artifact-classification-2026-06-28.md`:
- Platform tracked modified packages:
  - `C:\Projects\xiigen-platform-dot-net-angular\artifacts\packages\Xiigen.Auth.0.1.0.nupkg`
  - `C:\Projects\xiigen-platform-dot-net-angular\artifacts\packages\Xiigen.Fabrics.InMemory.0.1.1.nupkg`
  - `C:\Projects\xiigen-platform-dot-net-angular\artifacts\packages\Xiigen.Fabrics.Scheduler.0.1.0.nupkg`
  - `C:\Projects\xiigen-platform-dot-net-angular\artifacts\packages\Xiigen.Freedom.0.1.0.nupkg`
  - `C:\Projects\xiigen-platform-dot-net-angular\artifacts\packages\Xiigen.Kernel.0.1.1.nupkg`
  - `C:\Projects\xiigen-platform-dot-net-angular\artifacts\packages\Xiigen.Platform.Marketplace.0.1.0.nupkg`
- Flows untracked FLOW-47 packages:
  - `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-47-design-conversion-intake\artifacts\nuget\FLOW-47\Xiigen.Flow.DesignConversionIntake.Application.0.1.0.nupkg`
  - `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-47-design-conversion-intake\artifacts\nuget\FLOW-47\Xiigen.Flow.DesignConversionIntake.Infrastructure.0.1.0.nupkg`
- Evidence boundary: 8 package artifacts classified; no build/test/source provenance verification was performed.

Dry-run/readiness check before execution:
```text
git -C C:\Projects\xiigen-platform-dot-net-angular status --porcelain=v1 -- artifacts/packages/*.nupkg
git -C C:\Projects\xiigen-platform-dot-net-angular diff --name-status -- artifacts/packages/*.nupkg
git -C C:\Projects\xiigen-flows-dot-net-angular status --porcelain=v1 --untracked-files=all -- flows/FLOW-47-design-conversion-intake/artifacts/nuget/FLOW-47/*.nupkg
Get-FileHash -Algorithm SHA256 <selected packages>
```

No-secret/no-destructive preflight:
- Do not restore/delete package binaries without explicit approval.
- Do not publish packages.
- Do not commit packages without provenance approval.

Exact commands/actions after approval:

Restore platform packages:
```text
git -C C:\Projects\xiigen-platform-dot-net-angular restore -- artifacts/packages/Xiigen.Auth.0.1.0.nupkg artifacts/packages/Xiigen.Fabrics.InMemory.0.1.1.nupkg artifacts/packages/Xiigen.Fabrics.Scheduler.0.1.0.nupkg artifacts/packages/Xiigen.Freedom.0.1.0.nupkg artifacts/packages/Xiigen.Kernel.0.1.1.nupkg artifacts/packages/Xiigen.Platform.Marketplace.0.1.0.nupkg
```

Archive selected packages:
```text
Copy selected `.nupkg` files to <Luba-approved archive destination>.
Create SHA256 manifest and metadata table.
Do not remove originals unless cleanup is separately approved.
```

Commit after provenance verification:
```text
1. Run approved provenance checks mapping packages to accepted source/build outputs.
2. Stage only approved package paths:
   git add -- <selected package paths>
3. Commit only with approved message:
   git commit -m "<approved message>"
4. Push only if separately approved.
```

Cleanup untracked flows packages:
```text
1. Optional archive/backup if approved.
2. Remove only selected untracked FLOW-47 `.nupkg` files.
3. Verify targeted status.
```

Defer:
```text
Record package artifacts as unresolved owner decision; no mutation.
```

Rollback/recovery:
- Restore: if restored accidentally, recover modified packages from archive/backup only if one was created.
- Archive: remove archive copy only with cleanup approval.
- Commit: revert commit only with rollback approval.
- Cleanup: restore from archive/backup if available.

Risk/consequence:
- Package binaries can be deliverables, generated residue, or stale build output.
- Restore/cleanup can discard intended deliverables.
- Commit can publish unverified binaries.
- Archive copies binaries into another location and requires provenance/security policy.

Artifacts/docs to update after execution:
- Package classification successor report.
- Handoff index and owner-decision ledger.
- Archive/provenance/cleanup manifest.

Post-action verification:
- Targeted git status and diff in platform/flows repos.
- SHA256 manifest if archived or committed.
- If committed: commit SHA and `git show --stat`.

Packet blocked notes:
- No secret blocker.
- Archive is blocked until destination is supplied.
- Commit is blocked until provenance policy and commit approval are supplied.

## Packet Group 5 - Wiki-Domain Generated Corpora

Approval selectors Luba must name:
- Repo set: platform wiki-domain, flows wiki-domain, or both.
- Action: `commit`, `archive`, `cleanup`, `waive`, or `defer`.
- For archive: destination path and redaction policy.
- For cleanup: backup/archive requirement.

Targets and current evidence from `codex-wiki-figma-residue-classification-2026-06-28.md`:
- Platform root: `C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain`
  - 191 files
  - 5,177,720 bytes
  - 3,090 JSONL records
  - untracked; 0 tracked files
- Flows root: `C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain`
  - 2,052 files
  - 66,914,310 bytes
  - 36,894 JSONL records
  - untracked; 0 tracked files
- Manifest status: `documentation_ready_training_pending`, `weight_artifact_ref=not_trained_yet`.

Dry-run/readiness check before execution:
```text
git -C C:\Projects\xiigen-platform-dot-net-angular status --porcelain -- .xiigen/wiki-domain
git -C C:\Projects\xiigen-flows-dot-net-angular status --porcelain -- .xiigen/wiki-domain
Get-ChildItem -Recurse -LiteralPath C:\Projects\xiigen-platform-dot-net-angular\.xiigen\wiki-domain | Measure-Object
Get-ChildItem -Recurse -LiteralPath C:\Projects\xiigen-flows-dot-net-angular\.xiigen\wiki-domain | Measure-Object
```

No-secret/no-destructive preflight:
- Do not inspect/copy/delete corpora unless Luba approves the selected action.
- Do not claim training, model promotion, or learned weights from corpus presence.

Exact commands/actions after approval:

Commit corpora:
```text
1. Produce exact file list and size summary.
2. Run approved sensitivity/provenance check.
3. Stage only selected roots:
   git -C <repo> add -- .xiigen/wiki-domain
4. Commit with approved message.
5. Push only if separately approved.
```

Archive corpora:
```text
1. Copy selected `.xiigen/wiki-domain` roots to <approved archive destination>.
2. Generate file count, byte count, and SHA256 manifest.
3. Do not delete sources unless cleanup is separately approved.
```

Cleanup corpora:
```text
1. Verify archive/backup if required.
2. Remove selected `.xiigen/wiki-domain` roots only.
3. Verify targeted git status.
```

Waive:
```text
Record waiver with exact counts and forbidden clean-repo/product-readiness claims.
```

Defer:
```text
Keep corpora as unresolved owner decision and do not mutate.
```

Rollback/recovery:
- Commit: revert commit with approval.
- Archive: restore from archive if cleanup occurred; remove archive copy only with approval.
- Cleanup: recover only from backup/archive if available.
- Waive/defer: update docs to reverse policy.

Risk/consequence:
- Commit adds thousands of generated files and large data volume.
- Cleanup can delete useful documentation/training/evidence corpus.
- Archive may expose generated corpus outside repo history.
- Waiver/defer blocks clean-repo claims.

Artifacts/docs to update after execution:
- Wiki/Figma residue successor report.
- Handoff index and owner-decision ledger.
- Archive manifest or commit/cleanup manifest.

Post-action verification:
- Targeted git status for `.xiigen/wiki-domain`.
- File count/byte count after action.
- Archive checksum manifest if archived.

Packet blocked notes:
- No secret blocker unless Luba requires external archive storage requiring credentials. If so: `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` missing item: approved archive account/location credentials.

## Packet Group 6 - Root Trace JSON Commit/Archive/Cleanup/Waive/Defer

Approval selectors Luba must name:
- Action: `sensitivity inspect`, `commit after sensitivity review`, `archive outside repo`, `clean/delete`, `waive`, or `defer`.
- For archive: destination path and redaction policy.
- For commit: commit message and whether redaction is required before commit.
- For cleanup: backup/archive requirement.

Targets:
- `C:\Projects\llm_mvp_core\xiigen-codex-request.json`
- `C:\Projects\llm_mvp_core\xiigen-codex-response.json`
- `C:\Projects\llm_mvp_core\xiigen-codex-trace.json`

Current evidence:
- Core dirt manifest classifies these 3 files as evidence archive candidates and hard blockers before commit because content was not inspected for sensitivity.
- This sweep did not read the JSON contents.

Dry-run/readiness check before execution:
```text
git -C C:\Projects\llm_mvp_core status --short --untracked-files=all -- xiigen-codex-request.json xiigen-codex-response.json xiigen-codex-trace.json
Get-Item -LiteralPath C:\Projects\llm_mvp_core\xiigen-codex-request.json, C:\Projects\llm_mvp_core\xiigen-codex-response.json, C:\Projects\llm_mvp_core\xiigen-codex-trace.json | Select-Object FullName,Length,LastWriteTime
```

No-secret/no-destructive preflight:
- Do not print file contents to chat if secrets are suspected.
- Do not upload files externally.
- Do not stage/commit/archive/delete until sensitivity policy is approved.

Exact commands/actions after approval:

Sensitivity inspect:
```text
1. Inspect exactly the three JSON files locally.
2. Search for secrets, tokens, credentials, private URLs, account identifiers, and private prompt/response content.
3. Produce a redacted sensitivity report; do not paste secrets.
4. Recommend commit, archive, redact, cleanup, waive, or defer.
```

Commit after sensitivity review:
```text
1. Complete approved sensitivity inspection.
2. Redact if approved and required.
3. Stage exact files:
   git -C C:\Projects\llm_mvp_core add -- xiigen-codex-request.json xiigen-codex-response.json xiigen-codex-trace.json
4. Commit with approved message.
5. Push only if separately approved.
```

Archive outside repo:
```text
1. Complete approved sensitivity inspection or explicit no-inspection waiver.
2. Copy exact files to <approved archive destination>.
3. Generate SHA256 manifest and redaction note.
4. Do not delete source files unless cleanup is separately approved.
```

Clean/delete:
```text
1. Verify approved archive/backup if required.
2. Delete exactly the three files.
3. Verify targeted git status.
```

Waive:
```text
Record waiver that trace files remain unresolved and clean-repo/sensitivity claims are forbidden.
```

Defer:
```text
Record unresolved owner decision and perform no content inspection or mutation.
```

Rollback/recovery:
- Commit: revert commit only with approval.
- Archive: restore from archive if cleanup occurred.
- Cleanup: recover from archive/backup only if available.
- Redaction: keep original only in approved secure archive if required.

Risk/consequence:
- Files may contain prompts, responses, traces, credentials, internal paths, or sensitive data.
- Commit can permanently publish sensitive content in history.
- Cleanup can destroy evidence.
- Archive requires secure destination.

Artifacts/docs to update after execution:
- Trace sensitivity report.
- Handoff index and owner-decision ledger.
- Archive/cleanup/commit manifest.

Post-action verification:
- Targeted git status.
- SHA256 manifest if archived.
- Commit SHA if committed.
- Explicit statement that secrets were not printed.

Packet blocked notes:
- `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` only if approved archive storage requires missing external account/credentials.
- Otherwise blocked by missing explicit sensitivity/action approval.

## Packet Group 7 - External AI Visual Judge Provider/Credential/Use/Waiver

Approval selectors Luba must name:
- Policy: `configure and run external judge`, `use local/offline judge`, `waive external judge for handoff`, or `defer`.
- If external: provider, endpoint, model, approved secret environment variable name, data-sharing approval, artifact set, prompt/rubric, pass threshold.
- If local/offline: approved discovery scope and allowed runtime.

Targets:
- Saved-output browser proof evidence:
  - `C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof\browser-visual-proof-report.json`
  - `C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof\desktop-1440x900.png`
  - `C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof\mobile-390x844.png`
  - `C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof\dom-text.txt`
- Design references: not supplied in the current evidence. Missing if external/source fidelity judge is required.

Current evidence:
- Existing handoff says no external AI visual judge was configured or executed.
- Saved-output browser proof is saved HTML/CSS browser render evidence only, not source-design fidelity and not full runtime pipeline proof.

Dry-run/readiness check before execution:
```text
Test-Path -LiteralPath <approved screenshot/design paths>
Verify approved secret environment variable name exists without printing value.
Verify provider endpoint/model are named in Luba approval.
Verify data-sharing approval covers selected artifacts.
```

No-secret/no-destructive preflight:
- Never print secret values.
- Do not upload screenshots/design artifacts unless data-sharing approval names the provider and artifacts.
- Do not call external AI without endpoint/model/secret convention/rubric.

Exact commands/actions after approval:

Configure and run external judge:
```text
1. Verify required selector values:
   provider=<approved provider>
   endpoint=<approved endpoint>
   model=<approved model>
   secret_env_var=<approved env var name>
   artifacts=<approved screenshots/design references>
   rubric=<approved rubric>
   pass_threshold=<approved threshold>
2. Verify secret exists by environment variable name only; do not print it.
3. Run the approved judge script or API call.
4. Save prompt/rubric, inputs manifest, response, score, and boundary statement.
5. Do not claim product readiness unless threshold and Luba policy explicitly allow that claim.
```

Use local/offline judge only:
```text
1. Run approved local tool discovery.
2. Produce offline rubric/run plan.
3. Execute only after Luba approves the local run plan.
```

Waive external judge:
```text
Record explicit waiver and forbidden claims: no external AI judge proof, no full visual-fidelity proof, no merge-ready visual acceptance from this waiver alone.
```

Defer:
```text
Record unresolved external visual judge blocker.
```

Rollback/recovery:
- External call cannot be undone. Record exact data shared and provider response.
- Delete locally generated judge reports only with cleanup approval.
- Waiver/defer can be superseded by later approved judge run.

Risk/consequence:
- External judge may upload screenshots/design material to third party.
- Secret misuse could leak credentials.
- Judge result may be weak or misleading if rubric/model is poor.
- Waiver/defer blocks full visual fidelity claim.

Artifacts/docs to update after execution:
- Visual judge execution report.
- Handoff index and completion handoff proof boundary.
- Input artifact manifest and response transcript with secrets redacted.

Post-action verification:
- Report provider/model/endpoint boundary.
- Verify output file exists.
- Verify secret values are not present in report.
- Record pass/fail against approved threshold.

Packet blocked notes:
- `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT`
- Missing items for external judge: provider, endpoint, model, approved secret environment variable name, available credential in that environment variable, data-sharing approval, design reference inputs if source fidelity is required, rubric, pass threshold.
- Safe next work order: bounded visual-judge-config worker asks Luba for those exact missing items and does not run external AI until supplied.

## Packet Group 8 - Runtime / Design Proof Options After Approval

Approval selectors Luba must name:
- Policy: `require more proof`, `accept saved-output boundary for handoff only`, `waive additional proof`, or `defer`.
- If more proof: exact scenario scope, repos/services to start, commands allowed, browser/host UI permission, evidence directory, and pass thresholds.

Targets:
- Current saved-output proof directory: `C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof`
- Broader runtime/design proof: exact app/runtime paths are not selected by this packet and must be named in Luba approval or a scenario packet.

Current evidence:
- Saved-output proof exists per handoff reports.
- It is not full runtime pipeline proof, source-design fidelity proof, external AI proof, or product readiness.
- This sweep did not run tests/builds/browser/runtime.

Dry-run/readiness check before execution:
```text
Test-Path -LiteralPath C:\Projects\llm_mvp_core\evidence\run\flow-platform-bridge-20260628-161923\browser-visual-proof
Test-Path -LiteralPath <approved scenario command/script paths>
Verify ports/services/window targets named in scenario.
```

No-secret/no-destructive preflight:
- Do not use credentials.
- Do not run browser/runtime/tests/builds unless Luba approval names them.
- Do not capture secrets in screenshots or logs.

Exact commands/actions after approval:

Require more proof:
```text
1. Create exact Path A automated scenario:
   commands=<approved commands>
   expected outputs=<approved outputs>
   evidence files=<approved evidence paths>
2. Create exact Path B host/browser scenario:
   app/window target=<approved target>
   clicks/typing=<approved steps>
   expected visible state=<approved state>
   screenshots=<approved filenames>
   fallback for window/monitor overlap=<approved fallback>
   no secrets captured=true
3. Execute only after scenario approval.
4. Report pass/fail without upgrading evidence beyond what ran.
```

Accept saved-output boundary for handoff only:
```text
Record that saved-output browser proof is accepted for this handoff only and cannot support full runtime/design/readiness claims.
```

Waive additional proof:
```text
Record explicit waiver and forbidden claims; require Luba quote before any final readiness/merge claim relies on waiver.
```

Defer:
```text
Record runtime/design proof as unresolved hard blocker.
```

Rollback/recovery:
- Generated proof artifacts can be archived or cleaned only with approval.
- Waiver/defer can be superseded by later proof run.
- Runtime side effects require scenario-specific recovery commands if any service writes data.

Risk/consequence:
- Runtime/browser proof can alter local state, require services, or expose credentials in screenshots/logs.
- Accepting saved-output boundary is not product readiness.
- Waiver can allow risky merge/readiness claims if not bounded.

Artifacts/docs to update after execution:
- Runtime/design proof scenario and result report.
- Handoff proof boundary.
- Evidence directory manifest.

Post-action verification:
- Check evidence files exist.
- Record command outputs/screenshots.
- Confirm expected UI state and pass/fail thresholds.
- Confirm no secrets captured.

Packet blocked notes:
- No secret blocker unless approved runtime path needs external account login. If login/credential is required: `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` missing item: named external account/session/credential and no-secret capture policy.

## Packet Group 9 - Figma Nested Worktree `romantic-dewdney`

Approval selectors Luba must name:
- Policy: `preserve/exclude`, `commit-push`, `stash`, or `cleanup/reset`.
- If commit-push: owner intent, allowed checks, target remote/branch/PR.
- If stash: stash message.
- If cleanup/reset: backup/ref preservation policy and exact cleanup scope.

Target:
- `C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney`

Current evidence from wiki/Figma residue classifier:
- Branch/status: `## claude/romantic-dewdney...origin/claude/romantic-dewdney [ahead 1]`
- Dirty file: `M src/ui/components/App.tsx`
- Diff summary: `592 insertions, 592 deletions`
- Parent repo status marker: `m .claude/worktrees/romantic-dewdney`
- Classified as likely owner/other-model work and hard blocker for unrelated root cleanup/commit/push.

Dry-run/readiness check before execution:
```text
git -C C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney status --short --branch
git -C C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney diff --stat
git -C C:\Projects\FigmaToCode-design-gen\.claude\worktrees\romantic-dewdney log --oneline --decorate -5
```

No-secret/no-destructive preflight:
- Do not inspect full source diff unless approval includes owner review.
- Do not reset/stash/delete/commit/push without explicit policy.
- Record before SHA and status first.

Exact commands/actions after approval:

Preserve/exclude:
```text
No worktree mutation.
Record exclusion from unrelated cleanup and exact current status.
```

Commit-push:
```text
1. Inspect status and diff as approved.
2. Run approved checks only if named.
3. Stage selected files:
   git -C <worktree> add -- <approved paths>
4. Commit with approved message.
5. Push to approved remote/branch.
6. Update or open PR only if approved.
```

Stash:
```text
1. Record current branch, HEAD, status, and diff stat.
2. Create named stash:
   git -C <worktree> stash push -m "<approved stash message>" -- <approved paths or --all if explicitly approved>
3. Verify stash list and stash show.
```

Cleanup/reset:
```text
1. Create preservation branch/tag or backup patch if approved.
2. Reset/remove only approved state.
3. Verify status clean or expected.
```

Rollback/recovery:
- Preserve: no rollback.
- Commit-push: revert commit or reset branch only with approval.
- Stash: recover using recorded stash reference.
- Cleanup/reset: recover from backup/preservation ref only if created.

Risk/consequence:
- Commit/push may publish other-model/owner work.
- Stash may hide work.
- Cleanup/reset may destroy unpushed/dirty work.
- Preserve leaves parent status marker unresolved.

Artifacts/docs to update after execution:
- Figma worktree handoff report.
- Handoff index/owner ledger.
- PR or branch notes if pushed.

Post-action verification:
- Worktree `git status --short --branch`.
- Parent repo status marker check if relevant.
- Remote ref/PR metadata if pushed.

Packet blocked notes:
- No secret blocker.
- Commit-push/stash/cleanup blocked until explicit worktree policy and git permission are supplied.

## Packet Group 10 - Figma Fixture Status-Only Residue

Approval selectors Luba must name:
- Policy: `refresh index`, `waive`, `restore`, or `owner-inspect`.

Targets:
- Repo: `C:\Projects\FigmaToCode-design-gen`
- Paths:
  - `tests/fixtures/generated/cart-layout-report.json`
  - `tests/fixtures/generated/login-layout-report.json`
  - `tests/fixtures/generated/mail-layout-report.json`

Current evidence:
- Figma fixture waiver report and classifier report both say all three paths are `M` in git status.
- Content diff, numstat, raw diff, and quiet diff show no content delta.
- Waiver report recorded file hashes and status-only/index/metadata residue boundary.

Dry-run/readiness check before execution:
```text
git -C C:\Projects\FigmaToCode-design-gen status --porcelain=v1 -- tests/fixtures/generated/cart-layout-report.json tests/fixtures/generated/login-layout-report.json tests/fixtures/generated/mail-layout-report.json
git -C C:\Projects\FigmaToCode-design-gen diff --name-status -- tests/fixtures/generated/cart-layout-report.json tests/fixtures/generated/login-layout-report.json tests/fixtures/generated/mail-layout-report.json
git -C C:\Projects\FigmaToCode-design-gen diff --numstat -- tests/fixtures/generated/cart-layout-report.json tests/fixtures/generated/login-layout-report.json tests/fixtures/generated/mail-layout-report.json
git -C C:\Projects\FigmaToCode-design-gen diff --raw -- tests/fixtures/generated/cart-layout-report.json tests/fixtures/generated/login-layout-report.json tests/fixtures/generated/mail-layout-report.json
```

No-secret/no-destructive preflight:
- No secrets.
- Refresh-index mutates git index; restore mutates working tree. Require explicit approval.
- Do not regenerate fixtures unless separately approved.

Exact commands/actions after approval:

Refresh index:
```text
git -C C:\Projects\FigmaToCode-design-gen update-index --refresh -- tests/fixtures/generated/cart-layout-report.json tests/fixtures/generated/login-layout-report.json tests/fixtures/generated/mail-layout-report.json
```

Waive:
```text
Record waiver that these three paths are status-only/index/metadata residue with no content delta from checked evidence.
Do not claim fixture correctness or clean repo.
```

Restore:
```text
git -C C:\Projects\FigmaToCode-design-gen restore -- tests/fixtures/generated/cart-layout-report.json tests/fixtures/generated/login-layout-report.json tests/fixtures/generated/mail-layout-report.json
```

Owner-inspect:
```text
Inspect metadata/status cause for exactly the three paths.
Recommend refresh, waive, restore, or reclassify if content delta appears.
```

Rollback/recovery:
- Refresh index: no content rollback expected; rerun status and report.
- Waive: reverse by doc update.
- Restore: recover only from previous file copy/archive if content unexpectedly changes.
- Owner-inspect: no mutation if inspection-only.

Risk/consequence:
- Refresh index may alter index metadata.
- Restore may touch file content/metadata.
- Waiver leaves visible `M` status and blocks clean-repo claim.
- Owner-inspect may still require later action.

Artifacts/docs to update after execution:
- Figma fixture status successor report.
- Handoff owner-decision ledger.

Post-action verification:
- Repeat status, name-status, numstat, raw diff.
- Record whether `M` cleared.

Packet blocked notes:
- No secret blocker.
- Refresh/restore blocked by git index/working-tree mutation approval.

## Other Approval-Dependent Actions From Decision Workflow / Default Sweep

The following are not separate packet groups because they are covered above, but they remain explicitly approval-dependent:

- PR retarget, label, assign, edit title/body: use Packet Group 1 with PR action selector extended to the exact mutation; requires explicit PR mutation approval.
- Branch/ref delete, force update, push, retarget PR: use Packet Group 2; requires explicit branch/ref mutation approval.
- Stage/commit/push any residue: use Packet Groups 3, 4, 5, or 6; requires exact file list and commit/push approval.
- Archive/copy evidence/package/wiki/trace artifacts: use Packet Groups 3, 4, 5, or 6; requires destination and policy approval.
- Delete/clean/reset/restore/stash: use the relevant packet group; requires destructive/working-tree mutation approval.
- Browser/runtime/test/build execution: use Packet Group 8; requires scenario approval.
- External AI, provider credential, artifact upload: use Packet Group 7; currently `PACKET_BLOCKED_BY_MISSING_SECRET_OR_EXTERNAL_ACCOUNT` until provider/account/secret/data-use/rubric inputs exist.

## Safe Follow-Up Work Discovered

Safe follow-up work discovered:
- Add this report to `codex-designconversion-handoff-index-2026-06-28.md` so the handoff/RAG index includes `codex-approval-ready-execution-packets-2026-06-28.md`.
- Update the handoff index and completion handoff current core PR #6 head from older recorded SHA values to the latest work-order supplied SHA `14af29483ed5497df18ecb35edabeeeab3f23057`, with a boundary note that no live ref verification was run by this packet sweep.
- Update the completion handoff owner-decision ledger to reference this packet sweep as the execution-ready replacement for stop-on-approval blockers.
- Add a doc-only note that `explicit approval required is not final-stop authority under live continue`.
- No product/source repo edits are needed for this follow-up.
- No PR mutation, branch/ref mutation, stage, commit, push, test, build, browser, runtime, cleanup, archive, external AI, or credential action is part of this follow-up.

Exact safe follow-up work order for parent dispatch:
```text
Worker: bounded-approval-packet-index-inclusion-worker
Allowed write scope:
- C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-handoff-index-2026-06-28.md
- C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-designconversion-completion-handoff-2026-06-28.md
Allowed read scope:
- C:\Users\lubao\Documents\xiigen\llm mvp\design conversion handoff\codex-approval-ready-execution-packets-2026-06-28.md
- Existing same-folder handoff/workflow/sweep/classification reports.
Task:
- Add this approval-ready packet report to the handoff index/RAG list.
- Update current core PR #6 head references to `14af29483ed5497df18ecb35edabeeeab3f23057`, clearly downgrading to work-order supplied SHA unless live ref verification is separately run.
- Record that approval-dependent blockers now have execution packets and do not authorize a final stop under live continue.
- Preserve boundaries: not product completion, not merge readiness, not clean-repo proof, not full runtime proof, not full visual-fidelity proof, not external AI judge proof.
Forbidden:
- No product/source repo edits.
- No PR mutation.
- No branch/ref mutation.
- No stage/commit/push.
- No tests/builds/browser/runtime/external AI.
- No cleanup/archive/delete/stash/restore.
Verification:
- Targeted readback for the new report filename, latest SHA `14af29483ed5497df18ecb35edabeeeab3f23057`, and required boundary literals.
```

## Final Status

`APPROVAL_READY_EXECUTION_PACKET_SWEEP_COMPLETE`

`FINAL_ALLOWED_AFTER_APPROVAL_READY_EXECUTION_PACKET_SWEEP`

The evidence supports only: this report created approval-ready execution packets from existing handoff evidence and current work-order supplied latest core PR #6 head. It does not prove any packet was executed.

Safe executable action remains for parent dispatch:
- Yes, doc-only handoff/index inclusion and latest-SHA update as listed in `Safe Follow-Up Work Discovered`.

No unsafe action executed:
- no_git_actions: true
- no_pr_mutations: true
- no_branch_ref_mutations: true
- no_stage_commit_push: true
- no_merge_close_mark_ready: true
- no_cleanup_archive_delete_stash_restore: true
- no_tests_builds_browser_runtime: true
- no_external_ai: true
- no_secrets_used: true

must_stop_after_report=true
