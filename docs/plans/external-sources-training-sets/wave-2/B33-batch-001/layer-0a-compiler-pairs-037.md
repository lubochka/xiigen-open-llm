# Layer 0-A — Compiler Pairs (037)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-037-001 — gap: neural network standalone

A: "The neural network needs more layers to improve accuracy."
B: [] — "neural network" = AI model type. Requirement statement. No code verb present. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-037-002 — IMPLEMENT overrides neural network object

A: "Build a neural network with three hidden layers and ReLU activations."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "neural network" is object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-037-003 — gap: training loop without code verb

A: "The training loop runs for 100 epochs with batch size 32."
B: [] — "training loop" = AI training operation. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-037-004 — ARCHITECTURE: design neural network layer structure

A: "Design the neural network architecture for the image classification pipeline."
B: ["ARCHITECTURE"] — "design" + "architecture" (structural noun) fires ARCHITECTURE. Neural network = object context.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-037-005 — API_CONTRACT: integrate training loop result with serving API

A: "Integrate the trained neural network with the model serving API."
B: ["API_CONTRACT"] — "integrate" always fires API_CONTRACT. "trained neural network" and "model serving API" are object context.
CompiledPredicateIds: ["API_CONTRACT"]
