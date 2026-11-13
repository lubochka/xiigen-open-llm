# Executor Prompt - Code Documentation 50-Pair Learning Cycle
# Version: 1.0 - 2026-05-11
# Domain: Legacy .NET / C# documentation
# Runtime contract: CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>

---

## Copy-Paste Prompt For Executor

You are executing one legacy .NET code documentation learning cycle.

First read:

```text
documentation learning/CODE-DOC-EXECUTOR-HARD-RESET-v1.md
```

Do not train a broad mixed documentation set.
Do not mix all frameworks at once.
Do not bypass the routing flow.
Do not implement product/runtime code unless the user explicitly asks for code
implementation.
Do not report static validation, file creation, or unit-test pass rate as
training accuracy.

Your job is to build, train, evaluate, and record exactly one new 50-pair
documentation learning batch inside an ongoing stream, then decide the next
batch.

Important: 50 pairs is the batch size, not the total flow size. A stack,
artifact kind, or documentation level may require many accepted 50-pair batches
before it is considered learned.

---

## Critical Acceptance Rule

Do not mark a batch accepted just because the files were created.

A prepared-only pairset is:

```text
PAIRSET_READY_TRAINING_PENDING
```

It becomes:

```text
ACCEPT_BATCH
```

only after:

1. `CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>`
   exists and is used for routing/inference.
2. DPO training runs on exactly 50 pairs.
3. Held-out inference runs on exactly 50 disjoint rows.
4. Actual metrics pass the level gate.

If the flow or training runner is missing, do not ask the user what to do next.
Continue in prepared-pairset mode, write all required output files, and write
`NEXT-BATCH-DECISION.md` with `pairset_ready_training_pending` or
`pairset_ready_training_runner_missing`.

Projected metrics are allowed only in look-like reports. They are not evidence
for acceptance or promotion.

---

## Required Document Use Protocol

Use the documents in this exact order. Write `DOCUMENT-USE-LOG.md` in the run
directory showing what rule you took from each document.

### Step 0 - Restore Prior State

Open:

```text
documentation learning/documentation-runs/
```

Answer:

- What stream is currently active?
- What was the latest prepared batch?
- What was the latest accepted batch?
- Was the latest decision `continue_same_level`, `repair_same_batch`,
  `pairset_ready_training_pending`, `promote_to_next_level`,
  `switch_stack_same_level`, or `stop_stream`?
- What were the last accepted CPU, RAM, DPO, factual accuracy, side-effect
  recall, and call-edge metrics?

If no prior run exists, start with:

```text
LIBRARY_INFRASTRUCTURE x public_method x D1 / batch-001
```

### Step 1 - Read Source Registry

Read:

```text
documentation learning/CODE-DOC-SOURCE-REGISTRY-v1.md
```

Use it to decide:

- whether the repository/tag is MIT or Apache-2.0
- whether the exact license proof is captured
- whether the source can be chosen-side authority
- which stack label applies
- which app side, runtime family, target framework, inferred C# language
  version, project style, web framework version, and frontend framework version
  apply
- whether every row has repository URL, commit/tag, file path, symbol id, and
  line span when available

If license proof is missing, do not use that source. Write
`SOURCE-REGISTRATION-REQUIRED.md`, continue with registered sources if enough
remain, and otherwise write the exact missing source-registration fields.
If the source locator is synthetic, route it to support/review and do not use it
as chosen-side documentation authority.

### Step 2 - Read Stack-Version And Call-Transit Gate

Read:

```text
documentation learning/CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md
```

Use it before building pairs to route:

- server/client side
- runtime family
- target framework
- inferred C# language version
- project style
- web/client framework version
- bottom-up call-transit stage

Do not build a broad C# documentation batch without these fields.

### Step 3 - Read Call-Tree Orchestration Plan

Read:

```text
documentation learning/CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md
```

Use it to route:

- stack label
- version-aware routing fields
- artifact kind
- documentation level D1-D4
- required evidence
- whether callee summaries are required

Do not replace this with a filename switch.

### Step 4 - Read Four-Level Training Plan

Read:

```text
documentation learning/CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md
```

Use it to select the batch mix and promotion gate for the current level.

### Step 5 - Read 50-Pair Runbook

Read:

```text
documentation learning/CODE-DOC-50-PAIR-ABLATION-RUNBOOK-v1.md
```

Use it as the operational order for build, train, evaluate, and decide.

### Step 6 - Read DPO Registry Guide

Read:

```text
documentation learning/CODE-DOC-DPO-REGISTRY-GUIDE-v1.md
```

Use it to enforce pair shape, pair type, coverage labels, and rejected
annotation format.

### Step 7 - Read Pair Authoring Guidance

Read:

```text
documentation learning/CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md
```

Use it to prevent name-echo documentation, invented behavior, and weak rejected
sides.

### Step 8 - Read Examination Method

Read:

```text
documentation learning/CODE-DOC-EXAMINATION-METHOD-v1.md
```

Use it to score every held-out row and decide whether the batch is accepted.

### Step 9 - Read Rejected Prompt Library

Read:

```text
documentation learning/CODE-DOC-DPO-REJECTED-PROMPTS-v1.md
```

Use it to create or repair rejected sides. Do not use generic "bad answer"
rejections.

### Step 10 - Read Report Template

Read:

```text
documentation learning/CODE-DOC-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md
```

Use it to write the human-readable report.

### Step 11 - Read Small-Model Rationale

Read:

```text
documentation learning/CODE-DOC-SMALL-MODEL-RATIONALE-v1.md
```

Use it only for motivation and task framing. Do not use it to override source
gates, DPO gates, or evaluation gates.

---

## Required Outputs

Write these files in the run directory:

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

---

## Training Task

Build exactly 50 train-ready DPO pairs for the active stream.

Each pair must teach one documentation behavior:

- local method purpose
- input/output documentation
- side-effect documentation
- error-path documentation
- framework lifecycle
- callee propagation
- component summary
- workflow summary

Each rejected side must contain a specific `[WRONG: ...]` annotation.

If the runtime flow or training runner is missing, continue through all output
files in prepared-pairset mode. Write the remaining reports with
`metric_mode: "not_run"` or `metric_mode: "projected"` and do not claim actual
accuracy.

---

## Evaluation Task

Evaluate exactly 50 held-out rows from the same stream.

This requires actual model inference through the documented runtime flow. If the
flow is not implemented, do not invent held-out accuracy. Write projected
quality only as `metric_mode: "projected"` and block acceptance.

Measure:

- factual accuracy
- member/API coverage
- side-effect recall
- error-path recall
- call-edge preservation
- framework lifecycle accuracy
- invented behavior count
- missing side-effect count
- missing error-path count
- wrong framework count
- CPU, RAM, and elapsed time

---

## Decision Rule

Accept the batch only if:

- hard gates pass
- runtime flow exists and was used
- DPO training executed
- held-out inference executed
- DPO reward margin > 0
- held-out factual accuracy meets the level threshold
- invented behavior is within threshold
- look-like report is complete

Otherwise choose `pairset_ready_training_pending`, `repair_same_batch`, or
`stop_stream` and explain why.

Never end the run with "awaiting your direction" after discovering a missing
runner. Leave a machine-usable pairset or repair ledger and the exact next
action contract.
