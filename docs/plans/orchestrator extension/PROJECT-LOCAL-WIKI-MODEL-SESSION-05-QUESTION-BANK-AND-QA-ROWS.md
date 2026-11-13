# SESSION-05 - Question Bank And Wiki Q/A Rows

Status: EXECUTION SESSION.

This is Phase 2. It is not part of the Phase 1 documentation-negative DTO path.

Do not start this session until SESSION-04 has produced and accepted the local
documentation wiki model.

## Goal

Generate/update the trainable question bank, export selected question seeds,
then build `batch-001` with 50 Q/A train rows and 50 held-out rows from accepted
project documentation.

## Required Reads

1. `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
2. Selected project's `.xiigen/wiki-domain/domain-manifest.json`
3. Selected project's `.xiigen/wiki-domain/evidence/evidence-index.jsonl`
4. Selected project's `.xiigen/wiki-domain/relations/documentation-units.jsonl`
5. Selected project's `.xiigen/wiki-domain/relations/call-edges.jsonl`
6. Selected project's `.xiigen/wiki-domain/relations/value-flow-edges.jsonl`
7. Accepted documentation under `.xiigen/wiki-domain/documentation/`
8. `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`
9. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`
10. `EXECUTOR-STAGE-PROMPT-v2.md`
11. `EXECUTOR-DISCIPLINE-RULES-v1.md`

If `.xiigen/wiki-domain/question-bank/question-bank-manifest.json` already
exists, read it and update it. If it does not exist, create it in this session.

## Canonical Question Taxonomy

Use the expanded developer-wiki taxonomy:

- Level 0: `wrong_project_question`, `stale_version_question`
- Level 1: `symbol_question`, `location_question`, `parameter_question`, `return_question`
- Level 2: `caller_question`, `callee_question`, `dependency_question`, `value_flow_question`
- Level 3: `entity_impact_question`, `flow_trace_question`, `change_location_question`, `impact_analysis_question`, `regression_risk_question`, `stored_procedure_impact_question`, `stored_procedure_trace_question`
- Level 4: `architectural_decision_question`, `system_boundary_question`, `feature_question`, `integration_boundary_question`
- Level 5: `missing_evidence_question`, `evidence_question`, `wrong_level_question`, `partial_trace_question`

These are question classes only. Phase 1 developer-reasoning DPO rows may use
the same task names, but the Phase 2 question bank still contains Q/A rows, not
rejected answer pairs.

When the project wiki contains accepted SQL/stored-procedure evidence, include
stored-procedure question rows in the bank and validate the eight stored
procedure variations from `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`. DNN, old
nopCommerce, and SmartStoreNET are validation-only surfaces unless separately
approved as training sources.

## Outputs

```text
.xiigen/wiki-domain/question-bank/question-bank-export.jsonl
.xiigen/wiki-domain/question-bank/question-bank-manifest.json
.xiigen/wiki-domain/question-bank/question-seeds-batch-001.jsonl
.xiigen/wiki-domain/question-bank/question-selection-ledger-batch-001.jsonl
.xiigen/wiki-domain/training/batch-001/wiki-qa-train-50.jsonl
.xiigen/wiki-domain/heldout/batch-001/wiki-heldout-50.jsonl
.xiigen/wiki-domain/training/batch-001/wiki-query-plan-ledger.jsonl
.xiigen/wiki-domain/training/batch-001/row-validation-ledger.jsonl
```

First batch distribution:

| Count | Question seed type |
|---:|---|
| 10 | `entity_impact_question` |
| 8 | `flow_trace_question` |
| 6 | `change_location_question` |
| 6 | `impact_analysis_question` |
| 6 | `caller/callee/dependency/value_flow` |
| 5 | `architectural/system_boundary/feature` |
| 4 | `missing_evidence/evidence/partial_trace` |
| 5 | `wrong_project/stale_version/wrong_level` |

This table is the executable mapping for batch-001. It is the only distribution
to use for the first question batch. Total must remain exactly 50 rows.

## Trainable Question Bank

Do not select questions by hand from a flat file. Use the question bank flow:

```text
accepted documentation + evidence index + previous wiki Q/A failures
  -> TrainableQuestionBankFlow
  -> question candidates in project-wiki-questions
  -> ProjectWikiQueryPlanningFlow expected plan candidates
  -> utility scoring
  -> 50 train seed export + 50 held-out seed export
```

Each question bank row must include `question_id`, `question_status`,
`utility_score`, `selection_policy_version`, `times_used_for_training`,
`times_used_for_heldout`, and `last_eval_result`.

Selection rules:

- train and held-out questions must be disjoint;
- under-covered symbols and edges get higher utility;
- previous held-out failure types get higher utility in the next batch;
- ambiguous questions are demoted or repaired;
- missing-evidence questions must stay in the bank as first-class rows, not as
  afterthoughts.

## Questions Are Not Documentation Negatives

This session must not call or emulate the Phase 1
`NegativeDtoDocumentationOrchestrator`, its verifier, or its validation ledgers.

Question rows do not contain rejected answers. They contain expected answers or
expected abstention behavior. They also contain an expected query plan that
must pass before the answer is evaluated.

Each Q/A train row must include:

- question id;
- project id;
- documentation version;
- question type;
- question text;
- expected query plan intent;
- expected query plan steps;
- expected sufficiency checks;
- expected missing-evidence policy;
- expected answer or expected abstention;
- expected evidence ids;
- expected answer type;
- source commit.

The Phase 1 negative DTO documentation unit is a separate path and must not be
used to generate question rows.

## Row Gate

- 50 Q/A train rows;
- 50 held-out rows;
- every row has an expected `ProjectWikiQueryPlan`;
- entity-impact and workflow rows have multi-step plans, not one lookup;
- all evidence ids exist in `evidence-index.jsonl`;
- no rejected answer fields in question rows;
- no stale documentation accepted answer;
- no cross-project accepted answer;
- missing-evidence questions choose abstention;
- question type distribution is recorded.

## Stop Gate

Stop after structural validation passes.

Do not train the adapter in this session.
