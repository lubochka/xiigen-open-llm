# QUALITY-SEMANTIC-GATES-25

Status: PASS_QSG25_STAGED_TRAINED_ADAPTIVE_EVAL_RECORDED_NO_COMMON_OR_PRODUCTION_READINESS

## What this proves

- QSG-24 rows were classified into staged user-adaptive trainable, common-trainable, eval-only, and quarantine routes.
- PromptToTask and SkillSelection staged checkpoints were loaded from existing QSG-23 staged assets and continued through project machine APIs.
- Trained staged targets were fresh-loaded and evaluated on source-disjoint held-out rows with ablation against the pre-QSG-25 staged base.
- Backup, export, import/share, rollback, and load-order evidence was recorded under QSG-25 artifacts only.
- Production model/checkpoint/locator/index refs were hash-checked and not mutated.

## What this does not prove

- It does not prove common model readiness because common-training license/provenance remains closed.
- It does not prove product final readiness, production locator promotion, token savings, or cross-host quality improvement.
- It does not claim semantic quality because the project APIs and checkpoints still explicitly keep semantic_quality_claimed=false.
- Any narrow staged learnability claim is limited to the named machine whose held-out+ablation gate passed.

## Row classification

- User-adaptive trainable rows: 34
- Common-trainable rows: 0
- Eval-only rows: 16
- Quarantined rows: 12

Common readiness remains closed because no row has clean MIT/Apache/explicit common-training approval.

## Real training and eval

- PromptToTaskMachine: PromptToTaskMachine.ContinueWithAcceptedPattern; train rows 8; held-out learned accuracy 1; ablation learned accuracy 1; strict gate false.
- SkillSelectionMachine: SkillSelectionMachine.TrainMachineBatchAsync; train rows 26; held-out exact accuracy 1; ablation exact accuracy 1; order invariant rate 1; strict gate false.

## Claims

Opened:
- qsg25_qsg24_rows_classified_for_user_adaptive_common_eval_quarantine
- qsg25_no_common_training_rows_opened_without_clean_license
- qsg25_prompt_to_task_continue_training_api_invoked_from_staged_checkpoint
- qsg25_skill_selection_train_machine_batch_api_invoked_from_staged_checkpoint
- qsg25_trained_staged_checkpoints_fresh_loaded
- qsg25_backup_export_import_rollback_paths_recorded
- qsg25_source_disjoint_heldout_ablation_eval_against_trained_checkpoint_recorded
- qsg25_no_production_locator_or_checkpoint_mutation_verified

Kept closed:
- common_readiness_claim_allowed
- semantic_quality_claimed
- quality_improvement_claim_allowed
- token_savings_claim_allowed
- production_locator_promotion_done
- locator_update_done
- production_rollback_closure_done
- final_readiness_claimed
- qsg25_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed
- qsg25_skill_selection_narrow_staged_user_adaptive_learnability_gate_passed

## Next QSG step

QUALITY-SEMANTIC-GATES-26: repair the failed narrow staged held-out+ablation gaps by adding license/provenance-clean train-ready rows or improving the actual project machine training/eval API, still without production mutation.
