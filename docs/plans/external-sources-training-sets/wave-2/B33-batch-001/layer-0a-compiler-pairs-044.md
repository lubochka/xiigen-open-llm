# Layer 0-A — Compiler Pairs (044)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-044-001 — gap: backpropagation + gradient descent compound

A: "Backpropagation uses gradient descent to update weights in each layer."
B: [] — Explanatory compound. No code verb. Two gap terms. → gap-ai-ml-training-001. One event.
CompiledPredicateIds: []

---

## C0A-044-002 — IMPLEMENT: implement backpropagation + gradient descent module

A: "Implement the backpropagation and gradient descent module for the custom optimizer."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. Both AI training terms in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-044-003 — gap: hyperparameter + regularization combined

A: "Hyperparameter selection affects regularization strength and model generalization."
B: [] — Descriptive. No code verb. Both terms = gap-ai-ml-training-001. One event.
CompiledPredicateIds: []

---

## C0A-044-004 — IMPLEMENT+API_CONTRACT: build hyperparameter service and integrate

A: "Build the hyperparameter configuration service and integrate it with the training orchestrator API."
B: ["IMPLEMENT", "API_CONTRACT"] — "build" fires IMPLEMENT; "integrate" fires API_CONTRACT. Standard order.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

---

## C0A-044-005 — QA: QA the cross-validation pipeline

A: "QA the cross-validation pipeline before promoting to production."
B: ["QA"] — "QA" abbreviation fires QA. "cross-validation pipeline" = object context. QA is last.
CompiledPredicateIds: ["QA"]
