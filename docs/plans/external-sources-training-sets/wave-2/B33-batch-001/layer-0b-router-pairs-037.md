# Layer 0-B — Router Pairs (037)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-037-001 — Gap: neural network standalone

A: [] | "The neural network needs more layers to improve accuracy."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "neural network" = AI model type. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-037-002 — IMPLEMENT: build neural network

A: ["IMPLEMENT"] | "Build a neural network with three hidden layers and ReLU activations."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "neural network" = object context; does not suppress.

---

## C0B-037-003 — Gap: training loop without code verb

A: [] | "The training loop runs for 100 epochs with batch size 32."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "training loop" = AI training operation. No code verb. Gap ID: gap-ai-ml-training-001.

---

## C0B-037-004 — ARCHITECTURE: design neural network layer structure

A: ["ARCHITECTURE"] | "Design the neural network architecture for image classification."
B: ARCHITECTURE → ArchitectureFlow31.
Teaching: "design" + "architecture" fires ARCHITECTURE. "neural network" = object.

---

## C0B-037-005 — API_CONTRACT: integrate trained neural network with serving API

A: ["API_CONTRACT"] | "Integrate the trained neural network with the model serving API."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" always fires API_CONTRACT. AI vocabulary suppressed.
