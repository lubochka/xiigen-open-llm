# Layer 0-A — Compiler Pairs (030)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-030-001 — gap: "neural network" as AI model reference

A: "The neural network architecture needs more layers."
B: [] — "neural network" = AI model type. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-030-002 — gap: "training loop" vocabulary

A: "The training loop runs for 100 epochs."
B: [] — "training loop" = AI model training operation. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-030-003 — IMPLEMENT fires: build training infrastructure

A: "Build the training infrastructure that runs the neural network training loop."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "neural network" and "training loop" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-030-004 — gap: "gradient descent" optimization vocabulary

A: "Gradient descent is not converging with the current learning rate."
B: [] — "gradient descent" = AI optimization algorithm. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-030-005 — gap: "backpropagation" vocabulary

A: "Backpropagation is failing to update the weights correctly."
B: [] — "backpropagation" = AI training mechanism. No code verb. Gap.
CompiledPredicateIds: []
