# QUALITY-SEMANTIC-GATES-23 local adaptive readiness

QSG-23 создал staged/test-only доказательства локального adaptive lifecycle для полных машин PromptToTaskMachine и SkillSelectionMachine.

## Что подтверждено

- Для обеих машин staged user/adaptive checkpoint+manifest загружаются раньше repo/common fallback.
- SkillSelection fallback проверен через конкретный отсутствующий staged user checkpoint path, после чего зафиксирован переход на repo/common fallback.
- Runtime-вызов проходит через публичные границы машин: PromptToTaskMachine.Translate и SkillSelectionMachine.DecideFromTrainedState / ImportAsync.
- Continue-training начинается с уже существующего staged checkpoint и пишет только QSG-23 артефакты.
- Backup, export/import и rollback comparison записаны только для staged assets.
- SkillSelection candidate-position state проверен и помечен как baseline/scaffold/leakage-guard; staged semantic decision не использует candidate position как финальный источник истины.
- PromptToTask staged output проверен на universal task fields и отсутствие project-stack-specific leakage в этих полях.
- Валидация перечисляет и строго парсит все QSG-23 JSON/JSONL, включая asset checkpoint/manifest JSON, без снятия UTF-8 BOM.

## Что не подтверждено

- Synthetic/test-only controls не доказывают semantic quality, quality improvement, token savings, real learnability, common readiness, production promotion, production rollback closure или final readiness.
- Production locator/checkpoint файлы не изменялись.
- QSG-23 не открывает product-readiness claim.

## Следующий шаг

QUALITY-SEMANTIC-GATES-24 должен заменить synthetic/test-only controls на реальные AI-output/correction DPO rows и source-disjoint held-out evaluation, либо перейти в отдельный production-mutation track только после явного одобрения Лубы на точную мутацию.


## Открытые claims

- qsg23_prompt_to_task_staged_full_machine_checkpoint_manifest_fresh_load_invocation
- qsg23_skill_selection_staged_full_machine_checkpoint_manifest_fresh_load_invocation
- qsg23_user_adaptive_load_order_before_repo_common_fallback_recorded
- qsg23_continue_training_from_existing_staged_checkpoint_recorded
- qsg23_staged_backup_export_import_rollback_recorded
- qsg23_source_disjoint_synthetic_held_out_ablation_controls_recorded
- qsg23_skill_selection_candidate_position_boundary_checked
- qsg23_prompt_to_task_universal_task_boundary_checked

## Закрытые claims

- token_savings_claim_allowed
- quality_improvement_claim_allowed
- semantic_quality_claimed
- learnability_claimed
- common_readiness_claim_allowed
- production_locator_promotion_done
- locator_update_done
- production_rollback_closure_done
- final_readiness_claimed
