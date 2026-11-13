# CodeTraceBuilder Phase Metrics - 2026-05-20

report_scope: CodeTraceBuilder training and negative-DPO remediation
metric_format: raw_numbers
source_of_truth: runner-emitted JSON artifacts
false_positive_guard: structural trace recall is not enough for publication when DPO is below 0.75

## Current Status

CodeTraceBuilder structural trace gate passes: method, call-edge, and value-flow
recall are all `1.0` on the current synthetic generic batch. The minimum DPO
publication gate is now passed by
`code-trace-builder-ultra-compact-caller-abstract-balanced-combo-017`.

This is not a full wiki completion claim. The balanced DPO gate passed, but
`wrong_caller` remains a known weak rejected type and must stay in the next
remediation backlog before any "excellent" project-wiki claim.

Current promoted minimum-signal DPO direction:
`code-trace-builder-ultra-compact-caller-abstract-balanced-combo-017`, with
`dpo_heldout_accuracy = 0.8000000119` and positive margin.

## Raw Run Metrics

| batch | method_recall | call_edge_recall | value_flow_recall | dpo_heldout_accuracy | dpo_margin | dpo_loss | cpu_time_ms | peak_ram_mb | elapsed_ms | verdict |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|
| code-trace-builder-batch-001 | 1 | 1 | 1 | 0.2199999988 | -0.0013481092 | 0.6935808039 | 173859.375 | 88.546875 | 279825.4913 | DPO_GATE_FAILED |
| code-trace-builder-batch-002-dpo-remediation | 1 | 1 | 1 | 0.2599999905 | -0.0006838475 | 0.6933474970 | 178828.125 | 95.4140625 | 284113.8847 | DPO_GATE_FAILED |
| code-trace-builder-target-boost-003 | 1 | 1 | 1 | 0.2800000012 | -0.0029600521 | 0.6939614397 | 364656.25 | 105.2265625 | 512774.8243 | DPO_GATE_FAILED |
| code-trace-builder-compact-004 | 1 | 1 | 1 | 0.2800000012 | -0.0011931781 | 0.6933942676 | 177937.5 | 103.46484375 | 277511.3542 | DPO_GATE_FAILED |
| code-trace-builder-compact-continuation-005 | 1 | 1 | 1 | 0.3000000119 | -0.0019166260 | 0.6938138115 | 185937.5 | 85.49609375 | 299581.9059 | DPO_GATE_FAILED |
| code-trace-builder-compact-wrong-first-006 | 1 | 1 | 1 | 0.1800000072 | -0.0022696285 | 0.6936852300 | 174656.25 | 94.0625 | 273596.5437 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-007 | 1 | 1 | 1 | 0.5600000024 | 0.0011077708 | 0.6926765621 | 176703.125 | 105.8046875 | 284478.0901 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-continuation-008 | 1 | 1 | 1 | 0.5600000024 | 0.0027112060 | 0.6916625035 | 178390.625 | 105.0234375 | 281928.2838 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-wrong-caller-remediation-009 | 1 | 1 | 1 | 0.3600000143 | -0.0026093647 | 0.6920236814 | 175093.75 | 89.50390625 | 284995.9682 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-caller-status-010 | 1 | 1 | 1 | 0.3000000119 | -0.0026385216 | 0.6937531161 | 180562.5 | 89.03125 | 291509.9477 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-varied-caller-011 | 1 | 1 | 1 | 0.5600000024 | 0.0010632152 | 0.6926654351 | 173062.5 | 88.34765625 | 279257.5965 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-varied-caller-continuation-012 | 1 | 1 | 1 | 0.5600000024 | 0.0024712267 | 0.6915935349 | 167578.125 | 105.47265625 | 261689.7059 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-wrong-caller-chosen-013 | 1 | 1 | 1 | 0.5400000215 | 0.0003534651 | 0.6929542637 | 175765.625 | 104.35546875 | 279961.3589 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-caller-abstract-014 | 1 | 1 | 1 | 0.6800000072 | 0.0025688317 | 0.6924693644 | 142609.375 | 87.78125 | 194419.6525 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-caller-abstract-continuation-015 | 1 | 1 | 1 | 0.6800000072 | 0.0058969152 | 0.6909172809 | 118515.625 | 104.8828125 | 158282.1481 | DPO_GATE_FAILED |
| code-trace-builder-ultra-compact-caller-abstract-wrong-caller-remediation-016 | 1 | 1 | 1 | 0.7400000095 | 0.0109482752 | 0.6888683891 | 139062.5 | 102.79296875 | 179951.5543 | FOCUSED_GATE_NEEDS_BALANCED_COMBO |
| code-trace-builder-ultra-compact-caller-abstract-balanced-combo-017 | 1 | 1 | 1 | 0.8000000119 | 0.0142934117 | 0.6873843431 | 128562.5 | 89.61328125 | 177943.4781 | DPO_GATE_PASSED_WITH_KNOWN_DEBT |

## Weak Rejected Types After Promotion Run

From `code-trace-builder-ultra-compact-caller-abstract-balanced-combo-017`:

| rejected_type | heldout_count | accuracy | margin |
|---|---:|---:|---:|
| wrong_caller | 10 | 0.4 | -0.002662 |
| incomplete_trace | 10 | 0.8 | 0.009843 |
| missing_callee | 10 | 0.9 | 0.018677 |
| invented_callee | 10 | 0.9 | 0.030102 |
| missing_value_flow | 10 | 1.0 | 0.015508 |

## Rejected Directions

```text
full trace output: too long/noisy for DPO metric, held-out accuracy 0.22.
target-boost epochs: increased cost and worsened margin.
compact output: slight improvement only, still below 0.75.
wrong-first rejected marker: regressed to 0.18.
wrong-caller-only focus: regressed to 0.36.
caller-status chosen expansion: regressed to 0.30.
varied invented caller labels: did not improve count accuracy beyond 0.56.
wrong-caller chosen expansion: regressed to 0.54.
```

## Accepted Direction

```text
ultra-compact chosen + caller-abstract rejected improved DPO accuracy from
0.56 to 0.68.
focused wrong-caller remediation improved focused held-out accuracy to 0.74 but
was not accepted alone because the held-out split was focused, not balanced.
balanced combo 017 passed the minimum DPO gate at 0.8000000119 and published
the CodeTraceBuilder unit.
```

## Next Training Action

```text
source: advanced project-local wiki units that consume CodeTraceBuilder output.
current CodeTraceBuilder status: promoted minimum signal with known wrong_caller
debt.
next focus: EntityImpactMapper or WikiContextRoutingUnit, with CodeTraceBuilder
wrong_caller debt kept in the ten-batch remediation backlog.
```
