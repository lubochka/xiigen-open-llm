# Layer 0-B — Router Pairs (034)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-034-001 — Gap: adjective AI terms do not suppress code verbs — inverse case

A: [] | "RAG-powered hallucination-mitigating document search is required."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "required" is not a code verb. "RAG-powered" and "hallucination-mitigating" are adjectives; no code verb present. Gap ID: gap-ai-ml-model-001.

---

## C0B-034-002 — IMPLEMENT: build RAG-powered service

A: ["IMPLEMENT"] | "Build a RAG-powered, hallucination-mitigating document search service."
B: IMPLEMENT → ImplementationFlow41.
Teaching: "build" fires IMPLEMENT. "RAG-powered" and "hallucination-mitigating" are adjectives; they do not suppress IMPLEMENT.

---

## C0B-034-003 — Gap: latency optimization without code verb

A: [] | "Latency optimization for the transformer-based model serving layer is needed."
B: GapHandler → SCHEDULE_CAPABILITY_LEARNING
Reason: "latency optimization" = AI operational concern. "transformer-based" = adjective. No code verb. Gap ID: gap-ai-ml-model-001.

---

## C0B-034-004 — All seven predicates: Round 2 AI vocabulary + all code verbs

A: ["ARCHITECTURE", "API_CONTRACT", "IMPLEMENT", "SECURITY", "TEST", "DOCUMENT", "QA"] | "Architect the RAG platform, integrate the embedding API, implement the tokenizer, secure the inference endpoint, test the vector search output, document the context window policy, QA."
B: ARCHITECTURE → ArchitectureFlow31; API_CONTRACT → APIContractFlow33; IMPLEMENT → ImplementationFlow41; SECURITY → SecurityAuditFlow; TEST → E2ETestingFlow53; DOCUMENT → DocumentationFlow62; QA → QAGatingFlow73 (last). Seven flows.

---

## C0B-034-005 — Gap suppression: integrate + AI vocabulary

A: ["API_CONTRACT"] | "Integrate the transformer inference endpoint with the embedding vector store."
B: API_CONTRACT → APIContractFlow33.
Teaching: "integrate" fires API_CONTRACT. "transformer inference endpoint" and "embedding vector store" = AI context suppressed.
