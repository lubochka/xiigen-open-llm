# Layer 0-C — Knowledge Edge Pairs (025)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-025-001 — "tokenize" is AI model vocabulary, not a code verb

"tokenize" = the act of splitting text into tokens for LLM (Large Language Model) input. This is a model-level operation.
Rule: "tokenize the input" → gap. "build a tokenizer" → IMPLEMENT. The artifact noun (tokenizer) under a code verb (build) fires IMPLEMENT.

---

## KE-025-002 — "embed" is AI model vocabulary, not a code verb

"embed" = the act of converting text to a dense vector representation using an AI model. Model-level operation.
Rule: "embed the documents" → gap. "implement the embedding service" → IMPLEMENT. "integrate the embedding endpoint" → API_CONTRACT.

---

## KE-025-003 — "vector search" is AI retrieval vocabulary

"vector search" = querying a vector index to find semantically similar items. An AI retrieval strategy.
Rule: "use vector search" → gap. "build vector search component" → IMPLEMENT. "integrate the vector search API" → API_CONTRACT.

---

## KE-025-004 — "RAG pipeline" boundary: gap vs IMPLEMENT

"RAG (Retrieval-Augmented Generation) pipeline" = an AI system architecture pattern. Without code verb → gap.
Rule: "set up a RAG pipeline" → gap. "implement a RAG pipeline" → IMPLEMENT. "design the RAG architecture" → ARCHITECTURE.

---

## KE-025-005 — "context window" is AI model property vocabulary

"context window" = the maximum number of tokens an LLM can process in one call. Model parameter.
Rule: "manage the context window" → gap. "build a context window manager" → IMPLEMENT. Never fires API_CONTRACT alone.
