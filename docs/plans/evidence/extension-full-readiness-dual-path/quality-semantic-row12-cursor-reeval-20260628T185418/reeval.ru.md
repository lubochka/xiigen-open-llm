# QUALITY-SEMANTIC-ROW12-CURSOR-REEVAL-AFTER-REFERENCE-FIX-01

Итог: после reference propagation fix узкая Row12 Cursor semantic structure оценка улучшилась с `PARTIAL` до `PASS_NARROW_ROW12_SEMANTIC_STRUCTURE`.

Это не full semantic quality claim. Доказана только структура одного Row12 Cursor Path A output: task contract, universal plan, skill route, reference preservation и корректная evidence boundary.

## Honesty block

claim_scope: один bounded re-eval packet для Cursor Row12 semantic-output structure после reference propagation fix
evidence_scope: прочитаны или распарсены prep rubric, prior eval, fix packet, new Path A request/core-response/core-trace/runner/latest, Path B UI report и join report; token savings `6209 -> 6209` взят из текущего human status и prior eval context, не пересчитывался этим worker
count_claimed: 1 новый reeval packet; 6 requested layer verdicts; 1 overall verdict; 0 full semantic quality/product readiness/token savings/training/checkpoint/locator claims
independent_evidence_count: 10 source artifacts plus required post-write verification commands after file creation
subagent_id_or_NONE: QUALITY-SEMANTIC-ROW12-CURSOR-REEVAL-AFTER-REFERENCE-FIX-01
downgrade_needed: да для full semantic quality, product readiness, token savings, training, checkpoint promotion и locator update; нет для narrow Row12 semantic structure

## Что улучшилось против prior eval

- Prior eval был `PARTIAL_NARROW_ROW12_SEMANTIC_EVAL`, потому что `CheckSetResults.references` внутри core response показывал `marker`, `scenario_id` и `pair_id` как `not-provided`.
- New Path A evidence после fix содержит эти значения прямо в `CheckSetResults.references`: `FRDP-P01-ROW12-CURSOR-TOKEN-SAVINGS-BASELINE-V1`, `frdp-p01-cursor-token-savings-baseline`, `frdp-p01-row12-cursor-token-baseline-v1`.
- New Path A evidence также содержит `structured_task_refs`, `scenario_refs`, `raw_prompt_sha256` и `raw_prompt_redacted=true`.
- Runner validation подтверждает те же request/response reference values, а core trace показывает `user_prompt_redacted=true`, `raw_private_prompt_persisted=false`, `contains_secrets=false`.

## Что осталось прежним

- `prompt_to_task`: PASS в prior eval и PASS сейчас.
- `task_to_plan`: PASS в prior eval и PASS сейчас.
- `task_to_skills`: PASS в prior eval и PASS сейчас.
- Path B остается same-scenario UI route correlation only: `submitted_to_core=false`; screenshots и marker сами по себе не являются semantic quality proof.
- Token savings не доказаны: текущий status говорит `6209 -> 6209`, delta `0`.

## Verdicts

| Layer | Verdict | Почему |
| --- | --- | --- |
| prompt_to_task | PASS | New core response nested JSON парсится; есть `task_id`, `task_title`, `task_type=plan_only_or_approval_gated_task`, constraints, deliverables, acceptance gates, `raw_prompt_redacted=true`, prompt hash и non-claims. |
| task_to_plan | PASS | Nested plan парсится; есть ровно 3 ordered steps; у каждого шага есть `uses` и `verification_gate`; план universal/approval-gated, не Cursor-only implementation script. |
| task_to_skills | PASS | Skills ровно из разрешенной route surface: `prompt-to-task`, `task-to-plan`, `task-to-skills`, `evidence-ledger`; candidate order/position помечены diagnostics/scaffold only, не final semantic authority. |
| references | PASS | В отличие от prior eval, `marker`, `scenario_id`, `pair_id`, structured task refs, scenario refs и redacted prompt hash теперь сохранены в new Path A core response references. |
| evidence_boundary | PASS | Evidence поддерживает только narrow Row12 semantic structure. Full semantic quality не доказана без source-disjoint eval, ablations, fresh-load и continue-training gates. |
| path_b | PASS | Path B совпадает по same scenario marker/pair/scenario и показывает UI route, но не core semantic output; screenshots/marker не считаются quality proof. |

Overall verdict: `PASS_NARROW_ROW12_SEMANTIC_STRUCTURE`.

## Evidence boundary

product_readiness_claimed: false
semantic_quality_claimed: false
token_savings_claimed: false
training_or_learnability_claimed: false
checkpoint_mutation_or_promotion_claimed: false
locator_update_claimed: false
external_ai_used: false

The evidence supports only: narrow Row12 Cursor semantic-output structure after reference preservation fix.

It does not prove:
- full semantic quality;
- token savings;
- all-extension readiness;
- product readiness;
- training or learnability;
- checkpoint promotion;
- locator update;
- full live host UI semantic submission.

## Required verification

Post-write verification is recorded in `reeval.json` after the required commands are run.

## Next bounded work order

`QUALITY-SEMANTIC-ROW12-HELDOUT-ABLATION-FRESHLOAD-01`

Purpose: produce source-disjoint held-out semantic quality eval with ablation/no-leakage/no-memorization controls, fresh-load gate and continue-training gate before any full semantic quality claim.

Allowed scope: read existing Row12 semantic evidence, define or run bounded source-disjoint eval artifacts, write a bounded evidence packet.

Forbidden scope: product code edits, governance edits, skills edits, active plan edits, external AI, training, checkpoint mutation, locator update, commit, push.
