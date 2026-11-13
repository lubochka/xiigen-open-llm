# Layer 0-D — Flow Factory Pairs (047)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-047-001 — GapHandler: AI agent + neural network

A: [] | "AI agents use neural networks for decision-making and action selection."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Descriptive. No code verb. Both gap terms. Gap-ai-ml-model-001 (round-3 dominant).

---

## FF-047-002 — IMPLEMENT: build AI agent with neural network backbone

A: ["IMPLEMENT"] | "Build an AI agent that uses a neural network backbone for intent classification."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. Both AI terms in object position.

---

## FF-047-003 — IMPLEMENT: implement multi-agent message broker

A: ["IMPLEMENT"] | "Implement a multi-agent message broker that routes events between agent instances."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "multi-agent message broker" = implementation artifact.

---

## FF-047-004 — GapHandler: vector database + embedding + semantic search chain

A: [] | "The embedding model produces vectors stored in the vector database for semantic search queries."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Descriptive pipeline. No code verb. Three gap terms. One event.

---

## FF-047-005 — ARCHITECTURE+API_CONTRACT+IMPLEMENT: embedding pipeline build cycle

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT"] | "Design the embedding pipeline architecture, integrate the vector database API, implement the semantic search handler."
Factory: [ArchitectureFlow31, APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41.
