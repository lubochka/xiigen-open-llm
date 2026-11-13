# Layer 0-B — Router Pairs (039)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-039-001 — Gap: hyperparameter standalone

A: [] | "The hyperparameter search takes too long with grid search."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "hyperparameter" = AI training parameter. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-039-002 — Gap: overfitting without code verb

A: [] | "Overfitting is detected when validation loss diverges from training loss."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "overfitting" = AI quality problem. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-039-003 — IMPLEMENT: build hyperparameter tuning service

A: ["IMPLEMENT"] | "Build a hyperparameter tuning service that wraps the training job API."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "hyperparameter tuning service" = software artifact.

---

## C0B-039-004 — SECURITY: secure hyperparameter endpoint

A: ["SECURITY"] | "Secure the hyperparameter configuration endpoint against unauthorized modification."
B: SECURITY → SecurityAuditFlow.
Teaching: "secure" fires SECURITY. "hyperparameter" = object context.

---

## C0B-039-005 — API_CONTRACT: integrate hyperparameter tuner with orchestration API

A: ["API_CONTRACT"] | "Integrate the hyperparameter tuner with the orchestration API."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. "hyperparameter tuner" gap vocabulary suppressed.
