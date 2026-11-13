# Layer 0-C — Knowledge Edge Pairs (057)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-057-001 — GPT + Llama + BERT multi-model name rules

| Input | Result |
|-------|--------|
| "GPT, Llama, BERT are transformer-based LLMs" | [] → gap-ai-ml-model-001 |
| "Build a model-agnostic LLM wrapper for GPT, BERT, Llama" | IMPLEMENT |
| "Integrate GPT and BERT inference endpoints" | API_CONTRACT |

Multiple model names in one request → one SCHEDULE_CAPABILITY_LEARNING (gap). One code verb → one predicate (IMPLEMENT or API_CONTRACT).

---

## KE-057-002 — "AI agents orchestrate LLM calls" — descriptive orchestrate

Rule: "orchestrate" in a descriptive sentence (not imperative) does not fire ARCHITECTURE or any predicate.
"AI agents orchestrate LLM calls to complete reasoning tasks" = descriptive → [] → gap-ai-ml-model-001.
Contrast: "Orchestrate the LLM calls via API" = "orchestrate via API" → API_CONTRACT fires ("via API" triggers API_CONTRACT from earlier rules).

---

## KE-057-003 — Build LLM orchestration service: IMPLEMENT (not ARCHITECTURE)

Rule: "Build an LLM orchestration service" → IMPLEMENT (build + service artifact = IMPLEMENT).
"Build" is a code verb. "LLM orchestration service" = software artifact.
"Orchestration" in the service name does not trigger ARCHITECTURE.
ARCHITECTURE requires "design/architect" + structural noun, not "build/implement".

---

## KE-057-004 — ARCHITECTURE+IMPLEMENT+TEST: three-predicate build cycle

Pattern: "Design [architecture] + implement [artifact] + write tests for [artifact]" → [ARCHITECTURE, IMPLEMENT, TEST].
"Design the multi-agent architecture, implement the LLM gateway, write tests for knowledge graph connector" → [ARCHITECTURE, IMPLEMENT, TEST].
Ordering: ARCHITECTURE → IMPLEMENT → TEST (canonical rules 1, 3).

---

## KE-057-005 — All B33 rounds cross-reference summary

| Round | Gap terms | Gap ID |
|-------|-----------|--------|
| Round 1 | tokenize, embed, vector search, RAG, context window, attention, transformer, inference endpoint, model serving, hallucination mitigation | gap-ai-ml-model-001 |
| Round 2 | neural network, training loop, gradient descent, backpropagation, hyperparameter, overfitting, regularization, cross-validation, benchmark, evaluation metric, dataset curation | gap-ai-ml-training-001 |
| Round 3 | LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent | gap-ai-ml-model-001 |
