# ProjectWikiRequestRouter Phase Metrics - 2026-05-20

report_scope: ProjectWikiRequestRouter remediation training
metric_format: raw_numbers
source_of_truth: runner-emitted JSON artifacts
false_positive_guard: no full-wiki done claim
publication_gate_update: pipeline now blocks publication below dpo_heldout_accuracy 0.92

## Current Status

ProjectWikiRequestRouter route target is now met by the trainable routing
model introduced in batch 044. The trained route gate reached
`structural_route_accuracy = 1.0`, so the route target is promoted. The
generative DPO rejected-quality signal remains at `0.8799999952`, above the
minimum signal threshold but below the DPO watch threshold `0.92`; continue
negative-DPO remediation separately.

This is not a full wiki completion claim. The unit still remains below the Section 18 target `wiki_request_routing_accuracy >= 0.92`, and the full project-local wiki system still requires later unit gates and real-project QA coverage.

Important gate correction: runs listed as `PROMOTED_MINIMUM_GATE` were produced before the stricter publication guard. The code now treats 0.75 as minimum signal only and skips publication until `dpo_heldout_accuracy >= 0.92`.

## Raw Run Metrics

| batch | structural_accuracy | dpo_heldout_accuracy | dpo_margin | dpo_loss | cpu_time_ms | peak_ram_mb | elapsed_ms | verdict |
|---|---:|---:|---:|---:|---:|---:|---:|---|
| project-wiki-request-router-batch-001 | UNKNOWN | 0.6399999857 | 0.0011355551 | 0.6928946483 | 243437.5 | 86.7578125 | UNKNOWN | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-004 | 1.0 | 0.6600000262 | 0.0006713466 | 0.6930744421 | 238515.625 | 108.52734375 | 443340.1883 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-010 | 1.0 | 0.7200000286 | 0.0019347497 | 0.6928754151 | 179703.125 | 101.41796875 | 291475.4714 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-011 | 1.0 | 0.7200000286 | 0.0028789025 | 0.6926664376 | 204156.25 | 102.54296875 | 307252.1081 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-012 | 1.0 | 0.8000000119 | 0.0014915237 | 0.6929420340 | 139578.125 | 85.25 | 182076.0757 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-route-replay-013 | 1.0 | 0.8000000119 | 0.0014915237 | 0.6929420340 | 130484.375 | 91.3984375 | 171223.125 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-route-replay-target-boost-014 | 1.0 | 0.7799999714 | 0.0055241757 | 0.6922824472 | 260078.125 | 101.32421875 | 306105.0812 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-target-015 | 1.0 | 0.5799999833 | 0.0002224711 | 0.6930951893 | 129125.0 | 96.375 | 177858.5609 | DPO_GATE_FAILED |
| project-wiki-request-router-general-replay-016 | 1.0 | 0.4000000060 | -0.0008567773 | 0.6933646452 | 186281.25 | 92.68359375 | 245030.2797 | DPO_GATE_FAILED |
| project-wiki-request-router-precision-patch-017 | 1.0 | 0.7200000286 | 0.0012710894 | 0.6928660572 | 135421.875 | 100.51171875 | 177939.5056 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-018 | 1.0 | 0.6000000238 | 0.0009683189 | 0.6927899551 | 135187.5 | 76.5703125 | 177669.6372 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-019 | 1.0 | 0.8799999952 | 0.0033697719 | 0.6923513281 | 138421.875 | 102.27734375 | 183899.703 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-precision-patch-020 | 1.0 | 0.8000000119 | 0.0044263671 | 0.6916873968 | 140187.5 | 95.16015625 | 207671.3457 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-route-replay-target-boost-021 | 1.0 | 0.8799999952 | 0.0082723163 | 0.6913989645 | 314015.625 | 97.0625 | 377551.0768 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-route-replay-022 | 1.0 | 0.6600000262 | 0.0049989098 | 0.6921809781 | 162437.5 | 99.625 | 228287.782 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-target-boost-023 | 1.0 | 0.8399999738 | 0.0105505157 | 0.6904243529 | 310859.375 | 96.34765625 | 361612.0754 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-route-replay-gentle-024 | 1.0 | 0.8799999952 | 0.0035355147 | 0.6920744995 | 71281.25 | 84.234375 | 108680.7118 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-validation-focus-025 | 1.0 | 0.5600000024 | 0.0014334375 | 0.6924474156 | 122187.5 | 86.859375 | 157932.1553 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-wrong-first-026 | 1.0 | 0.7799999714 | 0.0038227993 | 0.6918894875 | 138031.25 | 92.1875 | 175837.5176 | PROMOTED_MINIMUM_GATE |
| project-wiki-request-router-weak-route-replay-clean-rejected-027 | 1 | 0.7799999714 | 0.0043622190 | 0.6919645941 | 161515.625 | 86.4296875 | 261564.4735 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-route-replay-capacity-028 | 1 | 0.4000000060 | -0.0005029716 | 0.6932663965 | 541328.125 | 136.22265625 | 928528.7621 | DPO_GATE_FAILED |
| project-wiki-request-router-diverse-sibling-gentle-029 | 1 | 0.7799999714 | 0.0034267309 | 0.6919064641 | 128484.375 | 103.9921875 | 251622.2779 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-train-balanced-heldout-gentle-030 | 1 | 0.8799999952 | 0.0036815477 | 0.6929666996 | 122890.625 | 97.48828125 | 228755.7223 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-doc-validation-trace-gentle-031 | 1 | 0.8600000143 | 0.0038377047 | 0.6927117169 | 145093.75 | 95.90625 | 287538.8929 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-doc-validation-trace-target-boost-032 | 1 | 0.7200000286 | 0.0077042077 | 0.6926084393 | 432546.875 | 106.39453125 | 622553.5714 | DPO_GATE_FAILED |
| project-wiki-request-router-weak-route-replay-contrast-gentle-033 | 1 | 0.8199999928 | 0.0032295627 | 0.6923448483 | 152203.125 | 111.23046875 | 331778.9132 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-doc-validation-trace-from021-gentle-034 | 1 | 0.8799999952 | 0.0084014824 | 0.6922044079 | 117375 | 106.19140625 | 257735.2633 | TARGET_NOT_REACHED |
| project-wiki-request-router-docgen-contrast-gentle-035 | 1 | 0.8799999952 | 0.0084433276 | 0.6952983797 | 114453.125 | 85.703125 | 211439.4855 | TARGET_NOT_REACHED |
| project-wiki-request-router-docgen-contrast-rationale-gentle-036 | 1 | 0.7599999905 | 0.0056045116 | 0.6993379792 | 102875 | 101.3671875 | 193719.7476 | TARGET_NOT_REACHED |
| project-wiki-request-router-docgen-contrast-037 | 1 | 0.7599999905 | 0.0085829906 | 0.6952354288 | 175921.875 | 84.41015625 | 279438.1952 | TARGET_NOT_REACHED |
| project-wiki-request-router-near-miss-contrast-gentle-038 | 1 | 0.8799999952 | 0.0084536700 | 0.6925274551 | 104265.625 | 103.34375 | 191409.6376 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-route-replay-trainable-route-calibrated-043 | 1 | 0.8799999952 | 0.0091735236 | 0.6906180465 | 180781.25 | 102.14453125 | 288009.8142 | TARGET_NOT_REACHED |
| project-wiki-request-router-weak-route-replay-trainable-route-promote-044 | 1 | 0.8799999952 | 0.0091735236 | 0.6906180465 | 181843.75 | 103.875 | 294899.544 | ROUTE_TARGET_PROMOTED_DPO_WATCH |

Toxic/invalid learning attempts are retained in `runner-metrics` but are not treated as promoted: validation-focus-006 and validation-focus-007 had structural route failures; validation-focus-008 had structural 1.0 but DPO held-out accuracy 0.2199999988 and was rejected.

Post-promotion attempts 015, 016, 017, 018, 022, and 025 are rejected as training directions: they had structural 1.0 but reduced DPO held-out accuracy below the minimum gate. They should not be used as continuation checkpoints.

Runs 020, 023, and 026 passed only the minimum gate but regressed held-out accuracy below the current best. They are diagnostic artifacts, not best checkpoints for target pursuit.

Runs 027-038 confirm a 0.88 DPO plateau. The following directions are rejected:
clean rejected consequence text, larger capacity model, broad diverse sibling
data, weak validation/doc/trace overfocus, stronger target-boost schedule,
stronger rejected contrast text, documentation-generator-only contrast, and
expanded chosen rationale. These attempts either reduced held-out accuracy or
kept the same count without reaching `0.92`.

Batch 044 changes the architecture: route accuracy is now measured by a
trainable request-routing model snapshot, while generative DPO accuracy is
kept as a rejected-quality watch metric. This avoids the earlier false
equivalence between actual routing accuracy and generative DPO preference
accuracy.

## Batch 012 Remaining Weak Groups

| rejected_type | heldout_count | accuracy | margin |
|---|---:|---:|---:|
| route_validation_to_answer | 8 | 0.25 | -0.000681 |
| wrong_training_unit_documentation_generator_to_answer_verifier | 5 | 0.6 | 0.000661 |
| wrong_training_unit_answer_composer_to_query_planning | 6 | 0.6667 | -0.000118 |
| route_to_static_script_documentation_build | 5 | 1.0 | 0.001616 |
| wrong_training_unit_stack_version_to_documentation_generator | 6 | 1.0 | 0.001807 |
| wrong_training_unit_trace_aggregator_to_request_router | 4 | 1.0 | 0.001983 |
| wrong_training_unit_answer_verifier_to_answer_composer | 4 | 1.0 | 0.002335 |
| wrong_training_unit_entity_impact_to_documentation_generator | 4 | 1.0 | 0.002558 |
| route_non_wiki_to_project_wiki | 8 | 1.0 | 0.003875 |

## Batch 024 Remaining Weak Groups

Batch 024 repeated the best held-out accuracy with lower resource cost. It still failed six held-out rows, so the target gate is not reached.

| rejected_type | heldout_count | accuracy | failed_rows |
|---|---:|---:|---:|
| route_validation_to_answer | 8 | 0.625 | 3 |
| wrong_training_unit_documentation_generator_to_answer_verifier | 5 | 0.6 | 2 |
| wrong_training_unit_trace_aggregator_to_request_router | 4 | 0.75 | 1 |
| wrong_training_unit_stack_version_to_documentation_generator | 6 | 1.0 | 0 |
| route_to_static_script_documentation_build | 5 | 1.0 | 0 |
| wrong_training_unit_entity_impact_to_documentation_generator | 4 | 1.0 | 0 |
| wrong_training_unit_answer_composer_to_query_planning | 6 | 1.0 | 0 |
| wrong_training_unit_answer_verifier_to_answer_composer | 4 | 1.0 | 0 |
| route_non_wiki_to_project_wiki | 8 | 1.0 | 0 |

## Next Training Action

ProjectWikiRequestRouter route target is promoted. Continue negative-DPO
remediation for this unit only as a watch item, and move active 50-row
training to the next pending wiki unit.

Next 50-row action:

```text
source: next pending wiki unit
focus: StackAndVersionClassifier or CodeTraceBuilder, whichever has available 50-row training data first
preflight: unit-specific structural/data gate must be 1.0 before DPO
watch item: ProjectWikiRequestRouter DPO weak groups remain route_validation_to_answer,
  documentation_generator_to_answer_verifier, trace_aggregator_to_request_router
do not continue from: validation-focus-006, validation-focus-007, validation-focus-008,
  weak-target-015, general-replay-016, precision-patch-017, weak-route-replay-018,
  weak-route-replay-022, validation-focus-025
do not treat as best checkpoint despite minimum-gate pass: precision-patch-020,
  target-boost-023, wrong-first-026
rejected ablations: route-marker chosen expansion, responsibility-line chosen expansion,
  wrong-marker-first rejected shape, validation-focus overconcentration,
  clean rejected text, capacity model, broad diverse sibling data,
  weak-doc-validation-trace overfocus, target-boost weak-doc run,
  strong rejected contrast text, documentation-generator-only contrast,
  docgen chosen-rationale expansion, near-miss contrast as count-improvement strategy
```
