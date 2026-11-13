# Layer 0-C — Knowledge Edge Pairs (052)
# Batch: B33-batch-001 | Scenario: B33 — AI/ML vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## KE-052-001 — Vector database + embedding model combination rules

| Input | Result |
|-------|--------|
| "vector database stores embeddings from embedding model" | [] → gap-ai-ml-model-001 |
| "Build an embedding pipeline that stores vectors in the vector database" | IMPLEMENT |
| "Integrate the embedding model API with the vector database write endpoint" | API_CONTRACT |
| "Secure the vector database endpoint" | SECURITY |

---

## KE-052-002 — GPT vs. BERT: same gap ID, different model characteristics

Rule: GPT (autoregressive, text generation) and BERT (bidirectional, classification) are different AI architectures.
Both map to gap-ai-ml-model-001 regardless of their architectural differences.
"GPT generates text, BERT classifies text" → [] → gap-ai-ml-model-001 (one event).
Neither model name creates a distinct gap ID.

---

## KE-052-003 — SECURITY: "prompt injection" fires SECURITY regardless of LLM context

Rule (re-affirmed for round-3): "prompt injection" in any context → SECURITY fires.
"Secure the LLM inference endpoint against unauthorized access and prompt injection" → SECURITY.
"LLM" in the object does not change the SECURITY trigger. "prompt injection" is sufficient alone.

---

## KE-052-004 — "Integrate" fires API_CONTRACT even with multiple AI model names

Rule: "Integrate the GPT and BERT inference endpoints into a unified model gateway" → API_CONTRACT.
Multiple model names (GPT, BERT) in the object do not create multiple API_CONTRACT predicates.
One "integrate" verb → one API_CONTRACT predicate.

---

## KE-052-005 — Embedding pipeline is an IMPLEMENT artifact, not a gap

Distinction:
- "embedding pipeline" (described as a process or concept) → gap-ai-ml-model-001.
- "Build an embedding pipeline" → IMPLEMENT (pipeline = software artifact when built).
The verb "build" converts the AI concept into a software artifact.
Same pattern applies to: "training pipeline", "inference pipeline", "serving pipeline".
