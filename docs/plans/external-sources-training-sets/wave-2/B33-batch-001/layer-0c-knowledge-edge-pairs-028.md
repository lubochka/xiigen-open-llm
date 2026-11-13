# Layer 0-C — Knowledge Edge Pairs (028)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-028-001 — "hyperparameter" is AI model tuning vocabulary

"hyperparameter" = a parameter set before training (learning rate, batch size, epochs) that controls the training process itself.
Rule: "tune the hyperparameters" → gap. "build hyperparameter search service" → IMPLEMENT. "hyperparameter tuning" alone always → gap.

---

## KE-028-002 — "overfitting" is AI model quality vocabulary

"overfitting" = the condition where a model performs well on training data but poorly on unseen data.
Rule: "the model is overfitting" → gap. "implement regularization to prevent overfitting" → IMPLEMENT. "overfitting" is never a predicate trigger.

---

## KE-028-003 — "regularization" is AI training technique vocabulary

"regularization" = mathematical techniques (L1, L2, dropout) that penalize model complexity to prevent overfitting.
Rule: "apply L2 regularization" → gap. "implement a regularization module" → IMPLEMENT. "apply" in AI context does not fire IMPLEMENT.

---

## KE-028-004 — "cross-validation" is AI evaluation vocabulary

"cross-validation" = the technique of splitting data into multiple folds to evaluate model generalization.
Rule: "cross-validation results are inconsistent" → gap. "build cross-validation harness" → IMPLEMENT. "write cross-validation tests" → TEST.

---

## KE-028-005 — "benchmark" disambiguation: AI evaluation vs TEST

"benchmark" has two meanings: (1) AI model evaluation activity → gap. (2) Test function with performance measurement → TEST when preceded by "write/create tests".
Rule: "benchmark the model against GPT-4" → gap. "write benchmark tests that measure throughput" → TEST (code verb "write tests" fires TEST).
