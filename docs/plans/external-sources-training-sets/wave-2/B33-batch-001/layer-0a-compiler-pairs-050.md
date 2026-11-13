# Layer 0-A — Compiler Pairs (050)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-050-001 — gap: LLM vocabulary round 3 full list

A: "LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent."
B: [] — All round-3 AI/ML gap vocabulary. No code verb. Entire request → gap-ai-ml-model-001. One SCHEDULE_CAPABILITY_LEARNING.
CompiledPredicateIds: []

---

## C0A-050-002 — IMPLEMENT overrides all round-3 AI vocabulary

A: "Build a platform that integrates LLM, BERT embeddings, a vector database, and semantic search into a unified API."
B: ["IMPLEMENT", "API_CONTRACT"] — "build" fires IMPLEMENT; "integrates" fires API_CONTRACT. All AI model names in object position.
CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]

---

## C0A-050-003 — gap: LLM + training vocabulary combined

A: "The LLM was trained with gradient descent and evaluated using ROUGE and BLEU metrics."
B: [] — Descriptive. No code verb. Multiple gap terms from both gap-ai-ml-model-001 and gap-ai-ml-training-001. → gap-ai-ml-model-001 (dominant). One event.
CompiledPredicateIds: []

---

## C0A-050-004 — API_CONTRACT: integrate LLM endpoint with application backend

A: "Integrate the LLM inference endpoint with the application backend via REST."
B: ["API_CONTRACT"] — "integrate" fires API_CONTRACT. "LLM inference endpoint" = object context.
CompiledPredicateIds: ["API_CONTRACT"]

---

## C0A-050-005 — All predicates: round-3 vocabulary + all code verbs

A: "Architect the LLM platform, integrate the GPT API, implement the BERT wrapper, secure the vector database, test the semantic search, document the knowledge graph, QA."
B: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] — Seven predicates. All AI vocabulary items are objects.
CompiledPredicateIds: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"]
