# GOV-RECOVERY-BROADER-EDITS-02A-3 Evidence - 20260706

## Claim Boundary

claim_scope: bounded governance recovery for `docs/skills/bug-to-tests-SKILL.md` only; not product pass 4/50; not product completion; not 50 passes.
evidence_scope: required context reads, target first-byte/BOM check, target diff inspection, no-op preservation decision, evidence file creation, and verification commands listed below.
count_claimed: 0 product passes; 0 review passes; 1 bounded recovery evidence packet for `bug-to-tests-SKILL.md`.
independent_evidence_count: 6 bounded target/evidence records: BOM check, target diff inspection, `git diff --check`, conflict marker scan, added-line mojibake scan, and evidence readback.
subagent_id_or_NONE: NONE; current instruction assigned this session as the authorized sub-agent executor and forbade nested agents.
downgrade_needed: no for this target file based on current byte/diff evidence; yes for broader chain because 19 pending files remain.

## Skill Load Status

- AGENTS.md: current prompt supplied the governing AGENTS.md instructions; filesystem read command was run and exited 0 before this evidence packet.
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-audit-20260705.md`: found/read; Nash classification for `bug-to-tests-SKILL.md` was `needs_fix` due to file-start BOM introduction and unrelated punctuation-only rewrites rather than a stop-loophole repair.
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-1-20260706.md`: found/read; previous slice accepted `audit-protocol-SKILL.md` by no-op preservation and left `bug-to-tests-SKILL.md` for a later slice.
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-2-20260706.md`: found/read; previous slice accepted `authority-requirement-binding-SKILL.md` by no-op preservation and named this slice as next.
- `docs/skills/bug-to-tests-SKILL.md`: found/read and inspected directly for bytes and diff scope.

## Target Scope

Allowed target files for this slice:

- `docs/skills/bug-to-tests-SKILL.md`
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-3-20260706.md`

Forbidden in this slice and not touched:

- `docs/skills/audit-protocol-SKILL.md`
- `docs/skills/authority-requirement-binding-SKILL.md`
- the 19 pending broader files
- product/source/test code
- `C:\Users\lubao\Documents\xiigen`
- git add/commit/push/merge/checkout/switch/PR

## Before Repair Evidence

First-byte/BOM check run against `docs/skills/bug-to-tests-SKILL.md`:

```text
length=7522
first16=23 20 42 75 67 20 74 6F 20 54 65 73 74 73 0A 0A
has_utf8_bom=False
```

Target diff size:

```text
git diff --numstat -- docs/skills/bug-to-tests-SKILL.md
result: exit 0; no output.

git diff --shortstat -- docs/skills/bug-to-tests-SKILL.md
result: exit 0; no output.
```

Target diff inspection:

```text
git diff --word-diff=porcelain -- docs/skills/bug-to-tests-SKILL.md
result: exit 0; no output.

git status --short -- docs/skills/bug-to-tests-SKILL.md docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-3-20260706.md
result before evidence file creation: exit 0; no output.
```

## Repair Decision

`docs/skills/bug-to-tests-SKILL.md` is repaired/accepted for this slice by no-op preservation:

- The current file start has no UTF-8 BOM.
- The active diff has no output, so no current punctuation-only or unrelated scope rewrite remains to roll back.
- No text edit was made to `docs/skills/bug-to-tests-SKILL.md` in this worker because the current worktree evidence did not show a remaining file-start BOM, punctuation-only rewrite, or other small scope defect.

After-repair byte check for `docs/skills/bug-to-tests-SKILL.md`:

```text
after_length=7522
after_first16=23 20 42 75 67 20 74 6F 20 54 65 73 74 73 0A 0A
after_has_utf8_bom=False
```

## Remaining Files

Remaining `needs_fix` files from Nash's three-file 02A group after this slice:

- none currently identified by this 02A-3 worker after no-op acceptance of `bug-to-tests-SKILL.md`; this does not inspect the 19 pending broader files.

Remaining pending broader files: 19 `not_accepted_pending_recovery` files from `codex-stop-loophole-broader-edits-audit-20260705.md`.

## Verification Commands And Results

Post-evidence verification:

```text
first-byte/BOM recheck for docs/skills/bug-to-tests-SKILL.md:
command: PowerShell [System.IO.File]::ReadAllBytes first16/BOM check
result: exit 0; after_length=7522; after_first16=23 20 42 75 67 20 74 6F 20 54 65 73 74 73 0A 0A; after_has_utf8_bom=False.

git diff --check over the two target files:
command: git diff --check -- docs/skills/bug-to-tests-SKILL.md docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-3-20260706.md
result: exit 0; no output. Note: the evidence file is new/untracked, so supplemental direct scans below also covered the evidence file content.

conflict marker scan over the two target files:
command: rg -n "^(<<<<<<<|=======|>>>>>>>)" docs/skills/bug-to-tests-SKILL.md docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-3-20260706.md
result: exit 1; no conflict markers printed.

added-line mojibake scan over the two target files:
command: PowerShell scan of tracked added lines for docs/skills/bug-to-tests-SKILL.md plus all lines of the new evidence file as added candidates for the configured mojibake markers by codepoint.
result: exit 0; added_line_mojibake_matches=0.

evidence readback includes bug-to-tests-SKILL.md repaired/accepted:
command: rg -n "bug-to-tests-SKILL\.md.*(accepted|repaired)|files_repaired:.*bug-to-tests-SKILL\.md" docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-3-20260706.md
result: exit 0; readback matched the repaired/accepted decision and files_repaired row.
```

## Return Packet Draft

claim_scope: bounded governance recovery for `docs/skills/bug-to-tests-SKILL.md` only.
evidence_scope: byte/BOM check, target diff inspection, no-op preservation repair decision, evidence file creation, and verification command results.
count_claimed: 0 product passes; 0 review passes; 1 bounded recovery evidence packet.
independent_evidence_count: 6 bounded target/evidence records: BOM check, target diff inspection, `git diff --check`, conflict marker scan, added-line mojibake scan, and evidence readback.
subagent_id_or_NONE: NONE
skill_load_status: AGENTS.md prompt instructions loaded with filesystem read command run; broader-edits audit evidence read; recovery 02A-1 and 02A-2 evidence read; bug-to-tests-SKILL.md read/inspected.
files_repaired: `docs/skills/bug-to-tests-SKILL.md` accepted/repaired by no-op preservation; no BOM present and no current active diff found.
files_remaining_needs_fix_or_pending: 19 broader pending files.
files_changed: `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-3-20260706.md`
verification_commands_and_results: first-byte/BOM recheck exit 0 with after_has_utf8_bom=False; git diff --check exit 0; conflict marker scan exit 1 with no markers; added-line mojibake scan exit 0 with 0 matches; evidence readback exit 0 with repaired/accepted row.
next_product_or_review_slice: `GOV-RECOVERY-BROADER-EDITS-02B` for the 19 pending files
downgrade_needed: no for this target file after current inspection; yes for broader chain because 19 pending files remain.
