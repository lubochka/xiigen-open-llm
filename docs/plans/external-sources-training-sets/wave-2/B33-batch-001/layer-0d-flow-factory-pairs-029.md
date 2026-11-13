# Layer 0-D — Flow Factory Pairs (029)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-029-001 — GapHandler: neural network standalone

A: [] | "The neural network needs more layers to improve accuracy."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: No code verb. "neural network" = AI model type. GapHandler emits SCHEDULE_CAPABILITY_LEARNING.

---

## FF-029-002 — IMPLEMENT: build neural network wrapper

A: ["IMPLEMENT"] | "Build a neural network wrapper that exposes training controls via a REST API."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. "neural network wrapper" = software artifact.

---

## FF-029-003 — ARCHITECTURE: design neural network layer structure

A: ["ARCHITECTURE"] | "Design the neural network architecture for the image classification pipeline."
Factory: [ArchitectureFlow31] | TransitionType: feature_addition
Teaching: "design" + "architecture" fires ARCHITECTURE. "neural network" = object context.

---

## FF-029-004 — GapHandler: training loop without code verb

A: [] | "The training loop runs for 100 epochs with batch size 32."
Factory: GapHandler | Gap ID: gap-ai-ml-training-001
Reason: "training loop" = AI training operation. No code verb. One SCHEDULE_CAPABILITY_LEARNING.

---

## FF-029-005 — IMPLEMENT+TEST: build neural network + write tests

A: ["IMPLEMENT", "TEST"] | "Build the neural network forward pass module and write unit tests for each layer."
Factory: [ImplementationFlow41, TestFlow51] | TransitionType: feature_addition
Ordering: ImplementationFlow41 → TestFlow51 (IMPLEMENT before TEST, canonical rule 3).
