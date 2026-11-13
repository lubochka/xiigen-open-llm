# QUALITY-SEMANTIC-GATES-28 - semantic quality / host matrix

## Итог

QSG-28 собрал матрицу по трем расширениям и двум trainable units. Узкое staged learnability теперь открыто для PromptToTask из QSG-27 и SkillSelection из QSG-26. Это не product final readiness и не broad semantic quality.

## Что доказано

- QSG-21: Claude, Cursor и Codex уже имеют target-bound host evidence, но без full live UI/chat и без final readiness.
- QSG-26: SkillSelectionMachine прошел source-disjoint held-out + common-only ablation как narrow staged learnability.
- QSG-27: PromptToTaskMachine прошел strict source-disjoint held-out + common-only ablation как narrow staged learnability.
- QSG-28: создан test-only combined locator/root: PromptToTask из QSG-27 и SkillSelection из QSG-26; production locator/checkpoints не менялись.
- QSG-28: Claude и Codex direct adapter/core paths прогнали PromptToTask и SkillSelection через staged root. Cursor Extension Host подтвердил staged target locator/root load, но meaningful SkillSelection row все равно требует AvailableSkills binding.
- QSG-28: staged target root содержит явно зафиксированный test-only/deterministic risk, если checkpoint содержит deterministic_shortcut/test_only_fixture; это ограничение не закрывает broad semantic quality и не является production readiness.

## Что не доказано

- Product final readiness.
- Common readiness.
- Production promotion / locator update / production rollback closure.
- Token savings и quality improvement.
- Broad semantic quality.
- Clean no-hidden-deterministic-shortcut claim для staged target root.
- Cursor task/plan->skills source-disjoint semantic row через host command, потому текущий Cursor command не передает AvailableSkills.

## Matrix

| Host | QSG-21 target-bound host | QSG-28 PromptToTask staged root | QSG-28 SkillSelection staged root | Meaningful SkillSelection row |
| --- | --- | --- | --- | --- |
| Claude | PASS | PASS | PASS | PASS |
| Cursor | PASS | PASS | PASS | NOT_PROVEN |
| Codex | PASS | PASS | PASS | PASS |

## Claims opened

- qsg28_source_disjoint_two_unit_matrix_created
- qsg28_prompt_to_task_narrow_staged_learnability_opened_from_qsg27
- qsg28_skill_selection_narrow_staged_learnability_opened_from_qsg26
- qsg28_combined_test_only_locator_created_for_qsg27_prompt_to_task_and_qsg26_skill_selection
- qsg28_non_production_staged_prompt_to_task_adapter_core_binding_recorded_for_claude_and_codex
- qsg28_non_production_staged_skill_selection_adapter_core_binding_recorded_for_claude_and_codex
- qsg28_cursor_staged_target_locator_asset_load_recorded_skill_available_skills_binding_gap_opened
- qsg28_staged_target_root_test_only_deterministic_signal_risk_recorded

## Claims kept closed

- product_final_readiness
- common_readiness_claim_allowed
- production_locator_promotion_done
- locator_update_done
- production_rollback_closure_done
- token_savings_claim_allowed
- quality_improvement_claim_allowed
- broad_semantic_quality_claimed
- full_live_ui_chat_readiness
- cursor_task_plan_to_skills_source_disjoint_host_semantic_evaluation
- qsg28_clean_no_hidden_deterministic_shortcut_for_target_root

## Next QSG step

Add or use a non-production Cursor host command/test binding that passes AvailableSkills/RequestedSkills into XiigenExtensionRequest, then rerun the QSG-26 SkillSelection source-disjoint held-out row through Cursor Extension Host with the QSG-28 combined staged root. Optionally parameterize Claude live plugin and Codex app-server runners with the QSG-28 locator for live-surface staged-root proof.
