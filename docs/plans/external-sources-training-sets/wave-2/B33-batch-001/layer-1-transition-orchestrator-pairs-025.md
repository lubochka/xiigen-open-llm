# Layer 1 — Transition Orchestrator Pairs (025)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## TO-025-001 — Orchestration: neural network + training loop vocabulary

Gap request: "Neural network with overfitting and poor evaluation metrics."
Predicates: [] | GapHandler → SCHEDULE_CAPABILITY_LEARNING | Gap ID: gap-ai-ml-training-001
Orchestration: No flow selected. One gap event. DomainId: software-development.

---

## TO-025-002 — Orchestration: build neural network + write tests

Predicates: ["IMPLEMENT", "TEST"]
Request: "Build the neural network forward pass module and write unit tests for each layer."
Flow order: ImplementationFlow41 → TestFlow51
TransitionType: feature_addition
Rule applied: IMPLEMENT before TEST (canonical rule 3).

---

## TO-025-003 — v6 corruption: NEURAL_NET invented predicate

v6 corrupt input: "neural network" → "NEURAL_NET" predicate
Correct output: [] → GapHandler → gap-ai-ml-training-001
Error type: v6-01 (invented predicate). Neural network is not a code verb. No NEURAL_NET predicate exists.

---

## TO-025-004 — Orchestration: design neural network architecture

Predicates: ["ARCHITECTURE"]
Request: "Design the neural network architecture for image classification."
Flow order: ArchitectureFlow31 (single flow)
TransitionType: feature_addition
Rule applied: ARCHITECTURE fires when "design" + "architecture" present.

---

## TO-025-005 — v6 corruption: training-loop → IMPLEMENT (v6-02)

v6 corrupt: "training loop" → IMPLEMENT (treating AI training term as code verb)
Correct output: [] → GapHandler → gap-ai-ml-training-001
Error: v6-02. "Training loop" is AI vocabulary; it does not fire IMPLEMENT without a code verb.
