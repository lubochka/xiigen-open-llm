# Layer 0-A — Compiler Pairs (041)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-041-001 — gap: benchmark standalone

A: "Benchmark the model on the GLUE evaluation suite."
B: [] — "benchmark" = AI evaluation activity (used as goal, not test-writing verb). No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-041-002 — gap: evaluation metric without code verb

A: "The evaluation metric for this task is F1 score on the held-out test set."
B: [] — "evaluation metric" = AI quality measurement. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-041-003 — IMPLEMENT: build benchmark harness

A: "Build a benchmark harness that calls the model inference API and measures latency."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "benchmark harness" = implementation artifact around model API.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-041-004 — IMPLEMENT: develop evaluation metric calculator

A: "Develop an evaluation metric calculator that computes F1, BLEU, and ROUGE scores."
B: ["IMPLEMENT"] — "develop" fires IMPLEMENT. "evaluation metric calculator" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-041-005 — DOCUMENT: document evaluation metric selection

A: "Document the evaluation metric selection criteria for the model quality dashboard."
B: ["DOCUMENT"] — "document" fires DOCUMENT. "evaluation metric selection criteria" = object context.
CompiledPredicateIds: ["DOCUMENT"]
