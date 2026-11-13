# Review/Fix Pass 05 - Quality scoring rubric depth and anti-token-overclaim

Requested floor: 10 review/fix passes. This is pass 5 only.

Reviewer: Codex review_fix_pass_05.

Loaded rules and gates:
- `AGENTS.md` OP-62: real pass requires an inspectable packet, not a ledger row.
- `docs/skills/plan-review-SKILL.md` Review depth floor: ledger rows are an index, not evidence.
- `docs/skills/claim-verification-SKILL.md`: review claims must be verified from live artifacts.

Reviewed artifact scope:
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl`
- Sections inspected: 4.2 token methodology handoff, 4.3 quality scoring algorithm, 4.6 examples, 6 QUALITY phases, 7 quality artifacts, 8 ten-pass ledger, 9 final acceptance matrix, 11 no-overclaim firewall.

Findings:
- FAIL before fix. Section 4.3 named four quality layers and had good/bad examples, but the rubric was too shallow to prove baseline/adaptive improvement or non-regression. It did not define score anchors, required PASS thresholds per layer, a failure taxonomy, reviewer notes, inter-rater handling, or the exact rule that token savings cannot compensate for quality regression.
- FAIL before fix. Section 8 row 4 still said plain `PASS` even though pass 4 had packet refs in `REVIEW.jsonl`; row 5 also said plain `PASS` and had no packet refs. OP-62 requires completed passes to point to distinct packets, with ledger rows treated as non-evidence.
- FAIL before fix. `REVIEW.jsonl` row 5 was ledger-only and did not point to an inspectable pass packet.

Fix applied:
- Replaced Section 4.3 with a detailed quality scoring gate covering PromptToTask, OrchestrationReasoning, SkillSelection, and Final output usefulness.
- Added score anchors 0..5, per-layer PASS thresholds, human-readable pass/fail examples, mandatory failure taxonomy, scored-row fields, reviewer notes, inter-rater/adjudication rules, and baseline/adaptive comparison rules.
- Added an explicit anti-overclaim rule: token savings cannot compensate for quality regression, new failure codes, inconclusive review, or any failed quality layer.
- Updated Section 8 rows 4 and 5 to use evidence packet refs for completed passes.
- Replaced `REVIEW.jsonl` row 5 only with evidence packet refs and `ledger_rows_are_evidence=false`.

Recheck evidence:
- Section 4.3 now requires adaptive to be >= baseline separately for PromptToTask, OrchestrationReasoning, SkillSelection, and Final output usefulness.
- Section 4.3 now forbids averaging from hiding a failed layer.
- Section 4.3 now requires `human_reviewer_note`, `failure_codes`, `quality_regression`, reviewer id, timestamp, and `inter_rater_status`.
- Section 4.3 now says fewer tokens plus any quality drop is `FAIL_QUALITY_REGRESSION` or `INCONCLUSIVE_REVIEW`, never token success.
- Section 8 rows 1-5 now point to packet refs for completed passes.
- `REVIEW.jsonl` row 5 points to this packet and keeps ledger rows as non-evidence.

Verdict:
- PASS_WITH_FIX for Pass 5 scope only.
- No overall completion claim.
