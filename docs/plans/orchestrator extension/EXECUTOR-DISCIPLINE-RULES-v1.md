# EXECUTOR DISCIPLINE RULES v1

Status: REQUIRED. Overrides any session default that would cause early stopping.
Language: English.
Date: 2026-05-15

These rules exist because two failure modes recur:

1. The executor stalls on a trivial step and asks for approval instead of
   continuing.
2. The executor reports "done" or "promoted" before the unit has reached
   the real minimum training threshold in a working system.

Both are wrong. These rules prevent both.

---

## Rule 1: Raw numbers. No formatting overhead.

Report metrics as plain numbers. Do not build ASCII bars, tables, or formatted
blocks unless explicitly asked. The operator reads numbers directly.

Correct report after a run:

```
run: train-ready-v310-candidate-001
rows: 50  domain: plans 35 / orchestration 15
standalone: accuracy 0.847  margin 0.031  loss 0.681  elapsed 8m 42s
cpu_peak 11.2%  mem_delta +14.3 MB / avg -0.1 MB
vs V309: accuracy +0.009  margin +0.003
verdict: HELD — standalone passed, combo not run yet
```

Wrong:

```
ACCURACY
  This run (weighted):   0.847  <decorative bar>  +0.009 from last run
  ...
```

The operator does not need the bars. The numbers are sufficient.

---

## Rule 2: The executor does not stop between steps unless a gate fails.

The normal flow is:

```
source rows → validate rows → standalone run → if passes → combo run →
if passes → promote → source next rows → repeat
```

The executor does not stop and ask for permission between these steps.
It runs them in sequence and reports results at the end.

Stops that are valid:

- Gate A (standalone) failed — stop, report, wait for instruction.
- Gate B (combo) failed — stop, report, wait for instruction.
- A row shape is unknown and cannot be classified — stop, ask.
- The operator explicitly set a pause point.

Stops that are not valid:

- "I have sourced the rows, should I run training?" — run training.
- "Training finished, should I check the metrics?" — check the metrics.
- "Standalone passed, should I run the combo?" — run the combo.
- "The combo passed, should I promote?" — promote.
- "I promoted, should I source the next batch?" — source the next batch.

Each of these is a trivial continuation. Run it.

---

## Rule 3: Minimum training thresholds before any "done" claim.

A unit is not done. A unit is at a point on a curve.

The executor may use "PROMOTED" only when a unit passes its Gate B combo run.
That is a milestone, not the end.

The executor must never use:
- "done"
- "complete"
- "finished training"
- "the unit is trained"
- "ready for production"

unless ALL of the following are true for that unit:

```
1. weighted accuracy >= unit target and all unit-specific minimum metrics
   from PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md
2. reward margin >= 0.02 for at least 2 consecutive combo runs
3. total rows trained is large enough for the unit target to be meaningful:
   at least 200 rows for first useful signal, and more if the unit-specific
   target in PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md has not been reached
4. unit has been tested on at least one validation project
   and exact_evidence_accuracy >= 0.95 on that project
5. for wiki question-answer units, `ProjectWikiQueryPlanningFlow` passed its
   query plan metrics and the answer trace includes planned and executed query
   steps
6. WikiUnitTrace shows PASS for this unit
```

The 200-row number is not a promotion threshold by itself. It is only the
minimum amount of data before the accuracy curve is treated as meaningful. A
unit below its target after 200 rows remains `TRAINING IN PROGRESS` and keeps
receiving 50-row batches.

If any condition is not met, the correct statement is:

```
unit: <name>
status: TRAINING IN PROGRESS
accuracy: <value>
rows trained: <count>
missing before done: <list the unmet conditions>
next action: <what is being done to meet them>
```

---

## Rule 4: Below 0.75 — source data and continue. Do not report accuracy trend.

When a unit's weighted accuracy is below 0.75:

- Do not report accuracy as a trend signal. It is too noisy to be useful.
- Do report: rows trained, time per batch, CPU, memory. These confirm
  training is running correctly.
- Do continue training. Do not pause to ask if you should continue.
- Do source additional data in parallel from the approved sources
  (XIIGen sessions first, then MIT/Apache-2.0 repositories).
- Do run the next 50-row batch as soon as 50 validated rows are ready.

The operator does not need to be asked "should I keep going?" while
accuracy is below 0.75. Keep going.

When accuracy crosses 0.75 for the first time, report it:

```
unit: <name>
accuracy crossed 0.75 threshold: <value>
rows trained: <count>
beginning per-unit metric reporting
next batch: <description>
```

---

## Rule 5: 50 steps is the minimum unit of work, not a resting point.

Running 50 rows and reporting is one step in the loop. It is not a stopping
point. After reporting, immediately:

1. Check which unit has the lowest accuracy or weakest trend.
2. Source 50 more rows for that unit.
3. Run the next batch.

The executor pauses only when a gate fails or an explicit pause was requested.

If the operator wants more than 50 rows per batch, they will say so. Until
then, 50 rows per batch is the operating unit. The executor can and should
run as many batches in sequence as are available and passing.

---

## Rule 6: "Needs more data" is an action, not a stop.

When a unit is below 0.75 and needs external data, the executor does not
report "needs more data" and wait. It:

1. Notes that the unit is below threshold.
2. Immediately begins sourcing from the approved sources.
3. Reports what it found and how many rows are now in the pipeline.
4. Runs the next batch when 50 rows are validated.

Sourcing and training run in parallel where possible. The executor does not
finish sourcing all data before starting training. It trains on each 50-row
batch as it becomes available.

---

## Rule 7: What the executor reports vs. what it asks.

The executor reports:

- metric values after every run (raw numbers)
- which conditions are met and which are not
- what the next action is

The executor asks only when:

- a gate failed and it does not know which rows to fix
- a new row shape has not been seen before and it cannot classify it
- explicit instruction is needed for something outside the normal loop

The executor does not ask:

- whether to continue after a gate passes
- whether to run a step that the protocol defines as next
- whether the metrics look good to the operator
  (the operator reads the metrics directly)

---

## Rule 8: Forbidden phrases.

The executor must not use these phrases in any status report:

```
"Training is complete."
"The unit is ready."
"Done for now."
"I have finished the training."
"The model is trained."
"Waiting for your instructions to continue."  ← unless a gate failed
"Should I proceed?"  ← unless the next step is ambiguous
"Let me know if you want me to run more batches."  ← just run them
```

The correct pattern when work remains:

```
"Batch <N> complete. <metric summary>. Sourcing next batch now."
```

The correct pattern when a gate fails:

```
"Gate <A/B> failed. <metric summary>. Reason: <cause>.
 Rows flagged: <ids if known>.
 Waiting for instruction on which rows to ablate or replace."
```

---

## Rule 9: Report gate before any user-facing status.

Before every user-facing status, final answer, or "done" claim, the executor
must run the report eligibility gate from
`ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` Section 22.0.

The executor must classify the answer scope first:

```text
training_runner_report
implementation_status
test_result
blocker_report
plan_review
```

If the scope is not `training_runner_report`, the executor must not use the
training verdict words:

```text
PROMOTED
HELD
REJECTED
DATA_NEEDED
TRAINING_IN_PROGRESS
```

Those words are reserved for actual training runner output.

The executor must not convert a green test suite into a training report. A
test suite can prove that implementation behavior is covered. It cannot prove
that a model unit was trained, promoted, or done unless a training runner also
emitted the required metrics.

The executor must not use generated manifests, temporary artifact folders, or
remembered values to fill missing runner metrics. If the runner did not emit a
metric, the metric is `UNKNOWN` and the report must include:

```text
missing_runner_metric: <metric_name>
next_action: add metric instrumentation before claiming resource cost
```

If any of these checks fail, the executor reports a blocker or partial status
and continues the next valid action. It does not claim completion.

For DPO units, the executor must also verify continuation state before claiming
improvement:

```text
policy_initialization
reference_initialization
initial_policy_reference_max_abs_delta
policy_checkpoint_path
reference_checkpoint_path
```

A focused remediation batch is not a promotion result. It only identifies or
repairs one weak rejected type. The executor must run a mixed diagnostic or
replay batch, then an ordinary mixed-confirmation batch, and only then an
explicit promotion-confirmation batch before reporting a DPO unit as promoted.
Ordinary mixed-confirmation batches must not publish weights; publication is
allowed only from the explicit promotion-confirmation batch after per-type
diagnostics pass.

Accepted example:

```text
negative-dpo-generator-mixed-parameter-variety-049
negative-dpo-generator-mixed-confirmation-050
negative-dpo-generator-promotion-confirmation-051
```

---

## Rule 10: Time is not a stop gate.

Elapsed time, session length, context compaction, operator absence, or the fact
that the work has already taken hours or days is not an approved stop condition.

The executor must not stop because a run is long. Long-running work is normal.
The executor must persist state, restore from state after resume, and continue
from the next valid action.

The executor must not convert a time limit, tired session, context transition,
or "this is taking a while" observation into:

```text
done
complete
ready
promoted
waiting for approval
```

If a platform interruption happens, the next executor must:

1. read `EXECUTOR-STAGE-PROMPT-v2.md`;
2. read the current state file for the active training flow;
3. run the report eligibility gate;
4. continue from the next valid action unless a real gate failed.

Time may be reported as a resource metric. Time must never be used as evidence
that a unit is trained, ready, blocked, or done.

---

## Rule 11: Metrics must be saved before they are reported.

CPU, RAM, accuracy, loss, reward margin, held-out quality, and elapsed-time
numbers are reportable only after the executor writes a machine-readable report
artifact.

The required artifact gate is
`ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` Section 22.0A.

The executor must include this field in every user-facing metric report:

```text
report_artifact_path: <absolute path to saved JSON report>
```

If a test runner or training runner produced useful output but the report
artifact was not written, the executor must not summarize the numbers from
memory. It must fix the report writer and rerun the phase, or report only:

```text
REPORT_ARTIFACT_WRITE_FAILED
```

This rule exists to prevent false-positive reports, wrong-format reports, and
partly-done claims that are not backed by a durable source of truth.

---

## Summary: when can the executor stop on its own?

The executor stops on its own only when:

1. A gate fails and it cannot determine the fix without guidance.
2. No validated rows are available and sourcing is blocked.
3. The operator set an explicit stop point.

Time, session length, context compaction, and convenience are not stop
conditions.

In all other situations the executor runs the next step, reports the result,
and continues.
