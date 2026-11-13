# Next bounded work order

`QUALITY-02-SAFE-NO-EVAL-NONPRIVATE-FIXTURE-ADMISSION-REHEARSAL`

## Purpose

Continue without stopping by creating a non-private/test-only no-eval admission rehearsal packet. This is only a schema/gate mechanics path. It does not admit the 21 `USER_ADAPTIVE_EVAL_ONLY` local-private rows and must keep `actual_eval_can_be_next=false`.

## Allowed writes

- New timestamped folder under `artifacts/extension-runtime-token-quality-learnability/quality-02-safe-no-eval-nonprivate-fixture-admission-rehearsal/`
- `docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json`

## Required

- Read this missing-approval packet and state.
- Use only non-private, synthetic, fixture, or test-only refs already present in repository artifacts.
- Mark every rehearsal row as `NOT_ACTUAL_LOCAL_PRIVATE_EVAL_INPUT`.
- Do not create `approved-eval-input.local-private.jsonl`.
- Do not run eval.

## Forbidden

Eval execution, eval runner dotnet run, external AI/model calls, training, DPO, checkpoint/locator/target/manifest mutation, source/test/skill/governance edits outside state JSON, raw private content reads/copies, admitting fixture rows as actual local-private eval, and git writes.
