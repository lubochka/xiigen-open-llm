# Layer 0-B — Router Pairs (035)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-035-001 — Gap → GapHandler: "neural network" + "training loop" compound

A: [] | "The neural network training loop needs to run for more epochs with lower learning rate."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "neural network" and "training loop" both = AI model vocabulary. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-035-002 — Gap → GapHandler: "gradient descent" + "backpropagation" compound

A: [] | "Gradient descent and backpropagation are both unstable with the current batch size."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "gradient descent" and "backpropagation" both = AI training vocabulary. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-035-003 — Gap → GapHandler: "hyperparameter" + "overfitting" + "regularization" compound

A: [] | "Tune the hyperparameters, apply regularization to prevent overfitting."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Three AI training vocabulary terms. No code verb. One event. Gap ID: gap-ai-ml-training-001.

---

## C0B-035-004 — Gap → GapHandler: "benchmark" + "evaluation metric" + "cross-validation" compound

A: [] | "Benchmark using cross-validation and report evaluation metrics."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All AI evaluation vocabulary. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-035-005 — TEST: write benchmark + evaluation tests

A: ["TEST"] | "Write benchmark tests and evaluation metric tests for the training pipeline."
B: TEST → E2ETestingFlow53.
Teaching: "write tests" fires TEST. "benchmark" and "evaluation metric" = object context.
