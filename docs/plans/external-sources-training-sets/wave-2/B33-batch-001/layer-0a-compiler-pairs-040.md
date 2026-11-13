# Layer 0-A — Compiler Pairs (040)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-040-001 — gap: regularization standalone

A: "Regularization prevents the model from memorizing training data."
B: [] — "regularization" = AI training technique. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-040-002 — gap: cross-validation without code verb

A: "Cross-validation gives a better estimate of model generalization performance."
B: [] — "cross-validation" = AI evaluation method. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-040-003 — IMPLEMENT: implement regularization wrapper

A: "Implement a regularization wrapper that applies L2 penalty to model weights."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "regularization wrapper" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-040-004 — IMPLEMENT: create cross-validation runner

A: "Create a cross-validation runner that splits the dataset and calls the training API."
B: ["IMPLEMENT"] — "create" fires IMPLEMENT. "cross-validation runner" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-040-005 — TEST: write tests for regularization behavior

A: "Write tests for the regularization service to verify weight decay is applied correctly."
B: ["TEST"] — "write tests" fires TEST. "regularization service" = object context.
CompiledPredicateIds: ["TEST"]
