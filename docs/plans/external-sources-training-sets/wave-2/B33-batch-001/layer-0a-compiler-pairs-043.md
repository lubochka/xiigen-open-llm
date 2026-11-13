# Layer 0-A — Compiler Pairs (043)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-043-001 — gap: AI+IMPLEMENT mixed — neural network + no code verb

A: "Neural network with overfitting issues and poor evaluation metrics."
B: [] — No code verb in the statement. "neural network", "overfitting", "evaluation metrics" all gap. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-043-002 — IMPLEMENT fires: build neural network wrapper

A: "Build a neural network wrapper that exposes training controls via a REST API."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "neural network wrapper" = software artifact wrapping AI.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-043-003 — gap mixed: training loop + overfitting concern

A: "The training loop experiences overfitting after 50 epochs."
B: [] — Diagnostic statement. No code verb. "training loop" + "overfitting" = two gap terms. → gap-ai-ml-training-001. One event.
CompiledPredicateIds: []

---

## C0A-043-004 — ARCHITECTURE: design training loop orchestration

A: "Design the training loop orchestration architecture for distributed GPU clusters."
B: ["ARCHITECTURE"] — "design" + "architecture" fires ARCHITECTURE. "training loop orchestration" = object context.
CompiledPredicateIds: ["ARCHITECTURE"]

---

## C0A-043-005 — IMPLEMENT+TEST: build neural network + write tests

A: "Build the neural network forward pass module and write unit tests for each layer."
B: ["IMPLEMENT", "TEST"] — "build" fires IMPLEMENT; "write unit tests" fires TEST. Two predicates, standard order.
CompiledPredicateIds: ["IMPLEMENT", "TEST"]
