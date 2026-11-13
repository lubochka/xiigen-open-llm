# Layer 0-B — Router Pairs (029)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-029-001 — Gap → GapHandler: "neural network" vocabulary

A: [] | "The neural network architecture needs more layers."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "neural network" = AI model type. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-029-002 — Gap → GapHandler: "training loop" vocabulary

A: [] | "The training loop runs for 100 epochs."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "training loop" = AI model training operation. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-029-003 — IMPLEMENT: build training infrastructure

A: ["IMPLEMENT"] | "Build the training infrastructure that runs the neural network training loop."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "neural network" and "training loop" = object context.

---

## C0B-029-004 — Gap → GapHandler: "gradient descent" vocabulary

A: [] | "Gradient descent is not converging with the current learning rate."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "gradient descent" = AI optimization algorithm. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-029-005 — Gap → GapHandler: "backpropagation" vocabulary

A: [] | "Backpropagation is failing to update the weights correctly."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "backpropagation" = AI training mechanism. No code verb. Gap ID: gap-ai-ml-training-001.
