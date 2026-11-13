# QUALITY-SEMANTIC-GATES-26

Status: PASS_QSG26_COMMON_ONLY_ABLATION_DELTA_EVAL_RECORDED_NO_PRODUCTION_MUTATION

## What this proves

- QSG-24 rows were classified into staged user-adaptive trainable, common-trainable, eval-only, and quarantine routes.
- PromptToTask and SkillSelection staged checkpoints were loaded from existing QSG-23 staged assets and continued through project machine APIs.
- Trained staged targets were fresh-loaded and evaluated on source-disjoint held-out rows with ablation against repo common-only/no-adaptive checkpoints.
- Backup, export, import/share, rollback, and load-order evidence was recorded under QSG-26 artifacts only.
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
- SkillSelectionMachine: SkillSelectionMachine.TrainMachineBatchAsync; train rows 26; held-out exact accuracy 0.8333; ablation exact accuracy 0.3333; order invariant rate 1; strict gate true.

## Root cause and fix

- Ablation was not truly no-adaptive: QSG-25 compared trained staged checkpoints against the pre-QSG-25 QSG-23 staged adaptive backup instead of repo common-only/no-adaptive checkpoints.
- SkillSelection training updated the typed final-selection state, but runtime underused that state: PromptContext/other units could return before SkillSelection, and SkillSelectionSkillUnit emitted only one top scored skill despite multi-label chosen rows.
- Keep real continue-training from staged adaptive base, evaluate ablation by loading repo common-only/no-adaptive checkpoints, make SkillSelection final-selection state reachable before broader units, and emit all available skills with positive learned final-selection scores.
- PromptToTask remains closed because the QSG-24 held-out rows are already learned by common/staged generic patterns under the material learned-status scorer; QSG-27 needs stricter source-disjoint real-reference held-out rows or a stricter task-type scorer.

## Claims

Opened:
- qsg26_qsg24_rows_classified_for_user_adaptive_common_eval_quarantine
- qsg26_no_common_training_rows_opened_without_clean_license
- qsg26_prompt_to_task_continue_training_api_invoked_from_staged_checkpoint
- qsg26_skill_selection_train_machine_batch_api_invoked_from_staged_checkpoint
- qsg26_trained_staged_checkpoints_fresh_loaded
- qsg26_backup_export_import_rollback_paths_recorded
- qsg26_ablation_corrected_to_repo_common_only_no_adaptive_state
- qsg26_source_disjoint_heldout_ablation_eval_against_trained_checkpoint_recorded
- qsg26_no_production_locator_or_checkpoint_mutation_verified
- qsg26_skill_selection_narrow_staged_user_adaptive_learnability_gate_passed

Kept closed:
- common_readiness_claim_allowed
- semantic_quality_claimed
- quality_improvement_claim_allowed
- token_savings_claim_allowed
- production_locator_promotion_done
- locator_update_done
- production_rollback_closure_done
- final_readiness_claimed
- qsg26_prompt_to_task_narrow_staged_user_adaptive_learnability_gate_passed

## Next QSG step

QUALITY-SEMANTIC-GATES-27: repair the remaining narrow staged held-out+ablation gaps with stricter source-disjoint real-reference rows or a narrow runtime/training API fix, still without production mutation.
