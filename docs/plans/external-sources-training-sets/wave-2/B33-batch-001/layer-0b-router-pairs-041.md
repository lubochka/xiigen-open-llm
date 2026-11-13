# Layer 0-B — Router Pairs (041)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-041-001 — Gap: benchmark standalone

A: [] | "Benchmark the model on the GLUE evaluation suite."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "benchmark" used as AI evaluation goal (not test-writing). No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-041-002 — Gap: evaluation metric without code verb

A: [] | "The evaluation metric for this task is F1 score on the held-out test set."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "evaluation metric" = AI quality measurement. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-041-003 — IMPLEMENT: build benchmark harness

A: ["IMPLEMENT"] | "Build a benchmark harness that calls the model inference API and measures latency."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "benchmark harness" = software artifact.

---

## C0B-041-004 — IMPLEMENT: develop evaluation metric calculator

A: ["IMPLEMENT"] | "Develop an evaluation metric calculator that computes F1, BLEU, and ROUGE scores."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "develop" fires IMPLEMENT. "evaluation metric calculator" = implementation artifact.

---

## C0B-041-005 — DOCUMENT: document evaluation metric selection

A: ["DOCUMENT"] | "Document the evaluation metric selection criteria for the model quality dashboard."
B: DOCUMENT → DocumentationFlow62.
Teaching: "document" fires DOCUMENT. "evaluation metric selection" = object context.
