# Codex Pass87GOV1 Sub-agent Micro-check Loop Repair

Date: 2026-07-02

claim_scope: Pass87GOV1 minimally repaired governance/claim rules for the
sub-agent micro-check loop loophole described by Luba. This packet covers only
`AGENTS.md`, `docs/skills/claim-verification-SKILL.md`, and this evidence
report. It is not product readiness, not mergeability clean, not a product
feature implementation, not a build/test result, not a commit, not a push, not
a PR, and not a staged change.

evidence_scope: Current-turn reads of the required governance/skill files,
targeted OP-67/sub-agent/claim-gate readbacks, current Pass87 evidence reports
when present, additive patches to the allowed files, changed-section readback,
marker search, `git diff --check`, conflict-marker scan, and evidence-file
readback over the changed files.

count_claimed: 1 bounded governance repair packet. 0 product feature edits. 0
production/test code edits. 0 dotnet commands. 0 merge-tree proofs. 0 commits. 0
pushes. 0 PRs. 0 staged files. 0 product readiness claims.

independent_evidence_count: 10 evidence groups, not 10 passes or review cycles:
required-file reads, targeted AGENTS gate/OP-67 readbacks, targeted
claim-verification readbacks, Pass87 context report reads, patch application,
changed-section readback, marker search, `git diff --check`, conflict-marker
scan, and evidence-report readback.

subagent_id_or_NONE: UNKNOWN_IN_SUBAGENT. No platform worker id is visible in
this context; this value is intentionally not `NONE`.

downgrade_needed: yes. The evidence supports only a governance/claim-rule
repair for the incomplete micro-check packet pattern. It does not prove product
completion, mergeability, or any worker pass count.

## skill_load_status

| file | status | evidence boundary |
| --- | --- | --- |
| `AGENTS.md` | found/read | `Get-Content` ran; targeted OP-67, no-overclaim, skill-load, sub-agent identity, firewall, and sub-agent error sections were read back. |
| `docs/SESSION-START-PROMPT.md` | found/read | `Get-Content` output was observed for required skill-load context. |
| `docs/skills/claim-verification-SKILL.md` | found/read | `Get-Content` ran; targeted sub-agent identity, high-risk claim, stop-loophole, sub-agent error, and evidence-scope sections were read back. |
| `docs/skills/verification-before-completion-SKILL.md` | found/read | `Get-Content` output was observed for required skill-load context. |
| `docs/skills/audit-protocol-SKILL.md` | found/read | `Get-Content` output was observed for required skill-load context. |
| `docs/skills/reconnaissance-gate-SKILL.md` | found/read | `Get-Content` output was observed for required skill-load context. |
| OP-67 in `AGENTS.md` | found/read | The `clean-mergeable-into-original-and-main invariant` block and required literals were read back. |
| Existing sub-agent/claim gates in `AGENTS.md` | found/read | No-overclaim, skill-load, identity, firewall, sub-agent error, and continuation gates were read back in targeted sections. |

## context_reports_read

| file | status | evidence boundary |
| --- | --- | --- |
| `docs/evidence/design-conversion/codex-pass87t-governance-merge-repair.md` | found/read | Read as evidence-only context. |
| `docs/evidence/design-conversion/codex-pass87r2-mergeability-risk-report.md` | found/read | Read as evidence-only context. |
| `docs/evidence/design-conversion/codex-pass87s-main-merge-conflict-diagnosis.md` | found/read | Read as evidence-only context. |

## loophole_classification

The repeated Lorentz/Epicurus/Dalton pattern is classified as a governance gap:
existing rules rejected errored sub-agents and count inflation, but did not
explicitly reject a spawned worker packet that attempted setup, reported `0
completed checks`, left cleanup unknown, or converted a parent/tool
interruption into a user interruption. The gap also allowed the parent to retry
the same composite temp-script work-order shape instead of repairing the
prompt/governance layer after repeated incomplete packets.

## changes_made

`AGENTS.md`:
- Added `Sub-agent micro-check incomplete packet recovery gate` after the
  existing `Sub-agent error is recovery queue, not stop` rule.
- The rule states that `0 completed checks` is not a pass, requires
  atomic command/result before composite scripts, forbids mislabelling
  parent/tool interruption as user interruption, requires cleanup proof after
  abort/timeout, and forces governance/prompt repair after two repeated
  0-check incomplete packets.

`docs/skills/claim-verification-SKILL.md`:
- Added the same gate as a claim-verification rejection rule near the existing
  sub-agent error and evidence-scope rules.
- The rule downgrades incomplete micro-check packets to failure-diagnosis
  evidence only and blocks product continuation when cleanup proof is unknown.

This report:
- Records the bounded governance repair, evidence boundary, skill-load status,
  mental simulation, and verification results.

## files_changed

```text
AGENTS.md
docs/skills/claim-verification-SKILL.md
docs/evidence/design-conversion/codex-pass87gov1-subagent-microcheck-loop-repair.md
```

## readback_markers

Changed-section readback found the new gate in both rule files:

```text
AGENTS.md:787:Sub-agent micro-check incomplete packet recovery gate: when a spawned worker is
AGENTS.md:803:- `Sub-agent micro-check incomplete packet recovery gate`
AGENTS.md:804:- `0 completed checks is not a pass`
AGENTS.md:805:- `atomic command/result before composite script`
AGENTS.md:806:- `unknown cleanup routes to cleanup worker`
AGENTS.md:807:- `after two 0-check incomplete packets repair the work-order shape`
docs/skills/claim-verification-SKILL.md:415:Sub-agent micro-check incomplete packet recovery gate: claim verification must
docs/skills/claim-verification-SKILL.md:432:- `Sub-agent micro-check incomplete packet recovery gate`
docs/skills/claim-verification-SKILL.md:433:- `0 completed checks is not a pass`
docs/skills/claim-verification-SKILL.md:434:- `atomic command/result before composite script`
docs/skills/claim-verification-SKILL.md:435:- `unknown cleanup routes to cleanup worker`
docs/skills/claim-verification-SKILL.md:436:- `after two 0-check incomplete packets repair the work-order shape`
```

## mental_simulation_result

If Dalton or Lorentz repeats the prior pattern under the new rule, a packet with
`0 completed checks` after an assigned micro-check cannot be accepted as a pass,
completed check, review/fix cycle, or product evidence. If it used a composite
temp/script flow without explicit authorization, the packet is invalid for the
verification claim. If it timed out or was aborted and cleanup is unknown, the
next action is cleanup proof or a cleanup worker. After two repeated 0-check
incomplete packets with the same work-order shape, the parent must repair the
governance/prompt/work-order shape before retrying product verification. The
chain therefore routes to recovery and repair instead of a stop-like packet or
a counted pass.

## verification

Changed-section readback:

```powershell
Select-String -LiteralPath AGENTS.md -Pattern 'Sub-agent micro-check incomplete packet recovery gate' -Context 0,25
Select-String -LiteralPath docs\skills\claim-verification-SKILL.md -Pattern 'Sub-agent micro-check incomplete packet recovery gate' -Context 0,25
```

Result: the added sections were read back in both files, including the `0
completed checks` invalid-packet rule, parent/tool interruption wording,
atomic command/result requirement, composite-script restriction, cleanup-proof
requirement, and two-repeat governance/prompt repair route.

Marker search:

```powershell
Select-String -LiteralPath AGENTS.md,docs\skills\claim-verification-SKILL.md,docs\evidence\design-conversion\codex-pass87gov1-subagent-microcheck-loop-repair.md -Pattern 'Sub-agent micro-check incomplete packet recovery gate','0 completed checks is not a pass','atomic command/result before composite script','unknown cleanup routes to cleanup worker','after two 0-check incomplete packets repair the work-order shape'
```

Result: all five marker strings were found in both `AGENTS.md` and
`docs/skills/claim-verification-SKILL.md`. The report also contains the title
and summary references.

Whitespace check:

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- AGENTS.md docs/skills/claim-verification-SKILL.md docs/evidence/design-conversion/codex-pass87gov1-subagent-microcheck-loop-repair.md
```

Output:

```text
<empty>
```

Result: passed for the changed files.

Conflict-marker scan:

```powershell
Select-String -LiteralPath AGENTS.md,docs\skills\claim-verification-SKILL.md,docs\evidence\design-conversion\codex-pass87gov1-subagent-microcheck-loop-repair.md -Pattern '^<<<<<<<','^=======','^>>>>>>>'
```

Output:

```text
<empty>
```

Result: no conflict-marker lines were found in the changed files.

Status check:

```powershell
git -c safe.directory=C:/Projects/llm_mvp_core status --short -- AGENTS.md docs/skills/claim-verification-SKILL.md docs/evidence/design-conversion/codex-pass87gov1-subagent-microcheck-loop-repair.md
```

Output:

```text
 M AGENTS.md
 M docs/skills/claim-verification-SKILL.md
?? docs/evidence/design-conversion/codex-pass87gov1-subagent-microcheck-loop-repair.md
warning: unable to access 'C:\Users\lubao/.config/git/ignore': Permission denied
warning: unable to access 'C:\Users\lubao/.config/git/ignore': Permission denied
```

Result: exactly the two allowed modified governance files and the new allowed
evidence report are in this scoped status output. The git ignore warnings are
recorded as environment warnings, not repaired in this pass.

## commands_run

Allowed shell command families used: `Get-Content`, `Select-String`, `rg`,
`git status`, and `git diff --check`.

Non-shell tools used: `apply_patch` for allowed-file edits and `update_plan`
for progress tracking.

One initial `git status --short` without `-c safe.directory` failed with a
dubious-ownership error and was not used as status proof. The safe-directory
scoped status command above was used as the accepted status evidence.

Forbidden commands not run: no `git add`, `git commit`, `git push`,
`git checkout`, `git switch`, `git reset`, `git merge`, `git rebase`,
`git stash`, `git clean`, `dotnet`, network command, production/test edit,
external `C:\Users\lubao\Documents\xiigen` write, staging, PR, or branch/ref
write.

## pass_verdict

PASS87GOV1_GOVERNANCE_REPAIR_PACKET_WRITTEN_AND_SCOPED_VERIFICATION_PASSED.

This verdict is only for the requested governance loophole repair in the
allowed files. It is not product readiness, not mergeability clean, and not a
claim that any product verification worker passed.

## remaining_gaps

- Product readiness, mergeability clean, and dotnet results were intentionally
  not checked in this governance pass.
- Existing inherited mojibake outside the newly added English text was not
  repaired.
