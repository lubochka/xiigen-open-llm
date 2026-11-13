# Layer 0-C — Knowledge Edge Pairs (050)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-050-001 — Complete round-3 gap-ai-ml-model-001 vocabulary list

All round-3 terms that trigger gap-ai-ml-model-001 (no code verb):
LLM (large language model), GPT (generative pre-trained transformer), BERT (bidirectional encoder representations from transformers), Llama (AI model family), embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent.

Rule: Any number of these terms in a single request → one SCHEDULE_CAPABILITY_LEARNING → gap-ai-ml-model-001.

---

## KE-050-002 — Gap ID priority when model + training terms combine

Priority rule for mixed gap requests:
1. gap-ai-ml-model-001 — when LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, or multi-agent terms are dominant
2. gap-ai-ml-training-001 — when neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, or dataset curation terms are dominant without model terms

Mixed request → gap-ai-ml-model-001 (model terms take priority).

---

## KE-050-003 — API_CONTRACT fires on "integrate" with any AI model name

Examples of API_CONTRACT firing from round-3 vocabulary:
- "Integrate the LLM endpoint" → API_CONTRACT
- "Integrate the GPT and BERT APIs" → API_CONTRACT
- "Integrate the vector database API" → API_CONTRACT
- "Integrate the knowledge graph REST endpoint" → API_CONTRACT
"Integrate" is absolute: fires API_CONTRACT in all contexts, regardless of AI vocabulary.

---

## KE-050-004 — Seven-predicate pattern applies to round-3 vocabulary

Verification: All seven predicates can fire in a single request containing round-3 AI vocabulary.
Pattern: "architect [AI platform], integrate [LLM API], implement [BERT wrapper], secure [vector database], test [semantic search], document [knowledge graph], QA [multi-agent system]" → all seven predicates.
AI vocabulary in each clause is object context; code verbs determine predicates.

---

## KE-050-005 — v6 corruptions for round-3 vocabulary

v6 corruptions to watch for:
- v6-01: "LLM" → "LLM_CALL" predicate (invented). Correct: [] → gap-ai-ml-model-001.
- v6-01: "vector-database" → "VECTOR_SEARCH" predicate (invented). Correct: [] → gap.
- v6-02: "semantic search" → IMPLEMENT (treating AI term as code verb). Correct: [] → gap.
- v6-03: "build LLM wrapper + integrate GPT API" → ["IMPLEMENT"] (collapsed). Correct: ["IMPLEMENT", "API_CONTRACT"].
- v6-02: "knowledge graph" → ARCHITECTURE (treating AI term as structural design). Correct: [] → gap.
