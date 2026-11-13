# Layer 0-B — Router Pairs (050)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-050-001 — Gap: all round-3 AI vocabulary

A: [] | "LLM, GPT, BERT, Llama, embedding model, vector database, semantic search, knowledge graph, AI agent, multi-agent."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All round-3 gap vocabulary. No code verb. One event. Gap ID: gap-ai-ml-model-001.

---

## C0B-050-002 — IMPLEMENT+API_CONTRACT: build + integrate round-3 vocabulary

A: ["IMPLEMENT", "API_CONTRACT"] | "Build a platform that integrates LLM, BERT embeddings, a vector database, and semantic search into a unified API."
B: IMPLEMENT → ImplementationFlow41; API_CONTRACT → APIContractFlow33.
Teaching: "build" fires IMPLEMENT; "integrates" fires API_CONTRACT. All AI model names suppressed.

---

## C0B-050-003 — Gap: mixed round-1 + round-3

A: [] | "LLM trained with gradient descent evaluated with ROUGE and BLEU metrics."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Descriptive. No code verb. Mixed gap terms. → gap-ai-ml-model-001 (dominant). One event.

---

## C0B-050-004 — API_CONTRACT: integrate LLM endpoint

A: ["API_CONTRACT"] | "Integrate the LLM inference endpoint with the application backend via REST."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. "LLM inference endpoint" suppressed.

---

## C0B-050-005 — All seven flows: round-3 vocabulary + all code verbs

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | "Architect the LLM platform, integrate GPT API, implement BERT wrapper, secure vector database, test semantic search, document knowledge graph, QA."
B: ARCHITECTURE → 3.1; API_CONTRACT → APIContractFlow33; IMPLEMENT → 4.1; SECURITY → SecurityAuditFlow; TEST → 5.1; DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73 (last). Seven flows.
