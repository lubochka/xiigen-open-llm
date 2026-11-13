# Layer 0-D — Flow Factory Pairs (026)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-026-001 — GapHandler: "vector search" vocabulary

A: [] | "Use vector search to find the most relevant documents."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "vector search" = AI retrieval method. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-026-002 — IMPLEMENT: build vector search

A: ["IMPLEMENT"] | "Build a vector search component that queries the embedding store."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "build" fires IMPLEMENT. AI vocabulary = object context.

---

## FF-026-003 — GapHandler: "RAG pipeline" as AI goal

A: [] | "Set up a RAG pipeline for the support bot."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "set up RAG pipeline" = AI system goal, not code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.

---

## FF-026-004 — IMPLEMENT: build RAG pipeline

A: ["IMPLEMENT"] | "Implement a RAG pipeline that retrieves documents and passes context to the LLM."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: "implement" fires IMPLEMENT. "RAG pipeline" and "LLM" = object context.

---

## FF-026-005 — GapHandler: "context window" vocabulary

A: [] | "The context window needs to be managed to avoid token overflow."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: "context window" = AI model property. No code verb. Single SCHEDULE_CAPABILITY_LEARNING emitted.
