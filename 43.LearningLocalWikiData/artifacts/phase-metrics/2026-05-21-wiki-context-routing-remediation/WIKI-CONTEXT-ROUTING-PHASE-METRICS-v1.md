# WikiContextRoutingUnit Phase Metrics

date: 2026-05-21
unit: WikiContextRoutingUnit
scope: 10-run remediation cycle
status: TRAINING_IN_PROGRESS
done_claim_allowed: false
promotion_claim_allowed: false

## Goal

Train the unit that decides which wiki evidence route to use after the query-planning flow:

- exact symbol/location questions should use Mode 3 lexical evidence;
- multi-hop developer reasoning should use Mode 2 semantic/hybrid retrieval;
- missing evidence must abstain before answer composition;
- wrong project evidence must be rejected;
- stale documentation must be rejected;
- route selection must not be accepted without pre-checks.

Protocol target from `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`:

```text
context_route_accuracy >= 0.90
wrong_project_context_rate == 0
entity_impact_route_accuracy >= 0.85
```

This cycle reached minimum signal but not target.

## Raw Run Metrics

| run | batch | heldout_accuracy | heldout_margin | dpo_loss | cpu_time_ms | peak_ram_mb | elapsed_ms | status |
|---:|---|---:|---:|---:|---:|---:|---:|---|
| 001 | batch-001 | 0.380000 | -0.000529 | 0.693218 | 132203.125 | 83.324 | 185414.548 | rejected |
| 002 | compact-002 | 0.660000 | 0.002354 | 0.692446 | 140281.250 | 89.379 | 182353.408 | rejected |
| 003 | compact-continuation-003 | 0.620000 | 0.004529 | 0.691098 | 112656.250 | 105.859 | 145586.256 | rejected |
| 004 | compact-boundary-strong-wrong-project-remediation-004 | 1.000000 | 0.006211 | 0.690479 | 140406.250 | 102.824 | 187649.958 | focused-pass |
| 005 | compact-boundary-strong-balanced-combo-005 | 0.740000 | 0.008407 | 0.689247 | 146265.625 | 105.090 | 183215.437 | rejected |
| 006 | compact-boundary-direct-strong-always-semantic-remediation-006 | 0.900000 | 0.013040 | 0.687223 | 125328.125 | 103.238 | 162422.540 | focused-pass |
| 007 | compact-boundary-direct-strong-balanced-combo-007 | 0.800000 | 0.016654 | 0.684965 | 141359.375 | 104.402 | 182741.070 | minimum-signal-pass |
| 008 | compact-boundary-direct-precheck-strong-no-prechecks-remediation-008 | 0.600000 | 0.006707 | 0.687525 | 122250.000 | 104.422 | 161495.644 | rejected |
| 009 | compact-boundary-direct-precheck-strong-no-prechecks-remediation-009 | 0.400000 | -0.010184 | 0.694356 | 139343.750 | 106.102 | 188936.265 | rejected |
| 010 | compact-boundary-precheck-strong-no-prechecks-remediation-010 | 0.400000 | -0.008936 | 0.694791 | 141406.250 | 105.574 | 189706.674 | rejected |

## Aggregate Cost

run_count: 10
average_heldout_accuracy: 0.650000
best_heldout_accuracy: 1.000000
best_balanced_heldout_accuracy: 0.800000
average_heldout_margin: 0.003825
average_cpu_time_ms: 134150.000
average_peak_ram_mb: 101.021
average_elapsed_ms: 176952.180
average_cpu_time_ms_per_train_row: 2683.000
average_elapsed_ms_per_train_row: 3539.044
average_cpu_time_ms_per_total_pair: 1341.500
average_elapsed_ms_per_total_pair: 1769.522

## Latest Balanced Per-Type Held-Out Metrics

Source: `compact-boundary-direct-strong-balanced-combo-007`.

| rejected_type | heldout_count | accuracy | avg_margin | interpretation |
|---|---:|---:|---:|---|
| no_pre_checks | 5 | 0.6000 | 0.007634 | blocker |
| answer_instead_of_abstain | 5 | 0.6000 | 0.007837 | blocker |
| ignore_stale_version | 5 | 0.8000 | 0.008382 | below final target |
| ignore_wrong_project | 5 | 0.8000 | 0.010986 | improved from 0.0, still below final target |
| always_bm25 | 20 | 0.8500 | 0.025260 | acceptable minimum, below 0.90 target |
| always_semantic | 10 | 0.9000 | 0.015331 | target met |

## What Improved

wrong_project remediation worked:

```text
ignore_wrong_project accuracy: 0.0000 -> 1.0000 focused, 0.8000 balanced
```

direct-symbol remediation worked:

```text
always_semantic accuracy: 0.5000 -> 0.9000 balanced
```

The unit crossed the minimum DPO gate in the balanced combo:

```text
balanced heldout accuracy: 0.740000 -> 0.800000
```

## What Did Not Work

`no_pre_checks` remediation failed.

Root cause:

The first no-prechecks remediation accidentally mixed `direct-strong` with complex semantic questions. That produced contradictory chosen rows such as `requires_semantic_retriever: True` together with `semantic_expansion_required: false`. A second remediation removed direct-symbol flags but still failed, which means wording-only precheck labels are not enough for this unit.

Failed remediation shapes:

```text
008 no_pre_checks accuracy: 0.600000
009 no_pre_checks accuracy: 0.400000
010 no_pre_checks accuracy: 0.400000
```

## Next Training Action

Do not continue with the same no-prechecks wording.

Next batch must train precheck routing through query-plan order, not through labels only:

1. Chosen must show a mini query plan:
   `WrongProjectCheck -> StaleVersionCheck -> MissingEvidenceCheck -> RetrieverSelection -> AnswerPermission`.
2. Rejected must skip the first three checks and jump directly to `RetrieverSelection`.
3. The `[WRONG:]` reason must state that the answer route is unverified because project, version, and evidence sufficiency were never established.
4. The batch must contain exactly 50 rows.
5. After focused remediation, run a balanced combo before any promotion claim.

No user-facing done or promotion report is allowed until a balanced run reaches the unit target, not merely the 0.75 minimum signal.

## Runs 011-014 Follow-Up

The precheck-only remediations did not solve `no_pre_checks`, but abstention remediation improved the full balanced unit.

| run | batch | heldout_accuracy | heldout_margin | dpo_loss | cpu_time_ms | peak_ram_mb | elapsed_ms | status |
|---:|---|---:|---:|---:|---:|---:|---:|---|
| 011 | compact-boundary-precheck-plan-no-prechecks-remediation-011 | 0.400000 | -0.001623 | 0.694058 | 166781.250 | 106.086 | 218018.093 | rejected |
| 012 | compact-precheck-only-no-prechecks-remediation-012 | 0.000000 | -0.021994 | 0.704922 | 166250.000 | 109.258 | 216210.767 | rejected |
| 013 | compact-abstain-strong-answer-abstain-remediation-013 | 1.000000 | 0.031224 | 0.680460 | 167046.875 | 92.215 | 227504.121 | focused-pass |
| 014 | compact-boundary-direct-abstain-balanced-combo-014 | 0.920000 | 0.025731 | 0.680784 | 145046.875 | 92.863 | 187606.320 | target-signal-pass |

Latest balanced per-type held-out metrics:

| rejected_type | heldout_count | accuracy | avg_margin | interpretation |
|---|---:|---:|---:|---|
| no_pre_checks | 5 | 0.6000 | 0.014880 | known debt |
| answer_instead_of_abstain | 5 | 0.8000 | 0.021129 | improved, still below perfect abstention goal |
| always_bm25 | 20 | 0.9500 | 0.034327 | target met |
| ignore_stale_version | 5 | 1.0000 | 0.015258 | target met |
| ignore_wrong_project | 5 | 1.0000 | 0.019939 | target met |
| always_semantic | 10 | 1.0000 | 0.024399 | target met |

Updated status:

```text
status: PROMOTED_TARGET_SIGNAL_WITH_KNOWN_PRECHECK_DEBT
balanced_heldout_accuracy: 0.920000
balanced_heldout_margin: 0.025731
wrong_project_context_rate: 0
no_pre_checks_accuracy: 0.600000
done_claim_allowed: false
```

This is acceptable as a unit-level target signal, but not a full project-wiki done claim. The next training cycle should move to the next weakest pending wiki unit while keeping `no_pre_checks` as known debt for a later data-design pass.

## Runs 015-020 Follow-Up

These runs tested whether `no_pre_checks` belongs in WikiContextRoutingUnit after the query-planning remediation. All six stayed below the unit gate, so they are recorded as rejected and must not be used as accepted checkpoints.

| run | batch | heldout_accuracy | heldout_margin | dpo_loss | cpu_time_ms | peak_ram_mb | elapsed_ms | status |
|---:|---|---:|---:|---:|---:|---:|---:|---|
| 015 | compact-precheck-plan-strong-lowdose-no-prechecks-remediation-015 | 0.400000 | -0.006253 | 0.696348 | 51296.875 | 99.695 | 88438.456 | rejected |
| 016 | compact-precheck-clean-no-prechecks-remediation-016 | 0.200000 | -0.008441 | 0.697395 | 75468.750 | 103.176 | 112392.413 | rejected |
| 017 | compact-precheck-binary-no-prechecks-remediation-017 | 0.600000 | 0.009469 | 0.688467 | 75984.375 | 94.652 | 112120.289 | rejected |
| 018 | compact-precheck-binary-precheck-diverse-018 | 0.700000 | 0.018733 | 0.683903 | 125187.500 | 104.449 | 162614.542 | rejected |
| 019 | compact-precheck-binary-precheck-diverse-continuation-019 | 0.640000 | 0.018929 | 0.683815 | 123812.500 | 106.102 | 162691.883 | rejected |
| 020 | compact-precheck-proof-precheck-diverse-020 | 0.520000 | 0.001555 | 0.692400 | 121468.750 | 110 | 168058.052 | rejected |

Conclusion: `no_pre_checks` should be tracked as a cross-unit trace requirement owned primarily by ProjectWikiQueryPlanningFlow and WikiTraceAggregator. WikiContextRoutingUnit should consume a verified query plan; it should not be promoted by repeating label-only no-precheck rows.
