# Codex Pass65 Mergeability Regression Repair

claim_scope: Pass65 extracted and repaired the current branch-level merge-tree conflict for a no-ref synthetic snapshot of the dirty `DesignToCode-clean` worktree plus selected untracked DesignConversion/evidence/training-data additions. This is synthetic mergeability evidence only.
evidence_scope: Required skill/context file load readbacks, `git fetch origin --prune`, temp-index no-ref synthetic snapshots, `git merge-tree` to `origin/main` and `origin/codex/chat-gpt-5.5`, blob/value inspection by `git show`, direct conflict-file readback, and scoped hash checks.
count_claimed: 1 sequential Pass65 repair. 0 commits, 0 pushes, 0 PRs, 0 branch switches, 0 actual merges, 0 rebases, 0 resets, 0 product readiness claims, 0 50-pass claims.
independent_evidence_count: 2 initial merge-tree target outputs plus 2 clean post-repair merge-tree target outputs before this artifact, with final after-artifact merge-tree recheck reported in the Pass65 return packet.
subagent_id_or_NONE: Pass65
downgrade_needed: no for branch-level synthetic mergeability after final recheck; yes for product readiness.

## mergeability

Targets:

- `origin/main` at `7af60091ad5b68014be916bccc0e552017b64ae0`
- `origin/codex/chat-gpt-5.5` at `489c4bbad2bb34c1cb644d937f64453cc08b088d`

Synthetic snapshot method:

- Built an alternate temporary index from current tracked index entries.
- Replaced entries with current working-tree blobs for tracked modified files.
- Removed tracked-deleted entries.
- Added selected untracked paths under `44.DesignConversionDomain/`, `Tests/DesignConversion*`, `Tests/ThinClient*`, `Tests/ProcessSpawning*`, `docs/evidence/`, `docs/plans/DESIGN-CONVERSION*`, and `training-data/design-conversion/`.
- Excluded unrelated untracked `artifacts/extension-runtime-token-quality-learnability/`, `artifacts/smart-skills-semantic-quality-completion/`, and `experiments-v2/` directories unless already tracked.
- Used `git hash-object`, `git update-index --index-info` with `GIT_INDEX_FILE` pointing to a temp index, `git write-tree`, and no-ref `git commit-tree`.
- Did not update refs and did not run `git merge`, checkout, switch, rebase, reset, clean, commit, push, or PR creation.

The first PowerShell pipeline/temp-index attempts were rejected as evidence because PowerShell inserted malformed/BOM-prefixed stdin into `git update-index --index-info` and one rejected tree was the empty tree. The accepted method used UTF-8-no-BOM temp index-info files and normal redirection into `git update-index --index-info`.

## initial_conflicts

Initial accepted synthetic snapshot: `5390538dae47abefb23e5b51d1c646fcfd8df46c`

Both targets reported the same exact content conflict:

```text
100644 354e8d6533ce90d8a3388c950f96b9e2148a2e13 1	artifacts/smart-skills-semantic-quality-completion/p22-skillselection-source-disjoint-runtime-eval/p22-source-disjoint-runtime-eval-report.json
100644 960237dd047c8371dec2e47691b489ee08705349 2	artifacts/smart-skills-semantic-quality-completion/p22-skillselection-source-disjoint-runtime-eval/p22-source-disjoint-runtime-eval-report.json
100644 8aa773fcf8bbfd12da20dc5ff52287316733097f 3	artifacts/smart-skills-semantic-quality-completion/p22-skillselection-source-disjoint-runtime-eval/p22-source-disjoint-runtime-eval-report.json
CONFLICT (content): Merge conflict in artifacts/smart-skills-semantic-quality-completion/p22-skillselection-source-disjoint-runtime-eval/p22-source-disjoint-runtime-eval-report.json
```

The exact conflict path was:

```text
artifacts/smart-skills-semantic-quality-completion/p22-skillselection-source-disjoint-runtime-eval/p22-source-disjoint-runtime-eval-report.json
```

Pass64 reported 9 conflict-relevant headers/lines, but Pass65 extracted the path-bearing conflict and found one unique conflict path.

## repair_performed

The only differing JSON field across base, target, and snapshot was `candidateCheckpointSha256`.

Observed values:

- base report field: `ae8fd9fc6dd4710d1d03279b200307f72257ae0792b4fc1a036b83678110968a`
- target/original report field: `005cdf3621b312b77e11d8a555d2197fc08c186d83a966edcd526b06b5394e96`
- initial snapshot report field: `bb859807a513efe39bc94570525864efbe4d493ef781ea6d0c19374a8714cea3`
- current worktree checkpoint file hash: `1ec83fa3d03d68b52c63abb12f2ee549208e3c88311ca26bd907bc5c33b8bc70`

Pass65 first tested a semantic normalization to the current checkpoint hash `1ec83fa3d03d68b52c63abb12f2ee549208e3c88311ca26bd907bc5c33b8bc70`; merge-tree still conflicted because the target branch had changed the same report field to `005cdf3621b312b77e11d8a555d2197fc08c186d83a966edcd526b06b5394e96`.

Final conflict-file-only repair:

- Set `candidateCheckpointSha256` in the report file to the target/original value `005cdf3621b312b77e11d8a555d2197fc08c186d83a966edcd526b06b5394e96`.
- This preserves target/original report normalization and clears the report-file content conflict without editing non-conflict files.
- The current checkpoint file still hashes to `1ec83fa3d03d68b52c63abb12f2ee549208e3c88311ca26bd907bc5c33b8bc70`; this is recorded as an evidence/artifact drift queue item, not hidden as product readiness.

Changed conflict file:

```text
artifacts/smart-skills-semantic-quality-completion/p22-skillselection-source-disjoint-runtime-eval/p22-source-disjoint-runtime-eval-report.json
```

## final_mergeability

Pre-artifact post-repair synthetic snapshot: `9b4ad0a41e6991992b2e34223a62c04f6f81fe54`

`git merge-tree origin/main 9b4ad0a41e6991992b2e34223a62c04f6f81fe54` output only:

```text
416f644cf01ab08679303b369b9744d2e1cad75e
```

`git merge-tree origin/codex/chat-gpt-5.5 9b4ad0a41e6991992b2e34223a62c04f6f81fe54` output only:

```text
416f644cf01ab08679303b369b9744d2e1cad75e
```

Final after-artifact merge-tree recheck is reported in the Pass65 return packet. The final synthetic object id is intentionally not embedded here because this evidence file is itself included in the final synthetic snapshot; embedding the object id would change the object id.

## verification

- `git fetch origin --prune` succeeded.
- Direct readback showed `candidateCheckpointSha256` set to `005cdf3621b312b77e11d8a555d2197fc08c186d83a966edcd526b06b5394e96`.
- Direct parse/hash check showed the report value no longer matches the current checkpoint worktree hash `1ec83fa3d03d68b52c63abb12f2ee549208e3c88311ca26bd907bc5c33b8bc70`; this drift is recorded rather than hidden.
- Pre-artifact post-repair `git merge-tree` to both targets had zero `CONFLICT` lines and output only the same tree hash.
- Final after-artifact `git merge-tree` and `git diff --check` are reported in the Pass65 final return, not embedded here to avoid self-reference churn.

## mental_simulation

If Luba attempts a PR/merge using a commit equivalent to the final synthetic snapshot after this pass, Git should not hit the Pass64/Pass65 report-file conflict by `git merge-tree` evidence. This is synthetic-only local Git evidence, not GitHub PR UI evidence and not an actual merge.

## product_not_ready

product_not_ready: true

This pass proves only branch-level synthetic mergeability for the scoped dirty snapshot after final recheck. It does not prove product readiness, full runtime evidence, provider evidence, visual QA readiness, training/promotion safety, or 50 passes.

## remaining_queue

1. Audit whether the current `p17e` production-export checkpoint drift to `1ec83fa3d03d68b52c63abb12f2ee549208e3c88311ca26bd907bc5c33b8bc70` is intended. This pass did not edit that non-conflict file.
2. Keep product readiness blocked until real source/DPO/provider/render/runtime evidence gates are satisfied.
3. Recheck mergeability after any future edits because the worktree is dirty and this proof is synthetic-only.
