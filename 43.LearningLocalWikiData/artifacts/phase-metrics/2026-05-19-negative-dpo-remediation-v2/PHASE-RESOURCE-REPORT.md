# Negative DPO Generator Remediation Report

Date: 2026-05-19
Scope: `NegativeDpoGenerator` only.
Source of truth: runner-emitted JSON artifacts under `runner-metrics/2026-05-19-negative-dpo-generator-sequence-047-rich-evidence-shape`.

Important boundary: this is not a claim that the whole project-local wiki is done. It is a persisted training and promotion report for the `NegativeDpoGenerator` unit.

## Verdict

Unit status: PROMOTED.
Whole wiki status: TRAINING IN PROGRESS.
False-positive guard: do not use this report as proof that documentation generation, wiki query planning, question-bank training, or project-local answer quality is complete.

Promotion artifact:
`C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-negative-dpo-generator-sequence-047-rich-evidence-shape\negative-dpo-generator-unit\negative-dpo-generator-20260519153042\generated_capability_manifest.json`

## Phase Metrics

| Phase | Batch | Purpose | Held-out accuracy | Held-out margin | Final accuracy | Final margin | Final loss | Avg CPU % | CPU ms | Peak RAM MB | Elapsed ms | Result |
|---|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| 1 | `negative-dpo-generator-mixed-rich-evidence-047` | Rich evidence baseline after fixing shallow return/callee/caller rows | 0.940000 | 0.002173159 | 0.944000 | 0.001149201 | 0.692572880 | 12.654437 | 123625.000 | 78.000000 | 163233.963 | DATA_NEEDED: `missing_parameters` 0.70 |
| 2 | `negative-dpo-generator-focus-missing-parameters-048` | Focused parameter remediation from batch047 checkpoint | 0.900000 | 0.002566752 | 0.860000 | 0.002079762 | 0.692108274 | 12.646528 | 70765.625 | 91.761719 | 112094.829 | REJECTED as checkpoint: parameter accuracy stayed 0.70 |
| 3 | code/data-shape remediation | Added chosen parameter verification marker and diversified parameter-defect units | n/a | n/a | n/a | n/a | n/a | n/a | n/a | n/a | n/a | Tests passed: 7/7 targeted |
| 4 | `negative-dpo-generator-mixed-parameter-variety-049` | Mixed run with parameter-variety shape from batch047 checkpoint | 0.980000 | 0.003598904 | 1.000000 | 0.003534670 | 0.691381645 | 12.668636 | 77203.125 | 86.148438 | 113648.773 | PASSED diagnostic, not publishable by batch type |
| 5 | `negative-dpo-generator-mixed-confirmation-050` | Confirmation run from batch049 checkpoint | 1.000000 | 0.003388823 | 0.940000 | 0.003589312 | 0.691354650 | 12.788825 | 25125.000 | 79.203125 | 62978.234 | PASSED confirmation, not publishable by batch type |
| 6 | `negative-dpo-generator-promotion-confirmation-051` | Explicit promotion confirmation from batch050 checkpoint | 0.980000 | 0.003840186 | 0.980000 | 0.003639221 | 0.691329753 | 12.734612 | 25296.875 | 81.199219 | 62500.289 | PROMOTED and registered |

## Per-Type Held-Out Accuracy

| Batch | missing_callee_transit | missing_caller_context | missing_location | missing_parameters | missing_return_contract |
|---|---:|---:|---:|---:|---:|
| 047 | 1.00 | 1.00 | 1.00 | 0.70 | 1.00 |
| 048 | 1.00 | 0.80 | 1.00 | 0.70 | 1.00 |
| 049 | 1.00 | 1.00 | 0.90 | 1.00 | 1.00 |
| 050 | 1.00 | 1.00 | 1.00 | 1.00 | 1.00 |
| 051 | 1.00 | 1.00 | 0.90 | 1.00 | 1.00 |

## What Improved

The previous weak type was `missing_parameters`.

Observed failure:
focused repetition of the same `RequestMapper.MapRequest` parameter shape did not improve the unit. Batch048 stayed at 0.70 for `missing_parameters` and reduced overall held-out accuracy to 0.90.

Fix:
the training shape now teaches parameter defects across multiple parameterized code roles: request mapping, entity update mapping, search query building, report filter creation, and workflow command creation. The chosen side also includes an explicit `parameter_verification_status: actual_input_values_present_in_code_trace` marker.

Measured result:
`missing_parameters` improved from 0.70 in batch047/batch048 to 1.00 in batch049, batch050, and batch051.

## Remaining Non-Done Items

This report does not close:

- `DocumentationGenerator` training and validation.
- `WikiQueryPlanningFlow` training and validation.
- `WikiContextRoutingUnit` validation on real project questions.
- Project-local wiki answer quality across all five .NET projects.
- Question-bank training as Phase 2.

