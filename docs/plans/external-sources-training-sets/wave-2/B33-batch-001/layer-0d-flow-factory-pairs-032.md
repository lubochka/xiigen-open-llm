# Layer 0-D — Flow Factory Pairs (032)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-032-001 — GapHandler: regularization standalone

A: [] | "Regularization prevents the model from memorizing training data."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "regularization" = AI training technique. No code verb.

---

## FF-032-002 — IMPLEMENT: implement regularization wrapper

A: ["IMPLEMENT"] | "Implement a regularization wrapper that applies L2 penalty to model weights."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "regularization wrapper" = implementation artifact.

---

## FF-032-003 — GapHandler: cross-validation without code verb

A: [] | "Cross-validation gives a better estimate of model generalization performance."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "cross-validation" = AI evaluation method. No code verb.

---

## FF-032-004 — IMPLEMENT: create cross-validation runner

A: ["IMPLEMENT"] | "Create a cross-validation runner that splits the dataset and calls the training API."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "create" fires IMPLEMENT. "cross-validation runner" = implementation artifact.

---

## FF-032-005 — TEST: write tests for regularization behavior

A: ["TEST"] | "Write tests for the regularization service to verify weight decay is applied correctly."
Factory: [TestFlow51] | TransitionType: N/A (testing phase)
Teaching: "write tests" fires TEST. "regularization service" = object context.
