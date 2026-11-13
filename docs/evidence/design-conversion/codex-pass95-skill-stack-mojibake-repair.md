# Codex Pass95 - Skill Stack Mojibake Repair

## Claim Scope

claim_scope: Pass95 governance/skill readability repair only.
evidence_scope: Direct reads, BOM checks, marker scans, bounded ASCII substitutions, and changed-line readbacks in `C:\Projects\llm_mvp_core`.
count_claimed: 1 bounded Pass95 governance repair packet; 0 product features; 0 product tests/builds; 0 git stage/commit/push actions.
independent_evidence_count: 25 required file-load records, 25 before marker-count records, 25 after marker-count records, changed-line readback records, BOM verification records, and follow-up verification commands.
subagent_id_or_NONE: NONE.
downgrade_needed: No product readiness or product completion claim. This repair proves only required stack readability for the configured mojibake marker set.

## Skill Load Status

All required stack files were found and read before edits. The parent observation was verified as a loader/readability defect: default Windows PowerShell `Get-Content` on UTF-8 files without BOM displayed mojibake, while `Get-Content -Encoding UTF8` showed the underlying text was valid UTF-8. `docs/skills/architect-behavior-classifier-SKILL.md` already had a UTF-8 BOM and read cleanly; the other required files lacked a BOM before Pass95.

| File | status |
|---|---|
| AGENTS.md | found/read |
| docs/SESSION-START-PROMPT.md | found/read |
| docs/skills/planning-session-startup-SKILL.md | found/read |
| docs/skills/architect-behavior-classifier-SKILL.md | found/read; BOM already present; not edited by Pass95 |
| docs/skills/authority-requirement-binding-SKILL.md | found/read |
| docs/skills/reference-inventory-and-source-map-SKILL.md | found/read |
| docs/skills/phase-planning-SKILL.md | found/read |
| docs/skills/plan-review-SKILL.md | found/read |
| docs/skills/goal-delivery-completeness-SKILL.md | found/read |
| docs/skills/claim-verification-SKILL.md | found/read |
| docs/skills/audit-protocol-SKILL.md | found/read |
| docs/skills/specificity-calibration-SKILL.md | found/read |
| docs/skills/output-skepticism-SKILL.md | found/read |
| docs/skills/reconnaissance-gate-SKILL.md | found/read |
| docs/skills/trainable-architecture-decision-SKILL.md | found/read |
| docs/skills/no-product-decisions-SKILL.md | found/read |
| docs/skills/output-readability-gate-SKILL.md | found/read |
| docs/skills/implementation-integrity-SKILL.md | found/read |
| docs/skills/generated-code-review-SKILL.md | found/read |
| docs/skills/self-verification-SKILL.md | found/read |
| docs/skills/verification-before-completion-SKILL.md | found/read |
| docs/skills/test-integrity-SKILL.md | found/read |
| docs/skills/e2e-test-matrix-SKILL.md | found/read |
| docs/skills/bug-to-tests-SKILL.md | found/read |
| docs/skills/test-failure-triage-SKILL.md | found/read |

## Files Changed By Pass95

- Added UTF-8 BOM to the 24 required stack files that lacked one so default `Get-Content` reads the existing Russian and Unicode text correctly.
- Kept `docs/skills/architect-behavior-classifier-SKILL.md` unchanged by Pass95 because it already had a UTF-8 BOM and marker count 0.
- Replaced obvious active-gate punctuation/symbol cases with ASCII in:
  `AGENTS.md`, `docs/SESSION-START-PROMPT.md`, `docs/skills/phase-planning-SKILL.md`,
  `docs/skills/plan-review-SKILL.md`, `docs/skills/goal-delivery-completeness-SKILL.md`,
  `docs/skills/claim-verification-SKILL.md`, `docs/skills/audit-protocol-SKILL.md`,
  `docs/skills/specificity-calibration-SKILL.md`, `docs/skills/output-skepticism-SKILL.md`,
  `docs/skills/reconnaissance-gate-SKILL.md`, `docs/skills/trainable-architecture-decision-SKILL.md`,
  `docs/skills/no-product-decisions-SKILL.md`, `docs/skills/output-readability-gate-SKILL.md`,
  `docs/skills/implementation-integrity-SKILL.md`, `docs/skills/generated-code-review-SKILL.md`,
  `docs/skills/self-verification-SKILL.md`, `docs/skills/verification-before-completion-SKILL.md`,
  `docs/skills/test-integrity-SKILL.md`, `docs/skills/e2e-test-matrix-SKILL.md`,
  `docs/skills/bug-to-tests-SKILL.md`, and `docs/skills/test-failure-triage-SKILL.md`.
- Wrote this evidence file: `docs/evidence/design-conversion/codex-pass95-skill-stack-mojibake-repair.md`.

Note: `git diff` showed unrelated pre-existing governance additions in some files, including `docs/skills/architect-behavior-classifier-SKILL.md`. Pass95 did not revert or normalize unrelated dirty worktree content.

## Mojibake Markers Before/After

Configured visible marker codepoints: U+00D0, U+00D1, U+00C2, U+00E2, U+FFFD as read by default `Get-Content`.

| File | before | after |
|---|---:|---:|
| AGENTS.md | 3475 | 0 |
| docs/SESSION-START-PROMPT.md | 45 | 0 |
| docs/skills/planning-session-startup-SKILL.md | 1105 | 0 |
| docs/skills/architect-behavior-classifier-SKILL.md | 0 | 0 |
| docs/skills/authority-requirement-binding-SKILL.md | 6 | 0 |
| docs/skills/reference-inventory-and-source-map-SKILL.md | 301 | 0 |
| docs/skills/phase-planning-SKILL.md | 1606 | 0 |
| docs/skills/plan-review-SKILL.md | 1471 | 0 |
| docs/skills/goal-delivery-completeness-SKILL.md | 67 | 0 |
| docs/skills/claim-verification-SKILL.md | 473 | 0 |
| docs/skills/audit-protocol-SKILL.md | 82 | 0 |
| docs/skills/specificity-calibration-SKILL.md | 39 | 0 |
| docs/skills/output-skepticism-SKILL.md | 63 | 0 |
| docs/skills/reconnaissance-gate-SKILL.md | 47 | 0 |
| docs/skills/trainable-architecture-decision-SKILL.md | 90 | 0 |
| docs/skills/no-product-decisions-SKILL.md | 11 | 0 |
| docs/skills/output-readability-gate-SKILL.md | 447 | 0 |
| docs/skills/implementation-integrity-SKILL.md | 22 | 0 |
| docs/skills/generated-code-review-SKILL.md | 109 | 0 |
| docs/skills/self-verification-SKILL.md | 35 | 0 |
| docs/skills/verification-before-completion-SKILL.md | 378 | 0 |
| docs/skills/test-integrity-SKILL.md | 114 | 0 |
| docs/skills/e2e-test-matrix-SKILL.md | 54 | 0 |
| docs/skills/bug-to-tests-SKILL.md | 31 | 0 |
| docs/skills/test-failure-triage-SKILL.md | 33 | 0 |

## Changed-Line Readback Summary

Default `Get-Content` readback after the repair showed clean active headers and gates, including:

- `AGENTS.md`: title line, Human Override arrow, and no-mojibake evidence gate.
- `docs/SESSION-START-PROMPT.md`: title, Human Override arrow, Human Language header, Round Contract header, and drift/feedback/done rows.
- Planning/review/QA/code-review skills: patched active headings, arrows, comparison operators, check/fail symbols, and checklist rows.
- Representative BOM-only readback: the planning startup dual-path gate now renders the Russian phrase for dual-path host verification correctly under default `Get-Content`.

Changed-line readback scripts reported `bad_markers=0` for every matched changed line. Initial readback had four missing needles because the search snippets were too broad or split across lines; reruns read those exact lines and ended with `missing=0` and `total_bad_marker_lines=0`.

## Remaining Mojibake Queue

None for the configured marker set in the required stack files. The after-scan reported 0 visible marker codepoints for all 25 required stack files under default `Get-Content`.

Remaining valid non-ASCII text, including Russian trigger words and Russian explanatory text, is not queued as mojibake because the BOM repair makes it readable in the default loader path and the marker scan is clean.

## Verification

- Loaded/read all 25 required stack files before edits.
- Verified root cause with default `Get-Content` vs `Get-Content -Encoding UTF8` on representative files.
- Added UTF-8 BOM to 24 required stack files that lacked it; post-check showed all 25 required stack files now have `utf8_bom=True`.
- Ran after marker scan for all 25 required stack files: all returned `visible_marker_codepoints=0`.
- Ran changed-line readback for patched active gate lines; final reruns ended with `missing=0` and `total_bad_marker_lines=0`.
- Product tests/builds were not run.
- Git stage/commit/push/switch/reset/stash/clean were not run.

- Final line-anchored conflict-marker scan across the required stack files plus this evidence file returned 0 hits.
- `git -c safe.directory=C:/Projects/llm_mvp_core diff --check -- <tracked changed stack files>` returned exit 0 with no output.
- Evidence-file readback succeeded with default `Get-Content`.

## Final Verdict

PASS95_GOVERNANCE_REPAIR_DONE_FOR_CONFIGURED_MOJIBAKE_MARKERS.

This does not prove product completion, plan readiness, or implementation authorization.
