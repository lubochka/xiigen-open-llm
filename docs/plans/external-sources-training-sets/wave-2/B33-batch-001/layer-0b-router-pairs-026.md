# Layer 0-B — Router Pairs (026)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-026-001 — Gap → GapHandler: "vector search" vocabulary

A: [] | "Use vector search to find the most relevant documents."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "vector search" = AI retrieval method. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-026-002 — IMPLEMENT: build vector search component

A: ["IMPLEMENT"] | "Build a vector search component that queries the embedding store."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "vector search" and "embedding store" are object context.

---

## C0B-026-003 — Gap → GapHandler: "RAG pipeline" as AI goal

A: [] | "Set up a RAG (Retrieval-Augmented Generation) pipeline for the support bot."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "set up RAG pipeline" = AI system goal, not code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-026-004 — IMPLEMENT: build RAG pipeline

A: ["IMPLEMENT"] | "Implement a RAG pipeline that retrieves documents and passes context to the LLM."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "implement" fires IMPLEMENT. "RAG pipeline", "LLM" = object context.

---

## C0B-026-005 — Gap → GapHandler: "context window" vocabulary

A: [] | "The context window needs to be managed to avoid token overflow."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "context window" = AI model property. No code verb. Gap ID: gap-ai-ml-model-001.
