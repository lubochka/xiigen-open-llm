# Code Documentation Examination Method
# Version: 1.0 - 2026-05-11

---

## Purpose

This method checks whether a documentation pair, batch, or model output is
accurate enough to enter training or promote to the next call-tree level.

---

## Examination Questions

For each chosen documentation output, answer:

1. Is the framework stack correct?
2. Are runtime family, target framework, C# language version, project style,
   and frontend framework version correct?
3. Is the artifact kind correct?
4. Does the documentation match the symbol level requested?
5. Are inputs and outputs documented?
6. Are side effects documented?
7. Are error paths documented?
8. Are call edges preserved?
9. Are callee summaries used only when supplied?
10. For D3/D4, did documentation start from inner methods and propagate
    accepted callee summaries upward?
11. Does the chosen output separate local behavior from delegated callee
    behavior?
12. Is any behavior invented?
13. Does the documentation over-modernize legacy code?

---

## Scoring

Use these per-row scores:

- `factual_accuracy`: 0.0 to 1.0
- `member_coverage`: 0.0 to 1.0
- `side_effect_recall`: 0.0 to 1.0
- `error_path_recall`: 0.0 to 1.0
- `call_edge_preservation`: 0.0 to 1.0
- `framework_lifecycle_accuracy`: 0.0 to 1.0
- `version_routing_accuracy`: 0.0 to 1.0
- `bottom_up_order_accuracy`: 0.0 to 1.0
- `callee_summary_transit_accuracy`: 0.0 to 1.0
- `local_vs_delegated_accuracy`: 0.0 to 1.0

Hard fail flags:

- `invented_behavior`
- `wrong_framework`
- `wrong_version`
- `missing_callee_summary`
- `caller_documented_before_callee`
- `missing_license_proof`
- `wrong_documentation_level`
- `chosen_equals_rejected`
- `missing_wrong_annotation`

---

## Batch Acceptance

A 50-pair batch is accepted only if:

- DPO reward margin > 0
- DPO accuracy does not regress against the current baseline
- held-out factual accuracy >= level threshold
- invented behavior count <= allowed threshold
- no high-severity source/license failure
- version routing accuracy is reported
- D3/D4 call-transit gates pass
- look-like report is written

---

## Level Thresholds

- D1 factual accuracy >= 0.80
- D2 factual accuracy >= 0.80
- D3 factual accuracy >= 0.78
- D4 factual accuracy >= 0.75

D3 and D4 may accept slightly lower factual accuracy only when call-edge
preservation and no-invention gates pass.
