# QUALITY-02 scorer/eval-only runner prep review

Sub-agent id: `review_internal_quality_02_scorer_eval_only_runner_prep_20260618_095502`

Verdict: `PASS_WITH_REPAIR_ACCEPTED`

Review scope: static artifact/state review only. No eval runner, host runtime, model call, training, checkpoint mutation, target promotion, locator update, source-code edit, commit, push, or child agent was run.

## Accepted repair

`score-output-schema.jsonl` had five `EVIDENCE_ONLY` examples with empty `per_layer_scores`, while `score-output-schema.json` defines score examples with four layer slots. I repaired only those five non-runnable rows by adding `prompt_to_task`, `orchestration_reasoning`, `skill_selection`, and `final_usefulness` slots with `score_0_to_4=null` and `pass_fail=NOT_SCORED`.

The repair does not make evidence-only rows runnable. They remain `score_status=NON_RUNNABLE_EVIDENCE_ONLY`, `runnable_now=false`, `runnable_later_after_approval=false`, and keep the `NON_RUNNABLE_EVIDENCE_ONLY` failure code.

## Counts

| Route | Scenarios | Pair rows | Representation | Result |
|---|---:|---:|---|---|
| `USER_ADAPTIVE_EVAL_ONLY` | 7 | 21 | JSONL examples | PASS |
| `EVIDENCE_ONLY` | 5 | 33 | JSONL examples, non-runnable | PASS |
| `COMMON_BLOCKED` | 12 | 54 | metadata/counts only, blocked | PASS |

## Review checks

| Check | Result |
|---|---|
| Required deliverables exist | PASS |
| JSON artifacts parse | PASS |
| `score-output-schema.jsonl` parses | PASS |
| `USER_ADAPTIVE_EVAL_ONLY` coverage is 7 scenarios / 21 pair rows | PASS |
| `EVIDENCE_ONLY` coverage is 5 scenarios / 33 pair rows | PASS |
| `COMMON_BLOCKED` coverage is 12 scenarios / 54 pair rows in counts metadata | PASS |
| Scorer contract covers PromptToTask, OrchestrationReasoning, SkillSelection, final usefulness | PASS |
| Scorer contract requires reviewer notes, fail labels, evidence-only exclusion, and no-shortcut/runtime substitution labels | PASS |
| Dry-run plan is no-execution and does not claim runner execution | PASS |
| Score schema examples do not pretend evidence-only rows are runnable | PASS |
| Claim firewall keeps token savings, quality improvement, semantic readiness, learnability readiness, live-host readiness, common readiness, strict QUALITY-03 ready, training-ready, target promotion, locator update, and final readiness closed | PASS |
| State current phase and next action point to QUALITY-02 parent review | PASS |
| State/status gives done/current/remaining/next information | PASS |
| No source code/model/checkpoint/locator/training changes accepted in this review scope | PASS |
| Old `cursor-user-data` not touched by this review; do not stage it | PASS |
| `git diff --check` | PASS |

## Changed files by this review

- `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/score-output-schema.jsonl`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/review-quality-02-scorer-eval-only-runner-prep-20260618-095502.md`
- `artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/review-quality-02-scorer-eval-only-runner-prep-20260618-095502.json`

## Safe commit instructions for parent

Current branch observed by review: `codex/chat-gpt-5.5`

Do not stage:

- `artifacts/extension-runtime-token-quality-learnability/qsg21-live-runtime-target-probes/cursor-extension-host/cursor-user-data/`

Stage exactly the active state file and the accepted QUALITY-02 target folder files:

```powershell
git add -- "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/scorer-contract.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/scorer-contract.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/eval-only-runner-dry-run-plan.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/eval-only-runner-dry-run-plan.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/score-output-schema.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/score-output-schema.jsonl" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/score-output-schema.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/quality02-preflight-validation.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/quality02-preflight-validation.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/no-overclaim-firewall-quality02.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/no-overclaim-firewall-quality02.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/next-phase-selection.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/next-phase-selection.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/summary-ru.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/quality-02-scorer-eval-only-runner-prep-checks-report.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/quality-02-scorer-eval-only-runner-prep-checks-report.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/quality-02-scorer-eval-only-runner-prep-state-entry.json" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/review-quality-02-scorer-eval-only-runner-prep-20260618-095502.md" `
  "artifacts/extension-runtime-token-quality-learnability/quality-02-scorer-eval-only-runner-prep/quality-02-scorer-eval-only-runner-prep-20260618-095502/review-quality-02-scorer-eval-only-runner-prep-20260618-095502.json"

git diff --cached --name-only
git commit -m "phase QUALITY-02: prepare scorer eval-only runner prep — state and quality-02 artifacts"
git push origin codex/chat-gpt-5.5
```

## Next bounded action

After parent accepts and commits/pushes this packet, the exact next safe bounded action is:

`QUALITY-02-ARTIFACT-RUNNER-DRY-RUN-VALIDATION-NO-EVAL`

Code implementation alternative remains blocked unless the parent has a fresh explicit current instruction authorizing source-code edits:

`QUALITY-02-EVAL-ONLY-RUNNER-IMPLEMENTATION-NO-EVAL-EXECUTION`
