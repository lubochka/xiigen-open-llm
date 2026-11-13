# Base Wiki Held-Out Evaluator Gap

Status: PARTIALLY CLOSED, QUERY-PLANNING FLOW STILL MISSING.

Date: 2026-05-15

## What Was Implemented

A checkpoint-backed held-out preference/Q&A evaluator now exists at:

`C:/Projects/llm_mvp_core/experiments-v2/PROJECT-WIKI-BASE-HELDOUT-EVAL/runner/ProjectWikiBaseHeldoutEvaluator/ProjectWikiBaseHeldoutEvaluator.csproj`

It loads a trained `policy_step_*.bin` checkpoint, scores expected wiki answers against rejected answers using the same DPO policy-vs-reference reward margin, and records:

- generic documentation Q/A preference accuracy
- citation format accuracy
- missing-evidence abstention accuracy
- hallucination preference rate
- answer contract accuracy
- elapsed time per question
- CPU percent
- RAM usage

## What Still Fails

The best repeated training run passes in-batch DPO metrics:

- final_accuracy: 1.000
- final_reward_margin: 0.02374749258160591

But checkpoint-backed held-out Q/A still fails:

- generic_documentation_qa_accuracy: 0.2857142857142857
- citation_format_accuracy: 1.000
- missing_evidence_abstention_accuracy: 1.000
- hallucination_rate: 0.680
- answer_contract_accuracy: 1.000

## Remaining Architecture Gap

The failure is not only answer generation. The local wiki also lacks the
trainable unit that turns a developer question into a multi-step evidence query
plan. Without `ProjectWikiQueryPlanningFlow`, the evaluator can reward a
candidate answer but cannot prove the system knows what to search before it
answers.

Free generation remains a model capability issue, but the first architecture
gap to close is:

```text
developer question -> ProjectWikiQueryPlanningFlow -> planned evidence queries
-> retrieval -> answer -> verifier
```

## Required Remediation

Do not mark `BASE_MODEL_ACCEPTED` yet.

Next architecture options:

1. Add and train `ProjectWikiQueryPlanningFlow` with query-plan held-out
   metrics before answer evaluation.
2. Add checkpoint continuation and held-out-aware validation to the DPO runner.
3. Replace hash tokenization with a reversible tokenizer and add
   checkpoint-backed answer generation.
4. Use a trained embedding/KNN wiki answer unit for project-local answers and
   keep the DPO policy for routing, query planning, or answer validation.
