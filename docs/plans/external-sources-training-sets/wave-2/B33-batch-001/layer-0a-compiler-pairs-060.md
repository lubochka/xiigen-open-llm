# Layer 0-A — Compiler Pairs (060)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-060-001 — Complete B33 AI vocabulary gap trigger list (all rounds)

All AI/ML vocabulary terms that trigger gap when used without a code verb:

Round 1 (gap-ai-ml-model-001): tokenize, embed, vector search, RAG, context window, attention mechanism, transformer, inference endpoint, model serving, latency optimization, hallucination mitigation.
Round 2 (gap-ai-ml-training-001): neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation.
Round 3 (gap-ai-ml-model-001): LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent.

B: [] — All terms listed without code verbs → gap. One SCHEDULE_CAPABILITY_LEARNING. Gap ID: gap-ai-ml-model-001 (dominant when model terms present).
CompiledPredicateIds: []

---

## C0A-060-002 — IMPLEMENT overrides all three rounds of AI vocabulary

A: "Build a comprehensive AI platform wrapper covering LLM serving, embedding pipelines, vector search, training job management, and multi-agent coordination."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. All AI vocabulary from all rounds in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-060-003 — gap: DomainId rule — no AI-specific domain

A: "The AI model operations, training loops, and LLM vocabulary all belong to DomainId: ai-development."
B: Incorrect. DomainId: software-development. No "ai-development", "ml-development", or "llm-development" domain exists. All B33 gaps → DomainId: software-development.
CompiledPredicateIds: []

---

## C0A-060-004 — gap + one predicate: single code verb suppresses all gap vocabulary

A: "LLM, GPT, BERT, Llama, embedding model, neural network, training loop, gradient descent — integrate results with the reporting API."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. All gap vocabulary silently skipped.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-060-005 — All predicates: complete B33 vocabulary + all code verbs (final reference)

A: "Architect the AI platform, integrate the LLM API, implement the neural network wrapper, secure the embedding endpoint, test the semantic search, document the knowledge graph schema, QA the multi-agent system."
B: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — Seven predicates. Complete AI vocabulary coverage. Code verbs fire independently.
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
