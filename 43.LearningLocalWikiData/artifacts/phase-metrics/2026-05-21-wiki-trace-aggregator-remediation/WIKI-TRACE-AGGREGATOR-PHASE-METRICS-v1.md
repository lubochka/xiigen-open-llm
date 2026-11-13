# WikiTraceAggregator Phase Metrics - 2026-05-21

report_scope: unit_training_phase
unit_id: wiki-trace-aggregator-unit
status: UNIT_SIGNAL_PROMOTED_FOR_TRACE_AGGREGATION_AND_CONTEXT_SEQUENCE
full_wiki_done: false
source_of_truth: runner-emitted project-wiki-dpo-metric-summary.json + dpo-pair-diagnostics.jsonl

## Goal

Train the trace aggregation unit to read WikiQueryTrace and WikiUnitTrace,
name failed units, preserve concrete remediation actions, and stop blind
continuation when a measured gap remains.

## Accepted Checkpoint

batch_id: wiki-trace-aggregator-context-before-confirmation-022
held_out_accuracy: 1.0
held_out_reward_margin: 0.016503408551216125
final_accuracy: 1.0
final_reward_margin: 0.017778544779866935
final_loss: 0.6843108028173447
average_cpu_percent: 12.69575509149772
total_cpu_time_ms: 103796.875
peak_working_set_mb: 101.1328125
elapsed_ms: 143022.8577
policy_checkpoint_path: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-wiki-trace-aggregator-context-before-confirmation-022\dpo-runs\wiki-trace-aggregator-unit\wiki-trace-aggregator-context-before-confirmation-022-wiki-trace-aggregator-unit-20260521212719\checkpoints\policy_final.bin
reference_checkpoint_path: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-wiki-trace-aggregator-context-before-confirmation-022\dpo-runs\wiki-trace-aggregator-unit\wiki-trace-aggregator-context-before-confirmation-022-wiki-trace-aggregator-unit-20260521212719\checkpoints\reference_base.bin
generated_manifest_path: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-wiki-trace-aggregator-context-before-confirmation-022\wiki-trace-aggregator-unit\wiki-trace-aggregator-20260521212942\generated_capability_manifest.json

Previous accepted unit signal:
batch_id: wiki-trace-aggregator-incompletefix-batch-016
held_out_accuracy: 1.0
held_out_reward_margin: 0.02904154360294342
final_accuracy: 0.9200000047683716
final_reward_margin: 0.025786187723278998

## Held-Out Rejected Type Metrics

| rejected_type | count | accuracy | avg_margin | min_margin | max_margin |
|---|---:|---:|---:|---:|---:|
| continues_blind_training | 10 | 1.0 | 0.0170775596052408 | 0.000193214043974876 | 0.0346333496272564 |
| false_failure | 10 | 1.0 | 0.048060417175293 | 0.048060417175293 | 0.048060417175293 |
| ignores_failed_unit | 10 | 1.0 | 0.0269416809082031 | 0.019396685063839 | 0.0369158759713173 |
| incomplete_trace_summary | 10 | 1.0 | 0.0359523393213749 | 0.0191093422472477 | 0.0474563576281071 |
| missing_remediation | 10 | 1.0 | 0.0171757318079472 | 0.00822544097900391 | 0.0273429863154888 |

## Run History

| batch | held_out_accuracy | held_out_margin | final_accuracy | final_margin | cpu_ms | peak_ram_mb | elapsed_ms | verdict |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| batch-001 | 0.7200000286102295 | 0.001539486227557063 | 0.7400000095367432 | 0.000833276005067205 | 165406.25 | 87.48046875 | 219830.5781 | HELD |
| cont-batch-003 | 0.7200000286102295 | 0.003228390123695135 | 0.7840000069141388 | 0.002532430290157208 | 146750 | 104.87109375 | 199422.0387 | HELD |
| ignorefix-batch-010 | 0.8799999952316284 | 0.008177670650184155 | 0.8800000071525573 | 0.006560257416567765 | 193250 | 103.69140625 | 273748.8127 | ACCEPT_SIGNAL_CONTINUE |
| ignorefix-cont-batch-011 | 0.9200000166893005 | 0.011679534800350666 | 0.9160000050067901 | 0.00996825072914362 | 191000 | 85.27734375 | 264526.0428 | ACCEPT_SIGNAL_CONTINUE |
| ignorefix-cont-batch-012 | 0.9200000166893005 | 0.015108397230505943 | 0.9200000047683716 | 0.013211458548903465 | 171218.75 | 87.91796875 | 222722.1896 | ACCEPT_SIGNAL_CONTINUE |
| ignorefix-cont-batch-013 | 0.9200000166893005 | 0.01857532002031803 | 0.9200000047683716 | 0.016429026145488024 | 121093.75 | 75.2578125 | 157026.5691 | ACCEPT_SIGNAL_CONTINUE |
| ignorefix-cont-batch-014 | 0.9200000166893005 | 0.021969353780150414 | 0.9200000047683716 | 0.019603839619085194 | 121187.5 | 87.69140625 | 158103.602 | ACCEPT_SIGNAL_CONTINUE |
| ignorefix-cont-batch-015 | 0.9599999785423279 | 0.025222355499863625 | 0.9280000042915344 | 0.02267255736514926 | 121703.125 | 107.140625 | 157507.3167 | ACCEPT_SIGNAL_CONTINUE |
| incompletefix-batch-016 | 1.0 | 0.02904154360294342 | 0.9200000047683716 | 0.025786187723278998 | 144859.375 | 90.62890625 | 191188.0473 | UNIT_SIGNAL_PROMOTED |
| cross-unit-sequence-017 | 0.5199999809265137 | -0.0022927753161638975 | n/a | n/a | 180515.625 | 102.50390625 | n/a | REJECT_DPO_BELOW_0_75 |
| cross-unit-sequence-fresh-018 | 0.03999999910593033 | -0.0013259467668831348 | n/a | n/a | 156000 | 88.40234375 | n/a | REJECT_DPO_BELOW_0_75 |
| cross-unit-sequence-019 | 0.07999999821186066 | -0.027974216267466545 | n/a | n/a | 150187.5 | 104.45703125 | n/a | REJECT_DPO_BELOW_0_75 |
| no-precheck-owner-generic-020 | 0.6800000071525574 | 0.0034961469937115908 | n/a | n/a | 126062.5 | 88.4296875 | n/a | HELD_DPO_BELOW_0_75 |
| context-before-generic-021 | 0.800000011920929 | 0.00798601470887661 | n/a | n/a | 122984.375 | 86.4765625 | n/a | ACCEPTED_BUT_REJECTED_REASON_IMPRECISE |
| context-before-confirmation-022 | 1.0 | 0.016503408551216125 | 1.0 | 0.017778544779866935 | 103796.875 | 101.1328125 | 143022.8577 | UNIT_SIGNAL_PROMOTED |

## Rejected Experiments

| batch | held_out_accuracy | held_out_margin | reason_rejected |
|---|---:|---:|---|
| signal-batch-002 | 0.03999999910593033 | -0.006137670483440161 | target labels trained in the wrong direction for this unit |
| remediation-batch-004 | 0.46000000834465027 | -0.0005669309175573289 | broad chosen/rejected expansion overloaded the pair shape |
| vocab-batch-005 | 0.30000001192092896 | -0.0007531295996159315 | all-unit vocabulary was grouped by rejected type and caused recency/type collapse |
| target-batch-006 | 0.23999999463558197 | -0.002604680135846138 | short target labels reversed for failed-trace rows |
| target-interleaved-batch-007 | 0.3199999928474426 | -0.002644491149112582 | interleaving did not fix target-label reversal |
| interleaved-batch-008 | 0.3400000035762787 | -0.000690053915604949 | all-unit interleaving still regressed from the best natural baseline |
| cont-batch-009 | 0.6800000071525574 | 0.005038059316575527 | continuation without ignores_failed_unit fix regressed accuracy |

## Proven Fixes

1. `ignores_failed_unit` rejected must not keep `stop_blind_training: true`.
   It must explicitly hide the failed unit and continue blindly:
   `failed_units: none`, `remediation: none`, `stop_blind_training: false`.
2. `incomplete_trace_summary` rejected must explicitly omit all three required
   trace outputs:
   `failed_units: none`, `remediation: none`, `stop_blind_training: false`.
3. Do not use the short target-signal branch for this unit. It reversed the
   failed-trace rows even after row interleaving.
4. Do not group all rows by rejected type when broadening vocabulary. If broad
   vocabulary is reintroduced, row order must be balanced and verified by
   per-type held-out diagnostics.
5. Cross-unit no-precheck failures are not a WikiContextRoutingUnit-only
   label problem. The trace must preserve the sequence:
   ProjectWikiQueryPlanningFlow verifies project/version/evidence prechecks,
   then WikiContextRoutingUnit retrieves context, then evidence analysis gates
   WikiAnswerComposer. Missing this order is a WikiTraceAggregator remediation
   target.
6. For context-before-plan rows, the rejected side must name the actual defect:
   `[WRONG: context_before_query_plan_verified ...]`. Batch 021 passed
   numerically with an imprecise fallback annotation, so batch 022 is the
   accepted confirmation checkpoint.

## Next Required Work

This is a unit-level success only. Before any full wiki/system done claim:

1. Run real project WikiQueryTrace/WikiUnitTrace validation across the five .NET projects.
2. Verify advanced developer prompts are routed through ProjectWikiQueryPlanningFlow before retrieval.
3. Verify ProjectAnswerVerifier still needs remediation or promote it in a later run.
4. Update project-local wiki coverage metrics after documentation and question-answer gates.
