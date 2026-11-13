# Review/Fix Pass 03 - Runtime proof depth

Requested pass: 3 of 10.

Requested artifact type: review_fix_pass.

Scope: `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md`, `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl`, and this pass packet.

Loaded rules and gates:
- `AGENTS.md` OP-62: real review/fix passes require inspectable evidence packets; ledger rows are only an index.
- `docs/skills/plan-review-SKILL.md` Review depth floor: `real_pass_count` requires distinct packets with before/after refs, reviewer id, loaded gates, findings, fix refs, and recheck evidence.
- `docs/skills/claim-verification-SKILL.md`: plugin/runtime readiness cannot be claimed from build, static contract, helper, package smoke, dashboard/API, or test-only evidence.

Review question: Are runtime proof requirements deep enough to prevent fake host evidence: old artifact reuse, local helper masquerade, package smoke, test-only adapter, dashboard/API substitution, missing chat/UI visible answer, missing debug input/output, missing correlation id/freshness, and missing negative/error path?

Artifact before refs:
- Plan section 4.1 required fresh artifacts, debug trace, request/response JSON, screenshot/chat transcript where possible, and correlation id.
- Plan sections 6 and 9 required per-host runtime proof, but RUNTIME-02/03/04 and final acceptance did not explicitly require raw/redacted input/output, freshness window checks, host-visible answer capture for each host, and a controlled negative/error path.
- Plan section 11 blocked dashboard/helper/smoke/test-only overclaims, but did not explicitly block dashboard/API explorer or direct handler substitution as runtime proof.
- REVIEW.jsonl row 3 was ledger-only `PASS` without `review-pass-03.md/json` packet refs.

Findings:
- F1: Runtime proof was too shallow for fake-evidence prevention. It could still be satisfied by a fresh-looking helper/API packet if the packet did not prove host-visible input/output and error behavior.
- F2: Host criteria were not distinct enough. Claude hook/plugin, Cursor Extension Host/UI, and Codex plugin/app each need their own admissible proof shape and explicit fail examples.
- F3: Row 3 violated OP-62/review-depth floor by claiming PASS without an inspectable evidence packet.

Fix applied:
- Added a Runtime proof depth gate to section 4.1 requiring fresh proof packets with run timestamp, scenario id, correlation id, host runtime version/build, command/entrypoint identity, same-run artifact creation times, raw/redacted user input, raw/redacted host-visible output, debug input/output trace, parent freshness check, and screenshot/chat transcript or limitation-backed transcript.
- Added distinct proof criteria for Claude, Cursor, and Codex, including per-host explicit FAIL examples.
- Made negative/error-path proof mandatory for all hosts.
- Strengthened RUNTIME-02, RUNTIME-03, RUNTIME-04 and final acceptance rows.
- Added no-overclaim firewall bullets for dashboard/API explorer/direct handler substitution and positive-only traces.
- Updated review ledger row 3 to reference this packet with `ledger_rows_are_evidence=false`.

Fix refs:
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` section 4.1
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` section 6
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` section 8
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` section 9
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md` section 11
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1-REVIEW.jsonl` row 3

Recheck evidence:
- Section 4.1 now rejects old artifact reuse, local helper masquerade, package smoke, test-only adapter, dashboard/API substitution, direct handler substitution, and positive-only proof.
- Section 4.1 now requires visible host answer, raw/redacted input/output, debug input/output, correlation id, same-run freshness, and negative/error path.
- Host-specific criteria now distinguish Claude hook/plugin, Cursor Extension Host/UI, and Codex plugin/app.
- Section 8 row 3 and REVIEW.jsonl row 3 now point to `review-pass-03.md/json`; ledger rows remain evidence indexes only.

Verdict: PASS_WITH_FIX for pass 3 only.

Overall task complete claimed: false.
