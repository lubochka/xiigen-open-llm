# ORCHESTRATION TRAINING PROTOCOL - RUNNER REPORTING v1

Status: REQUIRED.
Language: English.
Date: 2026-05-15

Purpose: define exactly what the executor must report after every real
training run, how to show computer resource use, and what to do when a unit has
not reached minimum useful accuracy yet.

This document is governed by `EXECUTOR-DISCIPLINE-RULES-v1.md`. Raw numbers are
preferred. ASCII bars and decorative dashboards are not allowed unless the
operator explicitly asks for them.

---

## Section 22: Human-Readable Run Report

### 22.0 Report Eligibility Gate

Before writing any runner-style report, the executor must decide whether a
runner report is actually allowed.

The report is allowed only when all of these are true:

```text
report_scope == training_runner_report
training_run_completed == true
runner_output_available == true
runner_output_is_source_of_truth == true
section_22_fields_checked == true
```

If the work was implementation, code editing, test execution, documentation
review, or plan review, this section is not a valid report format. In that
case the executor must write an implementation or blocker status instead, and
must not use `PROMOTED`, `HELD`, `REJECTED`, `DATA_NEEDED`, or
`TRAINING_IN_PROGRESS` as if a training runner had emitted them.

Required self-check before any user-facing status:

```text
report_scope: training_runner_report | implementation_status | test_result | blocker_report | plan_review
source_of_truth: <runner output path, command output, test command, or guide file>
report_artifact_path: <saved JSON report path or none>
artifact_written_before_message: true | false
runner_output_available: true | false
section_22_applicable: true | false
allowed_verdict_words: <exact allowed words for this report scope>
missing_runner_metrics: <comma-separated list or none>
false_positive_risk: none | metrics_missing | wrong_scope | partial_gate | unverified_claim
```

### 22.0A Persisted Report Artifact Gate

A user-facing metric report is valid only when a machine-readable report
artifact was written first. The executor must not summarize CPU, RAM, accuracy,
loss, reward margin, or held-out quality from memory, terminal scrollback, or an
unsaved object.

Required artifact locations:

```text
training_runner_report:
  <artifactRoot>/runner-reports/<run-id>-<unit-id>-section22.json

test_result or phase validation:
  C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\<run-id>\PHASE-RESOURCE-REPORT.json
```

Every persisted report artifact must contain:

```text
report_scope
source_of_truth
report_artifact_path
artifact_written_before_message
runner_output_available
section_22_applicable
false_positive_risk
missing_runner_metrics
raw metrics that were actually measured
allowed_claim
```

If the report artifact cannot be written, the only allowed user-facing status is:

```text
report_scope: blocker_report
allowed_claim: REPORT_ARTIFACT_WRITE_FAILED
false_positive_risk: unverified_claim
next_action: fix report writer and rerun the phase before reporting metrics
```

The user-facing response must include the saved `report_artifact_path`. If it
does not, the report is incomplete even when the underlying tests or runner
completed successfully.

If `false_positive_risk` is not `none`, the executor must not claim completion.
It must report the blocker and the next action.

Elapsed time, session length, context compaction, or a multi-day run is not a
report verdict. Time is a metric only. It cannot justify a `done`, `ready`,
`promoted`, `blocked`, or `waiting for approval` claim unless another real gate
or source-of-truth output supports that claim.

Manifest files, generated capability manifests, test logs, and build logs are
not substitutes for runner output. They may support a report, but they cannot
fill missing runner metrics unless the runner itself emitted those metrics.

After every completed training run - standalone probe or full combo - the
executor must produce one short raw-number report. The report may include JSON
or one-line text, but every value must come from runner output.

### 22.1 Required Raw Metrics

The runner report must include these fields when available:

```text
run_id
unit_name
batch_id
rows_trained_this_run
rows_trained_total_for_unit
baseline_id
comparison_run_id
standalone_or_combo
weighted_accuracy
weighted_accuracy_delta
reward_margin
reward_margin_delta
loss
loss_delta
heldout_accuracy
exact_evidence_accuracy
hallucination_rate
cpu_peak_percent
cpu_avg_percent
ram_peak_mb
ram_delta_mb
elapsed_ms
elapsed_per_row_ms
elapsed_per_50_rows_ms
checkpoint_size_mb
verdict
next_action
```

If a metric is not emitted by the runner, write `UNKNOWN` and add:

```text
missing_runner_metric: <metric_name>
next_action: add metric instrumentation before claiming resource cost
```

Do not invent, estimate, extrapolate, or remember values from earlier runs.
The executor may compute `elapsed_per_row_ms` and `elapsed_per_50_rows_ms` only
from runner-emitted `elapsed_ms` and row count.

### 22.2 Required Report Shape

Use plain numbers:

```text
run: train-ready-v310-candidate-001
unit: ProjectWikiRequestRouter
batch: wiki-router-batch-014
mode: standalone
rows_this_run: 50
rows_total_for_unit: 700
baseline: V309
weighted_accuracy: 0.847
weighted_accuracy_delta: +0.009
reward_margin: 0.031
reward_margin_delta: +0.003
loss: 0.681
heldout_accuracy: 0.840
exact_evidence_accuracy: UNKNOWN
hallucination_rate: UNKNOWN
cpu_peak_percent: 11.2
cpu_avg_percent: 8.7
ram_peak_mb: 735.4
ram_delta_mb: +14.3
elapsed_ms: 522000
elapsed_per_row_ms: 10440
elapsed_per_50_rows_ms: 522000
checkpoint_size_mb: 18.6
verdict: HELD
reason: standalone passed; combo not run yet
next_action: run combo without asking if Gate A passed
```

The verdict line must be one of:

- `PROMOTED` - combo passed all gates and baseline was updated.
- `HELD` - standalone passed but combo has not run yet.
- `REJECTED` - Gate A or Gate B failed; reason must be stated.
- `DATA_NEEDED` - unit accuracy is below 0.75; sourcing/training continues.
- `TRAINING_IN_PROGRESS` - unit is improving but has not met all done gates.

### 22.3 Resource Interpretation

Show raw numbers first. Then add only short interpretation text:

```text
resource_status:
  cpu: normal | heavy | stressed | unknown
  ram: normal | watch | investigate | unknown
  time_per_50_rows: fast | normal | slow | problem | unknown
```

Thresholds:

```text
cpu_peak_percent < 30: light
cpu_peak_percent 30..70: normal
cpu_peak_percent > 70: heavy
cpu_peak_percent > 95: stressed; pause only if the machine is unstable

ram_delta_mb < 50: normal
ram_delta_mb 50..200: watch
ram_delta_mb > 200: investigate replay buffer or checkpoint growth
ram_delta_mb grows for 3 consecutive runs: cap replay buffer before continuing

elapsed_per_50_rows_ms < 300000: fast
elapsed_per_50_rows_ms 300000..900000: normal
elapsed_per_50_rows_ms 900000..1800000: slow
elapsed_per_50_rows_ms > 1800000: problem; diagnose before adding more rows
```

---

## Section 23: Below-Threshold Units - Data Sourcing Rule

### 23.1 Threshold

A unit is in data sourcing phase when weighted accuracy is below 0.75.

While a unit is below 0.75:

- Do not present accuracy as a meaningful trend. It is too noisy.
- Do report rows trained, CPU, RAM, elapsed time, and missing runner metrics.
- Do continue sourcing and training. Do not ask whether to continue.
- Do run the next 50-row batch as soon as 50 validated rows are ready.
- Do not claim the unit is trained, complete, production-ready, or done.

Below-threshold report shape:

```text
unit: EntityImpactMapper
status: DATA_NEEDED
weighted_accuracy: 0.610
threshold: 0.750
rows_total_for_unit: 150
last_batch_rows: 50
cpu_peak_percent: 14.8
ram_delta_mb: +22.1
elapsed_per_50_rows_ms: 601000
source_priority_now: XIIGen internal prompts and accepted planning docs
next_action: source 50 more entity_impact_question rows and train next batch
```

### 23.2 Source Authority Gate

Every external source must be checked before it contributes rows. Accepted
source states:

```text
ACCEPT: MIT, Apache-2.0, BSD, CC-BY with attribution recorded.
ACCEPT_SUPPORT_ONLY: use for validation prompts or inspiration, not training rows.
QUARANTINE: ShareAlike, unclear license, or policy risk; isolated only if explicitly approved.
BLOCK: NC, AGPL/GPL for this purpose, source-available restrictions, copyright books/blogs, or terms that restrict LLM training.
```

The approved order for sourcing below 0.75 is:

1. XIIGen internal sessions and prompt files with first-party authority.
2. XIIGen planning, orchestration, skill, and registry documents already
   accepted as internal training authority.
3. Permissively licensed external repositories and texts.
4. CC-BY sources with explicit attribution rows.
5. Quarantine sources only after explicit policy approval and isolated testing.

### 23.3 Safe External Source Families

Use these as starting families. Verify license files at collection time.

```text
Source: dotnet/eShop
License: MIT
URL: https://github.com/dotnet/eShop
Use for: entity impact, service/repository flow, UI-to-domain-to-persistence paths.
Rows: EntityImpactMapper, ProjectWikiQueryPlanningFlow, WikiAnswerComposer, StackAndVersionClassifier.

Source: ardalis/CleanArchitecture
License: MIT
URL: https://github.com/ardalis/CleanArchitecture
Use for: clean architecture boundaries, use cases, entities, repository and service roles.
Rows: ProjectWikiRequestRouter, ProjectWikiQueryPlanningFlow, WikiContextRoutingUnit, WikiAnswerComposer.

Source: dotnet/efcore
License: MIT
URL: https://github.com/dotnet/efcore
Use for: entity mapping, query pipeline, migrations, change tracking, model metadata.
Rows: EntityImpactMapper, CodeTraceBuilder, DocumentationGenerator.

Source: temporalio/samples-dotnet
License: MIT
URL: https://github.com/temporalio/samples-dotnet
Use for: workflow execution, durable orchestration, activity/workflow boundaries.
Rows: CodeTraceBuilder, ProjectWikiQueryPlanningFlow, WikiContextRoutingUnit, WikiAnswerComposer.

Source: conductor-oss/conductor
License: Apache-2.0
URL: https://github.com/conductor-oss/conductor
Use for: workflow orchestration, task routing, step dependencies, worker boundaries.
Rows: ProjectWikiRequestRouter, ProjectWikiQueryPlanningFlow, CodeTraceBuilder, WikiContextRoutingUnit.

Source: MassTransit/MassTransit
License: Apache-2.0
URL: https://github.com/MassTransit/MassTransit
Use for: message routing, consumers, sagas, publish/subscribe impact paths.
Rows: CodeTraceBuilder, EntityImpactMapper, WikiAnswerComposer.

Source: dapr/dotnet-sdk and dapr/dapr
License: Apache-2.0
URL: https://github.com/dapr/dotnet-sdk
URL: https://github.com/dapr/dapr
Use for: service invocation, pub/sub, state stores, workflow and binding patterns.
Rows: WikiContextRoutingUnit, CodeTraceBuilder, ProjectAnswerVerifier.

Source: TruthfulQA
License: Apache-2.0
URL: https://github.com/sylinrl/TruthfulQA
Use for: abstention, false confidence, unsupported claim contrast patterns.
Rows: ProjectAnswerVerifier, WikiAnswerComposer.

Source: The Architecture of Open Source Applications
License: CC-BY 3.0
URL: https://aosabook.org/
Use for: architecture explanation and trade-off language with attribution.
Rows: DocumentationGenerator, WikiAnswerComposer.
```

Blocked or quarantined examples:

```text
Stack Overflow / Stack Exchange dump: BLOCK unless legal policy explicitly approves.
Reason: current dump terms and ShareAlike/LLM-training risk make it unsafe for default commercial training.

FEVER: QUARANTINE unless ShareAlike policy is approved.
Reason: CC BY-SA 3.0.

n8n: ACCEPT_SUPPORT_ONLY at most unless a specific permissive subset is verified.
Reason: mixed/source-available licensing risk in current project history.

Software Design by Example written text: BLOCK for commercial LLM training.
Reason: CC-BY-NC-4.0 for written material.

Michael Feathers / commercial books / company engineering blogs: BLOCK.
Reason: copyright or terms of service do not grant commercial LLM training rights.
```

### 23.4 Minimum Sourcing Before Meaningful Signal

The executor may run one 50-row batch as soon as it is ready. However, a unit
below 0.75 normally needs at least 200 sourced rows, or four 50-row batches,
before the accuracy trend is treated as meaningful.

This 200-row rule is not a promotion threshold. It is the minimum for the first
useful signal. Promotion/done thresholds are the unit-specific metrics in
`PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` plus the done rules in
`EXECUTOR-DISCIPLINE-RULES-v1.md`.

---

## Section 24: Improvement Loop - 50 Rows At A Time

The 50-row batch is the minimum unit of work, not a stopping point.

Loop:

```text
1. Pick the weakest unit or the unit blocked by a named gap.
2. Source 50 validated rows for that unit.
3. Validate row shape, source authority, chosen != rejected, [WRONG:], no mojibake.
4. Run standalone training.
5. Report raw metrics from runner output.
6. If Gate A passes, run combo without asking.
7. Report raw combo metrics.
8. If Gate B passes, promote and update living guide.
9. If Gate B fails, reject, write failure mode, and source targeted remediation rows.
10. Continue to the next 50-row batch unless a gate failed or the operator paused.
```

### 24.0A Continuation And Replay Gate

Standalone 50-row runs are diagnostics. They do not prove cumulative training
unless the next run loads the policy checkpoint produced by the previous run
and the original frozen reference checkpoint.

Every cumulative DPO report must include:

```text
policy_initialization
reference_initialization
initial_policy_reference_max_abs_delta
policy_checkpoint_path
reference_checkpoint_path
```

Fresh standalone runs must report:

```text
policy_initialization: fresh_random_policy
reference_initialization: copied_from_fresh_policy
initial_policy_reference_max_abs_delta: 0
```

Continued runs must report:

```text
policy_initialization: <previous policy_checkpoint_path>
reference_initialization: <original reference_checkpoint_path>
initial_policy_reference_max_abs_delta: > 0
```

If a continued run does not load a reference checkpoint, the report is invalid.
If a focused remediation batch improves one rejected type but the following
mixed replay batch falls below threshold, the unit remains `DATA_NEEDED`.

Promotion requires at least two stable mixed confirmation runs after the last
focused remediation batch. A focused batch alone cannot promote a unit.
Ordinary `mixed-confirmation` runs are validation runs, not publication runs.
Only a final `promotion-confirmation` run may publish weights, and only when
per-rejected-type diagnostics are all above gate.

Accepted example from 2026-05-19:

```text
unit: NegativeDpoGenerator
diagnostic batch: negative-dpo-generator-mixed-parameter-variety-049
confirmation batch: negative-dpo-generator-mixed-confirmation-050
promotion batch: negative-dpo-generator-promotion-confirmation-051
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-negative-dpo-remediation-v2\PHASE-RESOURCE-REPORT.json
promotion held-out accuracy: 0.980000019
promotion held-out reward margin: 0.003840186
publication registered: true
```

Real improvement means:

```text
weighted_accuracy_delta >= +0.002 in combo
reward_margin >= 0
no unit-specific metric regresses below its minimum
no blocked row type re-enters the dataset
no known-debt filter is violated
resource use remains within normal or explainable range
```

Stable is acceptable only when:

```text
weighted_accuracy_delta is between -0.002 and +0.002
reward_margin remains positive
the batch fixes a known coverage gap
no blocked row type re-enters
no worse unit regresses
```

Reject when:

```text
weighted_accuracy_delta < -0.002
reward_margin < 0
loss increases materially without compensating accuracy gain
known blocked row type re-enters
unit trace shows unhandled failure without remediation action
resource cost spikes without explanation
```

---

## Section 25: Per-Unit Metric Dashboard

Use the canonical unit names from `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`.
There are 11 wiki units:

```text
1. ProjectWikiRequestRouter
2. StackAndVersionClassifier
3. CodeTraceBuilder
4. EntityImpactMapper
5. DocumentationGenerator
6. NegativeDpoGenerator
7. ProjectWikiQueryPlanningFlow
8. WikiContextRoutingUnit
9. WikiAnswerComposer
10. ProjectAnswerVerifier
11. WikiTraceAggregator
```

For each unit at or above 0.75, report:

```text
unit: ProjectWikiRequestRouter
weighted_accuracy: 0.870
weighted_accuracy_delta: +0.040
reward_margin: 0.031
reward_margin_delta: +0.002
rows_total_for_unit: 501
target_metric: wiki_request_routing_accuracy >= 0.92
distance_to_target: 0.050
trend_last_3: 0.790, 0.830, 0.870
trend_status: improving
next_action: continue current row strategy
```

For each unit below 0.75, report only sourcing status plus resources:

```text
unit: EntityImpactMapper
status: DATA_NEEDED
weighted_accuracy: 0.610
rows_total_for_unit: 150
accuracy_trend_status: hidden_until_0.75
last_batch_cpu_peak_percent: 14.8
last_batch_ram_delta_mb: +22.1
last_batch_elapsed_per_50_rows_ms: 601000
next_action: source next 50 entity impact rows from XIIGen prompts and permissive repositories
```

Trend interpretation:

```text
3 combo improvements: continue current row strategy.
2 improvements then flat: add new row variations for same unit.
2 flat combo runs: add stronger rejected contrasts.
3 flat combo runs: stop blind continuation, run gap analysis, change strategy.
Any regression: ablate last batch before adding more rows.
```

Overall report after every 10 batches must include:

```text
units_at_target: <n>/11
units_above_0.75: <n>/11
units_below_0.75: <n>/11
total_rows_added_last_10_batches: <n>
cpu_avg_percent_last_10_batches: <number or UNKNOWN>
cpu_peak_percent_last_10_batches: <number or UNKNOWN>
ram_peak_mb_last_10_batches: <number or UNKNOWN>
elapsed_ms_last_10_batches: <number or UNKNOWN>
coverage_improved: true|false
coverage_delta: <human-readable delta>
if_no_improvement_gap_analysis: <missing source, missing row type, weak rejected, parser gap, runner metric gap>
next_10_batch_focus: <unit and row family>
```
