# Next Bounded Work Order

QUALITY-02-COLLECT-APPROVED-LOCAL-PRIVATE-REFS-AND-ADJUDICATION-NO-EVAL-EXECUTION

Requested artifact type: approved local-private eval input refs and reviewer/adjudication packet; no actual eval execution

Allowed write scope:
- timestamped folder under artifacts/extension-runtime-token-quality-learnability/quality-02-approved-local-private-eval-input-refs-and-adjudication/
- docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json

Required actions:
- Bind approval_ref and approved user_local_root_ref per row, or mark row not admitted.
- Bind explicit raw_private_input_ref and redacted_shared_evidence_ref per row without copying raw private content into shared/common artifacts.
- Decide whether local token-03/token-04 handler traces can be used as baseline_output_ref/adaptive_output_ref for approved eval; if not, generate/collect replacement refs through separately allowed host/core runs only.
- Create reviewer/adjudication refs with reviewer/adjudicator id, rubric/scorer refs, per-row notes, inter-rater status or explicit single-reviewer limitation.
- Only if every required field is bound, create approved-eval-input.local-private.jsonl; otherwise keep actual eval closed and route to the next missing-ref packet.

Forbidden actions:
- actual eval execution
- dotnet run of Quality02EvalOnlyRunner
- external AI/model calls unless fresh current authorization explicitly allows them for this exact adjudication task
- training or DPO creation
- checkpoint/locator/target/manifest mutation
- EVIDENCE_ONLY, COMMON_BLOCKED, or synthetic fixture rows in actual eval
- git add/commit/push
