# Layer 0-B — Router Pairs (058)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-058-001 — Gap: complete training vocabulary (gap-ai-ml-training-001)

A: [] | "Neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All gap-ai-ml-training-001 terms. No code verb. One event. Gap ID: gap-ai-ml-training-001.

---

## C0B-058-002 — IMPLEMENT: override complete training vocabulary

A: ["IMPLEMENT"] | "Build a training job manager that encapsulates the training loop, applies gradient descent, and tracks evaluation metrics."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. All training vocabulary silently suppressed.

---

## C0B-058-003 — Gap: complete round-3 model vocabulary (gap-ai-ml-model-001)

A: [] | "LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All round-3 gap-ai-ml-model-001 terms. No code verb. One event.

---

## C0B-058-004 — API_CONTRACT: integrate vector database with semantic search

A: ["API_CONTRACT"] | "Integrate the vector database write API with the semantic search indexing service."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. Both AI terms in object position suppressed.

---

## C0B-058-005 — Gap: model terms dominate when mixed with training terms

A: [] | "LLM trained with gradient descent for 100 epochs achieves BLEU score of 0.45."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Mixed gap terms. → gap-ai-ml-model-001 (dominant). One event.
