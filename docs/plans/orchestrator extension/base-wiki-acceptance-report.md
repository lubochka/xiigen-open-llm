# Base Wiki Acceptance Report

Status: SESSION-01 HELD-OUT Q/A FAILED - RUNNER ARCHITECTURE REMEDIATION REQUIRED.

Session: SESSION-01 - Base Wiki Model Definition And Validation

Date: 2026-05-15

## Best DPO Training Result

The strongest training-side result came from repeated exposure on the batch-004 held-out-style rows:

| Metric | Value |
|---|---:|
| run_id | base-wiki-session-01-batch-004-repeat10-500-20260515-1100 |
| pair_count | 500 |
| batch_count | 32 |
| total_elapsed_ms | 102735.5645 |
| average_accuracy | 0.86328125 |
| final_accuracy | 1 |
| average_reward_margin | 0.024884983664378524 |
| final_reward_margin | 0.02374749258160591 |
| final_loss | 0.6813642978668213 |

DPO-side gate: PASS.

## Checkpoint-Backed Held-Out Q/A Result

The held-out evaluator loads the trained policy checkpoint and scores valid wiki answers against invalid answers using DPO policy-vs-reference margin.

| Metric | Required | Actual | Status |
|---|---:|---:|---|
| generic_documentation_qa_accuracy | >= 0.90 | 0.2857142857142857 | Fail |
| citation_format_accuracy | >= 0.98 | 1 | Pass |
| missing_evidence_abstention_accuracy | >= 0.95 | 1 | Pass |
| hallucination_rate | 0.00 | 0.68 | Fail |
| answer_contract_accuracy | >= 0.98 | 1 | Pass |
| elapsed_ms_per_question | recorded | 131.891068 | Recorded |
| cpu_percent | recorded | 12.765032352304557 | Recorded |
| peak_ram_mb | recorded | 57.015625 | Recorded |

Final gate: NOT PASSED.

## Diagnosis

Adding more rows of the same hash-token DPO shape improves in-batch DPO metrics
but does not improve held-out wiki Q/A preference. The current runner can rank
familiar preference patterns, but it is not a sufficient base wiki answer model.
It also does not yet prove the missing orchestration step:
`ProjectWikiQueryPlanningFlow`, which must translate a developer question into
a multi-step evidence query plan before retrieval and answer composition.

## Required Remediation

Do not continue by blindly adding more same-shape rows.

The next implementation step must change the training/evaluation architecture:

1. Add and train `ProjectWikiQueryPlanningFlow` so held-out questions test the
   investigation plan before the answer.
2. Use a reversible tokenizer and checkpoint-backed answer generation, or
3. Add checkpoint continuation with held-out-aware validation, or
4. Train a project/wiki answer unit based on embeddings/KNN over accepted
   evidence instead of relying on this hash-token DPO policy for Q/A.

SESSION-02 remains blocked. BASE_MODEL_ACCEPTED is not reached.
