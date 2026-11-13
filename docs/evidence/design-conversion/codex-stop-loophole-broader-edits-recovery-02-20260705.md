# Codex Stop-Loophole Broader Edits Recovery 02 - 20260705

## Claim Boundary

claim_scope: GOV-RECOVERY-BROADER-EDITS-02 governance recovery only; not product pass 4/50; not product completion; not 50 passes.
evidence_scope: required context read commands, Nash audit readback, diffs inspected for the 3 needs_fix files and 19 pending files, BOM/first-byte check for the 3 repaired needs_fix files, and current bounded verification commands.
count_claimed: 3 needs_fix files repaired; 6 of 19 pending files accepted; 13 of 19 pending files remain needs_fix; 0 product passes.
independent_evidence_count: required context reads plus file diff inspections and bounded verification commands; no sub-agent packet was produced in this current-thread recovery.
subagent_id_or_NONE: NONE; current-thread bounded recovery, no nested sub-agent or codex.exe spawned.
downgrade_needed: yes. Broader governance edits are not fully accepted, so next slice is GOV-RECOVERY-BROADER-EDITS-03, not final review pass 4/50.

## Skill Load Status

- AGENTS.md: found; Get-Content command executed in current thread. Tool output was large/truncated in the UI, so this is not upgraded beyond command-read evidence.
- docs/skills/claim-verification-SKILL.md: found; Get-Content command executed in current thread. Tool output was large/truncated in the UI, so this is not upgraded beyond command-read evidence.
- docs/skills/plan-review-SKILL.md: found; Get-Content command executed in current thread. Tool output was large/truncated in the UI, so this is not upgraded beyond command-read evidence.
- docs/skills/goal-delivery-completeness-SKILL.md: found/read in current thread.
- docs/evidence/design-conversion/codex-stop-loophole-broader-edits-audit-20260705.md: found/read in current thread.

## Repaired Files

| File | Outcome | Evidence |
|---|---|---|
| docs/skills/audit-protocol-SKILL.md | repaired_and_accepted_for_this_slice | Removed introduced BOM and reverted unrelated punctuation-only normalization while preserving useful Current-Ledger, count-index, identity, pre-final continuation, and full-access cleanup governance content. First bytes after repair: `23 20 41`. |
| docs/skills/authority-requirement-binding-SKILL.md | repaired_and_accepted_for_this_slice | Removed introduced BOM while preserving useful B-1..B-22 and PLAN_NOT_READY continuation governance content. First bytes after repair: `23 20 41`. |
| docs/skills/bug-to-tests-SKILL.md | repaired_and_accepted_for_this_slice | Removed introduced BOM and reverted punctuation-only changes. Current `git diff -- docs/skills/bug-to-tests-SKILL.md` has no output. First bytes after repair: `23 20 42`. |

## Accepted Pending Files

| File | Outcome | Evidence |
|---|---|---|
| docs/skills/codex-planning-quality-GUIDE.md | accepted_broader_stop_loophole_repair | Inspected diff adds Pass172 planning-quality firewall, count-index snapshot rules, spawned-worker identity boundary, and forbidden-command recovery handling. No small current-slice repair applied. |
| docs/skills/coverage-completeness-gate-SKILL.md | accepted_broader_stop_loophole_repair | Inspected diff adds B-1..B-22 inventory and PLAN_NOT_READY continuation guard. No small current-slice repair applied. |
| docs/skills/phase-git-report-SKILL.md | accepted_broader_stop_loophole_repair | Inspected diff adds exact git authority, no-write mergeability, and product completion stale-evidence gates. No small current-slice repair applied. |
| docs/skills/qa-session-type-SKILL.md | accepted_broader_stop_loophole_repair | Inspected diff adds QA firewall, skill-load requirement, product completion evidence, no-overclaim, no-git-mutation, and historical label gates. No small current-slice repair applied. |
| docs/skills/reconnaissance-gate-SKILL.md | accepted_broader_stop_loophole_repair | Inspected diff adds reconnaissance pre-final continuation enforcement and heartbeat/no-silent-wait gates. No small current-slice repair applied. |
| docs/skills/verification-before-completion-SKILL.md | accepted_broader_stop_loophole_repair | Inspected diff adds current-readiness single-value, product local completion, pre-final continuation, prefix/location, and post-governance product-slice gates. No small current-slice repair applied. |

## Remaining Needs Fix

These files were inspected enough to avoid leaving them as uninspected, but not accepted because the visible diff still contains introduced BOM, punctuation-only normalization, mechanical arrow/dash changes, or mixed useful governance content plus mechanical drift.

| File | Classification | Reason |
|---|---|---|
| docs/skills/e2e-test-matrix-SKILL.md | needs_fix | Visible diff is mechanical BOM/punctuation/arrow normalization; no accepted stop-loophole content in inspected hunks. |
| docs/skills/generated-code-review-SKILL.md | needs_fix | Useful Pass172 generated-code firewall is present, but visible diff also has introduced BOM and unrelated arrow/dash normalization. |
| docs/skills/no-product-decisions-SKILL.md | needs_fix | Visible diff is mechanical BOM/punctuation/arrow normalization; no accepted stop-loophole content in inspected hunks. |
| docs/skills/output-readability-gate-SKILL.md | needs_fix | Visible diff has introduced BOM and unrelated punctuation normalization mixed with marker-list repair. |
| docs/skills/output-skepticism-SKILL.md | needs_fix | Useful output pre-final gate is present, but visible diff also has introduced BOM and unrelated arrow/dash normalization. |
| docs/skills/phase-planning-SKILL.md | needs_fix | Useful Pass172 and B-1..B-22 gates are present, but visible diff also has introduced BOM/title punctuation normalization. |
| docs/skills/planning-session-startup-SKILL.md | needs_fix | Useful startup governance, ledger, readiness, identity, and coverage-intake gates are present, but visible diff also has introduced BOM. |
| docs/skills/reference-inventory-and-source-map-SKILL.md | needs_fix | Useful source-addressability and pass-floor gate is present, but visible diff also has introduced BOM. |
| docs/skills/self-verification-SKILL.md | needs_fix | Visible diff is mechanical BOM/punctuation/icon replacement; no accepted stop-loophole content in inspected hunks. |
| docs/skills/specificity-calibration-SKILL.md | needs_fix | Useful specificity pre-final gate is present, but visible diff also has introduced BOM and unrelated punctuation normalization. |
| docs/skills/test-failure-triage-SKILL.md | needs_fix | Visible diff is mechanical BOM/punctuation/arrow normalization; no accepted stop-loophole content in inspected hunks. |
| docs/skills/test-integrity-SKILL.md | needs_fix | Visible diff is mechanical BOM/punctuation/arrow normalization; no accepted stop-loophole content in inspected hunks. |
| docs/skills/trainable-architecture-decision-SKILL.md | needs_fix | Useful newer-plan-partial residue gate is present, but visible diff also has introduced BOM and unrelated Section-symbol/punctuation normalization. |

## Classification Summary

- repaired_and_accepted_for_this_slice: 3
- accepted_broader_stop_loophole_repair from prior pending list: 6
- remaining_needs_fix: 13
- not_accepted_pending_recovery_uninspected: 0
- ready_for_final_review_pass_4_50: no

## Next Required Slice

next_product_or_review_slice: GOV-RECOVERY-BROADER-EDITS-03, not final review pass 4/50.

GOV-RECOVERY-BROADER-EDITS-03 targets:

```text
docs/skills/e2e-test-matrix-SKILL.md
docs/skills/generated-code-review-SKILL.md
docs/skills/no-product-decisions-SKILL.md
docs/skills/output-readability-gate-SKILL.md
docs/skills/output-skepticism-SKILL.md
docs/skills/phase-planning-SKILL.md
docs/skills/planning-session-startup-SKILL.md
docs/skills/reference-inventory-and-source-map-SKILL.md
docs/skills/self-verification-SKILL.md
docs/skills/specificity-calibration-SKILL.md
docs/skills/test-failure-triage-SKILL.md
docs/skills/test-integrity-SKILL.md
docs/skills/trainable-architecture-decision-SKILL.md
```

GOV-RECOVERY-BROADER-EDITS-03 acceptance:
- remove introduced BOM/mechanical punctuation drift;
- preserve useful stop-loophole governance content;
- run `git diff --check` on changed/evidence files;
- run conflict-marker scan on changed/evidence files;
- run added-line mojibake-marker scan on changed/evidence files using the configured marker regex from the work order;
- write successor evidence if any file remains unaccepted.

