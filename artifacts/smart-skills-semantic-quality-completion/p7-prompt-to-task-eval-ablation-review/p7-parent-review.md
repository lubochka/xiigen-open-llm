# P7 parent review: PromptToTask eval and ablation

## Вердикт

`ACCEPTED_FAIL_EVIDENCE_REPAIR_REQUIRED`

P7 не проходит quality gate: P6 baseline checkpoint loaded through project code, but reserved eval/holdout positive behavior is `0/4`. This is accepted as honest evidence, not as semantic-quality progress. The next step must repair generalization/training coverage before any P7 PASS claim.

## Review cycles

1. **Goal delivery review: PASS as FAIL evidence.** P7 used reserved eval/holdout rows and did not train on them. Result: `positive_reserved_behavior=0/4`, so P7 correctly fails.
2. **Ablation review: PASS as negative-control evidence.** 24 controls ran and shortcut leaks were `0`; however this does not rescue P7 because positive reserved behavior is still missing.
3. **Verification review: PASS.** Focused test `PromptToTaskP7EvalAblationTests` passed 1/1; JSON/JSONL parse and mojibake scan passed.
4. **Safety/no-claim review: PASS.** No training, no checkpoint rewrite, no final model overwrite, no external AI, no raw private text, no semantic-quality claim, no P7 pass claim.

## Accepted scope

- Accepted: P7 FAIL report and ablation evidence.
- Accepted: repair actions as the next actionable work.
- Not accepted: PromptToTask source-disjoint eval PASS, ablation PASS, semantic quality, checkpoint rewrite.

## Next action

Commit and push P7 fail evidence. Then issue a repair work order that changes the dataset/training candidate or baseline unit behavior enough to produce reserved-row generalization, while keeping eval/holdout separation intact.
