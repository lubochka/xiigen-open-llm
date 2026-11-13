# Codex Pass87T Governance Merge Repair

claim_scope: Pass87T minimally repaired the current worktree governance text for
the OP-67 `origin/main` merge-conflict risk diagnosed by Pass87S. This packet is
only a governance/merge-risk repair record for `AGENTS.md` and
`docs/skills/claim-verification-SKILL.md`. It is not product readiness, not
implementation authorization, not a 50-pass claim, not a commit, not a push, not
a PR, not a branch switch, not a merge/rebase/stash/reset/clean, and not a
dotnet build/test result.

evidence_scope: Current-turn reads of the required prompt/skill/governance
files, Pass87R2 and Pass87S evidence reports, targeted current/HEAD/origin-main
snippets for `AGENTS.md` and `docs/skills/claim-verification-SKILL.md`,
`apply_patch` results for allowed files only, changed-section readback,
conflict-marker scans, `git diff --check`, two-argument `merge-tree` attempts,
no-write three-tree `merge-tree` scans, and worktree literal-preservation checks.

count_claimed: 1 bounded Pass87T governance repair packet. 0 product feature
edits. 0 production/test/Claude-file edits. 0 dotnet commands. 0 commits. 0
pushes. 0 PRs. 0 branch/ref/index writes. 0 full product readiness claims. 0
50-pass claims.

independent_evidence_count: 11 evidence groups, not 11 passes: required-file
presence/reads, Pass87R2 read, Pass87S read, OP-67 readback, current/HEAD/main
target-snippet comparison, allowed-file patches, changed-section readback,
conflict-marker scan, `git diff --check`, merge-tree scans, and worktree
literal-preservation checks.

subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT. No platform worker id is visible in
this context; this value is intentionally not `NONE`.

downgrade_needed: yes. The evidence supports only that the current worktree now
additively preserves the diagnosed main-side and HEAD/current-side governance
literals in the two allowed shared governance files. Clean committed-HEAD
mergeability into `origin/main` is still NOT proven because `merge-tree` scans
against committed `HEAD` still report `changed in both` for the same two files.

## skill_load_status

| file | status | evidence boundary |
| --- | --- | --- |
| `AGENTS.md` | found/read | OP-67 block and Pass87 target governance sections read back; full 343 KB file not reproduced. |
| `docs/SESSION-START-PROMPT.md` | found/read | `Get-Content -Raw` output observed. |
| `docs/skills/claim-verification-SKILL.md` | found/read | Required claim-verification sections and current changed sections read back. |
| `docs/skills/implementation-integrity-SKILL.md` | found/read | `Get-Content -Raw` output observed, including Check 16 OP-67 mergeability gate. |
| `docs/skills/audit-protocol-SKILL.md` | found/read | `Get-Content -Raw` output observed. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found/read | `Get-Content -Raw` output observed. |
| `docs/skills/verification-before-completion-SKILL.md` | found/read | `Get-Content -Raw` output observed, including evidence-before-completion and no-count-overclaim gates. |
| `docs/evidence/design-conversion/codex-pass87r2-mergeability-risk-report.md` | found/read | Full report output observed. |
| `docs/evidence/design-conversion/codex-pass87s-main-merge-conflict-diagnosis.md` | found/read | Full report output observed. |

## files_changed

```text
AGENTS.md
docs/skills/claim-verification-SKILL.md
docs/evidence/design-conversion/codex-pass87t-governance-merge-repair.md
```

Pre-existing dirty-file boundary: `AGENTS.md` was already modified before this
worker edited it. The pre-existing current-side 50-pass line near the Claude
Baseline Plan Authority block was observed before this worker's patch and is
not claimed as a Pass87T edit.

## changes_made

`AGENTS.md`:
- Added the missing main-side `Sub-agent error is recovery queue, not stop`
  recovery block after the existing `Sub-agent waiting is not a stop` block.
- Added the missing main-side `Sub-agent Stop-Loophole Repair Gate
  (2026-06-28)` block plus old-product-readiness, human-readable status,
  sub-agent-only enforcement, why-trigger continuation, and evidence-scope
  paragraphs before the bounded phase-chain rule.
- Preserved existing current-side/HEAD rules, including the
  `Source-to-Plan count verification must report the exact canonical row count`
  rule and the ILearningDynamicGraph 50-pass evidence rule.
- Preserved OP-67 literals:
  `clean-mergeable-into-original-and-main invariant`,
  `every commit must merge cleanly into BOTH original and main at any time`,
  `no add/add on generated evidence at shared paths`, and
  `mergeability check: clean into <original> AND main`.

`docs/skills/claim-verification-SKILL.md`:
- Added the missing main-side `Stop-loophole evidence wording gate
  (2026-06-28)`.
- Added the missing main-side `Sub-agent error is recovery queue, not stop`
  claim rejection block.
- Added the missing main-side `Exact evidence scopes` bullets.
- Preserved the current-side/HEAD row-count rule requiring the canonical row
  count, status histogram, and exact offender list for noncanonical table rows.

No production code, tests, `CLAUDE.md`, `claude-*` files, `.git` refs/index, or
external `C:\Users\lubao\Documents\xiigen` files were edited.

## changed_section_readback

Readback confirmed these current-worktree literals after the patch:

```text
AGENTS OK: Sub-agent error is recovery queue, not stop :: line 758
AGENTS OK: Sub-agent Stop-Loophole Repair Gate (2026-06-28) :: line 1515
AGENTS OK: Dual-path same-scenario coverage is product readiness evidence :: line 1527
AGENTS OK: Class B continuation gate :: line 973
AGENTS OK: Source-to-Plan count verification must report the exact canonical row count :: line 241
AGENTS OK: For Luba's ILearningDynamicGraph DPO original task :: line 162
AGENTS OK: clean-mergeable-into-original-and-main invariant :: line 6411
AGENTS OK: mergeability check: clean into <original> AND main :: line 6438
CLAIM OK: Stop-loophole evidence wording gate (2026-06-28) :: line 387
CLAIM OK: Sub-agent error is recovery queue, not stop :: line 396
CLAIM OK: Exact evidence scopes :: line 415
CLAIM OK: The verification packet must show the canonical row count, status histogram :: line 559
CLAIM OK: and the exact offender list for noncanonical table lines beginning :: line 560
```

## conflict_marker_scan

Command:

```powershell
Select-String -LiteralPath AGENTS.md, docs/skills/claim-verification-SKILL.md, docs/evidence/design-conversion/codex-pass87t-governance-merge-repair.md -Pattern '^<<<<<<<','^=======','^>>>>>>>'
```

Output:

```text
<empty>
```

Result: no conflict-marker lines were found in the two edited governance files
or this report file.

## diff_check

Pre-report command:

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- AGENTS.md docs/skills/claim-verification-SKILL.md
```

Output:

```text
<empty>
```

Result: pre-report whitespace check passed for the two edited governance files.

Final report-inclusive command:

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- AGENTS.md docs/skills/claim-verification-SKILL.md docs/evidence/design-conversion/codex-pass87t-governance-merge-repair.md
```

Output:

```text
<empty>
```

Result: final whitespace check passed for the two edited governance files and
this report file.

## merge_verification_main

Requested two-argument command:

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core merge-tree origin/main HEAD | Select-String -Pattern 'changed in both|^<<<<<<<|^=======$|^>>>>>>>'
```

Output:

```text
error: insufficient permission for adding an object to repository database .git/objects
error: error: Unable to add docs/skills/claim-verification-SKILL.md to database
error: insufficient permission for adding an object to repository database .git/objects
error: insufficient permission for adding an object to repository database .git/objects
error: insufficient permission for adding an object to repository database .git/objects
error: error: Unable to add AGENTS.md to database
error: insufficient permission for adding an object to repository database .git/objects
```

Accepted as clean proof: false. This command attempted object writes and hit the
same sandbox `.git/objects` permission failure recorded by Pass87R2.

No-write equivalent command:

```powershell
$refs = git -c safe.directory=C:/Projects/llm_mvp_core rev-parse origin/main...HEAD
$base = ($refs | Where-Object { $_ -like '^*' } | ForEach-Object { $_.Substring(1) } | Select-Object -First 1)
git -c safe.directory=C:/Projects/llm_mvp_core merge-tree $base origin/main HEAD | Select-String -Pattern 'changed in both|^<<<<<<<|^=======$|^>>>>>>>'
```

Output:

```text
base=54399063198d984944ad09ab56433e6a4e86af6e

changed in both
changed in both
```

Context readback:

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

Result: committed `HEAD` mergeability into `origin/main` is still not cleanly
proven. This is expected because `merge-tree ... HEAD` does not include the
uncommitted Pass87T worktree repair.

## merge_verification_original

Requested two-argument command:

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core merge-tree origin/DesignToCode-clean HEAD | Select-String -Pattern 'changed in both|^<<<<<<<|^=======$|^>>>>>>>'
```

Output:

```text
error: insufficient permission for adding an object to repository database .git/objects
```

Accepted as clean proof: false. This command attempted object writes and hit the
same sandbox `.git/objects` permission failure recorded by Pass87R2.

No-write equivalent command:

```powershell
$refs = git -c safe.directory=C:/Projects/llm_mvp_core rev-parse origin/DesignToCode-clean...HEAD
$base = ($refs | Where-Object { $_ -like '^*' } | ForEach-Object { $_.Substring(1) } | Select-Object -First 1)
git -c safe.directory=C:/Projects/llm_mvp_core merge-tree $base origin/DesignToCode-clean HEAD | Select-String -Pattern 'changed in both|^<<<<<<<|^=======$|^>>>>>>>'
```

Output:

```text
base=54399063198d984944ad09ab56433e6a4e86af6e
```

Result: no `changed in both` or conflict-marker scan hits were printed for the
committed-HEAD no-write check into `origin/DesignToCode-clean`.

## worktree_aware_risk_check

Current worktree literal-preservation checks found the diagnosed main-side
governance literals and HEAD/current-side row-count/50-pass/OP-67 literals in
the current files:

```text
AGENTS OK: Sub-agent error is recovery queue, not stop
AGENTS OK: Sub-agent Stop-Loophole Repair Gate (2026-06-28)
AGENTS OK: Dual-path same-scenario coverage is product readiness evidence
AGENTS OK: Class B continuation gate
AGENTS OK: Source-to-Plan count verification must report the exact canonical row count
AGENTS OK: For Luba's ILearningDynamicGraph DPO original task
CLAIM OK: Stop-loophole evidence wording gate (2026-06-28)
CLAIM OK: Sub-agent error is recovery queue, not stop
CLAIM OK: Exact evidence scopes
CLAIM OK: The verification packet must show the canonical row count, status histogram
CLAIM OK: and the exact offender list for noncanonical table lines beginning
```

Interpretation: committing a tree containing this worktree repair would likely
remove the semantic-loss part of the OP-67 risk for the two repaired governance
files because the current content now includes both diagnosed sides. This packet
does not claim clean mergeability into `origin/main`, because the required
committed-HEAD `merge-tree` scan still reports `changed in both` and commit/merge
actions were not authorized.

## commands_run

Allowed command families used: `Get-Item`, `Get-Content`, `Select-String`,
`git status`, `git show`, `git diff`, `git diff --check`, `git rev-parse`,
`git merge-tree`, and `apply_patch`.

Forbidden commands not run: no `git add`, `git commit`, `git push`,
`git checkout`, `git switch`, `git reset`, `git merge`, `git rebase`,
`git stash`, `git clean`, `dotnet`, network command, production/test edit, or
Claude-file write.

## mental_simulation

1. Main-side content: Pass87S identified main-side recovery, stop-loophole, and
   evidence-scope blocks missing from the current branch governance text. Those
   literals now exist in the current worktree.
2. HEAD/current-side content: row-count/offender-list rules, OP-67 literals, and
   the current 50-pass guard remain present after the additive patch.
3. Conflict-shape risk: the patch did not delete or rewrite existing Luba rules;
   it only added missing blocks. The content-level semantic-loss risk is reduced.
4. Merge-proof boundary: no-write merge-tree into `origin/main` still reports
   committed `HEAD` conflicts because uncommitted worktree edits are outside
   `HEAD`. A clean proof requires a committed/tree-based check or an authorized
   merge strategy, neither of which was allowed in Pass87T.

## pass_verdict

PASS87T_GOVERNANCE_REPAIR_PACKET_WRITTEN_WITH_MERGEABILITY_DOWNGRADE.

The governance text repair was made in the allowed files and preserves the
diagnosed main-side plus HEAD/current-side rules. Clean `origin/main`
mergeability is not claimed.

## remaining_gaps

- `git merge-tree` against committed `HEAD` still reports `changed in both` for
  `AGENTS.md` and `docs/skills/claim-verification-SKILL.md` into `origin/main`.
- Proving that the current repair fully resolves OP-67 clean-into-main requires
  an authorized committed-tree or merge-strategy check. Pass87T forbids commit,
  merge, rebase, stash, reset, switch, and staging.
- The broader dirty worktree still contains unrelated changes outside this
  repair scope; they were not edited or normalized.
- Dotnet was not run because the work order forbids it.
