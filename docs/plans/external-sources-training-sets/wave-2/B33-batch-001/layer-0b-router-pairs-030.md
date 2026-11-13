# Layer 0-B — Router Pairs (030)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-030-001 — Gap → GapHandler: "hyperparameter" vocabulary

A: [] | "Tune the hyperparameters to improve model accuracy."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "tune hyperparameters" = AI model tuning operation. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-030-002 — Gap → GapHandler: "overfitting" vocabulary

A: [] | "The model is overfitting on the training data."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "overfitting" = AI model quality problem. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-030-003 — Gap → GapHandler: "regularization" vocabulary

A: [] | "Apply L2 regularization to reduce overfitting."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "apply regularization" = AI training technique. No code verb fires. Gap ID: gap-ai-ml-training-001.

---

## C0B-030-004 — IMPLEMENT: implement regularization module

A: ["IMPLEMENT"] | "Implement a regularization module that applies L2 penalties during training."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "regularization module" = artifact. AI vocabulary in object position.

---

## C0B-030-005 — Gap → GapHandler: "cross-validation" vocabulary

A: [] | "Cross-validation results show inconsistent performance across folds."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "cross-validation" = AI evaluation methodology. No code verb. Gap ID: gap-ai-ml-training-001.
