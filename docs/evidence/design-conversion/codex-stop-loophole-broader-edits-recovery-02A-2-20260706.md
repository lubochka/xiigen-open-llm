# GOV-RECOVERY-BROADER-EDITS-02A-2 Evidence - 20260706

## Claim Boundary

claim_scope: bounded governance recovery for `docs/skills/authority-requirement-binding-SKILL.md` only; not product pass 4/50; not product completion; not 50 passes.
evidence_scope: required context reads, target first-byte/BOM checks, target diff inspection, no-op preservation decision, evidence file creation, and verification commands listed below.
count_claimed: 0 product passes; 0 review passes; 1 bounded recovery evidence packet for `authority-requirement-binding-SKILL.md`.
independent_evidence_count: 6 bounded target/evidence records: BOM check, target diff inspection, `git diff --check`, conflict marker scan, added-line mojibake scan, and evidence readback.
subagent_id_or_NONE: NONE; current instruction assigned this session as the authorized sub-agent executor and forbade nested agents.
downgrade_needed: no for this target file after verification; yes for broader chain because later files remain.

## Skill Load Status

- AGENTS.md: current prompt included the governing AGENTS.md instructions; filesystem readback was attempted and returned relevant governing content before the raw full-file echo timed out/truncated.
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-audit-20260705.md`: found/read; Nash classification for `authority-requirement-binding-SKILL.md` was `needs_fix` due to file-start BOM introduction.
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-1-20260706.md`: found/read; previous slice accepted `audit-protocol-SKILL.md` by no-op preservation and named this slice as next.
- `docs/skills/authority-requirement-binding-SKILL.md`: found/read and inspected directly for bytes and diff scope.

## Target Scope

Allowed target files for this slice:

- `docs/skills/authority-requirement-binding-SKILL.md`
- `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-2-20260706.md`

Forbidden in this slice and not touched:

- `docs/skills/audit-protocol-SKILL.md`
- `docs/skills/bug-to-tests-SKILL.md`
- product/source/test code
- `C:\Users\lubao\Documents\xiigen`
- git add/commit/push/merge/checkout/switch/PR

## Before Repair Evidence

First-byte/BOM check run against `docs/skills/authority-requirement-binding-SKILL.md`:

```text
length=17700
first16=23 20 41 75 74 68 6F 72 69 74 79 20 52 65 71 75
has_utf8_bom=False
```

Target diff size:

```text
git diff --numstat -- docs/skills/authority-requirement-binding-SKILL.md
result: 4 insertions, 0 deletions

git diff --shortstat -- docs/skills/authority-requirement-binding-SKILL.md
result: 1 file changed, 4 insertions(+)
```

Target diff inspection:

```text
git diff -- docs/skills/authority-requirement-binding-SKILL.md
result: active diff adds only the B-1..B-22 blocker-inventory enforcement paragraph and the PLAN_NOT_READY continuation paragraph.
```

## Repair Decision

`docs/skills/authority-requirement-binding-SKILL.md` is repaired/accepted for this slice by no-op preservation:

- The current file start has no UTF-8 BOM.
- The active diff has no deletions and no visible punctuation-only or unrelated scope rewrite to roll back.
- The useful B-1..B-22 and `PLAN_NOT_READY` continuation enforcement content was preserved.
- No text edit was made to `docs/skills/authority-requirement-binding-SKILL.md` in this worker because the current worktree evidence did not show a remaining file-start BOM or other small scope defect.

After-repair byte check for `docs/skills/authority-requirement-binding-SKILL.md`:

```text
after_length=17700
after_first16=23 20 41 75 74 68 6F 72 69 74 79 20 52 65 71 75
after_has_utf8_bom=False
```

## Remaining Files

Remaining `needs_fix` file from Nash's broader audit:

- `docs/skills/bug-to-tests-SKILL.md`

Remaining pending broader files: 19 `not_accepted_pending_recovery` files from `codex-stop-loophole-broader-edits-audit-20260705.md`.

## Verification Commands And Results

Post-evidence verification:

```text
first-byte/BOM recheck for docs/skills/authority-requirement-binding-SKILL.md:
command: PowerShell [System.IO.File]::ReadAllBytes first16/BOM check
result: exit 0; first16=23 20 41 75 74 68 6F 72 69 74 79 20 52 65 71 75; has_utf8_bom=False.

git diff --check over the two target files:
command: git diff --check -- docs/skills/authority-requirement-binding-SKILL.md docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-2-20260706.md
result: exit 0; no output. Note: the evidence file is new/untracked, so supplemental direct scans below also covered the evidence file content.

conflict marker scan over the two target files:
command: rg -n "^(<<<<<<<|=======|>>>>>>>)" docs/skills/authority-requirement-binding-SKILL.md docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-2-20260706.md
result: exit 1; no conflict markers printed.

added-line mojibake scan over the two target files:
command: PowerShell scan of tracked added lines for docs/skills/authority-requirement-binding-SKILL.md plus all lines of the new evidence file as added candidates for `Ð|Ñ|Â|â€™|â€œ|â€�|�`
result: exit 0; added_line_mojibake_matches=0.

supplemental direct whitespace scan over the two target files:
command: PowerShell direct trailing-whitespace scan
result: exit 0; trailing_whitespace_matches=0.

evidence readback includes authority-requirement-binding-SKILL.md repaired/accepted:
command: rg -n "authority-requirement-binding-SKILL\.md.*(accepted|repaired)|files_repaired:.*authority-requirement-binding-SKILL\.md" docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-2-20260706.md
result: exit 0; readback matched the repaired/accepted decision and files_repaired row.
```

## Return Packet Draft

claim_scope: bounded governance recovery for `docs/skills/authority-requirement-binding-SKILL.md` only.
evidence_scope: byte/BOM check, target diff inspection, no-op preservation repair decision, evidence file creation, and verification command results.
count_claimed: 0 product passes; 0 review passes; 1 bounded recovery evidence packet.
independent_evidence_count: 6 bounded target/evidence records: BOM check, target diff inspection, `git diff --check`, conflict marker scan, added-line mojibake scan, and evidence readback.
subagent_id_or_NONE: NONE
skill_load_status: AGENTS.md prompt instructions loaded with filesystem readback attempted; broader-edits audit evidence read; recovery 02A-1 evidence read; authority-requirement-binding-SKILL.md read/inspected.
files_repaired: `docs/skills/authority-requirement-binding-SKILL.md` accepted/repaired by no-op preservation; no BOM present and no current unrelated scope drift found.
files_remaining_needs_fix_or_pending: `docs/skills/bug-to-tests-SKILL.md`; 19 broader pending files.
files_changed: `docs/evidence/design-conversion/codex-stop-loophole-broader-edits-recovery-02A-2-20260706.md`
verification_commands_and_results: first-byte/BOM recheck exit 0 with has_utf8_bom=False; git diff --check exit 0; conflict marker scan exit 1 with no markers; added-line mojibake scan exit 0 with 0 matches; direct trailing-whitespace scan exit 0 with 0 matches; evidence readback exit 0 with repaired/accepted row.
next_product_or_review_slice: `GOV-RECOVERY-BROADER-EDITS-02A-3` for `bug-to-tests-SKILL.md`
downgrade_needed: no for this target file after final verification; yes for broader chain because later files remain.
