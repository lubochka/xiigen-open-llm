# Codex Pass87R2 Mergeability Risk Report

claim_scope: Pass87R2 produced a bounded mergeability/governance evidence packet
for OP-67 on the current `DesignToCode-clean` slice. This is not product
readiness, not full dirty-worktree merge proof, not a commit, not a push, not a
PR, not a branch switch, not a real merge, not a rebase/reset/stash/clean, and
not a dotnet build/test result.

evidence_scope: Current-turn file existence/read outputs, OP-67 readback from
`AGENTS.md`, required skill/context file reads, branch/ref/status outputs,
temp-dir existence checks, no-write merge-tree attempts, dirty tracked overlap
checks, untracked path-collision checks, this file readback, and `git diff
--check` over this file. Existing Pass87 and prior packets are evidence only.

count_claimed: 1 bounded Pass87R2 evidence packet. 0 product feature edits. 0
production/test/Claude file edits. 0 dotnet builds/tests. 0 commits. 0 pushes.
0 PRs. 0 git write/ref actions. 0 temp dirs removed because both target temp dirs
were absent.

independent_evidence_count: 12 evidence groups, not 12 passes: required-file
existence, OP-67 readback, required skill/context reads, branch/ref state,
temp-dir before/after checks, two-argument merge-tree permission failure
evidence, no-write merge-tree original check, no-write merge-tree main check,
status inventory, dirty tracked overlap checks, untracked add/add collision
checks including this report path, and final report readback/diff-check.

subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT. No platform sub-agent id is visible in
this worker context; this value is intentionally not `NONE`.

downgrade_needed: yes. Evidence supports only the narrow statements below:
committed `HEAD` has no actual no-write merge-tree conflict headers into
`origin/DesignToCode-clean`; committed `HEAD` is not cleanly proven into
`origin/main` because no-write merge-tree reports actual `changed in both`
sections; all-dirty working-tree mergeability is NOT FULLY PROVEN.

## skill_load_status

| file | status | evidence boundary |
| --- | --- | --- |
| `AGENTS.md` | found/read | OP-67 section read back from lines 6338-6385; full 343 KB file was not reproduced here. |
| `docs/SESSION-START-PROMPT.md` | found/read | `Get-Content -Raw` output observed. |
| `docs/skills/claim-verification-SKILL.md` | found/read | `Get-Content -Raw` output observed; display truncation occurred, so the packet does not reproduce every line. |
| `docs/skills/verification-before-completion-SKILL.md` | found/read | `Get-Content -Raw` output observed. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read | `Get-Content -Raw` output observed, including Check 16 OP-67 mergeability gate. |
| `docs/skills/audit-protocol-SKILL.md` | found/read | `Get-Content -Raw` output observed. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found/read | `Get-Content -Raw` output observed. |

Notes:
- Several required files contain existing mojibake in readback. This packet did
  not edit those files and does not claim they are clean.
- A broad `rg` search over `AGENTS.md docs` failed with a sandbox
  `deny-read ACLs` error, so it is not used as proof. Exact file reads/searches
  are used instead.

## OP-67 literals

OP-67 was found in `AGENTS.md` at lines 6338-6385. Required exact literals read
back:

```text
clean-mergeable-into-original-and-main invariant
every commit must merge cleanly into BOTH original and main at any time
no add/add on generated evidence at shared paths
mergeability check: clean into <original> AND main
```

## context_files_read

| file | status | evidence boundary |
| --- | --- | --- |
| `docs/evidence/design-conversion/codex-pass87-local-full-loop-e2e.md` | found/read | Existing Pass87 local full-loop packet read as evidence only; it is not treated as product readiness. |

## branch_ref_state

Current branch/ref evidence:

```text
current_branch=DesignToCode-clean
HEAD=c25687041b123e6396aff946f944da6ca06733bd
origin/DesignToCode-clean=54399063198d984944ad09ab56433e6a4e86af6e
origin/main=ded0a6d8946db888f8398b8477c9c66c60d3e37e
preferred_original_ref=origin/DesignToCode-clean
preferred_main_ref=origin/main
fallback_refs_used=false
```

`git status --short --branch --untracked-files=all` reported:

```text
## DesignToCode-clean...origin/DesignToCode-clean [ahead 1]
tracked dirty paths present
untracked paths present
warnings: unable to access C:\Users\lubao/.config/git/ignore; missing askpass directory under artifacts
```

## temp_cleanup

Temp paths checked:

```text
.codex-patch-merge-pass87p before=false
.codex-patch-merge-pass87q before=false
remove_action=none
.codex-patch-merge-pass87p after=false
.codex-patch-merge-pass87q after=false
```

No temp directory was removed because both exact paths were absent.

## committed_head_mergeability_original

Target: `origin/DesignToCode-clean`.

The requested two-argument form was attempted first:

```text
git -c safe.directory=C:/Projects/llm_mvp_core merge-tree origin/DesignToCode-clean HEAD
output included tree id 6c418a2b45fc04e59fd34a6790310604f2ddd5d3
output also included: error: insufficient permission for adding an object to repository database .git/objects
accepted_as_proof=false
```

Because that form attempted to write objects and printed permission errors, I
used the no-write three-tree form with base resolved from
`git rev-parse origin/DesignToCode-clean...HEAD`:

```text
git -c safe.directory=C:/Projects/llm_mvp_core rev-parse origin/DesignToCode-clean...HEAD
c25687041b123e6396aff946f944da6ca06733bd
54399063198d984944ad09ab56433e6a4e86af6e
^54399063198d984944ad09ab56433e6a4e86af6e

git -c safe.directory=C:/Projects/llm_mvp_core merge-tree 54399063198d984944ad09ab56433e6a4e86af6e origin/DesignToCode-clean HEAD
exit=0
line_count=1086
actual_header_matches_for_changed_in_both_added_in_both_removed_in_CONFLICT=0
```

Raw broad marker scans found historical `CONFLICT` strings inside added evidence
file content. Tight actual-header extraction found no actual merge-tree conflict
headers for this target.

Result: committed `HEAD` into `origin/DesignToCode-clean` is clean by the
no-write merge-tree header scan used in this packet.

## committed_head_mergeability_main

Target: `origin/main`.

The requested two-argument form was attempted first:

```text
git -c safe.directory=C:/Projects/llm_mvp_core merge-tree origin/main HEAD
output included tree id e04c4c2b25a4851f89aa1efe6af16f8451172b16
output also included: error: insufficient permission for adding an object to repository database .git/objects
output also included: error: Unable to add docs/skills/claim-verification-SKILL.md to database
output also included: error: Unable to add AGENTS.md to database
accepted_as_proof=false
```

Because that form attempted to write objects and printed permission errors, I
used the no-write three-tree form with base resolved from
`git rev-parse origin/main...HEAD`:

```text
git -c safe.directory=C:/Projects/llm_mvp_core rev-parse origin/main...HEAD
c25687041b123e6396aff946f944da6ca06733bd
ded0a6d8946db888f8398b8477c9c66c60d3e37e
^54399063198d984944ad09ab56433e6a4e86af6e

git -c safe.directory=C:/Projects/llm_mvp_core merge-tree 54399063198d984944ad09ab56433e6a4e86af6e origin/main HEAD
exit=0
line_count=1088
actual_header_matches_for_changed_in_both=2
```

Actual no-write merge-tree header sections:

```text
changed in both
  base   100644 c9ba796e26dbb39f774b9c4b7540caf600d8702b AGENTS.md
  our    100644 8c83b40774e84a71e98774aae176c871269f9dda AGENTS.md
  their  100644 7094729225c6d43613ade87c476fc85c4eaf1c2f AGENTS.md

changed in both
  base   100644 6967f3372ab5ce52d85edee858fc3f813bfd613d docs/skills/claim-verification-SKILL.md
  our    100644 d1924028a0627159139c0dbe3fa092b393741f03 docs/skills/claim-verification-SKILL.md
  their  100644 6c6b6f402e96b6297ecfbd733db226b0eed7bb22 docs/skills/claim-verification-SKILL.md
```

Result: committed `HEAD` into `origin/main` is NOT cleanly proven. Per the
work-order scan terms, `changed in both` is a mergeability risk/fail marker for
OP-67 evidence.

## dirty_inventory_before_report_file

`git -c safe.directory=C:/Projects/llm_mvp_core status --porcelain=v1
--untracked-files=all` was parsed before this report file was written:

```text
tracked_dirty_count=25
untracked_count=20
status_warnings=git ignore permission warnings and missing askpass directory warning
```

Tracked dirty paths:

```text
44.DesignConversionDomain/DesignConversionBrowserQaProviders.cs
44.DesignConversionDomain/DesignConversionDpoDatasetBuilder.cs
44.DesignConversionDomain/DesignConversionDpoDisplay.cs
44.DesignConversionDomain/DesignConversionDpoStorage.cs
44.DesignConversionDomain/DesignConversionTrainingPipeline.cs
44.DesignConversionDomain/DesignConversionVisualJudge.cs
44.DesignConversionDomain/DesignConversionVisualQaBoundaryPolicy.cs
AGENTS.md
artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass-empty.sh
artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass-main.js
artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/askpass.sh
artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/ssh-askpass-empty.sh
artifacts/extension-runtime-token-quality-learnability/phase-10-cursor-full-extension-live-proof/run-20260623T1405422594307Z/code/user-data/User/globalStorage/vscode.git/askpass/493597c785d4c5cb/ssh-askpass.sh
CLAUDE.md
Tests/DesignConversionBrowserQaProviderTests.cs
Tests/DesignConversionDpoStorageTests.cs
Tests/DesignConversionInternalServiceTests.cs
Tests/DesignConversionVisualJudgeTests.cs
Tests/DesignConversionVisualQaBoundaryPolicyTests.cs
docs/SESSION-START-PROMPT.md
docs/skills/authority-requirement-binding-SKILL.md
docs/skills/coverage-completeness-gate-SKILL.md
docs/skills/phase-planning-SKILL.md
docs/skills/planning-session-startup-SKILL.md
docs/skills/verification-before-completion-SKILL.md
```

Untracked paths:

```text
44.DesignConversionDomain/DesignConversionDpoUsefulnessApi.cs
44.DesignConversionDomain/DesignConversionLocalArtifactDb.cs
44.DesignConversionDomain/DesignConversionTrainingMeasurementGate.cs
Tests/DesignConversionDpoUsefulnessApiTests.cs
Tests/DesignConversionLocalFullLoopE2eTests.cs
Tests/DesignConversionTrainingMeasurementGateTests.cs
docs/evidence/design-conversion/codex-pass74-live-host-probe.md
docs/evidence/design-conversion/codex-pass75-platform-gateway-proxy.md
docs/evidence/design-conversion/codex-pass76-platform-flow-two-process-probe.md
docs/evidence/design-conversion/codex-pass77-caller-client-to-platform-flow.md
docs/evidence/design-conversion/codex-pass78-browser-render-qa.md
docs/evidence/design-conversion/codex-pass79-visual-judge-provider-boundary.md
docs/evidence/design-conversion/codex-pass80-all-dirty-mergeability.md
docs/evidence/design-conversion/codex-pass81-storage-training-artifact-db.md
docs/evidence/design-conversion/codex-pass82-training-measurement-gate.md
docs/evidence/design-conversion/codex-pass83-all-dirty-mergeability.md
docs/evidence/design-conversion/codex-pass84-holistic-readiness-simulation.md
docs/evidence/design-conversion/codex-pass85-post-pass84-mergeability.md
docs/evidence/design-conversion/codex-pass86r-dpo-usefulness-api-verification.md
docs/evidence/design-conversion/codex-pass87-local-full-loop-e2e.md
```

This file itself is an additional untracked path after creation. Separate
target-tree collision checks for this report path returned no output for both
`origin/DesignToCode-clean` and `origin/main`.

## dirty_tracked_overlap_original

Target: `origin/DesignToCode-clean`.

```text
git -c safe.directory=C:/Projects/llm_mvp_core diff --name-only HEAD origin/DesignToCode-clean
target_changed_path_count=8

dirty_tracked_overlap_count=1
AGENTS.md
```

Risk boundary: this is an overlap-risk list only. It does not prove content
application for dirty tracked files.

## dirty_tracked_overlap_main

Target: `origin/main`.

```text
git -c safe.directory=C:/Projects/llm_mvp_core diff --name-only HEAD origin/main
target_changed_path_count=348

dirty_tracked_overlap_count=2
AGENTS.md
CLAUDE.md
```

Risk boundary: this is an overlap-risk list only. It does not prove content
application for dirty tracked files.

## untracked_add_add_original

Target: `origin/DesignToCode-clean`.

```text
git -c safe.directory=C:/Projects/llm_mvp_core ls-tree -r --name-only origin/DesignToCode-clean
untracked_add_add_collision_count=0
collision_paths=[]
```

Risk boundary: this proves no untracked path-name collision against the target
tree for the 20 pre-report untracked paths. It does not prove content merge,
runtime behavior, or product readiness.

## untracked_add_add_main

Target: `origin/main`.

```text
git -c safe.directory=C:/Projects/llm_mvp_core ls-tree -r --name-only origin/main
untracked_add_add_collision_count=0
collision_paths=[]
```

Risk boundary: this proves no untracked path-name collision against the target
tree for the 20 pre-report untracked paths. It does not prove content merge,
runtime behavior, or product readiness.

## mental_simulation

1. Committed `HEAD` into original/base:
   no-write merge-tree against `origin/DesignToCode-clean` has exit 0 and no
   actual conflict headers from the requested marker family. This narrow
   committed-head check is clean.

2. Committed `HEAD` into main:
   no-write merge-tree against `origin/main` has exit 0 but reports two actual
   `changed in both` sections: `AGENTS.md` and
   `docs/skills/claim-verification-SKILL.md`. Under OP-67 scan terms this is
   not clean mergeability evidence.

3. Dirty tracked overlap:
   current dirty tracked files overlap target-vs-HEAD changed paths in
   `AGENTS.md` for original/base and in `AGENTS.md`, `CLAUDE.md` for main.
   Therefore all-dirty mergeability is NOT FULLY PROVEN.

4. Untracked add/add risk:
   0 pre-report untracked path collisions against both target trees. This lowers
   path-collision risk but does not prove content merge or execution.

## commands_run

Allowed command families used: `Get-ChildItem`, `Get-Content`, `Select-String`,
`Test-Path`, `git status`, `git rev-parse`, `git show-ref`, `git merge-tree`,
`git diff`, `git ls-tree`, and final `git diff --check`. One broad `rg` command
was attempted and failed with sandbox ACL denial; it was not used as evidence.

Forbidden commands not run: no `git add`, `git commit`, `git push`,
`git checkout`, `git switch`, `git reset`, `git merge`, `git rebase`,
`git stash`, `git clean`, `dotnet`, network, provider calls, or external repo
writes.

## pass_verdict

PASS87R2 evidence packet produced, with a mergeability downgrade:

```text
committed_head_mergeability_original=clean_by_no_write_header_scan
committed_head_mergeability_main=not_cleanly_proven_changed_in_both_AGENTS_and_claim_verification
dirty_tracked_overlap_original=1 path: AGENTS.md
dirty_tracked_overlap_main=2 paths: AGENTS.md, CLAUDE.md
untracked_add_add_original=0
untracked_add_add_main=0
all_dirty_mergeability=NOT_FULLY_PROVEN
product_readiness_claim=false
```

## remaining_gaps

- Resolve or reconcile committed `HEAD` vs `origin/main` `changed in both`
  sections for `AGENTS.md` and
  `docs/skills/claim-verification-SKILL.md` before any OP-67 clean-into-main
  claim.
- Full all-dirty content mergeability remains unproven; tracked dirty overlap
  requires either a safe full dirty no-write method or bounded reconciliation.
- The two-argument `git merge-tree <target> HEAD` form printed `.git/objects`
  permission errors and was not accepted as proof.
- Dotnet builds/tests were not run because the work order forbade them.
- Existing git status warnings about user git ignore permissions and a missing
  askpass artifact directory remain evidence warnings, not repaired in this
  pass.
