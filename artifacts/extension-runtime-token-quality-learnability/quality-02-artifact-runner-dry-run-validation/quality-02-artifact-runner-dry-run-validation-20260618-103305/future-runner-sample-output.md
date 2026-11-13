# Future runner sample output

Статус: dry-run only, scores NOT_SCORED.

| Metric | Count |
| --- | --- |
| scenario_rows | 12 |
| pair_level_rows | 54 |
| user_adaptive_eval_only_scenarios | 7 |
| user_adaptive_eval_only_pair_rows | 21 |
| evidence_only_scenarios | 5 |
| evidence_only_pair_rows | 33 |
| common_blocked_scenarios_declared_excluded_from_sample_output | 12 |
| common_blocked_pair_rows_declared_excluded_from_sample_output | 54 |

JSONL содержит pair-level rows: 21 USER_ADAPTIVE_EVAL_ONLY rows с score_status=NOT_SCORED_DRY_RUN_ONLY и 33 EVIDENCE_ONLY rows с score_status=NON_RUNNABLE_EVIDENCE_ONLY. Evidence-only rows non-runnable сейчас и later-after-approval=false.

Все per_layer_scores имеют score_0_to_5=null и pass_fail=NOT_SCORED.
