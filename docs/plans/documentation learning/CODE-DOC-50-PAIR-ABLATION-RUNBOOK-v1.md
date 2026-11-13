# Code Documentation 50-Pair Ablation Runbook
# Version: 1.0 - 2026-05-11
# Domain: Legacy .NET documentation

---

## Purpose

Run exactly one measured 50-pair training batch inside one documentation stream.

Do not merge broad sources before measurement.
Do not mix documentation levels in the same batch unless the stream is a router
stream.

---

## Prepared Pairset Is Not An Accepted Batch

A documentation batch has three possible states:

- `PAIRSET_READY_TRAINING_PENDING` - 50 train pairs and 50 held-out rows were
  built and passed static gates, but no model training/inference has happened.
- `TRAINED_EVALUATION_PENDING` - DPO training ran, but held-out documentation
  evaluation is not complete.
- `ACCEPT_BATCH` - DPO training ran, held-out inference ran, actual metrics
  passed, and the stream state was updated.

Do not write a prepared-only pairset into `accepted_batches`.

Prepared pairsets go into `prepared_batches`. Only actual trained/evaluated
batches go into `accepted_batches`.

Projected metrics may appear in a look-like report, but they are not evidence
that documentation accuracy improved.

---

## Runtime Flow Blocker

The pairset can be built before the runtime flow exists.

The batch cannot be accepted as trained evidence until this runtime surface
exists and is used:

```csharp
CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>
```

For D1 method-level pairset building, a static extractor may prepare candidate
pairs. For D1 acceptance and all promotion decisions, the actual routing flow
must run so the system proves it can select:

- stack label
- artifact kind
- documentation level
- required evidence
- callee-summary requirement

D2, D3, and D4 are blocked until D1 has at least one actual accepted batch for
the same stack/artifact stream.

---

## Required Inputs

- source slice with license proof
- 50 candidate training pairs
- 50 held-out rows from the same stream
- previous stream state, if it exists
- routing decision from `CodeDocumentationRoutingFlow`

---

## Batch Build Steps

1. Verify license proof for every source row.
2. Classify stack label and artifact kind.
3. Build call graph evidence for each snippet.
4. Assign documentation level D1-D4.
5. Create chosen documentation with explicit evidence.
6. Create rejected documentation with specific `[WRONG: ...]` consequence.
7. Run hard gates.
8. If the runtime flow is missing, continue in
   `PAIRSET_READY_TRAINING_PENDING` mode and write all output files with
   `metric_mode: "not_run"`.
9. Train on exactly 50 pairs only after the runtime flow exists.
10. Evaluate on exactly 50 held-out rows with actual model inference.
11. Write the look-like report.
12. Decide next batch.

---

## Hard Gates

Reject or repair before training if:

- chosen equals rejected
- rejected has no `[WRONG: ...]`
- rejected only negates one sentence from chosen
- chosen invents behavior not visible in code or propagated summaries
- chosen drops a side effect, data access, or error path
- source license proof is missing
- source locator is synthetic or not traceable to repository path + commit
- stack label is `UNKNOWN_REVIEW_REQUIRED`
- documentation level is higher than available call-tree evidence supports
- metrics are projected but the batch is marked accepted

---

## Metrics

Record:

- elapsed time
- CPU average and peak
- RAM average and peak
- training loss
- reward margin
- DPO accuracy
- metric_mode: `actual`, `projected`, or `not_run`
- documentation factual accuracy
- API/member coverage
- side-effect recall
- call-edge preservation
- invented behavior count
- missing error path count
- framework mismatch count

If `metric_mode` is not `actual`, the next-batch decision cannot be
`promote_to_next_level` and cannot mark the batch accepted.

Do not stop with "awaiting direction" when metrics cannot run. Write the
prepared pairset, held-out rows if available, validation ledger, metrics file
with `metric_mode: "not_run"`, and the exact missing runner or flow contract.

---

## Next-Batch Decisions

Allowed decisions:

- `continue_same_level`
- `pairset_ready_training_pending`
- `repair_same_batch`
- `promote_to_next_level`
- `switch_stack_same_level`
- `stop_stream`

Promotion requires all gates to pass and no unresolved high-severity failure.
Promotion also requires actual training and actual held-out inference.
