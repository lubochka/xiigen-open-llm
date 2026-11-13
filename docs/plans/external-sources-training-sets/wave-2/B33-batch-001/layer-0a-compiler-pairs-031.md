# Layer 0-A — Compiler Pairs (031)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-031-001 — gap: "hyperparameter" vocabulary

A: "Tune the hyperparameters to improve model accuracy."
B: [] — "tune the hyperparameters" = AI model tuning operation. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-031-002 — gap: "overfitting" vocabulary

A: "The model is overfitting on the training data."
B: [] — "overfitting" = AI model quality problem. No code verb. Gap.
CompiledPredicateIds: []

---

## C0A-031-003 — gap: "regularization" vocabulary

A: "Apply L2 regularization to reduce overfitting."
B: [] — "apply L2 regularization" = AI training technique. No code verb fires. Gap.
CompiledPredicateIds: []

---

## C0A-031-004 — IMPLEMENT fires: implement regularization module

A: "Implement a regularization module that applies L2 penalties during training."
B: ["IMPLEMENT"] — "implement" fires IMPLEMENT. "regularization module" and "L2 penalties" are object context.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-031-005 — gap: "cross-validation" vocabulary

A: "Cross-validation results show inconsistent performance across folds."
B: [] — "cross-validation" = AI evaluation methodology. No code verb. Gap.
CompiledPredicateIds: []
