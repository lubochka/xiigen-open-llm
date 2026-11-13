# Layer 0-A — Compiler Pairs (042)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-042-001 — gap: dataset curation standalone

A: "Dataset curation involves filtering low-quality examples and deduplicating records."
B: [] — "dataset curation" = AI data preparation. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-042-002 — IMPLEMENT: build dataset curation pipeline

A: "Build a dataset curation pipeline that deduplicates and filters training samples."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "dataset curation pipeline" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-042-003 — gap: training vocabulary list (all gap-ai-ml-training-001)

A: "Neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation."
B: [] — All gap-ai-ml-training-001 vocabulary. No code verb. Entire request → gap.
CompiledPredicateIds: []

---

## C0A-042-004 — IMPLEMENT overrides all training vocabulary

A: "Implement a service that manages the training loop, applies gradient descent, handles backpropagation, and reports evaluation metrics."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. All training vocabulary (training loop, gradient descent, backpropagation, evaluation metrics) is object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-042-005 — API_CONTRACT: integrate dataset curation result with training API

A: "Integrate the dataset curation output with the model training API endpoint."
B: ["API_CONTRACT"] — "integrate" always fires API_CONTRACT. "dataset curation output" = AI vocabulary in object position.
CompiledPredicateIds: ["API_CONTRACT"]
