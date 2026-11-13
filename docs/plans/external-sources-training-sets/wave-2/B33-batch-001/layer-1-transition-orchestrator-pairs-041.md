# Layer 1 — Transition Orchestrator Pairs (041)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-041-001 — Orchestration: neural network + LLM combined gap (mixed rounds)

Gap request: "Neural networks form the backbone of LLMs used in semantic search applications."
Predicates: [] | GapHandler → gap-ai-ml-model-001 (model dominant) | One event.

---

## TO-041-002 — Orchestration: build LLM + neural network platform

Predicates: ["IMPLEMENT"]
Request: "Build an LLM platform that wraps neural network inference behind a clean API."
Flow: ImplementationFlow41 | TransitionType: feature_addition

---

## TO-041-003 — Orchestration: training vocabulary complete gap (pure round-2)

Gap request: "Neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation."
Predicates: [] | GapHandler → gap-ai-ml-training-001 | One event.
(No model-level terms present; round-2 only → gap-ai-ml-training-001.)

---

## TO-041-004 — Orchestration: build training manager + integrate benchmark API

Predicates: ["IMPLEMENT", "API_CONTRACT"]
Request: "Build a training job manager that tracks training loop state and integrate with the benchmark reporting API."
Flow order: APIContractFlow33 → ImplementationFlow41 | TransitionType: feature_addition

---

## TO-041-005 — v6 corruption: all predicates collapsed for rich training request (v6-03)

v6 corrupt: "implement training manager + integrate benchmark API + write tests" → ["IMPLEMENT"] only
Correct: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Error: v6-03. Each code verb fires independently. Three predicates.
