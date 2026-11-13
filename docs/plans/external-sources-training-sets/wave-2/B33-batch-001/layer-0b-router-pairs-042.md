# Layer 0-B — Router Pairs (042)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-042-001 — Gap: dataset curation standalone

A: [] | "Dataset curation involves filtering low-quality examples and deduplicating records."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "dataset curation" = AI data preparation. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-042-002 — IMPLEMENT: build dataset curation pipeline

A: ["IMPLEMENT"] | "Build a dataset curation pipeline that deduplicates and filters training samples."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "dataset curation pipeline" = implementation artifact.

---

## C0B-042-003 — Gap: all gap-ai-ml-training-001 vocabulary

A: [] | "Neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Complete training vocabulary. No code verb. One event. Gap ID: gap-ai-ml-training-001.

---

## C0B-042-004 — IMPLEMENT: override complete training vocabulary

A: ["IMPLEMENT"] | "Implement a service that manages the training loop, applies gradient descent, handles backpropagation, and reports evaluation metrics."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. All training vocabulary silently skipped.

---

## C0B-042-005 — API_CONTRACT: integrate dataset curation with training API

A: ["API_CONTRACT"] | "Integrate the dataset curation output with the model training API endpoint."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. "dataset curation output" gap vocabulary suppressed.
