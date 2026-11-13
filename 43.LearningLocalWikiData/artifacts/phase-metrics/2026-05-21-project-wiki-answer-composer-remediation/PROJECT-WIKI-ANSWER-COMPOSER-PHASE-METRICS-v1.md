# ProjectWikiAnswerComposer Phase Metrics

Date: 2026-05-21
Unit: WikiAnswerComposer / project-wiki-answer-composer-unit
Scope: DPO remediation for project-local wiki answer composition
Status: PROMOTED_TARGET_SIGNAL_FOR_UNIT, NOT_FULL_WIKI_DONE

This is not a full local wiki completion report. It records the real runner
metrics for the answer-composition unit only. The project-local wiki still
requires complete project validation, WikiQueryTrace/WikiUnitTrace coverage,
and advanced prompt QA before any "done" claim is allowed.

## Goal

Train the unit to prefer answers that:

- stay inside the requested project;
- include accepted evidence ids;
- include location, parameters, returns, callers, and callees;
- expose value flow and impact path for developer reasoning questions;
- abstain when evidence is missing;
- reject hallucinated, cross-project, partial, or single-line answers.

## Run Summary

| Batch | Shape | Init | Held-out accuracy | Held-out margin | Final accuracy | Final margin | Loss | CPU ms | Peak RAM MB | Elapsed ms | Verdict |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---|
| 001 | full text | fresh | 0.4600000083 | -0.0004326591 | 0.3960000098 | -0.0003835651 | unknown | 161640.625 | 92.996 | 217381.8325 | REJECTED |
| 002 | compact | fresh | 0.2399999946 | -0.0014098112 | 0.3440000075 | unknown | unknown | 121609.375 | 100.809 | 158236.6343 | REJECTED |
| 003 | signal, token evidence | fresh | 1.0000000000 | 0.0067358031 | 0.9040000045 | 0.0029847386 | 0.6916569245 | 117312.500 | 101.719 | 166846.8484 | SIGNAL_FOUND_BUT_GROUNDING_CHECK_WAS_OLD |
| 004 | signal, raw evidence | fresh | 0.7400000095 | 0.0021553575 | 0.6880000070 | 0.0010438881 | unknown | 146484.375 | 101.551 | 206817.9242 | REJECTED |
| 005 | signal, raw evidence | batch-004 | 0.7400000095 | 0.0043114112 | 0.7200000066 | 0.0033633394 | unknown | 145218.750 | 98.109 | 189127.2604 | HELD_BELOW_0_75 |
| 006 | weak-type focused | batch-005 | 0.5199999809 | 0.0012082556 | 0.6320000154 | 0.0022778221 | unknown | 129078.125 | 104.426 | 166445.0185 | REJECTED |
| 007 | signal, token evidence | fresh | 1.0000000000 | 0.0067358031 | 0.9040000045 | 0.0029847386 | 0.6916569245 | 126015.625 | 102.17578125 | 165849.1741 | ACCEPTED_SIGNAL |
| 008 | signal, token evidence | batch-007 | 1.0000000000 | 0.0134758214 | 0.9320000041 | 0.0092707741 | 0.6885276306 | 133453.125 | 100.07812500 | 218921.4857 | ACCEPTED_IMPROVED |
| 009 | signal, token evidence | batch-008 | 1.0000000000 | 0.0201731604 | 0.9320000041 | 0.0155262793 | 0.6854275072 | 119968.750 | 101.33593750 | 158979.7340 | ACCEPTED_STRONG_MARGIN_1 |
| 010 | signal, token evidence | batch-009 | 1.0000000000 | 0.0268192180 | 0.9280000043 | 0.0217067701 | 0.6823781979 | 121984.375 | 102.17187500 | 156705.7345 | ACCEPTED_STRONG_MARGIN_2 |

## Best Run

Batch: project-wiki-answer-composer-signal-token-cont-batch-010
Held-out accuracy: 1.0000000000
Held-out reward margin: 0.026819217950105667
Final accuracy: 0.9280000042915344
Final reward margin: 0.02170677011832595
Final loss: 0.6823781979084015
Average CPU percent: 12.574713877379457
Total CPU time ms: 121984.375
Peak working set MB: 102.171875
Total elapsed ms: 156705.7345

Checkpoint:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-project-wiki-answer-composer-signal-token-cont-batch-010\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-cont-batch-010-project-wiki-answer-composer-unit-20260521082612\checkpoints\policy_final.bin
```

Summary artifact:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-project-wiki-answer-composer-signal-token-cont-batch-010\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-cont-batch-010-project-wiki-answer-composer-unit-20260521082612\project-wiki-dpo-metric-summary.json
```

## Best Run Held-out By Rejected Type

| Rejected type | Count | Accuracy | Average margin |
|---|---:|---:|---:|
| hallucinated_behavior | 5 | 1.0000 | 0.011788 |
| invented_or_cross_project_evidence | 5 | 1.0000 | 0.020697 |
| missing_callers_and_callees | 10 | 1.0000 | 0.039263 |
| missing_evidence_id | 10 | 1.0000 | 0.021560 |
| missing_parameters_and_returns | 10 | 1.0000 | 0.034072 |
| single_line_summary_only | 5 | 1.0000 | 0.023510 |
| wrong_project_evidence | 5 | 1.0000 | 0.022409 |

## Training Batch Gate

Rows: 50
Wrong annotation rate: 1.0000000000
Evidence grounding rate: 1.0000000000
Warnings: 0

The successful answer shape is marker-based and evidence-token grounded:

```text
GOOD_PROJECT_WIKI_ANSWER
PROJECT_ID_<project>
EVIDENCE_ID_<token-safe evidence id>
LOCAL_EVIDENCE_PRESENT
LOCATION_PRESENT
PARAMETERS_PRESENT
RETURNS_PRESENT
CALLERS_PRESENT
CALLEES_PRESENT
PROJECT_BOUNDARY_SAME_PROJECT
VALUE_FLOW_PRESENT / IMPACT_PATH_PRESENT
ANSWER_PERMISSION_ALLOWED
```

The failed raw-evidence variant shows that adding the raw evidence id directly
to the chosen side lowered the DPO signal. Grounding must be checked by accepting
the token-safe evidence id as equivalent to the raw evidence id, not by putting
both into the training answer.

## Negative DPO Findings

Good rejected sides must still be strong training examples. The best run used
specific rejected-side markers plus specific `[WRONG: ...]` consequences:

- missing_evidence_id: evidence omitted, answer becomes unverifiable;
- missing_callers_and_callees: developer impact analysis becomes incomplete;
- missing_parameters_and_returns: input/output reasoning is impossible;
- single_line_summary_only: not actionable project documentation;
- hallucinated_behavior: corrupts the local wiki;
- wrong_project_evidence: violates project-local boundary;
- invented_or_cross_project_evidence: answers without accepted local evidence.

Weak-type focused data in batch-006 lowered accuracy to 0.5199999809. Do not
continue from that checkpoint.

## Status

Unit-level target signal is promoted for WikiAnswerComposer.

Not done:

- full local wiki coverage is not certified;
- project-specific QA with advanced prompts is not certified here;
- WikiQueryTrace and WikiUnitTrace gates are not certified here;
- this report does not promote a complete project wiki model.
