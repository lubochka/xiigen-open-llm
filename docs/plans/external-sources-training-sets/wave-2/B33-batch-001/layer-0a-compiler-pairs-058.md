# Layer 0-A — Compiler Pairs (058)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-058-001 — gap: complete training vocabulary all-gap (gap-ai-ml-training-001)

A: "Neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation."
B: [] — All gap-ai-ml-training-001 terms. No code verb. One SCHEDULE_CAPABILITY_LEARNING. → gap-ai-ml-training-001.
CompiledPredicateIds: []

---

## C0A-058-002 — IMPLEMENT overrides complete training vocabulary

A: "Build a training job manager that encapsulates the training loop, applies gradient descent, and tracks evaluation metrics."
B: ["IMPLEMENT"] — "build" fires IMPLEMENT. All training vocabulary in object position.
CompiledPredicateIds: ["IMPLEMENT"]

---

## C0A-058-003 — gap: model vocabulary all-gap (gap-ai-ml-model-001)

A: "LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent."
B: [] — All gap-ai-ml-model-001 round-3 terms. No code verb. One SCHEDULE_CAPABILITY_LEARNING. → gap-ai-ml-model-001.
CompiledPredicateIds: []

---

## C0A-058-004 — API_CONTRACT: integrate vector database with semantic search

A: "Integrate the vector database write API with the semantic search indexing service."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. Both AI terms in object position.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-058-005 — gap selection: model terms dominate when mixed with training terms

A: "LLM trained with gradient descent for 100 epochs achieves BLEU score of 0.45."
B: [] — Descriptive. No code verb. Mixed model + training terms. → gap-ai-ml-model-001 (model terms dominate). One event.
CompiledPredicateIds: []
