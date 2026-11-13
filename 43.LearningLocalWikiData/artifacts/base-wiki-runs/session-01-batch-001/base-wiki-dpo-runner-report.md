# Base Wiki DPO Runner Report - Session 01 Batch 001

Status: DPO_RUNNER_COMPLETE_GATE_FAILED.

Run id: base-wiki-session-01-batch-001-20260515-0915
Runner result root: C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\results\base-wiki-session-01-batch-001-20260515-0915
Pairs: 50
Batches: 4

## Raw Metrics

| Metric | Value |
|---|---:|
| total_elapsed_ms | 11768.3819 |
| elapsed_ms_per_row | 235.367638 |
| average_loss | 0.7066001743078232 |
| final_loss | 0.7113677859306335 |
| average_accuracy | 0.1875 |
| final_accuracy | 0 |
| average_reward_margin | -0.02652484964346513 |
| final_reward_margin | -0.03607501834630966 |
| average_cpu_percent | 12.608709 |
| max_cpu_percent | 13.411158 |
| total_working_set_delta_mb | 19.179688 |
| max_working_set_delta_mb | 16.59375 |
| checkpoint_bytes_total | 1189258 |

## Gate

Required final_accuracy >= 0.90 and final_reward_margin >= 0.02.

Actual final_accuracy = 0.
Actual final_reward_margin = -0.03607501834630966.

Gate result: FAILED.

## Remediation

The rows were structurally valid, but the diagnostic batch was too templated and repetitive. The next batch must use varied prompts and answer-specific chosen text, not repeated instruction-only chosen text.

SESSION-02 remains blocked until BASE_MODEL_ACCEPTED.
