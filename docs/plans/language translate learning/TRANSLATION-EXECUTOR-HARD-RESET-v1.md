# Translation Executor Hard Reset
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English translation learning

---

## Purpose

This document prevents the executor from confusing four different jobs:

1. implementing translation runtime code,
2. validating C# unit tests,
3. writing example translation files,
4. building and testing DPO learning sets.

For this task the executor is a learning-set executor only.

---

## Hard Role Lock

You are executing a translation learning cycle.

You are not a product feature executor.
You are not allowed to implement `TranslationRoutingFlow`,
`TranslationTopicRoutingFlow`, adapters, services, or tests unless the user
explicitly asks for code implementation.

You are not allowed to report static validation, C# unit tests, or file
creation as translation accuracy.

Your job is to build, repair, validate, train, evaluate, and report 50-pair
translation DPO batches.

Fifty pairs is the batch size, not the total curriculum size.

---

## Missing Runner Is Not A Stop Point

If the DPO training runner, model inference path, BLEU/chrF evaluator, or
translation routing flow is missing, do not ask the user what to do next.

Continue in `PAIRSET_READY_TRAINING_RUNNER_MISSING` mode and write all required
artifacts:

- `DOCUMENT-USE-LOG.md`
- `source-inventory.jsonl`
- `translation-train-50.jsonl`
- `translation-heldout-50.jsonl`
- `training-pairs-repair-required.jsonl`
- `translation-batch-summary.json`
- `translation-heldout-evaluation.json`
- `translation-error-ledger.jsonl`
- `translation-sample-review.md`
- `LOOK-LIKE-RUN-REPORT.md`
- `STREAM-STATE.json`
- `NEXT-BATCH-DECISION.md`
- `RUNNER-CONTRACT.md`

Use:

```json
{
  "metric_mode": "not_run",
  "status": "PAIRSET_READY_TRAINING_RUNNER_MISSING"
}
```

Do not invent CPU, RAM, loss, DPO accuracy, reward margin, BLEU, chrF,
terminology accuracy, script accuracy, or round-trip score.

---

## Required Continuation Behavior

When a blocker is found, the executor must still do the useful work that does
not require the missing component.

If source provenance is wrong:

- mark affected rows `source_registration_required`
- write `SOURCE-REGISTRATION-REQUIRED.md`
- continue with registered ACCEPT rows if enough remain
- if not enough remain, write a partial source ledger and the exact registration
  fields needed

If the pair shape is wrong:

- do not discard the batch silently
- write repaired candidates into `translation-train-50.repaired-candidates.jsonl`
- write failed rows into `training-pairs-repair-required.jsonl`
- explain which field was missing and how to repair it

If rejected sides lack `[WRONG: ...]`:

- repair them when the wrong-route or wrong-translation defect is obvious
- otherwise route them to `repair_required`
- never pass an unannotated rejected side into train-ready

If held-out rows are missing:

- create `translation-heldout-50.jsonl` when the source ledger allows a disjoint
  split
- otherwise write `HELDOUT-BLOCKER.md` with the missing source count

If the runtime flow is missing:

- keep the pairset in prepared state
- do not implement product code unless explicitly asked
- write `RUNNER-CONTRACT.md` with the runtime/evaluator fields required by the
  trainer

---

## Forbidden Claims

Never write:

- "translation accuracy" when only pair validation or unit tests ran
- "DPO accuracy" when DPO training did not run
- "BLEU" or "chrF" when no translation evaluator ran
- "batch accepted" when training/evaluation did not run
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

A translation batch can be accepted only when all of these are actual measured
values from the training/evaluation runner:

- rows trained
- rows held out
- elapsed time
- CPU average or CPU percent
- RAM delta or peak RAM
- training loss
- DPO accuracy
- reward margin
- BLEU or chrF, with the chosen primary metric named
- script accuracy for Hebrew/Russian where applicable
- direction accuracy
- copied-source rate
- wrong-direction rate
- terminology or named-entity accuracy when relevant

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
1. Provide or implement the DPO translation runner contract described in RUNNER-CONTRACT.md.
2. Then rerun this prepared 50-pair batch.
3. Do not rebuild the source ledger unless sources changed.
```

The executor should not pause simply because the final training step is blocked.
It should leave the next executor with a complete, machine-usable pairset and a
precise blocker contract.
