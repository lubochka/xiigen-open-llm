# Codex Pass96B-GOV2 Materialization Method Repair

claim_scope: Pass96B-GOV2 diagnosis, governance repair, and next-worker method
for the failed/incomplete Pass96OP67B candidate-materialization slice.
evidence_scope: Current Luba work order, targeted reads of required OP-67 and
verification rules, full reads of the named Pass96GOV/Pass96OP67A/Pass96OP67B
evidence files, narrow edits to governance files allowed by the work order, and
this evidence file. No candidate materialization, temp clone/copy/workspace,
merge-tree proof, product/test edit, test/build, git stage/commit/push/switch/
checkout/merge/rebase/reset/stash/clean, `grep.exe.stackdump` deletion, or
write outside `C:\Projects\llm_mvp_core` was performed.
count_claimed: 1 method-repair evidence file written; 3 governance files
patched; 0 candidate materialization runs; 0 merge-tree proof runs; 0 product/
test/build runs; 0 git write/ref actions.
independent_evidence_count: 0 OP-67 proof records claimed. The evidence here is
method/governance evidence only, not a mergeability proof or completed
materialization packet.
subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT
downgrade_needed: yes. The evidence supports only a repaired method and narrow
governance guard. OP-67 clean mergeability and candidate materialization remain
not proven.

## Skill Load Status

| file | status | read boundary |
| --- | --- | --- |
| `AGENTS.md` Pass96GOV and OP-67 | found / targeted-read | Read Pass96GOV OP-67 atomic worker gate, ordered slices, timeout/no-result wording, and OP-67 clean-mergeability invariant. Patched the missing candidate-materialization retry subgate. |
| `docs/skills/claim-verification-SKILL.md` | found / targeted-read | Read evidence scope, sub-agent timeout recovery, OP-67 timeout claim gate, and count/claim downgrade rules. Patched the missing candidate-materialization retry claim gate. |
| `docs/skills/implementation-integrity-SKILL.md` | found / targeted-read | Read OP-67 temp-proof integrity gate and clean-mergeability integrity context. Patched the missing candidate-materialization retry integrity gate. |
| `docs/skills/verification-before-completion-SKILL.md` | found / targeted-read | Read evidence-before-claim and completion verification boundaries. No patch needed. |
| `docs/evidence/design-conversion/codex-pass96op67a-inventory-only.md` | found / read | Read inventory packet: candidate include total 80, `grep.exe.stackdump` excluded/noise, no materialization/proof run. |
| `docs/evidence/design-conversion/codex-pass96op67b-candidate-materialization.md` | found / read | Read early artifact: `IN_PROGRESS_EARLY_ARTIFACT`, pending skill load/status/count/hash/verification/final verdict, 0 completed passes. |
| `docs/evidence/design-conversion/codex-pass96gov-op67-timeout-workorder-repair.md` | found / read | Read Pass96GOV work-order repair: OP-67 proof still not proven, next work split into atomic slices. |

## Previous Failure Diagnosis

previous_failure_diagnosis: METHOD_TOO_BROAD_OR_LONG_COPY_SCRIPT_INTERRUPTED_NO_FINAL_PACKET.

Evidence supports only this narrow diagnosis:

- Pass96OP67A provided an inventory input, not a materialized candidate:
  include total 80 and exclude `grep.exe.stackdump`.
- Pass96OP67B created only an early artifact. Its sections for skill load,
  inventory drift, temp path containment, materialization counts, candidate hash
  tree status, verification, and final verdict all remained `pending`.
- The current work order reports no temp path per parent check and no candidate
  hash/tree/status.

I did not verify the exact runtime cause. The supported classification is:
the candidate-materialization work order still allowed a too-broad/too-long
materialization step after the early artifact, and the worker returned no final
packet. That counts as zero candidate materialization and zero OP-67 proof.

## Governance Repair

new_governance_rules_if_any:

- `AGENTS.md`: added a candidate-materialization retry subgate. After an
  incomplete `candidate-materialization-only` slice, the next authorized
  materialization worker must micro-slice instead of running one broad all-path
  copy/script.
- `docs/skills/claim-verification-SKILL.md`: added the matching claim gate so
  another broad materialization claim is rejected after a B-style failure.
- `docs/skills/implementation-integrity-SKILL.md`: added the matching integrity
  gate so the retry must name temp path, avoid original `.git` writes/global
  git config, and record heartbeat evidence before long copy work.

No patch was needed in `docs/skills/verification-before-completion-SKILL.md`.

## Recommended Materialization Method

recommended_materialization_method: PATCH_BUNDLE_PLUS_MICRO_SLICES.

The next worker should be a materialization-only worker, not a proof worker.
It should write/read back its own evidence file before creating any temp path,
then proceed one micro-slice at a time. Commands below are for that next worker
only; they were not run in Pass96B-GOV2.

1. Preflight manifest gate.

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core status --short --untracked-files=all
git -c safe.directory=C:/Projects/llm_mvp_core diff --name-status HEAD --
git -c safe.directory=C:/Projects/llm_mvp_core ls-files --others --exclude-standard
```

Record whether the manifest mode is `OP67A_80_SNAPSHOT` or
`CURRENT_DIRTY_WORKTREE`. Do not silently reuse the OP67A count after OP67B/GOV2
changed evidence. If the mode is `OP67A_80_SNAPSHOT`, use exactly the OP67A
80 includes and exclude `grep.exe.stackdump`. If the mode is
`CURRENT_DIRTY_WORKTREE`, recompute the exact include count, list every delta
from OP67A, and still exclude `grep.exe.stackdump` unless Luba explicitly
reclassifies it. Abort with `NEEDS_FIX` if the manifest is not exact.

2. Early evidence and temp-path declaration.

Write/read back the evidence file section with: source repo, manifest mode,
expected counts by class, candidate temp path, cleanup owner, and the statement
`no original .git writes; no global git config; no merge-tree in this slice`.
Only then create a unique temp path outside the repo, for example
`C:\tmp\codex-pass96op67c-candidate-<timestamp>`.

3. Base temp repo creation.

Use a temp clone/repo only after the early evidence readback. Do not use
`git worktree` because it writes original `.git/worktrees`.

```powershell
git clone --no-local --no-checkout C:/Projects/llm_mvp_core $candidateRepo
git -C $candidateRepo -c advice.detachedHead=false checkout --detach $manifestHead
git -C $candidateRepo status --short --untracked-files=all
```

Record the temp `HEAD` and require a clean temp status before applying any
candidate slice.

4. Micro-slice A: tracked governance/evidence.

Create a binary patch for only the tracked governance/evidence paths from the
manifest, apply it inside the temp repo, then read back name-status and counts.

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core diff --binary HEAD -- <tracked-governance-evidence-paths> > $bundle\01-tracked-governance-evidence.patch
git -C $candidateRepo apply --binary $bundle\01-tracked-governance-evidence.patch
git -C $candidateRepo diff --name-status HEAD -- <tracked-governance-evidence-paths>
```

5. Micro-slice B: tracked product/tests.

Repeat the same patch/apply/readback pattern for only tracked product and test
paths. Count expected and actual modified paths before continuing.

6. Micro-slice C: untracked includes.

Copy exact untracked include paths from a manifest file, not a glob. Split this
into subgroups if the list is large: product/tests first, evidence second. After
each subgroup, count copied files and compare source/destination SHA256 hashes.

```powershell
$paths = Get-Content -LiteralPath $manifestUntrackedIncludeList
foreach ($p in $paths) {
  $src = Join-Path $sourceRepo $p
  $dst = Join-Path $candidateRepo $p
  New-Item -ItemType Directory -Force -LiteralPath (Split-Path -Parent $dst) | Out-Null
  Copy-Item -LiteralPath $src -Destination $dst -Force
}
```

7. Micro-slice D: tracked artifact deletions.

Apply tracked artifact deletions last through a binary patch or exact path
removal inside the temp repo only. Then read back the deletion count.

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core diff --binary HEAD -- <tracked-deleted-artifact-paths> > $bundle\04-tracked-deletions.patch
git -C $candidateRepo apply --binary $bundle\04-tracked-deletions.patch
git -C $candidateRepo diff --name-status HEAD -- <tracked-deleted-artifact-paths>
```

8. Final materialization-only readback.

```powershell
git -C $candidateRepo status --short --untracked-files=all
git -C $candidateRepo diff --name-status HEAD --
git -C $candidateRepo ls-files --others --exclude-standard
```

The final packet may claim only candidate materialization if the expected count
equals actual count, `grep.exe.stackdump` is absent, all copied untracked hashes
match, and the evidence file was read back after every micro-slice. It must not
claim mergeability, OP-67 proof, tests, builds, or product readiness.

## Files Changed

files_changed:

- `AGENTS.md`
- `docs/skills/claim-verification-SKILL.md`
- `docs/skills/implementation-integrity-SKILL.md`
- `docs/evidence/design-conversion/codex-pass96b-gov2-materialization-method-repair.md`

Not changed: product/test files, `C:\Users\lubao\Documents\xiigen`,
`claude-*`, temp proof workspaces, git refs/config, or `grep.exe.stackdump`.

## Verification

verification:

- evidence readback: PASS. `Get-Content -Raw` read back this file.
- governance line readbacks: PASS. `Select-String` read back the new
  candidate-materialization retry rules in `AGENTS.md`,
  `docs/skills/claim-verification-SKILL.md`, and
  `docs/skills/implementation-integrity-SKILL.md`.
- conflict marker scan: PASS. `rg -n "^(<<<<<<<|=======|>>>>>>>)"` on the
  touched files returned no matches.
- configured mojibake marker scan: PASS. `rg -n "\x{00D0}|\x{00D1}|\x{00C2}|\x{00E2}|\x{FFFD}"`
  on the touched files returned no matches.
- tracked touched-file whitespace check: PASS. `git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- AGENTS.md docs/skills/claim-verification-SKILL.md docs/skills/implementation-integrity-SKILL.md`
  exited 0 with no output.
- evidence trailing-whitespace scan: PASS. `rg -n "[ \t]+$"` on this evidence
  file returned no matches.

## Next Recommended Worker

next_recommended_worker: Pass96OP67C candidate-materialization micro-slice
worker.

The worker should materialize only. It should not run merge-tree proof, tests,
builds, product implementation, staging, commits, branch switches, checkout/
merge/rebase/reset/stash/clean in the original repo, or cleanup outside the
declared temp path.

## Final Verdict

final_verdict: PASS96B_GOV2_METHOD_REPAIR_WRITTEN_AND_VERIFIED_OP67_PROOF_STILL_NOT_PROVEN.

I did not run candidate materialization. I did not create a temp clone/copy/
workspace. I did not run merge-tree proof. I did not run tests or builds. I did
not stage, commit, push, switch, checkout, merge, rebase, reset, stash, clean,
or delete `grep.exe.stackdump`. The evidence supports only the diagnosis,
method repair, and governance patch described above.
