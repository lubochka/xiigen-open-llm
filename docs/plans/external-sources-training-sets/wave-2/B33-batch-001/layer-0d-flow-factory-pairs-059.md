# Layer 0-D — Flow Factory Pairs (059)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## FF-059-001 — GapHandler: all B33 gap vocabulary combined (rounds 1+2+3)

A: [] | "Tokenize, embed, vector search, RAG, context window, attention, transformer, inference endpoint, model serving, hallucination, neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation, LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001 | DomainId: software-development
Reason: All 31 AI vocabulary terms. No code verb. One SCHEDULE_CAPABILITY_LEARNING.

---

## FF-059-002 — IMPLEMENT: single build verb overrides all 31 AI terms

A: ["IMPLEMENT"] | "Build a comprehensive AI platform wrapper covering all model and training operations."
Factory: [ImplementationFlow41] | TransitionType: feature_addition
Teaching: One "build" fires one IMPLEMENT. All AI vocabulary silently suppressed.

---

## FF-059-003 — All seven flows: complete reference

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | Full B33 vocabulary + all seven code verbs.
Factory: [ArchitectureFlow31, APIContractFlow33, ImplementationFlow41, SecurityAuditFlow, TestFlow51, DocumentationFlow62, QAGatingFlow73] | TransitionType: feature_addition
Ordering: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestFlow51 → DocumentationFlow62 → QAGatingFlow73.

---

## FF-059-004 — IMPLEMENT: TransitionType = refactoring when refactor present

A: ["IMPLEMENT"] | "Refactor the multi-agent message broker to improve throughput and reduce memory allocation."
Factory: [ImplementationFlow41] | TransitionType: refactoring
Teaching: "refactor" fires IMPLEMENT. TransitionType = refactoring (no new feature, performance improvement).

---

## FF-059-005 — GapHandler: one SCHEDULE_CAPABILITY_LEARNING regardless of round count

A: [] | "LLM serving, neural network training, gradient descent, vector database, semantic search — all described without code verbs."
Factory: GapHandler | Gap ID: gap-ai-ml-model-001 | SCHEDULE_CAPABILITY_LEARNING: exactly one
Teaching: Multiple rounds of vocabulary → one event. Never multiple events per request.
