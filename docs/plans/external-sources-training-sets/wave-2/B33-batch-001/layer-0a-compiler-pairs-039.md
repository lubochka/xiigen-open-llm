# Layer 0-A — Compiler Pairs (039)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-039-001 — gap: hyperparameter standalone

A: "The hyperparameter search takes too long with grid search."
B: [] — "hyperparameter" = AI training parameter. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-039-002 — gap: overfitting without code verb

A: "Overfitting is detected when the validation loss diverges from training loss."
B: [] — "overfitting" = AI quality problem. Explanatory statement. No code verb. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-039-003 — IMPLEMENT: build hyperparameter tuning service

A: "Build a hyperparameter tuning service that wraps the training job API."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. "hyperparameter tuning service" = implementation artifact.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-039-004 — SECURITY: secure hyperparameter endpoint

A: "Secure the hyperparameter configuration endpoint against unauthorized modification."
B: ["SECURITY"] — "secure" fires SECURITY. "hyperparameter configuration endpoint" = object context.
CompiledPredicateIds: ["SECURITY"]

---

## C0A-039-005 — API_CONTRACT + gap: integrate hyperparameter tuner with orchestration API

A: "Integrate the hyperparameter tuner with the orchestration API."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. "hyperparameter tuner" = AI vocabulary in object position; silently skipped.
CompiledPredicateIds: ["API_CONTRACT"]
