# Layer 0-D — Flow Factory Pairs (034)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-034-001 — GapHandler: dataset curation standalone

A: [] | "Dataset curation involves filtering low-quality examples and deduplicating records."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "dataset curation" = AI data preparation. No code verb.

---

## FF-034-002 — IMPLEMENT: build dataset curation pipeline

A: ["IMPLEMENT"] | "Build a dataset curation pipeline that deduplicates and filters training samples."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "dataset curation pipeline" = implementation artifact.

---

## FF-034-003 — GapHandler: all gap-ai-ml-training-001 vocabulary

A: [] | "Neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: Complete training vocabulary. No code verb. One SCHEDULE_CAPABILITY_LEARNING.

---

## FF-034-004 — IMPLEMENT: override complete training vocabulary

A: ["IMPLEMENT"] | "Implement a service that manages the training loop, applies gradient descent, handles backpropagation, and reports evaluation metrics."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. All training vocabulary silently suppressed.

---

## FF-034-005 — API_CONTRACT: integrate dataset curation with training API

A: ["API_CONTRACT"] | "Integrate the dataset curation output with the model training API endpoint."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires API_CONTRACT. "dataset curation output" suppressed.
