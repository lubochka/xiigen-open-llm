# ProjectAnswerVerifier Phase Metrics

Date: 2026-05-21
Unit: ProjectAnswerVerifier / project-answer-verifier-unit
Scope: DPO remediation for verifier-result preference learning
Status: HELD_WITH_WEAK_TYPES, NOT_PROMOTED

This is not a project wiki completion report and not a promoted verifier report.
The deterministic verifier gate passes, but the DPO preference signal is not
stable enough across all rejected types.

## Why This Phase Was Needed

The original verifier pipeline only reported deterministic pass/fail metrics.
That created a false-positive risk: the verifier could pass structural logic
while DPO training learned the wrong verifier result. The pipeline now persists
runner-emitted DPO loss, accuracy, reward margin, CPU, RAM, elapsed time, and
per-rejected-type diagnostics.

## Run Summary

| Batch | Shape | Init | Held-out accuracy | Held-out margin | Final accuracy | Final margin | CPU ms | Peak RAM MB | Elapsed ms | Verdict |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---|
| 001 | plain verifier result | fresh | 0.3000000119 | -0.0025726827 | 0.4160000002 | -0.0009671982 | 171937.500 | 89.58203125 | 224641.0542 | REJECTED |
| 002 | signal | fresh | 0.7599999905 | 0.0034792181 | 0.7800000000 | 0.0018520420 | 160000.000 | 110.58593750 | 217929.3591 | HELD_WEAK_INCOMPLETE |
| 003 | signal + explicit incomplete rejected | fresh | 0.7400000095 | 0.0027031249 | 0.7800000000 | 0.0014485983 | 123140.625 | 103.03125000 | 158948.3806 | REJECTED |
| 004 | signal + declared abstention tokens | fresh | 1.0000000000 | 0.0036855692 | 0.9360000002 | 0.0018303000 | 116453.125 | 102.64453125 | 150877.2926 | HELD_COVERAGE_CLEAN_MARGIN_WEAK |
| 005 | signal continuation | batch-004 | 1.0000000000 | 0.0073112105 | 1.0000000000 | 0.0054894259 | 113015.625 | 89.28515625 | 145770.3139 | HELD_MARGIN_WEAK |
| 006 | signal continuation + extra wrong-project tokens | batch-005 | 0.8000000119 | 0.0097668935 | 0.8200000003 | 0.0085092654 | 115328.125 | 88.95703125 | 149417.1111 | REJECTED_WRONG_PROJECT_REGRESSION |
| 007 | signal corrected wrong-project rejected | fresh | 0.9599999785 | 0.0034876028 | 0.8240000007 | 0.0016308924 | 115687.500 | 93.19531250 | 150322.9771 | HELD_WRONG_PROJECT_WEAK |
| 008 | signal continuation | batch-007 | 0.9399999976 | 0.0068937503 | 0.8760000011 | 0.0049618611 | 119906.250 | 102.56250000 | 153033.3019 | HELD_WRONG_PROJECT_WEAK |
| 009 | focused weak-type mix | batch-008 | 0.2399999946 | 0.0011309490 | 0.4200000009 | 0.0015094645 | 200671.875 | 105.39453125 | 325163.4650 | REJECTED_FOCUSED_MIX_COLLAPSE |
| 010 | balanced, empty evidence index for abstention | fresh | 0.9599999785 | 0.0034876028 | 0.8240000007 | 0.0016308924 | 218750.000 | 91.96875000 | 352572.8113 | HELD_WRONG_PROJECT_WEAK |
| 011 | long continuation | batch-010 | 0.9399999976 | 0.0100968694 | 0.8720000030 | 0.0065595197 | 255562.500 | 102.69921875 | 295269.7232 | HELD_WRONG_PROJECT_WEAK |
| 012 | lr 0.001 continuation | batch-010 | 0.9399999976 | 0.0102076819 | 0.8640000015 | 0.0065377275 | 125984.375 | 101.39062500 | 165717.2381 | HELD_WRONG_PROJECT_WEAK |

## Best Non-Promoted Candidates

Clean coverage candidate: project-answer-verifier-signal-batch-004

- held-out accuracy: 1.0000000000
- held-out margin: 0.0036855692
- all rejected types accuracy: 1.0000
- weakness: margin too weak for promotion

Higher margin candidate: project-answer-verifier-signal-lr001-batch-012

- held-out accuracy: 0.9399999976
- held-out margin: 0.0102076819
- weakness: accepts_wrong_project accuracy 0.8000; rejects_valid_answer accuracy 0.9000

## Current Weak Types

From batch-012:

| Rejected type | Count | Accuracy | Average margin |
|---|---:|---:|---:|
| accepts_invalid_citation | 10 | 1.0000 | 0.024613 |
| accepts_missing_evidence | 10 | 1.0000 | 0.020000 |
| accepts_wrong_project | 10 | 0.8000 | 0.001556 |
| incomplete_verifier_result | 10 | 1.0000 | 0.000852 |
| rejects_valid_answer | 10 | 0.9000 | 0.004018 |

## Remediation Decisions

Do not continue from the focused weak-type checkpoint:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-project-answer-verifier-signal-focused-batch-009
```

It collapsed held-out accuracy to 0.2399999946.

Next useful work for this unit:

- split wrong-project verification from missing-evidence abstention into
  separate sub-capability probes;
- add external verifier examples only if license-clean sources provide
  citation/project-boundary validation examples;
- do not claim promotion until wrong-project and valid-answer rejected types
  both reach 1.0000 and held-out margin remains positive with no focused-mix
  collapse.

## Code Changes Supporting Real Metrics

- `ProjectAnswerVerifierTrainingPipeline` now accepts `ProjectWikiDpoMetricRunner`.
- `ProjectAnswerVerifierTrainingPipelineResult` persists `DpoMetrics`.
- `ProjectWikiTrainingOrchestrator` maps verifier DPO loss/accuracy/margin and resource metrics.
- The temp `ProjectWikiMetricRunner` supports the `answer-verifier` unit.

## Status

ProjectAnswerVerifier remains in remediation. It is above the minimum 0.75
global accuracy in several runs, but it is not promoted because weak rejected
types are not stable.
