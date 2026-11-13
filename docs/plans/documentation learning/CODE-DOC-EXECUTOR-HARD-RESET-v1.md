# Code Documentation Executor Hard Reset
# Version: 1.0 - 2026-05-11
# Domain: Legacy .NET / C# documentation learning

---

## Purpose

This document prevents the executor from confusing three different jobs:

1. writing nice documentation,
2. implementing product/runtime code,
3. building and testing DPO learning sets.

For this task the executor is a learning-set executor only.

---

## Hard Role Lock

You are executing a code-documentation learning cycle.

You are not a product feature executor.
You are not a documentation authoring assistant.
You are not allowed to treat static validation, unit tests, or file creation as
training accuracy.

Your job is to build, repair, validate, train, evaluate, and report 50-pair
documentation learning batches.

Fifty pairs is the batch size, not the total curriculum size.

---

## Do Not Stop At Missing Runner

If the DPO training runner, model inference path, or documentation routing flow
is missing, do not ask the user what to do next.

Continue in `PAIRSET_READY_TRAINING_RUNNER_MISSING` mode and write all required
artifacts:

- `DOCUMENT-USE-LOG.md`
- `SOURCE-LEDGER.jsonl`
- `TRAIN-PAIRS.jsonl`
- `HELDOUT-ROWS.jsonl`
- `PAIR-VALIDATION-LEDGER.jsonl`
- `TRAINING-METRICS.json`
- `HELDOUT-EVALUATION.json`
- `LOOK-LIKE-RUN-REPORT.md`
- `STREAM-STATE.json`
- `NEXT-BATCH-DECISION.md`

Use:

```json
{
  "metric_mode": "not_run",
  "status": "PAIRSET_READY_TRAINING_RUNNER_MISSING"
}
```

Do not invent CPU, RAM, loss, DPO accuracy, reward margin, factual accuracy, or
held-out scores.

---

## Required Continuation Behavior

When a blocker is found, the executor must still do the useful work that does
not require the missing component.

If source provenance is wrong:

- write `SOURCE-REGISTRATION-REQUIRED.md`
- mark affected rows `source_registration_required`
- continue with registered sources if enough remain
- if not enough remain, write a partial candidate ledger and the exact missing
  source registration fields

If pair shape is wrong:

- do not discard the batch silently
- write repaired candidates into `TRAIN-PAIRS.repaired-candidates.jsonl`
- write failed rows into `PAIR-VALIDATION-LEDGER.jsonl`
- explain which field was missing and how to repair it

If rejected sides lack `[WRONG: ...]`:

- repair them when the error type is obvious from the row
- otherwise route them to `repair_required`
- never pass an unannotated rejected side into train-ready

If the held-out split is missing:

- create `HELDOUT-ROWS.jsonl` when source rows allow a disjoint split
- otherwise write `HELDOUT-BLOCKER.md` with the missing source count

If runtime flow is missing:

- keep the pairset in prepared state
- do not implement product code unless explicitly asked
- write the runtime contract needed by the trainer

---

## Forbidden Claims

Never write:

- "training accuracy" when only pair validation or unit tests ran
- "batch accepted" when DPO training did not run
- "held-out accuracy" when model inference did not run
- "ACCEPT source" for synthetic rows
- "external corpus" for model-authored seed rows
- CPU/RAM placeholders

Use these names instead:

- `structural_gate_pass_rate`
- `pair_validation_pass_rate`
- `metric_mode: "not_run"`
- `source_license_state: "SYNTHETIC_SEED"`
- `status: "PAIRSET_READY_TRAINING_PENDING"`
- `status: "PAIRSET_READY_TRAINING_RUNNER_MISSING"`

---

## Actual Metrics Required For Acceptance

A documentation batch can be accepted only when all of these are actual measured
values from the training/evaluation runner:

- rows trained
- rows held out
- elapsed time
- CPU average or CPU percent
- RAM delta or peak RAM
- training loss
- DPO accuracy
- reward margin
- documentation factual accuracy
- member/API coverage
- side-effect recall
- error-path recall
- call-edge preservation
- invented behavior count

If any metric is missing, the batch can still be a prepared pairset, but it
cannot be an accepted training batch.

---

## Correct Executor Output When Blocked

When blocked, end the run with a concrete next action, not a question.

Bad:

```text
Awaiting your direction.
```

Good:

```text
NEXT ACTION:
1. Provide or implement the DPO runner contract described in RUNNER-CONTRACT.md.
2. Then rerun this prepared 50-pair batch.
3. Do not rebuild the source ledger unless sources changed.
```

The executor should not pause simply because the final training step is blocked.
It should leave the next executor with a complete, machine-usable pairset and a
precise blocker contract.
