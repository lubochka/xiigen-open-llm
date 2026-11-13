# EntityImpactMapper Phase Metrics - 2026-05-21

report_scope: EntityImpactMapper 10-batch training cycle
metric_format: raw_numbers
source_of_truth: runner-emitted JSON artifacts
false_positive_guard: structural entity-impact recall is not enough for publication without DPO held-out metrics

## Current Status

EntityImpactMapper structural gate passes: evidence recall and touchpoint recall
are both `1.0` on the current generic batch. DPO publication is now wired into
the pipeline, so this unit cannot publish from structural metrics alone.

Current best balanced accuracy run:
`entity-impact-mapper-compact-continuation-004`, with
`dpo_heldout_accuracy = 0.8600000143`.

Current strongest margin run:
`entity-impact-mapper-compact-from-target-boost-continuation-010`, with
`dpo_heldout_accuracy = 0.8000000119` and
`dpo_heldout_margin = 0.0303401537`.

This is not a full wiki completion claim. `missing_calculation` remains a known
weak rejected type and must stay in the remediation backlog before advanced
entity-impact prompts can be called excellent.

## Ten-Batch Aggregate Cost

```text
run_count = 10
average_dpo_heldout_accuracy = 0.7180000007
average_dpo_reward_margin = 0.0081059721
average_cpu_time_ms = 188314.0625
average_peak_ram_mb = 102.5703125
average_elapsed_ms = 245070.65632
average_cpu_time_ms_per_train_row = 3766.28125
average_elapsed_ms_per_train_row = 4901.4131264
average_cpu_time_ms_per_total_train_plus_heldout_row = 1883.140625
average_elapsed_ms_per_total_train_plus_heldout_row = 2450.7065632
```

## Raw Run Metrics

| batch | evidence_recall | touchpoint_recall | dpo_heldout_accuracy | dpo_margin | dpo_loss | cpu_time_ms | peak_ram_mb | elapsed_ms | verdict |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---|
| entity-impact-mapper-batch-001 | 1 | 1 | 0.8199999928 | 0.0011477584 | 0.6930231011 | 158937.5 | 104.75390625 | 230491.7867 | DPO_GATE_PASSED_WEAK_MARGIN |
| entity-impact-mapper-continuation-002 | 1 | 1 | 0.8199999928 | 0.0022877331 | 0.6927896452 | 135671.875 | 102.71484375 | 199635.1914 | DPO_GATE_PASSED_WEAK_MARGIN |
| entity-impact-mapper-compact-003 | 1 | 1 | 0.8399999738 | 0.0028273633 | 0.6922275889 | 165765.625 | 109.12890625 | 228862.4396 | DPO_GATE_PASSED_WEAK_MARGIN |
| entity-impact-mapper-compact-continuation-004 | 1 | 1 | 0.8600000143 | 0.0054805167 | 0.6903561985 | 162359.375 | 104.671875 | 220592.397 | BEST_ACCURACY |
| entity-impact-mapper-compact-calculation-strong-missing-calculation-remediation-005 | 1 | 1 | 0.6000000238 | 0.0013968375 | 0.6901502466 | 154562.5 | 88.90625 | 198397.3874 | REJECTED_SHAPE |
| entity-impact-mapper-compact-continuation-006 | 1 | 1 | 0.8199999928 | 0.0079906173 | 0.6886095488 | 121953.125 | 104.2734375 | 158069.6767 | DPO_GATE_PASSED |
| entity-impact-mapper-compact-derived-status-missing-calculation-remediation-007 | 1 | 1 | 0 | -0.0093137631 | 0.6955369949 | 212406.25 | 109.7265625 | 308492.5562 | REJECTED_SHAPE |
| entity-impact-mapper-compact-target-boost-008 | 1 | 1 | 0.8000000119 | 0.0151230935 | 0.6862445313 | 314828.125 | 93.4765625 | 382550.3444 | DPO_GATE_PASSED_HIGH_COST |
| entity-impact-mapper-compact-from-target-boost-continuation-009 | 1 | 1 | 0.8199999928 | 0.0237794109 | 0.6804088897 | 228750 | 103.20703125 | 263208.3375 | DPO_GATE_PASSED_STRONG_MARGIN |
| entity-impact-mapper-compact-from-target-boost-continuation-010 | 1 | 1 | 0.8000000119 | 0.0303401537 | 0.6753010720 | 227906.25 | 104.84375 | 260406.4463 | DPO_GATE_PASSED_STRONG_MARGIN |

## Weak Rejected Types After Batch 010

| rejected_type | heldout_count | accuracy | margin |
|---|---:|---:|---:|
| missing_calculation | 10 | 0.5 | 0.003538 |
| missing_persistence | 10 | 0.8 | 0.015305 |
| wrong_project_evidence | 10 | 0.8 | 0.019223 |
| partial_touchpoint_map | 10 | 0.9 | 0.016524 |
| missing_mapping | 10 | 1.0 | 0.097110 |

## Additional Remediation Runs 011-018

```text
011 calculation-path prose-heavy evidence design:
  heldout_accuracy = 0.3000000119
  heldout_margin = -0.0251729656
  verdict = REJECTED_SHAPE_TOO_LONG_AND_DISTRIBUTION_SHIFTED

012 calculation-signal applied to every rejected type:
  heldout_accuracy = 0.5400000215
  heldout_margin = 0.0059720571
  missing_calculation heldout_accuracy = 1.0
  verdict = REJECTED_OVERBROAD_SIGNAL_DAMAGED_OTHER_TYPES

013 calculation-signal hybrid, only missing_calculation gets the new signal:
  heldout_accuracy = 0.8000000119
  heldout_margin = 0.0361287780
  missing_calculation heldout_accuracy = 0.9
  verdict = ACCEPTED_SIGNAL_SHAPE_BUT_BOUNDARY_PERSISTENCE_WEAK

014 same hybrid continuation:
  heldout_accuracy = 0.8000000119
  heldout_margin = 0.0340407714
  verdict = STABLE_BUT_NO_WEAK_TYPE_IMPROVEMENT

015 coverage-signal for calculation, persistence, and project boundary:
  heldout_accuracy = 0.6600000262
  heldout_margin = 0.0162245706
  missing_calculation heldout_accuracy = 1.0
  wrong_project_evidence heldout_accuracy = 1.0
  verdict = REJECTED_SHAPE_DAMAGED_PERSISTENCE_AND_PARTIAL_MAP

016 compact continuation after 015:
  heldout_accuracy = 0.8000000119
  heldout_margin = 0.0347945243
  missing_calculation heldout_accuracy = 0.5
  verdict = REJECTED_REVERTED_TO_PREVIOUS_CALCULATION_GAP

017 low-dose calculation-signal hybrid from batch010:
  heldout_accuracy = 0.8999999762
  heldout_margin = 0.0391545258
  missing_calculation heldout_accuracy = 1.0
  verdict = ACCEPTED_MILESTONE_CANDIDATE

018 low-dose calculation-signal confirmation from batch017:
  heldout_accuracy = 0.8999999762
  heldout_margin = 0.0388998166
  missing_calculation heldout_accuracy = 1.0
  verdict = PROMOTED_MILESTONE_WITH_WATCH_ITEMS
```

## Weak Rejected Types After Batch 018

| rejected_type | heldout_count | accuracy | margin | note |
|---|---:|---:|---:|---|
| missing_persistence | 10 | 0.8 | 0.014401 | watch item, passes milestone |
| missing_mapping | 10 | 1.0 | 0.099447 | strong |
| missing_calculation | 10 | 1.0 | 0.047584 | repaired |
| wrong_project_evidence | 10 | 0.8 | 0.017078 | watch item, passes milestone |
| partial_touchpoint_map | 10 | 0.9 | 0.015989 | watch item due one negative-margin row |

## Accepted Directions

```text
plain full chosen/rejected: passes DPO accuracy but margin is too weak.
compact chosen/rejected: improves accuracy to 0.86 and margin to 0.00548.
target-boost from compact checkpoint: improves margin but costs more CPU and
reduces count accuracy to 0.80.
target-boost continuation: keeps accuracy above 0.80 and pushes margin above
0.02 for two consecutive runs.
low-dose calculation-signal hybrid from batch010: repairs missing_calculation
without destroying the existing compact mixed-map signal; confirmed in two
consecutive 50-row runs, batches 017 and 018.
```

## Rejected Directions

```text
calculation-strong focused remediation: regressed to 0.6000000238.
derived-status focused remediation: collapsed to 0.0 and negative margin.
Do not continue either shape without a new evidence design.
calculation-path prose-heavy shape: collapsed to 0.3000000119.
calculation-signal on every rejected type: fixed calculation but damaged the
other rejected types.
coverage-signal across multiple weak types: fixed calculation and project
boundary but damaged persistence and partial maps.
```

## Next Training Action

```text
source: real-project QA validation on advanced entity-impact prompts.
current entity-impact status: promoted milestone with runner manifest, not full
wiki done.
next focus: validate persistence and wrong-project boundary rows on the five
.NET test projects and keep low-dose calculation-signal as the accepted
missing_calculation remediation pattern.
```
