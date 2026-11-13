# P16B: SkillSelection source/DPO materialization

## Краткий вывод

P16B материализует конкретные SkillSelection source/DPO candidate rows и split/license ledgers. Это не обучение и не checkpoint. Все candidate rows остаются candidate_training_ready=false до parent review, license/source gates, P16C tests и P16E ablations.

## Что принято как candidate material

- Для каждого main unit target создано по три concrete candidate rows: `train_candidate`, `eval_candidate`, `holdout_candidate`.
- Main unit targets: `candidate_position_guard`, `prompt_understanding`, `task_understanding`, `prompt_context`, `intermediate_checks`, `runtime_chain`.
- Все строки сохраняют `candidate_training_ready=false`, потому что P16B не выполняет parent acceptance, license verification, P16C tests, P16E ablations или checkpoint lifecycle.

## Что помещено в quarantine

- `candidate_position`: выбор по индексу кандидата.
- `exact_prompt_or_title`: выбор по буквальному prompt/title.
- `filename_path_title_source_id`: выбор по filename/path/source id.
- `project_specific_path_or_source_ref`: private/project provenance как authority.
- `test_marker`: выбор по test class/name marker.
- `project_stack_execution_recipe`: project-stack execution recipe вместо универсального SkillSelection boundary.

## License/source gate

Common training не разрешен в P16B. Public candidates вроде `openai-cookbook-task-routing-examples`, `kubernetes-docs-task-routing-parallels` и `apache-airflow-docs-operator-boundary` имеют `license_status=unverified`, поэтому route остается `evidence_only`. Local project skills/user prompts route идут в `adaptive_user_cache` или `evidence_only`, raw private text не копировался.

## Counts

- source ledger rows: 17
- DPO candidate rows: 18
- quarantine rows: 6
- license ledger rows: 12
- candidate training-ready rows: 0

## Four self-review/fix cycles

- p16b_review_1_goal_delivery: pass. Созданы конкретные source/DPO/split/license артефакты для SkillSelection без кода, обучения и checkpoint.
- p16b_review_2_source_license_privacy: pass_with_blockers. Лицензии common-кандидатов не подтверждены локально; такие строки оставлены evidence_only/adaptive и candidate_training_ready=false.
- p16b_review_3_dpo_concreteness_near_miss: pass. Для каждого основного unit target есть concrete chosen/rejected near miss rows и quarantine row с причиной утечки.
- p16b_review_4_no_shortcut_split_integrity: pass_with_blockers. Train/eval/holdout роли назначены, но все строки остаются не training-ready до P16C/P16E и parent review.

## Что остается заблокированным

- Нужна родительская проверка источников и split roles.
- Нужна license verification для любых common-source кандидатов.
- Нужны P16C focused tests для unit/branch behavior.
- Нужны P16E source-disjoint eval и ablation controls.
- Только после этого можно обсуждать checkpoint candidate в P16D.

## No-claim boundary

P16B не заявляет semantic quality, SkillSelection readiness, final model readiness, full API readiness, dashboard readiness или host runtime readiness.
