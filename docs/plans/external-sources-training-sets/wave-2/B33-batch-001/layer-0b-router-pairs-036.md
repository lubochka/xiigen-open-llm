# Layer 0-B — Router Pairs (036)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-036-001 — Gap: full Round 2 AI vocabulary roster, no code verb

A: [] | "Tokenize, embed, vector search, RAG, context window, attention mechanism, transformer, inference endpoint, model serving, latency optimization, hallucination mitigation."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: All Round 2 AI/ML gap vocabulary. No code verb. Single event. Gap ID: gap-ai-ml-model-001.

---

## C0B-036-002 — IMPLEMENT: code verb overrides all Round 2 AI objects

A: ["IMPLEMENT"] | "Build a service that tokenizes input, embeds documents, runs vector search, and serves via inference endpoint."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. All AI vocabulary in object position suppressed.

---

## C0B-036-003 — Gap: requirement statement with Round 2 AI terms

A: [] | "The RAG pipeline needs context window management and hallucination mitigation."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: Requirement statement. No code verb. "RAG pipeline", "context window", "hallucination mitigation" → gap. Gap ID: gap-ai-ml-model-001.

---

## C0B-036-004 — API_CONTRACT: integrate transformer inference endpoint

A: ["API_CONTRACT"] | "Integrate the transformer inference endpoint with the embedding vector store."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" always fires API_CONTRACT. AI vocabulary not suppressing.

---

## C0B-036-005 — All seven flows: complete Round 2 vocabulary + all code verbs

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | "Architect the RAG platform, integrate the embedding API, implement the tokenizer, secure the inference endpoint, test vector search, document context window policy, QA."
B: ARCHITECTURE → 3.1; API_CONTRACT → APIContractFlow33; IMPLEMENT → 4.1; SECURITY → SecurityAuditFlow; TEST → 5.1; DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73 (last, absolute). Seven flows.
