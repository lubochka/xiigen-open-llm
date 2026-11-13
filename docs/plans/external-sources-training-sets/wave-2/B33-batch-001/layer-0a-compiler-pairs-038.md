# Layer 0-A — Compiler Pairs (038)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-038-001 — gap: gradient descent standalone

A: "Gradient descent converges slowly on this loss surface."
B: [] — "gradient descent" = AI optimization algorithm. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-038-002 — gap: backpropagation without code verb

A: "Backpropagation computes gradients through the network layers."
B: [] — "backpropagation" = AI training mechanism. Explanatory statement. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-038-003 — IMPLEMENT: implement gradient descent optimizer

A: "Implement a gradient descent optimizer with momentum and weight decay."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "gradient descent optimizer" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-038-004 — IMPLEMENT: write backpropagation routine

A: "Write the backpropagation routine for the custom activation function."
B: ["IMPLEMENT"] — "write" in code context fires IMPLEMENT. "backpropagation routine" = code artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-038-005 — All predicates: training vocabulary + all code verbs

A: "Architect the gradient descent platform, integrate the backpropagation API, implement the optimizer, secure the training endpoint, test the gradient computation, document the algorithm, QA."
B: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — Seven predicates. All AI training vocabulary items are objects; code verbs fire independently.
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
