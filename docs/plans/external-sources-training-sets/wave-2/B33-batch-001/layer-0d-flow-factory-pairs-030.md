# Layer 0-D — Flow Factory Pairs (030)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-030-001 — GapHandler: gradient descent standalone

A: [] | "Gradient descent converges slowly on this loss surface."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "gradient descent" = AI optimization algorithm. No code verb.

---

## FF-030-002 — IMPLEMENT: implement gradient descent optimizer

A: ["IMPLEMENT"] | "Implement a gradient descent optimizer with momentum and weight decay."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "gradient descent optimizer" = implementation artifact.

---

## FF-030-003 — GapHandler: backpropagation without code verb

A: [] | "Backpropagation computes gradients through the network layers."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "backpropagation" = AI training mechanism. No code verb.

---

## FF-030-004 — IMPLEMENT: write backpropagation routine

A: ["IMPLEMENT"] | "Write the backpropagation routine for the custom activation function."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "write" (code artifact) fires IMPLEMENT. "backpropagation routine" = code artifact.

---

## FF-030-005 — API_CONTRACT: integrate trained neural network with serving API

A: ["API_CONTRACT"] | "Integrate the trained neural network with the model serving API."
Factory: [APIContractFlow33] | TransitionType: feature_addition
Teaching: "integrate" fires API_CONTRACT. "trained neural network" = object context.
