# Eval-only runner dry-run plan

Run: `quality-02-scorer-eval-only-runner-prep-20260618-095502`

Этот dry-run plan не запускает eval. Он описывает, как будущий runner будет загружать QUALITY-01 pack/rubric/contract и какие статические проверки нужны до любой оценки.

## Dry-run steps allowed now

| Step | Allowed action | Forbidden boundary |
|---|---|---|
| `DRY-01-LOAD-QUALITY01-PACK` | Parse `eval-prompt-pack.json/jsonl`; confirm 12 scenarios. | No host/model/scorer/external AI/training/checkpoint/locator. |
| `DRY-02-LOAD-RUBRIC` | Parse `quality-rubric-ru.json`; confirm four layers and 0..4 anchors. | Do not assign scores. |
| `DRY-03-LOAD-INPUT-CONTRACT` | Parse `future-runner-input-contract.json`; verify gates `G1..G8`. | Do not treat `runner_may_start_now=false` as runnable. |
| `DRY-04-ROUTE-CLASSIFY` | Mark 7 user-adaptive scenarios as runnable later only; 5 evidence-only scenarios as non-runnable. | Evidence-only rows cannot become eval/training input. |
| `DRY-05-EMIT-SKELETON` | Emit schema examples with dry-run statuses. | Do not write PASS/FAIL score results. |
| `DRY-06-NO-OVERCLAIM-CHECK` | Keep all claim locks false. | No quality/token/learnability/readiness claims. |

## Future runner sequence, only after separate authority

1. Require exact current approval for local/private eval-only scoring scope.
2. Load QUALITY-01 pack and rubric.
3. Load approved baseline/adaptive output refs for the same `pair_id`.
4. Reject rows without source/privacy/pairing gates.
5. Score PromptToTask, OrchestrationReasoning, SkillSelection and final usefulness separately.
6. Write reviewer notes and failure labels for every row.
7. Keep `semantic_quality_claim_allowed=false` unless later gates explicitly pass.
8. Write comparison/regression guard reports only after all scored rows are valid.

## What this proves

- The future runner has a concrete dry-run loading and output plan.
- The QUALITY-01 pack can be used as an input inventory without executing eval now.

## What this does not prove

- It does not prove scores, quality improvement, semantic readiness, token savings, live host readiness, learnability, common readiness, strict QUALITY-03 readiness, training readiness, target promotion, locator update or final readiness.
