# Legacy .NET Code Documentation Learning Guides
# Version: 1.0 - 2026-05-11
# Domain: Bottom-up documentation for legacy .NET / C# code

---

## Purpose

This folder defines the learning method for a small local model that writes
accurate documentation for legacy .NET C# code in organizations where cloud AI
is not allowed.

The model is not trained to "explain code" in one broad pass. It is trained
bottom-up:

1. document the smallest method or function
2. propagate that meaning to callers
3. document classes and components with known callee behavior
4. document controllers, pages, workflows, and modules with accumulated call
   tree evidence

The training style matches the translation and orchestration learning process:
one measured 50-pair batch at a time, explicit source authority, explicit
rejected-side error, and no promotion without evaluation.

---

## Current Documents

Read and use the documents in this order:

1. `CODE-DOC-EXECUTOR-HARD-RESET-v1.md`
2. `CODE-DOC-SOURCE-REGISTRY-v1.md`
3. `CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md`
4. `CODE-DOC-CALL-TREE-ORCHESTRATION-PLAN-v1.md`
5. `CODE-DOC-FOUR-LEVEL-TRAINING-PLAN-v1.md`
6. `CODE-DOC-50-PAIR-ABLATION-RUNBOOK-v1.md`
7. `CODE-DOC-DPO-REGISTRY-GUIDE-v1.md`
8. `CODE-DOC-DPO-PAIR-AUTHORING-GUIDANCE-v1.md`
9. `CODE-DOC-EXAMINATION-METHOD-v1.md`
10. `CODE-DOC-DPO-REJECTED-PROMPTS-v1.md`
11. `CODE-DOC-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md`
12. `CODE-DOC-SMALL-MODEL-RATIONALE-v1.md`
13. `EXECUTOR-PROMPT-CODE-DOC-50-PAIR-LEARNING-v1.md`

The executor prompt is the operational prompt for one learning run.

`CODE-DOC-STACK-VERSION-AND-CALL-TRANSIT-GATE-v1.md` is mandatory before any
legacy project run. It prevents broad "C# documentation" routing by requiring
runtime family, target framework, inferred C# language version, server/client
side, frontend framework version, and bottom-up callee-to-caller transit
evidence.

---

## Core Rule

Fifty pairs is the batch size, not the total curriculum size.

A stream may require many accepted 50-pair batches before it is considered
learned.

Prepared pairsets are not accepted batches. If the runtime
`CodeDocumentationRoutingFlow` or DPO runner is not implemented yet, the
executor must still prepare or repair the 50 train pairs and 50 held-out rows.
The batch status must remain `PAIRSET_READY_TRAINING_PENDING` or
`PAIRSET_READY_TRAINING_RUNNER_MISSING` until actual DPO training and held-out
inference run.

The executor must not end with "awaiting direction" after discovering a missing
runner. It must write the prepared pairset, validation ledger, report, stream
state, and exact runner contract needed for the next run.
