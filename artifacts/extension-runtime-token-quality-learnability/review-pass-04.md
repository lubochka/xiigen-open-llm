# Review/Fix Pass 04 - Token before/after methodology

Requested floor: 10 review/fix passes. This is pass 4 only.

Reviewer: Codex review_fix_pass_04.

Loaded rules and gates:
- `AGENTS.md` OP-62: real pass requires an inspectable packet, not a ledger row.
- `docs/skills/plan-review-SKILL.md` Review depth floor: ledger rows are an index, not evidence.
- `docs/skills/claim-verification-SKILL.md`: completion and review claims must be verified from live artifacts.

Reviewed artifact scope:
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl`
- Sections inspected: 3 token notes, 4.2 token before/after algorithm, 4.3 quality scoring algorithm, 6 TOKEN/QUALITY phases, 7 required artifacts, 8 ten-pass ledger, 9 final acceptance matrix, 11 no-overclaim firewall.

Finding:
- FAIL before fix. Section 4.2 required same scenario pack, host, method, debug setting, and tokenizer, but it did not force paired-run identity deeply enough. Missing or weak fields: same prompt text/hash as a hard pair key, same host runtime/build and adapter/core version, same invocation method/settings, exact tokenizer/version/accounting spans, baseline/adaptive isolation, repeated runs or variance handling, full trace schema, privacy/redaction handling, and explicit failure outcomes for invalid pairs and quality regression.
- The REVIEW.jsonl row 4 was ledger-only evidence and did not point to an inspectable pass packet.

Fix applied:
- Added `4.2.1. Token before/after hard methodology gate` to the plan.
- Strengthened the final acceptance matrix token row.
- Strengthened the no-overclaim firewall so token delta alone cannot prove runtime readiness, model readiness, target promotion, rollback safety, or learnability.
- Replaced REVIEW.jsonl row 4 with evidence packet refs and `ledger_rows_are_evidence=false`.

Recheck evidence:
- The plan now requires same `scenario_id`, canonical prompt text, `prompt_text_sha256`, method, host/runtime/build, debug setting, tokenizer name/version/accounting scope, baseline/adaptive isolation, repeated-run variance handling, trace schema fields, privacy/redaction status, and failure classifications.
- The plan now says tokens cannot prove quality; token savings count only after quality non-regression or improvement for the same scenario pair.
- The plan now says token delta cannot close Claude/Cursor/Codex runtime readiness, semantic quality, model readiness, target promotion, rollback, or learnability.
- REVIEW.jsonl row 4 points to this packet and keeps ledger rows as non-evidence.

Verdict:
- PASS_WITH_FIX for Pass 4 scope only.
- No overall completion claim.
