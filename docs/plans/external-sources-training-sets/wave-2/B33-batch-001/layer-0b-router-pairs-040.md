# Layer 0-B — Router Pairs (040)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-040-001 — Gap: regularization standalone

A: [] | "Regularization prevents the model from memorizing training data."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "regularization" = AI training technique. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-040-002 — Gap: cross-validation without code verb

A: [] | "Cross-validation gives a better estimate of model generalization performance."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "cross-validation" = AI evaluation method. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-040-003 — IMPLEMENT: implement regularization wrapper

A: ["IMPLEMENT"] | "Implement a regularization wrapper that applies L2 penalty to model weights."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "regularization wrapper" = implementation artifact.

---

## C0B-040-004 — IMPLEMENT: create cross-validation runner

A: ["IMPLEMENT"] | "Create a cross-validation runner that splits the dataset and calls the training API."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "create" fires IMPLEMENT. "cross-validation runner" = implementation artifact.

---

## C0B-040-005 — TEST: write tests for regularization behavior

A: ["TEST"] | "Write tests for the regularization service to verify weight decay is applied correctly."
B: TEST → TestFlow51.
Teaching: "write tests" fires TEST. "regularization service" = object context.
