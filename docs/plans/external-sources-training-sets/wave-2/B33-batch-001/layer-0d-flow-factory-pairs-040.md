# Layer 0-D — Flow Factory Pairs (040)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-040-001 — GapHandler: all round-3 AI vocabulary

A: [] | "LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: All round-3 gap vocabulary. No code verb. One SCHEDULE_CAPABILITY_LEARNING.

---

## FF-040-002 — IMPLEMENT+API_CONTRACT: build + integrate round-3 vocabulary

A: ["IMPLEMENT", "API_CONTRACT"] | "Build a platform that integrates LLM, BERT embeddings, a vector database, and semantic search into a unified API."
Factory: [APIContractFlow33, ImplementationFlow41] | TransitionType: feature_addition
Ordering: APIContractFlow33 → ImplementationFlow41.

---

## FF-040-003 — SECURITY: secure LLM inference endpoint

A: ["SECURITY"] | "Secure the LLM inference endpoint against unauthorized access and prompt injection."
Factory: [SecurityAuditFlow] | TransitionType: N/A (security audit)
Teaching: "secure" + "prompt injection" fires SECURITY. "LLM" = object.

---

## FF-040-004 — ARCHITECTURE: design complete AI stack

A: ["ARCHITECTURE"] | "Design the architecture for the complete AI stack including LLM serving, vector search, and multi-agent coordination."
Factory: [ArchitectureFlow31] | TransitionType: feature_addition
Teaching: "design" + "architecture" fires ARCHITECTURE. All AI terms = object context.

---

## FF-040-005 — GapHandler: mixed round-1 + round-3 vocabulary

A: [] | "RAG pipeline uses a transformer embedding model to query a vector database for semantic search."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001
Reason: Multiple rounds of gap vocabulary combined. No code verb. One event.
