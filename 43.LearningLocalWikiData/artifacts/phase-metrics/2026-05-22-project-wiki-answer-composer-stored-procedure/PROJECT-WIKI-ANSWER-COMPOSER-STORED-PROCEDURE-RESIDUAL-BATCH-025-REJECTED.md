# Project Wiki Answer Composer Stored-Procedure Residual Batch 025 - Rejected Probe

Date: 2026-05-22
report_scope: runner_emitted_metrics
unit: WikiAnswerComposer
batch_id: project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025
status: REJECTED_PROBE_PUBLICATION_SKIPPED
full_wiki_done_claim: false
training_promotion_claim: false

## Source Artifacts

run_output: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025\batch-025-run-output.json
dpo_summary: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025-project-wiki-answer-composer-unit-20260521232838\project-wiki-dpo-metric-summary.json
pair_diagnostics: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025-project-wiki-answer-composer-unit-20260521232838\dpo-pair-diagnostics.jsonl
initial_policy: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024-project-wiki-answer-composer-unit-20260521231627\checkpoints\policy_final.bin

## Raw Metrics

train_rows: 50
heldout_rows: 50
final_accuracy: 0.9200000047683716
final_reward_margin: 0.03264994543045759
final_loss: 0.6770615875720978
heldout_accuracy: 0.8799999952316284
heldout_reward_margin: 0.02996075339615345
heldout_loss: 0.6783812642097473
cpu_time_ms: 153421.875
average_cpu_percent: 12.642282979761578
peak_ram_mb: 88.1484375
elapsed_ms: 199985.1323
cpu_ms_per_train_row: 3068.4375
elapsed_ms_per_train_row: 3999.702646

## Comparison Against Accepted Batch 024

batch_024_heldout_accuracy: 0.9200000166893005
batch_024_heldout_reward_margin: 0.023365933448076248
batch_024_final_accuracy: 1.0
batch_025_heldout_accuracy_delta_vs_024: -0.04000002145767212
batch_025_heldout_margin_delta_vs_024: 0.006594819948077202
batch_025_publication_result: skipped
runner_warning: answer_composer_gate_failed_publication_skipped

## Held-Out Rejected-Type Metrics

procedure_name_only_count: 25
procedure_name_only_accuracy: 0.8
procedure_name_only_average_margin: 0.023218764811754226
procedure_name_only_min_margin: -0.018854521214962009
procedure_name_only_max_overlap: 0.058823529411764705

missing_dotnet_callers_count: 25
missing_dotnet_callers_accuracy: 0.96
missing_dotnet_callers_average_margin: 0.036702740490436557
missing_dotnet_callers_min_margin: -0.013034529983997343
missing_dotnet_callers_max_overlap: 0.12

## Interpretation

The residual probe improved the `missing_dotnet_callers` rejected type from
0.8 accuracy in batch 024 to 0.96 accuracy in batch 025. This is useful
evidence.

The same probe did not repair `procedure_name_only`. Accuracy stayed at 0.8
and min margin worsened from -0.0085631385 in batch 024 to
-0.018854521214962009 in batch 025.

The global held-out accuracy regressed from 0.9200000166893005 to
0.8799999952316284. The runner skipped publication. Therefore batch 025 is not
a promoted checkpoint and must not be used as the next policy baseline.

## Next Action

Do not continue from batch 025.
Continue from accepted batch 024.
Build the next 50-row remediation as a low-dose `procedure_name_only` batch
mixed with stable stored-procedure rejected types. The target is to repair the
name-only failure without losing the global held-out accuracy gate.
